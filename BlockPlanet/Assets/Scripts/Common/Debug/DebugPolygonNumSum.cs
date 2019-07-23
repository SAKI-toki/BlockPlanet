using UnityEngine;

/// <summary>
/// 子オブジェクト全ての合計のポリゴン数を出力
/// </summary>
public class DebugPolygonNumSum : MonoBehaviour
{
    /// <summary>
    /// 子オブジェクト全ての合計のポリゴン数を出力
    /// </summary>
    [ContextMenu("子オブジェクトの合計ポリゴン数")]
    void OutputPolygonNum()
    {
        int sum = 0;
        //全てのメッシュを足す
        foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
        {
            if (meshFilter.sharedMesh.GetTopology(0) == MeshTopology.Triangles ||
               meshFilter.sharedMesh.GetTopology(0) == MeshTopology.Quads)
            {
                sum += meshFilter.sharedMesh.triangles.Length / 3;
            }
        }
        Debug.Log(sum);
    }
}
