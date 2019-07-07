using UnityEngine;
using UnityEditor;

/// <summary>
/// MeshFilterの三角ポリゴンの数を出力
/// </summary>
[CustomEditor(typeof(MeshFilter))]
public class MeshInfo : Editor
{
    /// <summary>
    /// Inspectorに表示
    /// </summary>
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MeshFilter filter = target as MeshFilter;
        if (filter.sharedMesh == null)
        {
            return;
        }
        string polygons = "Triangles:";
        if (filter.sharedMesh.GetTopology(0) == MeshTopology.Triangles ||
            filter.sharedMesh.GetTopology(0) == MeshTopology.Quads)
        {
            polygons += filter.sharedMesh.triangles.Length / 3;
        }
        else
        {
            polygons += "MeshTopologyがTrianglesかQuadsじゃないと計算できません";
        }
        EditorGUILayout.LabelField(polygons);
        EditorGUILayout.LabelField("VerticesCount:" + filter.sharedMesh.vertexCount);
    }
}

/// <summary>
/// SkinnedMeshRendererの三角ポリゴンの数を出力
/// </summary>
[CustomEditor(typeof(SkinnedMeshRenderer))]
public class SkinnedMeshInfo : Editor
{
    /// <summary>
    /// Inspectorに表示
    /// </summary>
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SkinnedMeshRenderer skin = target as SkinnedMeshRenderer;
        string polygons = "Triangles:";
        if (skin.sharedMesh.GetTopology(0) == MeshTopology.Triangles ||
            skin.sharedMesh.GetTopology(0) == MeshTopology.Quads)
        {
            polygons += skin.sharedMesh.triangles.Length / 3;
        }
        else
        {
            polygons += "MeshTopologyがTrianglesかQuadsじゃないと計算できません";
        }
        EditorGUILayout.LabelField(polygons);
        EditorGUILayout.LabelField("VerticesCount:" + skin.sharedMesh.vertexCount);
    }
}