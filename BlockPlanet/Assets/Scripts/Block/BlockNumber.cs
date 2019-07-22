using UnityEngine;

/// <summary>
/// ブロックの番号
/// </summary>
public class BlockNumber : MonoBehaviour
{
    [System.NonSerialized]
    public int line = 0, row = 0, height = 0;

    public void SetNum(int lineN, int rowN, int heightN)
    { line = lineN; row = rowN; height = heightN; }
}