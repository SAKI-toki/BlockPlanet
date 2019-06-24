using UnityEngine;
using nn.hid;

/// <summary>
/// Switchでジョイコンを横持ちにした場合の入力
/// </summary>
public enum SwitchButton : long
{
    Up = NpadButton.Y | NpadButton.Right,
    Down = NpadButton.A | NpadButton.Left,
    Right = NpadButton.X | NpadButton.Down,
    Left = NpadButton.B | NpadButton.Up,
    SR = NpadButton.RightSR | NpadButton.LeftSR,
    SL = NpadButton.RightSL | NpadButton.LeftSL,
    StickUp = NpadButton.StickRLeft | NpadButton.StickLRight,
    StickDown = NpadButton.StickRRight | NpadButton.StickLLeft,
    StickRight = NpadButton.StickRUp | NpadButton.StickLDown,
    StickLeft = NpadButton.StickRDown | NpadButton.StickLUp,
    Pause = NpadButton.Plus | NpadButton.Minus,
    Ok = SwitchButton.Right,
    Cancel = SwitchButton.Down,
    None = 0
}

/// <summary>
/// スイッチの入力情報
/// </summary>
static public class SwitchInput
{
    //1フレーム前のボタンの状態
    static long[] m_PrevButtons;
    //現在のボタンの状態
    static long[] m_CurrentButtons;
    //スティックの情報
    struct StickInfo
    {
        public float m_Horizontal, m_Vertical;
    }
    //スティックの水平
    static StickInfo[] m_StickInfos;
    //コントローラーの状態
    static NpadState m_NpadState = new NpadState();

    /// <summary>
    /// 入力の初期化
    /// </summary>
    /// <param name="_NpadIdsLength">使用するIDの配列の長さ</param>
    static public void InputInit(int _NpadIdsLength)
    {
        //配列の要素確保
        m_PrevButtons = new long[_NpadIdsLength];
        m_CurrentButtons = new long[_NpadIdsLength];
        m_StickInfos = new StickInfo[_NpadIdsLength];
#if UNITY_EDITOR
        m_XboxCurrentButtons = new bool[_NpadIdsLength, (int)XboxInput.None];
        m_XboxPrevButtons = new bool[_NpadIdsLength, (int)XboxInput.None];
#endif
    }

    /// <summary>
    /// 入力の更新
    /// </summary>
    /// <param name="_Index">コントローラーの番号</param>
    /// <param name="_NpadId">パッドのID</param>
    static public void InputUpdate(int _Index, NpadId _NpadId)
    {
#if UNITY_EDITOR
        for (int i = 0; i < (int)XboxInput.None; ++i)
        {
            m_XboxPrevButtons[_Index, i] = m_XboxCurrentButtons[_Index, i];
            m_XboxCurrentButtons[_Index, i] = InputXboxButton(_Index, (XboxInput)i);
        }
#endif
        m_PrevButtons[_Index] = m_CurrentButtons[_Index];
        //未接続
        if (!SwitchManager.GetInstance().IsConnect(_Index)) return;
        //スタイルを取得
        NpadStyle npadStyle = Npad.GetStyleSet(_NpadId);
        //スタイルが合うかどうか
        if ((npadStyle & SwitchManager.GetInstance().GetNpadStyle()) == 0) return;
        //入力情報を取得
        Npad.GetState(ref m_NpadState, _NpadId, npadStyle);
        m_CurrentButtons[_Index] = (long)m_NpadState.buttons;
        //スティックの更新
        if (npadStyle == NpadStyle.JoyRight)
        {
            m_StickInfos[_Index].m_Horizontal = m_NpadState.analogStickR.fy;
            m_StickInfos[_Index].m_Vertical = -m_NpadState.analogStickR.fx;
        }
        else if (npadStyle == NpadStyle.JoyLeft)
        {
            m_StickInfos[_Index].m_Horizontal = -m_NpadState.analogStickL.fy;
            m_StickInfos[_Index].m_Vertical = m_NpadState.analogStickL.fx;
        }
        else
        {
            Debug.LogError("JoyRight,JoyLeft以外のStyleを取得");
        }
    }

