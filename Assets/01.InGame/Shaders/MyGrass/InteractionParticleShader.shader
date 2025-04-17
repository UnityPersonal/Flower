Shader "custom/InteractionParticleShader"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1,1,1,1)
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "RenderPipeline" = "UniversalPipeline"
        }

        // ------------------------------------------------------------------
        //  Forward pass.
        Pass
        {
            Name "ForwardLit"
            
            ZWrite On
            Cull Off
            AlphaToMask On
            //Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_particles
             
            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // Custom Render Target 설정
            // _CustomRenderTarget은 C# 스크립트에서 설정해야 합니다.

            CBUFFER_START(UnityPerMaterial)
                sampler2D _BaseMap;
                float4 _BaseMap_ST;
                float4 _BaseColor;
            CBUFFER_END
            
            uniform float3 _Position;
            
            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 customData  : TEXCOORD1;                
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 customData  : TEXCOORD1;
            };

           
            v2f vert(appdata_t v)
            {
                v2f o;               

                // 클립 공간으로 변환
                float3 wpos = TransformObjectToWorld(v.vertex.xyz);
                
                o.position = TransformObjectToHClip(v.vertex.xyz);

                
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap) ;
                o.customData.xyz = v.customData.xzy;
                return o;
            }
            
            float4 frag(v2f i) : SV_Target
            {
                float alpha = tex2D(_BaseMap, i.uv).w;
                alpha = 1;
                // Set the color to white                
                return float4(i.customData.xyz, alpha);
            }

            ENDHLSL
        }

       
    }

}
