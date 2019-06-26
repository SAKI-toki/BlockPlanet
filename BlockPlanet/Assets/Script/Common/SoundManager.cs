using System.Collections.Generic;
using UnityEngine;

public class SoundManager : SingletonMonoBehaviour<SoundManager>
{

    /// <summary>
    /// 音再生の管理
    /// </summary>

    AudioSource Sounds;

    [SerializeField]
    List<AudioClip> audioClip = new List<AudioClip>();

    // Use this for initialization
    void Start()
    {
        Sounds = gameObject.GetComponent<AudioSource>();
    }

    //スティックの音
    public void Stick()
    {
        Sounds.PlayOneShot(audioClip[0]);
    }
    //決定音
    public void Push()
    {
        Sounds.PlayOneShot(audioClip[1]);
    }
    //ゲームスタートの音
    public void F_Start()
    {
        Sounds.PlayOneShot(audioClip[2]);
    }
    //ゲームオーバーの音
    public void F_GameSet()
    {
        Sounds.PlayOneShot(audioClip[3]);
    }
    //爆弾を発射する音
    public void BombThrow()
    {
        Sounds.PlayOneShot(audioClip[4]);
    }
    //爆弾が爆発する音
    public void Bomb()
    {
        Sounds.PlayOneShot(audioClip[5]);
    }

    //ジャンプの音
    public void Jump()
    {
        Sounds.PlayOneShot(audioClip[6]);
    }
}
