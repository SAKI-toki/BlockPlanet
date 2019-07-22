using UnityEngine;

public class DontDestroy : MonoBehaviour
{

    /// <summary>
    /// スカイボックスの回転
    /// </summary>


    // 1フレームに何度回すか[unit : deg]
    float anglePerFrame = 3;
    float rot = 0.0f;

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    private void FixedUpdate()
    {
        rot += anglePerFrame * Time.deltaTime;
        if (rot >= 360.0f)
        {
            rot -= 360.0f;
        }
        RenderSettings.skybox.SetFloat("_Rotation", rot);
    }

    void OnDestroy()
    {
        RenderSettings.skybox.SetFloat("_Rotation", 0.0f);
    }
}
