using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    [System.Serializable]
    public class MusicSet
    {
        public string name;
        public List<AudioClip> tracks;
    }

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string volumeParameter = "MusicVol";
    [SerializeField] private List<MusicSet> locationMusicSets;
    [SerializeField] private List<MusicSet> bossMusicSets;

    private AudioSource musicSource;
    private int currentTrackIndex = 0;
    private MusicSet currentMusicSet;
    private Dictionary<string, AudioClip> preloadedClips = new Dictionary<string, AudioClip>();

    private void Start()
    {
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Music")[0];
        float savedVolume = PlayerPrefs.GetFloat(volumeParameter, 0f);
        audioMixer.SetFloat(volumeParameter, savedVolume);

        StartCoroutine(PreloadAudioClips());
    }

    private void Update()
    {
        if (!musicSource.isPlaying && currentMusicSet != null)
        {
            PlayNextTrack();
        }
    }

    private IEnumerator PreloadAudioClips()
    {
        foreach (var set in locationMusicSets)
        {
            foreach (var clip in set.tracks)
            {
                yield return StartCoroutine(PreloadClip(clip));
            }
        }
        foreach (var set in bossMusicSets)
        {
            foreach (var clip in set.tracks)
            {
                yield return StartCoroutine(PreloadClip(clip));
            }
        }
    }

    private IEnumerator PreloadClip(AudioClip clip)
    {
        if (!preloadedClips.ContainsKey(clip.name))
        {
            if (clip.loadState == AudioDataLoadState.Unloaded)
            {
                clip.LoadAudioData();
            }
            while (clip.loadState == AudioDataLoadState.Loading)
            {
                yield return null;
            }
            if (clip.loadState == AudioDataLoadState.Loaded)
            {
                preloadedClips[clip.name] = clip;
            }
            else
            {
                Debug.LogError($"Failed to load audio clip: {clip.name}");
            }
        }
    }

    public void SetLocationMusic(int locationIndex)
    {
        StartCoroutine(SetLocationMusicAsync(locationIndex));
    }

    private IEnumerator SetLocationMusicAsync(int locationIndex)
    {
        yield return null; // ∆дем следующего кадра

        if (locationIndex >= 0 && locationIndex < locationMusicSets.Count)
        {
            yield return StartCoroutine(SetMusicSet(locationMusicSets[locationIndex]));
        }
        else
        {
            Debug.LogWarning($"Location music set for index {locationIndex} not found.");
        }
    }

    public void SetBossMusic(int bossIndex)
    {
        StartCoroutine(SetBossMusicAsync(bossIndex));
    }

    private IEnumerator SetBossMusicAsync(int bossIndex)
    {
        yield return null; // ∆дем следующего кадра

        if (bossIndex >= 0 && bossIndex < bossMusicSets.Count)
        {
            yield return StartCoroutine(SetMusicSet(bossMusicSets[bossIndex]));
        }
        else
        {
            Debug.LogWarning($"Boss music set for index {bossIndex} not found.");
        }
    }

    private IEnumerator SetMusicSet(MusicSet newSet)
    {
        if (newSet != null && newSet != currentMusicSet)
        {
            yield return StartCoroutine(SmoothTransition(newSet));
        }
    }

    private IEnumerator SmoothTransition(MusicSet newSet)
    {
        if (musicSource.isPlaying)
        {
            yield return StartCoroutine(FadeOut(0.5f));
        }

        // ”бедимс€, что первый трек нового набора готов к воспроизведению
        AudioClip nextClip = newSet.tracks[0];
        if (!preloadedClips.ContainsKey(nextClip.name))
        {
            yield return StartCoroutine(PreloadClip(nextClip));
        }

        currentMusicSet = newSet;
        currentTrackIndex = 0;
        yield return StartCoroutine(FadeIn(0.5f));
    }

    private IEnumerator FadeOut(float duration)
    {
        float startVolume = musicSource.volume;
        float timer = 0;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0, timer / duration);
            yield return null;
        }
        musicSource.Stop();
        musicSource.volume = startVolume;
    }

    private IEnumerator FadeIn(float duration)
    {
        PlayNextTrack();
        float startVolume = musicSource.volume;
        musicSource.volume = 0;
        float timer = 0;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0, startVolume, timer / duration);
            yield return null;
        }
    }

    private void PlayNextTrack()
    {
        if (currentMusicSet == null || currentMusicSet.tracks.Count == 0) return;

        AudioClip nextClip = currentMusicSet.tracks[currentTrackIndex];
        if (preloadedClips.TryGetValue(nextClip.name, out AudioClip preloadedClip))
        {
            musicSource.clip = preloadedClip;
        }
        else
        {
            musicSource.clip = nextClip;
        }

        musicSource.Play();
        currentTrackIndex = (currentTrackIndex + 1) % currentMusicSet.tracks.Count;
    }

    public void AdjustVolume(float adjustment)
    {
        float currentVolume;
        audioMixer.GetFloat(volumeParameter, out currentVolume);
        float newVolume = Mathf.Clamp(currentVolume + adjustment * 20f, -80f, 0f);
        audioMixer.SetFloat(volumeParameter, newVolume);
        PlayerPrefs.SetFloat(volumeParameter, newVolume);
        PlayerPrefs.Save();
    }
}