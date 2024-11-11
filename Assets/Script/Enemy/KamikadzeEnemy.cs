using UnityEngine;

public class KamikadzeEnemy : Enemy, IMovable, IEnemy
{
    private float moveSpeed = 10f;
    private float explosionRange = 5f;
    private float explosionForce = 500f;
    private int damageAmount = 1;
    [SerializeField] private GameObject explosionParticlePrefab;
    [SerializeField] private AudioClip explosionClip;

    private Vector3 targetPosition;
    private bool hasExploded = false;
    private bool hasReachedTarget = false;

    protected override void Start()
    {
        base.Start();
        moveSpeed = ConfigData.Instance.moveSpeedKamikadze;
        explosionRange = ConfigData.Instance.explosionRangeKamikadze;
        explosionForce = ConfigData.Instance.explosionForceKamikadze;
        damageAmount = ConfigData.Instance.damageAmountKamikadze;
        if (player != null)
        {
            targetPosition = player.position;
        }
    }

    protected override void Update()
    {
        base.Update();
        if (!hasExploded && !hasReachedTarget)
        {
            if (lookAtPlayerBehavior != null)
            {
                lookAtPlayerBehavior.LookAtPlayer(player);
            }
            Move();
        }
        else if (hasReachedTarget && !hasExploded)
        {
            Explode();
        }
    }

    public void Move()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            hasReachedTarget = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasExploded)
        {
            Explode();
        }
    }

    private void Explode()
    {
        hasExploded = true;
        killedByPlayer = false;

        if (explosionClip != null)
        {
            AudioPlayer.Instance.PlaySound(explosionClip, 1f, "KamikadzeExplosion", false);
        }
        if (explosionParticlePrefab != null)
        {
            GameObject explosionEffect = Instantiate(explosionParticlePrefab, transform.position, Quaternion.identity);
            Destroy(explosionEffect, 2f);
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRange);
        foreach (Collider hit in colliders)
        {
            if (hit.CompareTag("Player"))
            {
                hit.GetComponent<PlayerLife>()?.TakeDamage(damageAmount);

                Rigidbody playerRb = hit.GetComponent<Rigidbody>();
                if (playerRb != null)
                {
                    Vector3 direction = (hit.transform.position - transform.position).normalized;
                    direction.z = 0;
                    playerRb.AddForce(direction * explosionForce, ForceMode.Impulse);
                }

                break;
            }
        }

        Die();
    }
}