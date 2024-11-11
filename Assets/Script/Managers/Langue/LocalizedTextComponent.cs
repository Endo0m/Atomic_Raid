using TMPro;
using UnityEngine;

public class LocalizedTextComponent : MonoBehaviour
{
    [SerializeField] private string key;
    private TextMeshProUGUI textComponent;
    private bool isInitialized = false;

    private void Awake()
    {
        Debug.Log($"LocalizedTextComponent: Awake started for key {key}");
        textComponent = GetComponent<TextMeshProUGUI>();
        if (textComponent == null)
        {
            Debug.LogError($"LocalizedTextComponent: TextMeshProUGUI component not found on {gameObject.name}");
        }
        Debug.Log("LocalizedTextComponent: Awake completed");
    }

    private void Start()
    {
        Debug.Log($"LocalizedTextComponent: Start called for key {key}");
        InitializeLocalization();
    }

    private void InitializeLocalization()
    {
        if (LocalizationManager.Instance != null && !isInitialized)
        {
            LocalizationManager.Instance.OnLanguageChanged += UpdateText;
            UpdateText();
            isInitialized = true;
            Debug.Log($"LocalizedTextComponent: Initialized for key {key}");
        }
        else
        {
            Debug.Log($"LocalizedTextComponent: Waiting for LocalizationManager to initialize for key {key}");
            Invoke("InitializeLocalization", 0.1f);
        }
    }

    private void OnDestroy()
    {
        Debug.Log($"LocalizedTextComponent: OnDestroy called for key {key}");
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= UpdateText;
        }
    }

    private void UpdateText()
    {
        Debug.Log($"LocalizedTextComponent: UpdateText called for key {key}");
        if (textComponent != null && LocalizationManager.Instance != null)
        {
            string localizedValue = LocalizationManager.Instance.GetLocalizedValue(key);
            Debug.Log($"LocalizedTextComponent: Got localized value '{localizedValue}' for key {key}");
            textComponent.text = localizedValue;
        }
        else
        {
            if (textComponent == null)
                Debug.LogError($"LocalizedTextComponent: TextMeshProUGUI component is null for key {key}");
            if (LocalizationManager.Instance == null)
                Debug.LogError($"LocalizedTextComponent: LocalizationManager instance is null for key {key}");
        }
    }
}