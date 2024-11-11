using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Audio;

public class PlayerLife : MonoBehaviour, IHealth, IDamageable
{
    [SerializeField] private int _hp = 5;
    [SerializeField] private int _maxHp = 10;
    [SerializeField] private float _invulnerabilityTime = 2f;
    [SerializeField] private float shakeAmount = 0.1f;
    [SerializeField] private float shakeTime = 0.2f;
    [SerializeField] private string playerModelName = "Body";
    [SerializeField] private Material[] playerMaterials;
    [SerializeField] private Image damageImage;
    [SerializeField] private Material matBlink;
    [SerializeField] public AudioClip hitSFX;
    [SerializeField] public AudioClip shieldHitSFX;
    [SerializeField] private Shield shield;
    [SerializeField] private float deathDelay = 5f;
    [SerializeField] private GameObject fireJetpackParticle1;
    [SerializeField] private GameObject fireJetpackParticle2;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioMixerGroup soundGroup;
    private AudioSource playerAudioSource;
    private Material[] defaultMaterials;
    private GameUIManager _gameUIManager;
    private bool isInvulnerable = false;
    private Renderer playerRenderer;
    private Vector3 originalPosition;
    private Transform playerModelTransform;
    private PlayerParticleEffects particleEffects;
    private PlayerAvatarManager avatarManager;
    private bool isDead = false;
    private Rigidbody mainRigidbody;
    private Animator animator;
    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;
    private PlayerMove playerMove;
    private PlayerShoot playerShoot;
    private PlayerAimController playerAimController;
    private EnvironmentStaticGenerator environmentGenerator;
    public AudioSource PlayerAudioSource => playerAudioSource;
    public int CurrentHealth => _hp;
    public int MaxHealth => _maxHp;

