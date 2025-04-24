Shader "Custom/MyGrassSSS"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}    	
    	[NoScaleOffset] _LandColorMap("Land Color Map", 2D) = "white" {}
    	[NoScaleOffset] _GrassColorMap("Grass Color Map", 2D) = "white" {}
        
        _BladeWidth ("Blade Width", Range(0, 1)) = 0.5
        _BladeHeightMin ("Blade Height", Range(0, 10)) = 0.5       
        _BladeHeightMax ("Blade Height", Range(0, 10)) = 0.5
    	
        _WindMap("Wind Offset Map", 2D) = "bump" {}        
        _WindFrequency("Wind Frequency", Range(0, 1)) = 0.01
    	_WindVelocity("Wind Velocity", Vector) = (1, 0, 0, 0)
    	
    	_SunMap("Sun Color Noise Map", 2D) = "grayscale" {}
    	_SunColor("Sun Color", Color) = (1,1,1,1)
        
    	_SkyLightPower("Sky Light Power", Float) = 1
    	
        _TessMaxDistance("Tess Max Distance", Range(0,1000)) = 100
        _TessAmount("Tess Amount", Range(0,20)) = 10
	    _PaintWeight("Paint Weight", Range(0, 1)) = 0.5    	
    	
    	[NoScaleOffset] _PaintMap("Painted Map", 2D) = "black"    	
    	[NoScaleOffset] _PaintDetailMap("Paint DetailMap", 2D) = "black"
    	[NoScaleOffset] _ForceMap("Force Map", 2D) = "black"
    	
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
            #define BLADE_SEGMENTS 8

            CBUFFER_START(UnityPerMaterial)
                sampler2D _MainTex;
                float4 _MainTex_ST;

				sampler2D _LandColorMap;
				sampler2D _GrassColorMap;
			
            
                float _BladeWidth;
                float _BladeHeightMin;
                float _BladeHeightMax;

                float _WindStrength;
                float _WindFrequency;
				float4 _WindVelocity;

                sampler2D _WindMap;
                float4 _WindMap_ST;

                float _TessMaxDistance;
                float _TessAmount;

				sampler2D _SunMap;
				float4 _SunMap_ST;

				float4 _SunColor;
				float _SkyLightPower;
				sampler2D _PaintDetailMap;
				float _PaintWeight;
            CBUFFER_END

            struct appdata
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
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
            	float4 color : COLOR;
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

            float2 RotateUV(float2 uv, float2 pivot, float2 direction)
			{
			    // UV 좌표를 피벗 기준으로 이동
			    uv -= pivot;

            	float len = length(direction); 
			    // 방향 벡터 정규화
			    direction = normalize(direction);

			    // 회전 행렬 생성
			    float cosTheta = direction.x;
			    float sinTheta = direction.y;

			    float2 rotatedUV;
			    rotatedUV.x = uv.x * cosTheta - uv.y * sinTheta;
			    rotatedUV.y = uv.x * sinTheta + uv.y * cosTheta;

			    // 피벗 기준으로 다시 이동
			    rotatedUV += pivot;

			    return rotatedUV;
			}

            float3 GetSunshineColor(float3 pos)
            {
            	float2 uv = pos.xz * _SunMap_ST.xy + _SunMap_ST.zw + normalize(-_WindVelocity.xz) * _Time.y;
				uv = RotateUV(uv, float2(0,0), _WindVelocity.xz	 );
            	float3 noise = tex2D(_SunMap, uv).w;

            	return noise * _SunColor * pow(_SunColor.w, 2.2f);
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

            float Fresnel(float3 N, float3 V, float power)
            {
	            return pow((1.0-saturate(dot(N,V))), power);
            }

			float3 SampleSkyboxLight(float3 normalWS)
			{
			    // SH 데이터 가져오기
			    float3 ambient = 0.0;

			    // Unity의 SH 데이터 활용
			    ambient += normalWS.x * normalWS.x * unity_SHC.xyz;
			    ambient += normalWS.y * normalWS.y * unity_SHC.wyz;
			    ambient += normalWS.z * normalWS.z * unity_SHC.zwx;
			    ambient += normalWS.x * unity_SHAr.xyz;
			    ambient += normalWS.y * unity_SHAg.xyz;
			    ambient += normalWS.z * unity_SHAb.xyz;
			    ambient += unity_SHBr.xyz;

			    return ambient;
			}
            

            g2f make_g2_f(
            	float3 pos,
            	float3 terrainNormal,
            	float3 normal,
            	float3 offset,
            	float3x3 localMatrix,
            	float2 uv,
            	float4 grassColor
            	)
			{
				g2f o;           	
				o.positionCS = TransformWorldToHClip(pos + mul(localMatrix, offset) );
				o.positionWS = pos + mul(localMatrix, offset);
				
				o.uv = float4(uv,0,0);            	
            	//o.color = grassColor * NdotL;
            	o.color = float4(1,1,1,1);
            	o.color.xyz = grassColor;
            	// apply SSS
            	float3 viewWS = _WorldSpaceCameraPos - o.positionWS;            	
            	float ViewWSLength = length(viewWS);            	

            	float thickness =0.1f;
            	Light mainLight = GetMainLight();
            	
            	float3 L = normalize(mainLight.direction - o.positionWS);
            	float3 V = normalize(viewWS);
            	float3 N = normal;

            	float3 H =  normalize(L + N) ;
            	float diffuse =  dot(mainLight.direction, terrainNormal) * 0.5 + 0.5;
            	float specular = saturate(dot(N,H));
            	specular = pow(specular,1);

				//o.color.xyz *= diffuse;
            	//o.color.xyz += SampleSH(terrainNormal) * 0.1;            	
            	o.color.xyz +=
            		Fresnel(terrainNormal,V,5) *
            			SampleSH(terrainNormal) * uv.y * _SkyLightPower;

            	float fogCoord = ComputeFogFactor(o.positionCS.z);
            	o.color.xyz = MixFog(o.color.xyz, fogCoord);

				return o;
			}

            [maxvertexcount(2 * (BLADE_SEGMENTS+1))]
            void geom(triangle v2g input[3], inout TriangleStream<g2f> triStream)
            {
                float3 pivotPosWS = (input[0].positionWS + input[1].positionWS + input[2].positionWS) / 3.0f;
            	float3 terrainNormal = (input[0].normalWS + input[1].normalWS + input[2].normalWS) / 3.0f;

                float3x3 localMatrix = ExternalForceMatrix(pivotPosWS, terrainNormal);
            	localMatrix = mul(GetSlopeMatrix(pivotPosWS, terrainNormal), localMatrix);
            	//localMatrix = identity3x3();

				float r01 = rand01(pivotPosWS);
            	float grassHeight = lerp(_BladeHeightMin, _BladeHeightMax, r01);

				float width = _BladeWidth;
            	float tipWidth = width * 0.5f;
            	
            	float3 wind = GetWindVector(pivotPosWS);

				// rotation make grass Lookat() camera just like a bilboard;
            	float3 cameraTransformRightWS = UNITY_MATRIX_V[0].xyz;//UNITY_MATRIX_V[0].xyz == world space camera Right unit vector
                float3 cameraTransformUpWS = UNITY_MATRIX_V[1].xyz;//UNITY_MATRIX_V[1].xyz == world space camera Up unit vector
                float3 cameraTransformForwardWS = -UNITY_MATRIX_V[2].xyz;//UNITY_MATRIX_V[2].xyz == -1 * world space camera Forward unit vector

            	 
				float3 randomAddToN = (0.5 * sin(pivotPosWS.x * 82.32523 + pivotPosWS.z)) * cameraTransformRightWS;//random normal per grass 
				//default grass's normal is pointing 100% upward in world space, it is an important but simple grass normal trick
                //-apply random to normal else lighting is too uniform
                //-apply cameraTransformForwardWS to normal because grass is billboard
                float3 N = normalize(half3(0,1,0) + randomAddToN - cameraTransformForwardWS*0.5);

            	//camera distance scale (make grass width larger if grass is far away to camera, to hide smaller than pixel size triangle flicker)        
                float3 viewWS = _WorldSpaceCameraPos - pivotPosWS;
                float ViewWSLength = length(viewWS);

            	float2 mapuv = MapUV(pivotPosWS);
            	
            	float4 landColor = tex2Dlod(_LandColorMap, float4(mapuv, 0, 0));
            	float4 tipColor = tex2Dlod(_GrassColorMap, float4(mapuv, 0, 0));

				// bledn with wind color
				float windBlendWeight = saturate(length(wind));
            	tipColor.xyz += _SunColor.xyz * _SunColor.w *  windBlendWeight;
            	
            	// blend with paint color
            	float4 paintColor = PaintColor(pivotPosWS);
            	float paintMask = tex2Dlod(_PaintDetailMap, float4(mapuv,0,0)).x; 
            	paintMask *= paintColor.w * _PaintWeight;

            	tipColor = lerp(tipColor,paintColor, paintMask);

            	

	            for (int i = 0 ; i <= BLADE_SEGMENTS; ++i)
                {
                    float t = i / (float)BLADE_SEGMENTS;

	            	float w = lerp(width, tipWidth, t);
	            	
                    float3 offset = float3(w, grassHeight * t, 0); // tangent space up is z-axis
	            	float offT = pow(t , 2.0f);
	            	float3 vpos = pivotPosWS + (wind*offT);
	            	//vpos = pivotPosWS;

	            	float4 grassColor = lerp(landColor, tipColor , t);

	            	// Expand Bilboard (billboad Left + right)
	            	float3 posOS = offset.x * cameraTransformRightWS;
	            	float3 posOS2 = -offset.x * cameraTransformRightWS;

	            	// Expand Bilboard (billboard up)
	            	float3 up = float3(0,1,0);
	            	posOS += offset.y *  up;
	            	posOS2 += offset.y * up;

	            	//posOS += cameraTransformRightWS * max(0, ViewWSLength * _ExpandRate); 
	            	//posOS2 += -cameraTransformRightWS * max(0, ViewWSLength * _ExpandRate); 

                    triStream.Append(make_g2_f(vpos,terrainNormal,N, posOS,  localMatrix, float2(0,t),grassColor));                    
                    triStream.Append(make_g2_f(vpos,terrainNormal,N, posOS2, localMatrix, float2(1,t), grassColor));                                       
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
				VertexPositionInputs vertexInput = (VertexPositionInputs)0;
            	vertexInput.positionWS = i.positionWS;

                // sample the texture
            	float4 col= i.color;

            	col.w = 1;
            	// apply fog

            	// shadow
            	float4 shadowCoord = GetShadowCoord(vertexInput);
            	float shadowAttenuation = saturate(MainLightRealtimeShadow(shadowCoord) + 0.25f);
				float4 shadowColor = lerp(0.5f, 1.0f, shadowAttenuation);

            	//return i.color;
            	return float4(col.xyz,1) * shadowColor ;
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