    /// <summary>
    /// ボタンを今のフレームに押したか
    /// </summary>
    /// <param name="_Index">コントローラーの番号</param>
    /// <param name="_Button">取得するボタン</param>
    /// <returns>押したならtrueを返す</returns>
    static public bool GetButtonDown(int _Index, SwitchButton _Button)
    {
        //未接続ならfalse
        if (!SwitchManager.GetInstance().IsConnect(_Index)) return false;
        return !IsPrevButton(_Index, _Button) && IsCurrentButton(_Index, _Button);
    }

    /// <summary>
    /// 今のフレームにボタンを押しているか
    /// </summary>
    /// <param name="_Index">コントローラーの番号</param>
    /// <param name="_Button">取得するボタン</param>
    /// <returns>押しているならtrueを返す</returns>
    static public bool GetButton(int _Index, SwitchButton _Button)
    {
        //未接続ならfalse
        if (!SwitchManager.GetInstance().IsConnect(_Index)) return false;
        return IsCurrentButton(_Index, _Button);
    }

    /// <summary>
    /// 今のフレームにボタンを離したか
    /// </summary>
    /// <param name="_Index">コントローラーの番号</param>
    /// <param name="_Button">取得するボタン</param>
    /// <returns>離したならtrueを返す</returns>
    static public bool GetButtonUp(int _Index, SwitchButton _Button)
    {
        //未接続ならfalse
        if (!SwitchManager.GetInstance().IsConnect(_Index)) return false;
        return IsPrevButton(_Index, _Button) && !IsCurrentButton(_Index, _Button);
    }

    /// <summary>
    /// スティックの水平を取得
    /// </summary>
    /// <param name="_Index">コントローラーの番号</param>
    /// <returns>スティックの垂直</returns>
    static public float GetHorizontal(int _Index)
    {
        //未接続なら0.0f
        if (!SwitchManager.GetInstance().IsConnect(_Index)) return 0.0f;
#if UNITY_EDITOR
        return ConvertSwitchHorizontalToXboxHorizontal(_Index);
#else
        return m_StickInfos[_Index].m_Horizontal;
#endif
    }

    /// <summary>
    /// スティックの垂直を取得
    /// </summary>
    /// <param name="_Index">コントローラーの番号</param>
    /// <returns>スティックの垂直</returns>
    static public float GetVertical(int _Index)
    {
        //未接続なら0.0f
        if (!SwitchManager.GetInstance().IsConnect(_Index)) return 0.0f;
#if UNITY_EDITOR
        return ConvertSwitchVerticalToXboxVertical(_Index);
#else
        return m_StickInfos[_Index].m_Vertical;
#endif
    }

    /// <summary>
    /// 1フレーム前にボタンを押していたか
    /// </summary>
    /// <param name="_Index">コントローラーの番号</param>
    /// <param name="_Button">取得するボタン</param>
    /// <returns>押しているならtrueを返す</returns>
    static bool IsPrevButton(int _Index, SwitchButton _Button)
    {
#if UNITY_EDITOR
        return m_XboxPrevButtons[_Index, (int)SwitchConvertXbox(_Button)];
#else
        return (m_PrevButtons[_Index] & (long)_Button) != 0;
#endif
    }

    /// <summary>
    /// 今のフレームにボタンを押しているか
    /// </summary>
    /// <param name="_Index">コントローラーの番号</param>
    /// <param name="_Button">取得するボタン</param>
    /// <returns>押しているならtrueを返す</returns>
    static bool IsCurrentButton(int _Index, SwitchButton _Button)
    {
#if UNITY_EDITOR
        return m_XboxCurrentButtons[_Index, (int)SwitchConvertXbox(_Button)];
#else
        return (m_CurrentButtons[_Index] & (long)_Button) != 0;
#endif
    }

#if UNITY_EDITOR

