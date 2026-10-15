using UnityEngine;

namespace PhoenixReborn.Systems
{
    /// <summary>
    /// Particle effects for explosions, engine trails and hit feedback,
    /// using pooled ParticleSystems.
    /// </summary>
    public class VFXManager : MonoBehaviour
    {
        public static VFXManager Instance { get; private set; }

        [Header("Explosion Particle Prefab")]
        [SerializeField] private ParticleSystem explosionPrefab;
        [SerializeField] private ParticleSystem bossExplosionPrefab;

        private ParticleSystem[] _explosionPool;
        private int _poolIndex = 0;
        private const int PoolSize = 15;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            InitializePool();
        }

        private void InitializePool()
        {
            if (explosionPrefab == null) return;

            _explosionPool = new ParticleSystem[PoolSize];
            for (int i = 0; i < PoolSize; i++)
            {
                ParticleSystem ps = Instantiate(explosionPrefab, transform);
                ps.gameObject.SetActive(false);
                _explosionPool[i] = ps;
            }
        }

        public void PlayExplosion(Vector3 position)
        {
            if (_explosionPool != null && _explosionPool.Length > 0)
            {
                ParticleSystem ps = _explosionPool[_poolIndex];
                if (ps != null)
                {
                    ps.transform.position = position;
                    ps.gameObject.SetActive(true);
                    ps.Play();
                }

                _poolIndex = (_poolIndex + 1) % _explosionPool.Length;
            }
        }

        public void PlayBossExplosion(Vector3 position)
        {
            if (bossExplosionPrefab != null)
            {
                ParticleSystem bossPs = Instantiate(bossExplosionPrefab, position, Quaternion.identity);
                Destroy(bossPs.gameObject, 3f);
            }
            else
            {
                PlayExplosion(position);
            }
        }
    }
}
