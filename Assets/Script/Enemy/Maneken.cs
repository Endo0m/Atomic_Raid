using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Maneken : Enemy, IEnemy
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

    protected override void Start()
    {
        base.Start();
        moveSpeed = ConfigData.Instance.moveSpeedScout;
        mainCamera = Camera.main;
        enemyCounter = EnemyCounter.Instance;
        transform.rotation = Quaternion.Euler(0, 200, 0);
        startPosition = transform.position;
        moveHorizontally = Random.value > 0.5f;
      
    }

    protected override void Update()
    {
        base.Update();
        // Основное движение вперед
       // transform.Translate(Vector3.back * moveSpeed * Time.deltaTime, Space.World);
        CheckAndDestroy();
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


}
