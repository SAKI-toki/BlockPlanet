using UnityEngine;

public class DebugChildCount : MonoBehaviour
{
    [ContextMenu("子オブジェクトの数を出力")]
    void ChildLog()
    {
        Debug.Log(transform.childCount);
    }
}
