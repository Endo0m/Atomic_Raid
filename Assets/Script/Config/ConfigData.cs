using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Game/Config Data")]
public class ConfigData : ScriptableObject
{
    [System.Serializable]
    public class SpawnStage
    {
        [Header("процент появления бафов")]
        public float buffProbability;
        [Header("процент появления дебафов")]
        public float debuffProbability;
        [Header("интервал появления полей")]
        public float spawnInterval;
        [Header("сколько полей будет")]
        public int fieldsPerSpawn;
        [Header("через сколько следующая стадия")]
        public float stageDuration;
    }

    [Header("Стадии спавна")]
    public SpawnStage[] spawnStages;

    [Header("от начала игры через сколько начинаются поля")]
    public float initialSpawnDelay = 0f;

    [Header("Бафы")]
    public GameObject[] buffPrefabs;

    [Header("Дебафы")]
    public GameObject[] debuffPrefabs;

    [System.Serializable]
    public class FieldSpawnConfig
    {
        public Vector3 areaMin;
        public Vector3 areaMax;
        public LayerMask obstacleLayer;
        public int maxSpawnAttempts = 10;
        [Header("Минимальное расстояние между порталами")]
        public float minDistanceBetweenPortals = 5f;
    }

    [Header("Настройки спавна полей")]
    public FieldSpawnConfig fieldSpawnConfig;


    [Space(15)]
    [Header("Скорость стрельбы ИГРОКА")]
    public float playerShootRate = 0.5f;

    [Space(15)]
    [Header("НАСТРОЙКИ ЛЕТАЮЩЕГО ВРАГА")]
    [Header("скорость движения")]
    public float enemyMoveSpeed = 5f;
    [Header("скорость движения уворота")]
    public float enemyMoveSpeedChaos = 5f;
    [Header("скорость стрельбы")]
    public float enemyShootInterval = 1f;
    [Header("дальность стрельбы")]
    public float enemyShootRange = 10f;
    [Header("радиус уворота(лучше не трогать)")]
    public float enemyChaosRadius = 6f;
    [Header("скорость пули")]
    public float enemyBulletSpeed = 10f;
    [Header("жизнь пули(до того как пропадет)")]
    public float enemyBulletLifetime = 3f;

    [Space(15)]
    [Header("НАСТРОЙКИ ЛЕТАЮЩЕГО РАЗВЕДЧИКА ВРАГА")]
    public float moveSpeedScout = 50f;

    [Space(15)]
    [Header("НАСТРОЙКИ КАМИКАДЗЕ ")]
    [Header("скорость полета")]
    public float moveSpeedKamikadze = 80f;
    [Header("на каком расстоянии от игрока взорвется")]
    public float explosionRangeKamikadze = 5f;
    [Header("сила толчка")]
    public float explosionForceKamikadze = 400f;
    [Header("урон")]
    public int damageAmountKamikadze = 1;

    // Singleton instance
    private static ConfigData _instance;
    public static ConfigData Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<ConfigData>("GameConfig");
                if (_instance == null)
                {
                    Debug.LogError("GameConfig not found in Resources folder. Creating a new instance.");
                    _instance = CreateInstance<ConfigData>();
                }
            }
            return _instance;
        }
    }
}