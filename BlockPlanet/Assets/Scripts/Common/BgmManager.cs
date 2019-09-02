using UnityEngine;

public enum BgmEnum { TITLE, PLAYER_SELECT, STAGE_SELECT, BATTLE, DRUM_ROLL, RESULT, NONE };

[RequireComponent(typeof(AudioSource))]
public class BgmManager : SingletonMonoBehaviour<BgmManager>
{
    [SerializeField]
    AudioClip title;
    [SerializeField]
    AudioClip playerSelect;
    [SerializeField]
    AudioClip stageSelect;
    [SerializeField]
    AudioClip battle;
    [SerializeField]
    AudioClip drum_roll;
    [SerializeField]
    AudioClip result;
    AudioSource aud;

    BgmEnum currentBgm = BgmEnum.NONE;

    public void Play(BgmEnum bgm, bool is_loop = true, float volume = 1.0f)
    {
        GetAudioSource();
        aud.volume = volume;
        aud.loop = is_loop;
        if (currentBgm != bgm)
        {
            aud.Stop();
            currentBgm = bgm;
        }
        switch (bgm)
        {
            case BgmEnum.TITLE:
                aud.clip = title;
                break;
            case BgmEnum.PLAYER_SELECT:
                aud.clip = playerSelect;
                break;
            case BgmEnum.STAGE_SELECT:
                aud.clip = stageSelect;
                break;
            case BgmEnum.BATTLE:
                aud.clip = battle;
                break;
            case BgmEnum.DRUM_ROLL:
                aud.clip = drum_roll;
                break;
            case BgmEnum.RESULT:
                aud.clip = result;
                break;
        }
        aud.Play();
    }

    public void Stop()
    {
        GetAudioSource();
        aud.Stop();
    }

    public void SetVolume(float volume)
    {
        GetAudioSource();
        aud.volume = volume;
    }
    public float GetVolume()
    {
        GetAudioSource();
        return aud.volume;
    }

    void GetAudioSource()
    {
        if (!aud)
            aud = GetComponent<AudioSource>();
    }
}
