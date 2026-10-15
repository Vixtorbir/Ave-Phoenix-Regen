using System;
using UnityEngine;
using UnityEngine.Pool;
using PhoenixReborn.Core;

namespace PhoenixReborn.Gameplay.Enemies
{
    /// <summary>
    /// Abstract base class for all enemies. Implements IDamageable,
    /// emits lifecycle events for WaveManager and ScoreSystem, and
    /// manages recycling back into its ObjectPool.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class Enemy : MonoBehaviour, IDamageable
    {
        [Header("Enemy Attributes")]
        [SerializeField] protected int maxHealth = 1;
        [SerializeField] protected int scoreValue = 100;
        [SerializeField] private float spawnInvulnerabilityDuration = 0.6f;

        private IObjectPool<Enemy> _pool;
        private int _currentHealth;
        private bool _isReturned;
        private float _spawnTime;

        public event Action<Enemy> OnEnemyDied;

        public int ScoreValue => scoreValue;
        public int CurrentHealth => _currentHealth;
        public bool IsInvulnerable => (Time.time - _spawnTime) < spawnInvulnerabilityDuration;
        protected virtual Color ScorePopupColor => new Color(0.2f, 0.9f, 1f);

        private Systems.HitFlash _hitFlash;

        protected virtual void Awake()
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }

            _hitFlash = GetComponent<Systems.HitFlash>();
            if (_hitFlash == null) _hitFlash = GetComponentInChildren<Systems.HitFlash>();
        }

        public virtual void Init(IObjectPool<Enemy> pool)
        {
            _pool = pool;
            _currentHealth = maxHealth;
            _isReturned = false;
            _spawnTime = Time.time;

            if (_hitFlash != null) _hitFlash.ResetColor();

            OnSpawn();
        }

        protected virtual void OnSpawn() { }

        public void TakeDamage(int damage)
        {
            if (_isReturned || _currentHealth <= 0 || IsInvulnerable) return;

            _currentHealth -= damage;
            
            if (_hitFlash != null) _hitFlash.Flash();

            OnTakeDamage(damage);

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        protected virtual void OnTakeDamage(int damage) { }

        protected virtual void Die()
        {
            if (_isReturned) return;
            _isReturned = true;

            Systems.CameraShake.Instance?.Shake(0.12f, 0.15f);
            Systems.ScoreManager.Instance?.AddScore(scoreValue);
            Systems.VFXManager.Instance?.PlayExplosion(transform.position);
            Systems.FloatingScoreManager.Instance?.SpawnScore(transform.position, scoreValue, ScorePopupColor);

            if (maxHealth <= 1)
                Systems.AudioManager.Instance?.PlaySmallExplosion();
            else
                Systems.AudioManager.Instance?.PlayMediumExplosion();

            OnEnemyDied?.Invoke(this);

            Invoke(nameof(DeactivatePooledEnemy), 0.08f);
        }

        private void DeactivatePooledEnemy()
        {
            if (_pool != null)
            {
                _pool.Release(this);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public void RecycleWithoutScore()
        {
            if (_isReturned) return;
            _isReturned = true;

            OnEnemyDied?.Invoke(this);

            if (_pool != null)
            {
                _pool.Release(this);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
