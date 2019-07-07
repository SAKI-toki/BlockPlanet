using UnityEngine;

public class InAdvanceCreateField : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField]
    Material[] mats = new Material[8];

    [SerializeField, Range(1, 8)]
    int stageNumber = 1;

    void Start()
    {

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            CreateGameField(stageNumber);
    }

    void AllCreateSelectField()
    {
        for (int i = 1; i <= 8; ++i)
            CreateSelectField(i);
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
        var autoSet = combineField.AddComponent<AutoMaterialSet>();
        autoSet.SetMaterial(mats);
        Destroy(field);
        CreateAsset("Assets/Models/SelectField/Field" + num, "Assets/Prefabs/SelectField", combineField, num);
        Destroy(combineField);
    }

    void CreateGameField(int num)
    {
        GameObject field = new GameObject("field" + num);
        FieldBlockMeshCombine blockMap = new FieldBlockMeshCombine();
        //マップ生成
        BlockCreater.GetInstance().CreateField("Stage" + num, field.transform, blockMap, null, BlockCreater.SceneEnum.Game);
        field.isStatic = true;
        blockMap.BlockIsSurroundUpdate();
        blockMap.BlockRendererOff();
        GameObject combineField = new GameObject("field" + num);
        blockMap.Initialize(combineField);
        //統合
        //MeshCombine.Combine(field, combineField);
        var autoSet = combineField.AddComponent<AutoMaterialSet>();
        autoSet.SetMaterial(mats);
        Destroy(field);
        CreateAsset("Assets/Models/GameField/Field" + num, "Assets/Prefabs/GameField", combineField, num, true);
        Destroy(combineField);
    }

    void CreateAsset(string modelPath, string prefabPath, GameObject obj, int num, bool withPlayer = false)
    {
        //パスをセット
        //string path = "Assets/Models/SelectField/Field" + num;
        // System.IO.Directory.CreateDirectory(path);
        // //メッシュの生成
        // foreach (var filter in combineField.GetComponentsInChildren<MeshFilter>())
        // {
        //     string[] name = filter.GetComponent<MeshRenderer>().material.name.Split('(');
        //     UnityEditor.AssetDatabase.CreateAsset(filter.mesh, path + "/" + name[0] + ".asset");
        // }
        // //prefab化
        // UnityEditor.PrefabUtility.SaveAsPrefabAsset(combineField, "Assets/Prefabs/SelectField/Field" + num + ".prefab");
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
