using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class UIManager : GenericSingleton<UIManager>
{
    #region CLASS_VARIABLES

    [Header("HUD General")]
    [SerializeField] private TMP_Text interactionText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Image staminaFillImage;

    [Header("HUD de Camara")]
    [SerializeField] private GameObject cameraHudPanel;
    [SerializeField] private GameObject nightVisionOverlay;
    [SerializeField] private RawImage noiseRawImage;
    [SerializeField] private GameObject recIndicatorDot;
    [SerializeField] private TMP_Text recordingTimeText;
    [SerializeField] private Image batteryFillImage;
    [SerializeField] private TMP_Text batteryPercentText;
    [SerializeField] private TMP_Text nightVisionStateText;

    [Header("Panel de Documentos")]
    [SerializeField] private GameObject documentPanel;
    [SerializeField] private TMP_Text documentTitleText;
    [SerializeField] private TMP_Text documentBodyText;

    [Header("Panel de Teclado")]
    [SerializeField] private GameObject keypadPanel;
    [SerializeField] private TMP_Text keypadDisplayText;

    [Header("Pausa y Final")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject endingPanel;

    private float _messageTimer;
    private float _recBlinkTimer;
    private int _lastSecondShown = -1;
    private int _documentOpenedFrame;
    private Action _onDocumentClosed;
    private KeypadPuzzle _activeKeypad;
    private Texture2D _noiseTexture;

    #endregion

    #region UNITY_METHODS

    protected override void Initialization()
    {
        base.Initialization();
        CreateNoiseTexture();

        // Apagamos todos los paneles al partir el nivel
        HideInteractionText();
        if (messageText != null) messageText.gameObject.SetActive(false);
        if (cameraHudPanel != null) cameraHudPanel.SetActive(false);
        if (nightVisionOverlay != null) nightVisionOverlay.SetActive(false);
        if (documentPanel != null) documentPanel.SetActive(false);
        if (keypadPanel != null) keypadPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (endingPanel != null) endingPanel.SetActive(false);
    }

    private void Update()
    {
        // Oculta los mensajes temporales cuando termina su tiempo
        if (_messageTimer > 0f)
        {
            _messageTimer -= Time.deltaTime;
            if (_messageTimer <= 0f && messageText != null)
            {
                messageText.gameObject.SetActive(false);
            }
        }

        // Parpadeo del punto rojo REC y ruido de la vision nocturna
        if (cameraHudPanel != null && cameraHudPanel.activeSelf)
        {
            _recBlinkTimer += Time.unscaledDeltaTime;
            if (recIndicatorDot != null)
            {
                bool dotOn = (Mathf.FloorToInt(_recBlinkTimer * 2f) % 2) == 0;
                if (recIndicatorDot.activeSelf != dotOn) recIndicatorDot.SetActive(dotOn);
            }

            if (noiseRawImage != null && nightVisionOverlay != null && nightVisionOverlay.activeSelf)
            {
                noiseRawImage.uvRect = new Rect(UnityEngine.Random.value, UnityEngine.Random.value, 3f, 3f);
            }
        }

        // Permite cerrar la nota presionando E de nuevo
        if (IsDocumentOpen && Time.frameCount != _documentOpenedFrame)
        {
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                CloseDocumentPanel();
            }
        }
    }

    #endregion

    #region CLASS_METHODS

    // Genera una textura pequena con lineas y ruido verde para el visor
    private void CreateNoiseTexture()
    {
        const int size = 128;
        _noiseTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        _noiseTexture.wrapMode = TextureWrapMode.Repeat;

        Color32[] pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            bool line = (y % 3) == 0;
            for (int x = 0; x < size; x++)
            {
                byte g = (byte)UnityEngine.Random.Range(50, 215);
                byte a = (byte)(line ? 48 : UnityEngine.Random.Range(14, 36));
                pixels[y * size + x] = new Color32(25, g, 40, a);
            }
        }

        _noiseTexture.SetPixels32(pixels);
        _noiseTexture.Apply();

        if (noiseRawImage != null) noiseRawImage.texture = _noiseTexture;
    }

    public void ShowInteractionText(string text)
    {
        if (interactionText == null) return;
        interactionText.text = text;
        if (!interactionText.gameObject.activeSelf) interactionText.gameObject.SetActive(true);
    }

    public void HideInteractionText()
    {
        if (interactionText != null && interactionText.gameObject.activeSelf)
        {
            interactionText.gameObject.SetActive(false);
        }
    }

    public void ShowMessage(string message, float duration = 3f)
    {
        if (messageText == null) return;
        messageText.text = message;
        messageText.gameObject.SetActive(true);
        _messageTimer = duration;
    }

    public void UpdateStamina(float normalized, bool exhausted)
    {
        if (staminaFillImage == null) return;
        staminaFillImage.fillAmount = Mathf.Clamp01(normalized);
        staminaFillImage.color = exhausted
            ? new Color(0.95f, 0.3f, 0.25f, 0.9f)
            : new Color(0.75f, 0.88f, 0.78f, 0.85f);
    }

    public void ShowCameraHUD(bool visible)
    {
        if (cameraHudPanel != null) cameraHudPanel.SetActive(visible);
    }

    public void ShowNightVisionOverlay(bool active)
    {
        if (nightVisionOverlay != null) nightVisionOverlay.SetActive(active);
        if (nightVisionStateText != null)
        {
            nightVisionStateText.text = active ? "VISION NOCTURNA: ON [F]" : "VISION NOCTURNA: OFF [F]";
            nightVisionStateText.color = active ? new Color(0.45f, 1f, 0.55f) : Color.white;
        }
    }

    public void UpdateRecordingTime(float seconds)
    {
        if (recordingTimeText == null) return;

        int totalSeconds = Mathf.FloorToInt(seconds);
        if (totalSeconds == _lastSecondShown) return;
        _lastSecondShown = totalSeconds;

        int mins = (totalSeconds % 3600) / 60;
        int secs = totalSeconds % 60;
        recordingTimeText.text = string.Format("REC 00:{0:00}:{1:00}", mins, secs);
    }

    public void UpdateBattery(float percent, bool isLow)
    {
        float normalized = Mathf.Clamp01(percent / 100f);
        if (batteryFillImage != null)
        {
            batteryFillImage.fillAmount = normalized;
            batteryFillImage.color = isLow ? new Color(1f, 0.25f, 0.25f) : new Color(0.35f, 1f, 0.45f);
        }

        if (batteryPercentText != null)
        {
            batteryPercentText.text = "BATERIA: " + Mathf.RoundToInt(percent) + "%";
            batteryPercentText.color = isLow ? new Color(1f, 0.35f, 0.35f) : Color.white;
        }
    }

    public void OpenDocumentPanel(string title, string body, Action onCloseCallback = null)
    {
        _documentOpenedFrame = Time.frameCount;
        _onDocumentClosed = onCloseCallback;
        if (documentTitleText != null) documentTitleText.text = title;
        if (documentBodyText != null) documentBodyText.text = body;
        if (documentPanel != null) documentPanel.SetActive(true);
        HideInteractionText();
    }

    public void CloseDocumentPanel()
    {
        if (documentPanel != null) documentPanel.SetActive(false);
        Action cb = _onDocumentClosed;
        _onDocumentClosed = null;
        if (cb != null) cb.Invoke();
    }

    public void OpenKeypadPanel(KeypadPuzzle keypad)
    {
        _activeKeypad = keypad;
        if (keypadPanel != null) keypadPanel.SetActive(true);
        HideInteractionText();
    }

    public void CloseKeypadPanel()
    {
        _activeKeypad = null;
        if (keypadPanel != null) keypadPanel.SetActive(false);
    }

    public void UpdateKeypadDisplay(string displayValue)
    {
        if (keypadDisplayText != null) keypadDisplayText.text = displayValue;
    }

    // Metodos conectados a los botones del teclado numerico en el Canvas
    public void OnKeypadDigitButton(string digit)
    {
        if (_activeKeypad != null) _activeKeypad.PressNumber(digit);
    }

    public void OnKeypadClearButton()
    {
        if (_activeKeypad != null) _activeKeypad.ClearInput();
    }

    public void OnKeypadSubmitButton()
    {
        if (_activeKeypad != null) _activeKeypad.SubmitCode();
    }

    public void OnKeypadCloseButton()
    {
        if (_activeKeypad != null) _activeKeypad.CloseKeypad();
    }

    public void ShowPausePanel(bool show)
    {
        if (pausePanel != null) pausePanel.SetActive(show);
    }

    public void ShowEndingPanel()
    {
        if (cameraHudPanel != null) cameraHudPanel.SetActive(false);
        if (endingPanel != null) endingPanel.SetActive(true);
    }

    #endregion

    #region GET_SET

    public bool IsDocumentOpen => documentPanel != null && documentPanel.activeSelf;
    public bool IsKeypadOpen => keypadPanel != null && keypadPanel.activeSelf;
    public KeypadPuzzle ActiveKeypad => _activeKeypad;

    #endregion
}
