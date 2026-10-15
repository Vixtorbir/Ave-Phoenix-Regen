using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using PhoenixReborn.Data;

namespace PhoenixReborn.Gameplay.Enemies
{
    /// <summary>
    /// Manages spawning of enemy waves using data-driven WaveDataSO assets.
    /// Supports Space Invaders formation patterns (Grid, V-Shape, Alternating Wings).
    /// Enemies come from object pools.
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        [Header("Wave Sequence")]
        [SerializeField] private List<WaveDataSO> waves = new List<WaveDataSO>();
        [SerializeField] private bool loopWavesForArcade = true;
        [SerializeField] private bool autoStartOnStart = false;
        [SerializeField] private float delayBetweenWaves = 3f;

        private int _currentWaveIndex = 0;
        private int _enemiesRemainingToSpawn;
        private int _enemiesActiveCount;
        private bool _isSpawningWave;

        private readonly Dictionary<Enemy, ObjectPool<Enemy>> _enemyPools = new Dictionary<Enemy, ObjectPool<Enemy>>();

        public static WaveManager Instance { get; private set; }

        public event Action<string> OnWaveStarted;
        public event Action<int> OnWaveCompleted;
        public event Action OnAllWavesCleared;

        public int CurrentWaveNumber => _currentWaveIndex + 1;
        public bool IsWaveInProgress => _isSpawningWave || _enemiesActiveCount > 0;
        public bool CanPlayerFire { get; private set; } = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (autoStartOnStart && waves.Count > 0)
            {
                StartNextWave();
            }
        }

        public void EnableFiringForBoss()
        {
            CanPlayerFire = true;
        }

        public void DisableFiring()
        {
            CanPlayerFire = false;
        }

        public void SetLoopMode(bool loop)
        {
            loopWavesForArcade = loop;
        }

        public void StartNextWave()
        {
            if (_currentWaveIndex >= waves.Count)
            {
                if (loopWavesForArcade)
                {
                    _currentWaveIndex = 0;
                }
                else
                {
                    OnAllWavesCleared?.Invoke();
                    return;
                }
            }

            int waveIdx = _currentWaveIndex;
            if (waveIdx == 0 && Systems.DialogueManager.Instance != null)
            {
                Systems.DialogueManager.Instance.PlayWave1Story(() =>
                {
                    StartCoroutine(SpawnWaveRoutine(waves[waveIdx]));
                });
            }
            else if (waveIdx == 1 && Systems.DialogueManager.Instance != null)
            {
                Systems.DialogueManager.Instance.PlayWave2Story(() =>
                {
                    StartCoroutine(SpawnWaveRoutine(waves[waveIdx]));
                });
            }
            else
            {
                StartCoroutine(SpawnWaveRoutine(waves[waveIdx]));
            }
        }

        private IEnumerator SpawnWaveRoutine(WaveDataSO waveData)
        {
            if (waveData == null || waveData.EnemyPrefab == null)
            {
                Debug.LogError($"[WaveManager] Cannot start wave index {_currentWaveIndex}: WaveDataSO or EnemyPrefab is missing in the Inspector!");
                _isSpawningWave = false;
                _currentWaveIndex++;
                StartNextWave();
                yield break;
            }

            _isSpawningWave = true;
            _enemiesRemainingToSpawn = waveData.TotalEnemies;
            _enemiesActiveCount = 0;
            CanPlayerFire = false;

            OnWaveStarted?.Invoke(waveData.WaveTitle);
            Debug.Log($"[WaveManager] Starting {waveData.WaveTitle} (Enemies: {waveData.TotalEnemies})");

            yield return new WaitForSeconds(1.8f);

            ObjectPool<Enemy> pool = GetOrCreatePool(waveData.EnemyPrefab);
            int spawnIndex = 0;
            int squadSize = Mathf.Max(2, waveData.EnemiesPerRow);

            while (_enemiesRemainingToSpawn > 0)
            {
                int currentBatch = Mathf.Min(squadSize, _enemiesRemainingToSpawn);
                for (int b = 0; b < currentBatch; b++)
                {
                    SpawnEnemy(waveData, pool, spawnIndex);
                    if (spawnIndex == 0)
                    {
                        CanPlayerFire = true;
                    }
                    spawnIndex++;
                    _enemiesRemainingToSpawn--;
                    yield return new WaitForSeconds(0.55f);
                }

                if (_enemiesRemainingToSpawn > 0)
                {
                    yield return new WaitForSeconds(waveData.SpawnInterval);
                }
            }

            _isSpawningWave = false;
            CheckWaveCompletion();
        }

        private void SpawnEnemy(WaveDataSO waveData, ObjectPool<Enemy> pool, int spawnIndex)
        {
            Enemy enemy = pool.Get();
            if (enemy == null) return;

            Vector3 spawnPos = CalculateSpawnPosition(waveData, spawnIndex);

            enemy.transform.position = spawnPos;
            enemy.OnEnemyDied += HandleEnemyDied;
            enemy.Init(pool);

            _enemiesActiveCount++;
        }

        private Vector3 CalculateSpawnPosition(WaveDataSO waveData, int index)
        {
            float y = Mathf.Max(waveData.SpawnYPosition, 6.2f);
            float range = GetEffectiveSpawnWidth(waveData);
            int perRow = waveData.EnemiesPerRow;

            switch (waveData.SpawnPattern)
            {
                case SpawnPatternType.GridRowFormation:
                    int col = index % perRow;
                    float step = perRow > 1 ? (range * 2f) / (perRow - 1) : 0f;
                    float x = -range + (col * step);
                    return new Vector3(x, y, 0f);

                case SpawnPatternType.VShapeFormation:
                    int vCol = index % perRow;
                    float vStep = perRow > 1 ? (range * 2f) / (perRow - 1) : 0f;
                    float vX = -range + (vCol * vStep);
                    float vYOffset = Mathf.Abs(vCol - (perRow - 1) / 2f) * 0.6f;
                    return new Vector3(vX, y + vYOffset, 0f);

                case SpawnPatternType.AlternatingWings:
                    // Clean wide alternating lanes to guarantee wide physical separation
                    float[] xLanes = new float[] { -1.85f, 1.85f, -0.95f, 0.95f, -1.40f, 1.40f, 0.0f };
                    float laneX = xLanes[index % xLanes.Length];
                    return new Vector3(laneX, y, 0f);

                default:
                    float randomX = UnityEngine.Random.Range(-range, range);
                    return new Vector3(randomX, y, 0f);
            }
        }

        private float GetEffectiveSpawnWidth(WaveDataSO waveData)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                float halfWidth = cam.orthographicSize * cam.aspect;
                return Mathf.Min(waveData.SpawnWidthRange, Mathf.Max(1.4f, halfWidth - 0.4f));
            }
            return waveData.SpawnWidthRange;
        }

        private void HandleEnemyDied(Enemy enemy)
        {
            enemy.OnEnemyDied -= HandleEnemyDied;
            _enemiesActiveCount--;
            CheckWaveCompletion();
        }

        private void CheckWaveCompletion()
        {
            if (!_isSpawningWave && _enemiesActiveCount <= 0)
            {
                CanPlayerFire = false;
                OnWaveCompleted?.Invoke(_currentWaveIndex);
                Debug.Log($"[WaveManager] Wave {CurrentWaveNumber} Completed!");

                _currentWaveIndex++;
                Invoke(nameof(StartNextWave), delayBetweenWaves);
            }
        }

        private ObjectPool<Enemy> GetOrCreatePool(Enemy prefab)
        {
            if (_enemyPools.TryGetValue(prefab, out var existingPool))
            {
                return existingPool;
            }

            ObjectPool<Enemy> newPool = new ObjectPool<Enemy>(
                createFunc: () => Instantiate(prefab),
                actionOnGet: e => e.gameObject.SetActive(true),
                actionOnRelease: e => e.gameObject.SetActive(false),
                actionOnDestroy: e => Destroy(e.gameObject),
                defaultCapacity: 15,
                maxSize: 50);

            _enemyPools.Add(prefab, newPool);
            return newPool;
        }
    }
}
