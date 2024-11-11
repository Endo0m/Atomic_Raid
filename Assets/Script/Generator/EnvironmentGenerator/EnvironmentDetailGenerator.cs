using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EnvironmentDetailGenerator : MonoBehaviour
{
    [System.Serializable]
    public class DetailWithSpawnPoints
    {
        public GameObject prefab;
        public Transform[] spawnPoints;
    }

    public DetailWithSpawnPoints[] details;
    public float minFillPercentage = 0.3f;

    private Dictionary<GameObject, List<int>> availableSpawnPoints = new Dictionary<GameObject, List<int>>();
    private Dictionary<GameObject, int> lastSpawnedIndex = new Dictionary<GameObject, int>();

    void Start()
    {
        InitializeAvailableSpawnPoints();
        GenerateDetails();
    }

    void InitializeAvailableSpawnPoints()
    {
        foreach (var detail in details)
        {
            availableSpawnPoints[detail.prefab] = Enumerable.Range(0, detail.spawnPoints.Length).ToList();
            lastSpawnedIndex[detail.prefab] = -1;
        }
    }

    void GenerateDetails()
    {
        foreach (var detail in details)
        {
            int totalPoints = detail.spawnPoints.Length;
            int minDetailsToSpawn = Mathf.CeilToInt(totalPoints * minFillPercentage);
            int spawnedDetails = 0;

            while (spawnedDetails < minDetailsToSpawn)
            {
                SpawnDetailAtPoint(detail);
                spawnedDetails++;
            }
        }
    }

    void SpawnDetailAtPoint(DetailWithSpawnPoints detail)
    {
        if (availableSpawnPoints[detail.prefab].Count == 0)
        {
            Debug.LogWarning($"No more available spawn points for {detail.prefab.name}");
            return;
        }

        int spawnPointIndex = GetUniqueRandomIndex(detail.prefab);
        Transform spawnPoint = detail.spawnPoints[spawnPointIndex];

        // Создаем позицию, сохраняя Y-координату точки спавна
        Vector3 spawnPosition = new Vector3(spawnPoint.position.x, spawnPoint.position.y, spawnPoint.position.z);

        // Получаем начальную ротацию префаба
        Quaternion prefabRotation = detail.prefab.transform.rotation;

        // Добавляем случайную ротацию по Y в диапазоне от -60 до 60 градусов
        float randomYRotation = Random.Range(-60f, 60f);
        Quaternion randomRotation = Quaternion.Euler(0, randomYRotation, 0);

        // Комбинируем начальную ротацию префаба со случайной ротацией
        Quaternion finalRotation = prefabRotation * randomRotation;

        GameObject spawnedDetail = Instantiate(detail.prefab, spawnPosition, finalRotation, transform);
        spawnedDetail.transform.localScale = detail.prefab.transform.localScale;
    }

    int GetUniqueRandomIndex(GameObject prefab)
    {
        List<int> available = availableSpawnPoints[prefab];
        int randomIndex;

        do
        {
            randomIndex = available[Random.Range(0, available.Count)];
        } while (randomIndex == lastSpawnedIndex[prefab] && available.Count > 1);

        available.Remove(randomIndex);
        lastSpawnedIndex[prefab] = randomIndex;

        if (available.Count == 0)
        {
            availableSpawnPoints[prefab] = Enumerable.Range(0, details.First(d => d.prefab == prefab).spawnPoints.Length).ToList();
            availableSpawnPoints[prefab].Remove(randomIndex);
        }

        return randomIndex;
    }

    void OnDrawGizmos()
    {
        if (details == null) return;

        Gizmos.color = Color.yellow;
        foreach (var detail in details)
        {
            foreach (var point in detail.spawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawSphere(point.position, 0.1f);
                }
            }
        }
    }
}