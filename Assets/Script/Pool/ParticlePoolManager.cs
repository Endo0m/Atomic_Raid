using UnityEngine;

public class ParticlePoolManager : MonoBehaviour
{
    [SerializeField] private ParticleSystem dustEffectPrefab;
    [SerializeField] private int preloadCount = 10;
    private ObjectPool<ParticleSystem> dustEffectPool;

    private void Awake()
    {
        dustEffectPool = new ObjectPool<ParticleSystem>(
            () => Instantiate(dustEffectPrefab),
            OnGetParticle,
            OnReturnParticle,
            preloadCount
        );
    }

    private void OnGetParticle(ParticleSystem particle)
    {
        particle.gameObject.SetActive(true);
    }

    private void OnReturnParticle(ParticleSystem particle)
    {
        particle.gameObject.SetActive(false);
    }

    public ParticleSystem GetDustEffect()
    {
        return dustEffectPool.Get();
    }

    public void ReturnParticle(ParticleSystem particle)
    {
        if (particle.gameObject.name.StartsWith(dustEffectPrefab.name))
        {
            dustEffectPool.Return(particle);
        }
    }
}