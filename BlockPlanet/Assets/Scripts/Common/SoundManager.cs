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

    //スティックの音
    public void Stick()
    {
        sounds.PlayOneShot(audioClip[0]);
    }
    //決定音
    public void Push()
    {
        sounds.PlayOneShot(audioClip[1]);
    }
    //ゲームスタートの音
    public void GameStart()
    {
        sounds.PlayOneShot(audioClip[2]);
    }
    //ゲームオーバーの音
    public void GameOver()
    {
        sounds.PlayOneShot(audioClip[3]);
    }
    //爆弾を発射する音
    public void BombThrow()
    {
        sounds.PlayOneShot(audioClip[4]);
    }
    //爆弾が爆発する音
    public void Bomb()
    {
        sounds.PlayOneShot(audioClip[5]);
    }
    //ジャンプの音
    public void Jump()
    {
        sounds.PlayOneShot(audioClip[6]);
    }
}
