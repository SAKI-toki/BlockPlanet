using UnityEngine;

/// <summary>
/// プレイ人数を選ぶシーンのUIの制御
/// </summary>
public class PlayerNumberSelectUIController : MonoBehaviour
{
    [SerializeField]
    GameObject[] onObjects;
    [SerializeField]
    GameObject[] offObjects;

    /// <summary>
    /// 参加か不参加か
    /// </summary>
    public void SetOnOff(bool isOn)
    {
        foreach (var onObj in onObjects) onObj.SetActive(isOn);
        foreach (var offObj in offObjects) offObj.SetActive(!isOn);
    }
}