using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FieldManeger : SingletonMonoBehaviour<FieldManeger>
{
    /// <summary>
    /// フィールド内のフラグやUIを管理する
    /// </summary>

    [SerializeField]
    Text[] points;
    [System.NonSerialized]
    //リザルトに受け渡す情報
    public bool[] PlayerGameOvers = new bool[4];
    int[] PlayerWin = new int[3];
    int winIndex = 0;
    //プレイヤーのポイント。スタティックにしないといけない
    static public int[] PlayerPoints = new int[4];
    const int WinPoint = 4;
    [SerializeField]
    int[] WinPoints = new int[4];
    const int DestroyPoint = 1;
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
    [SerializeField]
    RectTransform[] UiRectTransforms = new RectTransform[3];
    int select_index = 0;
    Vector3 init_scale = new Vector3();
    [SerializeField]
    Vector3 max_scale = new Vector3();
    float scale_time = 0.0f;
    [System.NonSerialized]
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
        for (int i = 0; i < 4; ++i)
        {
            points[i].text = PlayerPoints[i].ToString();
        }
        if (!GameOver && winIndex == PlayerWin.Length)
        {
            GameEnd();
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

    void GameEnd()
    {
        GameOver = true;
        bool[] flg = new bool[4];
        bool[] winFlgs = new bool[4];
        bool winFlg = false;
        //順位に応じた点数を取得
        for (int i = 0; i < PlayerWin.Length; ++i)
        {
            PlayerPoints[PlayerWin[i]] += WinPoints[WinPoints.Length - i - 1];
            flg[PlayerWin[i]] = true;
            if (PlayerPoints[PlayerWin[i]] >= WinPoint)
            {
                winFlgs[PlayerWin[i]] = true;
                winFlg = true;
            }
        }
        for (int i = 0; i < flg.Length; ++i)
        {
            if (!flg[i])
            {
                PlayerPoints[i] += WinPoints[0];
                if (PlayerPoints[i] >= WinPoint)
                {
                    winFlgs[i] = true;
                    winFlg = true;
                }
            }
        }
        //特定の得点に達してるプレイヤーがいたら終了
        if (winFlg)
        {
            int maxPoint = WinPoint - 1;
            for (int i = 0; i < winFlgs.Length; ++i)
            {
                if (PlayerPoints[i] > maxPoint)
                {
                    WinPlayerNumber = i;
                    maxPoint = PlayerPoints[i];
                }
            }
            //ゲームオーバー時の処理に入る
            StartCoroutine("Gameover");
        }
        else
        {
            StartCoroutine("Restart");
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
        for (int i = 0; i < 4; ++i)
        {
            players[i] = GameObject.Find("Player" + (i + 1) + "(Clone)").GetComponent<Player>();
            players[i].GameStart = true;
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
        Panel.GetComponent<Image>().color = new Color32(255, 255, 255, 150);
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

    public void PlayerDestroy(int index, int enemy)
    {
        Debug.Log(index);
        Debug.Log(enemy);
        if (enemy != int.MaxValue)
            PlayerPoints[enemy] += DestroyPoint;
        PlayerWin[winIndex++] = index;
    }
}
