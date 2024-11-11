using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerParticleEffects : MonoBehaviour
{
    [System.Serializable]
    public class ParticleEffect
    {
        public BuffList buffType;
        public ParticleSystem particleSystem;
    }

    public List<ParticleEffect> buffEffects;
    public ParticleSystem healEffect;
    public List<ParticleEffect> debuffEffects;
    public GameObject invulnerabilityEffectObject;
    private Dictionary<BuffList, ParticleSystem> buffParticles;
    private Dictionary<BuffList, ParticleSystem> debuffParticles;

    private void Awake()
    {
        buffParticles = new Dictionary<BuffList, ParticleSystem>();
        debuffParticles = new Dictionary<BuffList, ParticleSystem>();

        foreach (var effect in buffEffects)
        {
            buffParticles[effect.buffType] = effect.particleSystem;
        }

        foreach (var effect in debuffEffects)
        {
            debuffParticles[effect.buffType] = effect.particleSystem;
        }
    }

    public void PlayBuffEffect(BuffList buffType, float duration)
    {
        if (buffParticles.TryGetValue(buffType, out ParticleSystem particleSystem))
        {
            StartCoroutine(PlayParticleEffect(particleSystem, duration));
        }
    }
    public void PlayDebuffEffect(BuffList debuffType, float duration)
    {
     if (debuffParticles.TryGetValue(debuffType, out ParticleSystem particleSystem))
        {
            StartCoroutine(PlayParticleEffect(particleSystem, duration));
        }
    }


    public void PlayInvulnerabilityEffect(bool isActive)
    {
        if (invulnerabilityEffectObject != null)
        {
            invulnerabilityEffectObject.SetActive(isActive);
        }
    }
    public void PlayShieldEffect(bool isActive)
    {
        if (invulnerabilityEffectObject != null)
        {
            invulnerabilityEffectObject.SetActive(isActive);
        }
    }
    public void PlayHealEffect()
    {
        if (healEffect != null)
        {
            healEffect.Play();
        }
    }

    private IEnumerator PlayParticleEffect(ParticleSystem particleSystem, float duration)
    {
        particleSystem.Play();
        yield return new WaitForSeconds(duration);
        particleSystem.Stop();
    }
}