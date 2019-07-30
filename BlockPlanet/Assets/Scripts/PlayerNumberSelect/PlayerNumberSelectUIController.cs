using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// プレイ人数を選ぶシーンのUIの制御
/// </summary>
public class PlayerNumberSelectUIController : MonoBehaviour
{
    [SerializeField]
    GameObject[] onObjects;
    [SerializeField]
    GameObject[] offObjects;

    [SerializeField]
    Image shutterImage;
    [SerializeField]
    AudioSource aud;
    const float ShutterSpeed = 2.0f;


    /// <summary>
    /// 参加か不参加か
    /// </summary>
    public void SetOnOff(bool isOn)
    {
        foreach (var onObj in onObjects) onObj.SetActive(isOn);
        foreach (var offObj in offObjects) offObj.SetActive(!isOn);
        ShutterAnimation(isOn);
    }

    /// <summary>
    /// シャッターのアニメーション
    /// </summary>
    /// <param name="isOn">参加かどうか</param>
    void ShutterAnimation(bool isOn)
    {
        aud.Stop();
        aud.Play();
        StopAllCoroutines();
        if (isOn) StartCoroutine(ShutterOpen());
        else StartCoroutine(ShutterClose());
    }

    IEnumerator ShutterOpen()
    {
        float amount = shutterImage.fillAmount;
        while (amount > 0.0f)
        {
            aud.volume = amount;
            amount -= Time.deltaTime * ShutterSpeed;
            shutterImage.fillAmount = amount;
            yield return null;
        }
    }

    IEnumerator ShutterClose()
    {
        float amount = shutterImage.fillAmount;
        while (amount < 1.0f)
        {
            aud.volume = 1.0f - amount;
            amount += Time.deltaTime * ShutterSpeed;
            shutterImage.fillAmount = amount;
            yield return null;
        }
    }

}