using UnityEngine;
using System.Collections.Generic;

public class ResultBlockMeshCombine : FieldBlockMeshCombine
{
    public override void BreakBlock(BlockNumber block_num)
    {
        updateMeshFlg = true;
        BlockArray[block_num.line, block_num.row, block_num.height].IsEnable = false;
        --BlockNum[block_num.line, block_num.row];
        if (block_num.line < BlockArray.GetLength(0) - 1)
            BlockArray[block_num.line + 1, block_num.row, block_num.height].isSurround = false;
        if (block_num.line > 0)
            BlockArray[block_num.line - 1, block_num.row, block_num.height].isSurround = false;

        if (block_num.row < BlockArray.GetLength(1) - 1)
            BlockArray[block_num.line, block_num.row + 1, block_num.height].isSurround = false;
        if (block_num.row > 0)
            BlockArray[block_num.line, block_num.row - 1, block_num.height].isSurround = false;

        if (block_num.height < BlockArray.GetLength(2) - 1)
            BlockArray[block_num.line, block_num.row, block_num.height + 1].isSurround = false;
        if (block_num.height > 0)
            BlockArray[block_num.line, block_num.row, block_num.height - 1].isSurround = false;
    }
    public override void BlockIsSurroundUpdate()
    {
        for (int i = 1; i < BlockArray.GetLength(0) - 1; ++i)
        {
            for (int j = 1; j < BlockArray.GetLength(1) - 1; ++j)
            {
                for (int k = 1; k < BlockArray.GetLength(2) - 1; ++k)
                {
                    if (BlockArray[i - 1, j, k].IsEnable && BlockArray[i + 1, j, k].IsEnable &&
                     BlockArray[i, j - 1, k].IsEnable && BlockArray[i, j + 1, k].IsEnable &&
                     BlockArray[i, j, k - 1].IsEnable && BlockArray[i, j, k + 1].IsEnable)
                        BlockArray[i, j, k].isSurround = true;
                }
            }
        }
    }

    protected override Mesh MakeOptimizeCube(MeshFilter filter, int row)
    {
        if (optimizeCubeMesh == null)
            CreateOptimizeCube(filter);
        return optimizeCubeMesh;
    }
    protected override void CreateOptimizeCube(MeshFilter filter)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        int[] indices = filter.sharedMesh.GetTriangles(0);
        filter.sharedMesh.GetVertices(vertices);
        filter.sharedMesh.GetUVs(0, uvs);
        //最適化したIndexを格納する配列
        int[] optimizeIndices = new int[30];
        int index = 0;
        for (int i = 0; i < 6; ++i)
        {
            //奥と下の場合は追加しない
            if (!(vertices[i * 4 + 0].y < 0 &&
            vertices[i * 4 + 1].y < 0 &&
            vertices[i * 4 + 2].y < 0 &&
            vertices[i * 4 + 3].y < 0))
            {
                for (int j = 0; j < 6; ++j)
                {
                    optimizeIndices[index++] = indices[i * 6 + j];
                }
            }
        }
        optimizeCubeMesh = new Mesh();
        optimizeCubeMesh.vertices = vertices.ToArray();
        optimizeCubeMesh.uv = uvs.ToArray();
        optimizeCubeMesh.triangles = optimizeIndices;
        optimizeCubeMesh.RecalculateNormals();
    }
}