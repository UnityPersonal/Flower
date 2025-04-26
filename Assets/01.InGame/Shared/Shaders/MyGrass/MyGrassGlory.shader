Shader "Custom/MyGrassGlory"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        
    	_GloryParticleSize ("Glory Particle Size", Float) = 1
    	_GloryParticleColor ("Glory Particle Color", Color) = (1,1,1,1)
    	
        _BladeHeightMin ("Blade Height", Range(0, 10)) = 0.5       
        _BladeHeightMax ("Blade Height", Range(0, 10)) = 0.5
    	
    	_InteractionDistance ("Interaction Distance", Range(0,20)) = 5
        
        _WindMap("Wind Offset Map", 2D) = "bump" {}        
        _WindFrequency("Wind Frequency", Range(0, 1)) = 0.01
    	_WindVelocity("Wind Velocity", Vector) = (1, 0, 0, 0)
    	
        _TessMaxDistance("Tess Max Distance", Range(0,1000)) = 100
        _TessAmount("Tess Amount", Range(0,20)) = 10
    	
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="AlphaTest"
            "RenderPipeline"="UniversalPipeline"            
        }
        LOD 100
        Cull Off
       
        HLSLINCLUDE

            #include "GrassCore.hlsl"
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #pragma target 3.0
            #pragma multi_compile _ _EMISSION
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _SHADOWS_SOFT

            #pragma multi_compile_fog

            #define UNITY_PI 3.14159265359f
			#define UNITY_TWO_PI 6.28318530718f

            CBUFFER_START(UnityPerMaterial)
                sampler2D _MainTex;
                float4 _MainTex_ST;

				float _GloryParticleSize;
				float4 _GloryParticleColor;
            
                float _BladeHeightMin;
                float _BladeHeightMax;
            
                float _WindStrength;
                float _WindFrequency;
				float4 _WindVelocity;

                sampler2D _WindMap;
                float4 _WindMap_ST;

                float _TessMaxDistance;
                float _TessAmount;
            
				sampler2D _ForceMap;
				sampler2D _GloryMap;
            CBUFFER_END

            struct appdata
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
            	float4 color : COLOR;
                float2 uv : TEXCOORD0;
            	float4 uv2 : TEXCOORD1;
            };

            struct tessControlPoint
			{
				float4 positionWS : INTERNALTESSPOS;
				float3 normalWS : NORMAL;
            	float2 uv : TEXCOORD0;
			};

            struct v2g
            {
                float4 positionWS : SV_POSITION;
                float3 normalWS  : NORMAL;
            	float2 uv : TEXCOORD0;

            };

            struct tessFactors
			{
				float edge[3] : SV_TessFactor;
				float inside  : SV_InsideTessFactor;
			};

            struct g2f
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float2 uv : TEXCOORD2;
            };

            tessControlPoint vert(appdata v)
            {
                tessControlPoint o;
                o.positionWS = float4(TransformObjectToWorld(v.positionOS), 1.0f);
				o.normalWS = TransformObjectToWorldNormal(v.normalOS);
				o.uv = v.uv;
                return o;
            }

            tessFactors patchConstantFunc(InputPatch<tessControlPoint, 3> patch)
			{
				tessFactors f;

				float3 triPos0 = patch[0].positionWS.xyz;
				float3 triPos1 = patch[1].positionWS.xyz;
				float3 triPos2 = patch[2].positionWS.xyz;

				float3 edgePos0 = 0.5f * (triPos1 + triPos2);
				float3 edgePos1 = 0.5f * (triPos0 + triPos2);
				float3 edgePos2 = 0.5f * (triPos0 + triPos1);

				float3 camPos = _WorldSpaceCameraPos;

				float dist0 = distance(edgePos0, camPos);
				float dist1 = distance(edgePos1, camPos);
				float dist2 = distance(edgePos2, camPos);

				float fadeDist = _TessMaxDistance;

				float edgeFactor0 = saturate(1.0f - (dist0) / fadeDist);
				float edgeFactor1 = saturate(1.0f - (dist1) / fadeDist);
				float edgeFactor2 = saturate(1.0f - (dist2) / fadeDist);

				f.edge[0] = max(pow(edgeFactor0, 2) * _TessAmount, 1);
				f.edge[1] = max(pow(edgeFactor1, 2) * _TessAmount, 1);
				f.edge[2] = max(pow(edgeFactor2, 2) * _TessAmount, 1);

				f.inside = (f.edge[0] + f.edge[1] + f.edge[2]) / 3.0f;

				return f;
			}

            [domain("tri")]
			[outputcontrolpoints(3)]
			[outputtopology("triangle_cw")]
			[partitioning("integer")]
			[patchconstantfunc("patchConstantFunc")]
			tessControlPoint hull(InputPatch<tessControlPoint, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

            [domain("tri")]
			v2g domain(tessFactors factors, OutputPatch<tessControlPoint, 3> patch, float3 barycentricCoordinates : SV_DomainLocation)
			{
				v2g i;

				#define INTERPOLATE(fieldname) i.fieldname = \
					patch[0].fieldname * barycentricCoordinates.x + \
					patch[1].fieldname * barycentricCoordinates.y + \
					patch[2].fieldname * barycentricCoordinates.z;

				INTERPOLATE(positionWS)
				INTERPOLATE(normalWS)
				INTERPOLATE(uv)
				return i;
			}

            float3 GetWindVector(float3 pos)
			{
				float2 windUV = pos.xz * _WindMap_ST.xy + _WindMap_ST.zw + normalize(-_WindVelocity.xz) * _WindFrequency * _Time.y;
            	float3 windNoise = tex2Dlod(_WindMap, float4(windUV, 0, 0)).xyz;
            	windNoise = clamp(windNoise,0.1,1);
            	float3 windSample = windNoise* _WindVelocity;
            	float3 wind = float3(windSample.x , 0, windSample.z) * _WindVelocity.w;
            	
            	return wind;
			}

            
            g2f make_g2_f(
            	float3 pivotPosWS,
            	float3 posOS,
            	float3x3 localMatrix,
            	float2 uv
            	)
			{
				g2f o;           	
				o.positionWS = pivotPosWS.xyz + mul(localMatrix,posOS);
				o.positionCS = TransformWorldToHClip(o.positionWS);
				o.uv = float4(uv,0,0);            	
				return o;
			}

			            
            [maxvertexcount(4)]
            void geom(triangle v2g input[3], inout TriangleStream<g2f> triStream)
            {
                float3 pivotPosWS = (input[0].positionWS + input[1].positionWS + input[2].positionWS) / 3.0f;
            	float3 terrainNormal = (input[0].normalWS + input[1].normalWS + input[2].normalWS) / 3.0f;

				float4 glory = GloryColor(pivotPosWS);
            	/*if (glory.w < 0.1)
            		return;   */         		
            	
                float3x3 localMatrix = ExternalForceMatrix(pivotPosWS, terrainNormal);
            	localMatrix = mul(GetSlopeMatrix(pivotPosWS, terrainNormal), localMatrix);

				float r01 = rand01(pivotPosWS);
            	float grassHeight = lerp(_BladeHeightMin, _BladeHeightMax, r01);
            	
            	float3 wind = GetWindVector(pivotPosWS);
            	 
            	float3 terrainPivotPosWS = pivotPosWS + wind;
            	float3 gloryPivotPosOS = float3(0,grassHeight + 0.1,0);

            	float dz[2] = {-0.5,0.5};

	            for (int i = 0 ; i <= 1; ++i)
                {
                    float t = i;

	            	//float3 posOS = gloryPivotPosOS + float3(-0.5, 0, dz[i]);
	            	//float3 posOS2 = gloryPivotPosOS + float3(0.5, 0, dz[i]);

	            	float3 posOS = gloryPivotPosOS + float3(-0.5, 0, dz[i]) * _GloryParticleSize;
	            	float3 posOS2 = gloryPivotPosOS + float3(0.5, 0, dz[i]) * _GloryParticleSize;

                    triStream.Append(make_g2_f(terrainPivotPosWS, posOS,  localMatrix, float2(0,t)));                    
                    triStream.Append(make_g2_f(terrainPivotPosWS, posOS2, localMatrix, float2(1,t)));                                       
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
            #pragma require tessellation tessHW

            #pragma vertex vert
            #pragma hull hull
            #pragma domain domain
            #pragma geometry geom
            #pragma fragment frag


            float4 frag (g2f i) : SV_Target
            {
            	float4 mask = tex2D(_MainTex, i.uv);
            	if (mask.a < 0.5)
            		clip(-1);
            	
            	return _GloryParticleColor * mask.a;
            }
            ENDHLSL
        }	

Pass
		{
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }

			ZWrite On
			ZTest LEqual

			HLSLPROGRAM
			#pragma vertex shadowVert
			#pragma hull hull
			#pragma domain domain
			#pragma geometry geom
			#pragma fragment shadowFrag

			//#pragma multi_compile_instancing

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

			float3 _LightDirection;
			float3 _LightPosition;

			// Custom vertex shader to apply shadow bias.
			tessControlPoint shadowVert(appdata v)
			{
				tessControlPoint o;
				o.normalWS = TransformObjectToWorldNormal(v.normalOS);
				o.uv = v.uv;
				float3 positionWS = TransformObjectToWorld(v.positionOS);

				// Code required to account for shadow bias.
#if _CASTING_PUNCTUAL_LIGHT_SHADOW
				float3 lightDirectionWS = normalize(_LightPosition - positionWS);
#else
				float3 lightDirectionWS = _LightDirection;
#endif
				o.positionWS = float4(ApplyShadowBias(positionWS, o.normalWS, lightDirectionWS), 1.0f);

				return o;
			}

			float4 shadowFrag(g2f i) : SV_Target
			{
				return 0;
			}
			ENDHLSL
		}	
    } // Fallback "Diffuse"
}
