using UnityEngine;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine.SceneManagement;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    [System.Serializable]
    public class LocalizationData
    {
        public LocalizationItem[] items;
    }

    [System.Serializable]
    public class LocalizationItem
    {
        public string key;
        public string value;
    }

    private string currentLanguage;
    private Dictionary<string, string> localizedText;

    [SerializeField] private TMP_Dropdown languageDropdown;

    public static bool isReady = false;
    public delegate void ChangeLangText();
    public event ChangeLangText OnLanguageChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeManager();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }
    public string CurrentLanguage
    {
        get { return currentLanguage; }
    }

    private void InitializeManager()
    {
        if (!PlayerPrefs.HasKey("Language"))
        {
            SystemLanguage systemLanguage = Application.systemLanguage;
            string defaultLanguage = IsCyrillicLanguage(systemLanguage) ? "ru_RU" : "en_US";
            PlayerPrefs.SetString("Language", defaultLanguage);
        }

        currentLanguage = PlayerPrefs.GetString("Language");
        Debug.Log($"LocalizationManager: Current language is {currentLanguage}");
        LoadLocalizedText(currentLanguage);
        Debug.Log("LocalizationManager: Initialization completed");
    }

    private bool IsCyrillicLanguage(SystemLanguage language)
    {
        SystemLanguage[] cyrillicLanguages = new SystemLanguage[]
        {
        SystemLanguage.Russian,
        SystemLanguage.Ukrainian,
        SystemLanguage.Belarusian,
        SystemLanguage.Bulgarian,
        };

        return System.Array.Exists(cyrillicLanguages, lang => lang == language);
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        InitializeDropdown();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"LocalizationManager: Scene loaded - {scene.name}");
        InitializeDropdown();
    }

    private void InitializeDropdown()
    {
        Debug.Log("LocalizationManager: Initializing dropdown");
        if (languageDropdown == null)
        {
            languageDropdown = FindObjectOfType<TMP_Dropdown>();
        }

        if (languageDropdown != null)
        {
            languageDropdown.ClearOptions();
            languageDropdown.AddOptions(new List<string> { "English", "Русский", "Français", "Deutsch" });
            languageDropdown.value = GetLanguageIndex(currentLanguage);
            languageDropdown.onValueChanged.RemoveAllListeners();
            languageDropdown.onValueChanged.AddListener(OnLanguageSelected);
            Debug.Log("LocalizationManager: Dropdown initialized successfully");
        }
        else
        {
            Debug.Log("LocalizationManager: LanguageDropdown not found in the scene. Language selection via dropdown will not be available.");
        }
    }

    private int GetLanguageIndex(string langCode)
    {
        switch (langCode)
        {
            case "en_US": return 0;
            case "ru_RU": return 1;
            case "fr_FR": return 2;
            case "de_DE": return 3;
            default: return 0;
        }
    }

    private string GetLanguageCode(int index)
    {
        switch (index)
        {
            case 0: return "en_US";
            case 1: return "ru_RU";
            case 2: return "fr_FR";
            case 3: return "de_DE";
            default: return "en_US";
        }
    }
    private void OnLanguageSelected(int index)
    {
        Debug.Log($"LocalizationManager: Language selected from dropdown - index: {index}");
        string newLanguage = GetLanguageCode(index);
        ChangeLanguage(newLanguage);
    }


    public void SetEnglishLanguage()
    {
        Debug.Log("LocalizationManager: SetEnglishLanguage called");
        ChangeLanguage("en_US");
    }

    public void SetRussianLanguage()
    {
        Debug.Log("LocalizationManager: SetRussianLanguage called");
        ChangeLanguage("ru_RU");
    }

    public void ChangeLanguage(string langCode)
    {
        Debug.Log($"LocalizationManager: ChangeLanguage called with langCode: {langCode}");
        if (langCode != currentLanguage)
        {
            LoadLocalizedText(langCode);
            if (languageDropdown != null)
            {
                languageDropdown.value = GetLanguageIndex(langCode);
            }
            PlayerPrefs.SetString("Language", langCode);
            Debug.Log($"LocalizationManager: Language changed to {langCode}");
            OnLanguageChanged?.Invoke();
        }
        else
        {
            Debug.Log($"LocalizationManager: Language is already set to {langCode}");
        }
    }
    public void LoadLocalizedText(string langName)
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Languages", langName + ".json");
        Debug.Log($"LocalizationManager: Attempting to load JSON from {path}");
        if (!File.Exists(path))
        {
            Debug.LogError($"LocalizationManager: JSON file not found at {path}");
            return;
        }

        try
        {
            string dataAsJson = File.ReadAllText(path);
            LocalizationData loadedData = JsonUtility.FromJson<LocalizationData>(dataAsJson);

            if (loadedData == null || loadedData.items == null)
            {
                Debug.LogError("LocalizationManager: Failed to parse JSON data");
                return;
            }

            localizedText = new Dictionary<string, string>();
            foreach (var item in loadedData.items)
            {
                localizedText[item.key] = item.value;
            }

            PlayerPrefs.SetString("Language", langName);
            currentLanguage = langName;
            isReady = true;
            OnLanguageChanged?.Invoke();
            Debug.Log($"LocalizationManager: Loaded {localizedText.Count} localized strings for {langName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"LocalizationManager: Error loading localization data: {e.Message}");
        }
    }

    public string GetLocalizedValue(string key)
    {
        if (localizedText != null && localizedText.ContainsKey(key))
        {
            return localizedText[key];
        }
        Debug.LogWarning($"LocalizationManager: Key '{key}' not found");
        return key;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}