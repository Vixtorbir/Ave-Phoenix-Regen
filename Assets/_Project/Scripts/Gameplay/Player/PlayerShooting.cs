using UnityEngine;
using UnityEngine.Pool;

namespace PhoenixReborn.Gameplay
{
    /// <summary>
    /// Fires pooled bullets while the Fire action is held, respecting
    /// a fire-rate cooldown. Never Instantiates/Destroys at runtime.
    /// Disables firing while shield is active.
    /// </summary>
    [RequireComponent(typeof(PlayerInputReader))]
    public class PlayerShooting : MonoBehaviour
    {
        [SerializeField] private Bullet bulletPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float shotsPerSecond = 7.5f;

        private PlayerInputReader _input;
        private PlayerShield _shield;
        private ObjectPool<Bullet> _bulletPool;
        private float _cooldown;

        private void Awake()
        {
            _input = GetComponent<PlayerInputReader>();
            _shield = GetComponent<PlayerShield>();

            if (firePoint == null)
            {
                firePoint = transform;
            }

            _bulletPool = new ObjectPool<Bullet>(
                createFunc: () =>
                {
                    if (bulletPrefab == null)
                    {
                        Debug.LogError("[PlayerShooting] BulletPrefab is missing in Inspector!");
                        return null;
                    }
                    return Instantiate(bulletPrefab);
                },
                actionOnGet: bullet =>
                {
                    if (bullet != null) bullet.gameObject.SetActive(true);
                },
                actionOnRelease: bullet =>
                {
                    if (bullet != null) bullet.gameObject.SetActive(false);
                },
                actionOnDestroy: bullet =>
                {
                    if (bullet != null) Destroy(bullet.gameObject);
                },
                defaultCapacity: 20,
                maxSize: 60);
        }

        private void Update()
        {
            _cooldown -= Time.deltaTime;

            // Do not fire during story dialogues
            if (Systems.DialogueManager.Instance != null && Systems.DialogueManager.Instance.IsDialogueActive)
            {
                return;
            }

            // Do not fire until the first enemy of the current wave appears
            if (Arcade.ArcadeModeManager.Instance != null)
            {
                if (!Arcade.ArcadeModeManager.Instance.CanPlayerFire) return;
            }
            else if (Enemies.WaveManager.Instance != null && !Enemies.WaveManager.Instance.CanPlayerFire)
            {
                return;
            }

            if (_input != null && _input.FireHeld && _cooldown <= 0f)
            {
                Fire();
                _cooldown = 1f / shotsPerSecond;
            }
        }

        private void Fire()
        {
            if (bulletPrefab == null)
            {
                Debug.LogWarning("[PlayerShooting] Cannot fire: BulletPrefab is null.");
                return;
            }

            Bullet bullet = _bulletPool.Get();
            if (bullet == null) return;

            Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
            Quaternion spawnRotation = firePoint != null ? firePoint.rotation : Quaternion.identity;

            bullet.transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            bullet.Init(_bulletPool);

            Systems.AudioManager.Instance?.PlayPlayerShoot();
        }
    }
}
