﻿Shader "Custom/SelectSpecial"
{
    properties
    {
        _X("x",Float)=0
        _Z("z",Float)=0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        struct Input
        {
            float3 worldPos;
        };
        float _X;
        float _Z;
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float dist = distance(fixed3(_X,0 ,_Z),IN.worldPos);
            float loopDist = 6;
            dist+=_Time.y*10;
            dist=dist%(loopDist*6);
        
            float minimumDist=dist%loopDist/loopDist;
            //128,0,128
            //r 128=>0 b 128=>255
            if(dist<1*loopDist)
            {
                o.Albedo=fixed4(0.5-minimumDist/2,0,0.5+minimumDist/2,1);
            }
            //0,0,255
            //g 0=>255
            else if(dist<2*loopDist)
            {
                o.Albedo=fixed4(0,minimumDist,1,1);
            }
            //0,255,255
            //g 255=>128 b 255=>0
            else if(dist<3*loopDist)
            {
                o.Albedo=fixed4(0,1-minimumDist/2,1-minimumDist,1);
            }
            //0,128,0
            //r 0=>255 g 128=>255
            else if(dist<4*loopDist)
            {
                o.Albedo=fixed4(minimumDist,0.5+minimumDist/2,0,1);
            }
            //255,255,0
            //g 255=>0
            else if(dist<5*loopDist)
            {
                o.Albedo=fixed4(1,1-minimumDist,0,1);
            }
            //255,0,0
            //r 255=>128 b 0=>128
            else if(dist<6*loopDist)
            {
                o.Albedo=fixed4(1-minimumDist/2,0,minimumDist/2,1);
            }

        }
        ENDCG
    }
    FallBack "Diffuse"
}
