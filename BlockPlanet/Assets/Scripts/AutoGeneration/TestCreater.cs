using UnityEngine;

public class TestCreater : MonoBehaviour
{
    public FieldBlockMeshCombine blockMap = new FieldBlockMeshCombine();
    void Start()
    {
        GameObject parentTemp = new GameObject("FieldObjectTemp");
        BlockCreater.GetInstance().AutoGenerate(AutoGeneration.Generate(), parentTemp.transform, blockMap);
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