using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : SingletonMonoBehaviour<CameraShake>
{
    [SerializeField]
    Transform CameraTransform;
    Vector3 InitPosition;
    float timeCount = 0.0f;
    [SerializeField]
    float ShakeTime = 1.0f;
    Vector3 incrementPosition = new Vector3();
    void Start()
    {
        InitPosition = CameraTransform.position;
        timeCount = ShakeTime;
    }

    void Update()
    {
        if (timeCount < ShakeTime)
        {
            timeCount += Time.deltaTime;
            incrementPosition.x = Mathf.Sin(timeCount * 1000) / 2;
            incrementPosition.y = Mathf.Cos(timeCount * 1000) / 2;
            incrementPosition.z = Mathf.Sin(timeCount * 500) / 2;
            CameraTransform.position = InitPosition + incrementPosition;
        }
        else
        {
            CameraTransform.position = InitPosition;
        }
    }

    public void Shake()
    {
        timeCount = 0.0f;
    }
}
