using UnityEngine;

/// <summary>
/// ブロックの番号
/// </summary>
public class BlockNumber : MonoBehaviour
{
    [System.NonSerialized]
    public int line = 0, row = 0, height = 0;

    /// <summary>
    /// 各値のセット
    /// </summary>
    /// <param name="lineN">縦</param>
    /// <param name="rowN">横</param>
    /// <param name="heightN">高さ</param>
    public void SetNum(int lineN, int rowN, int heightN)
    { line = lineN; row = rowN; height = heightN; }
}