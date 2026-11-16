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
        // Evita que la misma tecla E que cerro un panel vuelva a abrirlo en ese frame
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

    // Tira un rayo desde el centro de la camara para detectar objetos interactuables
    private void CheckRaycast()
    {
        _currentTarget = null;

        if (playerCamera != null &&
            Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit hit, interactDistance, interactionLayers, QueryTriggerInteraction.Ignore))
        {
            _currentTarget = hit.collider.GetComponentInParent<Interactable>();
        }

        // Actualiza el texto en pantalla solo si cambio el objeto apuntado
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
