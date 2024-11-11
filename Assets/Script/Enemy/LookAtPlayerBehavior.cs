using UnityEngine;

public class LookAtPlayerBehavior : MonoBehaviour, ILookAtPlayer
{
    public float rotationSpeed = 5f;
    public Vector3 rotationOffset = new Vector3(0, 180, 0); // Добавляем смещение поворота

    public void LookAtPlayer(Transform playerTransform)
    {
        if (playerTransform != null)
        {
            Vector3 directionToPlayer = playerTransform.position - transform.position;
            directionToPlayer.y = 0; // Игнорируем вертикальное вращение

            // Плавно поворачиваем объект к игроку с учетом смещения
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer) * Quaternion.Euler(rotationOffset);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    // Добавляем метод для визуализации направления взгляда в редакторе
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 forward = transform.rotation * Quaternion.Euler(-rotationOffset) * Vector3.forward;
        Gizmos.DrawRay(transform.position, forward * 3f);
    }
}