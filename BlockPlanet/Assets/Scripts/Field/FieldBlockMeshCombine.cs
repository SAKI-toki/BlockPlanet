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
    CombineMeshInfo[] combineMeshs = new CombineMeshInfo[8];
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
    protected int[,] blockNums = new int[BlockMapSize.LineN, BlockMapSize.RowN];
    BlockInfo blockInfo;

    BlockInfo[,] infos = new BlockInfo[8, BlockMapSize.LineN * BlockMapSize.RowN];

    public void CreateMesh()
    {
        //更新フラグが立っていなかったら何もしない
        if (!updateMeshFlg) return;
        updateMeshFlg = false;
        int length0 = blockArray.GetLength(0);
        int length1 = blockArray.GetLength(1);
        int length2 = blockArray.GetLength(2);
        int[] indexs = new int[8];
        for (int i = 0; i < length0; ++i)
        {
            for (int j = 0; j < length1; ++j)
            {
                //ブロックがない場所はループしない
                if (blockNums[i, j] == 0) continue;
                for (int k = 0; k < length2; ++k)
                {
                    blockInfo = blockArray[i, j, k];
                    if (!blockInfo.isEnable ||
                        blockInfo.isSurround ||
                        blockInfo.materialNumber == 0) continue;
                    //位置を格納
                    infos[blockInfo.materialNumber, indexs[blockInfo.materialNumber]++] = blockInfo;
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
            combineMeshs[i].meshFilter.mesh = mesh;
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
        combineMeshInfo.renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    public override void BreakBlock(BlockNumber blockNum)
    {
        updateMeshFlg = true;
        blockArray[blockNum.line, blockNum.row, blockNum.height].isEnable = false;
        --blockNums[blockNum.line, blockNum.row];

        if (blockNum.line < blockArray.GetLength(0) - 1)
        {
            blockArray[blockNum.line + 1, blockNum.row, blockNum.height].isSurround = false;
        }

        if (blockNum.row < blockArray.GetLength(1) - 1)
        {
            blockArray[blockNum.line, blockNum.row + 1, blockNum.height].isSurround = false;
        }

        if (blockNum.row > 0)
        {
            blockArray[blockNum.line, blockNum.row - 1, blockNum.height].isSurround = false;
        }

        if (blockNum.height > 0)
        {
            blockArray[blockNum.line, blockNum.row, blockNum.height - 1].isSurround = false;
        }
    }

    public virtual void BlockIsSurroundUpdate()
    {
        for (int i = 1; i < blockArray.GetLength(0); ++i)
        {
            for (int j = 1; j < blockArray.GetLength(1) - 1; ++j)
            {
                for (int k = 0; k < blockArray.GetLength(2) - 1; ++k)
                {
                    //壊れないブロックのみ別の処理
                    if (blockArray[i, j, k].materialNumber == 0)
                    {
                        if (blockArray[i - 1, j, k].isEnable && blockArray[i - 1, j, k].materialNumber == 0 &&
                        blockArray[i, j - 1, k].isEnable && blockArray[i, j - 1, k].materialNumber == 0 &&
                        blockArray[i, j + 1, k].isEnable && blockArray[i, j + 1, k].materialNumber == 0 &&
                        blockArray[i, j, k + 1].isEnable && blockArray[i, j, k + 1].materialNumber == 0)
                        {
                            blockArray[i, j, k].isSurround = true;
                        }
                    }
                    else
                    {
                        if (blockArray[i - 1, j, k].isEnable && blockArray[i, j - 1, k].isEnable &&
                        blockArray[i, j + 1, k].isEnable && blockArray[i, j, k + 1].isEnable)
                        {
                            blockArray[i, j, k].isSurround = true;
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
        const int CenterRange = 2;
        if (optimizeCubeMesh == null)
            CreateOptimizeCube(filter);
        if (row < BlockMapSize.RowN / 2 - CenterRange)
        {
            return optimizeCubeMeshLeft;
        }
        if (row > BlockMapSize.RowN / 2 + CenterRange)
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
        int length0 = blockArray.GetLength(0);
        int length1 = blockArray.GetLength(1);
        int length2 = blockArray.GetLength(2);
        int[] indexs = new int[8];
        List<CombineInstance> strongCombineInstanceList = new List<CombineInstance>();
        for (int i = 0; i < length0; ++i)
        {
            for (int j = 0; j < length1; ++j)
            {
                for (int k = 0; k < length2; ++k)
                {
                    blockInfo = blockArray[i, j, k];
                    if (blockInfo.materialNumber != 0) ++blockNums[i, j];
                    if (!blockInfo.isEnable || blockInfo.isSurround) continue;
                    //位置を格納
                    if (blockInfo.materialNumber == 0)
                    {
                        strongCombineInstanceList.Add(blockInfo.cmesh);
                    }
                    else
                    {
                        infos[blockInfo.materialNumber, indexs[blockInfo.materialNumber]++] = blockInfo;
                    }
                }
            }
        }
        //マテリアルごとにメッシュ統合
        for (int i = 0; i < 8; ++i)
        {
            combineMeshs[i] = new CombineMeshInfo();
            CreateCombineMeshObject(BlockCreater.GetInstance().mats[i].name, combineMeshs[i]);
            combineMeshs[i].obj.transform.parent = parent.transform;
            combineMeshs[i].renderer.sharedMaterial = BlockCreater.GetInstance().mats[i];
            mesh = new Mesh();
            if (i == 0)
            {
                mesh.CombineMeshes(strongCombineInstanceList.ToArray());
            }
            else
            {
                CombineInstance[] instances = new CombineInstance[indexs[i]];
                for (int j = 0; j < indexs[i]; ++j)
                {
                    instances[j] = infos[i, j].cmesh;
                }
                //先にメッシュを統合してから入れたほうが軽い
                mesh.CombineMeshes(instances);
            }
            combineMeshs[i].meshFilter.mesh = mesh;
        }
    }
}
