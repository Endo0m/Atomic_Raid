using UnityEngine;
using Cinemachine;

public class PlayerAimController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform playerBody;
    [SerializeField] private float aimDistance = 15f;
    [SerializeField] private float maxHorizontalAngle = 60f;
    [SerializeField] private float maxVerticalAngle = 45f;
    [SerializeField] private float minVerticalAngle = -30f;
    [SerializeField] private float verticalRotationMultiplier = 0.5f;
    [SerializeField] private float smoothTime = 0.1f;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    private Vector3 currentRotation;
    private Vector3 rotationVelocity;
    private bool isAimingEnabled = true;

    private void Start()
    {
        if (virtualCamera == null)
        {
            virtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
        }

        if (virtualCamera != null)
        {
            virtualCamera.Follow = playerBody;
            virtualCamera.LookAt = playerBody;
        }
    }

    private void LateUpdate()
    {
        if (Time.timeScale == 0 || !isAimingEnabled) return;

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = aimDistance;
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);

        Vector3 aimDirection = (worldPosition - playerBody.position).normalized;

        float verticalAngle = Mathf.Clamp(Mathf.Atan2(aimDirection.y, new Vector2(aimDirection.x, aimDirection.z).magnitude) * Mathf.Rad2Deg, minVerticalAngle, maxVerticalAngle);
        float horizontalAngle = Mathf.Clamp(Mathf.Atan2(aimDirection.x, aimDirection.z) * Mathf.Rad2Deg, -maxHorizontalAngle, maxHorizontalAngle);

        Vector3 targetRotation = new Vector3(-verticalAngle * verticalRotationMultiplier, horizontalAngle, 0);
        currentRotation = Vector3.SmoothDamp(currentRotation, targetRotation, ref rotationVelocity, smoothTime);

        playerBody.rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0);
    }

    public Vector3 GetAimDirection()
    {
        return Quaternion.Euler(currentRotation) * Vector3.forward;
    }

    public Vector3 GetAimPoint()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = aimDistance;
        return mainCamera.ScreenToWorldPoint(mousePosition);
    }

    public void ResetState()
    {
        currentRotation = Vector3.zero;
        rotationVelocity = Vector3.zero;
    }

    public void SetAimingEnabled(bool enabled)
    {
        isAimingEnabled = enabled;
        if (!enabled)
        {
            // Фиксируем камеру в центральном положении
            currentRotation = Vector3.zero;
            playerBody.rotation = Quaternion.identity;
            if (virtualCamera != null)
            {
                virtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset = new Vector3(0, 5, -10); // Примерное значение, настройте по необходимости
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (playerBody == null) return;

        Gizmos.color = Color.red;
        Vector3 aimPoint = playerBody.position + playerBody.forward * aimDistance;
        Gizmos.DrawSphere(aimPoint, 0.5f);

        Gizmos.color = Color.blue;
        Vector3 leftBoundary = Quaternion.Euler(0, -maxHorizontalAngle, 0) * playerBody.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, maxHorizontalAngle, 0) * playerBody.forward;
        Gizmos.DrawRay(playerBody.position, leftBoundary * aimDistance);
        Gizmos.DrawRay(playerBody.position, rightBoundary * aimDistance);

        Gizmos.color = Color.green;
        Vector3 upBoundary = Quaternion.Euler(-maxVerticalAngle, 0, 0) * playerBody.forward;
        Vector3 downBoundary = Quaternion.Euler(minVerticalAngle, 0, 0) * playerBody.forward;
        Gizmos.DrawRay(playerBody.position, upBoundary * aimDistance);
        Gizmos.DrawRay(playerBody.position, downBoundary * aimDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(playerBody.position, GetAimDirection() * aimDistance);
    }
}