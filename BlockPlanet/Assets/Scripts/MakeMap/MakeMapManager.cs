using UnityEngine;
using System.IO;

public class MakeMapManager : MonoBehaviour
{
#if UNITY_EDITOR
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

    //プレイヤーの位置
    Vector2Int[] PlayerPositions = new Vector2Int[4];

    //生成したcsvのパス
    const string Path = "Assets/Resources/csv/MakeMap";
    delegate void StateType();
    StateType State;
    string StateName;
    float[] CursorMoveTime = new float[8];

    const string BlockStateName = "ブロックセット";
    const string PlayerStateName = "プレイヤーセット";

    bool DisplayUI = true;

    Vector3 CameraInitPosition;
    Quaternion CameraInitRotation;
    [SerializeField]
    GameObject CameraObject;

    [SerializeField]
    TextAsset CsvAsset;
    bool IsTracking = false;
    float TrackingDistance = 0.0f;

    void Start()
    {
        CameraInitPosition = CameraObject.transform.position;
        CameraInitRotation = CameraObject.transform.rotation;
        FieldParent = new GameObject("FieldObject");
        FieldParent.isStatic = true;
        State = BlockState;
    }

    void Update()
    {
        if (CsvAsset)
        {
            Clear();
            LoadCsv(CsvAsset);
            CsvAsset = null;
        }
        //UIの表示・非表示
        if (Input.GetKeyDown(KeyCode.M)) DisplayUI = !DisplayUI;
        CursorMove();
        State();
    }

    void LateUpdate()
    {
        CameraControl();
    }

