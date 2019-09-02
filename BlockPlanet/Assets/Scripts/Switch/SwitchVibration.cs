#if UNITY_SWITCH  && !(UNITY_EDITOR)
using nn.hid;
#endif

/// <summary>
/// スイッチの振動
/// </summary>
static public class SwitchVibration
{
#if UNITY_SWITCH  && !(UNITY_EDITOR)
    //デバイスのハンドラ
    static VibrationDeviceHandle[] vibrationDeviceHandles = new VibrationDeviceHandle[1];
    //振動の値
    static VibrationValue vibrationValue = VibrationValue.Make();
#endif

    /// <summary>
    /// 低周波の振動
    /// </summary>
    /// <param name="index">コントローラーの番号</param>
    /// <param name="lowPow">振動の強さ</param>
    static public void LowVibration(int index, float lowPow)
    {
        VibrationImpl(index, UnityEngine.Mathf.Clamp01(lowPow), 0.0f);
    }

    /// <summary>
    /// 高周波の振動
    /// </summary>
    /// <param name="index">コントローラーの番号</param>
    /// <param name="highPow">振動の強さ</param>
    static public void HighVibration(int index, float highPow)
    {
        VibrationImpl(index, 0.0f, UnityEngine.Mathf.Clamp01(highPow));
    }

    /// <summary>
    /// 低周波と高周波の振動
    /// </summary>
    /// <param name="index">コントローラーの番号</param>
    /// <param name="lowPow">低周波の振動の強さ</param>
    /// <param name="highPow">高周波の振動の強さ</param>
    static public void LowAndHighVibration(int index, float lowPow, float highPow)
    {
        VibrationImpl(index, UnityEngine.Mathf.Clamp01(lowPow), UnityEngine.Mathf.Clamp01(highPow));
    }

    /// <summary>
    /// 振動の実装部
    /// </summary>
    /// <param name="index">コントローラーの番号</param>
    /// <param name="lowPow">低周波の振動の強さ</param>
    /// <param name="highPow">高周波の振動の強さ</param>
    static void VibrationImpl(int index, float lowPow, float highPow)
    {
#if UNITY_SWITCH  && !(UNITY_EDITOR)
        //未接続なら何もしない
        if (!SwitchManager.GetInstance().IsConnect(index)) return;
        //IDの取得
        NpadId npadId = SwitchManager.GetInstance().GetNpadId(index);
        //スタイルの取得
        NpadStyle npadStyle = Npad.GetStyleSet(npadId);
        //デバイスの数を取得(0か1のみ取得する)
        int deviceCount = Vibration.GetDeviceHandles(
            vibrationDeviceHandles, 1, npadId, npadStyle);
        //デバイスの数が1じゃない場合は何もしない
        if (deviceCount != 1) return;
        //パワーをセット
        vibrationValue.amplitudeLow = lowPow;
        vibrationValue.amplitudeHigh = highPow;
        //デバイスの初期化
        Vibration.InitializeDevice(vibrationDeviceHandles[0]);
        //振動の値をセット
        Vibration.SendValue(vibrationDeviceHandles[0], vibrationValue);
#endif
    }
}