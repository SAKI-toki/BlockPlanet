using UnityEngine;

public class InAdvanceCreateField : MonoBehaviour
{
#if UNITY_EDITOR

    [SerializeField, Range(1, 8)]
    int stageNumber = 1;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl))
            {
                AllCreateSelectField();
            }
            else
            {
                CreateSelectField(stageNumber);
            }
        }
    }

    void AllCreateSelectField()
    {
        for (int i = 0; i < 8; ++i)
        {
            CreateSelectField(i + 1);
        }
    }

    void CreateSelectField(int num)
    {
        Debug.Log("Stage" + num + ":CreateStart");
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
        Debug.Log("Stage" + num + ":CreateEnd");
    }

    void CreateAsset(string modelPath, string prefabPath, GameObject obj, int num)
    {
        System.IO.Directory.CreateDirectory(modelPath);
        System.IO.Directory.CreateDirectory(prefabPath);
        obj.transform.position = new Vector3(25, 0, 25);
        //メッシュの生成
        foreach (var filter in obj.GetComponentsInChildren<MeshFilter>())
        {
            filter.transform.position = Vector3.zero;
            string[] names = filter.GetComponent<MeshRenderer>().material.name.Split('(');
            UnityEditor.AssetDatabase.CreateAsset(filter.mesh, modelPath + "/" + names[0] + ".asset");
        }
        //prefab化
        UnityEditor.PrefabUtility.SaveAsPrefabAsset(obj, prefabPath + "/Field" + num + ".prefab");
    }
#endif
}
