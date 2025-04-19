Shader "Custom/FadeOut" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _MinDistance ("Minimum Distance", float) = 2
        _MaxDistance ("Maximum Distance", float) = 3
    }
    SubShader {
        Tags {
            "Queue"="Geometry"
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"   
            
        }
 
        Pass {
            
            Name "GrassPass"
            Tags { "LightMode" = "UniversalForward"}
            
            ZWrite On
            //ColorMask 0
            LOD 100

            
            HLSLINCLUDE
            #pragma vertex vert;
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            sampler2D _MainTex;

            struct appdata
            {
                float4 positionOS : POSITION;
            };

            
            struct v2f {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };
            float _MinDistance;
            float _MaxDistance;
            float4 _Color;


            v2f vert(appdata v)
            {
                v2f o = (v2f)0;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);

                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 c = float4(1,1,1,1);
                
                // help: https://developer.download.nvidia.com/cg/index_stdlib.html
                float distanceFromCamera = distance(i.positionWS, _WorldSpaceCameraPos);
                float fade = saturate((distanceFromCamera - _MinDistance) / _MaxDistance);
                c.a *= fade;
                return c;
            }
            
            /*float4 surf (Input IN, inout SurfaceOutput o) {
                float4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
                //o.Albedo = c.rgb;
                
                // help: https://developer.download.nvidia.com/cg/index_stdlib.html
                float distanceFromCamera = distance(IN.worldPos, _WorldSpaceCameraPos);
                float fade = saturate((distanceFromCamera - _MinDistance) / _MaxDistance);
                
                //o.Alpha = c.a * fade;
                return c;
            }*/
            ENDHLSL
        }
   
        
    }
    FallBack "Diffuse"
}
