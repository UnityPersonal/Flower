Shader "Universal Render Pipeline/Particles/DirectionParticleShader"
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

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_particles

            // Custom Render Target 설정
            // _CustomRenderTarget은 C# 스크립트에서 설정해야 합니다.
            
            uniform float3 _Position;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 color : COLOR;
                float4 uv : TEXCOORD0;
                float4 custom : TEXCOORD1;                
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float3 custom : TEXCOORD0;
            };

            
            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Particles/ParticlesUnlitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Particles/ParticlesUnlitForwardPass.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            v2f vert(appdata_t v)
            {
                v2f o;
                
                // 월드 공간에서 float3(0,1,0) 방향으로 정렬
                float3 upDirection = float3(0, 1, 0);
                float3 right = normalize(cross(float3(0, 0, 1), upDirection));
                float3 forward = cross(upDirection, right);

                float3x3 rotationMatrix = float3x3(right, upDirection, forward);

                // 정점을 회전 행렬로 변환
                float3 alignedVertex = mul(rotationMatrix, v.vertex.xyz);

                // 클립 공간으로 변환
                o.position = TransformObjectToHClip(alignedVertex);
                o.custom.xy = v.uv.zw;
                o.custom.z = v.custom.x;
                return o;
            }
            
            float4 frag(v2f i) : SV_Target
            {
                // Set the color to white
                return float4(i.custom.xyz, 1);
            }

            ENDHLSL
        }

       
    }

}
