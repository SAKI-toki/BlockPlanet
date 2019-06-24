using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 一番最初のシーンの前にロードされるシーンの管理
/// </summary>
public class InitializeSceneLoader : MonoBehaviour
{
    /// <summary>
    /// 初期化シーンを読み込む
    /// 最初のシーンを読み込む前に読み込まれる
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void LoadInitializeScene()
    {
        SceneManager.LoadScene("InitializeScene", LoadSceneMode.Additive);
    }
}
