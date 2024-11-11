using UnityEngine;

public class LookAtPlayerBehavior : MonoBehaviour, ILookAtPlayer
{
    public float rotationSpeed = 5f;
    public Vector3 rotationOffset = new Vector3(0, 180, 0); // ��������� �������� ��������

    public void LookAtPlayer(Transform playerTransform)
    {
        if (playerTransform != null)
        {
            Vector3 directionToPlayer = playerTransform.position - transform.position;
            directionToPlayer.y = 0; // ���������� ������������ ��������

            // ������ ������������ ������ � ������ � ������ ��������
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer) * Quaternion.Euler(rotationOffset);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    // ��������� ����� ��� ������������ ����������� ������� � ���������
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 forward = transform.rotation * Quaternion.Euler(-rotationOffset) * Vector3.forward;
        Gizmos.DrawRay(transform.position, forward * 3f);
    }
}