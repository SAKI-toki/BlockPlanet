using UnityEngine;

public class InAdvanceCreateField : MonoBehaviour
{
#if UNITY_EDITOR

    [SerializeField, Range(1, 8)]
    int stageNumber = 1;

    void Start()
    {
        CreateSelectField(stageNumber);
    }

    void CreateSelectField(int num)
    {
        GameObject field = new GameObject("field" + num);
        BlockMap blockMap = new BlockMap();
        //マップ生成
        BlockCreater.GetInstance().CreateField("Stage" + num, field.transform, blockMap, null);
        GameObject combineField = new GameObject("field" + num);
        //中身のrendererをOff
        blockMap.BlockRendererUpdate();
        //統合
        MeshCombine.Combine(field, combineField);
        Destroy(field);
        CreateAsset("Assets/Models/SelectField/Field" + num, "Assets/Prefabs/SelectField", combineField, num);
        Destroy(combineField);
    }
    void CreateAsset(string modelPath, string prefabPath, GameObject obj, int num, bool withPlayer = false)
    {
        System.IO.Directory.CreateDirectory(modelPath);
        System.IO.Directory.CreateDirectory(prefabPath);
        //メッシュの生成
        foreach (var filter in obj.GetComponentsInChildren<MeshFilter>())
        {
            string[] name = filter.GetComponent<MeshRenderer>().material.name.Split('(');
            UnityEditor.AssetDatabase.CreateAsset(filter.mesh, modelPath + "/" + name[0] + ".asset");
        }
        if (withPlayer)
        {
            for (int i = 1; i <= 4; ++i)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player" + i);
                player.transform.parent = obj.transform;
            }
        }
        //prefab化
        UnityEditor.PrefabUtility.SaveAsPrefabAsset(obj, prefabPath + "/Field" + num + ".prefab");
    }
#endif
}
