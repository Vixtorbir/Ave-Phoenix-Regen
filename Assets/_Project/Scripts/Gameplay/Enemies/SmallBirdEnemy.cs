using UnityEngine;
using UnityEngine.Pool;
using PhoenixReborn.Gameplay.Weapons;

namespace PhoenixReborn.Gameplay.Enemies
{
    /// <summary>
    /// Classic Phoenix small bird enemy. Swoops down in a sine-wave pattern
    /// and fires occasional projectiles at the player.
    /// </summary>
    public class SmallBirdEnemy : Enemy
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 2.2f;
        [SerializeField] private float sineAmplitude = 1.8f;
        [SerializeField] private float sineFrequency = 2.4f;

        [Header("Shooting Settings")]
        [SerializeField] private EnemyProjectile projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float fireRateMin = 0.5f;
        [SerializeField] private float fireRateMax = 1.2f;

        private float _startX;
        private float _timeCounter;
        private float _nextFireTimer;
        private ObjectPool<EnemyProjectile> _projectilePool;
        private Vector3 _baseScale;

        protected override void Awake()
        {
            base.Awake();
            _baseScale = transform.localScale;

            if (projectilePrefab != null)
            {
                _projectilePool = new ObjectPool<EnemyProjectile>(
                    createFunc: () => Instantiate(projectilePrefab),
                    actionOnGet: proj => proj.gameObject.SetActive(true),
                    actionOnRelease: proj => proj.gameObject.SetActive(false),
                    actionOnDestroy: proj => Destroy(proj.gameObject),
                    defaultCapacity: 10,
                    maxSize: 30);
            }
        }

        protected override void OnSpawn()
        {
            _startX = transform.position.x;
            _timeCounter = Random.Range(0f, 6.28f);
            _nextFireTimer = Random.Range(0.4f, 0.9f);
            transform.localRotation = Quaternion.identity;
        }

        private void Update()
        {
            _timeCounter += Time.deltaTime * sineFrequency;
            _nextFireTimer -= Time.deltaTime;

            float amp = GetEffectiveSineAmplitude();
            float xOffset = Mathf.Sin(_timeCounter) * amp;
            float newX = _startX + xOffset;

            Camera cam = Camera.main;
            if (cam != null)
            {
                float halfWidth = cam.orthographicSize * cam.aspect;
                newX = Mathf.Clamp(newX, -halfWidth + 0.35f, halfWidth - 0.35f);
            }

            float newY = transform.position.y - (moveSpeed * Time.deltaTime);
            transform.position = new Vector3(newX, newY, transform.position.z);

            float velocityX = Mathf.Cos(_timeCounter) * sineFrequency * amp;
            float tiltAngle = -velocityX * 3.5f;
            transform.localRotation = Quaternion.Euler(0f, 0f, tiltAngle);

            float wingFlap = 1f + (Mathf.Sin(_timeCounter * 3.5f) * 0.08f);
            transform.localScale = new Vector3(_baseScale.x * wingFlap, _baseScale.y, _baseScale.z);

            if (_nextFireTimer <= 0f)
            {
                TryShoot();
                ResetFireTimer();
            }

            if (transform.position.y < -7f)
            {
                float maxSpawnX = amp * 0.6f;
                float randomX = Random.Range(-maxSpawnX, maxSpawnX);
                _startX = randomX;
                transform.position = new Vector3(randomX, 6.5f, transform.position.z);
            }
        }

        private float GetEffectiveSineAmplitude()
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                float halfWidth = cam.orthographicSize * cam.aspect;
                return Mathf.Min(sineAmplitude, Mathf.Max(0.6f, halfWidth * 0.45f));
            }
            return sineAmplitude;
        }

        private void TryShoot()
        {
            if (projectilePrefab == null)
            {
                Debug.LogWarning("[SmallBirdEnemy] Cannot shoot: ProjectilePrefab is missing in the Inspector of SmallBirdEnemy!");
                return;
            }

            if (_projectilePool == null)
            {
                _projectilePool = new ObjectPool<EnemyProjectile>(
                    createFunc: () => Instantiate(projectilePrefab),
                    actionOnGet: proj => proj.gameObject.SetActive(true),
                    actionOnRelease: proj => proj.gameObject.SetActive(false),
                    actionOnDestroy: proj => Destroy(proj.gameObject),
                    defaultCapacity: 10,
                    maxSize: 30);
            }

            EnemyProjectile proj = _projectilePool.Get();
            if (proj != null)
            {
                Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
                proj.transform.position = spawnPos;
                proj.Init(_projectilePool);
                Systems.AudioManager.Instance?.PlayEnemyShoot();
            }
        }

        private void ResetFireTimer()
        {
            _nextFireTimer = Random.Range(fireRateMin, fireRateMax);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player == null) player = other.GetComponentInParent<PlayerHealth>();

            if (player != null)
            {
                player.TakeDamage();
                TakeDamage(CurrentHealth);
            }
        }
    }
}
