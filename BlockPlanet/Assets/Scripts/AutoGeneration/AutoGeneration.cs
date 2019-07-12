using UnityEngine;

/// <summary>
/// 自動でマップを生成する
/// </summary>
static public class AutoGeneration
{
    static int[,] blockArray = new int[BlockCreater.line_n, BlockCreater.row_n];
    static int[,] oneQuaterBlockArray = new int[BlockCreater.line_n / 4, BlockCreater.row_n / 4];
    static bool isInitialize = false;
    static public int[,] Generate()
    {
        for (int i = 0; i < oneQuaterBlockArray.GetLength(0); ++i)
        {
            for (int j = 0; j < oneQuaterBlockArray.GetLength(1); ++j)
            {
                oneQuaterBlockArray[i, j] = Random.Range(0, 8);
                if (oneQuaterBlockArray[i, j] == 0) continue;
                //壊れないブロック
                if (Random.Range(0, 2) == 0)
                {
                    oneQuaterBlockArray[i, j] += 10;
                }
            }
        }

        //プレイヤーの位置
        oneQuaterBlockArray[oneQuaterBlockArray.GetLength(0) / 2, oneQuaterBlockArray.GetLength(1) / 2] += 100;
        //四分の一をコピー
        for (int i = 0; i < oneQuaterBlockArray.GetLength(0); ++i)
        {
            for (int j = 0; j < oneQuaterBlockArray.GetLength(1); ++j)
            {
                blockArray[i, j] = oneQuaterBlockArray[i, j];
                blockArray[i, BlockCreater.row_n - j - 1] =
                    oneQuaterBlockArray[i, j] + ((oneQuaterBlockArray[i, j] > 100) ? 100 : 0);
                blockArray[BlockCreater.line_n - i - 1, j] =
                    oneQuaterBlockArray[i, j] + ((oneQuaterBlockArray[i, j] > 100) ? 200 : 0);
                blockArray[BlockCreater.line_n - i - 1, BlockCreater.row_n - j - 1] =
                    oneQuaterBlockArray[i, j] + ((oneQuaterBlockArray[i, j] > 100) ? 300 : 0);
            }
        }

        return blockArray;
    }
}
