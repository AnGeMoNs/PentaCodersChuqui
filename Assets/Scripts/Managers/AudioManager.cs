using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : PersistentSingleton<AudioManager>
{
    #region CLASS_VARIABLES

    public const string VolumePrefKey = "MasterVolume";

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource ambientSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private string exposedVolumeParam = "MasterVolume";

    [Header("Clips Iniciales")]
    [SerializeField] private AudioClip defaultMusicClip;
    [SerializeField] private AudioClip defaultAmbientClip;

    private float _currentVolume = 0.85f;

    #endregion

    #region CLASS_METHODS

    protected override void Initialization()
    {
        base.Initialization();
        EnsureSources();

        _currentVolume = PlayerPrefs.GetFloat(VolumePrefKey, 0.85f);
        SetMasterVolume(_currentVolume);

        if (defaultMusicClip != null && !musicSource.isPlaying) PlayMusic(defaultMusicClip);
        if (defaultAmbientClip != null && !ambientSource.isPlaying) PlayAmbient(defaultAmbientClip);
    }

    private void EnsureSources()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.spatialBlend = 0f;
            musicSource.volume = 0.45f;
        }
        if (ambientSource == null)
        {
            ambientSource = gameObject.AddComponent<AudioSource>();
            ambientSource.loop = true;
            ambientSource.spatialBlend = 0f;
            ambientSource.volume = 0.4f;
        }
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.spatialBlend = 0f;
            sfxSource.volume = 0.9f;
        }
    }

    public void PlayMusic(AudioClip clip, float volume = 0.45f)
    {
        if (clip == null) return;
        EnsureSources();
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.volume = volume;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayAmbient(AudioClip clip, float volume = 0.4f)
    {
        if (clip == null) return;
        EnsureSources();
        ambientSource.clip = clip;
        ambientSource.volume = volume;
        ambientSource.loop = true;
        ambientSource.Play();
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        EnsureSources();
        sfxSource.PlayOneShot(clip, volume);
    }

    public void SetMasterVolume(float normalizedVolume)
    {
        _currentVolume = Mathf.Clamp01(normalizedVolume);
        AudioListener.volume = _currentVolume;

        if (mainMixer != null)
        {
            float db = _currentVolume > 0.001f ? Mathf.Log10(_currentVolume) * 20f : -80f;
            mainMixer.SetFloat(exposedVolumeParam, db);
        }

        PlayerPrefs.SetFloat(VolumePrefKey, _currentVolume);
        PlayerPrefs.Save();
    }

    #endregion

    #region GET_SET

    public float CurrentVolume => _currentVolume;

    #endregion
}
