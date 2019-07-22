using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 初期化シーンでオブジェクトを破棄する
/// </summary>
public class InitializeSceneDestroyer : MonoBehaviour
{
    void Start()
    {
        //初期化シーンの破棄コルーチンをスタート
        StartCoroutine(DestroyInitializeScene("InitializeScene"));
    }

    /// <summary>
    /// 初期化シーンは読み込み終わったら必要ないのでアンロードする
    /// </summary>
    /// <param name="sceneName">シーンの名前</param>
    /// <returns></returns>
    IEnumerator DestroyInitializeScene(string sceneName)
    {
        //初期化シーンが有効なシーンになるまで待機
        while (!SceneManager.GetSceneByName(sceneName).IsValid())
        {
            yield return null;
        }
        //シーンのアンロード
        SceneManager.UnloadSceneAsync(sceneName);
    }
}
