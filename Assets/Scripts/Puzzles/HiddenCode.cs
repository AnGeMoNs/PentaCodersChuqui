using UnityEngine;

public class HiddenCode : MonoBehaviour
{
    #region CLASS_VARIABLES

    [SerializeField] private GameObject[] visualElements;
    private Renderer[] _cachedRenderers;

    #endregion

    #region UNITY_METHODS

    private void Awake()
    {
        _cachedRenderers = GetComponentsInChildren<Renderer>(true);
        SetRevealed(false);
    }

    #endregion

    #region CLASS_METHODS

    public void SetRevealed(bool revealed)
    {
        if (visualElements != null && visualElements.Length > 0)
        {
            for (int i = 0; i < visualElements.Length; i++)
            {
                if (visualElements[i] != null) visualElements[i].SetActive(revealed);
            }
            return;
        }

        if (_cachedRenderers == null) return;
        for (int i = 0; i < _cachedRenderers.Length; i++)
        {
            if (_cachedRenderers[i] != null) _cachedRenderers[i].enabled = revealed;
        }
    }

    #endregion
}
