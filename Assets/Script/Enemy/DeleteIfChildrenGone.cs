using UnityEngine;

public class DeleteIfChildrenGone : MonoBehaviour
{
    private int initialChildCount;

    private void Start()
    {
        // ���������� ��������� ���������� �������� ��������
        initialChildCount = transform.childCount;
    }

    private void Update()
    {
        // ���������, �������� �� �������� �������
        if (transform.childCount == 0 && initialChildCount > 0)
        {
            // ���� ��� �������� ������� �������, ������� ���� ������
            Destroy(gameObject);
        }
    }
}