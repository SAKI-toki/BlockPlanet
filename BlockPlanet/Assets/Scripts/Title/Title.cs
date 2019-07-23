using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : SingletonMonoBehaviour<Title>
{
    //クレジットの画像
    [SerializeField]
    GameObject uiParent = null;
    [SerializeField]
    GameObject creditParent;
    //BGM
    [System.NonSerialized]
    public AudioSource bgmSound = null;
    //タイトルの虹色のマテリアル
    [SerializeField]
    Material rainbowMat = null;
    //タイトルの爆弾
    [SerializeField]
    GameObject titleBomb = null;
    [SerializeField]
    AudioSource fallSound = null;

    delegate void StateType();
    StateType state;

    [SerializeField]
    SelectChoice currentSelectChoice;

    private void Start()
    {
        state = BombFallState;
        //フェード
        Fade.Instance.FadeOut(1.0f);
        //BGM
        bgmSound = GetComponent<AudioSource>();
        uiParent.SetActive(false);
        creditParent.SetActive(false);
    }

    void Update()
    {
        if (!Fade.Instance.IsEnd) return;
        if (state != null) state();
    }

    /// <summary>
    /// 爆弾が落ちるステート
    /// </summary>
    void BombFallState()
    {
        //アニメーションの終了
        if (SwitchInput.GetButtonDown(0, SwitchButton.Ok))
        {
            BombExplosion();
            Destroy(titleBomb);
            fallSound.Stop();
            bgmSound.Play();
        }
        if (titleBomb == null)
        {
            uiParent.SetActive(true);
            state = PushButtonState;
        }
    }

    /// <summary>
    /// ボタンを押させるステート
    /// </summary>
    void PushButtonState()
    {
        //シーン遷移
        if (SwitchInput.GetButtonDown(0, SwitchButton.Ok))
        {
            SoundManager.Instance.Push();
            uiParent.SetActive(false);
            currentSelectChoice.gameObject.SetActive(true);
            state = PlayerNumSelect;
        }
        //クレジットの表示
        else if (SwitchInput.GetButtonDown(0, SwitchButton.Pause))
        {
            SoundManager.Instance.Push();
            uiParent.SetActive(false);
            creditParent.SetActive(true);
            state = CreditState;
        }
    }

    /// <summary>
    /// クレジットステート
    /// </summary>
    void CreditState()
    {
        //クレジットの非表示
        if (SwitchInput.GetButtonDown(0, SwitchButton.Pause) ||
            SwitchInput.GetButtonDown(0, SwitchButton.Down))
        {
            SoundManager.Instance.Push();
            creditParent.SetActive(false);
            uiParent.SetActive(true);
            state = PushButtonState;
        }
    }

    /// <summary>
    /// プレイヤーの人数を選択するステート
    /// </summary>
    void PlayerNumSelect()
    {
        var prev = currentSelectChoice;
        //人数決定&シーン遷移
        if (SwitchInput.GetButtonDown(0, SwitchButton.Ok))
        {
            BlockCreater.GetInstance().maxPlayerNumber = currentSelectChoice.number;
            StartCoroutine(Loadscene());
        }
        //カーソル移動
        else if (SwitchInput.GetButtonDown(0, SwitchButton.StickRight))
        {
            if (currentSelectChoice.Right)
            {
                currentSelectChoice = currentSelectChoice.Right;
            }
        }
        else if (SwitchInput.GetButtonDown(0, SwitchButton.StickLeft))
        {
            if (currentSelectChoice.Left)
            {
                currentSelectChoice = currentSelectChoice.Left;
            }
        }
        else if (SwitchInput.GetButtonDown(0, SwitchButton.StickDown))
        {
            if (currentSelectChoice.Down)
            {
                currentSelectChoice = currentSelectChoice.Down;
            }
        }
        else if (SwitchInput.GetButtonDown(0, SwitchButton.StickUp))
        {
            if (currentSelectChoice.Up)
            {
                currentSelectChoice = currentSelectChoice.Up;
            }
        }
        //カーソルが移動したら
        if (prev != currentSelectChoice)
        {
            prev.gameObject.SetActive(false);
            //スティックの音
            SoundManager.Instance.Stick();
            //アクティブにするフィールドの変更
            currentSelectChoice.gameObject.SetActive(true);
        }
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

    public void BombExplosion()
    {
        //キューブの削除
        foreach (GameObject cube in GameObject.FindGameObjectsWithTag("Cube"))
            Destroy(cube);
        //壊れないキューブのマテリアル変更
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("StrongCube"))
        {
            obj.GetComponent<MeshRenderer>().material = rainbowMat;
        }
    }
}
