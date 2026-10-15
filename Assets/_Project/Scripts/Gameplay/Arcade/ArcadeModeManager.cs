using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using PhoenixReborn.Gameplay.Enemies;
using PhoenixReborn.Gameplay.Enemies.Boss;
using PhoenixReborn.Systems;
using PhoenixReborn.UI;

namespace PhoenixReborn.Gameplay.Arcade
{
    /// <summary>
    /// Runs the endless Arcade Mode.
    /// Controls infinite escalating waves with smooth staggered spawning,
    /// combined enemy assault patterns, boss battles every 10 waves with compact scaling,
    /// long-lasting +1 HP floating recovery, and dedicated graphic boss warning banner.
    /// </summary>
    public class ArcadeModeManager : MonoBehaviour
    {
        public static ArcadeModeManager Instance { get; private set; }

        [Header("Enemy Prefabs")]
        [SerializeField] private SmallBirdEnemy smallBirdPrefab;
        [SerializeField] private MediumPhoenixEnemy mediumPhoenixPrefab;
        [SerializeField] private BossController bossPrefab;

        [Header("Arcade Progression Settings")]
        [SerializeField] private float delayBeforeFirstWave = 1.5f;
        [SerializeField] private float delayBetweenWaves = 2.5f;
        [SerializeField] private int bossWaveInterval = 10;
        [SerializeField] private int lifeRecoveryInterval = 3;

        public int CurrentWave { get; private set; } = 0;
        public bool CanPlayerFire { get; private set; } = false;
        public bool IsWaveActive { get; private set; } = false;

        private ObjectPool<Enemy> _smallBirdPool;
        private ObjectPool<Enemy> _mediumPhoenixPool;
        private readonly List<Enemy> _activeEnemies = new List<Enemy>();
        private BossController _activeBoss;
        private PlayerHealth _playerHealth;
        private bool _isCurrentlySpawning = false;
        private int _wavesSurvivedSinceDamage = 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _playerHealth = FindFirstObjectByType<PlayerHealth>();

            // Enemy pools
            if (smallBirdPrefab != null)
            {
                _smallBirdPool = new ObjectPool<Enemy>(
                    createFunc: () => Instantiate(smallBirdPrefab),
                    actionOnGet: enemy => enemy.gameObject.SetActive(true),
                    actionOnRelease: enemy => enemy.gameObject.SetActive(false),
                    actionOnDestroy: enemy => Destroy(enemy.gameObject),
                    defaultCapacity: 15,
                    maxSize: 30);
            }

            if (mediumPhoenixPrefab != null)
            {
                _mediumPhoenixPool = new ObjectPool<Enemy>(
                    createFunc: () => Instantiate(mediumPhoenixPrefab),
                    actionOnGet: enemy => enemy.gameObject.SetActive(true),
                    actionOnRelease: enemy => enemy.gameObject.SetActive(false),
                    actionOnDestroy: enemy => Destroy(enemy.gameObject),
                    defaultCapacity: 8,
                    maxSize: 16);
            }
        }

        private void Start()
        {
            Time.timeScale = 1f;
            StartCoroutine(StartArcadeGameRoutine());
        }

        public void EnableFiringForBoss()
        {
            CanPlayerFire = true;
        }

        private IEnumerator StartArcadeGameRoutine()
        {
            yield return new WaitForSeconds(delayBeforeFirstWave);
            StartNextArcadeWave();
        }

        public void StartNextArcadeWave()
        {
            CurrentWave++;
            StartCoroutine(SpawnWaveRoutine(CurrentWave));
        }

        private IEnumerator SpawnWaveRoutine(int waveNumber)
        {
            IsWaveActive = true;
            _activeEnemies.Clear();

            bool isBossWave = (waveNumber % bossWaveInterval == 0);

            // 1. Trigger HUD Announcements
            if (isBossWave)
            {
                // Only show the graphic sprite warning for Boss (no text banner)
                HUDManager.Instance?.TriggerBossWarning();
                AudioManager.Instance?.PlayBossWarning();
            }
            else
            {
                // Show standard text wave banner
                HUDManager.Instance?.ShowWaveBanner($"WAVE {waveNumber}");
                AudioManager.Instance?.PlayWaveStart();
            }

            yield return new WaitForSeconds(1.5f);
            CanPlayerFire = true;

            // 2. Spawn Enemies or Boss
            if (isBossWave)
            {
                SpawnBoss();
            }
            else
            {
                yield return StartCoroutine(SpawnSmoothStaggeredWave(waveNumber));
            }
        }

