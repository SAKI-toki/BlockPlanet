using UnityEngine;
using UnityEngine.SceneManagement;

public class FirstSceneTransition : MonoBehaviour
{
    const string TitleSceneName = "Title";
    void Start()
    {
        //一秒後にシーン遷移する
        Invoke("LoadStartScene", 1.0f);
    }
    void LoadStartScene()
    {
        SceneManager.LoadScene(TitleSceneName);
    }
}
