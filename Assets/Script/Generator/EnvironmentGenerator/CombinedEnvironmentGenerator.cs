using UnityEngine;
using System.Collections.Generic;

public class CombinedEnvironmentGenerator : MonoBehaviour
{
    [System.Serializable]
    public class ShipCategory
    {
        public string name;
        public GameObject[] prefabs;
        public SpawnZone[] spawnZones;
        [Range(1, 20)]
        public int minCount = 1;
        [Range(1, 20)]
        public int maxCount = 5;
        public float minDistanceBetweenShips = 10f;
    }

    [System.Serializable]
    public class SpawnZone
    {
        public Vector3 center;
        public Vector3 size;
    }

    public ShipCategory[] shipCategories;
    private List<GameObject> spawnedShips = new List<GameObject>();

    private void Start()
    {
        GenerateShips();
    }

    public void GenerateShips()
    {
        ClearShips();

        foreach (var category in shipCategories)
        {
            int shipsToSpawn = Random.Range(category.minCount, category.maxCount + 1);

            for (int i = 0; i < shipsToSpawn; i++)
            {
                SpawnShip(category);
            }
        }
    }

    private void SpawnShip(ShipCategory category)
    {
        GameObject prefab = category.prefabs[Random.Range(0, category.prefabs.Length)];
        SpawnZone zone = category.spawnZones[Random.Range(0, category.spawnZones.Length)];

        for (int attempts = 0; attempts < 50; attempts++)
        {
            Vector3 position = GetRandomPositionInZone(zone);
            if (IsPositionValid(position, category.minDistanceBetweenShips))
            {
                GameObject ship = Instantiate(prefab, position, Quaternion.Euler(0, Random.Range(0, 360), 0), transform);
                spawnedShips.Add(ship);
                break;
            }
        }
    }

    private Vector3 GetRandomPositionInZone(SpawnZone zone)
    {
        Vector3 randomPosition = new Vector3(
            Random.Range(-zone.size.x / 2, zone.size.x / 2),
            Random.Range(-zone.size.y / 2, zone.size.y / 2),
            Random.Range(-zone.size.z / 2, zone.size.z / 2)
        );

        return transform.position + zone.center + randomPosition;
    }

    private bool IsPositionValid(Vector3 position, float minDistance)
    {
        foreach (var ship in spawnedShips)
        {
            if (Vector3.Distance(ship.transform.position, position) < minDistance)
            {
                return false;
            }
        }
        return true;
    }

    private void ClearShips()
    {
        foreach (var ship in spawnedShips)
        {
            if (ship != null)
            {
                Destroy(ship);
            }
        }
        spawnedShips.Clear();
    }

    private void OnDrawGizmos()
    {
        if (shipCategories == null) return;

        foreach (var category in shipCategories)
        {
            Gizmos.color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
            foreach (var zone in category.spawnZones)
            {
                Gizmos.matrix = Matrix4x4.TRS(transform.position + zone.center, Quaternion.identity, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, zone.size);
            }
        }
    }
}