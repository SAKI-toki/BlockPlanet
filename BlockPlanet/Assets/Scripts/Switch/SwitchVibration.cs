using nn.hid;

/// <summary>
/// スイッチの振動
/// </summary>
static public class SwitchVibration
{
    //デバイスのハンドラ
    static VibrationDeviceHandle[] m_VibrationDeviceHandles = new VibrationDeviceHandle[1];
    //振動の値
    static VibrationValue m_VibrationValue = VibrationValue.Make();

    /// <summary>
    /// 低周波の振動
    /// </summary>
    /// <param name="_Index">コントローラーの番号</param>
    /// <param name="_LowPow">振動の強さ</param>
    static public void LowVibration(int _Index, float _LowPow)
    {
        VibrationImpl(_Index, UnityEngine.Mathf.Clamp01(_LowPow), 0.0f);
    }

    /// <summary>
    /// 高周波の振動
    /// </summary>
    /// <param name="_Index">コントローラーの番号</param>
    /// <param name="_HighPow">振動の強さ</param>
    static public void HighVibration(int _Index, float _HighPow)
    {
        VibrationImpl(_Index, 0.0f, UnityEngine.Mathf.Clamp01(_HighPow));
    }

    /// <summary>
    /// 低周波と高周波の振動
    /// </summary>
    /// <param name="_Index">コントローラーの番号</param>
    /// <param name="_LowPow">低周波の振動の強さ</param>
    /// <param name="_HighPow">高周波の振動の強さ</param>
    static public void LowAndHighVibration(int _Index, float _LowPow, float _HighPow)
    {
        VibrationImpl(_Index, UnityEngine.Mathf.Clamp01(_LowPow), UnityEngine.Mathf.Clamp01(_HighPow));
    }

    /// <summary>
    /// 振動の実装部
    /// </summary>
    /// <param name="_Index">コントローラーの番号</param>
    /// <param name="_LowPow">低周波の振動の強さ</param>
    /// <param name="_HighPow">高周波の振動の強さ</param>
    static void VibrationImpl(int _Index, float _LowPow, float _HighPow)
    {
        //未接続なら何もしない
        if (!SwitchManager.GetInstance().IsConnect(_Index)) return;
        //IDの取得
        NpadId npadId = SwitchManager.GetInstance().GetNpadId(_Index);
        //スタイルの取得
        NpadStyle npadStyle = Npad.GetStyleSet(npadId);
        //デバイスの数を取得(0か1のみ取得する)
        int deviceCount = Vibration.GetDeviceHandles(
            m_VibrationDeviceHandles, 1, npadId, npadStyle);
        //デバイスの数が1じゃない場合は何もしない
        if (deviceCount != 1) return;
        //パワーをセット
        m_VibrationValue.amplitudeLow = _LowPow;
        m_VibrationValue.amplitudeHigh = _HighPow;
        //デバイスの初期化
        Vibration.InitializeDevice(m_VibrationDeviceHandles[0]);
        //振動の値をセット
        Vibration.SendValue(m_VibrationDeviceHandles[0], m_VibrationValue);
    }
}