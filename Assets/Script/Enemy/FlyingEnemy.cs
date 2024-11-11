using UnityEngine;
using System.Collections;

public class FlyingEnemy : Enemy, IMovable, IShootable, IEnemy
{
    private float moveSpeed;
    private float moveSpeedChaos;
    private float shootInterval;
    private float shootRange;
    private float chaosRadius;
    private float bulletSpeed;
    private float bulletLifetime;

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private EndArea endArea;
    private Vector3 targetPoint;

    [SerializeField] private BulletPoolManager bulletPoolManager;
    private Vector3 lookDirection;
    private Vector3 aimDirection;
    private bool reachedEndPoint = false;
    private Vector3 chaosTarget;
    private Camera mainCamera;
    private float offScreenOffset = 30.0f;
    [SerializeField] private float lerpSpeed = 0.5f; // Скорость интерполяции для плавного движения
    [SerializeField] private float minDistanceToTarget = 0.5f; // Минимальное расстояние для смены цели
    [SerializeField] private float chaosTargetChangeDelay = 0.5f; // Задержка перед сменой цели
    protected override void Start()
    {
        base.Start();

        mainCamera = Camera.main;
        bulletPoolManager = FindObjectOfType<BulletPoolManager>();
        moveSpeed = ConfigData.Instance.enemyMoveSpeed;
        moveSpeedChaos = ConfigData.Instance.enemyMoveSpeedChaos;
        shootInterval = ConfigData.Instance.enemyShootInterval;
        shootRange = ConfigData.Instance.enemyShootRange;
        chaosRadius = ConfigData.Instance.enemyChaosRadius;
        bulletSpeed = ConfigData.Instance.enemyBulletSpeed;
        bulletLifetime = ConfigData.Instance.enemyBulletLifetime;

        StartCoroutine(ShootRoutine());
        SetNewChaosTarget();
        SetNewTargetInEndArea();
        if (lookAtPlayerBehavior == null)
        {
            lookAtPlayerBehavior = GetComponent<LookAtPlayerBehavior>();
            if (lookAtPlayerBehavior == null)
            {
                lookAtPlayerBehavior = gameObject.AddComponent<LookAtPlayerBehavior>();
            }
        }
    }


    protected override void Update()
    {
        base.Update();
        if (player != null)
        {
            lookDirection = (player.position - transform.position).normalized;
            if (lookAtPlayerBehavior != null)
            {
                lookAtPlayerBehavior.LookAtPlayer(player);
            }
            Move();
            UpdateAimDirection();
        }
        CheckAndDestroy();
    }

    public void Move()
    {
        if (!reachedEndPoint)
        {
            MoveToEndPoint();
        }
        else
        {
            MoveChaotically();
        }
    }

    private void MoveToEndPoint()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPoint, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPoint) < minDistanceToTarget)
        {
            reachedEndPoint = true;
            StartCoroutine(SetNewChaosTargetWithDelay());
        }
    }

    private void MoveChaotically()
    {
        transform.position = Vector3.Lerp(transform.position, chaosTarget, lerpSpeed * moveSpeedChaos * Time.deltaTime);

        if (Vector3.Distance(transform.position, chaosTarget) < minDistanceToTarget)
        {
            StartCoroutine(SetNewChaosTargetWithDelay());
        }
    }

    private IEnumerator SetNewChaosTargetWithDelay()
    {
        yield return new WaitForSeconds(chaosTargetChangeDelay);
        SetNewChaosTarget();
    }

    private void SetNewChaosTarget()
    {
        Vector3 randomPoint = new Vector3(
            Random.Range(endArea.min.x, endArea.max.x),
            Random.Range(endArea.min.y, endArea.max.y),
            Random.Range(endArea.min.z, endArea.max.z)
        );
        chaosTarget = randomPoint;
    }


    private void SetNewTargetInEndArea()
    {
        float randomX = Random.Range(endArea.min.x, endArea.max.x);
        float randomY = Random.Range(endArea.min.y, endArea.max.y);
        float randomZ = Random.Range(endArea.min.z, endArea.max.z);
        targetPoint = new Vector3(randomX, randomY, randomZ);
    }

    private void UpdateAimDirection()
    {
        if (player != null && bulletSpawnPoint != null)
        {
            aimDirection = (player.position - bulletSpawnPoint.position).normalized;
        }
    }
 

 

    public void Shoot()
    {
        if (bulletSpawnPoint != null && player != null)
        {
            Bullet bullet = bulletPoolManager.GetEnemyBullet();
            bullet.transform.position = bulletSpawnPoint.position;
            bullet.Initialize(aimDirection, bulletSpeed, bulletLifetime);
        }
    }

    private IEnumerator ShootRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(shootInterval);
            if (player != null && Vector3.Distance(transform.position, player.position) <= shootRange)
            {
                Shoot();
            }
        }
    }   
    private bool IsOutOfCameraView(GameObject obj)
    {
        return CameraUtility.IsOutOfCameraView(obj, mainCamera, offScreenOffset);
    }
    protected override void Die()
    {
        base.Die(); // Вызываем базовый метод Die, который теперь уничтожает объект
    }

    private void CheckAndDestroy()
    {
        if (IsOutOfCameraView(gameObject))
        {
            Destroy(gameObject); // Заменяем деактивацию на уничтожение
        }
    }
    private void OnDrawGizmosSelected()
    {
        // Draw end area
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube((endArea.min + endArea.max) / 2, endArea.max - endArea.min);

        // Draw current target point
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(targetPoint, 0.5f);

        // Draw shoot range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, shootRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, lookDirection * 3f);

        // Draw aim direction
        Gizmos.color = Color.green;
        if (bulletSpawnPoint != null)
        {
            Gizmos.DrawRay(bulletSpawnPoint.position, aimDirection * 5f);
        }
    }
}
[System.Serializable]
public class EndArea
{
    public Vector3 min;
    public Vector3 max;
}