    void BlockState()
    {
        StateName = BlockStateName;
        //ブロックを増やす
        if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            AddBlock(new Vector2Int((int)CursorTransform.position.x, (int)CursorTransform.position.z));
        }
        //ブロックを減らす
        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            SubBlock(new Vector2Int((int)CursorTransform.position.x, (int)CursorTransform.position.z));
        }
        //壊れる、壊れないブロックを切り替える
        if (Input.GetKey(KeyCode.Z))
        {
            SwitchStrong(true, new Vector2Int((int)CursorTransform.position.x, (int)CursorTransform.position.z));
        }
        if (Input.GetKey(KeyCode.X))
        {
            SwitchStrong(false, new Vector2Int((int)CursorTransform.position.x, (int)CursorTransform.position.z));
        }
        //押した数字までブロックを増減する
        for (int i = 0; i < 8; ++i)
        {
            if (Input.GetKey((KeyCode)((int)KeyCode.Alpha0 + i)) ||
            Input.GetKey((KeyCode)((int)KeyCode.Keypad0 + i)))
            {
                SetBlock(i, new Vector2Int((int)CursorTransform.position.x, (int)CursorTransform.position.z));
                break;
            }
        }
        //ステートの進行
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            State = PlayerState;
        }
    }

    void PlayerState()
    {
        StateName = PlayerStateName;
        //押した数字のプレイヤーを生成する
        for (int i = 1; i < 5; ++i)
        {
            if (Input.GetKey((KeyCode)((int)KeyCode.Alpha0 + i)) ||
            Input.GetKey((KeyCode)((int)KeyCode.Keypad0 + i)))
            {
                SetPlayer(i, new Vector2Int((int)CursorTransform.position.x, (int)CursorTransform.position.z));
                break;
            }
        }
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
        //移動の間隔
        float moveTimeLimit = 0.3f;
        //Shiftキーを押すと速く移動する
        if (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift))
        {
            moveTimeLimit = 0.05f;
        }
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            CursorMoveTime[0] += Time.deltaTime;
            if (CursorMoveTime[0] > moveTimeLimit)
            {
                CursorMoveTime[0] = 0.0f;
                ++position.z;
            }
        }
        else
        {
            CursorMoveTime[0] = moveTimeLimit;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            CursorMoveTime[1] += Time.deltaTime;
            if (CursorMoveTime[1] > moveTimeLimit)
            {
                CursorMoveTime[1] = 0.0f;
                --position.z;
            }
        }
        else
        {
            CursorMoveTime[1] = moveTimeLimit;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            CursorMoveTime[2] += Time.deltaTime;
            if (CursorMoveTime[2] > moveTimeLimit)
            {
                CursorMoveTime[2] = 0.0f;
                ++position.x;
            }
        }
        else
        {
            CursorMoveTime[2] = moveTimeLimit;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            CursorMoveTime[3] += Time.deltaTime;
            if (CursorMoveTime[3] > moveTimeLimit)
            {
                CursorMoveTime[3] = 0.0f;
                --position.x;
            }
        }
        else
        {
            CursorMoveTime[3] = moveTimeLimit;
        }
        //右斜め上
        if (Input.GetKey(KeyCode.O))
        {
            CursorMoveTime[4] += Time.deltaTime;
            if (CursorMoveTime[4] > moveTimeLimit)
            {
                CursorMoveTime[4] = 0.0f;
                ++position.z;
                ++position.x;
            }
        }
        else
        {
            CursorMoveTime[4] = moveTimeLimit;
        }
        //左斜め上
        if (Input.GetKey(KeyCode.I))
        {
            CursorMoveTime[5] += Time.deltaTime;
            if (CursorMoveTime[5] > moveTimeLimit)
            {
                CursorMoveTime[5] = 0.0f;
                ++position.z;
                --position.x;
            }
        }
        else
        {
            CursorMoveTime[5] = moveTimeLimit;
        }
        //右斜め下
        if (Input.GetKey(KeyCode.L))
        {
            CursorMoveTime[6] += Time.deltaTime;
            if (CursorMoveTime[6] > moveTimeLimit)
            {
                CursorMoveTime[6] = 0.0f;
                --position.z;
                ++position.x;
            }
        }
        else
        {
            CursorMoveTime[6] = moveTimeLimit;
        }
        //左斜め下
        if (Input.GetKey(KeyCode.K))
        {
            CursorMoveTime[7] += Time.deltaTime;
            if (CursorMoveTime[7] > moveTimeLimit)
            {
                CursorMoveTime[7] = 0.0f;
                --position.z;
                --position.x;
            }
        }
        else
        {
            CursorMoveTime[7] = moveTimeLimit;
        }
        //位置の調整
        position.Set(Mathf.Clamp(position.x, 0, BlockArray.GetLength(1) - 1),
                    BlockArray[(int)CursorTransform.position.z, (int)CursorTransform.position.x] + 1,
                    Mathf.Clamp(position.z, 0, BlockArray.GetLength(0) - 1));
        //位置のセット
        CursorTransform.position = position;
    }

    void AddBlock(Vector2Int position)
    {
        //既に最大なら何もしない
        if (BlockArray[position.y, position.x] >= BlockMapSize.height_n) return;
        //ブロックの数を増やす
        ++BlockArray[position.y, position.x];
        //一番上にブロックを生成
        BlockObjectArray[position.y, position.x, BlockArray[position.y, position.x] - 1] =
            Instantiate(IsStrongArray[position.y, position.x] ? StrongCube : Cubes[BlockArray[position.y, position.x] - 1]);
        //親オブジェクトをセット
        BlockObjectArray[position.y, position.x, BlockArray[position.y, position.x] - 1].transform.parent = FieldParent.transform;
        BlockObjectArray[position.y, position.x, BlockArray[position.y, position.x] - 1].transform.position =
            new Vector3(position.x, BlockArray[position.y, position.x], position.y);
    }

    void SubBlock(Vector2Int position)
    {
        //既に0なら何もしない
        if (BlockArray[position.y, position.x] <= 0) return;
        //一番上のブロックを削除
        Destroy(BlockObjectArray[position.y, position.x, BlockArray[position.y, position.x] - 1]);
        //ブロックの数を減らす
        --BlockArray[position.y, position.x];
        if (BlockArray[position.y, position.x] == 0)
        {
            IsStrongArray[position.y, position.x] = false;
        }
    }

    void SetBlock(int blockNum, Vector2Int position)
    {
        int blockNumber = BlockArray[position.y, position.x];
        if (blockNumber == blockNum) return;
        int diff = blockNum - blockNumber;
        //差分だけ増減させる
        if (diff > 0)
        {
            for (int i = 0; i < diff; ++i)
                AddBlock(position);
        }
        else
        {
            for (int i = 0; i > diff; --i)
                SubBlock(position);
        }
    }

    void SwitchStrong(bool isToStrong, Vector2Int position)
    {
        if (IsStrongArray[position.y, position.x] == isToStrong) return;
        IsStrongArray[position.y, position.x] = isToStrong;
        int blockNumber = BlockArray[position.y, position.x];
        if (blockNumber == 0) return;
        for (int i = 0; i < blockNumber; ++i)
            SubBlock(position);
        for (int i = 0; i < blockNumber; ++i)
            AddBlock(position);
    }

    void SetPlayer(int number, Vector2Int position)
    {
        //プレイヤー同士の距離の制限
        const int DistanceLimit = 4;
        if (BlockArray[position.y, position.x] == 0) return;
        for (int i = 0; i < PlayerPositions.Length; ++i)
        {
            //距離範囲外かどうか
            if (i != (number - 1) && Players[i].activeSelf &&
            Mathf.Abs(PlayerPositions[i].x - position.x) < DistanceLimit &&
            Mathf.Abs(PlayerPositions[i].y - position.y) < DistanceLimit)
            {
                return;
            }
        }
        Players[number - 1].SetActive(true);
        Players[number - 1].transform.position = new Vector3(position.x, BlockArray[position.y, position.x] + 3, position.y);
        PlayerPositions[number - 1] = position;
        Players[number - 1].transform.LookAt(
            new Vector3(BlockMapSize.row_n / 2.0f, Players[number - 1].transform.position.y, BlockMapSize.line_n / 2.0f));
    }

    void CameraControl()
    {
        //追尾モード
        if (Input.GetKeyDown(KeyCode.T))
        {
            IsTracking = true;
            CameraObject.transform.eulerAngles = new Vector3(90, 0, 0);
            TrackingDistance = 60;
        }
        //俯瞰モード
        if (Input.GetKeyDown(KeyCode.C))
        {
            IsTracking = false;
            CameraObject.transform.position = new Vector3(25.5f, 60, 25.5f);
            CameraObject.transform.eulerAngles = new Vector3(90, 0, 0);
        }
        //デフォルトモード
        if (Input.GetKeyDown(KeyCode.V))
        {
            IsTracking = false;
            CameraObject.transform.position = CameraInitPosition;
            CameraObject.transform.rotation = CameraInitRotation;
        }
        if (IsTracking)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0) --TrackingDistance;
            if (scroll < 0) ++TrackingDistance;
            TrackingDistance = Mathf.Clamp(TrackingDistance, 10, 60);
            Vector3 position = CursorTransform.position;
            position.y = TrackingDistance;
            CameraObject.transform.position = position;
        }
    }

    void MakeCsv()
    {
        //フォルダの作成
        Directory.CreateDirectory(Path);
        string csvString = "";
        for (int i = 0; i < BlockMapSize.line_n; ++i)
        {
            bool first = true;
            for (int j = 0; j < BlockMapSize.row_n; ++j)
            {
                //最初以外','をつける
                if (!first)
                {
                    csvString += ",";
                }
                first = false;
                //ブロックの数を取得
                int blockNumber = BlockArray[i, j];
                if (blockNumber != 0)
                {
                    //壊れないブロックなら+10
                    blockNumber += IsStrongArray[i, j] ? 10 : 0;
                    //プレイヤーの位置かチェックする
                    for (int k = 0; k < PlayerPositions.Length; ++k)
                    {
                        //プレイヤーの位置ならそのプレイヤー*100加算する
                        if (PlayerPositions[k] == new Vector2Int(j, i))
                        {
                            blockNumber += (k + 1) * 100;
                            break;
                        }
                    }
                }
                //stringに追加
                csvString += blockNumber.ToString();
            }
            csvString += "\n";
        }
        int number = Directory.GetFiles(Path, "*").Length / 2;
        StreamWriter sw = new StreamWriter(Path + "/MapData" + number.ToString() + ".csv", false);
        sw.Write(csvString);
        sw.Flush();
        sw.Close();
    }

    void LoadCsv(TextAsset textAsset)
    {
        //改行ごとに格納
        string[] lineString = textAsset.text.Split('\n');
        Vector2Int position = new Vector2Int();
        for (int z = 0; z < BlockMapSize.line_n; z++)
        {
            //カンマごとに格納
            string[] rowString = lineString[z].Split(',');
            for (int x = 0; x < BlockMapSize.line_n; x++)
            {
                //string型をint型にパース
                int number = int.Parse(rowString[x]);

                position.Set(x, z);
                //プレイヤーの位置
                if (number >= 100)
                {
                    int playerNumber = number / 100;
                    number -= playerNumber * 100;
                }
                if (number != 0)
                {
                    SwitchStrong(number >= 10, position);
                    SetBlock(number % 10, position);
                }
            }
        }
    }

    void Clear()
    {
        CursorTransform.position = Vector3.zero;
        BlockArray = new int[BlockMapSize.line_n, BlockMapSize.row_n];
        IsStrongArray = new bool[BlockMapSize.line_n, BlockMapSize.row_n];
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
        if (DisplayUI)
        {
            if (StateName == BlockStateName)
            {
                GUI.Label(new Rect(300, 20, 1000, 1000), StateName +
                "\n+ : ブロックを一段増やす\n- : ブロックを一段減らす\nWASD,矢印 : 移動(Shiftを押すと高速移動)\n" +
                "IOKL : 斜め移動(Shiftを押すと高速移動)\nZ : 壊れないブロックに変換\nX : 壊れるブロックに変換\n" +
                "0~7 : 押した数の段数にする\nEnter : プレイヤーをセットするステートに移行\n" +
                "C : 上からマップを見る\nV : 初期位置からマップを見る");
            }
            else if (StateName == PlayerStateName)
            {
                GUI.Label(new Rect(300, 20, 1000, 1000), StateName +
                "\nWASD,矢印 : 移動(Shiftを押すと高速移動)\nIOKL : 斜め移動(Shiftを押すと高速移動)\n" +
                "1~4 : 押した数のプレイヤーをセットする\n(近すぎるとセットできません)\n" +
                "Enter : CSVを生成し、マップをクリア\nBackSpace : ブロックをセットするステートに戻る\n" +
                "C : 上からマップを見る\nV : 初期位置からマップを見る");
            }
        }
        GUI.Label(new Rect(20, 350, 1000, 1000),
         "縦：" + (CursorTransform.position.z + 1).ToString("##") + "\n" +
         "横：" + (CursorTransform.position.x + 1).ToString("##"));
        GUI.Label(new Rect(20, 400, 1000, 1000), "M : 説明の表示・非表示");
    }
#endif
}
