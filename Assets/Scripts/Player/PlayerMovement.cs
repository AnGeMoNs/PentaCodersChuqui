using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    #region CLASS_VARIABLES

    [Header("Velocidades")]
    [SerializeField] private float walkSpeed = 2.7f;
    [SerializeField] private float runSpeed = 5.0f;
    [SerializeField] private float crouchSpeed = 1.4f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Estamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrain = 24f;
    [SerializeField] private float staminaRegen = 14f;
    [SerializeField] private float minStaminaToRun = 25f;

    [Header("Camara y Mouse")]
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private float mouseSensitivity = 0.12f;
    [SerializeField] private float maxLookAngle = 80f;

    [Header("Agacharse")]
    [SerializeField] private float standHeight = 1.8f;
    [SerializeField] private float crouchHeight = 1.0f;
    [SerializeField] private float standCameraY = 1.65f;
    [SerializeField] private float crouchCameraY = 0.9f;
    [SerializeField] private float crouchSpeedLerp = 8f;

    [Header("Sonidos")]
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private AudioClip tiredClip;
    [SerializeField] private float walkStepTime = 0.52f;
    [SerializeField] private float runStepTime = 0.32f;

    private CharacterController _controller;
    private float _verticalVelocity;
    private float _pitch;
    private float _stepTimer;
    private float _currentCamY;
    private float _currentStamina;
    private bool _isRunning;
    private bool _isCrouching;
    private bool _isExhausted;

    #endregion

    #region UNITY_METHODS

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _currentCamY = standCameraY;
        _currentStamina = maxStamina;
    }

    private void Start()
    {
        UIManager.Instance.UpdateStamina(1f, false);
    }

    private void Update()
    {
        RotateView();
        UpdateCrouch();
        MovePlayer();
        UpdateStamina();
    }

    #endregion

    #region CLASS_METHODS

    // Mueve la camara con el mouse y limita el angulo vertical
    private void RotateView()
    {
        if (Mouse.current == null || cameraHolder == null) return;

        Vector2 delta = Mouse.current.delta.ReadValue() * mouseSensitivity;
        transform.Rotate(0f, delta.x, 0f);

        _pitch = Mathf.Clamp(_pitch - delta.y, -maxLookAngle, maxLookAngle);
        cameraHolder.localEulerAngles = new Vector3(_pitch, 0f, 0f);
    }

    // Baja la altura del collider y de la camara cuando se mantiene Ctrl
    private void UpdateCrouch()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        bool crouchInput = kb.leftCtrlKey.isPressed || kb.rightCtrlKey.isPressed;

        // No levantarse si hay algo justo encima de la cabeza
        if (!crouchInput && _isCrouching)
        {
            Vector3 origin = transform.position + Vector3.up * crouchHeight;
            float dist = standHeight - crouchHeight;
            if (Physics.SphereCast(origin, _controller.radius * 0.85f, Vector3.up, out _, dist, ~0, QueryTriggerInteraction.Ignore))
            {
                crouchInput = true;
            }
        }

        _isCrouching = crouchInput;

        float targetH = _isCrouching ? crouchHeight : standHeight;
        if (Mathf.Abs(_controller.height - targetH) > 0.01f)
        {
            _controller.height = Mathf.Lerp(_controller.height, targetH, crouchSpeedLerp * Time.deltaTime);
            _controller.center = new Vector3(0f, _controller.height * 0.5f, 0f);
        }

        float targetCamY = _isCrouching ? crouchCameraY : standCameraY;
        _currentCamY = Mathf.Lerp(_currentCamY, targetCamY, crouchSpeedLerp * Time.deltaTime);
        if (cameraHolder != null)
        {
            cameraHolder.localPosition = new Vector3(0f, _currentCamY, 0f);
        }
    }

    // Lee WASD, calcula la velocidad segun estado y aplica gravedad
    private void MovePlayer()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        float x = 0f;
        float z = 0f;
        if (kb.dKey.isPressed) x += 1f;
        if (kb.aKey.isPressed) x -= 1f;
        if (kb.wKey.isPressed) z += 1f;
        if (kb.sKey.isPressed) z -= 1f;

        Vector3 dir = transform.right * x + transform.forward * z;
        if (dir.sqrMagnitude > 1f) dir.Normalize();

        // Solo corre hacia adelante y si no esta agachado ni cansado
        bool wantsRun = (kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed) && z > 0f && !_isCrouching;
        _isRunning = wantsRun && !_isExhausted && _currentStamina > 0f;

        float speed = walkSpeed;
        if (_isCrouching) speed = crouchSpeed;
        else if (_isRunning) speed = runSpeed;

        if (_controller.isGrounded && _verticalVelocity < 0f)
        {
            _verticalVelocity = -2f;
        }
        _verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = dir * speed;
        velocity.y = _verticalVelocity;
        _controller.Move(velocity * Time.deltaTime);

        PlayFootsteps(dir.sqrMagnitude > 0.05f && _controller.isGrounded);
    }

    // Gasta estamina al correr y la regenera al caminar o estar quieto
    private void UpdateStamina()
    {
        if (_isRunning)
        {
            _currentStamina -= staminaDrain * Time.deltaTime;
            if (_currentStamina <= 0f)
            {
                _currentStamina = 0f;
                _isExhausted = true;
                if (tiredClip != null) AudioManager.Instance.PlaySFX(tiredClip, 0.7f);
                UIManager.Instance.ShowMessage("Sin estamina. Espera para volver a correr.", 2f);
            }
        }
        else if (_currentStamina < maxStamina)
        {
            _currentStamina += staminaRegen * Time.deltaTime;
            if (_currentStamina >= minStaminaToRun)
            {
                _isExhausted = false;
            }
            if (_currentStamina > maxStamina)
            {
                _currentStamina = maxStamina;
            }
        }

        UIManager.Instance.UpdateStamina(_currentStamina / maxStamina, _isExhausted);
    }

    private void PlayFootsteps(bool moving)
    {
        if (!moving || footstepSource == null || footstepClips == null || footstepClips.Length == 0)
        {
            _stepTimer = 0f;
            return;
        }

        float interval = _isRunning ? runStepTime : (_isCrouching ? walkStepTime * 1.4f : walkStepTime);
        _stepTimer += Time.deltaTime;

        if (_stepTimer >= interval)
        {
            _stepTimer = 0f;
            AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
            footstepSource.pitch = Random.Range(0.9f, 1.08f);
            footstepSource.PlayOneShot(clip, _isCrouching ? 0.35f : (_isRunning ? 0.95f : 0.65f));
        }
    }

    #endregion

    #region GET_SET

    public bool IsRunning => _isRunning;
    public bool IsCrouching => _isCrouching;
    public float StaminaNormalized => _currentStamina / maxStamina;

    #endregion
}
