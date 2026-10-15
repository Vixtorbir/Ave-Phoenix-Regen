using UnityEngine;
using UnityEngine.Pool;
using PhoenixReborn.Gameplay.Enemies;
using PhoenixReborn.Gameplay.Enemies.Boss;

namespace PhoenixReborn.Gameplay.Weapons
{
    /// <summary>
    /// Pooled enemy projectile that travels downward toward the player ship.
    /// Inflicts damage on contact with the Player ship.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyProjectile : MonoBehaviour
    {
        [SerializeField] private float speed = 8.5f;
        [SerializeField] private float lifetime = 3.5f;

        private ObjectPool<EnemyProjectile> _pool;
        private float _timer;
        private bool _isReturned;

        private void Awake()
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }

        public void Init(ObjectPool<EnemyProjectile> pool)
        {
            _pool = pool;
            _timer = 0f;
            _isReturned = false;

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = new Color(1.6f, 1.6f, 1.6f, 1f);
            }
        }

        private void Update()
        {
            if (_isReturned) return;

            transform.Translate(Vector3.down * speed * Time.deltaTime, Space.World);

            _timer += Time.deltaTime;
            if (_timer >= lifetime)
            {
                ReturnToPool();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isReturned) return;

            if (other.CompareTag("Enemy") ||
                other.GetComponent<Enemy>() != null ||
                other.GetComponentInParent<Enemy>() != null ||
                other.GetComponent<BossBarrierSegment>() != null ||
                other.GetComponentInParent<BossBarrierSegment>() != null ||
                other.GetComponent<BossCore>() != null ||
                other.GetComponentInParent<BossCore>() != null ||
                other.GetComponentInParent<BossController>() != null ||
                other.GetComponent<EnemyProjectile>() != null ||
                other.GetComponent<Bullet>() != null)
            {
                return;
            }

            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth == null)
            {
                playerHealth = other.GetComponentInParent<PlayerHealth>();
            }

            if (playerHealth != null)
            {
                playerHealth.TakeDamage();
                ReturnToPool();
                return;
            }

            if (other.CompareTag("Player") || other.GetComponentInParent<PlayerMovement>() != null)
            {
                ReturnToPool();
            }
        }

        public void ReturnToPool()
        {
            if (_isReturned) return;
            _isReturned = true;

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
