using UnityEngine;

/// <summary>
/// ブロックの番号
/// </summary>
public class BlockNumber : MonoBehaviour
{
    [System.NonSerialized]
    public int _line, _row, _height;

    public void SetNum(int line_n, int row_n, int height_n)
    { _line = line_n; _row = row_n; _height = height_n; }

    public int line { get { return _line; } }
    public int row { get { return _row; } }
    public int height { get { return _height; } }
}