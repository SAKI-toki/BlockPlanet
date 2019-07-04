using System.Collections.Generic;
using UnityEngine;

public class FieldBlockMeshCombine : BlockMap
{
    Mesh mesh;
    class CombineMeshInfo
    {
        public GameObject obj = null;
        public MeshFilter meshFilter = null;
        public MeshRenderer renderer = null;
    }
    CombineMeshInfo[] CombineMeshs = new CombineMeshInfo[8];
    //元々MaterialをキーとしたDictionaryにしたが、呼び出す回数が多いため配列にして高速化した
    List<CombineInstance>[] combines = new List<CombineInstance>[8];
    class meshInfo
    {
        public List<Vector3> vertices = new List<Vector3>();
        public List<int> indices = new List<int>();
        public List<Vector2> uvs = new List<Vector2>();
    }
    protected bool updateMeshFlg = false;
    protected Mesh optimizeCubeMesh = null;
    Mesh optimizeCubeMeshRight = null;
    Mesh optimizeCubeMeshLeft = null;
    //縦方向にブロックの数を保持する
    protected int[,] BlockNum = new int[BlockCreater.line_n, BlockCreater.row_n];


    public void CreateMesh()
    {
        //更新フラグが立っていなかったら何もしない
        if (!updateMeshFlg) return;
        updateMeshFlg = false;
        //配列をクリア
        for (int i = 1; i < 8; ++i)
        {
            combines[i].Clear();
        }
        int length0 = BlockArray.GetLength(0);
        int length1 = BlockArray.GetLength(1);
        int length2 = BlockArray.GetLength(2);
        for (int i = 0; i < length0; ++i)
        {
            for (int j = 0; j < length1; ++j)
            {
                if (BlockNum[i, j] == 0) continue;
                for (int k = 0; k < length2; ++k)
                {
                    var block = BlockArray[i, j, k];
                    if (!block.IsEnable || block.isSurround || block.MaterialNumber == 0) continue;
                    //位置を格納
                    combines[block.MaterialNumber].Add(block.cmesh);
                }
            }
        }
        for (int i = 1; i < 8; ++i)
        {
            mesh = new Mesh();
            //先にメッシュを統合してから入れたほうが軽い
            mesh.CombineMeshes(combines[i].ToArray());
            CombineMeshs[i].meshFilter.mesh = mesh;
        }
    }


    /// <summary>
    /// 統合したメッシュオブジェクトの生成
    /// </summary>
    /// <param name="name">オブジェクト名</param>
    /// <param name="combineMeshInfo">メッシュの情報</param>
    void CreateCombineMeshObject(string name, CombineMeshInfo combineMeshInfo)
    {
        combineMeshInfo.obj = new GameObject(name);
        combineMeshInfo.meshFilter = combineMeshInfo.obj.AddComponent<MeshFilter>();
        combineMeshInfo.renderer = combineMeshInfo.obj.AddComponent<MeshRenderer>();
        combineMeshInfo.obj.isStatic = true;
    }

    public override void BreakBlock(BlockNumber block_num)
    {
        updateMeshFlg = true;
        BlockArray[block_num.line, block_num.row, block_num.height].IsEnable = false;
        --BlockNum[block_num.line, block_num.row];
        if (block_num.line < BlockArray.GetLength(0) - 1)
            BlockArray[block_num.line + 1, block_num.row, block_num.height].isSurround = false;

        if (block_num.row < BlockArray.GetLength(1) - 1)
            BlockArray[block_num.line, block_num.row + 1, block_num.height].isSurround = false;

        if (block_num.row > 0)
            BlockArray[block_num.line, block_num.row - 1, block_num.height].isSurround = false;

        if (block_num.height > 0)
            BlockArray[block_num.line, block_num.row, block_num.height - 1].isSurround = false;
    }

    public virtual void BlockIsSurroundUpdate()
    {
        for (int i = 1; i < BlockArray.GetLength(0); ++i)
        {
            for (int j = 1; j < BlockArray.GetLength(1) - 1; ++j)
            {
                for (int k = 0; k < BlockArray.GetLength(2) - 1; ++k)
                {
                    if (BlockArray[i - 1, j, k].IsEnable && BlockArray[i, j - 1, k].IsEnable &&
                    BlockArray[i, j + 1, k].IsEnable && BlockArray[i, j, k + 1].IsEnable)
                        BlockArray[i, j, k].isSurround = true;
                }
            }
        }
    }

    /// <summary>
    /// 最適化されたキューブを取得
    /// </summary>
    /// <param name="filter">キューブを取得するためのFilter</param>
    /// <param name="row">横の位置</param>
    /// <returns>最適化されたキューブ</returns>
    protected override Mesh MakeOptimizeCube(MeshFilter filter, int row)
    {
        if (optimizeCubeMesh == null)
            CreateOptimizeCube(filter);
        if (row < BlockCreater.row_n / 2 - 5)
        {
            return optimizeCubeMeshLeft;
        }
        if (row > BlockCreater.row_n / 2 + 5)
        {
            return optimizeCubeMeshRight;
        }
        return optimizeCubeMesh;
    }

