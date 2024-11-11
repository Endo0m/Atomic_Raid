using UnityEngine;
using System;
using System.Collections;

public class EnemyLife : MonoBehaviour, IHealth
{
    [SerializeField] private int _hp = 1;
    [SerializeField] private ParticleSystem bloodEffectPrefab;
    [SerializeField] private AudioClip explosionClip;

    private Material matBlink;
    private Material matDefault;
    private Renderer enemyRenderer;

    public event Action OnDeath;

    public int CurrentHealth => _hp;

    private void Start()
    {
        FindRenderer();
        LoadMaterials();
    }

    private void FindRenderer()
    {
        // Попробуем найти Renderer на текущем объекте
        enemyRenderer = GetComponent<Renderer>();
        // Если не нашли на текущем объекте, ищем в дочерних
        if (enemyRenderer == null)
        {
            enemyRenderer = GetComponentInChildren<Renderer>();
        }
        // Если Renderer не найден, просто логируем это, но не выдаем ошибку
        if (enemyRenderer == null)
        {
            Debug.Log("Renderer not found on the enemy object or its children. Visual effects will be skipped.");
        }
    }

    private void LoadMaterials()
    {
        matBlink = Resources.Load("EnemyBlink", typeof(Material)) as Material;
        if (matBlink == null)
        {
            Debug.LogWarning("EnemyBlink material not found. Blinking effect will be skipped.");
        }

        if (enemyRenderer != null)
        {
            matDefault = enemyRenderer.material;
        }
    }

    public void TakeDamage(int damage)
    {
        _hp -= damage;

        if (enemyRenderer != null && matBlink != null)
        {
            enemyRenderer.material = matBlink;
            StartCoroutine(ResetMaterialCoroutine());
        }

        if (_hp <= 0)
        {
            _hp = 0;
            PlayDeathSound();
            PlayBloodEffect(transform.position);
            OnDeath?.Invoke();
        }
    }

    private void PlayDeathSound()
    {
        if (explosionClip != null)
        {
            AudioPlayer.Instance.PlaySound(explosionClip);
        }
    }

    private void PlayBloodEffect(Vector3 position)
    {
        if (bloodEffectPrefab != null)
        {
            ParticleSystem bloodEffect = Instantiate(bloodEffectPrefab, position, Quaternion.identity);
            bloodEffect.Play();
        }
    }

    private IEnumerator ResetMaterialCoroutine()
    {
        yield return new WaitForSeconds(0.2f);
        ResetMaterial();
    }

    private void ResetMaterial()
    {
        if (enemyRenderer != null && matDefault != null)
        {
            enemyRenderer.material = matDefault;
        }
    }
}