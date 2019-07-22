using UnityEngine;

/// <summary>
/// ブロックを生成する
/// </summary>
public class BlockCreater : Singleton<BlockCreater>
{
    [SerializeField]
    GameObject[] players = new GameObject[4];
    [SerializeField]
    GameObject[] cubeLists = new GameObject[BlockMapSize.HeightN];
    [SerializeField]
    GameObject[] strongCubeLists = new GameObject[BlockMapSize.HeightN];

    //設置位置
    Vector3 position = new Vector3();

    public enum SceneEnum { Game, Result, Other };
    [SerializeField]
    public Material[] mats = new Material[8];

    [System.NonSerialized]
    public int maxPlayerNumber = 4;

    GameObject cubeList;

    /// <summary>
    /// フィールドの生成
    /// </summary>
    /// <param name="csvName">読み込むCSVの名前</param>
    /// <param name="parent">生成したブロックの親オブジェクト</param>
    /// <param name="blockMap">ブロックマップ</param>
    /// <param name="lookatObject">注視点</param>
    /// <param name="currentScene">現在のシーン</param>
    public void CreateField(string csvName, Transform parent, BlockMap blockMap,
    GameObject lookatObject, SceneEnum currentScene = SceneEnum.Other)
    {
        //csvの読み込み
        TextAsset csvfile = Resources.Load("csv/" + csvName) as TextAsset;
        //改行ごとに格納
        string[] lineString = csvfile.text.Split('\n');

        for (int z = 0; z < BlockMapSize.LineN; z++)
        {
            //カンマごとに格納
            string[] rowString = lineString[z].Split(',');
            for (int x = 0; x < BlockMapSize.LineN; x++)
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
                        GeneratePlayer(playerNumber - 1, currentScene, lookatObject);
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
    /// <param name="scene">シーン</param>
    /// <param name="lookatObject">注視オブジェクト</param>
    void GeneratePlayer(int playerNumber, SceneEnum scene, GameObject lookatObject)
    {
        if (maxPlayerNumber - 1 < playerNumber)
        {
            return;
        }
        GameObject player = Instantiate(players[playerNumber], position, Quaternion.identity);
        //ゲームならマップの中心を向かせる
        if (scene == SceneEnum.Game)
        {
            player.transform.LookAt(new Vector3(BlockMapSize.RowN / 2.0f, player.transform.position.y, BlockMapSize.LineN / 2.0f));
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
            cubeList = Instantiate(cubeLists[number - 1], position, Quaternion.identity);
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
            cubeList = Instantiate(strongCubeLists[number - 11], position, Quaternion.identity);
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
        for (int i = 0; i < BlockMapSize.LineN; i++)
        {
            for (int j = 0; j < BlockMapSize.RowN; j++)
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