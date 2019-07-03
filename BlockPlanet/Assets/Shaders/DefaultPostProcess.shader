Shader "Custom/DefaultPostProcess"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
    }
    SubShader
    {
		Cull Off
		ZWrite Off
		ZTest Always
		Pass
		{
			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma vertex vert_img
			#pragma fragment frag

			sampler2D _MainTex;

			//色を決める関数
			fixed4 frag(v2f_img i) : COLOR
			{
				return tex2D(_MainTex,i.uv);
			}
			ENDCG
		}
    }
    FallBack "Diffuse"
}