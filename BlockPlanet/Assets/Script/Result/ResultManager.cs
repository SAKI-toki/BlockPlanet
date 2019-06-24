using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ResultManager : MonoBehaviour
{

    /// <summary>
    /// リザルト画面
    /// </summary>


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
    [SerializeField]
    GameObject[] PlayerWins = new GameObject[4];
    [SerializeField]
    GameObject[] PlayerLoses = new GameObject[4];
    private bool Push = false;

    [SerializeField]
    ResultBomb[] Bombs = new ResultBomb[3];


    void Start()
    {
        foreach (var player in PlayerWins)
            player.SetActive(false);
        foreach (var player in PlayerLoses)
            player.SetActive(false);
        foreach (var ui in Uis)
            ui.SetActive(false);
        //フェード
        Fade.Instance.FadeOut(1.0f);
        int loseIndex = 0;
        //変数を持って来る
        for (int i = 0; i < 4; ++i)
        {
            if (i == FieldManeger.WinPlayerNumber)
            {
                PlayerWins[i].SetActive(true);
                Uis[i].SetActive(true);
            }
            else
            {
                var bombPos = Bombs[loseIndex++].transform.position;
                PlayerLoses[i].transform.position = new Vector3(bombPos.x, PlayerLoses[i].transform.position.y, bombPos.z);
                PlayerLoses[i].SetActive(true);
            }
        }
        init_scale = UiRectTransforms[0].localScale;
    }

    void Update()
    {
        if (Push) return;
        if (!Fade.Instance.IsEnd) return;
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
            StartCoroutine("Loadscene", SceneName);
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
        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadScene(SceneName);
    }
}
