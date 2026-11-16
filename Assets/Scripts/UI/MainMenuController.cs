using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    #region CLASS_VARIABLES

    [SerializeField] private string firstLevelSceneName = "HospitalNivel1";
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private AudioClip menuMusicClip;
    [SerializeField] private AudioClip buttonClickClip;

    #endregion

    #region UNITY_METHODS

    private void Start()
    {
        Time.timeScale = 1f;
        Application.targetFrameRate = 60;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (menuMusicClip != null) AudioManager.Instance.PlayMusic(menuMusicClip, 0.5f);

        // Sincroniza el slider con el volumen guardado
        if (volumeSlider != null) volumeSlider.SetValueWithoutNotify(AudioManager.Instance.CurrentVolume);
    }

    #endregion

    #region CLASS_METHODS

    public void PlayGame()
    {
        AudioManager.Instance.PlaySFX(buttonClickClip);
        SceneManager.LoadScene(firstLevelSceneName);
    }

    public void ToggleControlsPanel()
    {
        AudioManager.Instance.PlaySFX(buttonClickClip);
        if (controlsPanel != null) controlsPanel.SetActive(!controlsPanel.activeSelf);
    }

    public void OnVolumeChanged(float value)
    {
        AudioManager.Instance.SetMasterVolume(value);
    }

    public void QuitGame()
    {
        AudioManager.Instance.PlaySFX(buttonClickClip);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    #endregion
}
