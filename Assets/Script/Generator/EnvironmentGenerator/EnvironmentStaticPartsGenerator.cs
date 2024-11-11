using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EnvironmentStaticPartsGenerator : MonoBehaviour
{
    [SerializeField] private GameObject[] partsPrefabs;
    [SerializeField] private Transform[] spawnPoints;

    private void Start()
    {
        if (enabled)
        {
            GenerateParts();
        }
    }

    public void GenerateParts()
    {
        if (partsPrefabs.Length != 4 || spawnPoints.Length != 4)
        {
            Debug.LogError("There must be exactly 4 prefabs and 4 spawn points!");
            return;
        }

        // Очистка существующих частей
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Part_"))
            {
                Destroy(child.gameObject);
            }
        }

        List<int> shuffledIndices = Enumerable.Range(0, 4).ToList();
        ShuffleList(shuffledIndices);

        for (int i = 0; i < 4; i++)
        {
            GameObject prefab = partsPrefabs[shuffledIndices[i]];
            Transform spawnPoint = spawnPoints[i];

            GameObject instance = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation, transform);
            instance.name = $"Part_{i}";
        }
    }

    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}