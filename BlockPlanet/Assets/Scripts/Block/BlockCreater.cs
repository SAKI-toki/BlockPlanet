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
    GameObject StrongCube;

    //CSVデータの行数
    public const int line_n = 52;
    //CSVデータの列数
    public const int row_n = 52;

    //設置位置
    Vector3 position;

    public enum SceneEnum { Game, Other };

    /// <summary>
    /// フィールドの生成
    /// </summary>
    /// <param name="CsvName">読み込むCSVの名前</param>
    /// <param name="parent">生成したブロックの親オブジェクト</param>
    /// <param name="blockMap">ブロックマップ</param>
    /// <param name="currentScene">現在のシーン</param>   
    public void CreateField(string CsvName, Transform parent, BlockMap blockMap, SceneEnum currentScene = SceneEnum.Other)
    {
        //文字検索用
        int[] iDat = new int[4];
        GameObject cube = null;
        //CSVの全文字列を保存する
        string str = "";
        //取り出した文字列を保存する
        string strget = "";
        int Xcubepos = 0;
        int Zcubepos = 0;
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
            //ループするたびにxを0に。この位置からずらさないように
            Xcubepos = 0;
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

                position.Set(Xcubepos, 0, Zcubepos);
                //プレイヤーの位置
                if (iDat[3] >= 100)
                {
                    int playerNumber = iDat[3] / 100;
                    //プレイヤーを出現する
                    if (currentScene == SceneEnum.Game)
                    {
                        position.y = 20;
                        GeneratePlayer(playerNumber - 1, position);
                    }
                    iDat[3] -= playerNumber * 100;
                }
                //壊れるブロック
                if (iDat[3] < 10)
                {
                    for (int i = 1; i <= iDat[3]; ++i)
                    {
                        position.y = i - 1;
                        cube = Instantiate(StageCubes[i - 1], position, Quaternion.identity);
                        cube.transform.parent = parent;
                        blockMap.SetBlock(Zcubepos, Xcubepos, i - 1, cube);
                    }
                }
                //壊れないブロック
                else
                {
                    position.y = iDat[3] - 11;
                    cube = Instantiate(StrongCube, position, Quaternion.identity);
                    cube.transform.parent = parent;
                    blockMap.SetBlock(Zcubepos, Xcubepos, iDat[3] - 11, cube);
                }
                //配置位置を(カメラから見て)右に移動
                Xcubepos++;
            }
            //配置位置を(カメラから見て)縦に移動
            Zcubepos++;
        }
    }

    /// <summary>
    /// プレイヤーの生成
    /// </summary>
    /// <param name="playerNumber">プレイヤーの番号</param>
    /// <param name="position">生成位置</param>
    void GeneratePlayer(int playerNumber, Vector3 position)
    {
        GameObject player = Instantiate(Players[playerNumber], position, Quaternion.identity);
        //マップの中心を向かせる
        player.transform.LookAt(new Vector3(row_n / 2.0f, position.y, line_n / 2.0f));
    }
}