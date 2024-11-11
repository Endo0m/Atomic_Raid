using UnityEngine;

public abstract class Enemy : MonoBehaviour, IDamageable, IEnemy
{
    protected Transform player;
    protected bool isPlayerDead = false;
    protected ILookAtPlayer lookAtPlayerBehavior;
    [SerializeField] protected int scoreValue;
    protected GameUIManager _gameUIManager;
    protected EnemyLife enemyLife;
    protected bool killedByPlayer = true;
    public int ScoreValue => scoreValue;
    private UnityEngine.Object explosion;
    
    protected virtual void Start()
    {
        
        lookAtPlayerBehavior = GetComponent<ILookAtPlayer>();
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        explosion = Resources.Load("DieEnemy");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        _gameUIManager = FindObjectOfType<GameUIManager>();
        enemyLife = GetComponent<EnemyLife>();
        if (enemyLife == null)
        {
            enemyLife = gameObject.AddComponent<EnemyLife>();
        }
        enemyLife.OnDeath += Die;
    }

    protected virtual void Update()
    {
        if (player == null && !isPlayerDead)
        {
            OnPlayerDeath();
        }
    }


    protected virtual void Die()
    {
        if (killedByPlayer && _gameUIManager != null)
        {
            ScoreManager.Instance.AddScore(scoreValue);
        }
        enemyLife.OnDeath -= Die;
        Destroy(gameObject); // Заменяем деактивацию на уничтожение
    }
    public virtual void TakeDamage(int damage)
    {
        
        enemyLife.TakeDamage(damage);
    }
    protected virtual void OnPlayerDeath()
    {
        isPlayerDead = true;
    }

    void IEnemy.Die()
    {
        Die();
    }

    private void OnDestroy()
    {
        if (enemyLife != null)
        {
            enemyLife.OnDeath -= Die;
        }
    }
}