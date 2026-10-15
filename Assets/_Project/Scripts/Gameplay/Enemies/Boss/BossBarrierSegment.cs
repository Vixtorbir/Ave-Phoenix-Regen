using System;
using UnityEngine;
using PhoenixReborn.Core;

namespace PhoenixReborn.Gameplay.Enemies.Boss
{
    /// <summary>
    /// Individual destructible segment of the Phoenix Mother Ship barrier.
    /// Implements IDamageable. When destroyed, opens up gaps in the shield
    /// for player bullets to reach the core.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class BossBarrierSegment : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHealth = 5;
        [SerializeField] private int scoreReward = 150;

        private int _currentHealth;
        private SpriteRenderer _spriteRenderer;
        private Color _originalColor;

        public event Action<BossBarrierSegment> OnSegmentDestroyed;
        public bool IsDestroyed { get; private set; }

        private Systems.HitFlash _hitFlash;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer != null)
            {
                _originalColor = _spriteRenderer.color;
            }
            _hitFlash = GetComponent<Systems.HitFlash>();
            if (_hitFlash == null) _hitFlash = GetComponentInChildren<Systems.HitFlash>();
            if (_hitFlash == null) _hitFlash = gameObject.AddComponent<Systems.HitFlash>();

            _currentHealth = maxHealth;
            IsDestroyed = false;
        }

        public void TakeDamage(int damage)
        {
            if (IsDestroyed) return;

            _currentHealth -= damage;

            if (_hitFlash != null)
            {
                _hitFlash.SetFlashColor(new Color(1f, 0.2f, 0.2f, 1f));
                _hitFlash.Flash();
            }

            Systems.AudioManager.Instance?.PlayBarrierHit();

            if (_currentHealth <= 0)
            {
                DestroySegment();
            }
        }

        private void DestroySegment()
        {
            IsDestroyed = true;
            Systems.ScoreManager.Instance?.AddScore(scoreReward);
            Systems.VFXManager.Instance?.PlayExplosion(transform.position);
            Systems.FloatingScoreManager.Instance?.SpawnScore(transform.position, scoreReward, new Color(1f, 0.8f, 0.2f));
            Systems.AudioManager.Instance?.PlaySmallExplosion();
            OnSegmentDestroyed?.Invoke(this);
            gameObject.SetActive(false);
        }
    }
}
