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
    public bool[] playerGameOvers = new bool[4];
    //プレイヤーのポイント。スタティックにしないといけない
    static public int[] playerPoints = new int[4];
    //勝利ポイント
    const int WinPoint = 3;
    int winPlayerNumber = 0;
    //ゲームオーバーに一回だけ通る
    [System.NonSerialized]
    public bool isGameOver = false;
    //ボタンを押す
    bool pausePush = false;
    //ポーズの表示非表示を管理
    [System.NonSerialized]
    public bool isPause = false;
    //BGM
    AudioSource bgmSound = null;
    //カウントダウン
    [SerializeField]
    List<GameObject> countDownImage = new List<GameObject>();
    [SerializeField]
    GameObject gameSetImage;
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
    public Player[] players = new Player[4];

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
        //プレイしないプレイヤーの途中経過の画像を差し替える
        for (int i = 0; i < playerImages.Length; ++i)
        {
            if (BlockCreater.GetInstance().isPlays[i]) continue;
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
        //生き残っているプレイヤーの数
        int surviveCount = 0;
        //生き残っているプレイヤーの数を計算
        for (int i = 0; i < playerGameOvers.Length; ++i)
        {
            if (!BlockCreater.GetInstance().isPlays[i]) continue;
            if (!playerGameOvers[i])
            {
                winPlayerNumber = i;
                ++surviveCount;
            }
        }
        //生き残っているプレイヤーの数が1以下になったら終了
        if (surviveCount <= 1)
        {
            //ポーズ解除
            isPause = false;
            isGameOver = true;
            //勝者決定
            if (surviveCount == 1)
            {
                ++playerPoints[winPlayerNumber];
            }
            //勝利ポイントに達したかどうか
            if (playerPoints[winPlayerNumber] == WinPoint)
            {
                StartCoroutine(GameOver());
            }
            else
            {
                //引き分けの場合、生き残っているプレイヤーの数が0になる
                StartCoroutine(Restart(surviveCount == 0));
            }
        }
        //ポーズ中
        else if (isPause)
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

    /// <summary>
    /// ゲーム開始時のコルーチン
    /// </summary>
    IEnumerator Gamestart()
    {
        //フェード開始
        Fade.Instance.FadeOut(1.0f);
        //少し待つ
        while (!Fade.Instance.IsEnd) yield return null;
        //カウントダウンの音
        SoundManager.Instance.GameStart();
        //カウントダウン開始
        for (int count = countDownImage.Count - 1; count >= 0; count--)
        {
            //表示
            countDownImage[count].SetActive(true);
            //1秒待つ
            yield return new WaitForSeconds(1.0f);
            //消す
            countDownImage[count].SetActive(false);
        }
        //プレイヤーを動けるようにする
        for (int i = 0; i < players.Length; ++i)
        {
            if (!BlockCreater.GetInstance().isPlays[i]) continue;
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

    /// <summary>
    /// まだ決着がついていないとき
    /// </summary>
    /// <param name="isDraw">引き分けかどうか</param>
    IEnumerator Restart(bool isDraw = false)
    {
        //BGMを停止する
        bgmSound.Stop();
        //ゲーム終了のSE
        SoundManager.Instance.GameOver();
        //少し待つ
        yield return new WaitForSeconds(1);
        //引き分けじゃない
        if (!isDraw)
        {
            //途中経過を表示
            yield return StartCoroutine(OnTheWayCoroutine());
        }
        //フェード開始
        Fade.Instance.FadeIn(1.0f);
        //少し待つ
        while (!Fade.Instance.IsEnd) yield return null;
        //リザルト画面に遷移
        SceneManager.LoadScene("Field");
    }

    /// <summary>
    /// 決着がついたとき
    /// </summary>
    IEnumerator GameOver()
    {
        //表示
        gameSetImage.SetActive(true);
        //BGMを停止する
        bgmSound.Stop();
        //ゲーム終了のSE
        SoundManager.Instance.GameOver();
        //少し待つ
        yield return new WaitForSeconds(1);
        //途中経過を表示
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
    IEnumerator OnTheWayCoroutine()
    {
        //途中経過のUIを表示
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
        //回転を中途半端に止めず、0にする
        playerRectTransforms[winPlayerNumber].rotation = Quaternion.identity;
        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>
    /// ポーズ
    /// </summary>
    public void OnPause()
    {
        //時間を止める
        Time.timeScale = 0;

        //ボタンを見えるように
        pauseObject.SetActive(true);
        SelectUpdate();
        //ポーズボタンを押したら
        if (SwitchInput.GetButtonDown(0, SwitchButton.Pause) && !pausePush)
        {
            SoundManager.Instance.Push();
            pausePush = true;
            isPause = false;
            pauseObject.SetActive(false);
            selectIndex = 0;
            foreach (var obj in uiRectTransforms)
                obj.localScale = initScale;
            scaleTime = 0;
        }
        //決定ボタンを押したら
        if (SwitchInput.GetButtonDown(0, SwitchButton.Ok) && !pausePush)
        {
            pausePush = true;
            isPause = false;
            //決定音
            SoundManager.Instance.Push();
            switch (selectIndex)
            {
                //BACK
                case 0:
                    {
                        pauseObject.SetActive(false);
                        selectIndex = 0;
                        foreach (var obj in uiRectTransforms)
                            obj.localScale = initScale;
                        scaleTime = 0;
                    }
                    break;
                //SELECT
                case 1:
                    {
                        //スタティックなので値を0に
                        playerPoints = new int[4];
                        StartCoroutine(TransitionSelectScene());
                    }
                    break;
                //TITLE
                case 2:
                    {
                        //スタティックなので値を0に
                        playerPoints = new int[4];
                        StartCoroutine(TransitionTitleScene());
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// カーソルの移動処理
    /// </summary>
    void SelectUpdate()
    {
        int prevIndex = selectIndex;
        //選んでいるものを変更
        if (SwitchInput.GetButtonDown(0, SwitchButton.StickDown))
        {
            ++selectIndex;
        }
        else if (SwitchInput.GetButtonDown(0, SwitchButton.StickUp))
        {
            --selectIndex;
        }
        //範囲外にならないようにClamp
        selectIndex = Mathf.Clamp(selectIndex, 0, uiRectTransforms.Length - 1);
        //選んでいるものが変わったら(移動したら)
        if (prevIndex != selectIndex)
        {
            uiRectTransforms[prevIndex].localScale = initScale;
            //スティック音を鳴らす
            SoundManager.Instance.Stick();
            scaleTime = 0.0f;
        }
        scaleTime += Time.unscaledDeltaTime * 6;
        //サインカーブで拡縮のアニメーション
        uiRectTransforms[selectIndex].localScale =
            initScale + (maxScale - initScale) * ((Mathf.Sin(scaleTime) + 1) / 2);
    }

    /// <summary>
    /// ポーズ終了
    /// </summary>
    public void OnUnPause()
    {
        Time.timeScale = 1;
    }

    /// <summary>
    /// セレクトシーンに遷移
    /// </summary>
    IEnumerator TransitionSelectScene()
    {
        //フェード開始
        Fade.Instance.FadeIn(1.0f);
        //少し待つ
        while (!Fade.Instance.IsEnd) yield return null;
        //セレクト画面に遷移
        SceneManager.LoadScene("Select");
    }

    /// <summary>
    /// タイトルシーンに遷移
    /// </summary>
    IEnumerator TransitionTitleScene()
    {
        //フェード開始
        Fade.Instance.FadeIn(1.0f);
        //少し待つ
        while (!Fade.Instance.IsEnd) yield return null;
        //タイトル画面に遷移
        SceneManager.LoadScene("Title");
    }
}
