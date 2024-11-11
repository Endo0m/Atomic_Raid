using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera _virtualCamera;
    [SerializeField] private float minX = -6.5f, maxX = 6.5f, minY = 3f, maxY = 7f, minZ = -10f, maxZ = 2f;
    [SerializeField] private float smoothSpeed = 0f;
    [SerializeField] private float verticalOffset = 4f;
    [SerializeField] private float followOffset = 8f;
    [SerializeField] private float aimSmoothTime = 0f;
    [SerializeField] private float aimDistance = 1f;
    [SerializeField] private float sideOffset = 0f;

    private Transform _playerTransform;
    private Vector3 _cameraPosition;
    private Vector3 _aimDirection;
    private Vector2 _currentAimAngles;
    private Vector2 _aimVelocity;
    private bool isGameOver = false;

    private void Start()
    {
        if (_virtualCamera == null)
        {
            _virtualCamera = GetComponent<CinemachineVirtualCamera>();
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
        }

        if (_virtualCamera != null)
        {
            _virtualCamera.Follow = null;
            _virtualCamera.LookAt = null;
            UpdateCameraPosition();
        }
    }

    private void LateUpdate()
    {
        if (isGameOver) return;

        if (_playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
            }
            else
            {
                return;
            }
        }

        UpdateCameraPosition();
        UpdateAim();
    }

    private void UpdateCameraPosition()
    {
        if (_virtualCamera != null && _playerTransform != null)
        {
            // Рассчитываем целевую позицию камеры
            Vector3 targetPosition = _playerTransform.position;

            // Добавляем боковое смещение
            Vector3 rightOffset = _playerTransform.right * sideOffset;
            targetPosition += rightOffset;

            // Ограничиваем позицию камеры
            targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
            targetPosition.z = Mathf.Clamp(targetPosition.z, minZ, maxZ);

            // Плавно перемещаем камеру к целевой позиции
            _cameraPosition = Vector3.Lerp(_cameraPosition, targetPosition, smoothSpeed * Time.unscaledDeltaTime);

            // Устанавливаем позицию виртуальной камеры
            _virtualCamera.transform.position = _cameraPosition + new Vector3(0, verticalOffset, -followOffset);
        }
    }

    private void UpdateAim()
    {
        if (_virtualCamera == null || _playerTransform == null)
        {
            return;
        }

        float aimHorizontal = Input.GetAxis("Mouse X");
        float aimVertical = Input.GetAxis("Mouse Y");

        _currentAimAngles.x = Mathf.SmoothDamp(_currentAimAngles.x, aimHorizontal, ref _aimVelocity.x, aimSmoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
        _currentAimAngles.y = Mathf.SmoothDamp(_currentAimAngles.y, aimVertical, ref _aimVelocity.y, aimSmoothTime, Mathf.Infinity, Time.unscaledDeltaTime);

        _currentAimAngles.x = Mathf.Clamp(_currentAimAngles.x, -89f, 89f);
        _currentAimAngles.y = Mathf.Clamp(_currentAimAngles.y, -89f, 89f);

        _aimDirection = Quaternion.Euler(-_currentAimAngles.y, _currentAimAngles.x, 0) * Vector3.forward;

        Vector3 aimPoint = _playerTransform.position + _aimDirection * aimDistance;
        _virtualCamera.LookAt = null;
        _virtualCamera.transform.LookAt(aimPoint);
    }

    public void SetGameOver(bool gameOver)
    {
        isGameOver = gameOver;
    }

    public void ResetState()
    {
        _cameraPosition = Vector3.zero;
        _aimDirection = Vector3.forward;
        _currentAimAngles = Vector2.zero;
        _aimVelocity = Vector2.zero;
        isGameOver = false;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawLine(new Vector3(minX, minY, minZ), new Vector3(maxX, minY, minZ));
        Gizmos.DrawLine(new Vector3(minX, minY, minZ), new Vector3(minX, minY, maxZ));
        Gizmos.DrawLine(new Vector3(maxX, minY, minZ), new Vector3(maxX, minY, maxZ));
        Gizmos.DrawLine(new Vector3(minX, minY, maxZ), new Vector3(maxX, minY, maxZ));

        Gizmos.DrawLine(new Vector3(minX, maxY, minZ), new Vector3(maxX, maxY, minZ));
        Gizmos.DrawLine(new Vector3(minX, maxY, minZ), new Vector3(minX, maxY, maxZ));
        Gizmos.DrawLine(new Vector3(maxX, maxY, minZ), new Vector3(maxX, maxY, maxZ));
        Gizmos.DrawLine(new Vector3(minX, maxY, maxZ), new Vector3(maxX, maxY, maxZ));

        Gizmos.DrawLine(new Vector3(minX, minY, minZ), new Vector3(minX, maxY, minZ));
        Gizmos.DrawLine(new Vector3(maxX, minY, minZ), new Vector3(maxX, maxY, minZ));
        Gizmos.DrawLine(new Vector3(minX, minY, maxZ), new Vector3(minX, maxY, maxZ));
        Gizmos.DrawLine(new Vector3(maxX, minY, maxZ), new Vector3(maxX, maxY, maxZ));
    }
}