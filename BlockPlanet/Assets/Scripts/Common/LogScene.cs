using UnityEngine;
using UnityEngine.SceneManagement;

public class LogScene : MonoBehaviour
{
    string currentSceneName = "";
    string prevSceneName = "";
    void Update()
    {
        currentSceneName = SceneManager.GetActiveScene().name;
        if (prevSceneName != currentSceneName)
        {
            Debug.Log(currentSceneName);
            prevSceneName = currentSceneName;
        }
    }
}
