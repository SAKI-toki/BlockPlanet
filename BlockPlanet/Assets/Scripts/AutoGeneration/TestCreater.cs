using UnityEngine;

public class TestCreater : MonoBehaviour
{
    public FieldBlockMeshCombine blockMap = new FieldBlockMeshCombine();
    void Start()
    {
        GameObject parentTemp = new GameObject("FieldObjectTemp");
        BlockCreater.GetInstance().AutoGenerate(AutoGeneration.Generate(3, 0.9f), parentTemp.transform, blockMap);
        parentTemp.isStatic = true;
        blockMap.BlockIsSurroundUpdate();
        blockMap.BlockRendererOff();
        GameObject parent = new GameObject("FieldObject");
        blockMap.Initialize(parent);
    }
    void Update()
    {
        blockMap.CreateMesh();
    }
}