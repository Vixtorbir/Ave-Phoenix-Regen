using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using PhoenixReborn.Gameplay.Weapons;

namespace PhoenixReborn.Gameplay.Enemies.Boss
{
    /// <summary>
    /// Controls the Phoenix Mother Ship boss fight.
    /// Manages Boss State Machine, weapon firing routines, barrier status,
    /// and periodic minion escort spawns during combat.
    /// </summary>
    public class BossController : MonoBehaviour
    {
        [Header("Positions & Movement")]
        [SerializeField] private Vector3 spawnStartPosition = new Vector3(0f, 8f, 0f);
        [SerializeField] private Vector3 battleStartPosition = new Vector3(0f, 4.3f, 0f);
        [SerializeField] private float horizontalRange = 3.5f;

        [Header("Phase Parameters")]
        [SerializeField] private float phase1Speed = 1.4f;
        [SerializeField] private float phase1FireRate = 2.4f;
        [SerializeField] private float phase2Speed = 2.5f;
        [SerializeField] private float phase2FireRate = 1.6f;

        [Header("Minion Escorts (Wave 3 Reinforcements)")]
        [SerializeField] private bool enableMinionSpawns = true;
        [SerializeField] private float minionSpawnInterval = 6.0f;
        [SerializeField] private Enemy scoutMinionPrefab;
        [SerializeField] private Enemy warriorMinionPrefab;

        [Header("Weapons & Barriers")]
        [SerializeField] private EnemyProjectile projectilePrefab;
        [SerializeField] private List<Transform> firePoints = new List<Transform>();
        [SerializeField] private BossCore bossCore;
        [SerializeField] private List<BossBarrierSegment> barrierSegments = new List<BossBarrierSegment>();

        private IBossState _currentState;
        private int _moveDirection = 1;
        private ObjectPool<EnemyProjectile> _projectilePool;
        private int _destroyedBarrierCount;
        private Vector3 _baseScale;
        private float _minionTimer;
        private readonly List<Enemy> _activeMinions = new List<Enemy>();

        public Vector3 SpawnStartPosition => spawnStartPosition;
        public Vector3 BattleStartPosition => battleStartPosition;
        public float Phase1Speed => phase1Speed;
        public float Phase1FireRate => phase1FireRate;
        public float Phase2Speed => phase2Speed;
        public float Phase2FireRate => phase2FireRate;

        public static event Action OnBossIncoming;
        public static event Action OnBossDefeated;

        public static void NotifyBossIncoming() => OnBossIncoming?.Invoke();

        public void SetBaseScale(Vector3 scale)
        {
            _baseScale = scale;
            transform.localScale = scale;
        }

        private void Awake()
        {
            _baseScale = transform.localScale;
            _minionTimer = minionSpawnInterval;

            if (projectilePrefab != null)
            {
                _projectilePool = new ObjectPool<EnemyProjectile>(
                    createFunc: () => Instantiate(projectilePrefab),
                    actionOnGet: proj => proj.gameObject.SetActive(true),
                    actionOnRelease: proj => proj.gameObject.SetActive(false),
                    actionOnDestroy: proj => Destroy(proj.gameObject),
                    defaultCapacity: 15,
                    maxSize: 40);
            }

            if (bossCore != null)
            {
                bossCore.OnCoreDestroyed += HandleCoreDestroyed;
            }

            if (barrierSegments == null || barrierSegments.Count == 0)
            {
                barrierSegments = new List<BossBarrierSegment>(GetComponentsInChildren<BossBarrierSegment>(true));
            }

            foreach (var segment in barrierSegments)
            {
                if (segment != null)
                {
                    segment.OnSegmentDestroyed += HandleSegmentDestroyed;
                }
            }
        }

        private void Start()
        {
            transform.position = spawnStartPosition;
            ChangeState(new BossIntroState());
        }

        private void Update()
        {
            _currentState?.Update(this, Time.deltaTime);

            float pulse = 1f + (Mathf.Sin(Time.time * 2.5f) * 0.03f);
            transform.localScale = new Vector3(_baseScale.x * pulse, _baseScale.y * (2f - pulse), _baseScale.z);

            if (enableMinionSpawns && (_currentState is BossPhase1State || _currentState is BossPhase2State))
            {
                _minionTimer -= Time.deltaTime;
                if (_minionTimer <= 0f)
                {
                    SpawnMinionEscort();
                    _minionTimer = minionSpawnInterval;
                }
            }
        }

        public void ChangeState(IBossState newState)
        {
            _currentState?.Exit(this);
            _currentState = newState;
            _currentState?.Enter(this);
        }

        public void MoveHorizontally(float deltaTime, float speed)
        {
            float maxRange = GetMaxHorizontalRange();
            float newX = transform.position.x + (_moveDirection * speed * deltaTime);

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

            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
        }

        private float GetMaxHorizontalRange()
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                float halfWidth = cam.orthographicSize * cam.aspect;
                return Mathf.Min(horizontalRange, Mathf.Max(1.0f, halfWidth - 1.2f));
            }
            return horizontalRange;
        }

        public void FireBossWeapons()
        {
            if (_projectilePool == null || firePoints.Count == 0) return;

            foreach (var firePoint in firePoints)
            {
                if (firePoint == null || !firePoint.gameObject.activeInHierarchy) continue;

                EnemyProjectile proj = _projectilePool.Get();
                if (proj != null)
                {
                    proj.transform.position = firePoint.position;
                    proj.Init(_projectilePool);
                }
            }

            Systems.AudioManager.Instance?.PlayEnemyShoot();
        }

        private void SpawnMinionEscort()
        {
            Enemy prefab = (UnityEngine.Random.value > 0.5f && warriorMinionPrefab != null) ? warriorMinionPrefab : scoutMinionPrefab;
            if (prefab == null) return;

            Enemy minion = Instantiate(prefab);
            float spawnX = (UnityEngine.Random.value > 0.5f ? -1 : 1) * UnityEngine.Random.Range(2.5f, 4.5f);
            minion.transform.position = new Vector3(spawnX, 6.8f, 0f);
            minion.Init(null);
            _activeMinions.Add(minion);
        }

        public bool HasActiveBarriers => barrierSegments != null && barrierSegments.Exists(s => s != null && !s.IsDestroyed && s.gameObject.activeInHierarchy);
        public int ActiveBarrierCount => barrierSegments != null ? barrierSegments.FindAll(s => s != null && !s.IsDestroyed && s.gameObject.activeInHierarchy).Count : 0;

        public float GetDestroyedBarrierRatio()
        {
            if (barrierSegments == null || barrierSegments.Count == 0) return 1f;
            return (float)_destroyedBarrierCount / barrierSegments.Count;
        }

        private void HandleSegmentDestroyed(BossBarrierSegment segment)
        {
            segment.OnSegmentDestroyed -= HandleSegmentDestroyed;
            _destroyedBarrierCount++;
        }

        private void HandleCoreDestroyed()
        {
            ChangeState(new BossDefeatedState());
        }

        public void TriggerDeathSequence()
        {
            foreach (var minion in _activeMinions)
            {
                if (minion != null && minion.gameObject.activeInHierarchy)
                {
                    minion.RecycleWithoutScore();
                }
            }
            _activeMinions.Clear();

            OnBossDefeated?.Invoke();
            Destroy(gameObject, 0.5f);
        }

        private void OnDestroy()
        {
            if (bossCore != null)
            {
                bossCore.OnCoreDestroyed -= HandleCoreDestroyed;
            }
        }
    }
}
