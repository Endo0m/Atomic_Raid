using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Game/Config Data")]
public class ConfigData : ScriptableObject
{
    [System.Serializable]
    public class SpawnStage
    {
        [Header("������� ��������� �����")]
        public float buffProbability;
        [Header("������� ��������� �������")]
        public float debuffProbability;
        [Header("�������� ��������� �����")]
        public float spawnInterval;
        [Header("������� ����� �����")]
        public int fieldsPerSpawn;
        [Header("����� ������� ��������� ������")]
        public float stageDuration;
    }

    [Header("������ ������")]
    public SpawnStage[] spawnStages;

    [Header("�� ������ ���� ����� ������� ���������� ����")]
    public float initialSpawnDelay = 0f;

    [Header("����")]
    public GameObject[] buffPrefabs;

    [Header("������")]
    public GameObject[] debuffPrefabs;

    [System.Serializable]
    public class FieldSpawnConfig
    {
        public Vector3 areaMin;
        public Vector3 areaMax;
        public LayerMask obstacleLayer;
        public int maxSpawnAttempts = 10;
        [Header("����������� ���������� ����� ���������")]
        public float minDistanceBetweenPortals = 5f;
    }

    [Header("��������� ������ �����")]
    public FieldSpawnConfig fieldSpawnConfig;


    [Space(15)]
    [Header("�������� �������� ������")]
    public float playerShootRate = 0.5f;

    [Space(15)]
    [Header("��������� ��������� �����")]
    [Header("�������� ��������")]
    public float enemyMoveSpeed = 5f;
    [Header("�������� �������� �������")]
    public float enemyMoveSpeedChaos = 5f;
    [Header("�������� ��������")]
    public float enemyShootInterval = 1f;
    [Header("��������� ��������")]
    public float enemyShootRange = 10f;
    [Header("������ �������(����� �� �������)")]
    public float enemyChaosRadius = 6f;
    [Header("�������� ����")]
    public float enemyBulletSpeed = 10f;
    [Header("����� ����(�� ���� ��� ��������)")]
    public float enemyBulletLifetime = 3f;

    [Space(15)]
    [Header("��������� ��������� ���������� �����")]
    public float moveSpeedScout = 50f;

    [Space(15)]
    [Header("��������� ��������� ")]
    [Header("�������� ������")]
    public float moveSpeedKamikadze = 80f;
    [Header("�� ����� ���������� �� ������ ���������")]
    public float explosionRangeKamikadze = 5f;
    [Header("���� ������")]
    public float explosionForceKamikadze = 400f;
    [Header("����")]
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