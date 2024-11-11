using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ResolutionManager : MonoBehaviour
{
    public static ResolutionManager Instance { get; private set; }

    private Resolution[] resolutions;
    private int currentResolutionIndex;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeResolutions();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeResolutions()
    {
        resolutions = Screen.resolutions.Select(resolution => new Resolution { width = resolution.width, height = resolution.height }).Distinct().ToArray();
        currentResolutionIndex = FindCurrentResolutionIndex();
    }

    private int FindCurrentResolutionIndex()
    {
        return System.Array.FindIndex(resolutions, r => r.width == Screen.width && r.height == Screen.height);
    }

    public Resolution[] GetResolutions()
    {
        return resolutions;
    }

    public void SetResolution(int index)
    {
        if (index >= 0 && index < resolutions.Length)
        {
            Resolution resolution = resolutions[index];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
            currentResolutionIndex = index;
        }
    }

    public int GetCurrentResolutionIndex()
    {
        return currentResolutionIndex;
    }

    public void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }
}