using UnityEngine;

// Clase base para todo lo que el jugador puede usar con E
public abstract class Interactable : MonoBehaviour
{
    #region CLASS_VARIABLES

    [SerializeField] protected string interactionText = "Interactuar";

    #endregion

    #region CLASS_METHODS

    public virtual string GetInteractionText()
    {
        return interactionText;
    }

    // Cada objeto implementa su propia accion al presionar E
    public abstract void Interact(PlayerInteraction player);

    #endregion
}
