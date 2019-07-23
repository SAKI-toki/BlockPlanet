using UnityEngine;

/// <summary>
/// シーン遷移しても破棄されないオブジェクト
/// </summary>
public class DontDestroy : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(this);
    }
}
