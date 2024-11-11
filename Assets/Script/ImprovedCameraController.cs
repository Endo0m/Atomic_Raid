using UnityEngine;
using Cinemachine;

public class ImprovedCameraController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Transform player;
    [SerializeField] private Transform aimTarget;
    [SerializeField] private float minX = -6.5f, maxX = 6.5f, minY = 3f, maxY = 7f, minZ = -10f, maxZ = 2f;
    [SerializeField] private float verticalOffset = 4f;
    [SerializeField] private float followOffset = 8f;
    [SerializeField] private float aimSmoothTime = 0.1f;
    [SerializeField] private float aimDistance = 10f;

    private CinemachineFramingTransposer framingTransposer;
    private Vector3 currentVelocity;
    private Vector3 targetPosition;

    private void Start()
    {
        if (virtualCamera == null)
        {
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
        }

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        if (aimTarget == null)
        {
            aimTarget = new GameObject("AimTarget").transform;
        }

        framingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        if (framingTransposer != null)
        {
            framingTransposer.m_XDamping = 0;
            framingTransposer.m_YDamping = 0;
            framingTransposer.m_ZDamping = 0;
        }

        virtualCamera.Follow = player;
        virtualCamera.LookAt = aimTarget;
    }

    private void LateUpdate()
    {
        if (player == null) return;

        UpdateAimTarget();
        UpdateCameraPosition();
    }

    private void UpdateAimTarget()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, player.position);

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 targetPoint = ray.GetPoint(distance);
            targetPoint.y = player.position.y; // Держим цель на уровне игрока
            aimTarget.position = Vector3.SmoothDamp(aimTarget.position, targetPoint, ref currentVelocity, aimSmoothTime);
        }
    }

    private void UpdateCameraPosition()
    {
        targetPosition = player.position + Vector3.up * verticalOffset - player.forward * followOffset;
        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
        targetPosition.z = Mathf.Clamp(targetPosition.z, minZ, maxZ);

        if (framingTransposer != null)
        {
            framingTransposer.m_TrackedObjectOffset = targetPosition - player.position;
        }
    }

    public Vector3 GetAimPoint()
    {
        return aimTarget.position;
    }

    private void OnDrawGizmos()
    {
        if (player != null && aimTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(player.position, aimTarget.position);
            Gizmos.DrawSphere(aimTarget.position, 0.5f);
        }
    }
}