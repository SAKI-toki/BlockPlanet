using UnityEngine;

/// <summary>
/// ブロックの番号
/// </summary>
public class BlockNumber : MonoBehaviour
{
    [System.NonSerialized]
    public int line = 0, row = 0, height = 0;

    public void SetNum(int line_n, int row_n, int height_n)
    { line = line_n; row = row_n; height = height_n; }
}