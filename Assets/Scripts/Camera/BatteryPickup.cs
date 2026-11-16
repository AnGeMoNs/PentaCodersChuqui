using UnityEngine;

public class BatteryPickup : Interactable
{
    #region CLASS_VARIABLES

    [SerializeField] private float chargeAmount = 40f;
    [SerializeField] private AudioClip pickupClip;

    #endregion

    #region CLASS_METHODS

    public override void Interact(PlayerInteraction player)
    {
        CameraBattery battery = player.GetComponent<CameraBattery>();
        if (battery == null) return;

        if (!battery.Recharge(chargeAmount))
        {
            UIManager.Instance.ShowMessage("La batería ya está al máximo.", 2f);
            return;
        }

        AudioManager.Instance.PlaySFX(pickupClip);
        UIManager.Instance.ShowMessage("Batería recargada +" + chargeAmount + "%", 2.5f);
        gameObject.SetActive(false);
    }

    #endregion
}
