using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.IO;

public class BossEventManager : MonoBehaviour
{
    public static BossEventManager Instance { get; private set; }

    [System.Serializable]
    public class BossEvent
    {
        public string textKey;
        public string audioKey;
        public float delay;
    }

    [System.Serializable]
    public class LocalizedBossData
    {
        public List<BossEventLocalization> bossEvents;
    }

    [System.Serializable]
    public class BossEventLocalization
    {
        public string textKey;
        public string localizedText;
        public string audioPath;
    }

    [SerializeField] private GameObject uiBossEventPanel;
    [SerializeField] private TextMeshProUGUI bossEventText;
    [SerializeField] private BossEvent[] bossEvents;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioMixerGroup soundGroup;
    [SerializeField] private AudioMixerGroup kateGroup;

    private Dictionary<string, BossEventLocalization> localizedBossEvents;
    private AudioSource soundAudioSource;
    private AudioSource kateAudioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }

        LoadLocalizedBossEvents();
    }

    private void InitializeAudioSources()
    {
        soundAudioSource = gameObject.AddComponent<AudioSource>();
        soundAudioSource.outputAudioMixerGroup = soundGroup;

        kateAudioSource = gameObject.AddComponent<AudioSource>();
        kateAudioSource.outputAudioMixerGroup = kateGroup;
    }

    private void Start()
    {
        LocalizationManager.Instance.OnLanguageChanged += LoadLocalizedBossEvents;
    }

    private void OnDisable()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= LoadLocalizedBossEvents;
        }
    }

    private void LoadLocalizedBossEvents()
    {
        string currentLanguage = LocalizationManager.Instance.CurrentLanguage;
        string path = Path.Combine(Application.streamingAssetsPath, "BossEvents", $"boss_events_{currentLanguage}.json");
        Debug.Log($"Попытка загрузить события боссов из: {path}");

        if (File.Exists(path))
        {
            string jsonContent = File.ReadAllText(path);
            Debug.Log($"Содержимое JSON файла событий боссов: {jsonContent}");
            LocalizedBossData data = JsonUtility.FromJson<LocalizedBossData>(jsonContent);

            if (data != null && data.bossEvents != null && data.bossEvents.Count > 0)
            {
                localizedBossEvents = new Dictionary<string, BossEventLocalization>();
                foreach (var bossEvent in data.bossEvents)
                {
                    localizedBossEvents[bossEvent.textKey] = bossEvent;
                    Debug.Log($"Загружено событие босса: {bossEvent.textKey} = {bossEvent.localizedText}");
                }
                Debug.Log($"Загружено событий боссов: {localizedBossEvents.Count}");
            }
            else
            {
                Debug.LogError("Не удалось десериализовать JSON или список событий боссов пуст");
            }
        }
        else
        {
            Debug.LogError($"Файл локализованных событий боссов не найден: {path}");
        }
    }

    public void PlayBossEvent(string key)
    {
        foreach (var bossEvent in bossEvents)
        {
            if (bossEvent.textKey == key)
            {
                if (localizedBossEvents.TryGetValue(bossEvent.textKey, out BossEventLocalization localization))
                {
                    StartCoroutine(ShowBossEvent(localization.localizedText, localization.audioPath, bossEvent.delay));
                }
                else
                {
                    Debug.LogError($"Локализация не найдена для ключа: {bossEvent.textKey}");
                }
                break;
            }
        }
    }

    private IEnumerator ShowBossEvent(string text, string audioPath, float delay)
    {
        yield return new WaitForSeconds(delay);

        uiBossEventPanel.SetActive(true);
        bossEventText.text = text;

        AudioClip bossEventSound = Resources.Load<AudioClip>(audioPath);
        if (bossEventSound != null)
        {
            // Воспроизводим звук через оба аудиоисточника
            soundAudioSource.PlayOneShot(bossEventSound);
            kateAudioSource.PlayOneShot(bossEventSound);
            yield return new WaitForSeconds(bossEventSound.length);
        }
        else
        {
            Debug.LogError($"Звуковой файл не найден: {audioPath}");
            yield return new WaitForSeconds(2f);
        }

        uiBossEventPanel.SetActive(false);
    }
}