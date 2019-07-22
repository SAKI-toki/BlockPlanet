using UnityEngine;

/// <summary>
/// 自動でマップを生成する
/// </summary>
static public class AutoGeneration
{
    //マップ全体の配列,Generate関数で返す配列
    static int[,] blockArray = new int[BlockMapSize.LineN, BlockMapSize.RowN];
    //上のマップの4分の1の配列
    static int[,] oneQuaterBlockArray = new int[BlockMapSize.LineN / 2, BlockMapSize.RowN / 2];

    /// <summary>
    /// 自動生成
    /// </summary>
    /// <param name="maxHeightDiff">最大の段差の差</param>
    /// <param name="sameHeightPercent">同じ高さになる確率</param>
    /// <returns>生成した配列</returns>
    static public int[,] Generate(int maxHeightDiff, float sameHeightPercent)
    {
        //ランダムに角の高さを決める
        int randomPlayerPositionRandomHeight = Random.Range(1, BlockMapSize.HeightN + 1);
        //角に値をセット
        for (int i = 0; i < 3; ++i)
        {
            for (int j = 0; j < 3; ++j)
            {
                oneQuaterBlockArray[i, j] = randomPlayerPositionRandomHeight;
            }
        }

        //ランダムにブロックを配置する
        for (int i = 0; i < oneQuaterBlockArray.GetLength(0); ++i)
        {
            for (int j = 0; j < oneQuaterBlockArray.GetLength(1); ++j)
            {
                //初期に設定したところは何もしない
                if (i == 0 && (j == 0 || j == 1 || j == 2) ||
                i == 1 && (j == 0 || j == 1 || j == 2) ||
                i == 2 && (j == 0 || j == 1 || j == 2)) continue;
                //高さの合計
                int heightSum = 0;
                //壊れないブロックの合計
                int strongNum = 0;
                int blockCount = 0;
                if (i != 0)
                {
                    heightSum += oneQuaterBlockArray[i - 1, j] % 10;
                    if (oneQuaterBlockArray[i - 1, j] / 10 > 0)
                    {
                        ++strongNum;
                    }
                    ++blockCount;
                }
                if (j != 0)
                {
                    heightSum += oneQuaterBlockArray[i, j - 1] % 10;
                    if (oneQuaterBlockArray[i, j - 1] / 10 > 0)
                    {
                        ++strongNum;
                    }
                    ++blockCount;
                }
                if (i != 0 && j != 0)
                {
                    heightSum += oneQuaterBlockArray[i - 1, j - 1] % 10;
                    if (oneQuaterBlockArray[i - 1, j - 1] / 10 > 0)
                    {
                        ++strongNum;
                    }
                    ++blockCount;
                }
                //平均の高さにする
                if (Random.Range(0.0f, 1.0f) <= sameHeightPercent)
                {
                    oneQuaterBlockArray[i, j] = Mathf.RoundToInt(1.0f * heightSum / blockCount);
                }
                //平均の値から指定の範囲内に収まるように高さを設定する
                else
                {
                    oneQuaterBlockArray[i, j] = Mathf.Clamp(Random.Range(-maxHeightDiff, maxHeightDiff + 1) + heightSum / blockCount,
                        0, BlockMapSize.HeightN);
                }
                //結果が0ならcontinue
                if (oneQuaterBlockArray[i, j] == 0) continue;
                //壊れないブロック
                if (Random.Range(0.0f, 1.0f) < 0.1f + 0.4f * strongNum / blockCount)
                {
                    oneQuaterBlockArray[i, j] += 10;
                }
            }
        }
        //4分の1を残りの4分の3にコピー
        for (int i = 0; i < oneQuaterBlockArray.GetLength(0); ++i)
        {
            for (int j = 0; j < oneQuaterBlockArray.GetLength(1); ++j)
            {
                blockArray[i, j] = oneQuaterBlockArray[i, j];
                blockArray[i, BlockMapSize.RowN - j - 1] = oneQuaterBlockArray[i, j];
                blockArray[BlockMapSize.LineN - i - 1, j] = oneQuaterBlockArray[i, j];
                blockArray[BlockMapSize.LineN - i - 1, BlockMapSize.RowN - j - 1] = oneQuaterBlockArray[i, j];
            }
        }

        //プレイヤーの位置
        blockArray[BlockMapSize.LineN - 2, 1] = 100;
        blockArray[BlockMapSize.LineN - 2, BlockMapSize.RowN - 2] = 200;
        blockArray[1, 1] = 300;
        blockArray[1, BlockMapSize.RowN - 2] = 400;

        return blockArray;
    }
}
