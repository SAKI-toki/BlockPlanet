using UnityEngine;

public class DontDestroy : MonoBehaviour
{

    /// <summary>
    /// スカイボックスの回転
    /// </summary>


    // 1フレームに何度回すか[unit : deg]
    float AnglePerFrame = 3;
    float Rot = 0.0f;

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    private void FixedUpdate()
    {
        Rot += AnglePerFrame * Time.deltaTime;
        if (Rot >= 360.0f)
        {
            Rot -= 360.0f;
        }
        RenderSettings.skybox.SetFloat("_Rotation", Rot);
    }

    void OnDestroy()
    {
        RenderSettings.skybox.SetFloat("_Rotation", 0.0f);
    }
}
