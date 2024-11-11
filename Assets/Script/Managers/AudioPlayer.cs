using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;

public class AudioPlayer : MonoBehaviour
{
    private static AudioPlayer instance;
    private Dictionary<string, AudioSource> namedSources = new Dictionary<string, AudioSource>();
    [SerializeField] private AudioMixer audioMixer;
    private List<AudioSource> audioSources = new List<AudioSource>();

    // Добавляем ссылки на группы аудиомиксера
    private AudioMixerGroup masterGroup;
    private AudioMixerGroup musicGroup;
    private AudioMixerGroup soundGroup;

    public static AudioPlayer Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AudioPlayer>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("AudioPlayer");
                    instance = obj.AddComponent<AudioPlayer>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioMixer();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudioMixer()
    {
        if (audioMixer != null)
        {
            masterGroup = audioMixer.FindMatchingGroups("Master")[0];
            musicGroup = audioMixer.FindMatchingGroups("Music")[0];
            soundGroup = audioMixer.FindMatchingGroups("Sound")[0];
        }
        else
        {
            Debug.LogError("AudioMixer не назначен в инспекторе AudioPlayer!");
        }
    }

    public void PlaySound(AudioClip clip, float volume = 1f, string soundName = null, bool isMusic = false)
    {
        AudioSource audioSource = GetAvailableAudioSource();
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.outputAudioMixerGroup = isMusic ? musicGroup : soundGroup;
        audioSource.Play();
        if (!string.IsNullOrEmpty(soundName))
        {
            namedSources[soundName] = audioSource;
        }
    }

    private AudioSource GetAvailableAudioSource()
    {
        AudioSource availableSource = audioSources.Find(source => !source.isPlaying);
        if (availableSource == null)
        {
            availableSource = gameObject.AddComponent<AudioSource>();
            availableSource.playOnAwake = false;
            availableSource.spatialBlend = 0f; // 2D sound
            audioSources.Add(availableSource);
        }
        return availableSource;
    }

    public void UpdateAudioMixer(AudioMixer mixer)
    {
        audioMixer = mixer;
        InitializeAudioMixer();
        ApplyAudioMixerToSources();
    }

    private void ApplyAudioMixerToSources()
    {
        foreach (var source in audioSources)
        {
            // Проверяем, к какой группе принадлежит источник звука
            if (source.outputAudioMixerGroup == musicGroup)
            {
                source.outputAudioMixerGroup = musicGroup;
            }
            else
            {
                source.outputAudioMixerGroup = soundGroup;
            }
        }
    }

    public void StopSound(string soundName)
    {
        if (namedSources.TryGetValue(soundName, out AudioSource source))
        {
            source.Stop();
            namedSources.Remove(soundName);
        }
    }

    public bool IsPlaying(string soundName)
    {
        return namedSources.TryGetValue(soundName, out AudioSource source) && source.isPlaying;
    }

    // Метод для обновления громкости
    public void UpdateVolume(string parameterName, float volume)
    {
        if (audioMixer != null)
        {
            float decibelVolume = volume <= 0 ? -80f : Mathf.Log10(volume) * 20f;
            audioMixer.SetFloat(parameterName, decibelVolume);
        }
    }
}