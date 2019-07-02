using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;

public class DynamicBlockMeshCombine : BlockMap
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
    bool updateMeshFlg = false;
    public void Initialize()
    {
        mesh = new Mesh();
        for (int i = 0; i < 8; ++i)
        {
            combines[i] = new List<CombineInstance>();
        }
        foreach (var block in BlockArray)
        {
            if (!block.IsEnable) continue;
            break;
        }
        foreach (var block in BlockArray)
        {
            if (!block.IsEnable || block.isSurround) continue;
            //位置を格納
            combines[block.MaterialNumber].Add(block.cmesh);
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
        foreach (var block in BlockArray)
        {
            if (!block.IsEnable || block.isSurround || block.MaterialNumber == 0) continue;
            //位置を格納
            combines[block.MaterialNumber].Add(block.cmesh);
        }
        for (int i = 1; i < 8; ++i)
        {
            mesh = new Mesh();
            //先にメッシュを統合してから入れたほうが軽い
            mesh.CombineMeshes(combines[i].ToArray());
            CombineMeshs[i].meshFilter.mesh = mesh;
        }
    }


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
        if (block_num.line < BlockArray.GetLength(0) - 1)
            BlockArray[block_num.line + 1, block_num.row, block_num.height].isSurround = false;

        // if (block_num.line > 0)
        //    BlockArray[block_num.line - 1, block_num.row, block_num.height].isSurround = false;

        if (block_num.row < BlockArray.GetLength(1) - 1)
            BlockArray[block_num.line, block_num.row + 1, block_num.height].isSurround = false;

        if (block_num.row > 0)
            BlockArray[block_num.line, block_num.row - 1, block_num.height].isSurround = false;

        // if (block_num.height < BlockArray.GetLength(2) - 1)
        //     BlockArray[block_num.line, block_num.row, block_num.height + 1].isSurround = false;

        if (block_num.height > 0)
            BlockArray[block_num.line, block_num.row, block_num.height - 1].isSurround = false;
    }

    public void BlockIsSurroundUpdate()
    {
        // for (int i = 1; i < BlockArray.GetLength(0) - 1; ++i)
        // {
        //     for (int j = 1; j < BlockArray.GetLength(1) - 1; ++j)
        //     {
        //         for (int k = 1; k < BlockArray.GetLength(2) - 1; ++k)
        //         {
        //             BlockArray[i, j, k].isSurround = IsSurround(i, j, k);
        //         }
        //     }
        // }
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
}
