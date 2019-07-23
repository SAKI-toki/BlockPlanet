using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// switchではUnityのロゴが出ているときもシーンが進行してしまうので
/// 最初のシーンで1秒待機する
/// </summary>
public class FirstSceneTransition : MonoBehaviour
{
    const string TitleSceneName = "Title";
    const float WaitTime = 1.0f;
    void Start()
    {
        StartCoroutine(LoadStartScene());
    }

    /// <summary>
    /// 最初のシーンに指定した秒数待機して遷移する
    /// </summary>
    IEnumerator LoadStartScene()
    {
        yield return new WaitForSeconds(WaitTime);
        SceneManager.LoadScene(TitleSceneName);
    }
}
