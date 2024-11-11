using UnityEngine;
using System.Collections.Generic;

public class NatureGenerator : MonoBehaviour
{
    [System.Serializable]
    public class NatureElement
    {
        public GameObject prefab;
        public int minCount;
        public int maxCount;
        public float minDistance = 1f;
        public List<SpawnArea> spawnAreas;
    }

    public NatureElement rocks;
    public NatureElement trees;
    public NatureElement grass;

    private List<GameObject> spawnedObjects = new List<GameObject>();

    public void GenerateNatureForSegment(GameObject segment)
    {
        ClearNature();
        GenerateNature(segment);
    }

    void ClearNature()
    {
        foreach (var obj in spawnedObjects)
        {
            if (obj != null)
            {
                DestroyImmediate(obj);
            }
        }
        spawnedObjects.Clear();
    }

    void GenerateNature(GameObject segment)
    {
        SpawnElements(rocks, segment);
        SpawnElements(trees, segment);
        SpawnElements(grass, segment);
    }

    void SpawnElements(NatureElement element, GameObject segment)
    {
        int count = Random.Range(element.minCount, element.maxCount + 1);
        foreach (var area in element.spawnAreas)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 localPosition = GetRandomPosition(area, element.minDistance);
                if (localPosition != Vector3.zero)
                {
                    GameObject obj = Instantiate(element.prefab, segment.transform);
                    obj.transform.localPosition = localPosition;
                    obj.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                    spawnedObjects.Add(obj);
                }
            }
        }
    }

    Vector3 GetRandomPosition(SpawnArea area, float minDistance)
    {
        for (int attempts = 0; attempts < 10; attempts++)
        {
            Vector3 position = new Vector3(
                Random.Range(area.min.x, area.max.x),
                Random.Range(area.min.y, area.max.y),
                Random.Range(area.min.z, area.max.z)
            );
            if (!IsOverlapping(position, minDistance))
            {
                return position;
            }
        }
        return Vector3.zero;
    }

    bool IsOverlapping(Vector3 position, float minDistance)
    {
        foreach (var obj in spawnedObjects)
        {
            if (Vector3.Distance(obj.transform.localPosition, position) < minDistance)
            {
                return true;
            }
        }
        return false;
    }

    void OnDrawGizmos()
    {
        DrawSpawnAreas(rocks.spawnAreas, Color.red);
        DrawSpawnAreas(trees.spawnAreas, Color.green);
        DrawSpawnAreas(grass.spawnAreas, Color.blue);
    }

    void DrawSpawnAreas(List<SpawnArea> areas, Color color)
    {
        foreach (var area in areas)
        {
            Gizmos.color = new Color(color.r, color.g, color.b, 0.3f);
            Gizmos.DrawCube(transform.position + (area.min + area.max) / 2, area.max - area.min);
            Gizmos.color = color;
            Gizmos.DrawWireCube(transform.position + (area.min + area.max) / 2, area.max - area.min);
        }
    }
}

[System.Serializable]
public class SpawnArea
{
    public Vector3 min;
    public Vector3 max;
}