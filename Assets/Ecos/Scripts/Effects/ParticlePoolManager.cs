using UnityEngine;

namespace Ecos
{
    /// <summary>
    /// Manages pooled ParticleSystem bursts using SimpleObjectPool + PoolableObject
    /// to avoid runtime Instantiate/Destroy overhead.
    /// </summary>
    public class ParticlePoolManager : GenericSingleton<ParticlePoolManager>
    {
        private SimpleObjectPool _pool;

        protected override void Initialization()
        {
            base.Initialization();
            EnsurePool();
        }

        private void OnEnable()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnParticleBurstRequested += SpawnBurst;
            }
        }

        private void OnDisable()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnParticleBurstRequested -= SpawnBurst;
            }
        }

        private void EnsurePool()
        {
            if (_pool != null) return;

            GameObject template = CreateBurstTemplate();
            _pool = gameObject.AddComponent<SimpleObjectPool>();
            _pool.gameObjectToPool = template;
            _pool.poolSize = 8;
            _pool.poolCanExpand = true;
            _pool.Initialization();
            template.SetActive(false);
        }

        private GameObject CreateBurstTemplate()
        {
            GameObject go = new GameObject("PooledParticleBurst");
            go.transform.SetParent(transform, false);
            go.SetActive(false);

            PoolableObject poolable = go.AddComponent<PoolableObject>();
            poolable.lifeTime = 1.6f;

            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.6f;
            main.loop = false;
            main.startLifetime = 1.1f;
            main.startSpeed = 2.4f;
            main.startSize = 0.08f;
            main.maxParticles = 32;
            main.playOnAwake = false;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 18) });

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.25f;

            return go;
        }

        public void SpawnBurst(Vector3 position, Color color)
        {
            EnsurePool();
            GameObject pooled = _pool.GetPooledGameObject();
            if (pooled == null) return;

            pooled.transform.position = position;
            ParticleSystem ps = pooled.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.startColor = color;
            }

            pooled.SetActive(true);
            if (ps != null) ps.Play();
        }
    }
}
