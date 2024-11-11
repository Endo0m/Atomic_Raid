using System;
using UnityEngine;

public class ManekenGen : MonoBehaviour
{
    [Serializable]
    public class SpawnPointConfig
    {
        public Transform spawnPoint;
        public GenerationType generationType;
        [Range(0f, 1f)]
        public float spawnProbability = 0.5f;
    }

    public enum GenerationType
    {
        RandomPrefabOrEmpty,
        RandomPrefab
    }

    [SerializeField] private GameObject[] arcPrefabs;
    [SerializeField] private SpawnPointConfig[] spawnPointConfigs;

    private void Start()
    {
        GenerateArcs();
    }

    public void GenerateArcs()
    {
        foreach (var config in spawnPointConfigs)
        {
            if (config.generationType == GenerationType.RandomPrefabOrEmpty)
            {
                if (UnityEngine.Random.value < config.spawnProbability)
                {
                    SpawnRandomPrefab(config.spawnPoint);
                }
            }
            else if (config.generationType == GenerationType.RandomPrefab)
            {
                SpawnRandomPrefab(config.spawnPoint);
            }
        }
    }

    private void SpawnRandomPrefab(Transform spawnPoint)
    {
        int randomPrefabIndex = UnityEngine.Random.Range(0, arcPrefabs.Length);
        Instantiate(arcPrefabs[randomPrefabIndex], spawnPoint.position, spawnPoint.rotation, transform);
    }
}