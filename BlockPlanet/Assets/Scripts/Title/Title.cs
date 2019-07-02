using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : SingletonMonoBehaviour<Title>
{
    [SerializeField]
    RectTransform[] UiRectTransforms = new RectTransform[3];
    int select_index = 0;
    Vector3 init_scale = new Vector3();
    Vector3 increment_scale = new Vector3();
    [SerializeField]
    Vector3 max_scale = new Vector3();
    float scale_time = 0.0f;
    //クレジットの画像
    [SerializeField]
    GameObject Credit_Image = null;

    private bool Push = false;
    [System.NonSerialized]
    public bool Check = false;
    private bool Flg = false;
    bool START = false;
    bool Credit = false;
    private float Timer = 2.0f;
    //BGM
    [System.NonSerialized]
    public AudioSource sounds = null;
    [SerializeField]
    Material rainbowMat;

    private void Start()
    {
        //フェード
        Fade.Instance.FadeOut(1.0f);
        //BGM
        sounds = GetComponent<AudioSource>();
        init_scale = UiRectTransforms[0].localScale;
    }

    void Update()
    {
        if (START) return;
        if (!Flg)
            Timer -= Time.deltaTime;
        if (!Fade.Instance.IsEnd) return;

        //特定のブロックと爆弾を消す
        if (SwitchInput.GetButtonDown(0, SwitchButton.Ok) && !Check)
        {
            Timer = 0;
            Push = true;
            Check = true;
            Debug.Log("ok");

            GameObject[] cubes = GameObject.FindGameObjectsWithTag("Cube");

            foreach (GameObject cube in cubes)
                Destroy(cube);

            GameObject[] bombs = GameObject.FindGameObjectsWithTag("Bomb");

            foreach (GameObject bomb in bombs)
                Destroy(bomb);
            GameObject[] strongCubes = GameObject.FindGameObjectsWithTag("StrongCube");
            foreach (GameObject obj in strongCubes)
            {
                obj.GetComponent<MeshRenderer>().material = rainbowMat;
            }
        }
        else
        {
            Push = false;
        }

        if (Timer <= 0)
        {
            foreach (var obj in UiRectTransforms)
                obj.gameObject.SetActive(true);
            if (!Push)
            {
                Flg = true;
            }
            if (Credit)
            {
                CreditUpdate();
            }
            else
            {
                SelectUpdate();
                UIActive();
            }
        }
    }

    private void UIActive()
    {
        if (SwitchInput.GetButtonDown(0, SwitchButton.Ok) && Flg && !START && !Credit)
        {
            switch (select_index)
            {
                case 0:
                    START = true;
                    //ロードする時の処理
                    StartCoroutine("Loadscene");
                    //音再生
                    SoundManager.Instance.Push();
                    break;
                case 1:
                    Credit = true;
                    Credit_Image.SetActive(true);
                    //音再生
                    SoundManager.Instance.Push();
                    break;
            }
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

    //セレクト画面に遷移
    private IEnumerator Loadscene()
    {
        //フェード
        Fade.Instance.FadeIn(1.0f);
        //少し待つ
        while (!Fade.Instance.IsEnd) yield return null;
        //シーン遷移
        SceneManager.LoadScene("Select");
    }

    //ゲームを終了させる
    private IEnumerator End()
    {
        //フェード
        Fade.Instance.FadeIn(1.0f);
        //少し待つ
        while (!Fade.Instance.IsEnd) yield return null;
        //ゲームを終了させる処理
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
    UnityEngine.Application.Quit();
#endif
    }

    void CreditUpdate()
    {
        if (SwitchInput.GetButtonDown(0, SwitchButton.Ok) || SwitchInput.GetButtonDown(0, SwitchButton.Cancel))
        {
            Credit = false;
            Credit_Image.SetActive(false);
        }
    }
}
