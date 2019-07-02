using UnityEngine;
static public class FieldAutoCreateImpl
{
    public static int[,] map = new int[BlockCreater.line_n, BlockCreater.row_n];

    static public void AutoCreate()
    {
        for (int i = 0; i < BlockCreater.line_n / 2; ++i)
        {
            for (int j = 0; j < BlockCreater.row_n / 2; ++j)
            {
                SetNumber(i, j, Random.Range(0, 8));
            }
        }
        SetPlayer(BlockCreater.line_n / 4, BlockCreater.row_n / 4);
    }

    static void SetNumber(int line, int row, int number)
    {
        map[line, row] = number;
        map[line, BlockCreater.row_n - row - 1] = number;
        map[BlockCreater.line_n - line - 1, row] = number;
        map[BlockCreater.line_n - line - 1, BlockCreater.row_n - row - 1] = number;
    }

    static void SetPlayer(int line, int row)
    {
        map[line, row] += 100;
        map[line, BlockCreater.row_n - row - 1] += 200;
        map[BlockCreater.line_n - line - 1, row] += 300;
        map[BlockCreater.line_n - line - 1, BlockCreater.row_n - row - 1] += 400;
    }
}