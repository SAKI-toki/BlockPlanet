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
    const int StageNum = 6;
    Vector2Int select_index = new Vector2Int();
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

    BlockMap[] blockMaps = new BlockMap[6];


    void Start()
    {
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
        list[0].SetActive(true);
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
        //フェード
        Fade.Instance.FadeIn(1.0f);
        //少し待つ
        yield return new WaitForSeconds(1.0f);
        //シーン遷移
        if (next_is_title)
            SceneManager.LoadScene("Title");
        else
        {
            SceneManager.LoadScene("Field");
        }
    }

}
