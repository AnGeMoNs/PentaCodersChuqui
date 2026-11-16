using UnityEngine;

namespace Ecos
{
    public class GameUI : MonoBehaviour
    {
        private GUIStyle titulo, texto, boton, camText, camAlert;
        private string seleccionado = "welcome";

        void OnGUI()
        {
            GameSession g = GameSession.Instance;
            if (g == null || !g.Ready) return;

            if (titulo == null)
            {
                titulo = new GUIStyle(GUI.skin.label) { fontSize = 36, fontStyle = FontStyle.Bold, wordWrap = true };
                texto = new GUIStyle(GUI.skin.label) { fontSize = 18, wordWrap = true };
                boton = new GUIStyle(GUI.skin.button) { fontSize = 18, fixedHeight = 42 };
                camText = new GUIStyle(GUI.skin.label) { fontSize = 17, fontStyle = FontStyle.Bold };
                camAlert = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            }

            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Screen.width / 1280f, Screen.height / 720f, 1f));

            if (g.State == SessionState.Menu)
            {
                GUI.Box(new Rect(0, 0, 1280, 720), "");
                GUILayout.BeginArea(new Rect(420, 160, 440, 420));
                GUILayout.Label("ECOS DE CHUQUICAMATA", titulo);
                GUILayout.Space(20);
                if (GUILayout.Button("Nueva Partida", boton)) g.NewGame();
                GUI.enabled = SaveService.Read() != null;
                if (GUILayout.Button("Continuar", boton)) g.Continue();
                GUI.enabled = true;
                if (GUILayout.Button("Salir", boton)) g.Quit();
                GUILayout.EndArea();
                return;
            }

            if (g.State == SessionState.Paused)
            {
                GUI.Box(new Rect(420, 180, 440, 340), "");
                GUILayout.BeginArea(new Rect(450, 200, 380, 300));
                GUILayout.Label("PAUSA", titulo);
                GUILayout.Space(15);
                if (GUILayout.Button("Reanudar", boton)) g.Resume();
                if (GUILayout.Button("Inventario [Tab]", boton)) { g.Resume(); g.OpenPanel("inventory"); }
                if (GUILayout.Button("Menú Principal", boton)) g.Menu();
                GUILayout.EndArea();
                return;
            }

            if (g.State == SessionState.Dead || g.State == SessionState.Ending)
            {
                GUI.Box(new Rect(340, 160, 600, 400), "");
                GUILayout.BeginArea(new Rect(370, 190, 540, 350));
                GUILayout.Label(g.State == SessionState.Dead ? "HAS MUERTO" : "FIN DE LA HISTORIA", titulo);
                GUILayout.Space(15);
                GUILayout.Label(g.Result, texto);
                GUILayout.Space(25);
                if (g.State == SessionState.Dead && SaveService.Read() != null && GUILayout.Button("Reintentar Checkpoint", boton)) g.Continue();
                if (GUILayout.Button("Volver al Menú", boton)) g.Menu();
                GUILayout.EndArea();
                return;
            }