    private void Start()
    {
        InitializeComponents();
        SetupMaterials();
        SetupRagdoll();
        SetupAudio();
        if (shield == null)
        {
            Debug.LogError("Shield component is not assigned in the inspector!");
        }
        avatarManager = FindObjectOfType<PlayerAvatarManager>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            RestoreHealth(1);
        }
    }
    private void SetupAudio()
    {
        playerAudioSource = gameObject.AddComponent<AudioSource>();
        playerAudioSource.outputAudioMixerGroup = soundGroup;
    }
    private void InitializeComponents()
    {
        _gameUIManager = FindObjectOfType<GameUIManager>();
        particleEffects = GetComponent<PlayerParticleEffects>();
        playerRenderer = FindRendererInChildren(transform, playerModelName);
        if (playerRenderer == null)
        {
            Debug.LogError($"Renderer component not found on child object named '{playerModelName}'!");
            return;
        }
        playerModelTransform = playerRenderer.transform;
        originalPosition = playerModelTransform.localPosition;
        mainRigidbody = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        playerMove = GetComponent<PlayerMove>();
        playerShoot = GetComponent<PlayerShoot>();
        playerAimController = GetComponent<PlayerAimController>();
        environmentGenerator = FindObjectOfType<EnvironmentStaticGenerator>();
        UpdateUIHealth();
    }
    
        private void SetupMaterials()
    {
        if (matBlink == null)
        {
            matBlink = Resources.Load("PlayerBlink", typeof(Material)) as Material;
            if (matBlink == null)
            {
                Debug.LogError("Failed to load PlayerBlink material!");
                return;
            }
        }

        defaultMaterials = playerRenderer.materials;
        playerMaterials = new Material[defaultMaterials.Length];
        for (int i = 0; i < defaultMaterials.Length; i++)
        {
            playerMaterials[i] = matBlink;
        }

        if (damageImage != null)
        {
            damageImage.color = new Color(damageImage.color.r, damageImage.color.g, damageImage.color.b, 0);
        }
    }

    private void SetupRagdoll()
    {
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();
        SetRagdollEnabled(false);
    }

    private void SetRagdollEnabled(bool enabled)
    {
        foreach (var rb in ragdollRigidbodies)
        {
            if (rb != mainRigidbody)
            {
                rb.isKinematic = !enabled;
                rb.useGravity = enabled;
            }
        }

        foreach (var col in ragdollColliders)
        {
            if (col != GetComponent<Collider>())
            {
                col.enabled = enabled;
            }
        }

        if (animator != null)
        {
            animator.enabled = !enabled;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isInvulnerable || isDead) return;

        _hp -= damage;
        _hp = Mathf.Clamp(_hp, 0, _maxHp);
        avatarManager.UpdateAvatarState(AvatarState.Negative);
        if (_hp <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvulnerabilityCoroutine(_invulnerabilityTime));
            StartCoroutine(BlinkEffect());
            StartCoroutine(ShakeEffect());
            AnimateDamageImage();
        }
        UpdateUIHealth();
    }

    public void RestoreHealth(int amount)
    {
        if (isDead) return;
        _hp = Mathf.Min(_hp + amount, _maxHp);
        UpdateUIHealth();
        if (particleEffects != null)
        {
            particleEffects.PlayHealEffect();
        }
    }

    private void UpdateUIHealth()
    {
        if (_gameUIManager != null)
        {
            _gameUIManager.UpdateHealthDisplay(_hp);
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // Деинициализация баффов оружия
        PlayerFlags.LargeBullet = false;
        PlayerFlags.instance.StopLaser();
        
        // Отключаем частицы FireJetpackParticle
        if (fireJetpackParticle1 != null)
        {
            fireJetpackParticle1.SetActive(false);
        }
        if (fireJetpackParticle2 != null)
        {
            fireJetpackParticle2.SetActive(false);
        }
        // Активируем ragdoll
        SetRagdollEnabled(true);

        // Включаем гравитацию для основного Rigidbody
        if (mainRigidbody != null)
        {
            mainRigidbody.useGravity = true;
            mainRigidbody.isKinematic = false;
        }

        // Отключаем скрипты движения, стрельбы и наведения
        if (playerMove != null) playerMove.enabled = false;
        if (playerShoot != null) playerShoot.enabled = false;
        if (playerAimController != null) playerAimController.enabled = false;

        // Отключаем аниматор
        if (animator != null)
        {
            animator.enabled = false;
        }

        // Останавливаем движение мира
        if (environmentGenerator != null)
        {
            environmentGenerator.StopWorldOnPlayerDeath();
        }

        // Запускаем корутину для завершения игры после задержки
        StartCoroutine(EndGameAfterDelay());
    }

    private IEnumerator EndGameAfterDelay()
    {
        yield return new WaitForSeconds(deathDelay);
        GameOverManager.Instance.ShowGameOver();
        //GameoverManagerOffline.Instance.ShowGameOver();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;
        if (other.CompareTag("EnemyBullet") || other.CompareTag("BossBullet"))
        {
            HandleDamage(other);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isDead)
        {
            // Добавьте здесь логику для реалистичного поведения при столкновениях во время смерти
            // Например, воспроизведение звука удара или применение дополнительной силы
        }
        else
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Objects") || collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                HandleObjectCollision(collision.collider);
            }
        }
    }

    private void HandleDamage(Collider other)
    {
        if (shield != null && shield.IsActive)
        {
            if (shieldHitSFX != null)
            {
                playerAudioSource.PlayOneShot(shieldHitSFX);
            }
        }
        else
        {
            if (hitSFX != null)
            {
                playerAudioSource.PlayOneShot(hitSFX);
            }
            else
            {
                Debug.LogWarning("hitSFX is null!");
            }

            int damage = 1;
            if (other.CompareTag("BossBullet"))
            {
                BulletEffect bulletEffect = other.GetComponent<BulletEffect>();
                if (bulletEffect != null)
                {
                    damage = bulletEffect.damage;
                }
                playerAudioSource.PlayOneShot(hitSFX);
            }
            TakeDamage(damage);
        }
    }
    private void HandleObjectCollision(Collider other)
    {
        if (shield != null && shield.IsActive)
        {
            Debug.Log("Shield is active, playing shieldHitSFX for object collision");
            if (shieldHitSFX != null)
            {
                playerAudioSource.PlayOneShot(shieldHitSFX);
            }
            else
            {
                Debug.LogWarning("shieldHitSFX is null!");
            }
        }
        else
        {
            Debug.Log("Shield is not active or null, playing hitSFX for object collision");
            if (hitSFX != null)
            {
                playerAudioSource.PlayOneShot(hitSFX);
            }
            else
            {
                Debug.LogWarning("hitSFX is null!");
            }

            TakeDamage(1);
        }

        ApplyPushForce(other);
    }

    private void ApplyPushForce(Collider other)
    {
        Vector3 pushDirection = transform.position - other.ClosestPoint(transform.position);
        pushDirection.z = 0;
        pushDirection = pushDirection.normalized;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(pushDirection * 10f, ForceMode.Impulse);
        }
        else
        {
            transform.position += pushDirection * 0.5f;
        }
    }

    public IEnumerator InvulnerabilityCoroutine(float time)
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(time);
        isInvulnerable = false;
    }

    private IEnumerator BlinkEffect()
    {
        if (playerRenderer != null && playerMaterials != null && defaultMaterials != null)
        {
            for (int i = 0; i < 3; i++)
            {
                playerRenderer.materials = playerMaterials;
                yield return new WaitForSeconds(0.1f);
                playerRenderer.materials = defaultMaterials;
                yield return new WaitForSeconds(0.1f);
            }
        }
        else
        {
            Debug.LogWarning("BlinkEffect: Some materials are not initialized!");
        }
    }

    private IEnumerator ShakeEffect()
    {
        float elapsed = 0f;
        while (elapsed < shakeTime)
        {
            Vector3 randomPoint = originalPosition + Random.insideUnitSphere * shakeAmount;
            playerModelTransform.localPosition = randomPoint;
            elapsed += Time.deltaTime;
            yield return null;
        }
        playerModelTransform.localPosition = originalPosition;
    }

    private void AnimateDamageImage()
    {
        if (damageImage != null)
        {
            damageImage.DOKill();
            damageImage.DOFade(1, 0.1f).OnComplete(() => damageImage.DOFade(0, 0.5f));
        }
    }

    private Renderer FindRendererInChildren(Transform parent, string targetName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == targetName)
            {
                Renderer renderer = child.GetComponent<Renderer>();
                if (renderer != null) return renderer;
            }
            Renderer childRenderer = FindRendererInChildren(child, targetName);
            if (childRenderer != null) return childRenderer;
        }
        return null;
    }
}