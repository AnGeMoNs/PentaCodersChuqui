using UnityEngine;

namespace Ecos
{
    public class AudioDirector : MonoBehaviour
    {
        private AudioSource musica, efectos, latidos;

        public void Initialize()
        {
            musica = gameObject.AddComponent<AudioSource>();
            musica.loop = true;

            efectos = gameObject.AddComponent<AudioSource>();

            latidos = gameObject.AddComponent<AudioSource>();
            latidos.loop = true;
            latidos.volume = 0f;
            latidos.clip = Resources.Load<AudioClip>("Audio/Heartbeat");
            if (latidos.clip != null) latidos.Play();

            Zone(0, true);
        }

        public void Play(string nombre, float volumen = 0.45f)
        {
            if (efectos == null) return;
            AudioClip clip = Resources.Load<AudioClip>("Audio/" + nombre);
            if (clip != null) efectos.PlayOneShot(clip, volumen);
        }

        public void PlayGhostAppear() => Play("MapOpenSH", 0.55f);
        public void PlayGhostChase() => Play("Error", 0.35f);
        public void PlayGhostSpot() => Play("MandolinStingSH", 0.55f);
        public void PlayItemPickup() => Play("ItemPickupRE", 0.4f);
        public void PlayAt(string nombre, Vector3 pos, float vol = 0.4f) => Play(nombre, vol);

        public void UpdateHeartbeat(float cercania01, bool activo)
        {
            if (latidos == null || latidos.clip == null) return;
            if (!activo || cercania01 <= 0.01f)
            {
                latidos.volume = Mathf.MoveTowards(latidos.volume, 0f, Time.deltaTime * 1.5f);
                return;
            }
            latidos.volume = Mathf.Lerp(0.08f, 0.65f, cercania01);
            latidos.pitch = Mathf.Lerp(0.9f, 1.55f, cercania01);
        }

        public void Zone(int zona, bool enMenu = false)
        {
            if (musica == null) return;
            string pista = (enMenu || zona == 0) ? "SilentPromiseSH" : (zona >= 4 ? "OtherworldIndustrialSH" : "HallwayExploreRE");
            musica.volume = enMenu ? 0.25f : (zona == 0 ? 0.11f : 0.07f);

            AudioClip clip = Resources.Load<AudioClip>("Audio/" + pista);
            if (clip != null && musica.clip != clip) { musica.clip = clip; musica.Play(); }
        }
    }
}