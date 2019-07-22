﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// フィールド内のフラグやUIを管理する
/// </summary>
public class FieldManeger : SingletonMonoBehaviour<FieldManeger>
{
    [System.NonSerialized]
    public bool[] PlayerGameOvers;
    //プレイヤーのポイント。スタティックにしないといけない
    static public int[] PlayerPoints = new int[4];
    //勝利ポイント
    const int WinPoint = 3;
    static public int WinPlayerNumber = 0;
    //ゲームオーバーに一回だけ通る
    [System.NonSerialized]
    public bool IsGameOver = false;
    //ボタンを押す
    private bool Pause_Push = false;
    //ポーズの表示非表示を管理
    [System.NonSerialized]
    public bool IsPause = false;
    //BGM
    AudioSource Sound = null;
    //カウントダウン
    [SerializeField] List<GameObject> image = new List<GameObject>();

    bool IsGameStart = false;
    //ポーズ画面のパネル
    [SerializeField]
    GameObject PauseObject = null;
    [SerializeField]
    RectTransform[] UiRectTransforms = new RectTransform[3];
    int select_index = 0;
    Vector3 init_scale = new Vector3();
    [SerializeField]
    Vector3 max_scale = new Vector3();
    float scale_time = 0.0f;
    [System.NonSerialized]
    public Player[] players;

    [SerializeField]
    GameObject OnTheWayObject;
    [SerializeField]
    Image[] OnTheWayImages;
    [SerializeField]
    Image BackgroundImage;
    [SerializeField]
    RectTransform[] PlayerRectTransforms;
    [SerializeField]
    RectTransform[] PlayerWinRectTransforms;
    [SerializeField]
    Image[] PlayerImages;
    [SerializeField]
    Sprite stopSprite;
    [SerializeField]
    AudioSource hornSound;
    void Start()
    {
        //プレイ人数分要素を確保
        players = new Player[BlockCreater.GetInstance().maxPlayerNumber];
        PlayerGameOvers = new bool[BlockCreater.GetInstance().maxPlayerNumber];
        //プレイしないプレイヤーの途中経過の画像を差し替える
        for (int i = BlockCreater.GetInstance().maxPlayerNumber; i < PlayerImages.Length; ++i)
        {
            PlayerImages[i].sprite = stopSprite;
            PlayerImages[i].SetNativeSize();
        }
        //ゲームスタート時
        StartCoroutine(Gamestart());
        //BGM
        Sound = GetComponent<AudioSource>();
        init_scale = UiRectTransforms[0].localScale;
    }

    void Update()
    {
        if (IsGameOver || !IsGameStart) return;
        //プレイヤーが死んだ数
        int deathCount = 0;
        for (int i = 0; i < PlayerGameOvers.Length; ++i)
        {
            if (PlayerGameOvers[i])
                ++deathCount;
            else
                WinPlayerNumber = i;
        }
        if (deathCount >= PlayerGameOvers.Length - 1)
        {
            IsGameOver = true;
            //勝者決定
            if (deathCount == PlayerGameOvers.Length - 1)
            {
                ++PlayerPoints[WinPlayerNumber];
            }
            //勝利ポイントに達したかどうか
            if (PlayerPoints[WinPlayerNumber] == WinPoint)
            {
                StartCoroutine(Gameover());
            }
            else
            {
                StartCoroutine(Restart(deathCount == PlayerGameOvers.Length));
            }
        }
        if (IsPause)
        {
            OnPause();
        }
        else
        {
            Pause_Push = false;
            OnUnPause();
            //ポーズ画面
            if (SwitchInput.GetButtonDown(0, SwitchButton.Pause))
                IsPause = true;
        }
    }

    //ゲームが始まるとき
    private IEnumerator Gamestart()
    {
        //フェード開始
        Fade.Instance.FadeOut(1.0f);
        //少し待つ
        while (!Fade.Instance.IsEnd) yield return null;
        //カウントダウンの音
        SoundManager.Instance.F_Start();
        //カウントダウン開始
        for (int count = 3; count >= 1; count--)
        {
            //表示
            image[count].SetActive(true);
            //1秒待つ
            yield return new WaitForSeconds(1.0f);
            //消す
            image[count].SetActive(false);
        }
        //プレイヤーを動けるようにする
        for (int i = 0; i < players.Length; ++i)
        {
            players[i] = GameObject.Find("Player" + (i + 1) + "(Clone)").GetComponent<Player>();
            players[i].GameStart = true;
        }

        //ゲームが開始された
        IsGameStart = true;
        //少し待つ
        yield return new WaitForSeconds(1.0f);
        //BGMを再生する
        Sound.Play();
    }

    //二ラウンド目以降
    private IEnumerator Restart(bool isDraw = false)
    {
        //BGMを停止する
        Sound.Stop();
        //ゲーム終了のSE
        SoundManager.Instance.F_GameSet();
        //少し待つ
        yield return new WaitForSeconds(1);
        if (!isDraw)
            yield return StartCoroutine(OnTheWayCoroutine());
        //フェード開始
        Fade.Instance.FadeIn(1.0f);
        //少し待つ
        while (!Fade.Instance.IsEnd) yield return null;
        //リザルト画面に遷移
        SceneManager.LoadScene("Field");
    }

    //決着がついたとき
    private IEnumerator Gameover()
    {
        //表示
        image[0].SetActive(true);
        //BGMを停止する
        Sound.Stop();
        //ゲーム終了のSE
        SoundManager.Instance.F_GameSet();
        //少し待つ
        yield return new WaitForSeconds(1);
        yield return StartCoroutine(OnTheWayCoroutine());
        //フェード開始
        Fade.Instance.FadeIn(1.0f);
        //少し待つ
        while (!Fade.Instance.IsEnd) yield return null;
        for (int i = 0; i < 4; ++i)
        {
            ResultManager.ResultPoints[i] = PlayerPoints[i];
        }
        //スタティックなので値を0に
        PlayerPoints = new int[4];
        //リザルト画面に遷移
        SceneManager.LoadScene("Result");
    }

