using UnityEngine;
using UnityEngine.Profiling;

/// <summary>
/// FPSを表示する
/// </summary>
public class DebugProfiling : MonoBehaviour
{
    [SerializeField, Header("表示する位置")]
    Rect RenderRect = new Rect(20, 20, 200, 200);
    [SerializeField, Header("文字サイズ")]
    int CharSize = 20;
    [SerializeField, Header("文字色")]
    Color CharColor = Color.white;
    [SerializeField]
    float UpdateInterval = 0.5f;

    float TimeCount;
    int count = 0;
    float fps = 0.0f;

    void Start()
    {
        TimeCount = UpdateInterval;
    }

    void Update()
    {
        TimeCount += Time.unscaledDeltaTime;
        ++count;
        if (TimeCount > UpdateInterval)
        {
            fps = count / TimeCount;
            count = 0;
            TimeCount = 0.0f;
        }
    }

    /// <summary>
    /// レンダリングとGUIイベントのハンドリング
    /// </summary>
    void OnGUI()
    {
        GUI.skin.label.fontSize = CharSize;
        GUI.color = CharColor;
        GUI.Label(RenderRect,
        "FPS:" + fps.ToString("#.##"));
    }
}
