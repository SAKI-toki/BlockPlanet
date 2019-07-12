using UnityEngine;

/// <summary>
/// 自動でマップを生成する
/// </summary>
static class AutoGeneration
{
    class BlockNumber
    {
        public int n;
    }

    static int[,] blockArray = new int[BlockCreater.line_n, BlockCreater.row_n];
    static BlockNumber[,] oneQuaterBlockArray = new BlockNumber[BlockCreater.line_n / 4, BlockCreater.row_n / 4];

    static public int[,] Generate()
    {
        foreach (var block in oneQuaterBlockArray)
        {
            block.n += Random.Range(0, 8);
            if (block.n == 0) continue;
            //壊れないブロック
            if (Random.Range(0, 2) == 0)
            {
                block.n += 10;
            }
        }

        //プレイヤーの位置
        oneQuaterBlockArray[oneQuaterBlockArray.GetLength(0) / 2, oneQuaterBlockArray.GetLength(1) / 2].n += 100;
        //四分の一をコピー
        for (int i = 0; i < oneQuaterBlockArray.GetLength(0); ++i)
        {
            for (int j = 0; j < oneQuaterBlockArray.GetLength(1); ++j)
            {
                blockArray[i, j] = oneQuaterBlockArray[i, j].n;
                blockArray[i, BlockCreater.row_n - j - 1] =
                    oneQuaterBlockArray[i, j].n + ((oneQuaterBlockArray[i, j].n > 100) ? 100 : 0);
                blockArray[BlockCreater.line_n - i - 1, j] =
                    oneQuaterBlockArray[i, j].n + ((oneQuaterBlockArray[i, j].n > 100) ? 200 : 0);
                blockArray[BlockCreater.line_n - i - 1, BlockCreater.row_n - j - 1] =
                    oneQuaterBlockArray[i, j].n + ((oneQuaterBlockArray[i, j].n > 100) ? 300 : 0);
            }
        }

        return blockArray;
    }
}
