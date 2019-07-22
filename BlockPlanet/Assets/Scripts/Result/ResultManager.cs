using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// リザルト画面
/// </summary>
public class ResultManager : SingletonMonoBehaviour<ResultManager>
{
    [SerializeField]
    RectTransform[] UiRectTransforms = new RectTransform[3];
    int select_index = 0;
    Vector3 init_scale = new Vector3();
    Vector3 increment_scale = new Vector3();
    [SerializeField]
    Vector3 max_scale = new Vector3();
    float scale_time = 0.0f;
    [SerializeField]
    GameObject[] Uis = new GameObject[4];
    private bool Push = false;
    bool IsEndResultAnimation = false;
    [SerializeField]
    GameObject ResultBombObject = null;
    //ポイントを格納
    public static int[] ResultPoints = new int[4];
    public ResultBlockMeshCombine blockMap = new ResultBlockMeshCombine();
    GameObject[] players;
    [SerializeField]
    GameObject uiCanvas = null;
    [SerializeField]
    ParticleSystem[] winEffects;
    [SerializeField]
    GameObject cameraObject = null;
    [SerializeField]
    Transform PlayerEndTransform = null;

#if UNITY_EDITOR
    [SerializeField]
    bool IsDebug = false;
    [SerializeField]
    int winPlayerDebug = 0;
#endif
    GameObject fieldObjectParent;
    void Start()
    {
#if UNITY_EDITOR
        if (IsDebug)
        {
            ResultPoints[winPlayerDebug] = int.MaxValue;
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
        players = new GameObject[BlockCreater.GetInstance().maxPlayerNumber];
        //プレイヤーを取得
        for (int i = 0; i < players.Length; ++i)
        {
            players[i] = GameObject.FindGameObjectWithTag("Player" + (i + 1).ToString());
            players[i].GetComponent<Player>().enabled = false;
        }
        //プレイヤーを地面につける
        Physics.autoSimulation = false;
        Physics.Simulate(10.0f);
        Physics.autoSimulation = true;
        //UIの表示
        foreach (var ui in Uis)
            ui.SetActive(false);
        uiCanvas.SetActive(false);
        init_scale = UiRectTransforms[0].localScale;
        //リザルトのアニメーション開始
        StartCoroutine(ResultAnimation());
    }

    void Update()
    {
        blockMap.CreateMesh();
        if (Push) return;
        if (!Fade.Instance.IsEnd) return;
        if (!IsEndResultAnimation) return;
        SelectUpdate();
        if (SwitchInput.GetButtonDown(0, SwitchButton.Ok) && !Push)
        {
            Push = true;
            string SceneName = "";
            switch (select_index)
            {
                case 0:
                    SceneName = "Field";
                    break;
                case 1:
                    SceneName = "Select";
                    break;
                case 2:
                    SceneName = "Title";
                    break;
            }
            //フェード開始
            StartCoroutine(Loadscene(SceneName));
            //プッシュの音を鳴らす
            SoundManager.Instance.Push();
        }
    }
    void SelectUpdate()
    {
        int prev_index = select_index;
        if (SwitchInput.GetButtonDown(0, SwitchButton.StickRight))
        {
            ++select_index;
        }
        else if (SwitchInput.GetButtonDown(0, SwitchButton.StickLeft))
        {
            --select_index;
        }
        select_index = Mathf.Clamp(select_index, 0, UiRectTransforms.Length - 1);
        if (prev_index != select_index)
        {
            UiRectTransforms[prev_index].localScale = init_scale;
            SoundManager.Instance.Stick();
            increment_scale.Set(0, 0, 0);
            scale_time = 0.0f;
        }
        scale_time += Time.deltaTime * 6;
        increment_scale = (max_scale - init_scale) * ((Mathf.Sin(scale_time) + 1) / 2);
        UiRectTransforms[select_index].localScale = init_scale + increment_scale;
    }

    private IEnumerator Loadscene(string SceneName)
    {
        Fade.Instance.FadeIn(1.0f);
        while (!Fade.Instance.IsEnd) yield return null;
        SceneManager.LoadScene(SceneName);
    }

    IEnumerator ResultAnimation()
    {
        //フェード
        Fade.Instance.FadeOut(1.0f);
        while (!Fade.Instance.IsEnd) yield return null;
        bool[] isEnd = new bool[BlockCreater.GetInstance().maxPlayerNumber];
        //負けたプレイヤーを順位の低い順番に爆弾で落としていく
        for (int i = 0; i < isEnd.Length - 1; ++i)
        {
            int minPoint = int.MaxValue;
            int minPlayer = int.MaxValue;
            for (int j = 0; j < isEnd.Length; ++j)
            {
                if (isEnd[j]) continue;
                if (ResultPoints[j] < minPoint)
                {
                    minPoint = ResultPoints[j];
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
                (Vector3.Lerp(InitPosition, PlayerEndTransform.position, time),
                Quaternion.Slerp(InitRotation, PlayerEndTransform.rotation, time));
            yield return null;
        }
        //UIの表示
        uiCanvas.SetActive(true);
        Uis[winPlayerNumber].SetActive(true);
        //エフェクトの開始
        foreach (var effect in winEffects)
        {
            effect.Play();
        }
        IsEndResultAnimation = true;
    }

    IEnumerator BombAnimation(int index)
    {
        Vector3 pos = players[index].transform.position;
        //爆弾の高さ
        pos.y = 20;
        GameObject bomb = Instantiate(ResultBombObject, pos, Quaternion.identity);
        while (bomb != null) yield return null;
        Destroy(players[index], 3);
    }
}
