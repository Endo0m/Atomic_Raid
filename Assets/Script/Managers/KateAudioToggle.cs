using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class KateAudioToggle : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string kateVolumeParameter = "KateVol";
    [SerializeField] private string masterSoundVolumeParameter = "SoundVol";
    [SerializeField] private Toggle toggle;

    private bool isKateEnabled;
    private float masterSoundVolume;

    private void Start()
    {
        // Инициализация состояния переключателя
        isKateEnabled = PlayerPrefs.GetInt(kateVolumeParameter + "_enabled", 1) == 1;
        toggle.isOn = isKateEnabled;

        // Получение текущей громкости основного звука
        audioMixer.GetFloat(masterSoundVolumeParameter, out masterSoundVolume);

        // Установка начального значения громкости Kate
        UpdateKateVolume();

        // Добавление слушателя события изменения состояния переключателя
        toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    private void OnToggleValueChanged(bool isOn)
    {
        isKateEnabled = isOn;
        UpdateKateVolume();
        // Сохранение состояния в PlayerPrefs
        PlayerPrefs.SetInt(kateVolumeParameter + "_enabled", isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void UpdateKateVolume()
    {
        float kateVolume = isKateEnabled ? masterSoundVolume : -80f;
        audioMixer.SetFloat(kateVolumeParameter, kateVolume);
    }

    private void OnEnable()
    {
        // Обновление состояния при активации объекта
        isKateEnabled = PlayerPrefs.GetInt(kateVolumeParameter + "_enabled", 1) == 1;
        toggle.isOn = isKateEnabled;
        audioMixer.GetFloat(masterSoundVolumeParameter, out masterSoundVolume);
        UpdateKateVolume();
    }

    // Этот метод должен вызываться извне, когда меняется основная громкость звука
    public void OnMasterSoundVolumeChanged(float newVolume)
    {
        masterSoundVolume = newVolume;
        UpdateKateVolume();
    }
}