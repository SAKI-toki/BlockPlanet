using UnityEngine;

/// <summary>
/// ブロックを生成する
/// </summary>
public class BlockCreater : Singleton<BlockCreater>
{
    [SerializeField]
    GameObject[] Players = new GameObject[4];
    [SerializeField]
    GameObject[] CubeLists = new GameObject[7];
    [SerializeField]
    GameObject[] StrongCubeLists = new GameObject[7];

    //CSVデータの行数
    public const int line_n = 52;
    //CSVデータの列数
    public const int row_n = 52;

    //設置位置
    Vector3 position = new Vector3();

    public enum SceneEnum { Game, Result, Other };
    [SerializeField]
    public Material[] mats = new Material[8];

    GameObject cubeList;

    /// <summary>
    /// フィールドの生成
    /// </summary>
    /// <param name="CsvName">読み込むCSVの名前</param>
    /// <param name="parent">生成したブロックの親オブジェクト</param>
    /// <param name="blockMap">ブロックマップ</param>
    /// <param name="LookatObject">注視点</param>
    /// <param name="currentScene">現在のシーン</param>
    public void CreateField(string CsvName, Transform parent, BlockMap blockMap,
    GameObject LookatObject, SceneEnum currentScene = SceneEnum.Other)
    {
        //csvの読み込み
        TextAsset csvfile = Resources.Load("csv/" + CsvName) as TextAsset;
        //改行ごとに格納
        string[] lineString = csvfile.text.Split('\n');

        for (int z = 0; z < line_n; z++)
        {
            //カンマごとに格納
            string[] rowString = lineString[z].Split(',');
            for (int x = 0; x < row_n; x++)
            {
                //string型をint型にパース
                int number = int.Parse(rowString[x]);

                position.Set(x, 0, z);
                //プレイヤーの位置
                if (number >= 100)
                {
                    int playerNumber = number / 100;
                    //プレイヤーを出現する
                    if (currentScene == SceneEnum.Game ||
                        currentScene == SceneEnum.Result)
                    {
                        position.y = 20;
                        GeneratePlayer(playerNumber - 1, currentScene, LookatObject);
                    }
                    number -= playerNumber * 100;
                }
                position.y = 0;
                if (number != 0) GenerateBlock(number, x, z, parent, blockMap);
            }
        }
    }

    /// <summary>
    /// プレイヤーの生成
    /// </summary>
    /// <param name="playerNumber">プレイヤーの番号</param>
    void GeneratePlayer(int playerNumber, SceneEnum scene, GameObject lookatObject)
    {
        GameObject player = Instantiate(Players[playerNumber], position, Quaternion.identity);
        //ゲームならマップの中心を向かせる
        if (scene == SceneEnum.Game)
        {
            player.transform.LookAt(new Vector3(row_n / 2.0f, player.transform.position.y, line_n / 2.0f));
        }
        //リザルトならカメラに向かせる
        else if (scene == SceneEnum.Result)
        {
            Vector3 lookatPosition = lookatObject.transform.position;
            lookatPosition.y = 0;
            player.transform.LookAt(lookatPosition);
        }
    }

    /// <summary>
    /// ブロックの生成
    /// </summary>
    /// <param name="number">取得した番号</param>
    /// <param name="x">x座標</param>
    /// <param name="z">z座標</param>
    /// <param name="parent">親オブジェクト</param>
    /// <param name="blockMap">ブロックマップ</param>
    void GenerateBlock(int number, int x, int z, Transform parent, BlockMap blockMap)
    {
        //壊れるブロック
        if (number < 10)
        {
            cubeList = Instantiate(CubeLists[number - 1], position, Quaternion.identity);
            if (parent != null) cubeList.transform.parent = parent;
            if (blockMap != null)
            {
                for (int i = 0; i < cubeList.transform.childCount; ++i)
                {
                    blockMap.SetBlock(z, x, i, cubeList.transform.GetChild(i).gameObject);
                }
            }
        }
        //壊れないブロック
        else
        {
            cubeList = Instantiate(StrongCubeLists[number - 11], position, Quaternion.identity);
            if (parent != null) cubeList.transform.parent = parent;
            if (blockMap != null)
            {
                for (int i = 0; i < cubeList.transform.childCount; ++i)
                {
                    blockMap.SetBlock(z, x, i, cubeList.transform.GetChild(i).gameObject);
                }
            }
        }
    }

    /// <summary>
    /// 高速化のため、マテリアルを番号で管理する
    /// </summary>
    /// <param name="mat">番号を取得するマテリアル</param>
    /// <returns>マテリアルの番号</returns>
    public int GetMaterialNumber(Material mat)
    {
        for (int i = 0; i < mats.Length; ++i)
        {
            if (mats[i] == mat) return i;
        }
        Debug.LogError("見つかりませんでした");
        return -1;
    }

    public void AutoGenerate(int[,] blockArray, Transform parent, BlockMap blockMap)
    {
        for (int i = 0; i < line_n; i++)
        {
            for (int j = 0; j < row_n; j++)
            {
                int number = blockArray[i, j];
                position.Set(j, 0, i);
                //プレイヤーの位置
                if (number >= 100)
                {
                    int playerNumber = number / 100;
                    position.y = 20;
                    GeneratePlayer(playerNumber - 1, SceneEnum.Game, null);
                    number -= playerNumber * 100;
                }
                position.y = 0;
                if (number != 0) GenerateBlock(number, j, i, parent, blockMap);
            }
        }
    }
}