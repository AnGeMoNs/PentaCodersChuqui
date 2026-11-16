using UnityEngine;
using UnityEngine.Rendering;

public class NightVisionController : MonoBehaviour
{
    #region CLASS_VARIABLES

    [Header("Referencias")]
    [SerializeField] private CameraBattery battery;
    [SerializeField] private Volume nightVisionVolume;
    [SerializeField] private Light infraredLight;
    [SerializeField] private HiddenCode[] hiddenCodes;

    [Header("Niebla")]
    [SerializeField] private float normalFogDensity = 0.08f;
    [SerializeField] private float nightVisionFogDensity = 0.025f;

    [Header("Sonidos")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip turnOnClip;
    [SerializeField] private AudioClip turnOffClip;
    [SerializeField] private AudioClip noBatteryClip;

    private bool _isActive;

    #endregion

    #region UNITY_METHODS

    private void Start()
    {
        ApplyNightVision(false);
    }

    private void Update()
    {
        if (_isActive && !battery.HasCharge)
        {
            TurnOff();
            UIManager.Instance.ShowMessage("Batería agotada. Busca pilas en el hospital.", 3f);
        }
    }

    #endregion

    #region CLASS_METHODS

    public void Toggle()
    {
        if (_isActive) TurnOff();
        else TurnOn();
    }

    public void TurnOn()
    {
        if (!battery.HasCharge)
        {
            if (audioSource != null && noBatteryClip != null) audioSource.PlayOneShot(noBatteryClip);
            UIManager.Instance.ShowMessage("Sin batería en la cámara.", 2.5f);
            return;
        }

        if (audioSource != null && turnOnClip != null) audioSource.PlayOneShot(turnOnClip);
        ApplyNightVision(true);
    }

    public void TurnOff()
    {
        if (!_isActive) return;

        if (audioSource != null && turnOffClip != null) audioSource.PlayOneShot(turnOffClip);
        ApplyNightVision(false);
    }

    private void ApplyNightVision(bool active)
    {
        _isActive = active;

        if (nightVisionVolume != null) nightVisionVolume.weight = active ? 1f : 0f;
        if (infraredLight != null) infraredLight.enabled = active;

        RenderSettings.fogDensity = active ? nightVisionFogDensity : normalFogDensity;

        if (battery != null) battery.SetDraining(active);
        UIManager.Instance.ShowNightVisionOverlay(active);

        if (hiddenCodes == null) return;
        for (int i = 0; i < hiddenCodes.Length; i++)
        {
            if (hiddenCodes[i] != null) hiddenCodes[i].SetRevealed(active);
        }
    }

    #endregion

    #region GET_SET

    public bool IsActive => _isActive;

    #endregion
}
