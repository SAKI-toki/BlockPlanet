using UnityEngine;
using UnityEngine.Profiling;

/// <summary>
/// FPSを表示する
/// </summary>
public class DebugProfiling : MonoBehaviour
{
    [SerializeField, Header("表示する位置")]
    Rect renderRect = new Rect(20, 20, 200, 200);
    [SerializeField, Header("文字サイズ")]
    int charSize = 20;
    [SerializeField, Header("文字色")]
    Color charColor = Color.white;
    [SerializeField]
    float updateInterval = 0.5f;

    float timeCount;
    int count = 0;
    float fps = 0.0f;

    void Start()
    {
        timeCount = updateInterval;
    }

    void Update()
    {
        timeCount += Time.unscaledDeltaTime;
        ++count;
        if (timeCount > updateInterval)
        {
            fps = count / timeCount;
            count = 0;
            timeCount = 0.0f;
        }
    }

    /// <summary>
    /// レンダリングとGUIイベントのハンドリング
    /// </summary>
    void OnGUI()
    {
        GUI.skin.label.fontSize = charSize;
        GUI.color = charColor;
        GUI.Label(renderRect,
        "FPS:" + fps.ToString("#.##"));
    }
}
