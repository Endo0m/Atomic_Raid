using UnityEngine;

public class BulletPoolManager : MonoBehaviour
{
    public static BulletPoolManager instance = null;
    
    [SerializeField] private GameObject playerBulletPrefab;
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private GameObject plasmaBulletPrefab;
    [SerializeField] private GameObject firecannonBulletPrefab;
    [SerializeField] private GameObject chainlightningBulletPrefab;
    [SerializeField] private GameObject[] bossBulletPrefabs; // Массив из 5 префабов пуль босса
    [SerializeField] private int preloadCount = 20;
    [SerializeField] private int specialVFXPreloadCount = 100;

    private ObjectPool<Bullet> playerBulletPool;
    private ObjectPool<Bullet> enemyBulletPool;
    private ObjectPool<Bullet> plasmaBulletPool;
    private ObjectPool<Bullet> firecannonBulletPool;
    private ObjectPool<Bullet> chainlightningBulletPool;
    private ObjectPool<Bullet>[] bossBulletPools; // Массив пулов для пуль босса
    private Transform bulletPoolParent;

    private void Awake()
    {
        instance = this;
        
        bulletPoolParent = new GameObject("PoolBullet").transform;

        playerBulletPool = CreatePool(playerBulletPrefab);
        enemyBulletPool = CreatePool(enemyBulletPrefab);
        plasmaBulletPool = CreatePool(plasmaBulletPrefab, specialVFXPreloadCount);
        firecannonBulletPool = CreatePool(firecannonBulletPrefab, specialVFXPreloadCount);
        chainlightningBulletPool = CreatePool(chainlightningBulletPrefab, specialVFXPreloadCount);
        
        bossBulletPools = new ObjectPool<Bullet>[bossBulletPrefabs.Length];
        for (int i = 0; i < bossBulletPrefabs.Length; i++)
        {
            bossBulletPools[i] = CreatePool(bossBulletPrefabs[i]);
        }
    }

    private ObjectPool<Bullet> CreatePool(GameObject prefab, int loadCount = 0)
    {
        return new ObjectPool<Bullet>(
            () => CreateBullet(prefab),
            OnGetBullet,
            OnReturnBullet,
            loadCount == 0 ? preloadCount : loadCount
        );
    }


    private Bullet CreateBullet(GameObject prefab)
    {
        GameObject bulletObj = Instantiate(prefab, bulletPoolParent);
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet == null)
        {
            bullet = bulletObj.AddComponent<Bullet>();
        }
        return bullet;
    }
 
    private void OnGetBullet(Bullet bullet)
    {
        if (bullet != null && bullet.gameObject != null)
        {
            bullet.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Attempted to activate a null bullet.");
        }
    }

    private void OnReturnBullet(Bullet bullet)
    {
        bullet.gameObject.SetActive(false);
    }

    public Bullet GetPlayerBullet()
    {
        return playerBulletPool.Get();
    }

    public Bullet GetEnemyBullet()
    {
        Bullet bullet = enemyBulletPool.Get();
        if (bullet == null || bullet.gameObject == null)
        {
            Debug.LogWarning("Retrieved a null bullet from the pool. Creating a new one.");
            bullet = CreateBullet(enemyBulletPrefab);
        }
        return bullet;
    }
    
    public Bullet GetBossBullet(int type)
    {
        if (type >= 0 && type < bossBulletPools.Length)
        {
            return bossBulletPools[type].Get();
        }
        return null;
    }

    public Bullet GetPlasmaBullet()
    {
        return plasmaBulletPool.Get();
    }

    public Bullet GetFirecannonBullet()
    {
        return firecannonBulletPool.Get();
    }

    public Bullet GetChainlightningBullet()
    {
        return chainlightningBulletPool.Get();
    }

    public void ReturnBullet(Bullet bullet)
    {
        if (bullet.gameObject.name.StartsWith(playerBulletPrefab.name))
        {
            playerBulletPool.Return(bullet);
        }
        else if (bullet.gameObject.name.StartsWith(enemyBulletPrefab.name))
        {
            enemyBulletPool.Return(bullet);
        }
        else if (bullet.gameObject.name.StartsWith(plasmaBulletPrefab.name))
        {
            plasmaBulletPool.Return(bullet);
        }
        else if (bullet.gameObject.name.StartsWith(firecannonBulletPrefab.name))
        {
            firecannonBulletPool.Return(bullet);
        }
        else if (bullet.gameObject.name.StartsWith(chainlightningBulletPrefab.name))
        {
            chainlightningBulletPool.Return(bullet);
        }
        else
        {
            for (int i = 0; i < bossBulletPrefabs.Length; i++)
            {
                if (bullet.gameObject.name.StartsWith(bossBulletPrefabs[i].name))
                {
                    bossBulletPools[i].Return(bullet);
                    break;
                }
            }
        }
    }
}