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

    void Start()
    {
        sounds = gameObject.GetComponent<AudioSource>();
    }

    /// <summary>
    /// スティックの音
    /// </summary>
    public void Stick()
    {
        sounds.PlayOneShot(audioClip[0]);
    }
    /// <summary>
    /// 決定音
    /// </summary>
    public void Push()
    {
        sounds.PlayOneShot(audioClip[1]);
    }
    /// <summary>
    /// ゲームスタートの音
    /// </summary>
    public void GameStart()
    {
        sounds.PlayOneShot(audioClip[2]);
    }
    /// <summary>
    /// ゲームオーバーの音
    /// </summary>
    public void GameOver()
    {
        sounds.PlayOneShot(audioClip[3]);
    }
    /// <summary>
    /// 爆弾を発射する音
    /// </summary>
    public void BombThrow()
    {
        sounds.PlayOneShot(audioClip[4]);
    }
    /// <summary>
    /// 爆弾が爆発する音
    /// </summary>
    public void Bomb()
    {
        sounds.PlayOneShot(audioClip[5]);
    }
    /// <summary>
    /// ジャンプの音
    /// </summary>
    public void Jump()
    {
        sounds.PlayOneShot(audioClip[6]);
    }
}
