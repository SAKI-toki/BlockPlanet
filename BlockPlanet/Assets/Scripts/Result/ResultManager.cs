using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// リザルト画面
/// </summary>
public class ResultManager : SingletonMonoBehaviour<ResultManager>
{
    [SerializeField]
    RectTransform[] uiRectTransforms = new RectTransform[3];
    int selectIndex = 0;
    Vector3 initScale = new Vector3();
    Vector3 incrementScale = new Vector3();
    [SerializeField]
    Vector3 maxScale = new Vector3();
    float scaleTime = 0.0f;
    [SerializeField]
    GameObject[] playerNumberUis = new GameObject[4];
    private bool push = false;
    bool isEndResultAnimation = false;
    [SerializeField]
    GameObject resultBombObject = null;
    //ポイントを格納
    public static int[] resultPoints = new int[4];
    public ResultBlockMeshCombine blockMap = new ResultBlockMeshCombine();
    GameObject[] players;
    [SerializeField]
    GameObject uiCanvas = null;
    [SerializeField]
    ParticleSystem[] winEffects;
    [SerializeField]
    GameObject cameraObject = null;
    [SerializeField]
    Transform playerEndTransform = null;

#if UNITY_EDITOR
    [SerializeField]
    bool isDebug = false;
    [SerializeField]
    int winPlayerDebug = 0;
#endif
    GameObject fieldObjectParent;
    void Start()
    {
#if UNITY_EDITOR
        if (isDebug)
        {
            resultPoints[winPlayerDebug] = int.MaxValue;
        }
#endif
        fieldObjectParent = new GameObject("FieldObjectTemp");
        //マップ生成
        BlockCreater.GetInstance().CreateField("Result", fieldObjectParent.transform, blockMap, cameraObject, BlockCreater.SceneEnum.Result);
        fieldObjectParent.isStatic = true;
        blockMap.BlockIsSurroundUpdate();
        blockMap.BlockRendererOff();
        GameObject parent = new GameObject("FieldObject");
        blockMap.Initialize(parent);
        players = new GameObject[4];
        //プレイヤーを取得
        for (int i = 0; i < players.Length; ++i)
        {
            //参加していなかったら何もしない
            if (!BlockCreater.GetInstance().isPlays[i]) continue;
            players[i] = GameObject.FindGameObjectWithTag("Player" + (i + 1).ToString());
            players[i].GetComponent<Player>().enabled = false;
        }
        //プレイヤーを地面につける
        Physics.autoSimulation = false;
        Physics.Simulate(10.0f);
        Physics.autoSimulation = true;
        //UIの表示
        foreach (var ui in playerNumberUis)
            ui.SetActive(false);
        uiCanvas.SetActive(false);
        initScale = uiRectTransforms[0].localScale;
        //リザルトのアニメーション開始
        StartCoroutine(ResultAnimation());
    }

    void Update()
    {
        blockMap.CreateMesh();
        if (push) return;
        if (!Fade.Instance.IsEnd) return;
        if (!isEndResultAnimation) return;
        SelectUpdate();
        if (SwitchInput.GetButtonDown(0, SwitchButton.Ok) && !push)
        {
            push = true;
            string sceneName = "";
            switch (selectIndex)
            {
                case 0:
                    sceneName = "Field";
                    break;
                case 1:
                    sceneName = "Select";
                    break;
                case 2:
                    sceneName = "Title";
                    break;
            }
            //フェード開始
            StartCoroutine(Loadscene(sceneName));
            //プッシュの音を鳴らす
            SoundManager.Instance.Push();
        }
    }
    void SelectUpdate()
    {
        int prevIndex = selectIndex;
        if (SwitchInput.GetButtonDown(0, SwitchButton.StickRight))
        {
            ++selectIndex;
        }
        else if (SwitchInput.GetButtonDown(0, SwitchButton.StickLeft))
        {
            --selectIndex;
        }
        selectIndex = Mathf.Clamp(selectIndex, 0, uiRectTransforms.Length - 1);
        if (prevIndex != selectIndex)
        {
            uiRectTransforms[prevIndex].localScale = initScale;
            SoundManager.Instance.Stick();
            incrementScale.Set(0, 0, 0);
            scaleTime = 0.0f;
        }
        scaleTime += Time.deltaTime * 6;
        incrementScale = (maxScale - initScale) * ((Mathf.Sin(scaleTime) + 1) / 2);
        uiRectTransforms[selectIndex].localScale = initScale + incrementScale;
    }

