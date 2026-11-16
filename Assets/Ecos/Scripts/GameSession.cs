using UnityEngine;

namespace Ecos
{
    public enum SessionState { Menu, Playing, Paused, Dead, Ending }

    public class GameSession : MonoBehaviour
    {
        public static GameSession Instance { get; private set; }

        public PlayerRig Player;
        public HospitalMap Map;
        public SebastianAI Enemy;
        public AudioDirector Audio;

        public SaveData Data { get; private set; } = new SaveData();
        public SessionState State { get; private set; } = SessionState.Menu;
        public string Panel { get; private set; } = "";
        public string DocumentId = "", Message = "", Result = "", Code = "";
        public float MessageUntil, SavedUntil;
        public bool[] Switches = new bool[3];

        public float Volume = 0.75f, Brightness = 0.30f, Sensitivity = 2f, FieldOfView = 72f;
        public bool Ready { get; private set; }
        public bool Playing => State == SessionState.Playing && Panel == "";

        private int pasoJuguetes;
        private readonly int[] ordenJuguetes = { 0, 2, 1 };
        private bool lucesAtenuadas;

        void Awake() => Instance = this;

        void Start()
        {
            Audio.Initialize();
            Enemy.ResetActor();
            OscurecerEscena();
            ApplyOptions();
            SetCursor(false);
            Ready = true;
        }

        private void OscurecerEscena()
        {
            if (lucesAtenuadas) return;
            lucesAtenuadas = true;

            // Elimina luz del cielo y activa niebla densa de pasillo oscuro
            RenderSettings.skybox = null;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.008f, 0.010f, 0.012f);
            RenderSettings.fogDensity = 0.068f;

            if (Player != null && Player.view != null)
            {
                Player.view.clearFlags = CameraClearFlags.SolidColor;
                Player.view.backgroundColor = new Color(0.008f, 0.010f, 0.012f);
                Player.view.cullingMask &= ~(1 << PlayerRig.GhostLayer);
            }

            // Atenúa todas las lámparas y luces direccionales pre-guardadas en la escena
            foreach (Light luz in FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (Player != null && (luz == Player.infrared || luz == Player.eyeLight)) continue;
                if (luz.type == LightType.Directional)
                {
                    luz.intensity = 0.015f;
                }
                else
                {
                    luz.intensity *= 0.34f;
                    luz.range *= 0.78f;
                }
            }

            if (Player != null && Player.eyeLight != null)
            {
                Player.eyeLight.range = 4.6f;
                Player.eyeLight.intensity = 0.18f;
            }
        }

        void Update()
        {
            if (!Ready) return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (Panel != "") ClosePanel();
                else if (Player != null && Player.IsHiding) Player.ExitLocker();
                else if (State == SessionState.Playing) Pause();
                else if (State == SessionState.Paused) Resume();
            }

            if (Input.GetKeyDown(KeyCode.Tab) && State == SessionState.Playing)
            {
                if (Panel == "inventory") ClosePanel();
                else if (Panel == "") OpenPanel("inventory");
            }

