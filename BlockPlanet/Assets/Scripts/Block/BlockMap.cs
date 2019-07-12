using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ブロックの三次元配列
/// </summary>
public class BlockMap
{
    /// <summary>
    /// ブロックの情報
    /// </summary>
    protected class BlockInfo
    {
        public bool isSurround = false;
        public bool IsEnable = false;
        public MeshRenderer renderer = null;
        public MeshFilter meshFilter = null;
        public BoxCollider collider = null;
        public BlockNumber block_number = null;
        public CombineInstance cmesh = new CombineInstance();
        public int MaterialNumber = 0;
    }
    //サイズ分のマップを用意する
    protected BlockInfo[,,] BlockArray = new BlockInfo[BlockMapSize.line_n, BlockMapSize.row_n, BlockMapSize.height_n];
    bool IsInit = false;
    void Initialize()
    {
        for (int i = 0; i < BlockArray.GetLength(0); ++i)
        {
            for (int j = 0; j < BlockArray.GetLength(1); ++j)
            {
                for (int k = 0; k < BlockArray.GetLength(2); ++k)
                {
                    BlockArray[i, j, k] = new BlockInfo();
                }
            }
        }
        IsInit = true;
    }

    /// <summary>
    /// Rendererの更新
    /// </summary>
    public void BlockRendererUpdate()
    {
        for (int i = 1; i < BlockArray.GetLength(0) - 1; ++i)
        {
            for (int j = 1; j < BlockArray.GetLength(1) - 1; ++j)
            {
                for (int k = 1; k < BlockArray.GetLength(2) - 1; ++k)
                {
                    //囲まれていたらRendererをOffにする
                    if (IsSurround(i, j, k))
                    {
                        BlockArray[i, j, k].renderer.enabled = false;
                    }
                }
            }
        }
    }

    /// <summary>
    /// PhysicsをOffにする
    /// </summary>
    public void BlockPhysicsOff()
    {
        foreach (var block in BlockArray)
        {
            if (!block.IsEnable) continue;
            block.collider.enabled = false;
        }
    }

    /// <summary>
    /// rendererをOffにする
    /// </summary>
    public void BlockRendererOff()
    {
        foreach (var block in BlockArray)
        {
            if (!block.IsEnable) continue;
            block.renderer.enabled = false;
        }
    }

    /// <summary>
    /// ブロックのセット
    /// </summary>
    /// <param name="line">縦</param>
    /// <param name="row">横</param>
    /// <param name="height">高さ</param>
    /// <param name="block">ブロックのオブジェクト</param>
    public void SetBlock(int line, int row, int height, GameObject block)
    {
        if (!IsInit) Initialize();
        //範囲外かどうかチェックする
        if (!RangeCheck(line, row, height))
        {
            Debug.LogError("範囲外");
            return;
        }
        if (block == null) return;
        //各情報をクラスに格納
        BlockArray[line, row, height].IsEnable = true;
        BlockArray[line, row, height].renderer = block.GetComponent<MeshRenderer>();
        BlockArray[line, row, height].meshFilter = block.GetComponent<MeshFilter>();
        BlockArray[line, row, height].collider = block.GetComponent<BoxCollider>();
        BlockArray[line, row, height].block_number = block.GetComponent<BlockNumber>();
        BlockArray[line, row, height].cmesh.transform = block.transform.localToWorldMatrix;
        BlockArray[line, row, height].cmesh.mesh = MakeOptimizeCube(BlockArray[line, row, height].meshFilter, row);
        BlockArray[line, row, height].MaterialNumber =
            BlockCreater.GetInstance().GetMaterialNumber(BlockArray[line, row, height].renderer.sharedMaterial);
        BlockArray[line, row, height].block_number.SetNum(line, row, height);
    }

    /// <summary>
    /// ブロックが壊れたときに実行する
    /// </summary>
    /// <param name="block_num">ブロックの番号</param>
    public virtual void BreakBlock(BlockNumber block_num)
    {
        BlockArray[block_num.line, block_num.row, block_num.height].IsEnable = false;
        if (block_num.line < BlockArray.GetLength(0) - 1 &&
            BlockArray[block_num.line + 1, block_num.row, block_num.height].renderer)
            BlockArray[block_num.line + 1, block_num.row, block_num.height].renderer.enabled = true;

        if (block_num.line > 0 &&
            BlockArray[block_num.line - 1, block_num.row, block_num.height].renderer)
            BlockArray[block_num.line - 1, block_num.row, block_num.height].renderer.enabled = true;

        if (block_num.row < BlockArray.GetLength(1) - 1 &&
            BlockArray[block_num.line, block_num.row + 1, block_num.height].renderer)
            BlockArray[block_num.line, block_num.row + 1, block_num.height].renderer.enabled = true;

        if (block_num.row > 0 &&
            BlockArray[block_num.line, block_num.row - 1, block_num.height].renderer)
            BlockArray[block_num.line, block_num.row - 1, block_num.height].renderer.enabled = true;

        if (block_num.height < BlockArray.GetLength(2) - 1 &&
            BlockArray[block_num.line, block_num.row, block_num.height + 1].renderer)
            BlockArray[block_num.line, block_num.row, block_num.height + 1].renderer.enabled = true;

        if (block_num.height > 0 &&
            BlockArray[block_num.line, block_num.row, block_num.height - 1].renderer)
            BlockArray[block_num.line, block_num.row, block_num.height - 1].renderer.enabled = true;
    }

    /// <summary>
    /// 囲み判定
    /// </summary>
    /// <param name="line">縦</param>
    /// <param name="row">横</param>
    /// <param name="height">高さ</param>
    /// <returns>囲まれているかどうか</returns>
    bool IsSurround(int line, int row, int height)
    {
        //範囲外チェック
        if (line == 0 || row == 0 || height == 0 ||
        line == BlockArray.GetLength(0) - 1 ||
        row == BlockArray.GetLength(1) - 1 ||
        height == BlockArray.GetLength(2) - 1)
        {
            return false;
        }

        return BlockArray[line + 1, row, height].IsEnable &&
        BlockArray[line - 1, row, height].IsEnable &&
        BlockArray[line, row + 1, height].IsEnable &&
        BlockArray[line, row - 1, height].IsEnable &&
        BlockArray[line, row, height + 1].IsEnable &&
        BlockArray[line, row, height - 1].IsEnable;
    }

    bool RangeCheck(int line, int row, int height)
    {
        return line >= 0 && line < BlockArray.GetLength(0) &&
        row >= 0 && row < BlockArray.GetLength(1) &&
        height >= 0 && height < BlockArray.GetLength(2);
    }

    protected virtual Mesh MakeOptimizeCube(MeshFilter filter, int row)
    {
        return filter.sharedMesh;
    }
}
