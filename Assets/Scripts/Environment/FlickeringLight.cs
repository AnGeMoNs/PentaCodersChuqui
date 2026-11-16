using UnityEngine;

[RequireComponent(typeof(Light))]
public class FlickeringLight : MonoBehaviour
{
    #region CLASS_VARIABLES

    [SerializeField] private float minIntensity = 0.08f;
    [SerializeField] private float maxIntensity = 1.6f;
    [SerializeField] private float flickerSpeed = 8f;
    [SerializeField] private Renderer lampBulbRenderer;
    [SerializeField] private SimpleObjectPool sparkPool;
    [SerializeField] private float sparkInterval = 6f;

    private Light _light;
    private float _noiseSeed;
    private float _intenseTimer;
    private float _sparkTimer;
    private bool _forcedOff;

    #endregion

    #region UNITY_METHODS

    private void Awake()
    {
        _light = GetComponent<Light>();
        _noiseSeed = Random.Range(0f, 100f);
        _sparkTimer = Random.Range(1.5f, sparkInterval);
    }

    private void Update()
    {
        if (_forcedOff)
        {
            if (_light.intensity > 0f) _light.intensity = 0f;
            if (lampBulbRenderer != null && lampBulbRenderer.enabled) lampBulbRenderer.enabled = false;
            return;
        }

        float speed = _intenseTimer > 0f ? flickerSpeed * 3.5f : flickerSpeed;
        if (_intenseTimer > 0f) _intenseTimer -= Time.deltaTime;

        float noise = Mathf.PerlinNoise(_noiseSeed, Time.time * speed);
        _light.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);

        if (lampBulbRenderer != null)
        {
            bool bulbOn = _light.intensity > (minIntensity + 0.15f);
            if (lampBulbRenderer.enabled != bulbOn) lampBulbRenderer.enabled = bulbOn;
        }

        if (sparkPool != null)
        {
            _sparkTimer -= Time.deltaTime;
            if (_sparkTimer <= 0f)
            {
                _sparkTimer = _intenseTimer > 0f ? 0.6f : Random.Range(sparkInterval * 0.7f, sparkInterval * 1.4f);
                SpawnSparkFromPool();
            }
        }
    }

    #endregion

    #region CLASS_METHODS

    public void SpawnSparkFromPool()
    {
        if (sparkPool == null) return;

        GameObject spark = sparkPool.GetPooledGameObject();
        if (spark != null)
        {
            spark.transform.position = transform.position + Vector3.down * 0.1f;
            spark.SetActive(true);
            ParticleSystem ps = spark.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
        }
    }

    public void TriggerIntenseFlicker(float duration)
    {
        _intenseTimer = duration;
        SpawnSparkFromPool();
    }

    public void SetForcedOff(bool off)
    {
        _forcedOff = off;
    }

    #endregion
}
