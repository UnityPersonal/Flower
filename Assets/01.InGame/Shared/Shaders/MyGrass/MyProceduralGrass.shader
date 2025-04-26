Shader "Custom/MyProceduralGrass"
{
    Properties
    {
         _MainTex ("Texture", 2D) = "white" {}    	
    	[NoScaleOffset] _LandColorMap("Land Color Map", 2D) = "white" {}
    	[NoScaleOffset] _GrassColorMap("Grass Color Map", 2D) = "white" {}
        
    	_MapSize_Offset("Map Size Offset", Vector) = (1, 1, 0, 0)
    	
        _BladeWidth ("Blade Width", Range(0, 1)) = 0.5
        _BladeHeightMin ("Blade Height", Range(0, 10)) = 0.5       
        _BladeHeightMax ("Blade Height", Range(0, 10)) = 0.5
    	
        _WindMap("Wind Offset Map", 2D) = "bump" {}        
        _WindFrequency("Wind Frequency", Range(0, 1)) = 0.01
    	_WindVelocity("Wind Velocity", Vector) = (1, 0, 0, 0)
    	
        _SunMap("Sun Color Noise Map", 2D) = "grayscale" {}
    	_SunColor("Sun Color", Color) = (1,1,1,1)
        
    	_SkyLightPower("Sky Light Power", Float) = 1
    	
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

            #define UNITY_PI 3.14159265359f
			#define UNITY_TWO_PI 6.28318530718f
            #define BLADE_SEGMENTS 8

            CBUFFER_START(UnityPerMaterial)
                sampler2D _MainTex;
                float4 _MainTex_ST;

				sampler2D _LandColorMap;
				sampler2D _GrassColorMap;

				float4 _MapSize_Offset;
			
                float _BladeWidth;
                float _BladeHeightMin;
                float _BladeHeightMax;

                float _WindStrength;
                float _WindFrequency;
				float4 _WindVelocity;

                sampler2D _WindMap;
                float4 _WindMap_ST;

				sampler2D _SunMap;
				float4 _SunMap_ST;

				float4 _SunColor;
				float _SkyLightPower;
				sampler2D _PaintDetailMap;
				float _PaintWeight;
            CBUFFER_END

			StructuredBuffer<float3> _Positions;
			StructuredBuffer<float3> _Normals;
			StructuredBuffer<float2> _UVs;
			StructuredBuffer<float4x4> _TransformMatrices;

            struct appdata
            {
            	uint vertexID : SV_VertexID;
				uint instanceID : SV_InstanceID;
            };

            struct v2g
            {
                float4 positionWS : SV_POSITION;
                float3 normalWS  : NORMAL;
            };


            struct g2f
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            	float4 color : COLOR;
                float2 uv : TEXCOORD2;
            };

            

            v2g vert(appdata v)
            {
                v2g o = (v2g)0;

				float3 positionOS = float4(_Positions[v.instanceID], 1.0f);
				float3 normalOS = float4(_Normals[v.instanceID], 1.0f);
				float4x4 objectToWorld = _TransformMatrices[v.instanceID];
            	
                o.positionWS = mul(objectToWorld, positionOS);
				o.normalWS = mul(objectToWorld, normalOS);;
                return o;
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

            	float fogCoord = ComputeFogFactor(o.positionCS.z);
            	o.color.xyz = MixFog(o.color.xyz, fogCoord);

				return o;
			}

            [maxvertexcount(2 * (BLADE_SEGMENTS+1))]
            void geom(point v2g input[1], inout TriangleStream<g2f> triStream)
            {
            	float3 pivotPosWS = input[0].positionWS;
            	float3 terrainNormal = input[0].normalWS;

            	float3x3 localMatrix = ExternalForceMatrix(pivotPosWS, terrainNormal);
				float r01 = rand01(pivotPosWS);
            	float grassHeight = lerp(_BladeHeightMin, _BladeHeightMax, r01);
            	float width = _BladeWidth;
            	float tipWidth = width * 0.5f;

				float3 wind = float3(0,0,0);
            	
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

            	float2 mapuv = MapUV(pivotPosWS,_MapSize_Offset);
            	
            	float4 landColor = tex2Dlod(_LandColorMap, float4(mapuv, 0, 0));
            	float4 tipColor = tex2Dlod(_GrassColorMap, float4(mapuv, 0, 0));

				// bledn with wind color
				float windBlendWeight = saturate(length(wind));
            	tipColor.xyz += _SunColor.xyz * _SunColor.w *  windBlendWeight;
            	
            	// blend with paint color
            	float4 paintColor = SamplePaintColor(mapuv);
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

            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            

            float4 frag (g2f i) : SV_Target
            {
                // sample the texture
            	float4 col = float4(i.color.xyz,1);
                // apply fog
                return col;
            }
            ENDHLSL
        }
    }
}
