using System;
using UnityEngine;
using PhoenixReborn.Core;

namespace PhoenixReborn.Gameplay.Enemies.Boss
{
    /// <summary>
    /// The vulnerable core inside the Phoenix Mother Ship.
    /// Destroying the core defeats the Boss and completes the stage.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class BossCore : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHealth = 10;
        [SerializeField] private int scoreReward = 5000;

        private int _currentHealth;

        public event Action OnCoreDestroyed;
        public bool IsDestroyed { get; private set; }

        private Systems.HitFlash _hitFlash;

        private void Awake()
        {
            _currentHealth = maxHealth;
            IsDestroyed = false;

            _hitFlash = GetComponent<Systems.HitFlash>();
            if (_hitFlash == null) _hitFlash = GetComponentInChildren<Systems.HitFlash>();
            if (_hitFlash == null) _hitFlash = gameObject.AddComponent<Systems.HitFlash>();
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

            Systems.CameraShake.Instance?.Shake(0.25f, 0.25f);
            Systems.AudioManager.Instance?.PlayBossHit();
            Debug.Log($"[BossCore] Core hit! Remaining health: {_currentHealth}");

            if (_currentHealth <= 0)
            {
                IsDestroyed = true;
                Systems.CameraShake.Instance?.Shake(0.9f, 0.65f);
                Systems.ScoreManager.Instance?.AddScore(scoreReward);
                Systems.VFXManager.Instance?.PlayBossExplosion(transform.position);
                Systems.FloatingScoreManager.Instance?.SpawnScore(transform.position, scoreReward, new Color(1f, 0.3f, 0.3f));
                Systems.AudioManager.Instance?.PlayBossExplosion();
                OnCoreDestroyed?.Invoke();
            }
        }
    }
}
