using UnityEngine;

/// <summary>
/// 子オブジェクト全ての合計のポリゴン数を出力
/// </summary>
public class DebugPolygonNumSum : MonoBehaviour
{
    [ContextMenu("子オブジェクトの合計ポリゴン数")]
    void OutputPolygonNum()
    {
        int sum = 0;
        foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
        {
            sum += meshFilter.sharedMesh.triangles.Length / 3;
        }
        Debug.Log(sum);
    }
}