    private IEnumerator Loadscene(string sceneName)
    {
        Fade.Instance.FadeIn(1.0f);
        while (!Fade.Instance.IsEnd) yield return null;
        SceneManager.LoadScene(sceneName);
    }

    IEnumerator ResultAnimation()
    {
        //フェード
        Fade.Instance.FadeOut(1.0f);
        while (!Fade.Instance.IsEnd) yield return null;
        bool[] isEnd = new bool[4];
        int playerNum = 0;
        for (int i = 0; i < isEnd.Length; ++i)
        {
            //参加していなかったら既に終了しておく
            isEnd[i] = !BlockCreater.GetInstance().isPlays[i];
            //プレイ人数の加算
            if (BlockCreater.GetInstance().isPlays[i]) ++playerNum;
        }
        //負けたプレイヤーを順位の低い順番に爆弾で落としていく
        for (int i = 0; i < playerNum - 1; ++i)
        {
            int minPoint = int.MaxValue;
            int minPlayer = int.MaxValue;
            //一番点数が低いプレイヤーを探す
            for (int j = 0; j < isEnd.Length; ++j)
            {
                if (isEnd[j]) continue;
                if (resultPoints[j] < minPoint)
                {
                    minPoint = resultPoints[j];
                    minPlayer = j;
                }
            }
            isEnd[minPlayer] = true;
            yield return StartCoroutine(BombAnimation(minPlayer));
        }
        int winPlayerNumber = 0;
        //勝ったプレイヤーの番号を探す
        for (int i = 0; i < isEnd.Length; ++i)
        {
            if (!isEnd[i])
            {
                winPlayerNumber = i;
                break;
            }
        }
        var resultWinPlayerAnimation = players[winPlayerNumber].AddComponent<ResultWinPlayerAnimation>();
        fieldObjectParent.SetActive(false);
        //勝ったプレイヤーのアニメーション
        yield return StartCoroutine(resultWinPlayerAnimation.WinPlayerAnimation(winPlayerNumber));
        float time = 0;
        Vector3 InitPosition = players[winPlayerNumber].transform.position;
        Quaternion InitRotation = players[winPlayerNumber].transform.rotation;
        while (time <= 1.0f)
        {
            time += Time.deltaTime;
            //終了位置に移動する
            players[winPlayerNumber].transform.SetPositionAndRotation
                (Vector3.Lerp(InitPosition, playerEndTransform.position, time),
                Quaternion.Slerp(InitRotation, playerEndTransform.rotation, time));
            yield return null;
        }
        //UIの表示
        uiCanvas.SetActive(true);
        playerNumberUis[winPlayerNumber].SetActive(true);
        //エフェクトの開始
        foreach (var effect in winEffects)
        {
            effect.Play();
        }
        isEndResultAnimation = true;
    }

    /// <summary>
    /// 爆弾が降ってくるアニメーション
    /// </summary>
    /// <param name="index">降らせるプレイヤーのインデックス</param>
    IEnumerator BombAnimation(int index)
    {
        Vector3 pos = players[index].transform.position;
        //爆弾の高さ
        pos.y = 20;
        GameObject bomb = Instantiate(resultBombObject, pos, Quaternion.identity);
        while (bomb != null) yield return null;
        Destroy(players[index], 3);
    }
}
