﻿using UnityEngine;

public class ResultCreation : MonoBehaviour
{

    /// <summary>
    /// リザルト画面のブロック
    /// </summary>

    public BlockMap blockMap = new BlockMap();

    void Start()
    {
        GameObject parent = new GameObject("FieldObject");
        BlockCreater.GetInstance().CreateField("Result", parent.transform, blockMap);
        parent.isStatic = true;
        blockMap.BlockRendererUpdate();
    }
}