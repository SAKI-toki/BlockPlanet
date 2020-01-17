using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 音再生の管理
/// </summary>
public class SoundManager : SingletonMonoBehaviour<SoundManager>
{
    AudioSource sounds = null;

    [SerializeField]
    List<AudioClip> audioClip = new List<AudioClip>();
    [SerializeField]
    List<float> audioVolume = new List<float>();

    void Start()
    {
        sounds = gameObject.GetComponent<AudioSource>();
    }

    /// <summary>
    /// スティックの音
    /// </summary>
    public void Stick()
    {
        SoundPlayerOneShot(0);
    }
    /// <summary>
    /// 決定音
    /// </summary>
    public void Push()
    {
        SoundPlayerOneShot(1);
    }
    /// <summary>
    /// ゲームスタートの音
    /// </summary>
    public void GameStart()
    {
        SoundPlayerOneShot(2);
    }
    /// <summary>
    /// ゲームオーバーの音
    /// </summary>
    public void GameOver()
    {
        SoundPlayerOneShot(3);
    }
    /// <summary>
    /// 爆弾を発射する音
    /// </summary>
    public void BombThrow()
    {
        SoundPlayerOneShot(4);
    }
    /// <summary>
    /// 爆弾が爆発する音
    /// </summary>
    public void Bomb()
    {
        SoundPlayerOneShot(5);
    }
    /// <summary>
    /// ジャンプの音
    /// </summary>
    public void Jump()
    {
        SoundPlayerOneShot(6);
    }

    void SoundPlayerOneShot(int index)
    {
        sounds.PlayOneShot(audioClip[index], audioVolume[index]);
    }
}
