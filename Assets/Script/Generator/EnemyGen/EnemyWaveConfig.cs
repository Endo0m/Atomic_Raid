using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewEnemyWaveConfig", menuName = "Game/Enemy Wave Config")]
public class EnemyWaveConfig : ScriptableObject
{
    [System.Serializable]
    public class EnemyTypeConfig
    {
        [Header("Префаб врага")]
        public GameObject prefab;
        [Header("Максимальное количество врагов")]
        public int maxEnemies;
        [Header("Летающий враг")]
        public bool isFlying;
        [Header("Время начала появления")]
        public float spawnStartTime;
        [Header("Конфигурации волн")]
        public List<WaveConfig> waveConfigs;
        [Header("Уровень появления (начинается с 0)")]
        public int level;
    }

    [System.Serializable]
    public class SpawnArea
    {
        [Header("Минимальная точка зоны появления")]
        public Vector3 min;
        [Header("Максимальная точка зоны появления")]
        public Vector3 max;
    }

    [System.Serializable]
    public class WaveConfig
    {
        [Header("Продолжительность волны")]
        public float duration;
        [Header("Минимальное количество врагов")]
        public int minEnemies;
        [Header("Максимальное количество врагов")]
        public int maxEnemies;
        [Header("Минимальный интервал появления")]
        public float minSpawnInterval;
        [Header("Максимальный интервал появления")]
        public float maxSpawnInterval;
    }

    [Header("Типы врагов")]
    public List<EnemyTypeConfig> enemyTypes;

    [Header("Зона появления летающих врагов")]
    public SpawnArea flyingSpawnArea;

    [Header("Зона появления наземных врагов")]
    public SpawnArea groundSpawnArea;

    [Header("Слой препятствий")]
    public LayerMask obstacleLayer;
}