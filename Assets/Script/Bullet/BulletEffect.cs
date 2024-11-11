using UnityEngine;

public class BulletEffect : MonoBehaviour
{
    public enum EffectType
    {
        Normal,
        Fast
    }
    public int bulletHealth = 1;
    public EffectType effectType;
    public int damage = 1;
    private BulletPoolManager bulletPoolManager;

    private void Awake()
    {
        bulletPoolManager = FindObjectOfType<BulletPoolManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerLife playerLife = other.GetComponent<PlayerLife>();
            if (playerLife != null)
            {
                Shield shield = other.GetComponent<Shield>();
                if (shield != null && shield.IsActive)
                {
                    PlaySound(playerLife, playerLife.shieldHitSFX);
                    ReturnToPool();
                    return;
                }

                PlaySound(playerLife, playerLife.hitSFX);
                playerLife.TakeDamage(damage);
                ReturnToPool();
            }
        }
    }

    private void PlaySound(PlayerLife playerLife, AudioClip clip)
    {
        if (clip != null && playerLife.PlayerAudioSource != null)
        {
            playerLife.PlayerAudioSource.PlayOneShot(clip);
        }
    }

    public void TakeDamage(int damage)
    {
        bulletHealth -= damage;
        if (bulletHealth <= 0)
        {
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        if (bulletPoolManager != null)
        {
            bulletPoolManager.ReturnBullet(GetComponent<Bullet>());
        }
        else
        {
            Destroy(gameObject);
        }
    }
}