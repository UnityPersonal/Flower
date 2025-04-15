Shader "Custom/MyProceduralGrass"
{
    Properties
    {
        _GroundColor ("Ground Color", Color) = (1, 1, 1, 1)
        _TipColor ("Tip Color", Color) = (1, 1, 1, 1)
        _MainTex ("Texture", 2D) = "white" {}
        
        _BladeWidth ("Blade Width", Range(0, 1)) = 0.5
        _BladeHeight ("Blade Height", Range(0, 2)) = 0.5       
        
        _WindMap("Wind Offset Map", 2D) = "bump" {}
        _WindVelocity("Wind Velocity", Vector) = (1, 0, 0, 0)
        _WindStrength("Wind Strength", Range(0, 1)) = 0.5
        _WindFrequency("Wind Frequency", Range(0, 1)) = 0.01
        
        _TessMaxDistance("Tess Max Distance", float) = 10
        _TessAmount("Tess Amount", Range(0,20)) = 10
        
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="Geometry"
            "RenderPipeline"="UniversalPipeline"            
        }
        LOD 100
        Cull Off
        
        HLSLINCLUDE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #pragma multi_compile_local WIND_ON_

            #define UNITY_PI 3.14159265359f
			#define UNITY_TWO_PI 6.28318530718f
            #define BLADE_SEGMENTS 4

            CBUFFER_START(UnityPerMaterial)
                float4 _GroundColor;
                float4 _TipColor;
                sampler2D _MainTex;
                float4 _MainTex_ST;
            
                float _BladeWidth;
                float _BladeHeight;
                float _WindStrength;
                float _WindFrequency;

                sampler2D _WindMap;
                float4 _WindMap_ST;

                float _TessMaxDistance;
                float _TessAmount;
            CBUFFER_END

            struct appdata
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct v2g
            {
                float4 positionWS : SV_POSITION;
                float3 normalWS  : NORMAL;
                float4 tangentWS : TANGENT;
                float2 uv : TEXCOORD0;                
            };

            struct v2g2
            {
                float4 positionOS : SV_POSITION;
                float3 normalOS  : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;                
            };

            struct g2f
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            float rand01(float3 co)
            {
                //(https://github.com/IronWarrior/UnityGrassGeometryShader)
                //https://forum.unity.com/threads/am-i-over-complicating-this-random-function.454887/#post-2949326
                return frac(sin(dot(co, float3(12.9898, 78.233, 37.719))) * 43758.5453);
            }

            float3x3 angleAxis3x3(float angle, float3 axis)
            {
                //https://gist.github.com/keijiro/ee439d5e7388f3aafc5296005c8c3f33
                float c, s;
				sincos(angle, s, c);

				float t = 1 - c;
				float x = axis.x;
				float y = axis.y;
				float z = axis.z;

				return float3x3
				(
					t * x * x + c, t * x * y - s * z, t * x * z + s * y,
					t * x * y + s * z, t * y * y + c, t * y * z - s * x,
					t * x * z - s * y, t * y * z + s * x, t * z * z + c
				);
            }

            float3x3 identity3x3()
			{
				return float3x3
				(
					1, 0, 0,
					0, 1, 0,
					0, 0, 1
				);
			}

            v2g2 vert(appdata v)
            {
                v2g2 o;
                o.positionOS = v.positionOS;
                o.normalOS = v.normalOS;
                o.tangentOS = v.tangentOS;
                o.uv = v.uv;
                return o;
            }

            g2f worldToClip(float3 pos, float3 offset, float3x3 transformationMatrix, float2 uv)
			{
				g2f o;

				o.positionCS = TransformObjectToHClip(pos + mul(transformationMatrix, offset));
				o.positionWS = TransformObjectToWorld(pos + mul(transformationMatrix, offset));
				o.uv = TRANSFORM_TEX(uv, _MainTex);

				return o;
			}
            
            [maxvertexcount(3 * (BLADE_SEGMENTS+1))]
            void geom(triangle v2g2 input[3], inout TriangleStream<g2f> triStream)
            {
                float3 pos = (input[0].positionOS + input[1].positionOS + input[2].positionOS) / 3.0f;

				float3 normal = (input[0].normalOS + input[1].normalOS + input[2].normalOS) / 3.0f;
				float4 tangent = (input[0].tangentOS + input[1].tangentOS + input[2].tangentOS) / 3.0f;
				float3 bitangent = cross(normal, tangent.xyz) * tangent.w;

            	float3x3 tangentToLocal = float3x3
					(
						tangent.x, normal.x, bitangent.x,
						tangent.y, normal.y, bitangent.y,
						tangent.z, normal.z, bitangent.z
					);

            	float3x3 randRotMatrix = angleAxis3x3(rand01(pos) * UNITY_TWO_PI, float3(0, 1, 0));
                float3x3 baseTransfrmMatrix = mul(tangentToLocal, randRotMatrix);

                float width = _BladeWidth;
                float height = _BladeHeight;

                for (int i = 0 ; i <= BLADE_SEGMENTS; ++i)
                {
                    float t = i / (float)BLADE_SEGMENTS;
                    float3 offset = float3(width, height * t, 0); // tangent space up is z-axis
                    float3x3 transformMatrix = baseTransfrmMatrix;

                    triStream.Append(worldToClip(pos, float3(offset.x, offset.y, offset.z), transformMatrix, float2(0,t)));                    
                    triStream.Append(worldToClip(pos, float3(-offset.x, offset.y, offset.z), transformMatrix, float2(1,t)));                                       
                }

                triStream.RestartStrip();               
            }
        ENDHLSL

        Pass
        {
            Name "GrassPass"
            Tags { "LightMode" = "UniversalForward"}
            
            ZWrite On
            ZTest LEqual
            
            HLSLPROGRAM
            #pragma require geometry

            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            

            float4 frag (g2f i) : SV_Target
            {
                // sample the texture
            	float4 col = float4(i.uv.x,i.uv.y,0,1);
                // apply fog
                return col;
            }
            ENDHLSL
        }
    }
}
