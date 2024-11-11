using UnityEngine;
using System;

public class ArcGeneration : MonoBehaviour
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

    // Статическое свойство для отслеживания состояния босс-файта
    public static bool IsBossFightActive { get; set; }

    public void TryGenerateArcs()
    {
        Debug.Log($"Attempting to generate arcs. Is boss fight active: {IsBossFightActive}");

        if (IsBossFightActive)
        {
            Debug.Log("Boss fight is active. Skipping arc generation.");
            return;
        }

        GenerateArcs();
    }

    private void GenerateArcs()
    {
        Debug.Log("Starting arc generation.");
        foreach (var config in spawnPointConfigs)
        {
            if (config.spawnPoint == null)
            {
                Debug.LogWarning("Spawn point is null in ArcGeneration config.");
                continue;
            }

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
        if (arcPrefabs.Length == 0)
        {
            Debug.LogWarning("No arc prefabs assigned to ArcGeneration.");
            return;
        }

        int randomPrefabIndex = UnityEngine.Random.Range(0, arcPrefabs.Length);
        GameObject spawnedArc = Instantiate(arcPrefabs[randomPrefabIndex], spawnPoint.position, spawnPoint.rotation, transform);
        Debug.Log($"Spawned arc at position: {spawnedArc.transform.position}");
    }
}