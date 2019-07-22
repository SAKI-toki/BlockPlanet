using System.Collections;
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
    public bool[] playerGameOvers;
    //プレイヤーのポイント。スタティックにしないといけない
    static public int[] playerPoints = new int[4];
    //勝利ポイント
    const int WinPoint = 3;
    static public int winPlayerNumber = 0;
    //ゲームオーバーに一回だけ通る
    [System.NonSerialized]
    public bool isGameOver = false;
    //ボタンを押す
    private bool pausePush = false;
    //ポーズの表示非表示を管理
    [System.NonSerialized]
    public bool isPause = false;
    //BGM
    AudioSource bgmSound = null;
    //カウントダウン
    [SerializeField] List<GameObject> image = new List<GameObject>();

    bool isGameStart = false;
    //ポーズ画面のパネル
    [SerializeField]
    GameObject pauseObject = null;
    [SerializeField]
    RectTransform[] uiRectTransforms = new RectTransform[3];
    int selectIndex = 0;
    Vector3 initScale = new Vector3();
    [SerializeField]
    Vector3 maxScale = new Vector3();
    float scaleTime = 0.0f;
    [System.NonSerialized]
    public Player[] players;

    [SerializeField]
    GameObject onTheWayObject;
    [SerializeField]
    Image[] onTheWayImages;
    [SerializeField]
    Image backgroundImage;
    [SerializeField]
    RectTransform[] playerRectTransforms;
    [SerializeField]
    RectTransform[] playerWinRectTransforms;
    [SerializeField]
    Image[] playerImages;
    [SerializeField]
    Sprite stopSprite;
    [SerializeField]
    AudioSource hornSound;
    void Start()
    {
        //プレイ人数分要素を確保
        players = new Player[BlockCreater.GetInstance().maxPlayerNumber];
        playerGameOvers = new bool[BlockCreater.GetInstance().maxPlayerNumber];
        //プレイしないプレイヤーの途中経過の画像を差し替える
        for (int i = BlockCreater.GetInstance().maxPlayerNumber; i < playerImages.Length; ++i)
        {
            playerImages[i].sprite = stopSprite;
            playerImages[i].SetNativeSize();
        }
        //ゲームスタート時
        StartCoroutine(Gamestart());
        //BGM
        bgmSound = GetComponent<AudioSource>();
        initScale = uiRectTransforms[0].localScale;
    }

    void Update()
    {
        if (isGameOver || !isGameStart) return;
        //プレイヤーが死んだ数
        int deathCount = 0;
        for (int i = 0; i < playerGameOvers.Length; ++i)
        {
            if (playerGameOvers[i])
                ++deathCount;
            else
                winPlayerNumber = i;
        }
        if (deathCount >= playerGameOvers.Length - 1)
        {
            isGameOver = true;
            //勝者決定
            if (deathCount == playerGameOvers.Length - 1)
            {
                ++playerPoints[winPlayerNumber];
            }
            //勝利ポイントに達したかどうか
            if (playerPoints[winPlayerNumber] == WinPoint)
            {
                StartCoroutine(Gameover());
            }
            else
            {
                StartCoroutine(Restart(deathCount == playerGameOvers.Length));
            }
        }
        if (isPause)
        {
            OnPause();
        }
        else
        {
            pausePush = false;
            OnUnPause();
            //ポーズ画面
            if (SwitchInput.GetButtonDown(0, SwitchButton.Pause))
                isPause = true;
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
        SoundManager.Instance.GameStart();
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
            players[i].isGameStart = true;
        }

        //ゲームが開始された
        isGameStart = true;
        //少し待つ
        yield return new WaitForSeconds(1.0f);
        //BGMを再生する
        bgmSound.Play();
    }

    //二ラウンド目以降
    private IEnumerator Restart(bool isDraw = false)
    {
        //BGMを停止する
        bgmSound.Stop();
        //ゲーム終了のSE
        SoundManager.Instance.GameOver();
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
        bgmSound.Stop();
        //ゲーム終了のSE
        SoundManager.Instance.GameOver();
        //少し待つ
        yield return new WaitForSeconds(1);
        yield return StartCoroutine(OnTheWayCoroutine());
        //フェード開始
        Fade.Instance.FadeIn(1.0f);
        //少し待つ
        while (!Fade.Instance.IsEnd) yield return null;
        for (int i = 0; i < 4; ++i)
        {
            ResultManager.resultPoints[i] = playerPoints[i];
        }
        //スタティックなので値を0に
        playerPoints = new int[4];
        //リザルト画面に遷移
        SceneManager.LoadScene("Result");
    }

    /// <summary>
    /// 途中経過
    /// </summary>
    /// <returns></returns>
    IEnumerator OnTheWayCoroutine()
    {
        onTheWayObject.SetActive(true);
        Vector3 pos;
        //初期位置
        for (int i = 0; i < playerPoints.Length; ++i)
        {
            if (i == winPlayerNumber)
            {
                pos = playerWinRectTransforms[playerPoints[i] - 1].position;
            }
            else
            {
                pos = playerWinRectTransforms[playerPoints[i]].position;
            }
            pos.y = playerRectTransforms[i].position.y;
            playerRectTransforms[i].position = pos;
        }
        float alpha = 0.0f;
        Color color;
        //フェードイン
        while (alpha < 1.0f)
        {
            alpha += Time.deltaTime;
            foreach (var image in onTheWayImages)
            {
                color = image.color;
                color.a = alpha;
                image.color = color;
            }
            color = backgroundImage.color;
            color.a = alpha * 0.8f;
            backgroundImage.color = color;
            yield return null;
        }
        //移動速度
        float moveSpeed = playerWinRectTransforms[playerPoints[winPlayerNumber]].position.x -
                            playerRectTransforms[winPlayerNumber].position.x;
        //時間を測る変数
        float timeCount = 0.0f;
        pos = playerRectTransforms[winPlayerNumber].position;
        //勝ったプレイヤーの移動
        while (playerRectTransforms[winPlayerNumber].position.x <
        playerWinRectTransforms[playerPoints[winPlayerNumber]].position.x)
        {
            //煽りのクラクション
            if (SwitchInput.GetButtonDown(winPlayerNumber, SwitchButton.Horn))
            {
                if (hornSound.isPlaying) hornSound.Stop();
                hornSound.Play();
            }
            //移動処理
            timeCount += Time.deltaTime;
            pos.x += moveSpeed * Time.deltaTime / 3;
            playerRectTransforms[winPlayerNumber].position = pos;
            playerRectTransforms[winPlayerNumber].rotation = Quaternion.Euler(0, 0, Mathf.Sin(timeCount * 2) * Mathf.Rad2Deg / 4);
            yield return null;
        }
        playerRectTransforms[winPlayerNumber].rotation = Quaternion.identity;
        yield return new WaitForSeconds(0.5f);
    }

    //ポーズ画面展開
    public void OnPause()
    {
        //時間を止める
        Time.timeScale = 0;

        //ボタンを見えるように
        pauseObject.SetActive(true);
        SelectUpdate();
        if (SwitchInput.GetButtonDown(0, SwitchButton.Pause) && !pausePush)
        {
            pausePush = true;
            isPause = false;
            pauseObject.SetActive(false);
            selectIndex = 0;
            foreach (var obj in uiRectTransforms)
                obj.localScale = initScale;
            scaleTime = 0;
        }
        //ボタンを押したら
        if (SwitchInput.GetButtonDown(0, SwitchButton.Ok) && !pausePush)
        {
            pausePush = true;
            isPause = false;
            switch (selectIndex)
            {
                case 0:
                    pauseObject.SetActive(false);
                    selectIndex = 0;
                    foreach (var obj in uiRectTransforms)
                        obj.localScale = initScale;
                    scaleTime = 0;
                    break;
                case 1:
                    //スタティックなので値を0に
                    playerPoints = new int[4];
                    StartCoroutine(TransitionSelectScene());
                    break;

                case 2:
                    //スタティックなので値を0に
                    playerPoints = new int[4];
                    StartCoroutine(TransitionTitleScene());
                    break;
            }
        }
    }

    void SelectUpdate()
    {
        int prevIndex = selectIndex;
        if (SwitchInput.GetButtonDown(0, SwitchButton.StickDown))
        {
            ++selectIndex;
        }
        else if (SwitchInput.GetButtonDown(0, SwitchButton.StickUp))
        {
            --selectIndex;
        }
        selectIndex = Mathf.Clamp(selectIndex, 0, uiRectTransforms.Length - 1);
        if (prevIndex != selectIndex)
        {
            uiRectTransforms[prevIndex].localScale = initScale;
            SoundManager.Instance.Stick();
            scaleTime = 0.0f;
        }
        scaleTime += Time.unscaledDeltaTime * 6;
        uiRectTransforms[selectIndex].localScale = initScale + (maxScale - initScale) * ((Mathf.Sin(scaleTime) + 1) / 2);
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
