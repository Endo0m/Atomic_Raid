using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System.Collections;

public class TrashGenerator : MonoBehaviour
{
    [System.Serializable]
    public class TrashCategory
    {
        public string name;
        public GameObject[] prefabs;
        public SpawnZone[] spawnZones;
        [Range(1, 20)]
        public int minCount = 1;
        [Range(1, 20)]
        public int maxCount = 5;
        public float minDistanceBetweenObjects = 1f;
        public string attachmentPointTag = "AttachmentPoint";
    }

    [System.Serializable]
    public class SpawnZone
    {
        public Vector3 center;
        public Vector3 size;
    }
    public TrashCategory[] trashCategories;
    public LayerMask collisionCheckLayers;
    public LayerMask groundLayer;
    private List<GameObject> spawnedObjects = new List<GameObject>();

    private void Start()
    {
        GenerateTrash();
    }
    private void Update()
    {
        CheckAndDestroyDetachedObjects();
    }
    public void GenerateTrash()
    {
        ClearTrash();

        foreach (var category in trashCategories)
        {
            int itemsToSpawn = Random.Range(category.minCount, category.maxCount + 1);

            for (int i = 0; i < itemsToSpawn; i++)
            {
                SpawnTrashItem(category);
            }
        }
    }
    private void SpawnTrashItem(TrashCategory category)
    {
        GameObject prefab = category.prefabs[Random.Range(0, category.prefabs.Length)];
        SpawnZone zone = category.spawnZones[Random.Range(0, category.spawnZones.Length)];

        for (int attempts = 0; attempts < 50; attempts++)
        {
            Vector3 position = GetRandomPositionInZone(zone);
            if (IsPositionValid(position, category.minDistanceBetweenObjects))
            {
                // Найдем ближайшую точку на поверхности
                RaycastHit hit;
                if (Physics.Raycast(position + Vector3.up * 10, Vector3.down, out hit, 20f, groundLayer))
                {
                    position = hit.point;

                    // Найдем точку прикрепления на объекте
                    Transform attachmentPoint = prefab.transform.Find(category.attachmentPointTag);
                    if (attachmentPoint != null)
                    {
                        // Корректируем позицию с учетом точки прикрепления
                        position -= attachmentPoint.localPosition;
                    }

                    // Сохраняем оригинальную ротацию префаба
                    Quaternion originalRotation = prefab.transform.rotation;

                    // Генерируем случайный угол поворота вокруг оси Y
                    float randomYRotation = Random.Range(0f, 360f);

                    // Создаем новую ротацию, сохраняя оригинальные X и Z, но изменяя Y
                    Quaternion finalRotation = Quaternion.Euler(
                        originalRotation.eulerAngles.x,
                        randomYRotation,
                        originalRotation.eulerAngles.z
                    );

                    GameObject trashItem = Instantiate(prefab, position, finalRotation, transform);
                    trashItem.transform.localScale = prefab.transform.localScale;
                    trashItem.AddComponent<AttachmentChecker>();

                    // Настройка Rigidbody
                    Rigidbody rb = trashItem.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.isKinematic = true;
                        rb.constraints = RigidbodyConstraints.FreezeAll;
                    }

                    spawnedObjects.Add(trashItem);
                    break;
                }
            }
        }
    }
    private void CheckAndDestroyDetachedObjects()
    {
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            GameObject obj = spawnedObjects[i];
            if (obj != null && obj.transform.parent != transform)
            {
                StartCoroutine(DestroyAfterDelay(obj, 5f));
                spawnedObjects.RemoveAt(i);
            }
        }
    }

    private IEnumerator DestroyAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null)
        {
            Destroy(obj);
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
        Collider[] colliders = Physics.OverlapSphere(position, minDistance / 2, collisionCheckLayers);
        if (colliders.Length > 0)
        {
            return false;
        }

        foreach (var obj in spawnedObjects)
        {
            if (Vector3.Distance(obj.transform.position, position) < minDistance)
            {
                return false;
            }
        }

        return true;
    }

    private void ClearTrash()
    {
        foreach (var obj in spawnedObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        spawnedObjects.Clear();
    }

    private void OnDrawGizmos()
    {
        if (trashCategories == null) return;

        foreach (var category in trashCategories)
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