using UnityEngine;

public class DoorController : Interactable
{
    #region CLASS_VARIABLES

    [SerializeField] private Transform doorHinge;
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private float openAngle = 95f;
    [SerializeField] private float openSpeed = 3f;
    [SerializeField] private bool isLocked = false;
    [SerializeField] private string lockedMessage = "Puerta bloqueada. Ingresa el codigo en el teclado numerico.";

    [Header("Sonidos")]
    [SerializeField] private AudioClip openClip;
    [SerializeField] private AudioClip lockedClip;

    private bool _isOpen;
    private Quaternion _closedRotation;
    private Quaternion _openRotation;

    #endregion

    #region UNITY_METHODS

    private void Awake()
    {
        if (doorHinge == null) doorHinge = transform;
        _closedRotation = doorHinge.localRotation;
        _openRotation = _closedRotation * Quaternion.Euler(0f, openAngle, 0f);
    }

    private void Update()
    {
        if (doorAnimator != null || doorHinge == null) return;

        // Rota la bisagra de a poco hacia la rotacion abierta o cerrada
        Quaternion target = _isOpen ? _openRotation : _closedRotation;
        if (Quaternion.Angle(doorHinge.localRotation, target) > 0.1f)
        {
            doorHinge.localRotation = Quaternion.Slerp(doorHinge.localRotation, target, openSpeed * Time.deltaTime);
        }
    }

    #endregion

    #region CLASS_METHODS

    public override string GetInteractionText()
    {
        if (isLocked) return "Puerta cerrada (Teclado numerico)";
        return _isOpen ? "Cerrar puerta" : "Abrir puerta";
    }

    public override void Interact(PlayerInteraction player)
    {
        if (isLocked)
        {
            AudioManager.Instance.PlaySFX(lockedClip);
            UIManager.Instance.ShowMessage(lockedMessage, 2.5f);
            return;
        }

        ToggleDoor();
    }

    // La llama KeypadPuzzle cuando el jugador ingresa 1985
    public void UnlockAndOpen()
    {
        isLocked = false;
        if (!_isOpen) ToggleDoor();
    }

    public void ToggleDoor()
    {
        _isOpen = !_isOpen;
        if (doorAnimator != null) doorAnimator.SetBool("IsOpen", _isOpen);
        AudioManager.Instance.PlaySFX(openClip);
    }

    #endregion

    #region GET_SET

    public bool IsLocked => isLocked;
    public bool IsOpen => _isOpen;

    #endregion
}
