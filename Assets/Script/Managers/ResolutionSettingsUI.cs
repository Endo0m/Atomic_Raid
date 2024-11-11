using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ResolutionSettingsUI : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    private Resolution[] resolutions;

    private void Start()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        resolutions = ResolutionManager.Instance.GetResolutions();

        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);
        }
        resolutionDropdown.AddOptions(options);

        resolutionDropdown.value = ResolutionManager.Instance.GetCurrentResolutionIndex();
        resolutionDropdown.RefreshShownValue();

        fullscreenToggle.isOn = Screen.fullScreen;

        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggled);
    }

    private void OnResolutionChanged(int index)
    {
        ResolutionManager.Instance.SetResolution(index);
    }

    private void OnFullscreenToggled(bool isFullscreen)
    {
        ResolutionManager.Instance.ToggleFullscreen();
    }
}