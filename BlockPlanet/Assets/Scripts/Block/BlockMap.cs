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
    struct BlockInfo
    {
        public GameObject obj;
        public MeshRenderer renderer;
        public BoxCollider collider;
        public BlockNumber block_number;
    }
    //サイズ分のマップを用意する
    BlockInfo[,,] BlockArray = new BlockInfo[BlockCreater.line_n, BlockCreater.row_n, 7];

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

    public void MyCulling()
    {
        for (int i = 1; i < BlockArray.GetLength(0); ++i)
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
            if (!block.obj) continue;
            block.collider.enabled = false;
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
        //範囲外かどうかチェックする
        if (line < 0 || line >= BlockArray.GetLength(0) ||
        row < 0 || row >= BlockArray.GetLength(1) ||
        height < 0 || height >= BlockArray.GetLength(2))
        {
            Debug.LogError("範囲外");
            return;
        }
        BlockArray[line, row, height].obj = block;
        BlockArray[line, row, height].renderer = block.GetComponent<MeshRenderer>();
        BlockArray[line, row, height].collider = block.GetComponent<BoxCollider>();
        BlockArray[line, row, height].block_number = block.GetComponent<BlockNumber>();
        BlockArray[line, row, height].block_number.SetNum(line, row, height);
    }

    /// <summary>
    /// ブロックが壊れたときに実行する
    /// </summary>
    /// <param name="block_num">ブロックの番号</param>
    public void BreakBlock(BlockNumber block_num)
    {
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

        return BlockArray[line + 1, row, height].obj &&
        BlockArray[line - 1, row, height].obj &&
        BlockArray[line, row + 1, height].obj &&
        BlockArray[line, row - 1, height].obj &&
        BlockArray[line, row, height + 1].obj &&
        BlockArray[line, row, height - 1].obj;
    }
}
