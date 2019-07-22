using nn.hid;

/// <summary>
/// スイッチ関係を管理する
/// </summary>
public class SwitchManager : Singleton<SwitchManager>
{
    //使用するID
    NpadId[] npadIds = { NpadId.No1, NpadId.No2, NpadId.No3, NpadId.No4 };

    //使用するコントローラーのスタイル
    NpadStyle npadStyles = NpadStyle.JoyLeft | NpadStyle.JoyRight;

    //接続されているかどうか
    static bool[] isConnect;

    public override void MyStart()
    {
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
    }

    public override void MyUpdate()
    {
        for (int i = 0; i < npadIds.Length; ++i)
        {
            //接続状態の更新
            ConnectUpdate(i);
            //入力情報の更新
            SwitchInput.InputUpdate(i, npadIds[i]);
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
        isConnect[index] = (Npad.GetStyleSet(npadIds[index]) != NpadStyle.None);
    }

    /// <summary>
    /// 接続されているか
    /// </summary>
    /// <param name="index">コントローラーの番号</param>
    /// <returns>接続されていたらtrue</returns>
    public bool IsConnect(int index)
    {
#if UNITY_EDITOR
        return UnityEngine.Input.GetJoystickNames().Length >= index + 1 || index == 0;
#else
        return isConnect[index];
#endif
    }

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
}