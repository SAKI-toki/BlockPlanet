using UnityEngine;

/// <summary>
/// マテリアルの自動セット
/// </summary>
public class AutoMaterialSet : MonoBehaviour
{
    [SerializeField]
    Material[] mats;

    void Start()
    {
        if (mats.Length == 0) return;
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            foreach (var mat in mats)
            {
                if (mat.name == renderer.gameObject.name)
                {
                    renderer.sharedMaterial = mat;
                    break;
                }
            }
        }
    }

    public void SetMaterial(Material[] _mats)
    {
        mats = new Material[_mats.Length];
        for (int i = 0; i < mats.Length; ++i)
        {
            mats[i] = _mats[i];
        }
    }
}