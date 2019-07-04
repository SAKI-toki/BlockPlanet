using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public struct ResultPlayerInfo
{
    public Vector3 position;
    public Vector3 eulerAngle;
}

public class AnimationRecorderSceneController : MonoBehaviour
{
    [SerializeField]
    int RecordPlayerNumber = 0;
    bool IsRecord = false;
    List<ResultPlayerInfo> playerInfos = new List<ResultPlayerInfo>();
    ResultPlayerInfo playerInfo = new ResultPlayerInfo();
    Rigidbody targetRigidbody;
    Transform targetTransform;
    [SerializeField]
    GameObject lookatObject;

    void Start()
    {
        ResultBlockMeshCombine blockMap = new ResultBlockMeshCombine();
        GameObject parent = new GameObject("FieldObject");
        //マップ生成
        BlockCreater.GetInstance().CreateField("Result", parent.transform, blockMap, lookatObject, BlockCreater.SceneEnum.Result);
        parent.isStatic = true;
        blockMap.BlockIsSurroundUpdate();
        blockMap.BlockRendererOff();
        blockMap.Initialize();
        //プレイヤーを地面につけるために時間を進める
        Physics.autoSimulation = false;
        Physics.Simulate(10.0f);
        Physics.autoSimulation = true;
    }

    /// <summary>
    /// 説明の表示
    /// </summary>
    void OnGUI()
    {
        GUI.color = Color.white;
        GUI.Label(new Rect(20, 40, 1000, 1000), "Enter:Record Start and End\nSpace:Scene Reload");
        GUI.Label(new Rect(20, 80, 1000, 1000), "SelectPlayerNumber:" + RecordPlayerNumber);
        if (IsRecord) GUI.color = Color.red;
        GUI.Label(new Rect(20, 100, 1000, 1000), "REC");
    }

    void Update()
    {
        //シーンのリロード
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }
        if (!IsRecord)
        {
            //レコード開始
            if (Input.GetKeyDown(KeyCode.Return))
            {
                IsRecord = true;
                GameObject player = GameObject.FindGameObjectWithTag("Player" + (RecordPlayerNumber + 1));
                targetTransform = player.transform;
                targetRigidbody = player.GetComponent<Rigidbody>();
                Debug.Log("RecordStart!");
            }
            //書き出し
            else if (playerInfos.Count != 0)
            {
                WriteText();
                playerInfos.Clear();
            }
            return;
        }
        //レコード終了
        if (Input.GetKeyDown(KeyCode.Return))
        {
            IsRecord = false;
            Debug.Log("RecordEnd!\nRecordFrameNum:" + playerInfos.Count);
            return;
        }
        RaycastHit hit;

        if (Physics.SphereCast(targetTransform.position, 1.5f, Vector3.down, out hit, 1.7f) &&
        SwitchInput.GetButtonDown(0, SwitchButton.Jump))
        {
            if (hit.collider.tag == "Cube" || hit.collider.tag == "StrongCube")
            {
                //力を加える
                targetRigidbody.AddForce(Vector3.up * 1000f);
                //かかっている力をリセット
                targetRigidbody.velocity = Vector3.zero;
                SoundManager.Instance.Jump();
            }
        }
    }

    void FixedUpdate()
    {
        if (!IsRecord) return;
        float vertical = SwitchInput.GetVertical(0);
        float horizontal = SwitchInput.GetHorizontal(0);
        if (Mathf.Abs(vertical) >= 0.4f ||
        Mathf.Abs(horizontal) >= 0.4f)
        {
            float sqrt = Mathf.Sqrt(Mathf.Pow(vertical, 2) + Mathf.Pow(horizontal, 2));
            //=====移動=====
            targetTransform.Translate(Vector3.forward * 15 * Time.deltaTime * sqrt);
            //=====回転=====
            targetTransform.rotation = Quaternion.Slerp
                (targetTransform.rotation,
                 Quaternion.Euler(0, Mathf.Atan2(-vertical, horizontal) * Mathf.Rad2Deg + 90, 0),
                 Mathf.Sqrt(sqrt) / 2);
        }

        //リストへプレイヤーの情報の追加
        playerInfo.position = targetTransform.position;
        playerInfo.eulerAngle = targetTransform.eulerAngles;
        playerInfos.Add(playerInfo);
    }

    void WriteText()
    {
        StreamWriter sw = new StreamWriter("Assets/Resources/ResultPlayer/Record" + RecordPlayerNumber + ".txt", false);
        //プレイヤーの情報を書き込む
        foreach (var playerInfo in playerInfos)
        {
            sw.Write(string.Format("{0:f4}", playerInfo.position.x) + ",");
            sw.Write(string.Format("{0:f4}", playerInfo.position.y) + ",");
            sw.Write(string.Format("{0:f4}", playerInfo.position.z) + ",");
            sw.Write(string.Format("{0:f4}", playerInfo.eulerAngle.x) + ",");
            sw.Write(string.Format("{0:f4}", playerInfo.eulerAngle.y) + ",");
            sw.Write(string.Format("{0:f4}", playerInfo.eulerAngle.z) + "\n");
        }
        sw.Flush();
        sw.Close();
    }
}
