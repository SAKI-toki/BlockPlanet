public class FirstSceneTransition : UnityEngine.MonoBehaviour
{
    void Start()
    {
        Invoke("LoadStartScene", 1.0f);
    }
    void LoadStartScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Title");
    }
}
