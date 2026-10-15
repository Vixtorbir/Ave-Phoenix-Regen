using UnityEngine;
using UnityEngine.Pool;
using PhoenixReborn.Gameplay.Weapons;

namespace PhoenixReborn.Gameplay.Enemies
{
    /// <summary>
    /// Wave 2 Enemy: Medium Phoenix Bird. Tougher enemy with 2 HP,
    /// horizontal patrolling & swooping attacks, and dual laser firing.
    /// </summary>
    public class MediumPhoenixEnemy : Enemy
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 3.6f;
        [SerializeField] private float verticalSpeed = 1.8f;
        [SerializeField] private float horizontalSwoopRange = 3.5f;

        [Header("Shooting Settings")]
        [SerializeField] private EnemyProjectile projectilePrefab;
        [SerializeField] private Transform leftFirePoint;
        [SerializeField] private Transform rightFirePoint;
        [SerializeField] private float fireRateMin = 1.8f;
        [SerializeField] private float fireRateMax = 3.2f;

        private int _moveDirection = 1;
        private float _nextFireTimer;
        private bool _fireLeftWing = true;
        private ObjectPool<EnemyProjectile> _projectilePool;
        private SpriteRenderer _spriteRenderer;
        private Color _originalColor;
        private Vector3 _baseScale;
        private float _flapTimer;

        protected override Color ScorePopupColor => new Color(1f, 0.85f, 0.2f);

        protected override void Awake()
        {
            if (maxHealth <= 1) maxHealth = 2;
            if (scoreValue <= 100) scoreValue = 250;
            base.Awake();
            _baseScale = transform.localScale;

            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer != null)
            {
                _originalColor = _spriteRenderer.color;
            }
        }

        protected override void OnSpawn()
        {
            _moveDirection = Random.value > 0.5f ? 1 : -1;
            _flapTimer = Random.Range(0f, 6.28f);
            if (_spriteRenderer != null) _spriteRenderer.color = _originalColor;
            transform.localRotation = Quaternion.identity;
            _nextFireTimer = Random.Range(1.0f, 2.0f);
        }

        private void Update()
        {
            float maxRange = GetMaxHorizontalRange();
            float newX = transform.position.x + (_moveDirection * moveSpeed * Time.deltaTime);

            if (newX > maxRange)
            {
                newX = maxRange;
                _moveDirection = -1;
            }
            else if (newX < -maxRange)
            {
                newX = -maxRange;
                _moveDirection = 1;
            }

            float newY = transform.position.y - (verticalSpeed * Time.deltaTime);
            transform.position = new Vector3(newX, newY, transform.position.z);

            _flapTimer += Time.deltaTime * 5f;
            float wingFlap = 1f + (Mathf.Sin(_flapTimer) * 0.09f);
            transform.localScale = new Vector3(_baseScale.x * wingFlap, _baseScale.y, _baseScale.z);

            float targetTilt = -_moveDirection * 12f;
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0f, 0f, targetTilt), Time.deltaTime * 8f);

            _nextFireTimer -= Time.deltaTime;
            if (_nextFireTimer <= 0f)
            {
                TryShoot();
                ResetFireTimer();
            }

            if (transform.position.y < -7f)
            {
                float randomX = Random.Range(-maxRange * 0.8f, maxRange * 0.8f);
                transform.position = new Vector3(randomX, 6.5f, transform.position.z);
            }
        }

        private float GetMaxHorizontalRange()
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                float halfWidth = cam.orthographicSize * cam.aspect;
                return Mathf.Min(horizontalSwoopRange, Mathf.Max(1.5f, halfWidth - 0.4f));
            }
            return horizontalSwoopRange;
        }

        protected override void OnTakeDamage(int damage)
        {
        }

        private void TryShoot()
        {
            if (projectilePrefab == null) return;

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

            Transform selectedPoint = (_fireLeftWing && leftFirePoint != null) ? leftFirePoint : (rightFirePoint != null ? rightFirePoint : firePointFallback);
            Vector3 spawnPos = selectedPoint != null ? selectedPoint.position : transform.position;
            FireSingleLaser(spawnPos);
            _fireLeftWing = !_fireLeftWing;
        }

        private Transform firePointFallback => transform;

        private void FireSingleLaser(Vector3 position)
        {
            EnemyProjectile proj = _projectilePool.Get();
            if (proj != null)
            {
                proj.transform.position = position;
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
