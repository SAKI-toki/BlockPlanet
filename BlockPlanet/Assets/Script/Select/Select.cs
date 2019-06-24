using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.EventSystems;

public class Select : SingletonMonoBehaviour<Select>
{
    /// <summary>
    /// セレクトマネージャー
    /// </summary>

    [SerializeField]
    RectTransform[] UiRectTransforms = new RectTransform[7];
    Vector2Int select_index = new Vector2Int();
    Vector3 init_scale = new Vector3();
    Vector3 increment_scale = new Vector3();
    [SerializeField]
    Vector3 max_scale = new Vector3();
    float scale_time = 0.0f;

    [SerializeField]
    private List<GameObject> list = new List<GameObject>();

    private GameObject holdobject;

    public static int stagenumber = 1;
    private bool Push = false;

    BlockMap[] blockMaps = new BlockMap[6];

    void Start()
    {
        //フェード
        Fade.Instance.FadeOut(1.0f);
        stagenumber = 0;
        //6個のステージ
        for (int i = 0; i < UiRectTransforms.Length - 1; ++i)
        {
            //空のオブジェクト
            GameObject field = GameObject.Find("field" + (i + 1));
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
        init_scale = UiRectTransforms[0].localScale;
    }

    void Update()
    {
        if (stagenumber != UiRectTransforms.Length - 1)
        {
            //後ろのマップを回転させる
            list[stagenumber].transform.Rotate(0, 5.0f * Time.deltaTime, 0);
        }

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
            StartCoroutine("Loadscene");
        }
    }

    void SelectUpdate()
    {
        Vector2Int prev_index = select_index;
        if (SwitchInput.GetButtonDown(0, SwitchButton.StickRight))
        {
            ++select_index.x;
        }
        else if (SwitchInput.GetButtonDown(0, SwitchButton.StickLeft))
        {
            --select_index.x;
        }
        if (SwitchInput.GetButtonDown(0, SwitchButton.StickDown))
        {
            ++select_index.y;
        }
        else if (SwitchInput.GetButtonDown(0, SwitchButton.StickUp))
        {
            --select_index.y;
        }
        select_index.y = Mathf.Clamp(select_index.y, 0, 2);
        select_index.x = Mathf.Clamp(select_index.x, 0, 2);
        if (select_index.y == 2)
        {
            select_index.x = 0;
        }
        if (select_index.y == 1 && prev_index.y == 2)
        {
            select_index.x = 1;
        }
        if (prev_index != select_index)
        {
            UiRectTransforms[stagenumber].localScale = init_scale;
            if (stagenumber != UiRectTransforms.Length - 1)
            {
                list[stagenumber].SetActive(false);
            }
            SoundManager.Instance.Stick();
            increment_scale.Set(0, 0, 0);
            scale_time = 0.0f;
            stagenumber = select_index.y * 3 + select_index.x;
            if (stagenumber != UiRectTransforms.Length - 1)
            {
                list[stagenumber].SetActive(true);
            }
        }
        scale_time += Time.deltaTime * 6;
        increment_scale = (max_scale - init_scale) * ((Mathf.Sin(scale_time) + 1) / 2);
        UiRectTransforms[stagenumber].localScale = init_scale + increment_scale;
    }

    public static int Stagenum()
    {
        return stagenumber + 1;
    }

    private IEnumerator Loadscene()
    {
        //フェード
        Fade.Instance.FadeIn(1.0f);
        //少し待つ
        yield return new WaitForSeconds(1.0f);
        //シーン遷移
        if (stagenumber == UiRectTransforms.Length - 1)
            SceneManager.LoadScene("Title");
        else
            SceneManager.LoadScene("Field");
    }

}
