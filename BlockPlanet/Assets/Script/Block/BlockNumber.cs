using UnityEngine;

public class BlockNumber : MonoBehaviour
{
    [System.NonSerialized]
    public int line, row, height;

    public void SetNum(int _line, int _row, int _height)
    { line = _line; row = _row; height = _height; }
}