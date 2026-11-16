using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    #region CLASS_VARIABLES

    [SerializeField] private Transform playerCamera;
    [SerializeField] private float interactDistance = 2.6f;
    [SerializeField] private LayerMask interactionLayers = ~0;

    private Interactable _currentTarget;
    private Interactable _lastShownTarget;
    private int _enabledFrame;

    #endregion

    #region UNITY_METHODS

    private void OnEnable()
    {
        _enabledFrame = Time.frameCount;
        _lastShownTarget = null;
    }

    private void Update()
    {
        CheckRaycast();

        if (_currentTarget == null || Time.frameCount == _enabledFrame) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            _currentTarget.Interact(this);
            _lastShownTarget = null;
        }
    }

    #endregion

    #region CLASS_METHODS

    private void CheckRaycast()
    {
        _currentTarget = null;

        if (playerCamera != null &&
            Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit hit, interactDistance, interactionLayers, QueryTriggerInteraction.Ignore))
        {
            _currentTarget = hit.collider.GetComponentInParent<Interactable>();
        }

        // Solo actualizamos el texto del Canvas si cambió el objeto apuntado
        if (_currentTarget != _lastShownTarget)
        {
            _lastShownTarget = _currentTarget;
            if (_currentTarget != null)
            {
                UIManager.Instance.ShowInteractionText("[E] " + _currentTarget.GetInteractionText());
            }
            else
            {
                UIManager.Instance.HideInteractionText();
            }
        }
    }

    #endregion
}
