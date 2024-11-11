using System;
using System.Collections;
using UnityEngine;

public class BossLife : MonoBehaviour, IHealth, IDamageable
{
    [SerializeField] private int _maxHp;
    [SerializeField] private int _hp;
    [SerializeField] private ParticleSystem bloodEffectPrefab;
    [SerializeField] private AudioClip deathSoundEffect;

    private bool isDead = false;
    [SerializeField] private GameObject intactBodyMesh;
    [SerializeField] private GameObject fragmentedBodyMesh;
    [SerializeField] private ParticleSystem deathSmokeEffect;
    [SerializeField] private ParticleSystem explosionEffect;
    private BossFragmentation bossFragmentation;
    private Material matBlink;
    private Material matDefault;
    private Renderer enemyRenderer;
    private BossController bossController;
    public event Action OnBossDeath;
    private GameOverManager gameOverManager;
    private bool isInvulnerable = true;

    public int CurrentHealth => _hp;
    public int MaxHealth => _maxHp;

    private void Start()
    {
        _hp = _maxHp;
        bossFragmentation = GetComponent<BossFragmentation>();
        gameOverManager = GameOverManager.Instance;
        if (gameOverManager == null)
        {
            Debug.LogError("GameOverManager not found in the scene!");
        }
        if (BossHealthUI.Instance != null)
        {
            BossHealthUI.Instance.Initialize(_maxHp);
            BossHealthUI.Instance.UpdateHealth(_hp, _maxHp);
        }
        enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer == null)
        {
            enemyRenderer = GetComponentInChildren<Renderer>();
        }

        if (enemyRenderer == null)
        {
            Debug.LogError("Renderer component not found on the enemy object or its children!");
            return;
        }

        matBlink = Resources.Load("EnemyBlink", typeof(Material)) as Material;
        matDefault = enemyRenderer.material;

        bossController = GetComponent<BossController>();
        if (bossController == null)
        {
            Debug.LogError("BossController not found on the same GameObject!");
        }
    }

    public void TakeDamage(int damage)
    {
        if (isInvulnerable)
        {
            Debug.Log("Boss is invulnerable, no damage taken");
            return;
        }

        _hp -= damage;
        Debug.Log($"Boss took {damage} damage. Current HP: {_hp}");

        if (BossHealthUI.Instance != null)
        {
            BossHealthUI.Instance.UpdateHealth(_hp, _maxHp);
        }

        if (enemyRenderer != null)
        {
            enemyRenderer.material = matBlink;
        }

        if (_hp <= 0)
        {
            Die();
        }
        else
        {
            Invoke("ResetMaterial", 0.2f);
        }
    }


    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Boss is defeated!");
        OnBossDeath?.Invoke();

        if (BossHealthUI.Instance != null)
        {
            BossHealthUI.Instance.ShowHealthUI(false);
        }

        // Отключаем все скрипты на боссе, кроме этого
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
            {
                script.enabled = false;
            }
        }

        // Запускаем корутину для обработки смерти
        StartCoroutine(DeathSequence());
    }
    private IEnumerator DeathSequence()
    {
        // Отключаем коллайдер босса, чтобы предотвратить дальнейшие взаимодействия
        Collider bossCollider = GetComponent<Collider>();
        if (bossCollider != null) bossCollider.enabled = false;

        // Запускаем эффект дыма
        if (deathSmokeEffect != null) deathSmokeEffect.Play();

        // Проигрываем звук смерти
        AudioPlayer.Instance.PlaySound(deathSoundEffect);

        // Ждем 1 секунду
        yield return new WaitForSeconds(1f);

        // Отключаем интактное тело и включаем фрагментированное
        if (intactBodyMesh != null) intactBodyMesh.SetActive(false);
        if (fragmentedBodyMesh != null) fragmentedBodyMesh.SetActive(true);

        // Отключаем isKinematic у Rigidbody фрагментированного тела
        Rigidbody[] fragmentRigidbodies = fragmentedBodyMesh.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in fragmentRigidbodies)
        {
            rb.isKinematic = false;
        }

        // Запускаем эффект взрыва
        if (explosionEffect != null) explosionEffect.Play();

        // Применяем силу взрыва к фрагментам
        Vector3 explosionPos = transform.position;
        float explosionForce = 500f; // Настройте силу по вашему усмотрению
        float explosionRadius = 5f; // Настройте радиус по вашему усмотрению
        foreach (Rigidbody rb in fragmentRigidbodies)
        {
            rb.AddExplosionForce(explosionForce, explosionPos, explosionRadius);
        }

        // Ждем еще 2 секунды, чтобы увидеть эффект взрыва и разлета частей
        yield return new WaitForSeconds(2f);

        // Показываем экран Game Over
        GameOverManager.Instance.ShowGameOver();

        // Уничтожаем объект босса
        Destroy(gameObject);
    }

    // Добавьте этот метод для проверки, мертв ли босс
    public bool IsDead()
    {
        return isDead;
    }

    private void ResetMaterial()
    {
        if (enemyRenderer != null)
        {
            enemyRenderer.material = matDefault;
        }
    }

    public void SetInvulnerability(bool invulnerable)
    {
        isInvulnerable = invulnerable;
    }
}