    /// <summary>
    /// 途中経過
    /// </summary>
    /// <returns></returns>
    IEnumerator OnTheWayCoroutine()
    {
        OnTheWayObject.SetActive(true);
        Vector3 pos;
        //初期位置
        for (int i = 0; i < PlayerPoints.Length; ++i)
        {
            if (i == WinPlayerNumber)
            {
                pos = PlayerWinRectTransforms[PlayerPoints[i] - 1].position;
            }
            else
            {
                pos = PlayerWinRectTransforms[PlayerPoints[i]].position;
            }
            pos.y = PlayerRectTransforms[i].position.y;
            PlayerRectTransforms[i].position = pos;
        }
        float alpha = 0.0f;
        Color color;
        //フェードイン
        while (alpha < 1.0f)
        {
            alpha += Time.deltaTime;
            foreach (var image in OnTheWayImages)
            {
                color = image.color;
                color.a = alpha;
                image.color = color;
            }
            color = BackgroundImage.color;
            color.a = alpha * 0.8f;
            BackgroundImage.color = color;
            yield return null;
        }
        //移動速度
        float moveSpeed = PlayerWinRectTransforms[PlayerPoints[WinPlayerNumber]].position.x -
                            PlayerRectTransforms[WinPlayerNumber].position.x;
        //時間を測る変数
        float timeCount = 0.0f;
        pos = PlayerRectTransforms[WinPlayerNumber].position;
        //勝ったプレイヤーの移動
        while (PlayerRectTransforms[WinPlayerNumber].position.x <
        PlayerWinRectTransforms[PlayerPoints[WinPlayerNumber]].position.x)
        {
            //煽りのクラクション
            if (SwitchInput.GetButtonDown(WinPlayerNumber, SwitchButton.Horn))
            {
                if (hornSound.isPlaying) hornSound.Stop();
                hornSound.Play();
            }
            //移動処理
            timeCount += Time.deltaTime;
            pos.x += moveSpeed * Time.deltaTime / 3;
            PlayerRectTransforms[WinPlayerNumber].position = pos;
            PlayerRectTransforms[WinPlayerNumber].rotation = Quaternion.Euler(0, 0, Mathf.Sin(timeCount * 2) * Mathf.Rad2Deg / 4);
            yield return null;
        }
        PlayerRectTransforms[WinPlayerNumber].rotation = Quaternion.identity;
        yield return new WaitForSeconds(0.5f);
    }

    //ポーズ画面展開
    public void OnPause()
    {
        //時間を止める
        Time.timeScale = 0;

        //ボタンを見えるように
        PauseObject.SetActive(true);
        SelectUpdate();
        if (SwitchInput.GetButtonDown(0, SwitchButton.Pause) && !Pause_Push)
        {
            Pause_Push = true;
            IsPause = false;
            PauseObject.SetActive(false);
            select_index = 0;
            foreach (var obj in UiRectTransforms)
                obj.localScale = init_scale;
            scale_time = 0;
        }
        //ボタンを押したら
        if (SwitchInput.GetButtonDown(0, SwitchButton.Ok) && !Pause_Push)
        {
            Pause_Push = true;
            IsPause = false;
            switch (select_index)
            {
                case 0:
                    PauseObject.SetActive(false);
                    select_index = 0;
                    foreach (var obj in UiRectTransforms)
                        obj.localScale = init_scale;
                    scale_time = 0;
                    break;
                case 1:
                    //スタティックなので値を0に
                    PlayerPoints = new int[4];
                    StartCoroutine(TransitionSelectScene());
                    break;

                case 2:
                    //スタティックなので値を0に
                    PlayerPoints = new int[4];
                    StartCoroutine(TransitionTitleScene());
                    break;
            }
        }
    }

    void SelectUpdate()
    {
        int prev_index = select_index;
        if (SwitchInput.GetButtonDown(0, SwitchButton.StickDown))
        {
            ++select_index;
        }
        else if (SwitchInput.GetButtonDown(0, SwitchButton.StickUp))
        {
            --select_index;
        }
        select_index = Mathf.Clamp(select_index, 0, UiRectTransforms.Length - 1);
        if (prev_index != select_index)
        {
            UiRectTransforms[prev_index].localScale = init_scale;
            SoundManager.Instance.Stick();
            scale_time = 0.0f;
        }
        scale_time += Time.unscaledDeltaTime * 6;
        UiRectTransforms[select_index].localScale = init_scale + (max_scale - init_scale) * ((Mathf.Sin(scale_time) + 1) / 2);
    }

    //ポーズ画面をしまう
    public void OnUnPause()
    {
        Time.timeScale = 1;
    }

    ////ポーズ画面からの遷移
    private IEnumerator TransitionSelectScene()
    {
        //フェード開始
        Fade.Instance.FadeIn(1.0f);
        //少し待つ
        while (!Fade.Instance.IsEnd) yield return null;
        //セレクト画面に遷移
        SceneManager.LoadScene("Select");
    }
    private IEnumerator TransitionTitleScene()
    {
        //フェード開始
        Fade.Instance.FadeIn(1.0f);
        //少し待つ
        while (!Fade.Instance.IsEnd) yield return null;
        //タイトル画面に遷移
        SceneManager.LoadScene("Title");
    }
}
