using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class VictoryTrigger : MonoBehaviour
{
    #region CLASS_VARIABLES

    [SerializeField] private bool triggerEndingScreen = false;
    private bool _triggered;

    #endregion

    #region UNITY_METHODS

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered || !other.CompareTag("Player")) return;

        _triggered = true;
        UIManager.Instance.ShowMessage("Victoria, espera a que continuemos la historia...", 5f);

        if (triggerEndingScreen)
        {
            GameManager.Instance.TriggerEndingSequence();
        }
    }

    #endregion
}
