using UnityEngine;
using UnityEngine.Audio;

namespace Ecos
{
    /// <summary>
    /// AudioManager using GenericSingleton and AudioMixer support, bridging with AudioDirector.
    /// </summary>
    public class AudioManager : GenericSingleton<AudioManager>
    {
        public AudioMixer audioMixer;
        public AudioDirector director;

        [Range(0.001f, 1f)] public float sfxVolume = 0.7f;
        [Range(0.001f, 1f)] public float musicVolume = 0.72f;

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp(volume, 0.0001f, 1f);
            if (audioMixer != null)
            {
                audioMixer.SetFloat("SFXVolume", Mathf.Log10(sfxVolume) * 20f);
            }
            if (GameSession.Instance != null)
            {
                GameSession.Instance.HorrorVolume = sfxVolume;
            }
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp(volume, 0.0001f, 1f);
            if (audioMixer != null)
            {
                audioMixer.SetFloat("MusicVolume", Mathf.Log10(musicVolume) * 20f);
            }
            if (GameSession.Instance != null)
            {
                GameSession.Instance.MusicVolume = musicVolume;
            }
        }

        public void PlaySFX(string clipName, float volume = 0.45f)
        {
            if (director != null)
            {
                director.Play(clipName, volume * sfxVolume);
            }
        }
    }
}
