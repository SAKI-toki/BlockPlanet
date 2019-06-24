using UnityEngine;

/// <summary>
/// 継承するだけでシングルトン化する
/// </summary>
/// <typeparam name="T">シングルトン化するクラスの型</typeparam>
public class Singleton<T> : MyMonoBehaviour where T : MyMonoBehaviour
{
    //インスタンス
    static T m_Instance = default(T);
    //インスタンス化フラグ
    static bool m_IsInstantiate = false;

    void Start()
    {
        //インスタンス化は一度だけ
        if (!m_IsInstantiate) Instantiate();
    }

    void Update()
    {
        if (!m_IsInstantiate) Instantiate();
        m_Instance.MyUpdate();
    }

    /// <summary>
    /// インスタンスのゲッタ
    /// </summary>
    /// <returns>インスタンス</returns>
    public static T GetInstance()
    {
        if (!m_IsInstantiate) Instantiate();
        return m_Instance;
    }

    static void Instantiate()
    {
        var instanceObjects = FindObjectsOfType(typeof(T));
        //一つのみ見つかった場合は正常
        if (instanceObjects.Length == 1)
        {
            //インスタンス化フラグをtrueにする
            m_IsInstantiate = true;
            m_Instance = (T)instanceObjects[0];
            m_Instance.MyStart();
        }
        //複数見つかった場合はエラーを出す
        else if (instanceObjects.Length > 1)
        {
            Debug.LogError(instanceObjects[0].GetType().FullName +
            "をComponentしているオブジェクトが複数あります。\n" +
            "一つにしてください");
        }
        else
        {
            Debug.LogError("Singletonにて未知のエラー:インスタンスが一つも見つかりません");
        }
    }
}

/// <summary>
/// オーバーライドするクラス
/// </summary>
public class MyMonoBehaviour : MonoBehaviour
{
    public virtual void MyStart() { }
    public virtual void MyUpdate() { }
}
