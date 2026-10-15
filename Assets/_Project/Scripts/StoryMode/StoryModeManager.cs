using System.Collections;
using UnityEngine;
using PhoenixReborn.Data;
using PhoenixReborn.Gameplay.Enemies;
using PhoenixReborn.Gameplay.Enemies.Boss;
using PhoenixReborn.Core;
using PhoenixReborn.UI;

namespace PhoenixReborn.StoryMode
{
    /// <summary>
    /// Runs Story Mode and sets up Arcade Mode.
    /// In Story Mode: Controls Waves 1 & 2 -> Boss Fight -> Victory Sequence.
    /// In Arcade Mode: Sets up infinite looping waves for high-score survival.
    /// </summary>
    public class StoryModeManager : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private WaveManager waveManager;
        [SerializeField] private BossController bossPrefab;

        [Header("Narrative Sequences (ScriptableObjects)")]
        [SerializeField] private StorySequenceSO introDialogueSequence;
        [SerializeField] private StorySequenceSO preBossDialogueSequence;
        [SerializeField] private StorySequenceSO victoryDialogueSequence;

        private BossController _activeBoss;

        private void Awake()
        {
#if UNITY_2023_1_OR_NEWER
            if (waveManager == null) waveManager = FindFirstObjectByType<WaveManager>();
#else
            if (waveManager == null) waveManager = FindObjectOfType<WaveManager>();
#endif
        }

        private void Start()
        {
            if (MainMenuManager.IsArcadeModeOnly)
            {
                Debug.Log("<color=#FFD54F>[GameMode] Starting Arcade Mode (Infinite Waves & High Score Survival)...</color>");
                StartArcadeWaves();
            }
            else
            {
                Debug.Log("<color=#00FF88>[GameMode] Starting Story Mode Campaign (Waves + Boss Fight)...</color>");
                StartEnemyWaves();
            }
        }

        private void StartArcadeWaves()
        {
            if (waveManager != null)
            {
                waveManager.SetLoopMode(true);
                waveManager.StartNextWave();
            }
            else
            {
                Debug.LogWarning("[StoryModeManager] WaveManager reference missing in Arcade Mode.");
            }
        }

        private void StartEnemyWaves()
        {
            if (waveManager != null)
            {
                waveManager.SetLoopMode(false);
                waveManager.OnAllWavesCleared += HandleAllWavesCleared;
                waveManager.StartNextWave();
            }
            else
            {
                Debug.LogWarning("[StoryModeManager] WaveManager reference missing.");
            }
        }

        private void HandleAllWavesCleared()
        {
            if (waveManager != null) waveManager.OnAllWavesCleared -= HandleAllWavesCleared;

            Debug.Log("[StoryModeManager] Waves 1 & 2 cleared! Preparing Boss Fight...");
            SpawnBossFight();
        }

        private void SpawnBossFight()
        {
            if (bossPrefab == null)
            {
                Debug.LogError("[StoryModeManager] BossPrefab is not assigned in the Inspector!");
                return;
            }

            _activeBoss = Instantiate(bossPrefab);
            BossController.OnBossDefeated += HandleBossDefeated;
        }

        private void HandleBossDefeated()
        {
            BossController.OnBossDefeated -= HandleBossDefeated;
            Debug.Log("[StoryModeManager] Victory! Phoenix Mother Ship Destroyed.");
        }
    }
}
