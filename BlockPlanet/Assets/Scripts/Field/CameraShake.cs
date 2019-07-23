using UnityEngine;

/// <summary>
/// カメラの振動
/// </summary>
public class CameraShake : SingletonMonoBehaviour<CameraShake>
{
    Transform cameraTransform;
    //初期位置
    Vector3 initPosition;
    float timeCount = 0.0f;
    [SerializeField]
    float shakeTime = 1.0f;
    Vector3 incrementPosition = new Vector3();
    void Start()
    {
        cameraTransform = GetComponent<Transform>();
        initPosition = cameraTransform.position;
        timeCount = shakeTime;
    }

    void Update()
    {
        //揺れる
        if (timeCount < shakeTime)
        {
            timeCount += Time.deltaTime;
            incrementPosition.x = Mathf.Sin(timeCount * 1000) / 2;
            incrementPosition.y = Mathf.Cos(timeCount * 1000) / 2;
            incrementPosition.z = Mathf.Sin(timeCount * 500) / 2;
            cameraTransform.position = initPosition + incrementPosition;
        }
        else
        {
            cameraTransform.position = initPosition;
        }
    }

    /// <summary>
    /// 揺らす
    /// </summary>
    public void Shake()
    {
        timeCount = 0.0f;
    }
}
