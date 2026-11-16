using UnityEngine;
using UnityEngine.InputSystem;

public class VideoCameraController : MonoBehaviour
{
    #region CLASS_VARIABLES

    [Header("Referencias")]
    [SerializeField] private NightVisionController nightVision;
    [SerializeField] private Transform cameraModel;
    [SerializeField] private Camera playerCamera;

    [Header("Posiciones")]
    [SerializeField] private Vector3 loweredPosition = new Vector3(0.28f, -0.32f, 0.42f);
    [SerializeField] private Vector3 raisedPosition = new Vector3(0.05f, -0.12f, 0.2f);
    [SerializeField] private float moveSpeed = 10f;

    [Header("Zoom")]
    [SerializeField] private float normalFieldOfView = 65f;
    [SerializeField] private float raisedFieldOfView = 55f;

    [Header("Sonidos")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip raiseClip;
    [SerializeField] private AudioClip lowerClip;

    private bool _isRaised;
    private float _recordingTime;

    #endregion

    #region UNITY_METHODS

    private void Start()
    {
        if (cameraModel != null) cameraModel.localPosition = loweredPosition;
        UIManager.Instance.ShowCameraHUD(false);
    }

    private void Update()
    {
        // Clic derecho levanta o baja la camara
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (_isRaised) LowerCamera();
            else RaiseCamera();
        }

        // F prende o apaga la vision nocturna solo si la camara esta arriba
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (_isRaised) nightVision.Toggle();
            else UIManager.Instance.ShowMessage("Levanta la camara (Clic derecho) para activar la vision nocturna.", 2.5f);
        }

        UpdateCameraTransition();

        if (_isRaised)
        {
            _recordingTime += Time.deltaTime;
            UIManager.Instance.UpdateRecordingTime(_recordingTime);
        }
    }

    #endregion

    #region CLASS_METHODS

    public void RaiseCamera()
    {
        _isRaised = true;
        if (audioSource != null && raiseClip != null) audioSource.PlayOneShot(raiseClip);
        UIManager.Instance.ShowCameraHUD(true);
    }

    public void LowerCamera()
    {
        _isRaised = false;
        if (audioSource != null && lowerClip != null) audioSource.PlayOneShot(lowerClip);

        // Al bajar la camara se apaga la vision nocturna para no gastar pila
        nightVision.TurnOff();
        UIManager.Instance.ShowCameraHUD(false);
    }

    // Mueve el modelo 3D suavemente y ajusta el FOV al apuntar con la camara
    private void UpdateCameraTransition()
    {
        if (cameraModel != null)
        {
            Vector3 target = _isRaised ? raisedPosition : loweredPosition;
            cameraModel.localPosition = Vector3.Lerp(cameraModel.localPosition, target, moveSpeed * Time.deltaTime);

            bool hideMesh = _isRaised && (cameraModel.localPosition - raisedPosition).sqrMagnitude < 0.0025f;
            if (cameraModel.gameObject.activeSelf == hideMesh)
            {
                cameraModel.gameObject.SetActive(!hideMesh);
            }
        }

        if (playerCamera != null)
        {
            float targetFov = _isRaised ? raisedFieldOfView : normalFieldOfView;
            if (Mathf.Abs(playerCamera.fieldOfView - targetFov) > 0.05f)
            {
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFov, moveSpeed * Time.deltaTime);
            }
        }
    }

    #endregion

    #region GET_SET

    public bool IsRaised => _isRaised;

    #endregion
}
