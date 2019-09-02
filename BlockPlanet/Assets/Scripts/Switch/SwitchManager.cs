#if UNITY_SWITCH
using nn.hid;
#endif

/// <summary>
/// スイッチ関係を管理する
/// </summary>
public class SwitchManager : Singleton<SwitchManager>
{
#if UNITY_SWITCH
    //使用するID
    NpadId[] npadIds = { NpadId.No1, NpadId.No2, NpadId.No3, NpadId.No4 };

    //使用するコントローラーのスタイル
    NpadStyle npadStyles = NpadStyle.JoyLeft | NpadStyle.JoyRight;
#endif
    //接続されているかどうか
    static bool[] isConnect;

    public override void MyStart()
    {
#if UNITY_SWITCH
        //コントローラーの初期化
        Npad.Initialize();
        //サポートするタイプをセット
        Npad.SetSupportedIdType(npadIds);
        //サポートするスタイルをセット
        Npad.SetSupportedStyleSet(npadStyles);
        //配列の要素確保
        isConnect = new bool[npadIds.Length];
        //入力の初期化
        SwitchInput.InputInit(npadIds.Length);
#endif
        //配列の要素確保
        isConnect = new bool[4];
        //入力の初期化
        SwitchInput.InputInit(4);
    }

    public override void MyUpdate()
    {
        for (int i = 0; i <
#if UNITY_SWITCH
        npadIds.Length
#else
        4
#endif
        ; ++i)
        {
            //接続状態の更新
            ConnectUpdate(i);
            //入力情報の更新
            SwitchInput.InputUpdate(i
#if UNITY_SWITCH
            , npadIds[i]
#endif
            );
        }
    }

    /// <summary>
    /// 接続状態の更新
    /// </summary>
    /// <param name="index">コントローラーの番号</param>
    /// <returns></returns>
    void ConnectUpdate(int index)
    {
        //スタイルがNoneならfalse
#if UNITY_SWITCH
        isConnect[index] = (Npad.GetStyleSet(npadIds[index]) != NpadStyle.None);
#else
        isConnect[index] = UnityEngine.Input.GetJoystickNames().Length >= index + 1;
#endif
    }

    /// <summary>
    /// 接続されているか
    /// </summary>
    /// <param name="index">コントローラーの番号</param>
    /// <returns>接続されていたらtrue</returns>
    public bool IsConnect(int index)
    {
        return isConnect[index];
    }

#if UNITY_SWITCH
    /// <summary>
    /// NpadIdのゲッタ
    /// </summary>
    /// <param name="index">コントローラーの番号</param>
    /// <returns>NpadId</returns>
    public NpadId GetNpadId(int index)
    {
        return npadIds[index];
    }

    /// <summary>
    /// NpadStyleのゲッタ
    /// </summary>
    /// <returns>NpadStyle</returns>
    public NpadStyle GetNpadStyle()
    {
        return npadStyles;
    }
#endif
}