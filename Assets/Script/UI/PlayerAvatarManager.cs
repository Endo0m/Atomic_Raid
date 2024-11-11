using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerAvatarManager : MonoBehaviour
{
    [SerializeField] private Image avatarImage;
    [SerializeField] private float effectDuration = 1.5f;

    [Header("Спрайты состояния покоя")]
    [SerializeField] private Sprite calmHighHpSprite;
    [SerializeField] private Sprite calmMediumHpSprite;
    [SerializeField] private Sprite calmLowHpSprite;

    [Header("Спрайты положительного эффекта")]
    [SerializeField] private Sprite positiveHighHpSprite;
    [SerializeField] private Sprite positiveMediumHpSprite;
    [SerializeField] private Sprite positiveLowHpSprite;

    [Header("Спрайты отрицательного эффекта/урона")]
    [SerializeField] private Sprite negativeHighHpSprite;
    [SerializeField] private Sprite negativeMediumHpSprite;
    [SerializeField] private Sprite negativeLowHpSprite;

    private PlayerLife playerLife;
    private Coroutine effectCoroutine;

    private void Start()
    {
        playerLife = FindObjectOfType<PlayerLife>();
        if (playerLife == null)
        {
            Debug.LogError("PlayerLife компонент не найден!");
        }

        UpdateAvatarSprite(AvatarState.Calm);
    }

    public void UpdateAvatarState(AvatarState state)
    {
        if (effectCoroutine != null)
        {
            StopCoroutine(effectCoroutine);
        }

        UpdateAvatarSprite(state);

        if (state != AvatarState.Calm)
        {
            effectCoroutine = StartCoroutine(ResetToCalm());
        }
    }

    private void UpdateAvatarSprite(AvatarState state)
    {
        Sprite selectedSprite = GetSpriteForState(state);
        if (selectedSprite != null)
        {
            avatarImage.sprite = selectedSprite;
        }
        else
        {
            Debug.LogWarning("Спрайт не назначен для текущего состояния и уровня здоровья!");
        }
    }

    private Sprite GetSpriteForState(AvatarState state)
    {
        int currentHealth = playerLife.CurrentHealth;

        if (currentHealth > 4)
        {
            return state switch
            {
                AvatarState.Calm => calmHighHpSprite,
                AvatarState.Positive => positiveHighHpSprite,
                AvatarState.Negative => negativeHighHpSprite,
                _ => null,
            };
        }
        else if (currentHealth >= 3 && currentHealth <= 4)
        {
            return state switch
            {
                AvatarState.Calm => calmMediumHpSprite,
                AvatarState.Positive => positiveMediumHpSprite,
                AvatarState.Negative => negativeMediumHpSprite,
                _ => null,
            };
        }
        else
        {
            return state switch
            {
                AvatarState.Calm => calmLowHpSprite,
                AvatarState.Positive => positiveLowHpSprite,
                AvatarState.Negative => negativeLowHpSprite,
                _ => null,
            };
        }
    }

    private IEnumerator ResetToCalm()
    {
        yield return new WaitForSeconds(effectDuration);
        UpdateAvatarSprite(AvatarState.Calm);
    }
}

public enum AvatarState
{
    Calm,
    Positive,
    Negative
}