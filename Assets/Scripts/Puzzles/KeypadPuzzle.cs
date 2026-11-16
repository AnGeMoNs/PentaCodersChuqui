using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class KeypadPuzzle : Interactable
{
    #region CLASS_VARIABLES

    [SerializeField] private string correctCode = "1985";
    [SerializeField] private DoorController targetDoor;
    [SerializeField] private Renderer statusLightRenderer;
    [SerializeField] private Material unlockedLightMaterial;

    [Header("Sonidos")]
    [SerializeField] private AudioClip keyPressClip;
    [SerializeField] private AudioClip successClip;
    [SerializeField] private AudioClip errorClip;

    private string _currentInput = "";
    private bool _isSolved;
    private bool _isPanelOpen;
    private bool _isChecking;

    #endregion

    #region UNITY_METHODS

    private void Update()
    {
        if (!_isPanelOpen || _isChecking) return;

        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        for (int i = 0; i <= 9; i++)
        {
            if (kb[Key.Digit0 + i].wasPressedThisFrame || kb[Key.Numpad0 + i].wasPressedThisFrame)
            {
                PressNumber(i.ToString());
            }
        }

        if (kb.backspaceKey.wasPressedThisFrame) ClearInput();
        else if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame) SubmitCode();
    }

    #endregion

    #region CLASS_METHODS

    public override string GetInteractionText()
    {
        return _isSolved ? "Teclado desbloqueado" : "Usar teclado numérico";
    }

    public override void Interact(PlayerInteraction player)
    {
        if (_isSolved)
        {
            UIManager.Instance.ShowMessage("Victoria, espera a que continuemos la historia...", 3.5f);
            return;
        }

        OpenKeypad();
    }

    public void OpenKeypad()
    {
        _isPanelOpen = true;
        _currentInput = "";
        _isChecking = false;
        UIManager.Instance.OpenKeypadPanel(this);
        UIManager.Instance.UpdateKeypadDisplay("_ _ _ _");
        GameManager.Instance.SetGameplayInputEnabled(false);
    }

    public void CloseKeypad()
    {
        if (!_isPanelOpen) return;
        _isPanelOpen = false;
        UIManager.Instance.CloseKeypadPanel();
        GameManager.Instance.SetGameplayInputEnabled(true);
    }

    public void PressNumber(string digit)
    {
        if (_isChecking || _currentInput.Length >= 4) return;

        _currentInput += digit;
        AudioManager.Instance.PlaySFX(keyPressClip);

        string padded = _currentInput.PadRight(4, '_');
        UIManager.Instance.UpdateKeypadDisplay(padded[0] + " " + padded[1] + " " + padded[2] + " " + padded[3]);
    }

    public void ClearInput()
    {
        if (_isChecking) return;

        _currentInput = "";
        AudioManager.Instance.PlaySFX(keyPressClip);
        UIManager.Instance.UpdateKeypadDisplay("_ _ _ _");
    }

    public void SubmitCode()
    {
        if (_isChecking) return;

        if (_currentInput == correctCode)
        {
            StartCoroutine(HandleCorrectCode());
        }
        else
        {
            StartCoroutine(HandleWrongCode());
        }
    }

    private IEnumerator HandleCorrectCode()
    {
        _isChecking = true;
        _isSolved = true;

        AudioManager.Instance.PlaySFX(successClip);
        UIManager.Instance.UpdateKeypadDisplay("1 9 8 5");

        if (statusLightRenderer != null && unlockedLightMaterial != null)
        {
            statusLightRenderer.sharedMaterial = unlockedLightMaterial;
        }

        yield return new WaitForSecondsRealtime(0.75f);

        CloseKeypad();
        if (targetDoor != null)
        {
            targetDoor.UnlockAndOpen();
        }
        UIManager.Instance.ShowMessage("Victoria, espera a que continuemos la historia...", 4.5f);
    }

    private IEnumerator HandleWrongCode()
    {
        _isChecking = true;
        AudioManager.Instance.PlaySFX(errorClip);
        UIManager.Instance.UpdateKeypadDisplay("ERROR");

        yield return new WaitForSecondsRealtime(0.75f);

        _currentInput = "";
        _isChecking = false;
        UIManager.Instance.UpdateKeypadDisplay("_ _ _ _");
    }

    #endregion

    #region GET_SET

    public bool IsPanelOpen => _isPanelOpen;

    #endregion
}
