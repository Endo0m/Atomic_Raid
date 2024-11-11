using UnityEngine;

public class CraneHangerGenerator : MonoBehaviour
{
    [Header("������ �������� ��������")]
    [SerializeField] private GameObject hangerPrefab;

    [Header("����� �������")]
    [SerializeField] private Transform hangPoint;

    [Header("��������� ��������")]
    [SerializeField] private Vector2 rotationRangeY = new Vector2(0f, 360f);

    [Header("����������� ��������� �������� ��������")]
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

        // ��������� �������� �������� �������� �� Y
        float rotationY = Random.Range(rotationRangeY.x, rotationRangeY.y);
        spawnedHanger.transform.localRotation = Quaternion.Euler(0f, rotationY, 0f);
    }

    // ����� ��� ������ �� ��������� Unity
    [ContextMenu("Generate Hanger")]
    private void GenerateHangerEditor()
    {
        GenerateHanger();
    }

    // ����������� gizmo ��� ����� ������� � ���������
    private void OnDrawGizmos()
    {
        if (hangPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(hangPoint.position, 0.2f);
        }
    }
}