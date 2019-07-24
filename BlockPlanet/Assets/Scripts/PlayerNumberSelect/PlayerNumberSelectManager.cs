using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// プレイヤーの人数を選択するシーンの管理クラス
/// </summary>
public class PlayerNumberSelectManager : MonoBehaviour
{
    //参加するかどうか
    bool[] isPlays = new bool[4];

    delegate void StateType();
    StateType state;

    [SerializeField]
    GameObject[] playObject;

#if UNITY_EDITOR
    [SerializeField, Range(0, 3)]
    int debugNumber;
    [SerializeField]
    bool debugIsPlay;
    [SerializeField]
    bool changeIsPlay;
#endif

    void Start()
    {
        state = PushButtonPlayer;
        //フェード
        Fade.Instance.FadeOut(1.0f);
        foreach (var obj in playObject) obj.SetActive(false);
    }

    void Update()
    {
        if (!Fade.Instance.IsEnd) return;

        //タイトルに戻る
        if (!isPlays[0] && SwitchInput.GetButtonDown(0, SwitchButton.Cancel))
        {
            StartCoroutine(TranslationPrevScene());
            state = null;
        }

        if (state != null) state();
    }

    /// <summary>
    /// ボタンを押して参加するプレイヤーを決める
    /// </summary>
    void PushButtonPlayer()
    {
        //プレイ人数
        int playNumCount = 0;
        for (int i = 0; i < isPlays.Length; ++i)
        {
            bool isPlay = isPlays[i];
            //参加かどうか
            if (SwitchInput.GetButtonDown(i, SwitchButton.Ok))
            {
                isPlay = true;
            }
            else if (SwitchInput.GetButtonDown(i, SwitchButton.Cancel))
            {
                isPlay = false;
            }
#if UNITY_EDITOR
            if (debugNumber == i && changeIsPlay)
            {
                isPlay = debugIsPlay;
                changeIsPlay = false;
            }
#endif
            //選択したものが変わったかどうか
            if (isPlay != isPlays[i])
            {
                SoundManager.Instance.Push();
                isPlays[i] = isPlay;
                playObject[i].SetActive(isPlay);
            }
            //プレイ人数の加算
            if (isPlays[i]) ++playNumCount;
        }
        //二人以上参加しているときにポーズを押すとプレイヤー人数の選択を終了
        if (playNumCount >= 2 && SwitchInput.GetButtonDown(0, SwitchButton.Pause))
        {
            SoundManager.Instance.Push();
            BlockCreater.GetInstance().isPlays = isPlays;
            StartCoroutine(TranslationNextScene());
            state = null;
        }
    }

    /// <summary>
    /// 次のシーンに遷移する
    /// </summary>
    IEnumerator TranslationNextScene()
    {
        yield return StartCoroutine(SceneTranslationFade());
        //シーン遷移
        SceneManager.LoadScene("Select");
    }

    /// <summary>
    /// 前のシーンに遷移する
    /// </summary>
    IEnumerator TranslationPrevScene()
    {
        yield return StartCoroutine(SceneTranslationFade());
        //シーン遷移
        SceneManager.LoadScene("Title");
    }

    /// <summary>
    /// シーン遷移時のフェードイン
    /// </summary>
    IEnumerator SceneTranslationFade()
    {
        //フェード
        Fade.Instance.FadeIn(1.0f);
        //少し待つ
        while (!Fade.Instance.IsEnd) yield return null;
    }
}
