using UnityEngine;

/// <summary>
/// キーボード対応したため、どれがどのボタンかを表示する
/// </summary>
public class KeyBindForSwitch : MonoBehaviour
{
#if UNITY_EDITOR
    /// <summary>
    /// レンダリングとGUIイベントのハンドリング
    /// </summary>
    void OnGUI()
    {
        GUI.Label(new Rect(20, 40, 1000, 1000),
        "Button : Arrow\nStick : WASD\n+- : 1\nR : E\nL : Q");
    }
#endif
}