using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FieldManeger : SingletonMonoBehaviour<FieldManeger>
{
    /// <summary>
    /// フィールド内のフラグやUIを管理する
    /// </summary>

    //リザルトに受け渡す情報
    public bool[] PlayerGameOvers = new bool[4];
    //プレイヤーのポイント。スタティックにしないといけない
    static public int[] PlayerPoints = new int[4];
    static public int WinPlayerNumber = 0;
    //ゲームオーバーに一回だけ通る
    private bool GameOver;
    //ボタンを押す
    private bool Pause_Push = false;
    //ポーズの表示非表示を管理
    public bool Pause_Flg = false;
    //BGM
    AudioSource Sound;
    //カウントダウン
    [SerializeField] List<GameObject> image = new List<GameObject>();

    bool Game_Start = false;
    //ポーズ画面のパネル
    [SerializeField]
    Image Panel;

    //勝ち星
    [SerializeField] List<GameObject> Star = new List<GameObject>();
    [SerializeField]
    RectTransform[] UiRectTransforms = new RectTransform[3];
    int select_index = 0;
    Vector3 init_scale = new Vector3();
    [SerializeField]
    Vector3 max_scale = new Vector3();
    float scale_time = 0.0f;

    [SerializeField, Header("プレイヤーリスト")]
    public Player[] players = new Player[4];

    void Start()
    {
        //ゲームスタート時
        StartCoroutine("Gamestart");
        //BGM
        Sound = GetComponent<AudioSource>();
        init_scale = UiRectTransforms[0].localScale;
    }

    void Update()
    {
        if (PlayerPoints[0] == 1)
            Star[0].SetActive(true);
        if (PlayerPoints[1] == 1)
            Star[2].SetActive(true);
        if (PlayerPoints[2] == 1)
            Star[4].SetActive(true);
        if (PlayerPoints[3] == 1)
            Star[6].SetActive(true);


        if (!GameOver)
        {
            int survivePlayerNumber = 0;
            int deathCount = 0;
            for (int i = 0; i < PlayerGameOvers.Length; ++i)
            {
                if (PlayerGameOvers[i]) ++deathCount;
                else survivePlayerNumber = i;
            }
            if (deathCount == PlayerGameOvers.Length - 1)
            {
                GameOver = true;
                ++PlayerPoints[survivePlayerNumber];
                if (PlayerPoints[survivePlayerNumber] == 2)
                {
                    //ゲームオーバー時の処理に入る
                    StartCoroutine("Gameover");
                    //星生成
                    Star[survivePlayerNumber * 2 + 1].SetActive(true);
                    WinPlayerNumber = survivePlayerNumber;
                }
                else
                {
                    StartCoroutine("Restart");
                }
            }
        }
        if (Pause_Flg)
        {
            OnPause();
        }
        else
        {
            Pause_Push = false;
            OnUnPause();
            //ポーズ画面
            if (SwitchInput.GetButtonDown(0, SwitchButton.Pause) && !Pause_Flg && Game_Start)
                Pause_Flg = true;
        }
    }

    //ゲームが始まるとき
    private IEnumerator Gamestart()
    {
        //フェード開始
        Fade.Instance.FadeOut(1.0f);
        //少し待つ
        yield return new WaitForSeconds(0.5f);
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
        foreach (var player in players)
        {
            player.GameStart = true;
        }

        //ゲームが開始された
        Game_Start = true;
        //少し待つ
        yield return new WaitForSeconds(1.0f);
        //BGMを再生する
        Sound.Play();
    }

    //二ラウンド目以降
    private IEnumerator Restart()
    {
        //BGMを停止する
        Sound.Stop();
        //ゲーム終了のSE
        SoundManager.Instance.F_GameSet();
        //少し待つ
        yield return new WaitForSeconds(1);
        //フェード開始
        Fade.Instance.FadeIn(1.0f);
        //少し待つ
        yield return new WaitForSeconds(1);
        //リザルト画面に遷移
        SceneManager.LoadScene("Field");
    }

    //決着がついたとき
    private IEnumerator Gameover()
    {
        //表示
        image[0].SetActive(true);
        //スタティックなので値を0に
        PlayerPoints = new int[4];
        //BGMを停止する
        Sound.Stop();
        //ゲーム終了のSE
        SoundManager.Instance.F_GameSet();
        //少し待つ
        yield return new WaitForSeconds(2);
        //フェード開始
        Fade.Instance.FadeIn(1.0f);
        //少し待つ
        yield return new WaitForSeconds(1);
        //リザルト画面に遷移
        SceneManager.LoadScene("Result");
    }

    //ポーズ画面展開
    public void OnPause()
    {
        //時間を止める
        Time.timeScale = 0;

        //ボタンを見えるように
        Panel.GetComponent<Image>().color = new Color32(0, 0, 0, 150);
        for (int i = 0; i < Panel.transform.childCount; i++)
        {
            Panel.transform.transform.GetChild(i).GetComponent<Image>().color = Color.white;
        }
        SelectUpdate();
        if (SwitchInput.GetButtonDown(0, SwitchButton.Pause) && !Pause_Push)
        {
            Pause_Push = true;
            Pause_Flg = false;
            Panel.GetComponent<Image>().color = new Color32(0, 0, 0, 0);

            for (int i = 0; i < Panel.transform.childCount; i++)
            {
                Panel.transform.transform.GetChild(i).GetComponent<Image>().color = new Color32(0, 0, 0, 0);
            }
            select_index = 0;
            foreach (var obj in UiRectTransforms)
                obj.localScale = init_scale;
            scale_time = 0;
        }
        //ボタンを押したら
        if (SwitchInput.GetButtonDown(0, SwitchButton.Ok) && !Pause_Push)
        {
            Pause_Push = true;
            Pause_Flg = false;
            switch (select_index)
            {
                case 0:
                    Panel.GetComponent<Image>().color = new Color32(0, 0, 0, 0);

                    for (int i = 0; i < Panel.transform.childCount; i++)
                    {
                        Panel.transform.transform.GetChild(i).GetComponent<Image>().color = new Color32(0, 0, 0, 0);
                    }
                    select_index = 0;
                    foreach (var obj in UiRectTransforms)
                        obj.localScale = init_scale;
                    scale_time = 0;
                    break;
                case 1:
                    //スタティックなので値を0に
                    PlayerPoints = new int[4];
                    StartCoroutine("SelectScene");
                    break;

                case 2:
                    //スタティックなので値を0に
                    PlayerPoints = new int[4];
                    StartCoroutine("TitleScene");
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
    private IEnumerator SelectScene()
    {
        //フェード開始
        Fade.Instance.FadeIn(1.0f);
        //少し待つ
        yield return new WaitForSeconds(1);
        //セレクト画面に遷移
        SceneManager.LoadScene("Select");
    }
    private IEnumerator TitleScene()
    {
        //フェード開始
        Fade.Instance.FadeIn(1.0f);
        //少し待つ
        yield return new WaitForSeconds(1);
        //タイトル画面に遷移
        SceneManager.LoadScene("Title");
    }
}
