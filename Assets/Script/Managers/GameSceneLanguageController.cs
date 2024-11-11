using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GameSceneLanguageController : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown languageDropdown;

    private void Start()
    {
        InitializeDropdown();
    }

    private void InitializeDropdown()
    {
        if (languageDropdown != null)
        {
            languageDropdown.ClearOptions();
            languageDropdown.AddOptions(new List<string> { "English", "Русский", "Français", "Deutsch" });
            languageDropdown.value = GetLanguageIndex(LocalizationManager.Instance.CurrentLanguage);
            languageDropdown.onValueChanged.RemoveAllListeners();
            languageDropdown.onValueChanged.AddListener(OnLanguageSelected);
            Debug.Log("GameSceneLanguageController: Dropdown initialized successfully");
        }
        else
        {
            Debug.LogError("GameSceneLanguageController: LanguageDropdown is not assigned");
        }
    }

    private void OnLanguageSelected(int index)
    {
        string newLanguage = GetLanguageCode(index);
        LocalizationManager.Instance.ChangeLanguage(newLanguage);
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
}