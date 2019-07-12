using UnityEngine;
using System.IO;

public class MakeMapManager : MonoBehaviour
{
    [SerializeField]
    GameObject[] Cubes;
    [SerializeField]
    GameObject StrongCube;
    int[,] BlockArray = new int[BlockMapSize.line_n, BlockMapSize.row_n];
    bool[,] IsStrongArray = new bool[BlockMapSize.line_n, BlockMapSize.row_n];
    GameObject[,,] BlockObjectArray = new GameObject[BlockMapSize.line_n, BlockMapSize.row_n, BlockMapSize.height_n];

    [SerializeField]
    Transform CursorTransform;
    //生成したブロックの親オブジェクト
    GameObject FieldParent;

    [SerializeField]
    GameObject[] Players = new GameObject[4];

    Vector2Int[] PlayerPositions = new Vector2Int[4];

    //生成したcsvのパス
    const string Path = "Assets/Resources/csv/MakeMap";
    delegate void StateType();
    StateType State;
    string StateName;

    void Start()
    {
        CursorTransform.position = Vector3.zero;
        FieldParent = new GameObject("FieldObject");
        State = BlockState;
    }

    void Update()
    {
        State();
    }

    void BlockState()
    {
        StateName = "ブロックセット";
        //ブロックを増やす
        if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            AddBlock();
        }
        //ブロックを減らす
        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            SubBlock();
        }
        //壊れる、壊れないブロックを切り替える
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SwitchStrong();
        }
        //押した数字までブロックを増減する
        for (int i = 1; i < 8; ++i)
        {
            if (Input.GetKey((KeyCode)((int)KeyCode.Alpha0 + i)) ||
            Input.GetKey((KeyCode)((int)KeyCode.Keypad0 + i)))
            {
                SetBlock(i);
                break;
            }
        }
        CursorMove();
        //ステートの進行
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            State = PlayerState;
        }
    }

    void PlayerState()
    {
        StateName = "プレイヤーセット";
        //押した数字のプレイヤーを生成する
        for (int i = 1; i < 5; ++i)
        {
            if (Input.GetKey((KeyCode)((int)KeyCode.Alpha0 + i)) ||
            Input.GetKey((KeyCode)((int)KeyCode.Keypad0 + i)))
            {
                SetPlayer(i);
                break;
            }
        }
        CursorMove();
        //ステートの後退
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            foreach (var player in Players)
            {
                player.SetActive(false);
            }
            State = BlockState;
        }
        //ステートの進行
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            foreach (var player in Players)
            {
                if (player.activeSelf == false) return;
            }
            MakeCsv();
            Clear();
            State = BlockState;
        }
    }

    void CursorMove()
    {
        Vector3 position = CursorTransform.position;
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            ++position.z;
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            --position.z;
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++position.x;
        }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --position.x;
        }
        position.Set(Mathf.Clamp(position.x, 0, BlockArray.GetLength(1) - 1),
                    position.y,
                    Mathf.Clamp(position.z, 0, BlockArray.GetLength(0) - 1));
        position.y = BlockArray[(int)CursorTransform.position.z, (int)CursorTransform.position.x] + 1;
        CursorTransform.position = position;
    }

    void AddBlock()
    {
        //既に最大なら何もしない
        if (BlockArray[(int)CursorTransform.position.z, (int)CursorTransform.position.x] >= BlockMapSize.height_n) return;
        //ブロックの数を増やす
        ++BlockArray[(int)CursorTransform.position.z, (int)CursorTransform.position.x];
        //一番上にブロックを生成
        BlockObjectArray[(int)CursorTransform.position.z,
                        (int)CursorTransform.position.x,
                        BlockArray[(int)CursorTransform.position.z, (int)CursorTransform.position.x] - 1]
         = Instantiate(
             IsStrongArray[(int)CursorTransform.position.z, (int)CursorTransform.position.x] ?
             StrongCube :
             Cubes[BlockArray[(int)CursorTransform.position.z, (int)CursorTransform.position.x] - 1],
                        new Vector3(CursorTransform.position.x,
                                    BlockArray[(int)CursorTransform.position.z, (int)CursorTransform.position.x],
                                    CursorTransform.position.z),
                       Quaternion.identity);
        //親オブジェクトをセット
        BlockObjectArray[(int)CursorTransform.position.z,
                        (int)CursorTransform.position.x,
                        BlockArray[(int)CursorTransform.position.z, (int)CursorTransform.position.x] - 1].transform.parent
                        = FieldParent.transform;
    }

    void SubBlock()
    {
        //既に0なら何もしない
        if (BlockArray[(int)CursorTransform.position.z, (int)CursorTransform.position.x] <= 0) return;
        //一番上のブロックを削除
        Destroy(BlockObjectArray[(int)CursorTransform.position.z,
                        (int)CursorTransform.position.x,
                        BlockArray[(int)CursorTransform.position.z, (int)CursorTransform.position.x] - 1]);
        //ブロックの数を減らす
        --BlockArray[(int)CursorTransform.position.z, (int)CursorTransform.position.x];
    }

    void SetBlock(int blockNum)
    {
        int blockNumber = BlockArray[(int)CursorTransform.position.z, (int)CursorTransform.position.x];
        if (blockNumber == blockNum) return;
        int diff = blockNum - blockNumber;
        //差分だけ増減させる
        if (diff > 0)
        {
            for (int i = 0; i < diff; ++i)
            {
                AddBlock();
            }
        }
        else
        {
            for (int i = 0; i > diff; --i)
            {
                SubBlock();
            }
        }
    }

    void SwitchStrong()
    {
        IsStrongArray[(int)CursorTransform.position.z, (int)CursorTransform.position.x] =
            !IsStrongArray[(int)CursorTransform.position.z, (int)CursorTransform.position.x];
        int blockNumber = BlockArray[(int)CursorTransform.position.z, (int)CursorTransform.position.x];
        if (blockNumber == 0) return;
        for (int i = 0; i < blockNumber; ++i)
        {
            SubBlock();
        }
        for (int i = 0; i < blockNumber; ++i)
        {
            AddBlock();
        }
    }

    void SetPlayer(int number)
    {
        if (BlockArray[(int)CursorTransform.position.z, (int)CursorTransform.position.x] == 0) return;
        for (int i = 0; i < PlayerPositions.Length; ++i)
        {
            if (i != (number - 1) && Players[i].activeSelf &&
            Mathf.Abs(PlayerPositions[i].x - (int)CursorTransform.position.z) < 3 &&
            Mathf.Abs(PlayerPositions[i].y - (int)CursorTransform.position.x) < 3)
            {
                return;
            }
        }
        Players[number - 1].SetActive(true);
        Players[number - 1].transform.position = CursorTransform.position;
        PlayerPositions[number - 1].Set((int)CursorTransform.position.z, (int)CursorTransform.position.x);
        Debug.Log(PlayerPositions[number - 1]);
    }

    void MakeCsv()
    {
        Directory.CreateDirectory(Path);
        string csvString = "";
        for (int i = 0; i < BlockMapSize.line_n; ++i)
        {
            bool first = true;
            for (int j = 0; j < BlockMapSize.row_n; ++j)
            {
                if (!first)
                {
                    csvString += ",";
                }
                first = false;
                int blockNumber = BlockArray[i, j];
                if (blockNumber != 0)
                {
                    blockNumber += IsStrongArray[i, j] ? 10 : 0;
                    for (int k = 0; k < PlayerPositions.Length; ++k)
                    {
                        if (PlayerPositions[k] == new Vector2Int(i, j))
                        {
                            blockNumber += (k + 1) * 100;
                        }
                    }
                }
                csvString += blockNumber.ToString();
            }
            csvString += "\n";
        }
        int number = Directory.GetFiles(Path, "*").Length;
        StreamWriter sw = new StreamWriter(Path + "/MapData" + number.ToString() + ".csv", false);
        sw.Write(csvString);
        sw.Flush();
        sw.Close();
    }

    void Clear()
    {
        BlockArray = new int[BlockMapSize.line_n, BlockMapSize.row_n];
        foreach (var block in BlockObjectArray)
        {
            Destroy(block);
        }
        foreach (var player in Players)
        {
            player.SetActive(false);
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(500, 20, 1000, 1000), StateName);
    }
}
