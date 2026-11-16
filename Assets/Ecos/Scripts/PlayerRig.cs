using UnityEngine;

namespace Ecos
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerRig : MonoBehaviour
    {
        public const int GhostLayer = 9;

        public Camera view;
        public Light infrared;
        public Light eyeLight;
        public Transform cameraModel;

        [Header("Ajustes de Movimiento y Cámara")]
        public float velocidadCaminar = 3.8f;
        public float velocidadCorrer = 6.5f;
        public float velocidadAgachado = 1.9f;
        public float sensibilidadRaton = 1.6f;
        public float gravedad = -19.6f;

        public float Stamina { get; private set; } = 1f;
        public bool Exhausted { get; private set; }
        public bool Running { get; private set; }
        public bool Crouching { get; private set; }
        public bool Aiming { get; private set; }
        public bool IsHiding { get; private set; }
        public bool HidingUnderBed { get; private set; }
        public bool NightVisionEnabled { get; private set; } = true;
        public bool NightVisionActive => GameSession.Instance != null && GameSession.Instance.Playing && Aiming && NightVisionEnabled && GameSession.Instance.Data.battery > 0f;
        public Interactable Target { get; private set; }

        private CharacterController controller;
        private float rotacionVertical;
        private Vector3 velocidadCaida;
        private float relojPasos;
        private Collider colisionEscondite;
        private Vector3 posAntesEsconderse;
        private float rotAntesEsconderse;
        private bool ratonConfigurado = true;

        void Awake() => controller = GetComponent<CharacterController>();

        void Start()
        {
            try { Input.GetAxis("Mouse X"); Input.GetAxis("Mouse Y"); }
            catch { ratonConfigurado = false; }
        }

        private float LeerMouseX() => ratonConfigurado ? Input.GetAxis("Mouse X") : 0f;
        private float LeerMouseY() => ratonConfigurado ? Input.GetAxis("Mouse Y") : 0f;

        private void ActualizarCapaFantasma()
        {
            if (view == null) return;
            int bit = 1 << GhostLayer;
            view.cullingMask = Aiming ? (view.cullingMask | bit) : (view.cullingMask & ~bit);
        }

        void Update()
        {
            GameSession g = GameSession.Instance;
            if (g == null || !g.Ready || !g.Playing)
            {
                if (infrared != null) infrared.enabled = false;
                if (eyeLight != null) eyeLight.enabled = false;
                if (cameraModel != null) cameraModel.gameObject.SetActive(false);
                Target = null;
                return;
            }

            if (Input.GetKeyDown(KeyCode.R)) g.ReloadCamera();
            if (Input.GetMouseButtonDown(1))
            {
                Aiming = !Aiming;
                ActualizarCapaFantasma();
            }
            if (Aiming && Input.GetKeyDown(KeyCode.Q)) NightVisionEnabled = !NightVisionEnabled;

            if (cameraModel != null) cameraModel.gameObject.SetActive(Aiming);
            if (infrared != null) infrared.enabled = NightVisionActive;
            if (eyeLight != null)
            {
                eyeLight.enabled = true;
                eyeLight.intensity = NightVisionActive ? 0.25f : 0.42f;
            }

            if (NightVisionActive)
                g.Data.battery = Mathf.Max(0f, g.Data.battery - Time.deltaTime * 0.35f);

            if (IsHiding)
            {
                float sens = g.Sensitivity * 0.5f;
                transform.Rotate(0f, Mathf.Clamp(LeerMouseX() * sens, -25f, 25f), 0f);
                rotacionVertical = Mathf.Clamp(rotacionVertical - LeerMouseY() * sens, -20f, 25f);
                view.transform.localRotation = Quaternion.Euler(rotacionVertical, 0f, 0f);
                if (Input.GetKeyDown(KeyCode.E)) ExitLocker();
                return;
            }

            // 1. Cámara FPS
            float zoom = (Aiming && Input.GetMouseButton(0)) ? 0.5f : 1f;
            float mouseX = LeerMouseX() * sensibilidadRaton * g.Sensitivity * zoom;
            float mouseY = LeerMouseY() * sensibilidadRaton * g.Sensitivity * zoom;

            rotacionVertical = Mathf.Clamp(rotacionVertical - mouseY, -80f, 80f);
            view.transform.localRotation = Quaternion.Euler(rotacionVertical, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);
            view.fieldOfView = Mathf.Lerp(view.fieldOfView, Aiming ? (Input.GetMouseButton(0) ? 42f : 60f) : g.FieldOfView, Time.deltaTime * 10f);

            // 2. Movimiento y Resistencia
            float x = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1f : 0f)
                    - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1f : 0f);
            float z = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1f : 0f)
                    - (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1f : 0f);

            Vector3 direccion = Vector3.ClampMagnitude(transform.right * x + transform.forward * z, 1f);

            Crouching = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            Running = Input.GetKey(KeyCode.LeftShift) && !Crouching && !Aiming && !Exhausted && direccion.sqrMagnitude > 0.01f;

            if (Running)
            {
                Stamina = Mathf.Max(0f, Stamina - Time.deltaTime * 0.25f);
                if (Stamina <= 0f) Exhausted = true;
            }
            else
            {
                Stamina = Mathf.Min(1f, Stamina + Time.deltaTime * 0.18f);
                if (Stamina >= 0.25f) Exhausted = false;
            }

            controller.height = Crouching ? 1.05f : 1.8f;
            controller.center = Vector3.up * (controller.height * 0.5f);
            view.transform.localPosition = Vector3.Lerp(view.transform.localPosition, Vector3.up * (Crouching ? 0.92f : 1.62f), Time.deltaTime * 12f);

            float vel = Crouching ? velocidadAgachado : (Running ? velocidadCorrer : velocidadCaminar);
            controller.Move(direccion * vel * Time.deltaTime);

            if (controller.isGrounded && velocidadCaida.y < 0f) velocidadCaida.y = -2f;
            velocidadCaida.y += gravedad * Time.deltaTime;
            controller.Move(velocidadCaida * Time.deltaTime);

            // Pasos
            if (controller.isGrounded && direccion.sqrMagnitude > 0.01f)
            {
                relojPasos += vel * Time.deltaTime;
                if (relojPasos >= (Running ? 2.2f : 1.6f))
                {
                    relojPasos = 0f;
                    g.Audio.Play("Step", Crouching ? 0.08f : 0.22f);
                    if (g.Enemy != null && !Crouching) g.Enemy.HearNoise(transform.position, Running ? 13f : 5.5f);
                }
            }

            // 3. Interacción con E
            Target = null;
            if (Physics.Raycast(view.transform.position, view.transform.forward, out RaycastHit hit, 2.5f, ~0, QueryTriggerInteraction.Ignore))
            {
                Interactable item = hit.collider.GetComponentInParent<Interactable>();
                if (item != null && item.Available) Target = item;
            }
            if (Input.GetKeyDown(KeyCode.E) && Target != null) Target.Use();
        }

        public void Teleport(SaveData data)
        {
            if (controller == null) controller = GetComponent<CharacterController>();
            controller.enabled = false;
            IsHiding = HidingUnderBed = Aiming = Exhausted = false;
            Stamina = 1f;
            ActualizarCapaFantasma();
            transform.position = new Vector3(data.x, data.y, data.z);
            transform.rotation = Quaternion.Euler(0f, data.yaw, 0f);
            rotacionVertical = 0f;
            velocidadCaida = Vector3.zero;
            view.transform.localPosition = Vector3.up * 1.62f;
            view.transform.localRotation = Quaternion.identity;
            controller.enabled = true;
        }

        public void ToggleLocker(Transform locker)
        {
            if (IsHiding) { ExitLocker(); return; }
            EntrarEscondite(locker, false, 1.48f);
        }

        public void ToggleBedHide(Transform bed)
        {
            if (IsHiding) { ExitLocker(); return; }
            EntrarEscondite(bed, true, 0.24f);
        }

        private void EntrarEscondite(Transform obj, bool esCama, float alturaOjos)
        {
            GameSession g = GameSession.Instance;
            bool visto = g.Enemy != null && g.Enemy.Active && g.Enemy.SeesTarget && g.Enemy.TargetDistance < 11f;
            posAntesEsconderse = transform.position;
            rotAntesEsconderse = transform.eulerAngles.y;
            colisionEscondite = obj.GetComponent<Collider>();
            IsHiding = true;
            HidingUnderBed = esCama;
            Aiming = false;
            ActualizarCapaFantasma();
            controller.enabled = false;

            transform.position = new Vector3(obj.position.x, 0.03f, obj.position.z);
            view.transform.localPosition = Vector3.up * alturaOjos;
            if (g.Enemy != null) g.Enemy.OnPlayerHid(obj.position, visto);
            g.Notify(visto ? "¡Sebastián te vio esconderte!" : "Escondido. Pulsa E para salir.");
        }

        public void ExitLocker()
        {
            if (!IsHiding) return;
            IsHiding = HidingUnderBed = false;
            if (colisionEscondite != null) colisionEscondite.enabled = false;
            Physics.SyncTransforms();
            transform.position = posAntesEsconderse;
            transform.rotation = Quaternion.Euler(0f, rotAntesEsconderse, 0f);
            if (colisionEscondite != null) colisionEscondite.enabled = true;
            view.transform.localPosition = Vector3.up * 1.62f;
            controller.enabled = true;
        }
    }
}