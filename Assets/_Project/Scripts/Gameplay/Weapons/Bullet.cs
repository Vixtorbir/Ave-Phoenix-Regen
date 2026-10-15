using UnityEngine;
using UnityEngine.Pool;

namespace PhoenixReborn.Gameplay
{
    /// <summary>
    /// Pooled projectile. Returns itself to the pool on timeout or
    /// on hitting an enemy — never destroyed and reinstantiated.
    /// Uses Kinematic Rigidbody2D so trigger events fire correctly.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private float speed = 16f;
        [SerializeField] private float lifetime = 2.5f;

        private ObjectPool<Bullet> _pool;
        private float _timer;
        private bool _isReturned;

        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            if (_rb != null)
            {
                _rb.gravityScale = 0f;
                _rb.bodyType = RigidbodyType2D.Kinematic;
                _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }

        public void Init(ObjectPool<Bullet> pool)
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

            transform.Translate(Vector3.up * speed * Time.deltaTime, Space.Self);

            _timer += Time.deltaTime;
            if (_timer >= lifetime)
            {
                ReturnToPool();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isReturned) return;

            if (other.CompareTag("Player") || other.GetComponentInParent<PlayerMovement>() != null)
            {
                return;
            }

            if (other.GetComponent<Bullet>() != null)
            {
                return;
            }

            Core.IDamageable damageable = other.GetComponent<Core.IDamageable>();
            if (damageable == null)
            {
                damageable = other.GetComponentInParent<Core.IDamageable>();
            }

            if (damageable != null)
            {
                damageable.TakeDamage(1);
            }

            ReturnToPool();
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
