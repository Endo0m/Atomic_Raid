using UnityEngine;

public class BuffSoundManager : MonoBehaviour
{
    [System.Serializable]
    public class BuffSound
    {
        public BuffList buffType;
        public AudioClip sound;
    }

    public BuffSound[] buffSounds;

    private void Awake()
    {
        // ��������, ��� AudioPlayer ���������� � �����
        if (AudioPlayer.Instance == null)
        {
            Debug.LogError("AudioPlayer �� ������ � �����!");
        }
    }

    public void PlayBuffSound(BuffList buffType)
    {
        AudioClip clip = GetSoundForBuff(buffType);
        if (clip != null)
        {
            AudioPlayer.Instance.PlaySound(clip, 1f, $"Buff_{buffType}", false);
        }
    }

    private AudioClip GetSoundForBuff(BuffList buffType)
    {
        foreach (BuffSound buffSound in buffSounds)
        {
            if (buffSound.buffType == buffType)
            {
                return buffSound.sound;
            }
        }
        Debug.LogWarning($"No sound found for buff type: {buffType}");
        return null;
    }

    // ���� ����� ���������� ���� �����
    public void StopBuffSound(BuffList buffType)
    {
        AudioPlayer.Instance.StopSound($"Buff_{buffType}");
    }

    // ���� ����� ���������, ������������� �� ���� �����
    public bool IsBuffSoundPlaying(BuffList buffType)
    {
        return AudioPlayer.Instance.IsPlaying($"Buff_{buffType}");
    }
}