using UnityEngine;

namespace PhoenixReborn.Gameplay
{
    /// <summary>
    /// Player lives. TakeDamage is ignored while the shield is active.
    /// </summary>
    [RequireComponent(typeof(PlayerShield))]
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private int maxLives = 3;
        [SerializeField] private float invulnerabilityDuration = 2.2f;

        private PlayerShield _shield;
        private bool _isDead;
        private bool _isInvulnerable;
        private SpriteRenderer[] _spriteRenderers;

        public int CurrentLives { get; private set; }
        public int MaxLives => maxLives;
        public bool IsInvulnerable => _isInvulnerable;
        public event System.Action<int> OnLivesChanged;

        private void Awake()
        {
            _shield = GetComponent<PlayerShield>();
            _spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            CurrentLives = maxLives;
            OnLivesChanged?.Invoke(CurrentLives);
        }

        public void TakeDamage()
        {
            if (_isDead || _isInvulnerable) return;
            if (_shield != null && _shield.IsActive) return;

            CurrentLives--;
            OnLivesChanged?.Invoke(CurrentLives);

            Systems.HitFlash hitFlash = GetComponent<Systems.HitFlash>();
            if (hitFlash == null) hitFlash = GetComponentInChildren<Systems.HitFlash>();
            if (hitFlash != null) hitFlash.Flash();

            Systems.CameraShake.Instance?.Shake(0.35f, 0.4f);
            Systems.AudioManager.Instance?.PlayPlayerHit();

            Debug.Log($"[PlayerHealth] Hit! Lives remaining: {CurrentLives}");

            if (CurrentLives <= 0)
            {
                _isDead = true;
                TriggerPlayerDeath();
            }
            else
            {
                StartCoroutine(InvulnerabilityRoutine());
            }
        }

        private System.Collections.IEnumerator InvulnerabilityRoutine()
        {
            _isInvulnerable = true;
            float elapsed = 0f;
            float blinkInterval = 0.12f;

            while (elapsed < invulnerabilityDuration)
            {
                elapsed += blinkInterval;
                SetSpritesAlpha(0.25f);
                yield return new WaitForSeconds(blinkInterval * 0.5f);
                SetSpritesAlpha(1.0f);
                yield return new WaitForSeconds(blinkInterval * 0.5f);
            }

            SetSpritesAlpha(1.0f);
            _isInvulnerable = false;
        }

        private void SetSpritesAlpha(float alpha)
        {
            if (_spriteRenderers == null) return;
            foreach (var sr in _spriteRenderers)
            {
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = alpha;
                    sr.color = c;
                }
            }
        }

        /// <summary>
        /// Restores player life up to maxLives. Used by the Arcade Mode life recovery.
        /// </summary>
        public bool RestoreLife(int amount = 1)
        {
            if (_isDead || CurrentLives >= maxLives) return false;

            CurrentLives = Mathf.Min(maxLives, CurrentLives + amount);
            OnLivesChanged?.Invoke(CurrentLives);
            Debug.Log($"[PlayerHealth] Life Restored! Lives remaining: {CurrentLives}");
            return true;
        }

        private void TriggerPlayerDeath()
        {
            Systems.CameraShake.Instance?.Shake(0.8f, 0.6f);
            Systems.VFXManager.Instance?.PlayBossExplosion(transform.position);
            Systems.VFXManager.Instance?.PlayExplosion(transform.position + new Vector3(0.25f, 0.25f, 0f));
            Systems.VFXManager.Instance?.PlayExplosion(transform.position - new Vector3(0.25f, 0.25f, 0f));
            Systems.AudioManager.Instance?.PlayBossExplosion();

            PlayerMovement move = GetComponent<PlayerMovement>();
            if (move != null) move.enabled = false;

            PlayerShooting shoot = GetComponent<PlayerShooting>();
            if (shoot != null) shoot.enabled = false;

            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
            foreach (var r in renderers)
            {
                if (r != null) r.enabled = false;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_shield != null && _shield.IsActive) return;

            Enemies.Enemy enemy = other.GetComponent<Enemies.Enemy>();
            if (enemy == null) enemy = other.GetComponentInParent<Enemies.Enemy>();

            if (enemy != null)
            {
                TakeDamage();
                enemy.TakeDamage(1);
                return;
            }

            Enemies.Boss.BossBarrierSegment barrier = other.GetComponent<Enemies.Boss.BossBarrierSegment>();
            if (barrier == null) barrier = other.GetComponentInParent<Enemies.Boss.BossBarrierSegment>();
            if (barrier != null)
            {
                TakeDamage();
                barrier.TakeDamage(1);
                return;
            }

            Enemies.Boss.BossCore core = other.GetComponent<Enemies.Boss.BossCore>();
            if (core == null) core = other.GetComponentInParent<Enemies.Boss.BossCore>();
            if (core != null)
            {
                TakeDamage();
                core.TakeDamage(1);
                return;
            }
        }
    }
}
