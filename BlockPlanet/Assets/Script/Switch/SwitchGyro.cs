using System.Collections.Generic;
using nn.hid;

/// <summary>
/// Switchのジャイロ
/// </summary>
static public class SwitchGyro
{
    //ジャイロのハンドラ
    static SixAxisSensorHandle[] m_GyroHandles = new SixAxisSensorHandle[1];
    //ジャイロの状態
    static SixAxisSensorState m_GyroState = new SixAxisSensorState();
    //ジャイロの基準を保持
    static Dictionary<int, float> m_BaseGyro = new Dictionary<int, float>();

    /// <summary>
    /// ジャイロの基準をセット
    /// </summary>
    /// <param name="_Index">コントローラーの番号</param>
    static public void SetBaseGyro(int _Index)
    {
        //キーがない場合は追加しておく
        if (!m_BaseGyro.ContainsKey(_Index)) m_BaseGyro.Add(_Index, 0.0f);
        m_BaseGyro[_Index] = GetGyroX(_Index) + m_BaseGyro[_Index];
    }

    /// <summary>
    /// ジャイロの取得
    /// </summary>
    /// <param name="_Index">コントローラーの番号</param>
    /// <returns>ジャイロの回転(使用するx軸のみ)</returns>
    static public float GetGyroX(int _Index)
    {
        //未接続なら0.0f
        if (!SwitchManager.GetInstance().IsConnect(_Index)) return 0.0f;
        //キーがない場合は追加しておく
        if (!m_BaseGyro.ContainsKey(_Index)) m_BaseGyro.Add(_Index, 0.0f);
        //IDの取得
        NpadId npadId = SwitchManager.GetInstance().GetNpadId(_Index);
        //スタイルの取得
        NpadStyle npadStyle = Npad.GetStyleSet(npadId);
        //ハンドラを取得
        int handleCount = SixAxisSensor.GetHandles(m_GyroHandles, 1, npadId, npadStyle);
        //ハンドラの数が1じゃない場合は0.0f
        if (handleCount != 1) return 0.0f;
        //ジャイロスタート
        SixAxisSensor.Start(m_GyroHandles[0]);
        //状態の取得
        SixAxisSensor.GetState(ref m_GyroState, m_GyroHandles[0]);
        //右か左で返す値を変換する
        if (npadStyle == NpadStyle.JoyRight)
        {
            return m_GyroState.angle.x % 1 * 360 - m_BaseGyro[_Index];
        }
        else
        {
            return m_GyroState.angle.x % 1 * -360 - m_BaseGyro[_Index];
        }
    }
}