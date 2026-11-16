using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : GenericSingleton<GameManager>
{
    #region CLASS_VARIABLES

    [Header("Jugador")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private VideoCameraController videoCamera;

    [Header("Secuencia Final")]
    [SerializeField] private GameObject sebastianSilhouette;
    [SerializeField] private FlickeringLight endingCorridorLight;
    [SerializeField] private AudioClip childSoundClip;
    [SerializeField] private AudioClip stingerClip;

    [Header("Escenas")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool _isPaused;
    private bool _isEnding;

    #endregion

    #region UNITY_METHODS

    protected override void Initialization()
    {
        base.Initialization();
        Application.targetFrameRate = 60;
        Time.timeScale = 1f;

        if (sebastianSilhouette != null) sebastianSilhouette.SetActive(false);
        LockCursor(true);
    }

    private void Start()
    {
        UIManager.Instance.ShowMessage("Hospital Roy H. Glover - Clic derecho: Camara | F: Vision nocturna | E: Interactuar", 5f);
    }

    private void Update()
    {
        if (_isEnding) return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            HandleEscapeKey();
        }
    }

    #endregion

    #region CLASS_METHODS

    // Esc cierra el panel que este abierto o pausa el juego
    private void HandleEscapeKey()
    {
        if (UIManager.Instance.IsDocumentOpen)
        {
            UIManager.Instance.CloseDocumentPanel();
            return;
        }

        if (UIManager.Instance.IsKeypadOpen && UIManager.Instance.ActiveKeypad != null)
        {
            UIManager.Instance.ActiveKeypad.CloseKeypad();
            return;
        }

        if (_isPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        _isPaused = true;
        Time.timeScale = 0f;
        SetGameplayInputEnabled(false);
        UIManager.Instance.ShowPausePanel(true);
    }

    public void ResumeGame()
    {
        _isPaused = false;
        Time.timeScale = 1f;
        UIManager.Instance.ShowPausePanel(false);
        SetGameplayInputEnabled(true);
    }

    // Activa o desactiva los controles del jugador al abrir menus o notas
    public void SetGameplayInputEnabled(bool enabled)
    {
        if (playerMovement != null) playerMovement.enabled = enabled;
        if (playerInteraction != null) playerInteraction.enabled = enabled;
        if (videoCamera != null) videoCamera.enabled = enabled;
        LockCursor(enabled);
    }

    private void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    public void TriggerEndingSequence()
    {
        if (_isEnding) return;
        _isEnding = true;
        StartCoroutine(EndingSequenceCoroutine());
    }

    // Secuencia final: sonido infantil, parpadeo de luz, silueta y pantalla de fin
    private IEnumerator EndingSequenceCoroutine()
    {
        UIManager.Instance.ShowMessage("Victoria, espera a que continuemos la historia...", 4f);
        AudioManager.Instance.PlaySFX(childSoundClip, 1f);

        if (endingCorridorLight != null) endingCorridorLight.TriggerIntenseFlicker(3f);
        if (sebastianSilhouette != null) sebastianSilhouette.SetActive(true);

        yield return new WaitForSeconds(1.2f);
        AudioManager.Instance.PlaySFX(stingerClip, 0.9f);

        yield return new WaitForSeconds(1.6f);

        if (sebastianSilhouette != null) sebastianSilhouette.SetActive(false);
        if (endingCorridorLight != null) endingCorridorLight.SetForcedOff(true);

        yield return new WaitForSeconds(0.8f);

        SetGameplayInputEnabled(false);
        UIManager.Instance.ShowEndingPanel();
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    #endregion

    #region GET_SET

    public bool IsPaused => _isPaused;

    #endregion
}
