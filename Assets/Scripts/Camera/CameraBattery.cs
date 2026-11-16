using UnityEngine;

public class CameraBattery : MonoBehaviour
{
    #region CLASS_VARIABLES

    [SerializeField] private float maxCharge = 100f;
    [SerializeField] private float startCharge = 100f;
    [SerializeField] private float drainPerSecond = 5.5f;
    [SerializeField] private float lowBatteryThreshold = 25f;

    private float _currentCharge;
    private bool _isDraining;
    private int _lastDisplayedPercent = -1;

    #endregion

    #region UNITY_METHODS

    private void Awake()
    {
        _currentCharge = Mathf.Clamp(startCharge, 0f, maxCharge);
    }

    private void Start()
    {
        RefreshUI(true);
    }

    private void Update()
    {
        // Solo consume energia cuando la vision nocturna esta prendida
        if (!_isDraining || _currentCharge <= 0f) return;

        _currentCharge -= drainPerSecond * Time.deltaTime;
        if (_currentCharge < 0f) _currentCharge = 0f;

        RefreshUI(false);
    }

    #endregion

    #region CLASS_METHODS

    public void SetDraining(bool draining)
    {
        _isDraining = draining;
    }

    // Recarga la bateria al recoger una pila, devuelve false si ya estaba al 100%
    public bool Recharge(float amount)
    {
        if (_currentCharge >= maxCharge) return false;

        _currentCharge = Mathf.Min(_currentCharge + amount, maxCharge);
        RefreshUI(true);
        return true;
    }

    // Actualiza el HUD solo cuando cambia el porcentaje entero
    private void RefreshUI(bool force)
    {
        int rounded = Mathf.RoundToInt(ChargePercent);
        if (!force && rounded == _lastDisplayedPercent) return;

        _lastDisplayedPercent = rounded;
        UIManager.Instance.UpdateBattery(ChargePercent, ChargePercent <= lowBatteryThreshold);
    }

    #endregion

    #region GET_SET

    public bool HasCharge => _currentCharge > 0f;
    public float ChargePercent => (_currentCharge / maxCharge) * 100f;

    #endregion
}
