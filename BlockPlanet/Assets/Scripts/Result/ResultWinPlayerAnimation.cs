using UnityEngine;
using System.Collections;

/// <summary>
/// 勝ったプレイヤーのアニメーション
/// </summary>
public class ResultWinPlayerAnimation : MonoBehaviour
{
    public IEnumerator WinPlayerAnimation(int winNumber)
    {
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        //ファイルの読み込み
        TextAsset recordFile = Resources.Load("ResultPlayer/Record" + winNumber) as TextAsset;
        string[] recordText = recordFile.text.Split('\n');
        Vector3 position = new Vector3();
        Vector3 eulerAngle = new Vector3();
        //最後の行は空白のため考慮しない
        for (int i = 0; i < recordText.Length - 1; ++i)
        {
            string[] infos = recordText[i].Split(',');
            position.Set(float.Parse(infos[0]), float.Parse(infos[1]), float.Parse(infos[2]));
            eulerAngle.Set(float.Parse(infos[3]), float.Parse(infos[4]), float.Parse(infos[5]));
            transform.position = position;
            transform.eulerAngles = eulerAngle;
            yield return null;
        }
    }
}