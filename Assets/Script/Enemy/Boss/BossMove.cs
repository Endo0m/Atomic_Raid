using System.Collections;
using UnityEngine;

public class BossMove : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private EndArea _endArea;
    [SerializeField] private float _chaosRadius = 5f;
    [SerializeField] private float _attackInterval = 2f; // Интервал между атаками

    private BossController _bossController;
    private Vector3 _targetPoint;
    private Vector3 _chaosTarget;
    private bool _reachedEndPoint = false;
    private float _lastAttackTime;

    void Start()
    {
        _bossController = GetComponent<BossController>();
        if (_bossController == null)
        {
            Debug.LogError("BossController component not found on this GameObject!");
        }
        SetNewTargetInEndArea();
        _lastAttackTime = -_attackInterval; // Позволяет атаковать сразу после старта
    }

    void Update()
    {
        Move();

        /*if (_reachedEndPoint && Time.time - _lastAttackTime >= _attackInterval)
        {
            Attack();
            _lastAttackTime = Time.time;
        }*/
    }

    private void Move()
    {
        Vector3 direction;
        if (!_reachedEndPoint)
        {
            direction = (_targetPoint - transform.position).normalized;
            if (Vector3.Distance(transform.position, _targetPoint) < 0.2f)
            {
                _reachedEndPoint = true;
                SetNewChaosTarget();
            }
        }
        else
        {
            direction = (_chaosTarget - transform.position).normalized;
            if (Vector3.Distance(transform.position, _chaosTarget) < 0.2f)
            {
                SetNewChaosTarget();
            }
        }
        transform.position += direction * _speed * Time.deltaTime;
    }

    private void SetNewTargetInEndArea()
    {
        float randomX = Random.Range(_endArea.min.x, _endArea.max.x);
        float randomY = Random.Range(_endArea.min.y, _endArea.max.y);
        float randomZ = Random.Range(_endArea.min.z, _endArea.max.z);
        _targetPoint = new Vector3(randomX, randomY, randomZ);
    }

    private void SetNewChaosTarget()
    {
        Vector3 randomOffset = Random.insideUnitSphere * _chaosRadius;
        _chaosTarget = _targetPoint + randomOffset;
    }

    private void Attack()
    {
        if (_bossController != null && _bossController.IsPlayerInRange())
        {
            // Вызываем метод стрельбы из BossController
            for (int i = 0; i < _bossController.ShootPoints.Length; i++)
            {
                _bossController.ShootBullet(i);
            }
        }
    }
}