using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Image[] baseHealthImages;
    [SerializeField] private Image[] extraHealthImages;
    [SerializeField] private Image buffIcon;
    [SerializeField] private TMPro.TextMeshProUGUI buffCountdownText;
    [SerializeField] private BuffData buffData;

    private Coroutine currentBuffCoroutine;
    private Dictionary<BuffList, Sprite> buffIcons;

    private void Awake()
    {
        InitializeBuffIcons();
    }
    private void Start()
    {
        ScoreManager.Instance.OnScoreChanged += UpdateScoreDisplay;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= UpdateScoreDisplay;
        }
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateScoreDisplay(ScoreManager.Instance.GetScore());
        UpdateHealthDisplay(5); // Предполагаем, что начальное здоровье равно 5
        if (scene.name == "Game") // Имя игровой сцены
        {
            CameraController cameraController = FindObjectOfType<CameraController>();
            if (cameraController != null)
            {
                cameraController.ResetState();
            }
        }
    }

    private void InitializeBuffIcons()
    {
        buffIcons = new Dictionary<BuffList, Sprite>();
        foreach (var buffInfo in buffData.buffs)
        {
            buffIcons[buffInfo.buffType] = buffInfo.icon;
        }
    }
    public void UpdateHealthDisplay(int health)
    {
        int baseHealth = Mathf.Min(health, 5);
        int extraLives = Mathf.Max(0, health - 5);

        // Отображение базовых жизней
        for (int i = 0; i < baseHealthImages.Length; i++)
        {
            baseHealthImages[i].gameObject.SetActive(i < baseHealth);
        }

        // Отображение дополнительных жизней
        for (int i = 0; i < extraHealthImages.Length; i++)
        {
            extraHealthImages[i].gameObject.SetActive(i < extraLives);
        }
    }

    public void UpdateScoreDisplay(int newScore)
    {
        if (scoreText != null)
        {
            string localizedScoreText = LocalizationManager.Instance.GetLocalizedValue("Score_Text");
            scoreText.text = $"{localizedScoreText} {newScore}";
        }
    }


    private Sprite GetBuffSprite(BuffList buffType)
    {
        if (buffIcons.TryGetValue(buffType, out Sprite sprite))
        {
            return sprite;
        }
        Debug.LogWarning($"Sprite for buff type {buffType} not found.");
        return null;
    }

    public void ShowBuff(BuffList buffType, float duration)
    {
        if (currentBuffCoroutine != null)
        {
            StopCoroutine(currentBuffCoroutine);
        }

        buffIcon.gameObject.SetActive(true);
        buffCountdownText.gameObject.SetActive(true);

        buffIcon.sprite = GetBuffSprite(buffType);

        currentBuffCoroutine = StartCoroutine(BuffCountdown(duration));
    }

    public void ClearBuff()
    {
        if (currentBuffCoroutine != null)
        {
            StopCoroutine(currentBuffCoroutine);
        }

        buffIcon.gameObject.SetActive(false);
        buffCountdownText.gameObject.SetActive(false);
    }

    public void UpdateBuffDisplay(BuffList buffType, float remainingTime)
    {
        buffIcon.sprite = GetBuffSprite(buffType);
        if (buffCountdownText != null)
        {
            int seconds = Mathf.CeilToInt(remainingTime);
            int minutes = seconds / 60;
            seconds %= 60;
            buffCountdownText.text = string.Format("{0}:{1:00}", minutes, seconds);
        }
    }

    private IEnumerator BuffCountdown(float duration)
    {
        float remainingTime = duration;

        while (remainingTime > 0)
        {
            int seconds = Mathf.CeilToInt(remainingTime);
            int minutes = seconds / 60;
            seconds %= 60;
            buffCountdownText.text = string.Format("{0}:{1:00}", minutes, seconds);

            yield return new WaitForSeconds(0.1f);
            remainingTime -= 0.1f;
        }

        ClearBuff();
    }

}