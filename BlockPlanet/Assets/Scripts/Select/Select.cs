using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Select : SingletonMonoBehaviour<Select>
{
    /// <summary>
    /// セレクトマネージャー
    /// </summary>

    [SerializeField]
    SelectChoice CurrentSelectChoice = null;
    RectTransform currentChoiceRectTransform = null;
    const int StageNum = 8;
    Vector3 init_scale = new Vector3();
    Vector3 increment_scale = new Vector3();
    [SerializeField]
    Vector3 max_scale = new Vector3();
    float scale_time = 0.0f;

    [System.NonSerialized]
    private List<GameObject> list = new List<GameObject>();

    private GameObject holdobject;

    public static int stagenumber = 0;
    private bool Push = false;

    BlockMap[] blockMaps = new BlockMap[StageNum];
    [SerializeField]
    GameObject CameraObject;
    [SerializeField]
    GameObject ui;
    [SerializeField]
    Material PostProcess;

    void Start()
    {
        PostProcess.SetFloat("_Strength", 0);
        //フェード
        Fade.Instance.FadeOut(1.0f);
        stagenumber = 0;
        //6個のステージ
        for (int i = 0; i < StageNum; ++i)
        {
            //空のオブジェクト
            GameObject field = new GameObject("field" + (i + 1));
            field.transform.position = new Vector3(25, 0, 25);
            //リストに追加
            list.Add(field);
            blockMaps[i] = new BlockMap();
            BlockCreater.GetInstance().CreateField("Stage" + (i + 1), field.transform, blockMaps[i]);
            blockMaps[i].BlockRendererUpdate();
            blockMaps[i].BlockPhysicsOff();
            //見えなくする
            list[i].SetActive(false);
        }
        stagenumber = CurrentSelectChoice.stageNumber - 1;
        list[stagenumber].SetActive(true);
        currentChoiceRectTransform = CurrentSelectChoice.GetComponent<RectTransform>();
        init_scale = currentChoiceRectTransform.localScale;
    }

    void Update()
    {
        list[stagenumber].transform.Rotate(Vector3.up * Time.deltaTime * 10);

        if (Push) return;
        if (!Fade.Instance.IsEnd) return;
        SelectUpdate();

        //決定
        if (SwitchInput.GetButtonDown(0, SwitchButton.Ok) && !Push)
        {
            Push = true;
            //プッシュの音を鳴らす
            SoundManager.Instance.Push();
            //ロード処理に入る
            StartCoroutine("Loadscene", false);
        }
        else if (SwitchInput.GetButtonDown(0, SwitchButton.Cancel) && !Push)
        {
            Push = true;
            //プッシュの音を鳴らす
            SoundManager.Instance.Push();
            //ロード処理に入る
            StartCoroutine("Loadscene", true);
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
            list[stagenumber].SetActive(false);
            SoundManager.Instance.Stick();
            increment_scale.Set(0, 0, 0);
            scale_time = 0.0f;
            stagenumber = CurrentSelectChoice.stageNumber - 1;
            list[stagenumber].SetActive(true);
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

    private IEnumerator Loadscene(bool next_is_title)
    {
        //次がタイトルかどうか
        if (next_is_title)
        {
            Fade.Instance.FadeIn(1.0f);
            while (!Fade.Instance.IsEnd) yield return null;
            SceneManager.LoadScene("Title");
        }
        else
        {
            ui.SetActive(false);
            Vector3 initPosition = list[stagenumber].transform.position;
            Vector3 endPosition = CameraObject.transform.position;
            endPosition.y = initPosition.y;
            endPosition.z += 15;
            float timeCount = 0.0f;
            while (list[stagenumber].transform.position != endPosition)
            {
                timeCount += Time.deltaTime;
                list[stagenumber].transform.position = Vector3.Lerp(initPosition, endPosition, timeCount);
                yield return null;
            }
            timeCount = 0.0f;
            initPosition = CameraObject.transform.position;
            endPosition = list[stagenumber].transform.position;
            endPosition.y += 10;
            Quaternion initRotation = CameraObject.transform.rotation;
            Quaternion endRotation = Quaternion.Euler(90, 0, 0);
            const float speed = 0.5f;
            //フェード
            Fade.Instance.FadeIn(1.0f / speed);
            while (CameraObject.transform.position != endPosition)
            {
                timeCount += Time.deltaTime * speed;
                PostProcess.SetFloat("_Strength", Mathf.Min(timeCount, 1));
                CameraObject.transform.position = Vector3.Lerp(initPosition, endPosition, timeCount);
                CameraObject.transform.rotation = Quaternion.Slerp(initRotation, endRotation, timeCount);
                yield return null;
            }
            while (!Fade.Instance.IsEnd) yield return null;
            SceneManager.LoadScene("Field");
        }
    }
}
