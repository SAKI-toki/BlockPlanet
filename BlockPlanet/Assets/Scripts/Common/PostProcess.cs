using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class PostProcess : MonoBehaviour
{
    [SerializeField, Header("マテリアル")]
    Material InvertMaterial = null;

    /// <summary>
    /// 全てのレンダリングが完了した時に呼ばれる関数
    /// </summary>
    /// <param name="src">コピー元の画像</param>
    /// <param name="dest">コピー先のRenderTextureオブジェクト</param>
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(src, dest, InvertMaterial);
    }
}
