using UnityEngine;
using System.IO;

/// <summary>
/// ブロックを生成する
/// </summary>
public class BlockCreater : Singleton<BlockCreater>
{
    [SerializeField]
    GameObject[] Players = new GameObject[4];
    //ブロック
    [SerializeField]
    GameObject[] StageCubes = new GameObject[7];
    [SerializeField]
    GameObject StrongCube = null;

    //CSVデータの行数
    public const int line_n = 52;
    //CSVデータの列数
    public const int row_n = 52;

    //設置位置
    Vector3 position = new Vector3();

    public enum SceneEnum { Game, Result, Other };
    [SerializeField]
    public Material[] mats = new Material[8];

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
        //文字検索用
        int[] iDat = new int[4];
        //CSVの全文字列を保存する
        string str = "";
        //取り出した文字列を保存する
        string strget = "";
        TextAsset csvfile = Resources.Load("csv/" + CsvName) as TextAsset;
        //文字列読み取る
        StringReader reader = new StringReader(csvfile.text);

        //戻り値:使用できる文字がないか、ストリームがシークをサポートしていない場合は -1
        while (reader.Peek() > -1)
        {
            string line = reader.ReadLine();
            //真ん中の点が無いと12とか21が出るよ
            str = str + "," + line;
        }

        //最後に検索文字列の","を追記。最後を取りこぼす
        str = str + ",";


        for (int z = 0; z < line_n; z++)
        {
            for (int x = 0; x < row_n; x++)
            {
                //IndexOfメソッドは文字列内に含まれる文字、文字列の位置を取得することができる。
                iDat[0] = str.IndexOf(",", iDat[0]);
                //次の","を検索
                iDat[1] = str.IndexOf(",", iDat[0] + 1);
                //何文字取り出すか決定
                iDat[2] = iDat[1] - iDat[0] - 1;
                //iDat[2]文字ぶんだけ取り出す
                strget = str.Substring(iDat[0] + 1, iDat[2]);
                //文字列を数値型に変換  
                iDat[3] = int.Parse(strget);
                //次のインデックスへ
                iDat[0]++;

                position.Set(x, 0, z);
                //プレイヤーの位置
                if (iDat[3] >= 100)
                {
                    int playerNumber = iDat[3] / 100;
                    //プレイヤーを出現する
                    if (currentScene == SceneEnum.Game ||
                        currentScene == SceneEnum.Result)
                    {
                        position.y = 20;
                        GeneratePlayer(playerNumber - 1, currentScene, LookatObject);
                    }
                    iDat[3] -= playerNumber * 100;
                }
                GenerateBlock(iDat[3], x, z, parent, blockMap);
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
        GameObject cube;
        //壊れるブロック
        if (number < 10)
        {
            for (int i = 0; i < number; ++i)
            {
                position.y = i;
                cube = Instantiate(StageCubes[i], position, Quaternion.identity);
                if (parent != null) cube.transform.parent = parent;
                if (blockMap != null) blockMap.SetBlock(z, x, i, cube);
            }
        }
        //壊れないブロック
        else
        {
            for (int i = 0; i < number - 10; ++i)
            {
                position.y = i;
                cube = Instantiate(StrongCube, position, Quaternion.identity);
                if (parent != null) cube.transform.parent = parent;
                if (blockMap != null) blockMap.SetBlock(z, x, i, cube);
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
        for (int i = 0; i < 8; ++i)
        {
            if (mats[i] == mat) return i;
        }
        Debug.LogError("見つかりませんでした");
        return -1;
    }
}