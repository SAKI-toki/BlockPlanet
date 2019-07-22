using UnityEngine;

/// <summary>
/// カメラの振動
/// </summary>
public class CameraShake : SingletonMonoBehaviour<CameraShake>
{
    Transform cameraTransform;
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

    public void Shake()
    {
        timeCount = 0.0f;
    }
}
