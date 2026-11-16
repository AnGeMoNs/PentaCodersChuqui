using System.Collections.Generic;
using UnityEngine;

namespace Ecos
{
    public class SebastianAI : MonoBehaviour
    {
        public Transform visual;
        public bool Active { get; private set; }
        public bool SeesTarget { get; private set; }
        public bool IsCameraSlowed { get; private set; }
        public float TargetDistance { get; private set; } = 100f;
        public float Speed { get; private set; }

        private List<Vector3> ruta = new List<Vector3>();
        private Vector3 ultimaPos;
        private float tiempoActivo, cooldown = 30f, relojRuta, relojVoz, relojPasos;
        private bool vioEsconderse, yaDetectado;
        private SkinnedMeshRenderer malla;

        public void RegisterLamp(Light l) { }

        void Start()
        {
            if (visual != null)
            {
                visual.gameObject.layer = PlayerRig.GhostLayer;
                malla = visual.GetComponentInChildren<SkinnedMeshRenderer>(true);
            }
        }

        public void ResetActor()
        {
            Active = SeesTarget = IsCameraSlowed = vioEsconderse = yaDetectado = false;
            TargetDistance = 100f;
            Speed = 0f;
            cooldown = 11f;
            ruta.Clear();
            if (visual != null) visual.gameObject.SetActive(false);
            if (GameSession.Instance != null) GameSession.Instance.Audio.UpdateHeartbeat(0f, false);
        }

        public void HearNoise(Vector3 pos, float radio)
        {
            if (!Active) { cooldown = Mathf.Max(2f, cooldown - 3f); return; }
            if (Vector3.Distance(transform.position, pos) <= radio) { ultimaPos = pos; relojRuta = 0f; }
        }

        public void OnPlayerHid(Vector3 pos, bool visto)
        {
            vioEsconderse = visto;
            if (visto) ultimaPos = pos;
            else SeesTarget = false;
        }

        public void Encounter()
        {
            GameSession g = GameSession.Instance;
            if (g == null || g.Data.zone < 1 || Active) return;

            foreach (Vector2Int celda in g.Map.Cells)
            {
                if (celda.y / 7 != g.Data.zone) continue;
                Vector3 punto = g.Map.Position(celda);
                float dist = Vector3.Distance(punto, g.Player.transform.position);
                if (dist >= 8f && dist <= 15f && g.Map.IsWalkable(punto))
                {
                    transform.position = punto;
                    ultimaPos = g.Player.transform.position;
                    ruta.Clear();
                    Active = true;
                    yaDetectado = false;
                    relojVoz = 3.5f;
                    tiempoActivo = g.Data.anger >= 70f ? 22f : 12f;
                    if (visual != null) visual.gameObject.SetActive(true);
                    g.Audio.PlayGhostAppear();
                    return;
                }
            }
        }

        void Update()
        {
            GameSession g = GameSession.Instance;
            if (g == null || !g.Playing)
            {
                if (g != null) g.Audio.UpdateHeartbeat(0f, false);
                return;
            }

            if (!Active)
            {
                g.Audio.UpdateHeartbeat(0f, false);
                cooldown -= Time.deltaTime;
                if (cooldown <= 0f && g.Data.zone >= 1) Encounter();
                return;
            }

            // Visible únicamente a través del visor de la videocámara
            bool usandoCamara = g.Player != null && g.Player.Aiming;
            if (malla == null && visual != null) malla = visual.GetComponentInChildren<SkinnedMeshRenderer>(true);
            if (malla != null) malla.enabled = usandoCamara;

            tiempoActivo -= Time.deltaTime;
            relojRuta -= Time.deltaTime;
            relojVoz -= Time.deltaTime;
            TargetDistance = Vector3.Distance(transform.position, g.Player.transform.position);

            float cercania = Mathf.Clamp01((16f - TargetDistance) / 14f);
            g.Audio.UpdateHeartbeat(cercania, true);

            // Línea de visión
            SeesTarget = (!g.Player.IsHiding || vioEsconderse)
                && TargetDistance < 17f
                && Physics.Linecast(transform.position + Vector3.up, g.Player.view.transform.position, out RaycastHit hit, ~0, QueryTriggerInteraction.Ignore)
                && hit.collider.GetComponent<PlayerRig>() != null;

            if (SeesTarget)
            {
                if (!yaDetectado) { yaDetectado = true; g.Audio.PlayGhostSpot(); }
                ultimaPos = g.Player.transform.position;
                if (g.Data.anger >= 70f) tiempoActivo = Mathf.Max(tiempoActivo, 6f);
            }

            Vector3 pantalla = g.Player.view.WorldToViewportPoint(transform.position + Vector3.up);
            IsCameraSlowed = SeesTarget && usandoCamara && pantalla.z > 0f && pantalla.x > 0.22f && pantalla.x < 0.78f;

            if (tiempoActivo <= 0f) { ResetActor(); return; }

            bool furioso = g.Data.anger >= 70f;
            if (relojVoz <= 0f) { relojVoz = furioso ? 4f : 6.5f; g.Audio.PlayGhostChase(); }

            // Si la ira es menor al 70%, solo observa desde lejos
            if (!furioso)
            {
                Speed = 0f;
                Vector3 mirar = g.Player.transform.position - transform.position;
                mirar.y = 0f;
                if (mirar.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(-mirar);
                if (TargetDistance < 3.2f) { g.Audio.PlayGhostAppear(); ResetActor(); }
                return;
            }

            // Si la ira >= 70%, persigue
            Vector3 destino = ultimaPos;
            if (SeesTarget && g.Map.ClearSegment(transform.position, ultimaPos))
            {
                ruta.Clear();
            }
            else
            {
                if (relojRuta <= 0f || ruta.Count == 0) { relojRuta = 0.25f; ruta = g.Map.Path(transform.position, ultimaPos); }
                while (ruta.Count > 0 && Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(ruta[0].x, ruta[0].z)) < 0.35f)
                    ruta.RemoveAt(0);
                if (ruta.Count > 0) destino = ruta[0];
            }

            Vector3 dir = destino - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(-dir), Time.deltaTime * 10f);
                Speed = IsCameraSlowed ? 1.4f : 3.3f + ((g.Data.anger - 70f) / 30f) * 1.1f;
                transform.position = Vector3.MoveTowards(transform.position, new Vector3(destino.x, transform.position.y, destino.z), Speed * Time.deltaTime);

                relojPasos += Speed * Time.deltaTime;
                if (relojPasos >= 1.45f) { relojPasos = 0f; g.Audio.Play("Step", Mathf.Lerp(0.15f, 0.58f, cercania)); }
            }
            else Speed = 0f;

            if (TargetDistance < 1.15f && (!g.Player.IsHiding || vioEsconderse))
            {
                g.Audio.PlayGhostSpot();
                g.Lose("Sebastián entró en furia y te alcanzó. Mantén su ira bajo 70% resolviendo acertijos o escóndete.");
            }
        }
    }
}