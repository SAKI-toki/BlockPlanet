using UnityEngine;

/// <summary>
/// ステージ生成
/// </summary>
public class StageCreation : MonoBehaviour
{
    int stagenumber;
    public FieldBlockMeshCombine blockMap = new FieldBlockMeshCombine();

    void Start()
    {
        //どのマップを使うか設定
        stagenumber = Select.Stagenum();
        //当たり判定のみのオブジェクト
        GameObject parentTemp = new GameObject("FieldObjectPhysics");
        BlockCreater.GetInstance().CreateField("Stage" + stagenumber,
                parentTemp.transform, blockMap, null, BlockCreater.SceneEnum.Game);
        parentTemp.isStatic = true;
        blockMap.BlockIsSurroundUpdate();
        blockMap.BlockRendererOff();
        //メッシュのみのオブジェクト
        GameObject parent = new GameObject("FieldObjectMesh");
        blockMap.Initialize(parent);
    }
    void Update()
    {
        blockMap.CreateMesh();
    }
}
