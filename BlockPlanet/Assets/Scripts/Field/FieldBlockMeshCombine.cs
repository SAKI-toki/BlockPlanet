using System.Collections.Generic;
using UnityEngine;
using System.IO;

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
    BlockInfo blockInfo;

    BlockInfo[,] infos = new BlockInfo[8, BlockCreater.line_n * BlockCreater.row_n];

    public void CreateMesh()
    {
        //更新フラグが立っていなかったら何もしない
        if (!updateMeshFlg) return;
        updateMeshFlg = false;
        int length0 = BlockArray.GetLength(0);
        int length1 = BlockArray.GetLength(1);
        int length2 = BlockArray.GetLength(2);
        int[] indexs = new int[8];
        for (int i = 0; i < length0; ++i)
        {
            for (int j = 0; j < length1; ++j)
            {
                //ブロックがない場所はループしない
                if (BlockNum[i, j] == 0) continue;
                for (int k = 0; k < length2; ++k)
                {
                    blockInfo = BlockArray[i, j, k];
                    if (!blockInfo.IsEnable ||
                        blockInfo.isSurround ||
                        blockInfo.MaterialNumber == 0) continue;
                    //位置を格納
                    infos[blockInfo.MaterialNumber, indexs[blockInfo.MaterialNumber]++] = blockInfo;
                }
            }
        }
        //マテリアルごとにメッシュ統合
        for (int i = 1; i < 8; ++i)
        {
            mesh = new Mesh();
            CombineInstance[] instances = new CombineInstance[indexs[i]];
            for (int j = 0; j < indexs[i]; ++j)
            {
                instances[j] = infos[i, j].cmesh;
            }
            //先にメッシュを統合してから入れたほうが軽い
            mesh.CombineMeshes(instances);
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
        {
            BlockArray[block_num.line + 1, block_num.row, block_num.height].isSurround = false;
        }

        if (block_num.row < BlockArray.GetLength(1) - 1)
        {
            BlockArray[block_num.line, block_num.row + 1, block_num.height].isSurround = false;
        }

        if (block_num.row > 0)
        {
            BlockArray[block_num.line, block_num.row - 1, block_num.height].isSurround = false;
        }

        if (block_num.height > 0)
        {
            BlockArray[block_num.line, block_num.row, block_num.height - 1].isSurround = false;
        }
    }

    public virtual void BlockIsSurroundUpdate()
    {
        for (int i = 1; i < BlockArray.GetLength(0); ++i)
        {
            for (int j = 1; j < BlockArray.GetLength(1) - 1; ++j)
            {
                for (int k = 0; k < BlockArray.GetLength(2) - 1; ++k)
                {
                    //壊れないブロックのみ別の処理
                    if (BlockArray[i, j, k].MaterialNumber == 0)
                    {
                        if (BlockArray[i - 1, j, k].IsEnable && BlockArray[i - 1, j, k].MaterialNumber == 0 &&
                        BlockArray[i, j - 1, k].IsEnable && BlockArray[i, j - 1, k].MaterialNumber == 0 &&
                        BlockArray[i, j + 1, k].IsEnable && BlockArray[i, j + 1, k].MaterialNumber == 0 &&
                        BlockArray[i, j, k + 1].IsEnable && BlockArray[i, j, k + 1].MaterialNumber == 0)
                        {
                            BlockArray[i, j, k].isSurround = true;
                        }
                    }
                    else
                    {
                        if (BlockArray[i - 1, j, k].IsEnable && BlockArray[i, j - 1, k].IsEnable &&
                        BlockArray[i, j + 1, k].IsEnable && BlockArray[i, j, k + 1].IsEnable)
                        {
                            BlockArray[i, j, k].isSurround = true;
                        }
                    }
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
        const int centerRange = 2;
        if (optimizeCubeMesh == null)
            CreateOptimizeCube(filter);
        if (row < BlockCreater.row_n / 2 - centerRange)
        {
            return optimizeCubeMeshLeft;
        }
        if (row > BlockCreater.row_n / 2 + centerRange)
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
        int[] indices = filter.sharedMesh.GetIndices(0);
        filter.sharedMesh.GetVertices(vertices);
        filter.sharedMesh.GetUVs(0, uvs);
        Vector3[] optimizeVertices = new Vector3[16];
        Vector3[] optimizeVerticesRight = new Vector3[12];
        Vector3[] optimizeVerticesLeft = new Vector3[12];
        Vector2[] optimizeUvs = new Vector2[16];
        Vector2[] optimizeUvsRight = new Vector2[12];
        Vector2[] optimizeUvsLeft = new Vector2[12];

        //底と奥がないインデックスリスト
        int[] optimizeIndices = new int[24];
        //底と奥と右がないインデックスリスト
        int[] optimizeIndicesRight = new int[18];
        //底と奥と左がないインデックスリスト
        int[] optimizeIndicesLeft = new int[18];
        //中間
        int index = 0;
        int indicesIndex = 0;
        int skipCount = 0;
        //右
        int rightIndex = 0;
        int rightIndicesIndex = 0;
        int skipCountRight = 0;
        //左
        int leftIndex = 0;
        int leftIndicesIndex = 0;
        int skipCountLeft = 0;
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
                for (int j = 0; j < 4; ++j)
                {
                    optimizeVertices[index] = vertices[i * 4 + j];
                    optimizeUvs[index] = uvs[i * 4 + j];
                    ++index;
                }
                for (int j = 0; j < 6; ++j)
                {
                    optimizeIndices[indicesIndex] = indices[i * 6 + j] - skipCount * 4;
                    ++indicesIndex;
                }
                //右なし
                if (!(vertices[i * 4 + 0].x > 0 &&
                vertices[i * 4 + 1].x > 0 &&
                vertices[i * 4 + 2].x > 0 &&
                vertices[i * 4 + 3].x > 0))
                {
                    for (int j = 0; j < 4; ++j)
                    {
                        optimizeVerticesRight[rightIndex] = vertices[i * 4 + j];
                        optimizeUvsRight[rightIndex] = uvs[i * 4 + j];
                        ++rightIndex;
                    }
                    for (int j = 0; j < 6; ++j)
                    {
                        optimizeIndicesRight[rightIndicesIndex] = indices[i * 6 + j] - skipCountRight * 4;
                        ++rightIndicesIndex;
                    }
                }
                else
                {
                    ++skipCountRight;
                }
                //左なし
                if (!(vertices[i * 4 + 0].x < 0 &&
                vertices[i * 4 + 1].x < 0 &&
                vertices[i * 4 + 2].x < 0 &&
                vertices[i * 4 + 3].x < 0))
                {
                    for (int j = 0; j < 4; ++j)
                    {
                        optimizeVerticesLeft[leftIndex] = vertices[i * 4 + j];
                        optimizeUvsLeft[leftIndex] = uvs[i * 4 + j];
                        ++leftIndex;
                    }
                    for (int j = 0; j < 6; ++j)
                    {
                        optimizeIndicesLeft[leftIndicesIndex] = indices[i * 6 + j] - skipCountLeft * 4;
                        ++leftIndicesIndex;
                    }
                }
                else
                {
                    ++skipCountLeft;
                }
            }
            else
            {
                ++skipCount;
                ++skipCountRight;
                ++skipCountLeft;
            }
        }
        //メッシュの生成
        optimizeCubeMesh = new Mesh();
        optimizeCubeMeshRight = new Mesh();
        optimizeCubeMeshLeft = new Mesh();
        optimizeCubeMesh.vertices = optimizeVertices;
        optimizeCubeMeshRight.vertices = optimizeVerticesRight;
        optimizeCubeMeshLeft.vertices = optimizeVerticesLeft;
        optimizeCubeMesh.uv = optimizeUvs;
        optimizeCubeMeshRight.uv = optimizeUvsRight;
        optimizeCubeMeshLeft.uv = optimizeUvsLeft;
        optimizeCubeMesh.triangles = optimizeIndices;
        optimizeCubeMeshRight.triangles = optimizeIndicesRight;
        optimizeCubeMeshLeft.triangles = optimizeIndicesLeft;
        optimizeCubeMesh.RecalculateNormals();
        optimizeCubeMeshRight.RecalculateNormals();
        optimizeCubeMeshLeft.RecalculateNormals();
    }

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="parent">格納する親オブジェクト</param>
    public void Initialize(GameObject parent)
    {
        mesh = new Mesh();
        //毎回GetLengthすると重いので、前もって変数に格納しておく
        int length0 = BlockArray.GetLength(0);
        int length1 = BlockArray.GetLength(1);
        int length2 = BlockArray.GetLength(2);
        int[] indexs = new int[8];
        for (int i = 0; i < length0; ++i)
        {
            for (int j = 0; j < length1; ++j)
            {
                for (int k = 0; k < length2; ++k)
                {
                    blockInfo = BlockArray[i, j, k];
                    if (blockInfo.MaterialNumber != 0) ++BlockNum[i, j];
                    if (!blockInfo.IsEnable || blockInfo.isSurround) continue;
                    //位置を格納
                    infos[blockInfo.MaterialNumber, indexs[blockInfo.MaterialNumber]++] = blockInfo;
                }
            }
        }
        //マテリアルごとにメッシュ統合
        for (int i = 0; i < 8; ++i)
        {
            CombineMeshs[i] = new CombineMeshInfo();
            CreateCombineMeshObject(BlockCreater.GetInstance().mats[i].name, CombineMeshs[i]);
            CombineMeshs[i].obj.transform.parent = parent.transform;
            CombineMeshs[i].renderer.sharedMaterial = BlockCreater.GetInstance().mats[i];
            mesh = new Mesh();
            CombineInstance[] instances = new CombineInstance[indexs[i]];
            for (int j = 0; j < indexs[i]; ++j)
            {
                instances[j] = infos[i, j].cmesh;
            }
            //先にメッシュを統合してから入れたほうが軽い
            mesh.CombineMeshes(instances);
            CombineMeshs[i].meshFilter.mesh = mesh;
        }
    }
}