    static bool[,] m_XboxCurrentButtons;
    static bool[,] m_XboxPrevButtons;
    enum XboxInput
    {
        Up, Down, Right, Left, SR, SL, StickUp, StickDown, StickRight, StickLeft, Pause, None
    }

    /// <summary>
    /// Switchの入力からXboxの入力にコンバートする
    /// </summary>
    /// <param name="_Button">Switchの入力</param>
    /// <returns>Xboxの入力</returns>
    static XboxInput SwitchConvertXbox(SwitchButton _Button)
    {
        switch (_Button)
        {
            case SwitchButton.Up:
                return XboxInput.Up;
            case SwitchButton.Down:
                return XboxInput.Down;
            case SwitchButton.Right:
                return XboxInput.Right;
            case SwitchButton.Left:
                return XboxInput.Left;
            case SwitchButton.SR:
                return XboxInput.SR;
            case SwitchButton.SL:
                return XboxInput.SL;
            case SwitchButton.StickUp:
                return XboxInput.StickUp;
            case SwitchButton.StickDown:
                return XboxInput.StickDown;
            case SwitchButton.StickRight:
                return XboxInput.StickRight;
            case SwitchButton.StickLeft:
                return XboxInput.StickLeft;
            case SwitchButton.Pause:
                return XboxInput.Pause;
            default:
                return XboxInput.None;
        }
    }

    /// <summary>
    /// XBoxの入力
    /// </summary>
    /// <param name="_Index">コントローラーの番号</param>
    /// <param name="_Button">ボタン</param>
    /// <returns>入力されているかどうか</returns>
    static bool InputXboxButton(int _Index, XboxInput _Button)
    {
        const int addNum = KeyCode.Joystick2Button0 - KeyCode.Joystick1Button0;
        switch (_Button)
        {
            case XboxInput.Up:
                return Input.GetKey(KeyCode.Joystick1Button3 + _Index * addNum);
            case XboxInput.Down:
                return Input.GetKey(KeyCode.Joystick1Button0 + _Index * addNum);
            case XboxInput.Right:
                return Input.GetKey(KeyCode.Joystick1Button1 + _Index * addNum);
            case XboxInput.Left:
                return Input.GetKey(KeyCode.Joystick1Button2 + _Index * addNum);
            case XboxInput.SR:
                return Input.GetKey(KeyCode.Joystick1Button5 + _Index * addNum);
            case XboxInput.SL:
                return Input.GetKey(KeyCode.Joystick1Button4 + _Index * addNum);
            case XboxInput.StickUp:
                return Input.GetAxisRaw("Vertical" + (_Index + 1).ToString()) > 0;
            case XboxInput.StickDown:
                return Input.GetAxisRaw("Vertical" + (_Index + 1).ToString()) < 0;
            case XboxInput.StickRight:
                return Input.GetAxisRaw("Horizontal" + (_Index + 1).ToString()) > 0;
            case XboxInput.StickLeft:
                return Input.GetAxisRaw("Horizontal" + (_Index + 1).ToString()) < 0;
            case XboxInput.Pause:
                return Input.GetKey(KeyCode.Joystick1Button7 + _Index * addNum);
            default:
                return false;
        }
    }

    /// <summary>
    /// スティックの水平入力のコンバーター
    /// </summary>
    /// <param name="_Index">コントローラーの番号</param>
    /// <returns>スティックの水平入力</returns>
    static float ConvertSwitchHorizontalToXboxHorizontal(int _Index)
    {
        return Input.GetAxisRaw("Horizontal" + (_Index + 1).ToString());
    }

    /// <summary>
    /// スティックの垂直入力のコンバーター
    /// </summary>
    /// <param name="_Index">コントローラーの番号</param>
    /// <returns>スティックの垂直入力</returns>
    static float ConvertSwitchVerticalToXboxVertical(int _Index)
    {
        return Input.GetAxisRaw("Vertical" + (_Index + 1).ToString());
    }

#endif
}
