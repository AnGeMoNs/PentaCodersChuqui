using UnityEngine;

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

    public abstract void Interact(PlayerInteraction player);

    #endregion
}
