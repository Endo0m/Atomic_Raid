using UnityEngine;
using System.Collections;

public class BulletImpactEffects : MonoBehaviour
{
    [SerializeField] private ParticleSystem hitEffectDustPrefab;
    [SerializeField] private float hitEffectLifetime = 5f;
    [SerializeField] private LayerMask layersForEffect;
    [SerializeField] private float raycastDistance = 0.1f;

    private ParticlePoolManager particlePoolManager;
    private Vector3 lastPosition;

    private void Awake()
    {
        particlePoolManager = FindObjectOfType<ParticlePoolManager>();
        lastPosition = transform.position;
    }

    private void Update()
    {
        CheckForImpact();
        lastPosition = transform.position;
    }

    private void CheckForImpact()
    {
        Vector3 direction = (transform.position - lastPosition).normalized;
        float distance = Vector3.Distance(transform.position, lastPosition);

        RaycastHit hit;
        if (Physics.Raycast(lastPosition, direction, out hit, distance + raycastDistance, layersForEffect))
        {
            HandleImpact(hit.collider.gameObject, hit.point, hit.normal);
            ReturnBulletToPool();
        }
    }

    private void HandleImpact(GameObject hitObject, Vector3 hitPoint, Vector3 hitNormal)
    {
        PlayImpactEffect(hitPoint, hitNormal, hitObject.transform);
    }

    private void PlayImpactEffect(Vector3 hitPoint, Vector3 hitNormal, Transform parent)
    {
        if (particlePoolManager == null || hitEffectDustPrefab == null)
        {
            Debug.LogWarning("ParticlePoolManager or hitEffectDustPrefab is not assigned!");
            return;
        }

        ParticleSystem dustEffect = particlePoolManager.GetDustEffect();
        if (dustEffect != null)
        {
            dustEffect.transform.position = hitPoint;
            dustEffect.transform.rotation = Quaternion.LookRotation(hitNormal);
            dustEffect.transform.SetParent(parent, true);

            dustEffect.transform.localScale = transform.localScale;

            var main = dustEffect.main;
            main.stopAction = ParticleSystemStopAction.None;

            dustEffect.Play();

            StartCoroutine(StopEmissionAfterDelay(dustEffect, 0.1f));
            StartCoroutine(ReturnParticleAfterDelay(dustEffect, hitEffectLifetime));
        }
    }

    private IEnumerator StopEmissionAfterDelay(ParticleSystem particle, float delay)
    {
        yield return new WaitForSeconds(delay);
        var emission = particle.emission;
        emission.enabled = false;
    }

    private IEnumerator ReturnParticleAfterDelay(ParticleSystem particle, float delay)
    {
        yield return new WaitForSeconds(delay);
        particle.transform.SetParent(null);
        particle.transform.localScale = Vector3.one;
        particlePoolManager.ReturnParticle(particle);
    }

    private void ReturnBulletToPool()
    {
        Bullet bullet = GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.ReturnToPool();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}