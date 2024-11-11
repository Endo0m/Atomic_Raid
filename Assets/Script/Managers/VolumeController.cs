using UnityEngine.Audio;
using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    [SerializeField] private string _volumeParametr = "MasterVolume";
    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private Slider _slider;
    [SerializeField] private KateAudioToggle kateAudioToggle;
    private float _volumeValue;
    private const float _multiplier = 20f;

    private void Awake()
    {
        if (_slider != null && _slider.gameObject.activeInHierarchy)
        {
            _slider.onValueChanged.AddListener(HandleSliderValueChanged);
        }
    }
    public void Start()
    {
        _volumeValue = PlayerPrefs.GetFloat(_volumeParametr, 0f);
        if (_slider != null && _slider.gameObject.activeInHierarchy)
        {
            _slider.value = Mathf.Pow(10f, _volumeValue / _multiplier);
        }
        _audioMixer.SetFloat(_volumeParametr, _volumeValue);
    }

    private void HandleSliderValueChanged(float value)
    {
        SetVolume(value);
    }

    public void SetVolume(float value)
    {
        float volumeValue = Mathf.Log10(value) * _multiplier;
        _audioMixer.SetFloat(_volumeParametr, volumeValue);
        PlayerPrefs.SetFloat(_volumeParametr, volumeValue);
        PlayerPrefs.Save();

        // Обновляем громкость Kate
        if (kateAudioToggle != null)
        {
            kateAudioToggle.OnMasterSoundVolumeChanged(volumeValue);
        }
    }

    private void OnEnable()
    {
        // Обновляем значение при активации объекта
        float savedVolume = PlayerPrefs.GetFloat(_volumeParametr, 0f);
        _audioMixer.SetFloat(_volumeParametr, savedVolume);
        if (_slider != null && _slider.gameObject.activeInHierarchy)
        {
            _slider.value = Mathf.Pow(10f, savedVolume / _multiplier);
        }
    }
}