            if (g.Panel != "")
            {
                GUI.Box(new Rect(280, 100, 720, 520), "");
                GUILayout.BeginArea(new Rect(310, 120, 660, 480));

                if (g.Panel == "document")
                {
                    Entry e = StoryData.Get(g.DocumentId);
                    GUILayout.Label(e.title, titulo);
                    GUILayout.Space(10);
                    GUILayout.Label(e.description, texto);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Cerrar [ESC]", boton)) g.ClosePanel();
                }
                else if (g.Panel == "inventory")
                {
                    GUILayout.Label("INVENTARIO Y NOTAS", titulo);
                    foreach (string id in g.Data.inventory)
                        if (GUILayout.Button(StoryData.Get(id).title, boton)) seleccionado = id;
                    GUILayout.Space(10);
                    GUILayout.Label(StoryData.Get(seleccionado).description, texto);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Cerrar [TAB]", boton)) g.ClosePanel();
                }
                else if (g.Panel == "safe")
                {
                    GUILayout.Label("CÓDIGO DE ARCHIVADOR: " + g.Code.PadRight(4, '_'), titulo);
                    GUILayout.BeginHorizontal();
                    for (int i = 1; i <= 10; i++)
                    {
                        string digito = (i % 10).ToString();
                        if (GUILayout.Button(digito, boton) && g.Code.Length < 4) g.Code += digito;
                    }
                    GUILayout.EndHorizontal();
                    if (GUILayout.Button("Borrar", boton)) g.Code = "";
                    if (GUILayout.Button("Abrir", boton)) g.SubmitCode();
                    if (GUILayout.Button("Cerrar", boton)) g.ClosePanel();
                }
                else if (g.Panel == "decision")
                {
                    GUILayout.Label("EXPEDIENTE SECRETO", titulo);
                    GUILayout.Label(StoryData.Get("secret").description, texto);
                    if (GUILayout.Button("Conservar pruebas (-15% Ira)", boton)) g.DecideEvidence(true);
                    if (GUILayout.Button("Destruir pruebas (+45% Ira)", boton)) g.DecideEvidence(false);
                }
                else if (g.Panel == "generator")
                {
                    GUILayout.Label("GENERADOR ELÉCTRICO", titulo);
                    GUILayout.BeginHorizontal();
                    for (int i = 0; i < 3; i++)
                        if (GUILayout.Button("Fase " + (char)('A' + i) + ": " + (g.Switches[i] ? "ON" : "OFF"), boton)) g.Switches[i] = !g.Switches[i];
                    GUILayout.EndHorizontal();
                    if (GUILayout.Button("Encender Generador", boton)) g.Generator();
                    if (GUILayout.Button("Cerrar", boton)) g.ClosePanel();
                }
                else if (g.Panel == "ending")
                {
                    GUILayout.Label("DECISIÓN FINAL", titulo);
                    if (GUILayout.Button("Contar la verdad", boton)) g.End(true);
                    if (GUILayout.Button("Guardar silencio", boton)) g.End(false);
                }

                GUILayout.EndArea();
                return;
            }

            if (g.Player != null && g.Player.Aiming)
            {
                DibujarCamara(g);
                return;
            }

            string estado = g.Data.anger < 45f ? "Calmo (Observa)" : (g.Data.anger < 70f ? "Inquieto (Observa)" : "FURIOSO (PERSIGUE)");
            GUI.Label(new Rect(20, 20, 820, 36), "OBJETIVO: " + g.Objective(), texto);
            GUI.Label(new Rect(20, 54, 420, 30), "Ira de Sebastián: " + Mathf.RoundToInt(g.Data.anger) + "% — " + estado, texto);

            GUI.Box(new Rect(20, 84, 240, 14), "");
            Color prev = GUI.color;
            GUI.color = g.Data.anger < 45f ? new Color(0.3f, 0.75f, 0.9f) : (g.Data.anger < 70f ? new Color(0.95f, 0.7f, 0.2f) : new Color(0.95f, 0.2f, 0.2f));
            GUI.DrawTexture(new Rect(22, 86, 236f * (g.Data.anger / 100f), 10), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(22 + 236f * 0.70f, 83, 2, 16), Texture2D.whiteTexture);
            GUI.color = prev;

            GUI.Label(new Rect(960, 20, 300, 36), "Videocámara: " + Mathf.CeilToInt(g.Data.battery) + "% | Pilas: " + g.Data.spareBatteries, texto);
            GUI.Label(new Rect(960, 50, 300, 30), "[Clic Derecho] Mirar por cámara", texto);

            if (g.Player != null)
            {
                GUI.Label(new Rect(20, 622, 160, 28), "RESISTENCIA", texto);
                GUI.Box(new Rect(20, 650, 220, 18), "");
                GUI.color = g.Player.Exhausted ? new Color(0.9f, 0.25f, 0.2f) : new Color(0.3f, 0.85f, 0.55f);
                GUI.DrawTexture(new Rect(22, 652, 216f * g.Player.Stamina, 14), Texture2D.whiteTexture);
                GUI.color = prev;
            }

            if (g.Player != null && g.Player.Target != null && !g.Player.IsHiding)
                GUI.Label(new Rect(440, 520, 400, 40), "[E] " + g.Player.Target.Prompt, texto);
            if (Time.unscaledTime < g.MessageUntil)
                GUI.Label(new Rect(260, 640, 800, 40), g.Message, texto);

