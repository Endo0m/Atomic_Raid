using UnityEngine;

public class FlyingScoutEnemy : Enemy, IEnemy
{
    private Camera mainCamera;
    private float offScreenOffset = 30.0f;
    private float moveSpeed = 50f;
    private float sideMovementSpeed = 3f;
    private float maxSideMovement = 4f;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private bool moveHorizontally;
    private EnemyCounter enemyCounter;

    [SerializeField]
    private LayerMask obstacleLayerMask; // Слои, которые считаются препятствиями
    [SerializeField]
    private float detectionRadius = 1.5f; // Радиус обнаружения препятствий

    protected override void Start()
    {
        base.Start();
        moveSpeed = ConfigData.Instance.moveSpeedScout;
        mainCamera = Camera.main;
        enemyCounter = EnemyCounter.Instance;
        transform.rotation = Quaternion.Euler(0, 200, 0);
        startPosition = transform.position;
        moveHorizontally = Random.value > 0.5f;
        SetNewTargetPosition();
    }

    protected override void Update()
    {
        base.Update();
        // Основное движение вперед
        transform.Translate(Vector3.back * moveSpeed * Time.deltaTime, Space.World);

        // Боковое движение с учетом препятствий
        Vector3 sideMovement = CalculateSideMovement();
        ApplySideMovement(sideMovement);

        CheckAndDestroy();
    }

    private Vector3 CalculateSideMovement()
    {
        Vector3 sideMovement = moveHorizontally
            ? Vector3.right * sideMovementSpeed * Time.deltaTime
            : Vector3.up * sideMovementSpeed * Time.deltaTime;

        // Проверка на препятствия
        if (DetectObstacle(sideMovement))
        {
            sideMovementSpeed = -sideMovementSpeed; // Меняем направление
            sideMovement = -sideMovement; // Инвертируем движение
        }

        return sideMovement;
    }

    private void ApplySideMovement(Vector3 sideMovement)
    {
        Vector3 potentialPosition = transform.position + sideMovement;

        if (moveHorizontally)
        {
            float distanceFromStart = Mathf.Abs(potentialPosition.x - startPosition.x);
            if (distanceFromStart > maxSideMovement)
            {
                SetNewTargetPosition();
                return;
            }
        }
        else
        {
            float distanceFromStart = Mathf.Abs(potentialPosition.y - startPosition.y);
            if (distanceFromStart > maxSideMovement)
            {
                SetNewTargetPosition();
                return;
            }
        }

        transform.Translate(sideMovement, Space.World);
    }

    private bool DetectObstacle(Vector3 movement)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius, obstacleLayerMask);

        if (hitColliders.Length > 0)
        {
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider is MeshCollider)
                    if (!(hitCollider as MeshCollider).convex)
                        continue;
                
                Vector3 directionToObstacle = hitCollider.ClosestPoint(transform.position) - transform.position;
                float angleToObstacle = Vector3.Angle(movement, directionToObstacle);

                // Если препятствие находится в направлении движения (угол меньше 90 градусов)
                if (angleToObstacle < 90f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void SetNewTargetPosition()
    {
        if (moveHorizontally)
        {
            float targetX = startPosition.x + (Random.value > 0.5f ? maxSideMovement : -maxSideMovement);
            targetPosition = new Vector3(targetX, transform.position.y, transform.position.z);
        }
        else
        {
            float targetY = startPosition.y + (Random.value > 0.5f ? maxSideMovement : -maxSideMovement);
            targetPosition = new Vector3(transform.position.x, targetY, transform.position.z);
        }
        sideMovementSpeed = -sideMovementSpeed;
    }

    protected override void Die()
    {
        base.Die();
    }

    private void CheckAndDestroy()
    {
        if (IsOutOfCameraView(gameObject))
        {
            killedByPlayer = false;
            EnemyCounter.Instance.IncrementMissedEnemies();
            Die();
        }
    }

    private bool IsOutOfCameraView(GameObject obj)
    {
        return CameraUtility.IsOutOfCameraView(obj, mainCamera, offScreenOffset);
    }

    // Метод для отображения сферы обнаружения в редакторе Unity
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}