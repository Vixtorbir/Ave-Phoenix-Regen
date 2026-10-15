using UnityEngine;
using PhoenixReborn.Gameplay.Enemies;

namespace PhoenixReborn.Data
{
    public enum SpawnPatternType
    {
        RandomStream,
        GridRowFormation,
        VShapeFormation,
        AlternatingWings
    }

    /// <summary>
    /// ScriptableObject defining the configuration of a single enemy wave.
    /// Modular and data-driven — supports Space Invaders / Arcade formation patterns.
    /// </summary>
    [CreateAssetMenu(fileName = "WaveData_01", menuName = "PhoenixReborn/Wave Data")]
    public class WaveDataSO : ScriptableObject
    {
        [Header("Wave Info")]
        [SerializeField] private string waveTitle = "Wave 1";
        [SerializeField] private Enemy enemyPrefab;

        [Header("Spawn Parameters")]
        [SerializeField] private SpawnPatternType spawnPattern = SpawnPatternType.GridRowFormation;
        [SerializeField] private int totalEnemies = 16;
        [SerializeField] private int enemiesPerRow = 4;
        [SerializeField] private float spawnInterval = 0.6f;
        [SerializeField] private float spawnWidthRange = 4.0f;
        [SerializeField] private float spawnYPosition = 6.5f;

        public string WaveTitle => waveTitle;
        public Enemy EnemyPrefab => enemyPrefab;
        public SpawnPatternType SpawnPattern => spawnPattern;
        public int TotalEnemies => totalEnemies;
        public int EnemiesPerRow => Mathf.Max(1, enemiesPerRow);
        public float SpawnInterval => spawnInterval;
        public float SpawnWidthRange => spawnWidthRange;
        public float SpawnYPosition => spawnYPosition;
    }
}