    /// <summary>
    /// 奥と下のポリゴンを消去したCubeMeshを生成
    /// </summary>
    /// <param name="filter"></param>
    protected virtual void CreateOptimizeCube(MeshFilter filter)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        int[] indices = filter.sharedMesh.GetTriangles(0);
        filter.sharedMesh.GetVertices(vertices);
        filter.sharedMesh.GetUVs(0, uvs);
        //最適化したIndexを格納する配列
        int[] optimizeIndices = new int[24];
        int index = 0;
        for (int i = 0; i < 6; ++i)
        {
            //奥と下の場合は追加しない
            if (!((vertices[i * 4 + 0].y < 0 &&
            vertices[i * 4 + 1].y < 0 &&
            vertices[i * 4 + 2].y < 0 &&
            vertices[i * 4 + 3].y < 0) ||
            (vertices[i * 4 + 0].z > 0 &&
            vertices[i * 4 + 1].z > 0 &&
            vertices[i * 4 + 2].z > 0 &&
            vertices[i * 4 + 3].z > 0)))
            {
                for (int j = 0; j < 6; ++j)
                {
                    optimizeIndices[index++] = indices[i * 6 + j];
                }
            }
        }
        int[] optimizeIndicesRight = new int[18];
        int[] optimizeIndicesLeft = new int[18];
        int rightIndex = 0;
        int leftIndex = 0;
        for (int i = 0; i < 6; ++i)
        {
            if (!((vertices[i * 4 + 0].y < 0 &&
               vertices[i * 4 + 1].y < 0 &&
               vertices[i * 4 + 2].y < 0 &&
               vertices[i * 4 + 3].y < 0) ||
               (vertices[i * 4 + 0].z > 0 &&
               vertices[i * 4 + 1].z > 0 &&
               vertices[i * 4 + 2].z > 0 &&
               vertices[i * 4 + 3].z > 0)))
            {
                if (!(vertices[i * 4 + 0].x > 0 &&
                vertices[i * 4 + 1].x > 0 &&
                vertices[i * 4 + 2].x > 0 &&
                vertices[i * 4 + 3].x > 0))
                {
                    for (int j = 0; j < 6; ++j)
                    {
                        optimizeIndicesRight[rightIndex++] = indices[i * 6 + j];
                    }
                }
                if (!(vertices[i * 4 + 0].x < 0 &&
                vertices[i * 4 + 1].x < 0 &&
                vertices[i * 4 + 2].x < 0 &&
                vertices[i * 4 + 3].x < 0))
                {
                    for (int j = 0; j < 6; ++j)
                    {
                        optimizeIndicesLeft[leftIndex++] = indices[i * 6 + j];
                    }
                }
            }
        }
        optimizeCubeMesh = new Mesh();
        optimizeCubeMesh.vertices = vertices.ToArray();
        optimizeCubeMesh.uv = uvs.ToArray();
        optimizeCubeMesh.triangles = optimizeIndices;
        optimizeCubeMesh.RecalculateNormals();
        optimizeCubeMeshRight = new Mesh();
        optimizeCubeMeshRight.vertices = vertices.ToArray();
        optimizeCubeMeshRight.uv = uvs.ToArray();
        optimizeCubeMeshRight.triangles = optimizeIndicesRight;
        optimizeCubeMeshRight.RecalculateNormals();
        optimizeCubeMeshLeft = new Mesh();
        optimizeCubeMeshLeft.vertices = vertices.ToArray();
        optimizeCubeMeshLeft.uv = uvs.ToArray();
        optimizeCubeMeshLeft.triangles = optimizeIndicesLeft;
        optimizeCubeMeshLeft.RecalculateNormals();
    }

    public void Initialize()
    {
        mesh = new Mesh();
        for (int i = 0; i < 8; ++i)
        {
            combines[i] = new List<CombineInstance>(3000);
        }
        foreach (var block in BlockArray)
        {
            if (!block.IsEnable) continue;
            break;
        }
        int length0 = BlockArray.GetLength(0);
        int length1 = BlockArray.GetLength(1);
        int length2 = BlockArray.GetLength(2);
        for (int i = 0; i < length0; ++i)
        {
            for (int j = 0; j < length1; ++j)
            {
                for (int k = 0; k < length2; ++k)
                {
                    var block = BlockArray[i, j, k];
                    if (block.MaterialNumber != 0) ++BlockNum[i, j];
                    if (!block.IsEnable || block.isSurround) continue;
                    //位置を格納
                    combines[block.MaterialNumber].Add(block.cmesh);
                }
            }
        }
        for (int i = 0; i < 8; ++i)
        {
            CombineMeshs[i] = new CombineMeshInfo();
            CreateCombineMeshObject("Field" + BlockCreater.GetInstance().mats[i].name, CombineMeshs[i]);
            CombineMeshs[i].renderer.sharedMaterial = BlockCreater.GetInstance().mats[i];
            mesh = new Mesh();
            //先にメッシュを統合してから入れたほうが軽い
            mesh.CombineMeshes(combines[i].ToArray());
            CombineMeshs[i].meshFilter.mesh = mesh;
        }
    }
}
