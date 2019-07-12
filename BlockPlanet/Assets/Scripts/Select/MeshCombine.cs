using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// メッシュ統合
/// </summary>
static public class MeshCombine
{
    /// <summary>
    /// 統合する
    /// </summary>
    /// <param name="combine">統合対象の親オブジェクト</param>
    /// <param name="parent">統合したものの親オブジェクト</param>
    static public void Combine(GameObject combine, GameObject parent)
    {
        //レンダラーの取得
        Renderer[] meshRenderers = combine.GetComponentsInChildren<Renderer>();
        //マテリアルごとに統合する
        Dictionary<Material, List<CombineInstance>> instances = new Dictionary<Material, List<CombineInstance>>();

        //リストに追加していく
        foreach (var renderer in meshRenderers)
        {
            //レンダラーがdisableなら何もしない
            if (!renderer.enabled) continue;
            var mat = renderer.sharedMaterial;
            //キーがない場合
            if (!instances.ContainsKey(mat))
            {
                //キーの追加
                instances.Add(mat, new List<CombineInstance>());
            }
            var cmesh = new CombineInstance();
            cmesh.transform = renderer.transform.localToWorldMatrix;
            cmesh.mesh = renderer.GetComponent<MeshFilter>().sharedMesh;
            instances[mat].Add(cmesh);
        }

        //統合する
        foreach (var instance in instances)
        {
            GameObject obj = new GameObject(instance.Key.name);
            MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
            MeshRenderer renderer = obj.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = instance.Key;
            meshFilter.mesh = new Mesh();
            meshFilter.mesh.CombineMeshes(instance.Value.ToArray());
            if (meshFilter.sharedMesh.vertexCount > ushort.MaxValue)
            {
                Debug.LogError("頂点数が多すぎます");
            }
            obj.isStatic = true;
            obj.transform.parent = parent.transform;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }
}