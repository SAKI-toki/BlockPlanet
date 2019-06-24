using nn.hid;

/// <summary>
/// スイッチ関係を管理する
/// </summary>
public class SwitchManager : Singleton<SwitchManager>
{
    //使用するID
    NpadId[] m_NpadIds = { NpadId.No1, NpadId.No2, NpadId.No3, NpadId.No4 };

    //使用するコントローラーのスタイル
    NpadStyle m_NpadStyles = NpadStyle.JoyLeft | NpadStyle.JoyRight;

    //接続されているかどうか
    static bool[] m_IsConnect;

    public override void MyStart()
    {
        //コントローラーの初期化
        Npad.Initialize();
        //サポートするタイプをセット
        Npad.SetSupportedIdType(m_NpadIds);
        //サポートするスタイルをセット
        Npad.SetSupportedStyleSet(m_NpadStyles);
        //配列の要素確保
        m_IsConnect = new bool[m_NpadIds.Length];
        //入力の初期化
        SwitchInput.InputInit(m_NpadIds.Length);
    }

    public override void MyUpdate()
    {
        for (int i = 0; i < m_NpadIds.Length; ++i)
        {
            //接続状態の更新
            ConnectUpdate(i);
            //入力情報の更新
            SwitchInput.InputUpdate(i, m_NpadIds[i]);
        }
    }

    /// <summary>
    /// 接続状態の更新
    /// </summary>
    /// <param name="_Index">コントローラーの番号</param>
    /// <returns></returns>
    void ConnectUpdate(int _Index)
    {
        //スタイルがNoneならfalse
        m_IsConnect[_Index] = (Npad.GetStyleSet(m_NpadIds[_Index]) != NpadStyle.None);
    }

    /// <summary>
    /// 接続されているか
    /// </summary>
    /// <param name="_Index">コントローラーの番号</param>
    /// <returns>接続されていたらtrue</returns>
    public bool IsConnect(int _Index)
    {
#if UNITY_EDITOR
        return UnityEngine.Input.GetJoystickNames().Length >= _Index + 1;
#else
        return m_IsConnect[_Index];
#endif
    }

    /// <summary>
    /// NpadIdのゲッタ
    /// </summary>
    /// <param name="_Index">コントローラーの番号</param>
    /// <returns>NpadId</returns>
    public NpadId GetNpadId(int _Index)
    {
        return m_NpadIds[_Index];
    }

    /// <summary>
    /// NpadStyleのゲッタ
    /// </summary>
    /// <returns>NpadStyle</returns>
    public NpadStyle GetNpadStyle()
    {
        return m_NpadStyles;
    }
}