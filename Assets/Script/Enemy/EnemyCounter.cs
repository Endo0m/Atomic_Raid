using UnityEngine;
using System;

public class EnemyCounter : MonoBehaviour
{
    public static EnemyCounter Instance { get; private set; }

    private int missedEnemiesCount = 0;

    public event Action<int> OnEnemyMissed;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void IncrementMissedEnemies()
    {
        missedEnemiesCount++;
        OnEnemyMissed?.Invoke(missedEnemiesCount);
    }

    public void ResetCounter()
    {
        missedEnemiesCount = 0;
    }
}