using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// セレクトマネージャー
/// </summary>
public class Select : SingletonMonoBehaviour<Select>
{
    [SerializeField]
    SelectChoice currentSelectChoice = null;
    RectTransform currentChoiceRectTransform = null;
    Vector3 initScale = new Vector3();
    Vector3 incrementScale = new Vector3();
    [SerializeField]
    Vector3 maxScale = new Vector3();
    float scaleTime = 0.0f;

    [SerializeField]
    List<GameObject> fieldList = new List<GameObject>();
    List<GameObject> instanceFieldList = new List<GameObject>();

    public static int stagenumber = 0;

    [SerializeField]
    GameObject cameraObject;
    [SerializeField]
    GameObject ui;
    [SerializeField]
    Material postProcessMaterial;
    [SerializeField]
    PostProcess postProcess;
    [SerializeField]
    Material[] mats;
    [SerializeField]
    GameObject selectUIparent;
    [SerializeField]
    GameObject descriptionUIparent;
    delegate void StateType();
    StateType state;

    void Start()
    {
        BgmManager.Instance.Play(BgmEnum.STAGE_SELECT);
        descriptionUIparent.SetActive(false);
        //RadialBlurの強さを0にする
        postProcessMaterial.SetFloat("_Strength", 0);
        //ポストプロセスをOffにする
        postProcess.enabled = false;
        //フェード
        Fade.Instance.FadeOut(1.0f);
        stagenumber = currentSelectChoice.number - 1;
        //フィールドの生成
        for (int i = 0; i < 8; ++i)
        {
            //生成したものをリストに追加
            instanceFieldList.Add(Instantiate(fieldList[i], new Vector3(25, 0, 25), Quaternion.identity));
            foreach (var renderer in instanceFieldList[i].transform.GetComponentsInChildren<Renderer>())
            {
                //マテリアルのセット
                foreach (var mat in mats)
                {
                    if (renderer.transform.name == mat.name)
                    {
                        renderer.sharedMaterial = mat;
                        break;
                    }
                }
            }
            instanceFieldList[i].SetActive(false);
        }
        instanceFieldList[stagenumber].SetActive(true);
        currentChoiceRectTransform = currentSelectChoice.GetComponent<RectTransform>();
        initScale = currentChoiceRectTransform.localScale;
        state = SelectFieldState;
    }

    void Update()
    {
        instanceFieldList[stagenumber].transform.Rotate(Vector3.up * Time.deltaTime * 10);

        if (!Fade.Instance.IsEnd) return;

        if (state != null) state();
    }

    /// <summary>
    /// マップを選択するステート
    /// </summary>
    void SelectFieldState()
    {
        SelectUpdate();
        //決定
        if (SwitchInput.GetButtonDown(0, SwitchButton.Ok))
        {
            ui.SetActive(false);
            //プッシュの音を鳴らす
            SoundManager.Instance.Push();
            //ロード処理に入る
            StartCoroutine(LoadFieldScene());
            state = null;
        }
        //前のシーンに戻る
        else if (SwitchInput.GetButtonDown(0, SwitchButton.Cancel))
        {
            //プッシュの音を鳴らす
            SoundManager.Instance.Push();
            //ロード処理に入る
            StartCoroutine(LoadPrevScene());
            state = null;
        }
        //説明を表示
        else if (SwitchInput.GetButtonDown(0, SwitchButton.Pause))
        {
            selectUIparent.SetActive(false);
            instanceFieldList[stagenumber].SetActive(false);
            descriptionUIparent.SetActive(true);
            //プッシュの音を鳴らす
            SoundManager.Instance.Push();
            state = DescriptState;
        }
    }

    /// <summary>
    /// 説明を表示するステート
    /// </summary>
    void DescriptState()
    {
        //説明を非表示
        if (SwitchInput.GetButtonDown(0, SwitchButton.Pause) ||
            SwitchInput.GetButtonDown(0, SwitchButton.Down))
        {
            selectUIparent.SetActive(true);
            instanceFieldList[stagenumber].SetActive(true);
            descriptionUIparent.SetActive(false);
            //プッシュの音を鳴らす
            SoundManager.Instance.Push();
            state = SelectFieldState;
        }
    }

