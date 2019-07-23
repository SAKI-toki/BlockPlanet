using UnityEngine;

/// <summary>
/// 子オブジェクトの数を出力する
/// </summary>
public class DebugChildCount : MonoBehaviour
{
    /// <summary>
    /// 子オブジェクトの数を出力
    /// </summary>
    [ContextMenu("子オブジェクトの数を出力")]
    void ChildLog()
    {
        Debug.Log(transform.childCount);
    }
}
