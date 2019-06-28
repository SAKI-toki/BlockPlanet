using UnityEngine;
using System.Collections.Generic;

public class StageCreation : MonoBehaviour
{

    /// <summary>
    /// ステージを生成
    /// </summary>
    int stagenumber = 1;

    public BlockMap blockMap = new BlockMap();

    void Start()
    {
        //どのマップを使うか設定
        stagenumber = Select.Stagenum();
        GameObject parent = new GameObject("FieldObject");
        BlockCreater.GetInstance().CreateField("Stage" + stagenumber, parent.transform, blockMap, BlockCreater.SceneEnum.Game);
        parent.isStatic = true;
        blockMap.BlockRendererUpdate();
    }
}
