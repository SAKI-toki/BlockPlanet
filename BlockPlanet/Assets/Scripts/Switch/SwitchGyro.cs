using System.Collections.Generic;
#if UNITY_SWITCH  && !(UNITY_EDITOR)
using nn.hid;
#endif

/// <summary>
/// Switchのジャイロ
/// </summary>
static public class SwitchGyro
{
#if UNITY_SWITCH  && !(UNITY_EDITOR)
    //ジャイロのハンドラ
    static SixAxisSensorHandle[] gyroHandles = new SixAxisSensorHandle[1];
    //ジャイロの状態
    static SixAxisSensorState gyroState = new SixAxisSensorState();
#endif
    //ジャイロの基準を保持
    static Dictionary<int, float> baseGyro = new Dictionary<int, float>();

    /// <summary>
    /// ジャイロの基準をセット
    /// </summary>
    /// <param name="index">コントローラーの番号</param>
    static public void SetBaseGyro(int index)
    {
        //キーがない場合は追加しておく
        if (!baseGyro.ContainsKey(index)) baseGyro.Add(index, 0.0f);
        baseGyro[index] = GetGyroX(index) + baseGyro[index];
    }

    /// <summary>
    /// ジャイロの取得
    /// </summary>
    /// <param name="index">コントローラーの番号</param>
    /// <returns>ジャイロの回転(使用するx軸のみ)</returns>
    static public float GetGyroX(int index)
    {

#if UNITY_SWITCH  && !(UNITY_EDITOR)
        //未接続なら0.0f
        if (!SwitchManager.GetInstance().IsConnect(index)) return 0.0f;
        //キーがない場合は追加しておく
        if (!baseGyro.ContainsKey(index)) baseGyro.Add(index, 0.0f);
        //IDの取得
        NpadId npadId = SwitchManager.GetInstance().GetNpadId(index);
        //スタイルの取得
        NpadStyle npadStyle = Npad.GetStyleSet(npadId);
        //ハンドラを取得
        int handleCount = SixAxisSensor.GetHandles(gyroHandles, 1, npadId, npadStyle);
        //ハンドラの数が1じゃない場合は0.0f
        if (handleCount != 1) return 0.0f;
        //ジャイロスタート
        SixAxisSensor.Start(gyroHandles[0]);
        //状態の取得
        SixAxisSensor.GetState(ref gyroState, gyroHandles[0]);
        //右か左で返す値を変換する
        if (npadStyle == NpadStyle.JoyRight)
        {
            return gyroState.angle.x % 1 * 360 - baseGyro[index];
        }
        else
        {
            return gyroState.angle.x % 1 * -360 - baseGyro[index];
        }
#else
        return 0.0f;
#endif
    }
}