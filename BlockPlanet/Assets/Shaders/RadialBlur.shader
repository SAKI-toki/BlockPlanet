//円形ブラー
Shader "Custom/RadialBlur"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		_SampleCount("SampleCount",Range(4,40)) = 8
		_Strength("Strength",Range(0.0,1.0)) = 0.5
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
			half _SampleCount;
			uniform half _Strength;

			//色を決める関数
			fixed4 frag(v2f_img i) : COLOR
			{
				half4 col = 0;
				half2 symmetryUv = i.uv - 0.5;
				half distance = length(symmetryUv);
				half factor = _Strength / _SampleCount * distance;
				for (int j = 0; j < _SampleCount; ++j)
				{
					half uvOffset = 1 - factor * j;
					col += tex2D(_MainTex, symmetryUv * uvOffset + 0.5);
				}
				col /= _SampleCount;
				return col;
			}
			ENDCG
		}
    }
    FallBack "Diffuse"
}