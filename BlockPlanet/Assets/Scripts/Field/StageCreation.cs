using UnityEngine;

public class StageCreation : MonoBehaviour
{
    /// <summary>
    /// ステージを生成
    /// </summary>
    int stagenumber = 1;
    public FieldBlockMeshCombine blockMap = new FieldBlockMeshCombine();

    void Start()
    {
        //どのマップを使うか設定
        stagenumber = Select.Stagenum();
        GameObject parentTemp = new GameObject("FieldObjectTemp");
        BlockCreater.GetInstance().CreateField("Stage" + stagenumber, parentTemp.transform, blockMap, null, BlockCreater.SceneEnum.Game);
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
