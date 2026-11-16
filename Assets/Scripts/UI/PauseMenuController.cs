using UnityEngine;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    #region CLASS_VARIABLES

    [SerializeField] private Slider volumeSlider;
    [SerializeField] private AudioClip buttonClickClip;

    #endregion

    #region UNITY_METHODS

    private void OnEnable()
    {
        // Actualiza el slider cada vez que se abre el menu de pausa
        if (volumeSlider != null)
        {
            volumeSlider.SetValueWithoutNotify(AudioManager.Instance.CurrentVolume);
        }
    }

    #endregion

    #region CLASS_METHODS

    public void ResumeGame()
    {
        AudioManager.Instance.PlaySFX(buttonClickClip);
        GameManager.Instance.ResumeGame();
    }

    public void RestartLevel()
    {
        AudioManager.Instance.PlaySFX(buttonClickClip);
        GameManager.Instance.RestartLevel();
    }

    public void ReturnToMainMenu()
    {
        AudioManager.Instance.PlaySFX(buttonClickClip);
        GameManager.Instance.LoadMainMenu();
    }

    public void OnVolumeChanged(float value)
    {
        AudioManager.Instance.SetMasterVolume(value);
    }

    #endregion
}
