using UnityEngine;
using System.Collections.Generic;

public class CreateSelectField : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField]
    Material[] mats = new Material[8];

    void Start()
    {
        //AllCreate();
    }

    void AllCreate()
    {
        for (int i = 1; i <= 8; ++i)
        {
            CreateField(i);
        }
    }

    void CreateField(int num)
    {
        GameObject field = new GameObject("field" + num);
        BlockMap blockMaps = new BlockMap();
        BlockCreater.GetInstance().CreateField("Stage" + num, field.transform, blockMaps, null);
        GameObject combineField = new GameObject("field" + num);
        blockMaps.BlockRendererUpdate();
        MeshCombine.Combine(field, combineField);
        var autoSet = combineField.AddComponent<AutoMaterialSet>();
        autoSet.SetMaterial(mats);
        Destroy(field);
        string path = "Assets/Models/SelectField/Field" + num;
        System.IO.Directory.CreateDirectory(path);
        //メッシュの生成
        foreach (var filter in combineField.GetComponentsInChildren<MeshFilter>())
        {
            string[] name = filter.GetComponent<MeshRenderer>().material.name.Split('(');
            UnityEditor.AssetDatabase.CreateAsset(filter.mesh, path + "/" + name[0] + ".asset");
        }
        //prefab化
        UnityEditor.PrefabUtility.SaveAsPrefabAsset(combineField, "Assets/Prefabs/SelectField/Field" + num + ".prefab");
        Destroy(combineField);
    }
#endif
}
