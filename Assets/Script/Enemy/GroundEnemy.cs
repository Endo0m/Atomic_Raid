using System.Collections;
using UnityEngine;

public class GroundEnemy : Enemy, IEnemy
{
    public float attackRange = 2f;
    public float patrolSpeed = 1f;
    public float raycastDistance = 5f;
    public float raycastHeight = 0.1f;
    public Vector3 raycastOffset = Vector3.zero;

    [SerializeField] private bool drawGizmosAlways = true;
    [SerializeField] private LayerMask layersForObstacles;

    private Camera mainCamera;
    private float offScreenOffset = 30.0f;
    private Transform environmentParent;
    private Vector3 localPositionOnEnvironment;
    private Vector3 lastPosition;
    private Animator animator;
    private static readonly int IsMovingParam = Animator.StringToHash("IsMoving");
    private static readonly int MovementDirectionParam = Animator.StringToHash("MovementDirection");

    private bool movingRight = true;
    private bool isWaiting = false;

    protected override void Start()
    {
        base.Start();
        mainCamera = Camera.main;
        animator = GetComponent<Animator>();
        AttachToNearestEnvironment();
        lastPosition = transform.position;

        Debug.Log($"GroundEnemy initialized. Obstacle layers: {LayerMaskToString(layersForObstacles)}");

        StartCoroutine(PatrolCoroutine());
    }

    protected override void Update()
    {
        base.Update();
        if (environmentParent != null)
        {
            transform.position = environmentParent.TransformPoint(localPositionOnEnvironment);
        }
        if (!isPlayerDead && Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            Attack();
        }
        UpdateAnimation();
        CheckAndDestroy();

        // Постоянная проверка препятствий для отладки
        CheckObstacles();
    }

    private void UpdateAnimation()
    {
        Vector3 currentPosition = transform.position;
        Vector3 movement = currentPosition - lastPosition;

        bool isMoving = movement.magnitude > 0.01f;
        animator.SetBool(IsMovingParam, isMoving);

        if (isMoving)
        {
            int direction = movement.x > 0.01f ? 1 : (movement.x < -0.01f ? -1 : 0);
            animator.SetInteger(MovementDirectionParam, direction);
        }

        lastPosition = currentPosition;
    }

    private IEnumerator PatrolCoroutine()
    {
        while (true)
        {
            if (isWaiting)
            {
                animator.SetBool(IsMovingParam, false);
                yield return new WaitForSeconds(0.5f);
                isWaiting = false;
            }

            bool obstacleDetected = CheckObstacles();

            if (obstacleDetected)
            {
                isWaiting = true;
                movingRight = !movingRight;
                Debug.Log($"Obstacle detected. Changing direction to: {(movingRight ? "right" : "left")}");
            }
            else
            {
                animator.SetBool(IsMovingParam, true);
                Vector3 moveDirection = movingRight ? Vector3.right : Vector3.left;
                localPositionOnEnvironment += moveDirection * patrolSpeed * Time.deltaTime;
            }

            yield return null;
        }
    }

    private bool CheckObstacles()
    {
        Vector3 rayOrigin = transform.position + raycastOffset + Vector3.up * raycastHeight;
        Vector3 rayDirection = movingRight ? Vector3.right : Vector3.left;

        Debug.DrawRay(rayOrigin, rayDirection * raycastDistance, Color.red, 0.1f);

        // Расширенная отладочная информация
        Debug.Log($"Checking obstacles for {gameObject.name}:");
        Debug.Log($"  Position: {transform.position}, Ray Origin: {rayOrigin}");
        Debug.Log($"  Direction: {rayDirection}, Distance: {raycastDistance}");
        Debug.Log($"  Obstacle Layer Mask: {LayerMaskToString(layersForObstacles)}");

        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, raycastDistance, layersForObstacles))
        {
            Debug.Log($"  Raycast hit: {hit.collider.name} on layer {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
            return true;
        }

        Debug.Log("  No obstacle detected");
        return false;
    }

    private string LayerMaskToString(LayerMask layerMask)
    {
        var layers = new System.Collections.Generic.List<string>();
        for (int i = 0; i < 32; i++)
        {
            if ((layerMask & (1 << i)) != 0)
            {
                layers.Add(LayerMask.LayerToName(i));
            }
        }
        return string.Join(", ", layers);
    }


    private void AttachToNearestEnvironment()
    {
        GameObject[] environmentObjects = GameObject.FindGameObjectsWithTag("Environment");
        float closestDistance = float.MaxValue;
        Transform closestEnvironment = null;
        foreach (GameObject envObject in environmentObjects)
        {
            float distance = Vector3.Distance(transform.position, envObject.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnvironment = envObject.transform;
            }
        }
        if (closestEnvironment != null)
        {
            environmentParent = closestEnvironment;
            localPositionOnEnvironment = environmentParent.InverseTransformPoint(transform.position);
            transform.SetParent(environmentParent, true);
        }
        else
        {
            Debug.LogWarning("No environment object found for GroundEnemy to attach to!");
        }
    }

    private void Attack()
    {
        // Реализация атаки
    }
    private void CheckAndDestroy()
    {
        if (IsOutOfCameraView(gameObject))
        {
            killedByPlayer = false;
            EnemyCounter.Instance.IncrementMissedEnemies();
            Die(); // Вызываем Die вместо деактивации
        }
    }

    private bool IsOutOfCameraView(GameObject obj)
    {
        return CameraUtility.IsOutOfCameraView(obj, mainCamera, offScreenOffset);
    }
    private void OnDrawGizmos()
    {
        if (!drawGizmosAlways && !Application.isPlaying) return;

        Vector3 rayOrigin = transform.position + raycastOffset + Vector3.up * raycastHeight;
        Vector3 rayDirection = movingRight ? Vector3.right : Vector3.left;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(rayOrigin, rayDirection * raycastDistance);
    }

}