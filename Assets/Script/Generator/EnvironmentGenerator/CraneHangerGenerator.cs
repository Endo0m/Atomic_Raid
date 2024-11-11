using UnityEngine;

public class CraneHangerGenerator : MonoBehaviour
{
    [Header("Префаб висящего элемента")]
    [SerializeField] private GameObject hangerPrefab;

    [Header("Точка подвеса")]
    [SerializeField] private Transform hangPoint;

    [Header("Настройки вращения")]
    [SerializeField] private Vector2 rotationRangeY = new Vector2(0f, 360f);

    [Header("Вероятность появления висящего элемента")]
    [Range(0f, 1f)]
    [SerializeField] private float hangerProbability = 0.5f;

    private GameObject spawnedHanger;

    private void Start()
    {
        GenerateHanger();
    }

    public void GenerateHanger()
    {
        if (spawnedHanger != null)
        {
            Destroy(spawnedHanger);
        }

        if (Random.value < hangerProbability)
        {
            SpawnHanger();
        }
    }

    private void SpawnHanger()
    {
        if (hangerPrefab == null || hangPoint == null) return;

        spawnedHanger = Instantiate(hangerPrefab, hangPoint.position, Quaternion.identity);
        spawnedHanger.transform.SetParent(transform, true);

        // Случайное вращение висящего элемента по Y
        float rotationY = Random.Range(rotationRangeY.x, rotationRangeY.y);
        spawnedHanger.transform.localRotation = Quaternion.Euler(0f, rotationY, 0f);
    }

    // Метод для вызова из редактора Unity
    [ContextMenu("Generate Hanger")]
    private void GenerateHangerEditor()
    {
        GenerateHanger();
    }

    // Отображение gizmo для точки подвеса в редакторе
    private void OnDrawGizmos()
    {
        if (hangPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(hangPoint.position, 0.2f);
        }
    }
}