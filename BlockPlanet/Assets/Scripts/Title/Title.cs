﻿using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : SingletonMonoBehaviour<Title>
{
    //クレジットの画像
    [SerializeField]
    GameObject uiParent = null;
    [SerializeField]
    GameObject creditParent;
    //BGM
    [System.NonSerialized]
    public AudioSource bgmSound = null;
    //タイトルの虹色のマテリアル
    [SerializeField]
    Material rainbowMat = null;
    //タイトルの爆弾
    [SerializeField]
    GameObject titleBomb = null;
    [SerializeField]
    AudioSource fallSound = null;

    private void Start()
    {
        //フェード
        Fade.Instance.FadeOut(1.0f);
        //BGM
        bgmSound = GetComponent<AudioSource>();
        uiParent.SetActive(false);
        creditParent.SetActive(false);
    }

    void Update()
    {
        if (!Fade.Instance.IsEnd) return;
        //爆弾が爆発後
        if (titleBomb == null)
        {
            if (creditParent.activeSelf)
            {
                uiParent.SetActive(false);
                //クレジットの非表示
                if (SwitchInput.GetButtonDown(0, SwitchButton.Pause) || SwitchInput.GetButtonDown(0, SwitchButton.Down))
                {
                    SoundManager.Instance.Push();
                    creditParent.SetActive(false);
                }
            }
            else
            {
                uiParent.SetActive(true);
                //シーン遷移
                if (SwitchInput.GetButtonDown(0, SwitchButton.Ok))
                {
                    SoundManager.Instance.Push();
                    StartCoroutine(Loadscene());
                }
                //クレジットの表示
                else if (SwitchInput.GetButtonDown(0, SwitchButton.Pause))
                {
                    SoundManager.Instance.Push();
                    creditParent.SetActive(true);
                }
            }
        }
        //特定のブロックと爆弾を消す
        else if (SwitchInput.GetButtonDown(0, SwitchButton.Ok))
        {
            BombExplosion();
            Destroy(titleBomb);
            fallSound.Stop();
            bgmSound.Play();
        }
    }
    //セレクト画面に遷移
    private IEnumerator Loadscene()
    {
        //フェード
        Fade.Instance.FadeIn(1.0f);
        //少し待つ
        while (!Fade.Instance.IsEnd) yield return null;
        //シーン遷移
        SceneManager.LoadScene("Select");
    }

    public void BombExplosion()
    {
        //キューブの削除
        foreach (GameObject cube in GameObject.FindGameObjectsWithTag("Cube"))
            Destroy(cube);
        //壊れないキューブのマテリアル変更
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("StrongCube"))
        {
            obj.GetComponent<MeshRenderer>().material = rainbowMat;
        }
    }
}
