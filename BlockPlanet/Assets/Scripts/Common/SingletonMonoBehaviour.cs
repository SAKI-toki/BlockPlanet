using UnityEngine;
using System;

/// <summary>
/// シングルトン
/// </summary>
public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    static T instance;
    /// <summary>
    /// インスタンスを取得するプロパティ
    /// </summary>
    /// <value>インスタンス</value>
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                Type t = typeof(T);

                instance = (T)FindObjectOfType(t);
                if (instance == null)
                {
                    Debug.LogError(t + " をアタッチしているGameObjectはありません");
                }
            }
            return instance;
        }
    }

    /// <summary>
    ///  他のゲームオブジェクトにアタッチされているか調べる
    ///  アタッチされている場合は破棄する。
    /// </summary>
    virtual protected void Awake()
    {
        CheckInstance();
    }

    /// <summary>
    /// インスタンスがあるかどうかチェックする
    /// </summary>
    /// <returns>インスタンスがない場合falseを返す</returns>
    protected bool CheckInstance()
    {
        if (instance == null)
        {
            instance = this as T;
            return true;
        }
        else if (Instance == this)
        {
            return true;
        }
        Destroy(this);
        return false;
    }
}
