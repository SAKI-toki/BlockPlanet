using UnityEngine;
using System.IO;

/// <summary>
/// Unity上で簡単にマップを作れる
/// </summary>
public class MakeMapManager : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField]
    GameObject[] cubes;
    [SerializeField]
    GameObject strongCube;
    //ブロックの数
    int[,] blockArray = new int[BlockMapSize.LineN, BlockMapSize.RowN];
    //壊れないブロックかどうか
    bool[,] isStrongArray = new bool[BlockMapSize.LineN, BlockMapSize.RowN];
    GameObject[,,] blockObjectArray = new GameObject[BlockMapSize.LineN, BlockMapSize.RowN, BlockMapSize.HeightN];

    [SerializeField]
    Transform cursorTransform;
    //生成したブロックの親オブジェクト
    GameObject fieldParent;

    [SerializeField]
    GameObject[] players = new GameObject[4];

    //プレイヤーの位置
    Vector2Int[] playerPositions = new Vector2Int[4];

    delegate void StateType();
    StateType state;
    string stateName;
    float[] cursorMoveTime = new float[8];

    const string BlockStateName = "ブロックセット";
    const string PlayerStateName = "プレイヤーセット";

    bool displayUI = true;

    Vector3 cameraInitPosition;
    Quaternion cameraInitRotation;
    [SerializeField]
    GameObject cameraObject;
    [SerializeField, Range(1.0f, 10.0f)]
    float scrollSpeed = 2.0f;

    [SerializeField]
    TextAsset csvAsset;
    bool isTracking = false;
    float trackingDistance = 0.0f;
    [SerializeField]
    bool makeMiddleAsset = false;
    void Start()
    {
        cameraInitPosition = cameraObject.transform.position;
        cameraInitRotation = cameraObject.transform.rotation;
        fieldParent = new GameObject("FieldObject");
        fieldParent.isStatic = true;
        state = BlockState;
    }

    void Update()
    {
        if (csvAsset)
        {
            Clear();
            LoadCsv(csvAsset);
            csvAsset = null;
        }
        //UIの表示・非表示
        if (Input.GetKeyDown(KeyCode.M)) displayUI = !displayUI;
        CursorMove();
        state();
        if (makeMiddleAsset)
        {
            MakeMiddle();
            makeMiddleAsset = false;
        }
    }

    void LateUpdate()
    {
        CameraControl();
    }

    void BlockState()
    {
        stateName = BlockStateName;
        //ブロックを増やす
        if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            AddBlock(new Vector2Int((int)cursorTransform.position.x, (int)cursorTransform.position.z));
        }
        //ブロックを減らす
        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            SubBlock(new Vector2Int((int)cursorTransform.position.x, (int)cursorTransform.position.z));
        }
        //壊れる、壊れないブロックを切り替える
        if (Input.GetKey(KeyCode.Z))
        {
            SwitchStrong(true, new Vector2Int((int)cursorTransform.position.x, (int)cursorTransform.position.z));
        }
        if (Input.GetKey(KeyCode.X))
        {
            SwitchStrong(false, new Vector2Int((int)cursorTransform.position.x, (int)cursorTransform.position.z));
        }
        //押した数字までブロックを増減する
        for (int i = 0; i < 8; ++i)
        {
            if (Input.GetKey((KeyCode)((int)KeyCode.Alpha0 + i)) ||
            Input.GetKey((KeyCode)((int)KeyCode.Keypad0 + i)))
            {
                SetBlock(i, new Vector2Int((int)cursorTransform.position.x, (int)cursorTransform.position.z));
                break;
            }
        }
        //ステートの進行
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            state = PlayerState;
        }
    }

    void PlayerState()
    {
        stateName = PlayerStateName;
        //押した数字のプレイヤーを生成する
        for (int i = 1; i < 5; ++i)
        {
            if (Input.GetKey((KeyCode)((int)KeyCode.Alpha0 + i)) ||
            Input.GetKey((KeyCode)((int)KeyCode.Keypad0 + i)))
            {
                SetPlayer(i, new Vector2Int((int)cursorTransform.position.x, (int)cursorTransform.position.z));
                break;
            }
        }
        //ステートの後退
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            foreach (var player in players)
            {
                player.SetActive(false);
            }
            state = BlockState;
        }
        //ステートの進行
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            foreach (var player in players)
            {
                if (player.activeSelf == false) return;
            }
            MakeCsv();
            Clear();
            state = BlockState;
        }
    }

    /// <summary>
    /// カーソルの移動
    /// </summary>
    void CursorMove()
    {
        Vector3 position = cursorTransform.position;
        //移動の間隔
        float moveTimeLimit = 0.3f;
        //Shiftキーを押すと速く移動する
        if (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift))
        {
            moveTimeLimit = 0.05f;
        }
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            cursorMoveTime[0] += Time.deltaTime;
            if (cursorMoveTime[0] > moveTimeLimit)
            {
                cursorMoveTime[0] = 0.0f;
                ++position.z;
            }
        }
        else
        {
            cursorMoveTime[0] = moveTimeLimit;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            cursorMoveTime[1] += Time.deltaTime;
            if (cursorMoveTime[1] > moveTimeLimit)
            {
                cursorMoveTime[1] = 0.0f;
                --position.z;
            }
        }
        else
        {
            cursorMoveTime[1] = moveTimeLimit;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            cursorMoveTime[2] += Time.deltaTime;
            if (cursorMoveTime[2] > moveTimeLimit)
            {
                cursorMoveTime[2] = 0.0f;
                ++position.x;
            }
        }
        else
        {
            cursorMoveTime[2] = moveTimeLimit;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            cursorMoveTime[3] += Time.deltaTime;
            if (cursorMoveTime[3] > moveTimeLimit)
            {
                cursorMoveTime[3] = 0.0f;
                --position.x;
            }
        }
        else
        {
            cursorMoveTime[3] = moveTimeLimit;
        }
        //右斜め上
        if (Input.GetKey(KeyCode.O))
        {
            cursorMoveTime[4] += Time.deltaTime;
            if (cursorMoveTime[4] > moveTimeLimit)
            {
                cursorMoveTime[4] = 0.0f;
                ++position.z;
                ++position.x;
            }
        }
        else
        {
            cursorMoveTime[4] = moveTimeLimit;
        }
        //左斜め上
        if (Input.GetKey(KeyCode.I))
        {
            cursorMoveTime[5] += Time.deltaTime;
            if (cursorMoveTime[5] > moveTimeLimit)
            {
                cursorMoveTime[5] = 0.0f;
                ++position.z;
                --position.x;
            }
        }
        else
        {
            cursorMoveTime[5] = moveTimeLimit;
        }
        //右斜め下
        if (Input.GetKey(KeyCode.L))
        {
            cursorMoveTime[6] += Time.deltaTime;
            if (cursorMoveTime[6] > moveTimeLimit)
            {
                cursorMoveTime[6] = 0.0f;
                --position.z;
                ++position.x;
            }
        }
        else
        {
            cursorMoveTime[6] = moveTimeLimit;
        }
        //左斜め下
        if (Input.GetKey(KeyCode.K))
        {
            cursorMoveTime[7] += Time.deltaTime;
            if (cursorMoveTime[7] > moveTimeLimit)
            {
                cursorMoveTime[7] = 0.0f;
                --position.z;
                --position.x;
            }
        }
        else
        {
            cursorMoveTime[7] = moveTimeLimit;
        }
        //位置の調整
        position.Set(Mathf.Clamp(position.x, 0, blockArray.GetLength(1) - 1),
                    blockArray[(int)cursorTransform.position.z, (int)cursorTransform.position.x] + 1,
                    Mathf.Clamp(position.z, 0, blockArray.GetLength(0) - 1));
        //位置のセット
        cursorTransform.position = position;
    }

    /// <summary>
    /// ブロックの追加
    /// </summary>
    /// <param name="position">位置</param>
    void AddBlock(Vector2Int position)
    {
        //既に最大なら何もしない
        if (blockArray[position.y, position.x] >= BlockMapSize.HeightN) return;
        //ブロックの数を増やす
        ++blockArray[position.y, position.x];
        //一番上にブロックを生成
        blockObjectArray[position.y, position.x, blockArray[position.y, position.x] - 1] =
            Instantiate(isStrongArray[position.y, position.x] ? strongCube : cubes[blockArray[position.y, position.x] - 1]);
        //親オブジェクトをセット
        blockObjectArray[position.y, position.x, blockArray[position.y, position.x] - 1].transform.parent = fieldParent.transform;
        blockObjectArray[position.y, position.x, blockArray[position.y, position.x] - 1].transform.position =
            new Vector3(position.x, blockArray[position.y, position.x], position.y);
    }

    /// <summary>
    /// ブロックの削除
    /// </summary>
    /// <param name="position">位置</param>
    void SubBlock(Vector2Int position)
    {
        //既に0なら何もしない
        if (blockArray[position.y, position.x] <= 0) return;
        //一番上のブロックを削除
        Destroy(blockObjectArray[position.y, position.x, blockArray[position.y, position.x] - 1]);
        //ブロックの数を減らす
        --blockArray[position.y, position.x];
        if (blockArray[position.y, position.x] == 0)
        {
            isStrongArray[position.y, position.x] = false;
        }
    }

    /// <summary>
    /// ブロックを指定数にする
    /// </summary>
    /// <param name="blockNum">数</param>
    /// <param name="position">位置</param>
    void SetBlock(int blockNum, Vector2Int position)
    {
        int blockNumber = blockArray[position.y, position.x];
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

    /// <summary>
    /// 壊れる、壊れないブロックの切り替え
    /// </summary>
    /// <param name="isToStrong">壊れないブロックにするかどうか</param>
    /// <param name="position">位置</param>
    void SwitchStrong(bool isToStrong, Vector2Int position)
    {
        if (isStrongArray[position.y, position.x] == isToStrong) return;
        int blockNumber = blockArray[position.y, position.x];
        if (blockNumber == 0) return;
        for (int i = 0; i < blockNumber; ++i)
            SubBlock(position);
        //壊れるか壊れないかを切り替える
        isStrongArray[position.y, position.x] = isToStrong;
        for (int i = 0; i < blockNumber; ++i)
            AddBlock(position);
    }

    /// <summary>
    /// プレイヤーのセット
    /// </summary>
    /// <param name="number">プレイヤーの番号</param>
    /// <param name="position">位置</param>
    void SetPlayer(int number, Vector2Int position)
    {
        //プレイヤー同士の距離の制限
        const int DistanceLimit = 4;
        if (blockArray[position.y, position.x] == 0) return;
        for (int i = 0; i < playerPositions.Length; ++i)
        {
            //距離範囲外かどうか
            if (i != (number - 1) && players[i].activeSelf &&
            Mathf.Abs(playerPositions[i].x - position.x) < DistanceLimit &&
            Mathf.Abs(playerPositions[i].y - position.y) < DistanceLimit)
            {
                return;
            }
        }
        players[number - 1].SetActive(true);
        players[number - 1].transform.position = new Vector3(position.x, blockArray[position.y, position.x] + 3, position.y);
        playerPositions[number - 1] = position;
        players[number - 1].transform.LookAt(
            new Vector3(BlockMapSize.RowN / 2.0f, players[number - 1].transform.position.y, BlockMapSize.LineN / 2.0f));
    }

    /// <summary>
    /// カメラの制御
    /// </summary>
    void CameraControl()
    {
        //追尾モード
        if (Input.GetKeyDown(KeyCode.T))
        {
            isTracking = true;
            cameraObject.transform.eulerAngles = new Vector3(90, 0, 0);
            trackingDistance = 60;
        }
        //俯瞰モード
        if (Input.GetKeyDown(KeyCode.C))
        {
            isTracking = false;
            cameraObject.transform.position = new Vector3(25.5f, 60, 25.5f);
            cameraObject.transform.eulerAngles = new Vector3(90, 0, 0);
        }
        //デフォルトモード
        if (Input.GetKeyDown(KeyCode.V))
        {
            isTracking = false;
            cameraObject.transform.position = cameraInitPosition;
            cameraObject.transform.rotation = cameraInitRotation;
        }
        if (isTracking)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0) trackingDistance -= scrollSpeed;
            if (scroll < 0) trackingDistance += scrollSpeed;
            trackingDistance = Mathf.Clamp(trackingDistance, 10, 60);
            Vector3 position = cursorTransform.position;
            position.y = trackingDistance;
            cameraObject.transform.position = position;
        }
    }

    /// <summary>
    /// csvの作成
    /// </summary>
    void MakeCsv()
    {
        //生成したcsvのパス
        const string Path = "Assets/Resources/csv/MakeMap";
        //フォルダの作成
        Directory.CreateDirectory(Path);
        string csvString = "";
        for (int i = 0; i < BlockMapSize.LineN; ++i)
        {
            bool first = true;
            for (int j = 0; j < BlockMapSize.RowN; ++j)
            {
                //最初以外','をつける
                if (!first)
                {
                    csvString += ",";
                }
                first = false;
                //ブロックの数を取得
                int blockNumber = blockArray[i, j];
                if (blockNumber != 0)
                {
                    //壊れないブロックなら+10
                    blockNumber += isStrongArray[i, j] ? 10 : 0;
                    //プレイヤーの位置かチェックする
                    for (int k = 0; k < playerPositions.Length; ++k)
                    {
                        //プレイヤーの位置ならそのプレイヤー*100加算する
                        if (playerPositions[k] == new Vector2Int(j, i))
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

    /// <summary>
    /// 中間データの作成
    /// </summary>
    void MakeMiddle()
    {
        const string Path = "Assets/Resources/csv/MakeMap/Middle";
        //フォルダの作成
        Directory.CreateDirectory(Path);
        string csvString = "";
        for (int i = 0; i < BlockMapSize.LineN; ++i)
        {
            bool first = true;
            for (int j = 0; j < BlockMapSize.RowN; ++j)
            {
                //最初以外','をつける
                if (!first)
                {
                    csvString += ",";
                }
                first = false;
                //ブロックの数を取得
                int blockNumber = blockArray[i, j];
                if (blockNumber != 0)
                {
                    //壊れないブロックなら+10
                    blockNumber += isStrongArray[i, j] ? 10 : 0;
                }
                //stringに追加
                csvString += blockNumber.ToString();
            }
            csvString += "\n";
        }
        int number = Directory.GetFiles(Path, "*").Length / 2;
        StreamWriter sw = new StreamWriter(Path + "/MiddleMapData" + number.ToString() + ".csv", false);
        sw.Write(csvString);
        sw.Flush();
        sw.Close();
    }

    /// <summary>
    /// CSVの読み込み
    /// </summary>
    /// <param name="textAsset">csv</param>
    void LoadCsv(TextAsset textAsset)
    {
        //改行ごとに格納
        string[] lineString = textAsset.text.Split('\n');
        Vector2Int position = new Vector2Int();
        for (int z = 0; z < BlockMapSize.LineN; z++)
        {
            //カンマごとに格納
            string[] rowString = lineString[z].Split(',');
            for (int x = 0; x < BlockMapSize.LineN; x++)
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
                    SetBlock(number % 10, position);
                    SwitchStrong(number >= 10, position);
                }
            }
        }
    }

    /// <summary>
    /// マップ情報をクリア
    /// </summary>
    void Clear()
    {
        cursorTransform.position = Vector3.zero;
        blockArray = new int[BlockMapSize.LineN, BlockMapSize.RowN];
        isStrongArray = new bool[BlockMapSize.LineN, BlockMapSize.RowN];
        foreach (var block in blockObjectArray)
        {
            Destroy(block);
        }
        foreach (var player in players)
        {
            player.SetActive(false);
        }
    }

    void OnGUI()
    {
        if (displayUI)
        {
            if (stateName == BlockStateName)
            {
                GUI.Label(new Rect(300, 20, 1000, 1000), stateName +
                "\n+ : ブロックを一段増やす\n- : ブロックを一段減らす\nWASD,矢印 : 移動(Shiftを押すと高速移動)\n" +
                "IOKL : 斜め移動(Shiftを押すと高速移動)\nZ : 壊れないブロックに変換\nX : 壊れるブロックに変換\n" +
                "0~7 : 押した数の段数にする\nEnter : プレイヤーをセットするステートに移行\n" +
                "C : 上からマップを見る\nV : 初期位置からマップを見る\nT : カメラが追尾する(マウスホイールで拡大縮小)");
            }
            else if (stateName == PlayerStateName)
            {
                GUI.Label(new Rect(300, 20, 1000, 1000), stateName +
                "\nWASD,矢印 : 移動(Shiftを押すと高速移動)\nIOKL : 斜め移動(Shiftを押すと高速移動)\n" +
                "1~4 : 押した数のプレイヤーをセットする\n(近すぎるとセットできません)\n" +
                "Enter : CSVを生成し、マップをクリア\nBackSpace : ブロックをセットするステートに戻る\n" +
                "C : 上からマップを見る\nV : 初期位置からマップを見る\nT : カメラが追尾する(マウスホイールで拡大縮小)");
            }
        }
        GUI.Label(new Rect(20, 350, 1000, 1000),
         "縦：" + (cursorTransform.position.z + 1).ToString("##") + "\n" +
         "横：" + (cursorTransform.position.x + 1).ToString("##"));
        GUI.Label(new Rect(20, 400, 1000, 1000), "M : 説明の表示・非表示");
    }
#endif
}
