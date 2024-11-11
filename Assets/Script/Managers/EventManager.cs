using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.IO;
using UnityEngine.Audio;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    [System.Serializable]
    public class WarningEvent
    {
        public int enemyCount;
        public string textKey;
        public string audioKey;
    }

    [System.Serializable]
    public class LocalizedWarningData
    {
        public List<WarningEventLocalization> warnings;
    }

    [System.Serializable]
    public class WarningEventLocalization
    {
        public string textKey;
        public string localizedText;
        public string audioPath;
    }

    [SerializeField] private GameObject uiWarningPanel;
    [SerializeField] private TextMeshProUGUI warningText;
    [SerializeField] private WarningEvent[] warningEvents;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioMixerGroup kateGroup;
    [SerializeField] private AudioMixerGroup soundGroup;
    private AudioSource kateAudioSource;

    private Dictionary<string, WarningEventLocalization> localizedWarnings;
    private AudioSource warningAudioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        LoadLocalizedWarnings();
        InitializeAudioSources();
        InitializeAudioSource();
    }

    private void InitializeAudioSource()
    {
        warningAudioSource = gameObject.AddComponent<AudioSource>();
        warningAudioSource.playOnAwake = false;
        warningAudioSource.spatialBlend = 0f; // 2D sound

        AudioMixerGroup[] groups = audioMixer.FindMatchingGroups("Kate");
        if (groups.Length > 0)
        {
            warningAudioSource.outputAudioMixerGroup = groups[0];
        }
        else
        {
            Debug.LogError("Sound группа не найдена в аудиомикшере");
        }
    }
    private void InitializeAudioSources()
    {
        warningAudioSource = gameObject.AddComponent<AudioSource>();
        warningAudioSource.playOnAwake = false;
        warningAudioSource.spatialBlend = 0f; // 2D sound
        warningAudioSource.outputAudioMixerGroup = soundGroup;

        kateAudioSource = gameObject.AddComponent<AudioSource>();
        kateAudioSource.playOnAwake = false;
        kateAudioSource.spatialBlend = 0f; // 2D sound
        kateAudioSource.outputAudioMixerGroup = kateGroup;
    }
    private void Start()
    {
        EnemyCounter.Instance.OnEnemyMissed += OnEnemyMissed;
        LocalizationManager.Instance.OnLanguageChanged += LoadLocalizedWarnings;
    }

    private void OnDisable()
    {
        if (EnemyCounter.Instance != null)
        {
            EnemyCounter.Instance.OnEnemyMissed -= OnEnemyMissed;
        }
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= LoadLocalizedWarnings;
        }
    }

    private void LoadLocalizedWarnings()
    {
        string currentLanguage = LocalizationManager.Instance.CurrentLanguage;
        string path = Path.Combine(Application.streamingAssetsPath, "Warnings", $"warnings_{currentLanguage}.json");
        Debug.Log($"Попытка загрузить предупреждения из: {path}");

        if (File.Exists(path))
        {
            string jsonContent = File.ReadAllText(path);
            Debug.Log($"Содержимое JSON файла предупреждений: {jsonContent}");
            LocalizedWarningData data = JsonUtility.FromJson<LocalizedWarningData>(jsonContent);

            if (data != null && data.warnings != null && data.warnings.Count > 0)
            {
                localizedWarnings = new Dictionary<string, WarningEventLocalization>();
                foreach (var warning in data.warnings)
                {
                    localizedWarnings[warning.textKey] = warning;
                    Debug.Log($"Загружено предупреждение: {warning.textKey} = {warning.localizedText}");
                }
                Debug.Log($"Загружено предупреждений: {localizedWarnings.Count}");
            }
            else
            {
                Debug.LogError("Не удалось десериализовать JSON или список предупреждений пуст");
            }
        }
        else
        {
            Debug.LogError($"Файл локализованных предупреждений не найден: {path}");
        }
    }

    public void OnEnemyMissed(int missedCount)
    {
        foreach (var warningEvent in warningEvents)
        {
            if (missedCount == warningEvent.enemyCount)
            {
                if (localizedWarnings.TryGetValue(warningEvent.textKey, out WarningEventLocalization localization))
                {
                    StartCoroutine(ShowWarning(localization.localizedText, localization.audioPath));
                }
                else
                {
                    Debug.LogError($"Локализация не найдена для ключа: {warningEvent.textKey}");
                }
                break;
            }
        }
    }

    private IEnumerator ShowWarning(string text, string audioPath)
    {
        uiWarningPanel.SetActive(true);
        warningText.text = text;

        AudioClip warningSound = Resources.Load<AudioClip>(audioPath);
        if (warningSound != null)
        {
            // Воспроизводим звук предупреждения через основной звуковой канал
            warningAudioSource.PlayOneShot(warningSound);

            // Воспроизводим тот же звук через канал Kate
            kateAudioSource.PlayOneShot(warningSound);

            yield return new WaitForSeconds(warningSound.length);
        }
        else
        {
            Debug.LogError($"Звуковой файл не найден: {audioPath}");
            yield return new WaitForSeconds(2f);
        }

        uiWarningPanel.SetActive(false);
    }
}