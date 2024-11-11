using UnityEngine.Audio;
using UnityEngine;

public class VolumeInit : MonoBehaviour
{
    [System.Serializable]
    public class VolumeParameter
    {
        public string name;
        public float defaultValue;
    }

    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private VolumeParameter[] volumeParameters;

    void Start()
    {
        InitializeVolumes();
    }

    public void InitializeVolumes()
    {
        foreach (var param in volumeParameters)
        {
            float volumeValue = PlayerPrefs.GetFloat(param.name, param.defaultValue);
            _audioMixer.SetFloat(param.name, volumeValue);
            Debug.Log($"Инициализация параметра {param.name}: {volumeValue}");
        }

        if (AudioPlayer.Instance != null)
        {
            AudioPlayer.Instance.UpdateAudioMixer(_audioMixer);
            Debug.Log("AudioPlayer обновлен в InitializeVolumes");
        }
        else
        {
            Debug.LogWarning("AudioPlayer.Instance не найден в InitializeVolumes");
        }
    }

    public void SaveVolume(string parameterName, float value)
    {
        PlayerPrefs.SetFloat(parameterName, value);
        _audioMixer.SetFloat(parameterName, value);
        Debug.Log($"Сохранение параметра {parameterName}: {value}");

        if (AudioPlayer.Instance != null)
        {
            AudioPlayer.Instance.UpdateAudioMixer(_audioMixer);
            Debug.Log("AudioPlayer обновлен в SaveVolume");
        }
        else
        {
            Debug.LogWarning("AudioPlayer.Instance не найден в SaveVolume");
        }
    }

    public AudioMixer GetAudioMixer()
    {
        return _audioMixer;
    }
}