        private IEnumerator SpawnSmoothStaggeredWave(int waveNumber)
        {
            _isCurrentlySpawning = true;

            int smallCount;
            int mediumCount;

            if (waveNumber == 1)
            {
                // Wave 1: 6 Small Birds
                smallCount = 6;
                mediumCount = 0;
            }
            else if (waveNumber == 2)
            {
                // Wave 2: 8 Small Birds
                smallCount = 8;
                mediumCount = 0;
            }
            else if (waveNumber == 3)
            {
                // Wave 3: 10 Total (7 Small Birds + 3 Medium Phoenixes)
                smallCount = 7;
                mediumCount = 3;
            }
            else if (waveNumber == 4)
            {
                // Wave 4: 12 Total (8 Small Birds + 4 Medium Phoenixes)
                smallCount = 8;
                mediumCount = 4;
            }
            else if (waveNumber == 5)
            {
                // Wave 5: 14 Total (9 Small Birds + 5 Medium Phoenixes)
                smallCount = 9;
                mediumCount = 5;
            }
            else
            {
                // Wave 6+: 16 Total, with +1 extra enemy after each boss cycle
                int bossCycles = waveNumber / bossWaveInterval;
                smallCount = Mathf.Min(16, 11 + bossCycles);
                mediumCount = Mathf.Min(6, 5 + (bossCycles / 2));
            }

            // 1. Spawn Medium Phoenixes steadily
            if (mediumCount > 0 && _mediumPhoenixPool != null)
            {
                float spacingX = 3.4f / Mathf.Max(1, mediumCount);
                float startX = -1.7f + (spacingX * 0.5f);

                for (int i = 0; i < mediumCount; i++)
                {
                    Enemy mediumEnemy = _mediumPhoenixPool.Get();
                    if (mediumEnemy != null)
                    {
                        Vector3 spawnPos = new Vector3(startX + (i * spacingX), 6.5f, 0f);
                        mediumEnemy.transform.position = spawnPos;
                        mediumEnemy.Init(_mediumPhoenixPool);
                        RegisterEnemy(mediumEnemy);
                    }
                    yield return new WaitForSeconds(0.55f);
                }

                yield return new WaitForSeconds(0.75f);
            }

            // 2. Spawn Small Birds steadily
            if (smallCount > 0 && _smallBirdPool != null)
            {
                for (int i = 0; i < smallCount; i++)
                {
                    Enemy smallEnemy = _smallBirdPool.Get();
                    if (smallEnemy != null)
                    {
                        float xSpread = 3.6f;
                        float x = UnityEngine.Random.Range(-xSpread * 0.5f, xSpread * 0.5f);
                        smallEnemy.transform.position = new Vector3(x, 6.0f, 0f);
                        smallEnemy.Init(_smallBirdPool);
                        RegisterEnemy(smallEnemy);
                    }

                    yield return new WaitForSeconds(0.55f);
                }
            }

            _isCurrentlySpawning = false;

            // Check in case all enemies were destroyed right as the last one spawned
            if (_activeEnemies.Count == 0 && IsWaveActive && _activeBoss == null)
            {
                OnWaveCleared();
            }
        }

        private void RegisterEnemy(Enemy enemy)
        {
            _activeEnemies.Add(enemy);
            enemy.OnEnemyDied += HandleEnemyDied;
        }

        private void HandleEnemyDied(Enemy enemy)
        {
            enemy.OnEnemyDied -= HandleEnemyDied;
            _activeEnemies.Remove(enemy);

            // Only trigger round clear if spawning has completely finished AND all enemies are dead
            if (!_isCurrentlySpawning && _activeEnemies.Count == 0 && IsWaveActive && _activeBoss == null)
            {
                OnWaveCleared();
            }
        }

        private void SpawnBoss()
        {
            if (bossPrefab == null)
            {
                Debug.LogWarning("[ArcadeModeManager] BossPrefab is missing in Inspector!");
                OnWaveCleared();
                return;
            }

            _activeBoss = Instantiate(bossPrefab, new Vector3(0f, 8f, 0f), Quaternion.identity);
            if (_activeBoss != null)
            {
                // Smaller boss in Arcade Mode (~70% of the original 4.5x3.5)
                _activeBoss.SetBaseScale(new Vector3(3.2f, 2.5f, 2.5f));
            }
            BossController.OnBossDefeated += HandleBossDefeated;
        }

        private void HandleBossDefeated()
        {
            BossController.OnBossDefeated -= HandleBossDefeated;
            _activeBoss = null;
            OnWaveCleared();
        }

        private void OnWaveCleared()
        {
            IsWaveActive = false;
            CanPlayerFire = false;

            StartCoroutine(WaveClearRoutine());
        }

        private IEnumerator WaveClearRoutine()
        {
            // Check Life Recovery: Must survive 3 full completed rounds after losing a life (or defeat a boss)
            bool isBossDefeated = (CurrentWave % bossWaveInterval == 0);

            if (_playerHealth != null && _playerHealth.CurrentLives < _playerHealth.MaxLives)
            {
                _wavesSurvivedSinceDamage++;
                Debug.Log($"[ArcadeModeManager] Round completed while damaged. Survival progress: {_wavesSurvivedSinceDamage}/3");

                if (_wavesSurvivedSinceDamage >= lifeRecoveryInterval || isBossDefeated)
                {
                    bool restored = _playerHealth.RestoreLife(1);
                    if (restored)
                    {
                        _wavesSurvivedSinceDamage = 0;
                        Vector3 popupPos = _playerHealth.transform.position + new Vector3(0f, 0.9f, 0f);
                        FloatingScoreManager.Instance?.SpawnCustomText(popupPos, "+1 HP", new Color(0.15f, 1f, 0.4f), 1.7f, 2.2f);
                        AudioManager.Instance?.PlayShieldActivate();
                        yield return new WaitForSeconds(1.8f);
                    }
                }
            }
            else
            {
                _wavesSurvivedSinceDamage = 0;
            }

            yield return new WaitForSeconds(delayBetweenWaves);
            StartNextArcadeWave();
        }

        private void OnDestroy()
        {
            BossController.OnBossDefeated -= HandleBossDefeated;
        }
    }
}