            if (Playing)
            {
                int zonaActual = Mathf.Clamp(Mathf.FloorToInt(Player.transform.position.z / 14f), 0, 7);
                if (zonaActual != Data.zone)
                {
                    Data.zone = zonaActual;
                    Audio.Zone(zonaActual, false);
                    if (zonaActual >= 2) ChangeAnger(8f);
                    Checkpoint();
                    Notify(StoryData.Chapters[zonaActual] + " · " + StoryData.Zones[zonaActual]);
                    Enemy.Encounter();
                }
            }
        }

        public bool Has(string flag) => Data.flags.Contains(flag);
        public void Flag(string flag) { if (!Has(flag)) Data.flags.Add(flag); }
        public void Add(string id) { if (!Data.inventory.Contains(id)) Data.inventory.Add(id); Notify("Obtenido: " + StoryData.Get(id).title); Audio.PlayItemPickup(); }
        public void ChangeAnger(float delta) => Data.anger = Mathf.Clamp(Data.anger + delta, 0f, 100f);
        public void Notify(string msg) { Message = msg; MessageUntil = Time.unscaledTime + 4f; }

        public void NewGame()
        {
            Data = new SaveData();
            Data.inventory.Add("camera");
            Data.inventory.Add("recorder");
            Data.journal.Add("welcome");
            IniciarPartida();
            Checkpoint();
        }

        public void Continue()
        {
            SaveData guardado = SaveService.Read();
            if (guardado == null) { Notify("No hay partida guardada."); return; }
            Data = guardado;
            IniciarPartida();
            Notify("Partida cargada · " + StoryData.Zones[Data.zone]);
        }

        private void IniciarPartida()
        {
            State = SessionState.Playing;
            Panel = "";
            pasoJuguetes = 0;
            Switches = new bool[3];
            Code = "";
            RefreshWorld();
            Player.Teleport(Data);
            Enemy.ResetActor();
            Audio.Zone(Data.zone, false);
            SincronizarPausa();
        }

        public void Interact(Interactable item)
        {
            if (!Playing) return;
            switch (item.kind)
            {
                case "locker": Player.ToggleLocker(item.transform); break;
                case "bed_hide": Player.ToggleBedHide(item.transform); break;
                case "valve":
                    if (Has("steam_purged")) { Notify("La presión de vapor ya está estabilizada."); break; }
                    Flag("steam_purged");
                    ChangeAnger(-10f);
                    Notify("Válvula purgada. El entorno se calma (-10% Ira).");
                    break;
                case "clue":
                    if (Has("clue_" + item.id)) { Notify("Ya revisaste esto."); break; }
                    Flag("clue_" + item.id);
                    Add(item.entry);
                    if (item.id == "frame")
                    {
                        ChangeAnger(-4f);
                        AbrirPuerta(2);
                        Notify("Llave del ala infantil obtenida (-4% Ira).");
                    }
                    else if (item.id == "xray")
                    {
                        ChangeAnger(12f);
                        Notify("Registros falsificados descubiertos (+12% Ira).");
                    }
                    else if (item.id == "seal")
                    {
                        ChangeAnger(15f);
                        AbrirPuerta(7);
                        Enemy.Encounter();
                        Notify("Sello de la morgue abierto (+15% Ira).");
                    }
                    Checkpoint();
                    break;
                case "document":
                    if (!Data.journal.Contains(item.entry)) Data.journal.Add(item.entry);
                    DocumentId = item.entry;
                    OpenPanel("document");
                    break;
                case "battery":
                    Data.spareBatteries++;
                    Flag("taken_" + item.id);
                    item.gameObject.SetActive(false);
                    Audio.PlayItemPickup();
                    Notify("Pila recogida. Pulsa R para recargar.");
                    break;
                case "key":
                    Add(item.entry);
                    Flag("taken_" + item.id);
                    item.gameObject.SetActive(false);
                    break;
                case "door": item.GetComponent<DoorGate>().Open(); break;
                case "toy":
                    if (Has("toys")) break;
                    if (item.number == ordenJuguetes[pasoJuguetes])
                    {
                        pasoJuguetes++;
                        item.transform.Rotate(0f, 30f, 0f);
                        Notify("Juguete correcto (" + pasoJuguetes + "/3)");
                        if (pasoJuguetes == 3)
                        {
                            Flag("toys");
                            ChangeAnger(-10f);
                            Add("urgency_key");
                            AbrirPuerta(3);
                            Notify("Melodía completada. Sebastián se calma (-10% Ira).");
                            Checkpoint();
                        }
                    }
                    else
                    {
                        pasoJuguetes = 0;
                        ChangeAnger(14f);
                        Enemy.HearNoise(Player.transform.position, 20f);
                        Notify("Orden incorrecto (+14% Ira). ¡El ruido alertó a Sebastián!");
                    }
                    break;
                case "safe":
                    if (Has("secret")) { DocumentId = "secret"; OpenPanel("document"); }
                    else { Code = ""; OpenPanel("safe"); }
                    break;
                case "generator":
                    if (!Has("power")) OpenPanel("generator");
                    break;
                case "child":
                    Flag("child_seen");
                    Notify("Camilla A1: Sebastián. Busca a su madre en la sala derecha (C2).");
                    break;
                case "mother":
                    if (!Has("child_seen")) { Notify("Identifica primero a Sebastián en la sala izquierda."); break; }
                    Flag("together");
                    ChangeAnger(-35f);
                    RefreshWorld();
                    Notify("Ambas camillas están juntas (-35% Ira). Revisa la grabadora.");
                    break;
                case "ending":
                    if (!Has("together")) Notify("Primero reúne ambas camillas.");
                    else OpenPanel("ending");
                    break;
            }
        }

        private void AbrirPuerta(int zona)
        {
            foreach (DoorGate puerta in FindObjectsByType<DoorGate>(FindObjectsSortMode.None))
                if (puerta.zone == zona) puerta.ForceUnlockAndAnimate();
        }

        public void SubmitCode()
        {
            if (Code != "1985")
            {
                Code = "";
                ChangeAnger(12f);
                Enemy.HearNoise(Player.transform.position, 18f);
                Notify("Código incorrecto (+12% Ira). Pista: 19 + 85.");
                return;
            }
            Flag("secret");
            Data.journal.Add("secret");
            Add("secret");
            Add("basement_key");
            OpenPanel("decision");
        }

        public void DecideEvidence(bool conservar)
        {
            if (conservar)
            {
                Flag("preserved");
                ChangeAnger(-15f);
                Notify("Pruebas conservadas (-15% Ira).");
            }
            else
            {
                Flag("destroyed");
                ChangeAnger(45f);
                Enemy.Encounter();
                Notify("¡Expediente destruido (+45% Ira)! Sebastián entró en furia.");
            }
            ClosePanel();
            AbrirPuerta(5);
            Checkpoint();
        }

        public void Generator()
        {
            if (!Data.inventory.Contains("fuse")) { Notify("Falta el fusible (búscalo en la sala de enfrente)."); return; }
            if (!(Switches[0] && !Switches[1] && Switches[2]))
            {
                ChangeAnger(10f);
                Enemy.HearNoise(Player.transform.position, 18f);
                Notify("Sobrecarga eléctrica (+10% Ira). Configura A:ON, B:OFF, C:ON.");
                return;
            }
            Data.inventory.Remove("fuse");
            Flag("power");
            ChangeAnger(-10f);
            Add("pathology_key");
            ClosePanel();
            AbrirPuerta(6);
            Notify("Generador estabilizado (-10% Ira).");
            Checkpoint();
        }

        public void End(bool contarVerdad)
        {
            Result = (contarVerdad && Has("preserved"))
                ? "FINAL VERDADERO: JUNTOS\n\nReuniste a Sebastián con su madre y conservaste las pruebas para revelar la verdad."
                : "FINAL ALTERNATIVO: EL SILENCIO\n\nMadre e hijo descansan juntos, pero la historia quedó oculta en el hospital.";
            State = SessionState.Ending;
            Panel = "";
            Enemy.ResetActor();
            SincronizarPausa();
        }

        public void RefreshWorld()
        {
            foreach (Interactable item in FindObjectsByType<Interactable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (item.kind == "battery" || item.kind == "key") item.gameObject.SetActive(!Has("taken_" + item.id));
                if (item.kind == "mother")
                {
                    item.transform.position = Has("together") ? new Vector3(-6.6f, 0f, 104f) : new Vector3(8f, 0f, 104f);
                    ModelLibrary.SetBaseHeight(item.gameObject, 0f);
                }
            }
            foreach (DoorGate gate in FindObjectsByType<DoorGate>(FindObjectsSortMode.None)) gate.Refresh();
        }

        public string Objective()
        {
            if (!Has("gate_2")) return "Revisa detrás del cuadro en Recepción para hallar la llave.";
            if (!Has("toys")) return "Activa los juguetes en orden: ROJO → AZUL → AMARILLO.";
            if (!Has("secret")) return "Busca los registros en Urgencias y abre el archivador (1985).";
            if (!Has("power")) return "Busca el fusible y enciende el generador (A:ON, B:OFF, C:ON).";
            if (!Has("gate_7")) return "Revisa el cajón de Anatomía Patológica para abrir la Morgue.";
            if (!Has("together")) return "Identifica a Sebastián (A1) y reúne la camilla de su madre (C2).";
            return "Usa la grabadora junto a las camillas para terminar.";
        }

        public void ReloadCamera()
        {
            if (Data.battery >= 99f || Data.spareBatteries <= 0) return;
            Data.spareBatteries--;
            Data.battery = 100f;
            Notify("Batería recargada. Pilas restantes: " + Data.spareBatteries);
        }

        public void Checkpoint()
        {
            Vector3 p = Player.transform.position;
            Data.x = p.x; Data.y = Mathf.Max(0.03f, p.y); Data.z = p.z;
            Data.yaw = Player.transform.eulerAngles.y;
            if (SaveService.Write(Data)) SavedUntil = Time.unscaledTime + 2.5f;
        }

        public void Lose(string motivo) { Result = motivo; State = SessionState.Dead; Panel = ""; SincronizarPausa(); }
        public void Pause() { State = SessionState.Paused; SincronizarPausa(); }
        public void Resume() { State = SessionState.Playing; Panel = ""; SincronizarPausa(); }
        public void Menu() { State = SessionState.Menu; Panel = ""; Enemy.ResetActor(); Audio.Zone(0, true); SincronizarPausa(); }
        public void OpenPanel(string p) { Panel = p; SincronizarPausa(); }
        public void ClosePanel() { Panel = ""; SincronizarPausa(); }

        public void ApplyOptions()
        {
            AudioListener.volume = Volume;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = Color.Lerp(new Color(0.004f, 0.005f, 0.007f), new Color(0.018f, 0.022f, 0.026f), Brightness);
        }

        private void SincronizarPausa()
        {
            Time.timeScale = Playing ? 1f : 0f;
            SetCursor(Playing);
        }

        private void SetCursor(bool bloqueado)
        {
            Cursor.lockState = bloqueado ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !bloqueado;
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}