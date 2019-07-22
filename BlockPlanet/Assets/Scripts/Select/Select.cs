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
    SelectChoice CurrentSelectChoice = null;
    RectTransform currentChoiceRectTransform = null;
    Vector3 init_scale = new Vector3();
    Vector3 increment_scale = new Vector3();
    [SerializeField]
    Vector3 max_scale = new Vector3();
    float scale_time = 0.0f;

    [SerializeField]
    List<GameObject> fieldList = new List<GameObject>();
    List<GameObject> instanceFieldList = new List<GameObject>();

    public static int stagenumber = 0;

    [SerializeField]
    GameObject CameraObject;
    [SerializeField]
    GameObject ui;
    [SerializeField]
    Material PostProcessMaterial;
    [SerializeField]
    PostProcess postProcess;
    [SerializeField]
    Material[] mats;
    [SerializeField]
    GameObject descriptionUIparent;
    [SerializeField]
    GameObject[] playerNumberUIs;
    delegate void StateType();
    StateType state;
    void Start()
    {
        descriptionUIparent.SetActive(false);
        foreach (var playerNumUi in playerNumberUIs) playerNumUi.SetActive(false);
        PostProcessMaterial.SetFloat("_Strength", 0);
        postProcess.enabled = false;
        //フェード
        Fade.Instance.FadeOut(1.0f);
        stagenumber = CurrentSelectChoice.number - 1;
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
        currentChoiceRectTransform = CurrentSelectChoice.GetComponent<RectTransform>();
        init_scale = currentChoiceRectTransform.localScale;
        state = SelectFieldState;
    }

    void Update()
    {
        instanceFieldList[stagenumber].transform.Rotate(Vector3.up * Time.deltaTime * 10);

        if (!Fade.Instance.IsEnd) return;

        if (state != null) state();
    }

    void SelectFieldState()
    {
        SelectUpdate();
        //決定
        if (SwitchInput.GetButtonDown(0, SwitchButton.Ok))
        {
            ui.SetActive(false);
            //プッシュの音を鳴らす
            SoundManager.Instance.Push();
            state = PlayerNumberChoiceState;
            BlockCreater.GetInstance().maxPlayerNumber = 2;
            playerNumberUIs[BlockCreater.GetInstance().maxPlayerNumber - 2].SetActive(true);
        }
        //タイトルに戻る
        else if (SwitchInput.GetButtonDown(0, SwitchButton.Cancel))
        {
            //プッシュの音を鳴らす
            SoundManager.Instance.Push();
            //ロード処理に入る
            StartCoroutine(LoadTitleScene());
            state = null;
        }
        //説明を表示
        else if (SwitchInput.GetButtonDown(0, SwitchButton.Pause))
        {
            descriptionUIparent.SetActive(true);
            //プッシュの音を鳴らす
            SoundManager.Instance.Push();
            state = DescriptState;
        }
    }

    void DescriptState()
    {
        //説明を非表示
        if (SwitchInput.GetButtonDown(0, SwitchButton.Pause) || SwitchInput.GetButtonDown(0, SwitchButton.Down))
        {
            descriptionUIparent.SetActive(false);
            //プッシュの音を鳴らす
            SoundManager.Instance.Push();
            state = SelectFieldState;
        }
    }

    void PlayerNumberChoiceState()
    {
        int maxPlayerNumber = BlockCreater.GetInstance().maxPlayerNumber;
        if (SwitchInput.GetButtonDown(0, SwitchButton.Ok))
        {
            //プッシュの音を鳴らす
            SoundManager.Instance.Push();
            //ロード処理に入る
            StartCoroutine(LoadFieldScene());
            playerNumberUIs[maxPlayerNumber - 2].SetActive(false);
        }
        else if (SwitchInput.GetButtonDown(0, SwitchButton.Cancel))
        {
            //プッシュの音を鳴らす
            SoundManager.Instance.Push();
            ui.SetActive(true);
            state = SelectFieldState;
            playerNumberUIs[maxPlayerNumber - 2].SetActive(false);
        }
        else if (SwitchInput.GetButtonDown(0, SwitchButton.StickUp))
        {
            ++maxPlayerNumber;
        }
        else if (SwitchInput.GetButtonDown(0, SwitchButton.StickDown))
        {
            --maxPlayerNumber;
        }
        maxPlayerNumber = Mathf.Clamp(maxPlayerNumber, 2, 4);
        if (maxPlayerNumber != BlockCreater.GetInstance().maxPlayerNumber)
        {
            playerNumberUIs[BlockCreater.GetInstance().maxPlayerNumber - 2].SetActive(false);
            playerNumberUIs[maxPlayerNumber - 2].SetActive(true);
            //スティックの音
            SoundManager.Instance.Stick();

            BlockCreater.GetInstance().maxPlayerNumber = maxPlayerNumber;
        }
    }

    void SelectUpdate()
    {
        var prev = CurrentSelectChoice;
        if (SwitchInput.GetButtonDown(0, SwitchButton.StickRight))
        {
            if (CurrentSelectChoice.Right)
            {
                CurrentSelectChoice = CurrentSelectChoice.Right;
            }
        }
        if (SwitchInput.GetButtonDown(0, SwitchButton.StickLeft))
        {
            if (CurrentSelectChoice.Left)
            {
                CurrentSelectChoice = CurrentSelectChoice.Left;
            }
        }
        if (SwitchInput.GetButtonDown(0, SwitchButton.StickDown))
        {
            if (CurrentSelectChoice.Down)
            {
                CurrentSelectChoice = CurrentSelectChoice.Down;
            }
        }
        if (SwitchInput.GetButtonDown(0, SwitchButton.StickUp))
        {
            if (CurrentSelectChoice.Up)
            {
                CurrentSelectChoice = CurrentSelectChoice.Up;
            }
        }
        //カーソルが移動したら
        if (prev != CurrentSelectChoice)
        {
            currentChoiceRectTransform.localScale = init_scale;
            instanceFieldList[stagenumber].SetActive(false);
            //スティックの音
            SoundManager.Instance.Stick();
            increment_scale.Set(0, 0, 0);
            scale_time = 0.0f;
            stagenumber = CurrentSelectChoice.number - 1;
            //アクティブにするフィールドの変更
            instanceFieldList[stagenumber].SetActive(true);
            currentChoiceRectTransform = CurrentSelectChoice.GetComponent<RectTransform>();
        }
        scale_time += Time.deltaTime * 6;
        increment_scale = (max_scale - init_scale) * ((Mathf.Sin(scale_time) + 1) / 2);
        currentChoiceRectTransform.localScale = init_scale + increment_scale;
    }

    public static int Stagenum()
    {
        return stagenumber + 1;
    }

    /// <summary>
    /// タイトルシーンの読み込み
    /// </summary>
    /// <returns></returns>
    IEnumerator LoadTitleScene()
    {
        Fade.Instance.FadeIn(1.0f);
        while (!Fade.Instance.IsEnd) yield return null;
        SceneManager.LoadScene("Title");
    }

    IEnumerator LoadFieldScene()
    {
        //横に移動
        Vector3 initPosition = instanceFieldList[stagenumber].transform.position;
        Vector3 endPosition = CameraObject.transform.position;
        endPosition.y = initPosition.y;
        endPosition.z += 15;
        float timeCount = 0.0f;
        //移動処理
        while (instanceFieldList[stagenumber].transform.position != endPosition)
        {
            timeCount += Time.deltaTime;
            instanceFieldList[stagenumber].transform.position = Vector3.Lerp(initPosition, endPosition, timeCount);
            yield return null;
        }
        //ステージに入り込むようなアニメーション
        timeCount = 0.0f;
        initPosition = CameraObject.transform.position;
        endPosition = instanceFieldList[stagenumber].transform.position;
        endPosition.y += 10;
        Quaternion initRotation = CameraObject.transform.rotation;
        Quaternion endRotation = Quaternion.Euler(90, 0, 0);
        const float fadeSpeed = 0.5f;
        //フェード
        Fade.Instance.FadeIn(1.0f / fadeSpeed);
        postProcess.enabled = true;
        //移動処理
        while (CameraObject.transform.position != endPosition)
        {
            timeCount += Time.deltaTime * fadeSpeed;
            //ポストポロセスを効かせる
            PostProcessMaterial.SetFloat("_Strength", Mathf.Min(timeCount, 1));
            CameraObject.transform.position = Vector3.Lerp(initPosition, endPosition, timeCount);
            CameraObject.transform.rotation = Quaternion.Slerp(initRotation, endRotation, timeCount);
            yield return null;
        }
        while (!Fade.Instance.IsEnd) yield return null;
        SceneManager.LoadScene("Field");
    }
}
