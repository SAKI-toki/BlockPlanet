using UnityEngine;
using System.Collections.Generic;

public class ResultBlockMeshCombine : FieldBlockMeshCombine
{
    public override void BreakBlock(BlockNumber blockNum)
    {
        updateMeshFlg = true;
        blockArray[blockNum.line, blockNum.row, blockNum.height].isEnable = false;
        --blockNums[blockNum.line, blockNum.row];
        if (blockNum.line < blockArray.GetLength(0) - 1)
            blockArray[blockNum.line + 1, blockNum.row, blockNum.height].isSurround = false;
        if (blockNum.line > 0)
            blockArray[blockNum.line - 1, blockNum.row, blockNum.height].isSurround = false;

        if (blockNum.row < blockArray.GetLength(1) - 1)
            blockArray[blockNum.line, blockNum.row + 1, blockNum.height].isSurround = false;
        if (blockNum.row > 0)
            blockArray[blockNum.line, blockNum.row - 1, blockNum.height].isSurround = false;

        if (blockNum.height < blockArray.GetLength(2) - 1)
            blockArray[blockNum.line, blockNum.row, blockNum.height + 1].isSurround = false;
        if (blockNum.height > 0)
            blockArray[blockNum.line, blockNum.row, blockNum.height - 1].isSurround = false;
    }
    public override void BlockIsSurroundUpdate()
    {
        for (int i = 1; i < blockArray.GetLength(0) - 1; ++i)
        {
            for (int j = 1; j < blockArray.GetLength(1) - 1; ++j)
            {
                for (int k = 1; k < blockArray.GetLength(2) - 1; ++k)
                {
                    if (blockArray[i - 1, j, k].isEnable && blockArray[i + 1, j, k].isEnable &&
                     blockArray[i, j - 1, k].isEnable && blockArray[i, j + 1, k].isEnable &&
                     blockArray[i, j, k - 1].isEnable && blockArray[i, j, k + 1].isEnable)
                        blockArray[i, j, k].isSurround = true;
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