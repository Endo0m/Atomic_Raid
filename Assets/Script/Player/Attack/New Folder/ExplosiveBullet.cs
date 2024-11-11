using UnityEngine;
using System.Collections;

public class ExplosiveBullet : MonoBehaviour
{
    public ExplosiveWeaponSettings settings;
    private BulletPoolManager poolManager;
    private GameObject player;
    private bool hasExploded = false;

    private void Awake()
    {
        poolManager = FindObjectOfType<BulletPoolManager>();
        player = GameObject.FindGameObjectWithTag("Player");
        GetComponent<Collider>().isTrigger = true;
    }

    public void Initialize(Vector3 direction, float speed)
    {
        GetComponent<Rigidbody>().velocity = direction * speed;
        StartCoroutine(LifetimeCoroutine());
    }

    private IEnumerator LifetimeCoroutine()
    {
        yield return new WaitForSeconds(settings.mainBulletLifetime);
        if (!hasExploded)
        {
            Explode();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasExploded && other.gameObject != player && ((1 << other.gameObject.layer) & settings.explosionLayers) != 0)
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        // Наносим урон в радиусе взрыва
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, settings.explosionRadius, settings.explosionLayers);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject != player)
            {
                IDamageable damageable = hitCollider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(settings.mainBulletDamage);
                }
            }
        }

        // Создаем осколки
        for (int i = 0; i < settings.fragmentCount; i++)
        {
            CreateFragment();
        }

        // Возвращаем основную пулю в пул после небольшой задержки
        StartCoroutine(ReturnToPoolAfterDelay(0.1f));
    }

    private void CreateFragment()
    {
        Bullet fragmentBullet = poolManager.GetPlayerBullet();
        if (fragmentBullet != null)
        {
            fragmentBullet.transform.position = transform.position;
            Vector3 randomDirection = Random.onUnitSphere;
            fragmentBullet.transform.rotation = Quaternion.LookRotation(randomDirection);
            fragmentBullet.Initialize(randomDirection, settings.fragmentSpeed, settings.fragmentLifetime);

            IgnorePlayerCollision ignorePlayer = fragmentBullet.gameObject.AddComponent<IgnorePlayerCollision>();
            ignorePlayer.SetPlayer(player);
        }
    }

    private IEnumerator ReturnToPoolAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        poolManager.ReturnBullet(GetComponent<Bullet>());
    }
}

public class IgnorePlayerCollision : MonoBehaviour
{
    private GameObject player;

    public void SetPlayer(GameObject playerObject)
    {
        player = playerObject;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == player)
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), player.GetComponent<Collider>(), true);
        }
    }
}