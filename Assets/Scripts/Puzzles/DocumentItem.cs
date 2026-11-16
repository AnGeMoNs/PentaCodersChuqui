using UnityEngine;

public class DocumentItem : Interactable
{
    #region CLASS_VARIABLES

    [SerializeField] private string documentTitle = "Nota";
    [TextArea(4, 10)]
    [SerializeField] private string documentBody = "";
    [SerializeField] private bool isFinalExpediente = false;
    [SerializeField] private AudioClip readClip;

    private bool _endingActivated;

    #endregion

    #region CLASS_METHODS

    public override void Interact(PlayerInteraction player)
    {
        AudioManager.Instance.PlaySFX(readClip);
        UIManager.Instance.OpenDocumentPanel(documentTitle, documentBody, OnClosed);
        GameManager.Instance.SetGameplayInputEnabled(false);
    }

    private void OnClosed()
    {
        GameManager.Instance.SetGameplayInputEnabled(true);

        if (isFinalExpediente && !_endingActivated)
        {
            _endingActivated = true;
            GameManager.Instance.TriggerEndingSequence();
        }
    }

    #endregion
}
