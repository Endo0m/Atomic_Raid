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

        // ��������� ��� ������� �� �����, ����� �����
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
            {
                script.enabled = false;
            }
        }

        // ��������� �������� ��� ��������� ������
        StartCoroutine(DeathSequence());
    }
    private IEnumerator DeathSequence()
    {
        // ��������� ��������� �����, ����� ������������� ���������� ��������������
        Collider bossCollider = GetComponent<Collider>();
        if (bossCollider != null) bossCollider.enabled = false;

        // ��������� ������ ����
        if (deathSmokeEffect != null) deathSmokeEffect.Play();

        // ����������� ���� ������
        AudioPlayer.Instance.PlaySound(deathSoundEffect);

        // ���� 1 �������
        yield return new WaitForSeconds(1f);

        // ��������� ��������� ���� � �������� �����������������
        if (intactBodyMesh != null) intactBodyMesh.SetActive(false);
        if (fragmentedBodyMesh != null) fragmentedBodyMesh.SetActive(true);

        // ��������� isKinematic � Rigidbody ������������������ ����
        Rigidbody[] fragmentRigidbodies = fragmentedBodyMesh.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in fragmentRigidbodies)
        {
            rb.isKinematic = false;
        }

        // ��������� ������ ������
        if (explosionEffect != null) explosionEffect.Play();

        // ��������� ���� ������ � ����������
        Vector3 explosionPos = transform.position;
        float explosionForce = 500f; // ��������� ���� �� ������ ����������
        float explosionRadius = 5f; // ��������� ������ �� ������ ����������
        foreach (Rigidbody rb in fragmentRigidbodies)
        {
            rb.AddExplosionForce(explosionForce, explosionPos, explosionRadius);
        }

        // ���� ��� 2 �������, ����� ������� ������ ������ � ������� ������
        yield return new WaitForSeconds(2f);

        // ���������� ����� Game Over
        GameOverManager.Instance.ShowGameOver();

        // ���������� ������ �����
        Destroy(gameObject);
    }

    // �������� ���� ����� ��� ��������, ����� �� ����
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