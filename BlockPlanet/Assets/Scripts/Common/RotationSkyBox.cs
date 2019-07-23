using UnityEngine;

/// <summary>
/// スカイボックスの回転
/// </summary>
public class RotationSkyBox : MonoBehaviour
{
    // 1フレームに何度回すか[unit : deg]
    const float AnglePerFrame = 3;
    float rot = 0.0f;

    Material skyboxMaterial;

    void Start()
    {
        skyboxMaterial = RenderSettings.skybox;
    }

    void FixedUpdate()
    {
        rot += AnglePerFrame * Time.deltaTime;
        if (rot >= 360.0f)
        {
            rot -= 360.0f;
        }
        skyboxMaterial.SetFloat("_Rotation", rot);
    }

    void OnDestroy()
    {
        skyboxMaterial.SetFloat("_Rotation", 0.0f);
    }
}