            GUI.Label(new Rect(20, 682, 1240, 30), "[WASD] Mover | [Shift] Correr | [Ctrl] Agacharse | [Clic Der] Cámara | [E] Interactuar | [Tab] Inventario", texto);
        }

        private void DibujarCamara(GameSession g)
        {
            Color prev = GUI.color;
            bool irOn = g.Player.NightVisionActive;
            bool enfocando = g.Enemy != null && g.Enemy.Active && g.Enemy.IsCameraSlowed;

            GUI.color = irOn ? new Color(0.05f, 0.24f, 0.10f, 0.20f) : new Color(0.05f, 0.08f, 0.14f, 0.14f);
            GUI.DrawTexture(new Rect(0, 0, 1280, 720), Texture2D.whiteTexture);

            GUI.color = new Color(0f, 0f, 0f, 0.42f);
            GUI.DrawTexture(new Rect(0, 0, 1280, 38), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(0, 682, 1280, 38), Texture2D.whiteTexture);

            // 4 esquinas del visor + cruceta central
            GUI.color = enfocando ? new Color(1f, 0.25f, 0.2f, 0.9f) : new Color(0.85f, 0.95f, 0.88f, 0.85f);
            for (int i = 0; i < 4; i++)
            {
                float x = (i % 2 == 0) ? 48f : 1232f;
                float y = (i < 2) ? 48f : 672f;
                float sx = (i % 2 == 0) ? 55f : -55f;
                float sy = (i < 2) ? 55f : -55f;
                GUI.DrawTexture(new Rect(x, y, sx, (i < 2) ? 3f : -3f), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(x, y, (i % 2 == 0) ? 3f : -3f, sy), Texture2D.whiteTexture);
            }
            GUI.DrawTexture(new Rect(618, 359, 44, 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(639, 338, 2, 44), Texture2D.whiteTexture);

            GUI.color = (Mathf.FloorToInt(Time.unscaledTime * 2f) % 2 == 0) ? Color.red : new Color(0.4f, 0.1f, 0.1f);
            GUI.DrawTexture(new Rect(72, 66, 14, 14), Texture2D.whiteTexture);

            GUI.color = Color.white;
            int seg = Mathf.FloorToInt(Time.timeSinceLevelLoad);
            GUI.Label(new Rect(94, 60, 260, 28), string.Format("REC   TC 00:{0:00}:{1:00}", (seg / 60) % 60, seg % 60), camText);
            GUI.Label(new Rect(72, 86, 360, 28), "LUZ INFRARROJA [Q]: " + (irOn ? "ENCENDIDA" : "APAGADA"), camText);
            GUI.Label(new Rect(72, 112, 360, 28), "IRA SEBASTIÁN: " + Mathf.RoundToInt(g.Data.anger) + "% (Furia >= 70%)", camText);

            GUI.Box(new Rect(1060, 62, 110, 24), "");
            GUI.color = g.Data.battery < 20f ? Color.red : new Color(0.35f, 0.95f, 0.5f);
            GUI.DrawTexture(new Rect(1063, 65, 104f * (g.Data.battery / 100f), 18), Texture2D.whiteTexture);

            GUI.color = Color.white;
            GUI.Label(new Rect(1060, 90, 200, 28), Mathf.CeilToInt(g.Data.battery) + "%  |  PILAS: " + g.Data.spareBatteries, camText);
            GUI.Label(new Rect(1020, 116, 220, 28), Input.GetMouseButton(0) ? "ZOOM: 2.0x [ACTIVO]" : "ZOOM: 1.0x [CLIC IZQ]", camText);

            if (enfocando)
            {
                GUI.color = new Color(1f, 0.25f, 0.2f);
                GUI.Label(new Rect(340, 575, 600, 34), "⚠ SEBASTIÁN ENFOCADO — RALENTIZANDO ENTIDAD", camAlert);
            }
            else if (g.Enemy != null && g.Enemy.Active && g.Enemy.TargetDistance < 14f)
            {
                GUI.color = new Color(1f, 0.8f, 0.25f);
                GUI.Label(new Rect(340, 575, 600, 34), "⚠ INTERFERENCIA CERCANA — MIRA POR EL VISOR", camAlert);
            }

            GUI.color = Color.white;
            if (g.Player.Target != null && !g.Player.IsHiding)
                GUI.Label(new Rect(440, 520, 400, 40), "[E] " + g.Player.Target.Prompt, texto);
            if (Time.unscaledTime < g.MessageUntil)
                GUI.Label(new Rect(260, 625, 800, 35), g.Message, texto);

            GUI.Label(new Rect(72, 686, 1150, 28), "DVCAM-1985  |  [Clic Der] Bajar Cámara  |  [Q] Luz IR  |  [Clic Izq] Zoom  |  [R] Recargar Pila  |  [E] Interactuar", camText);
            GUI.color = prev;
        }
    }
}