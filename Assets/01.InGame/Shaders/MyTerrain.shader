// MADE BY MATTHIEU HOULLIER
// Copyright 2021 BRUTE FORCE, all rights reserved.
// You are authorized to use this work if you have purchased the asset.
// Mail me at bruteforcegamesstudio@gmail.com if you have any questions or improvements you want.
Shader "Custom/MyTerrain"
{
	Properties
	{
		[Header(Terrain)]
		[Space]
		_MapSize_Offset("Map Size Offset", Vector) = (0,0,0,0)
		
		[Header(Tint Colors)]
		[Space]
		[MainColor]_Color("ColorTint",Color) = (0.5 ,0.5 ,0.5,1.0)
		_GroundColor("GroundColorTint",Color) = (0.7 ,0.68 ,0.68,1.0)
		_SelfShadowColor("ShadowColor",Color) = (0.41 ,0.41 ,0.36,1.0)
		_ProjectedShadowColor("ProjectedShadowColor",Color) = (0.45 ,0.42 ,0.04,1.0)
		_GrassShading("GrassShading", Range(0.0, 1)) = 0.197
		_GrassSaturation("GrassSaturation", Float) = 2

		[Header(Textures)]
		[Space]
		[MainTexture]_MainTex("Color Grass", 2D) = "white" {}
		[NoScaleOffset]_GroundTex("Ground Texture", 2D) = "white" {}
		[NoScaleOffset]_NoGrassTex("NoGrassTexture", 2D) = "white" {}
		[NoScaleOffset]_GrassTex("Grass Pattern", 2D) = "white" {}
		[NoScaleOffset]_Noise("NoiseColor", 2D) = "white" {}
		[NoScaleOffset]_Distortion("DistortionWind", 2D) = "white" {}
		
		
		
		[Header(Geometry Values)]
		[Space]
		_FadeDistanceStart("FadeDistanceStart", Float) = 16
		_FadeDistanceEnd("FadeDistanceEnd", Float) = 26
		_BladeHeight("BladeHeight", Range(0, 10)) = 1
		_BladeWidth("BladeWidth", Range(0, 17)) = 1
		_TessAmount("TessAmount", Range(0, 20)) = 10
		_TessMaxDistance("TessMaxDistance", Range(0, 1000)) = 100

		[Header(Rim Lighting)]
		[Space]
		_RimColor("Rim Color", Color) = (0.14, 0.18, 0.09, 1)
		_RimPower("Rim Power", Range(0.0, 8.0)) = 3.14
		_RimMin("Rim Min", Range(0,1)) = 0.241
		_RimMax("Rim Max", Range(0,1)) = 0.62

		[Header(Grass Values)]
		[Space]
		_GrassThinness("GrassThinness", Range(0.01, 3)) = 0.4
		_GrassThinnessIntersection("GrassThinnessIntersection", Range(0.01, 2)) = 0.43
		_TilingN1("TilingOfGrass", Float) = 6.06
		_WindMovement("WindMovementSpeed", Float) = 0.55
		_WindForce("WindForce", Float) = 0.35
		_TilingN3("WindNoiseTiling", Float) = 1
		_GrassCut("GrassCut", Range(0, 1)) = 0
		_TilingN2("TilingOfNoiseColor", Float) = 0.05
		_NoisePower("NoisePower", Float) = 2
		[Toggle(USE_RT)] _UseRT("Use RenderTexture Effect", Float) = 1
		[Toggle(USE_PD)] _UsePreciseDepth("Use Precise Depth Pass", Float) = 0

		[Header(Lighting Parameters)]
		[Space]
		_LightIntensity("Additional Lights Intensity", Range(0.00, 2)) = 1
		[Toggle(USE_AL)] _UseAmbientLight("Use Ambient Light", Float) = 0

	}
	
	SubShader
	{
		Tags
		{			
			"DisableBatching" = "true"			
		}
		pass
		{
		Tags
		{
			"RenderPipeline" = "UniversalPipeline"
		}
		LOD 100
		HLSLPROGRAM

		#pragma require geometry
		#pragma require tessellation tessHW

		#pragma vertex vert
		#pragma hull hull
		#pragma domain domain
		#pragma fragment frag
		#pragma geometry geom
		
		#pragma multi_compile_fog
		#pragma multi_compile_instancing
		#pragma prefer_hlslcc gles
		#pragma shader_feature USE_BP
		#pragma shader_feature USE_AL
		#pragma shader_feature USE_BMP

		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

		#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
		#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
		#pragma multi_compile _ _SHADOWS_SOFT
		#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
		#pragma multi_compile _ LIGHTMAP_ON

		struct appdata
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
			float3 normal : NORMAL;
#ifdef LIGHTMAP_ON
				half4 texcoord1 : TEXCOORD1;
#endif
			float2 uv_Control: TEXCOORD2;
		};

		struct tessControlPoint
		{
			float4 positionWS : INTERNALTESSPOS;
			float3 normal : NORMAL;
			float2 uv : TEXCOORD0;
		};
		
		struct tessFactors
		{
			float edge[3] : SV_TessFactor;
			float inside  : SV_InsideTessFactor;
		};

		struct v2g
        {
            float4 positionWS : SV_POSITION;
			float3 normal : NORMAL;
			float2 uv : TEXCOORD0;
        };
		
		struct g2f
		{
			float4 posCS : SV_POSITION;
			float3 normal : NORMAL;
			float3 posWS : TEXCOORD0;
			float2 uv : TEXCOORD1;
		};
		
		//uniform sampler2D _Control0;
		Texture2D _Control0;
		Texture2D _Control1;
		sampler2D _TerrainHolesTexture;

		int _NumberOfStacks, _RTEffect, _MinimumNumberStacks, _UseBiplanar;
		float _BladeHeight , _BladeWidth;
		float _TessAmount;
		float _TessMaxDistance;
		float4 _MapSize_Offset;
		
		Texture2D _MainTex;
		Texture2D _GroundTex;
		Texture2D _NoGrassTex;
		float4 _MainTex_ST;
		Texture2D _Distortion;
		Texture2D _Noise;
		float _TilingN1;
		float _TilingN2, _WindForce;
		float4 _Color, _SelfShadowColor, _GroundColor, _ProjectedShadowColor;
		float4 _OffsetVector;
		float _TilingN3, _BiPlanarStrength, _BiPlanarSize;
		float _WindMovement, _OffsetValue;
		half _GrassThinness, _GrassShading, _GrassThinnessIntersection, _GrassCut;
		half4 _RimColor;
		half _RimPower, _NoisePower, _GrassSaturation, _FadeDistanceStart, _FadeDistanceEnd;
		half _RimMin, _RimMax;
		half4 _Specular0, _Specular1, _Specular2, _Specular3, _Specular4, _Specular5, _Specular6, _Specular7;
		SamplerState my_linear_repeat_sampler;
		SamplerState my_trilinear_repeat_sampler;
		SamplerState my_linear_clamp_sampler;

		half _LightIntensity;

		tessControlPoint vert(appdata v)
		{
			tessControlPoint o;
			VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
			o.positionWS = float4(vertexInput.positionWS.xyz,1);
			o.normal = TransformObjectToWorldNormal(v.normal);
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

			INTERPOLATE(positionWS);
			INTERPOLATE(normal);
			INTERPOLATE(uv);
			return i;
		}


		#define BLADE_SEGMENTS (8)
		#define UnityObjectToWorld(o) mul(unity_ObjectToWorld, float4(o.xyz,1.0))
		[maxvertexcount((BLADE_SEGMENTS+1) * 8)]
		void geom(triangle v2g input[3], inout TriangleStream<g2f> tristream)
		{
			float3 pos = (input[0].positionWS +  input[1].positionWS +  input[2].positionWS)  / 3;
			float3 normal = (input[0].normal +  input[1].normal +  input[2].normal)  / 3;
			float2 uv = (input[0].uv +  input[1].uv +  input[2].uv)  / 3;

			
			g2f o;
			for (int i = 0; i < 3;i++)
			{
				o.posCS = TransformWorldToHClip(input[i].positionWS);
				o.normal = input[i].normal;
				o.posWS = input[i].positionWS;
				o.uv = input[i].uv;
				tristream.Append(o);
			}
			tristream.RestartStrip();	
			return;

			float h = _BladeHeight;
			float w = _BladeWidth;

			for (int i = 0 ; i < BLADE_SEGMENTS; i++)
			{
				float t = i / (float)BLADE_SEGMENTS;
				float3 offset  = float3(w,h*t,0);
				
				o.posCS = TransformWorldToHClip(pos + offset);
				o.normal = normal;
				o.posWS = pos;
				o.uv = uv;
				tristream.Append(o);

				offset  = float3(-w,h*t,0);				
				o.posCS = TransformWorldToHClip(pos + offset);
				o.normal = normal;
				o.uv = uv;
				tristream.Append(o);
			}

			tristream.RestartStrip();			
		}

		half4 frag(g2f i) : SV_Target
		{
			Light light = GetMainLight();
			float3 lightDir = light.direction;
			float ndotl = saturate( dot(lightDir , normalize(i.normal)) );
			float diffuse = ndotl * 0.5 + 0.5f;


			
			return _GroundTex.Sample(my_linear_clamp_sampler, float4(i.uv,0,0)) * ndotl;
			return _GroundTex.Sample(my_linear_clamp_sampler, float4(i.uv,0,0));
		}
		
		ENDHLSL
	}

	
	}// Fallback "VertexLit"
}

