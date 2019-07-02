using UnityEngine;
using System.IO;

public class TitleCreation : MonoBehaviour
{

    /// <summary>
    /// タイトルのブロック生成
    /// </summary>

    [SerializeField]
    GameObject TitleCube;
    [SerializeField]
    GameObject TitleStrongCube;

    private TextAsset csvfile;

    //CSVの全文字列を保存する
    string Str = "";
    //取り出した文字列を保存する
    string Strget = "";

    //CSVデータの行数
    int Line = 52;
    //CSVデータの列数
    int Row = 100;

    //文字検索用
    int[] iDat = new int[15];

    int Xcubepos = 0;
    int Zcubepos = 0;

    //設置位置
    Vector3 Position;

    // Use this for initialization
    void Start()
    {
        GameObject obj = new GameObject("TitleField");
        Creation(obj);
    }

    void Creation(GameObject parent)
    {
        csvfile = Resources.Load("csv/title") as TextAsset;
        //文字列読み取る
        StringReader reader = new StringReader(csvfile.text);
        GameObject obj;

        //戻り値:使用できる文字がないか、ストリームがシークをサポートしていない場合は -1
        while (reader.Peek() > -1)
        {
            string line = reader.ReadLine();
            //真ん中の点が無いと12とか21が出るよ
            Str = Str + "," + line;
        }

        //最後に検索文字列の","を追記。最後を取りこぼす
        Str = Str + ",";

        for (int z = 0; z < Line; z++)
        {
            //ループするたびにxを0に。この位置からずらさないように
            Xcubepos = 0;
            for (int x = 0; x < Row; x++)
            {
                //IndexOfメソッドは文字列内に含まれる文字、文字列の位置を取得することができる。
                iDat[0] = Str.IndexOf(",", iDat[0]);
                //次の","を検索
                iDat[1] = Str.IndexOf(",", iDat[0] + 1);
                //何文字取り出すか決定
                iDat[2] = iDat[1] - iDat[0] - 1;
                //iDat[2]文字ぶんだけ取り出す
                Strget = Str.Substring(iDat[0] + 1, iDat[2]);
                //文字列を数値型に変換  
                iDat[3] = int.Parse(Strget);
                //次のインデックスへ
                iDat[0]++;

                //透明
                if (iDat[3] == 0)
                {
                    Position = new Vector3(Xcubepos, 0, Zcubepos);
                    obj = Instantiate(TitleCube, Position, Quaternion.Euler(0, 0, 0));
                    obj.transform.parent = parent.transform;
                }
                //一段
                if (iDat[3] == 1)
                {
                    Position = new Vector3(Xcubepos, 0, Zcubepos);
                    obj = Instantiate(TitleStrongCube, Position, Quaternion.Euler(0, 0, 0));
                    obj.transform.parent = parent.transform;
                }

                //配置位置を(カメラから見て)右に移動
                Xcubepos++;
            }
            //配置位置を(カメラから見て)縦に移動
            Zcubepos++;
        }
    }
}