    /// <summary>
    /// セレクトの更新
    /// </summary>
    void SelectUpdate()
    {
        var prev = currentSelectChoice;
        if (SwitchInput.GetButtonDown(0, SwitchButton.StickRight))
        {
            if (currentSelectChoice.Right)
            {
                currentSelectChoice = currentSelectChoice.Right;
            }
        }
        if (SwitchInput.GetButtonDown(0, SwitchButton.StickLeft))
        {
            if (currentSelectChoice.Left)
            {
                currentSelectChoice = currentSelectChoice.Left;
            }
        }
        if (SwitchInput.GetButtonDown(0, SwitchButton.StickDown))
        {
            if (currentSelectChoice.Down)
            {
                currentSelectChoice = currentSelectChoice.Down;
            }
        }
        if (SwitchInput.GetButtonDown(0, SwitchButton.StickUp))
        {
            if (currentSelectChoice.Up)
            {
                currentSelectChoice = currentSelectChoice.Up;
            }
        }
        //カーソルが移動したら
        if (prev != currentSelectChoice)
        {
            currentChoiceRectTransform.localScale = initScale;
            instanceFieldList[stagenumber].SetActive(false);
            //スティックの音
            SoundManager.Instance.Stick();
            incrementScale.Set(0, 0, 0);
            scaleTime = 0.0f;
            stagenumber = currentSelectChoice.number - 1;
            //アクティブにするフィールドの変更
            instanceFieldList[stagenumber].SetActive(true);
            currentChoiceRectTransform = currentSelectChoice.GetComponent<RectTransform>();
        }
        scaleTime += Time.deltaTime * 6;
        incrementScale = (maxScale - initScale) * ((Mathf.Sin(scaleTime) + 1) / 2);
        currentChoiceRectTransform.localScale = initScale + incrementScale;
    }

    public static int Stagenum()
    {
        return stagenumber + 1;
    }

    /// <summary>
    /// 前のシーンの読み込み
    /// </summary>
    IEnumerator LoadPrevScene()
    {
        Fade.Instance.FadeIn(1.0f);
        while (!Fade.Instance.IsEnd) yield return null;
        SceneManager.LoadScene("PlayerNumberSelect");
    }

    IEnumerator LoadFieldScene()
    {
        //横に移動
        Vector3 initPosition = instanceFieldList[stagenumber].transform.position;
        Vector3 endPosition = cameraObject.transform.position;
        endPosition.y = initPosition.y;
        endPosition.z += 15;
        float timeCount = 0.0f;
        //移動処理
        while (instanceFieldList[stagenumber].transform.position != endPosition)
        {
            timeCount += Time.deltaTime;
            instanceFieldList[stagenumber].transform.position =
                Vector3.Lerp(initPosition, endPosition, timeCount);
            yield return null;
        }
        //ステージに入り込むようなアニメーション
        timeCount = 0.0f;
        initPosition = cameraObject.transform.position;
        endPosition = instanceFieldList[stagenumber].transform.position;
        endPosition.y += 10;
        Quaternion initRotation = cameraObject.transform.rotation;
        Quaternion endRotation = Quaternion.Euler(90, 0, 0);
        //フェードのスピード
        const float FadeSpeed = 0.5f;
        //フェード
        Fade.Instance.FadeIn(1.0f / FadeSpeed);
        postProcess.enabled = true;
        //移動処理
        while (cameraObject.transform.position != endPosition)
        {
            timeCount += Time.deltaTime * FadeSpeed;
            //ポストポロセスを効かせる
            postProcessMaterial.SetFloat("_Strength", Mathf.Min(timeCount, 1));
            cameraObject.transform.position = Vector3.Lerp(initPosition, endPosition, timeCount);
            cameraObject.transform.rotation = Quaternion.Slerp(initRotation, endRotation, timeCount);
            yield return null;
        }
        while (!Fade.Instance.IsEnd) yield return null;
        SceneManager.LoadScene("Field");
    }
}
