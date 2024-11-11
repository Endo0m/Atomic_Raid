using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewEnemyWaveConfig", menuName = "Game/Enemy Wave Config")]
public class EnemyWaveConfig : ScriptableObject
{
    [System.Serializable]
    public class EnemyTypeConfig
    {
        [Header("������ �����")]
        public GameObject prefab;
        [Header("������������ ���������� ������")]
        public int maxEnemies;
        [Header("�������� ����")]
        public bool isFlying;
        [Header("����� ������ ���������")]
        public float spawnStartTime;
        [Header("������������ ����")]
        public List<WaveConfig> waveConfigs;
        [Header("������� ��������� (���������� � 0)")]
        public int level;
    }

    [System.Serializable]
    public class SpawnArea
    {
        [Header("����������� ����� ���� ���������")]
        public Vector3 min;
        [Header("������������ ����� ���� ���������")]
        public Vector3 max;
    }

    [System.Serializable]
    public class WaveConfig
    {
        [Header("����������������� �����")]
        public float duration;
        [Header("����������� ���������� ������")]
        public int minEnemies;
        [Header("������������ ���������� ������")]
        public int maxEnemies;
        [Header("����������� �������� ���������")]
        public float minSpawnInterval;
        [Header("������������ �������� ���������")]
        public float maxSpawnInterval;
    }

    [Header("���� ������")]
    public List<EnemyTypeConfig> enemyTypes;

    [Header("���� ��������� �������� ������")]
    public SpawnArea flyingSpawnArea;

    [Header("���� ��������� �������� ������")]
    public SpawnArea groundSpawnArea;

    [Header("���� �����������")]
    public LayerMask obstacleLayer;
}