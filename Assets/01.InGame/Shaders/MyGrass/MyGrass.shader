Shader "Custom/MyGrass"
{
    Properties
    {
        _GroundColor ("Ground Color", Color) = (1, 1, 1, 1)
    	_GroundColor1 ("Ground Color1", Color) = (1, 1, 1, 1)
        _TipColor ("Tip Color", Color) = (1, 1, 1, 1)
    	_TipColor1 ("Tip Color1", Color) = (1, 1, 1, 1)
        _MainTex ("Texture", 2D) = "white" {}
        
        _BladeWidth ("Blade Width", Range(0, 1)) = 0.5
        _BladeHeightMin ("Blade Height", Range(0, 10)) = 0.5       
        _BladeHeightMax ("Blade Height", Range(0, 10)) = 0.5
    	
    	_InteractionDistance ("Interaction Distance", Range(0,20)) = 5
        
        _WindMap("Wind Offset Map", 2D) = "bump" {}        
        _WindFrequency("Wind Frequency", Range(0, 1)) = 0.01
    	_WindVelocity("Wind Velocity", Vector) = (1, 0, 0, 0)
    	
    	_SunMap("Sun Color Noise Map", 2D) = "grayscale" {}
    	_SunColor("Sun Color", Color) = (1,1,1,1)
        
        _TessMaxDistance("Tess Max Distance", Range(0,1000)) = 100
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

			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _SHADOWS_SOFT

            
            #pragma multi_compile_fog

            #define UNITY_PI 3.14159265359f
			#define UNITY_TWO_PI 6.28318530718f
            #define BLADE_SEGMENTS 8

            uniform float4 tipPallete = float4(0.34,0.99,0.18,1);
            uniform float4 tipPallete1 = float4(0.86,0.99,0.18,1);

            uniform Texture2D _GlobalEffectRT;
			uniform float3 _Position;
			uniform float _OrthographicCamSize;
			uniform float _HasRT;

            SamplerState my_linear_clamp_sampler;

            CBUFFER_START(UnityPerMaterial)
                float4 _GroundColor;
                float4 _GroundColor1;
                float4 _TipColor;
                float4 _TipColor1;
                sampler2D _MainTex;
                float4 _MainTex_ST;
            
                float _BladeWidth;
                float _BladeHeightMin;
                float _BladeHeightMax;

				float _InteractionDistance;
            
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
				float4 tangentWS : TANGENT;
            	float4 color : COLOR;
				float2 uv : TEXCOORD0;
            	float4 uv2 : TEXCOORD1;
			};

            struct v2g
            {
                float4 positionWS : SV_POSITION;
                float3 normalWS  : NORMAL;
                float4 tangentWS : TANGENT;
            	float4 color : COLOR;
                float2 uv : TEXCOORD0;
            	float4 uv2 : TEXCOORD1;
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
                float2 uv : TEXCOORD1;
            	float4 uv2 : TEXCOORD2;
            	float fogCoord : TEXCOORD3;
            	
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

            tessControlPoint vert(appdata v)
            {
                tessControlPoint o;
                o.positionWS = float4(TransformObjectToWorld(v.positionOS), 1.0f);
				o.normalWS = TransformObjectToWorldNormal(v.normalOS);
				o.tangentWS = v.tangentOS;
            	o.color = v.color;
                o.uv = v.uv;
            	o.uv2 = v.uv2;
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
				INTERPOLATE(tangentWS)
				INTERPOLATE(uv)
				INTERPOLATE(uv2)
				INTERPOLATE(color)
				return i;
			}

            float4 GetInteractionData(in float3 pos)
            {
	            float2 iuv = pos.xz - _Position.xz;
            	iuv  = iuv  / (_OrthographicCamSize * 2);
				iuv  += 0.5;
            	return _GlobalEffectRT.SampleLevel(my_linear_clamp_sampler, iuv, 0);
            }
            float3 GetSlopeVector(float3 pos)
            {
            	float4 data = GetInteractionData(pos);
            	if ( data.w <= 0.1)
            	{
            		return float3(0,0,0);
            	}
            	float3 dir = pos - data.xyz;
            	dir = normalize(float3(dir.x ,0 , dir.z));
            	return  dir;
            }

            float3x3 GetSlopeMatrix(float3 pos)
            {
            	float4 data = GetInteractionData(pos);

            	float maxDistance = _InteractionDistance;
            	float distance = maxDistance;
            	float3 dir = float3(0,0,1); 
            	if (data.w > 0.5)
            	{
            		distance = length(pos - data.xyz);
            		dir = pos - _Position.xyz;
            		dir = float3(dir.x,0, dir.z);
            		
            		dir = distance < 0.05f ? float3(0,0,1) : normalize(dir);          		
            	}
	            else
	            {
		            distance = length(pos - _Position.xyz);
            		dir = pos - _Position.xyz;
            		dir = float3(dir.x,0, dir.z);
            		
            		dir = distance < 0.05f ? float3(0,0,1) : normalize(dir);    
	            }
            	
				float3 axis = normalize(cross( dir.xzy, float3(0,0,1)));
				float t = 1 - saturate(distance / maxDistance);
            	t = pow(t,0.5f);
            	float angle =  lerp(0,90,t);
            	if (data.w > 0.5)
				{
					angle = 90;
				}
            	return angleAxis3x3(DegToRad(angle), axis);
            	return angleAxis3x3(0, float3(0,0,1));           	
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

            g2f worldToClip(
            	float3 pos,
            	float3 offset,
            	float3x3 transformationMatrix,
            	float2 uv,
            	float4 color,
            	float4 uv2)
			{
				g2f o;           	
				o.positionCS = TransformWorldToHClip(pos + mul(transformationMatrix, offset));
				o.positionWS = pos + mul(transformationMatrix, offset);
				o.uv = float4(uv,0,0);
            	o.uv2 = uv2;
            	o.color = color;
            	o.fogCoord = ComputeFogFactor(o.positionCS.z);

				return o;
			}
            
            void CreateVertexWithSlop(
            	inout TriangleStream<g2f> triStream,
            	float3 pos,
            	float width,
            	float height,
            	float3x3 m,
            	float4 color,
            	float4 uv2)
            {
            	float3 wind = GetWindVector(pos);

            	float tipWidth = width * 0.5f;
            	
	            for (int i = 0 ; i <= BLADE_SEGMENTS; ++i)
                {
                    float t = i / (float)BLADE_SEGMENTS;

	            	float w = lerp(width, tipWidth, t);
	            	
                    float3 offset = float3(w,0, height * t); // tangent space up is z-axis
	            	float offT = pow(t , 2.0f);
	            	float3 vpos = pos + (wind*offT);
	            	//float3 vpos = pos;

                    triStream.Append(worldToClip(vpos, float3(offset.x ,  offset.y, offset.z), m, float2(0,t), color,uv2));                    
                    triStream.Append(worldToClip(vpos, float3(-offset.x,  offset.y, offset.z), m, float2(1,t), color, uv2));                                       
                }

            }

            [maxvertexcount(3 * (BLADE_SEGMENTS+1))]
            void geom(triangle v2g input[3], inout TriangleStream<g2f> triStream)
            {
                float3 pos = (input[0].positionWS + input[1].positionWS + input[2].positionWS) / 3.0f;
            	float3 normal = (input[0].normalWS + input[1].normalWS + input[2].normalWS) / 3.0f;
				float4 tangent = (input[0].tangentWS + input[1].tangentWS + input[2].tangentWS) / 3.0f;
				float3 bitangent = cross(normal, tangent.xyz) * tangent.w;
            	
				float4 color = (input[0].color + input[1].color + input[2].color) / 3.0f;
				float4 uv2 = (input[0].uv2 + input[1].uv2 + input[2].uv2) / 3.0f;

            	float3x3 tangentToLocal = float3x3
					(
						tangent.x,bitangent.x, normal.x,
						tangent.y,bitangent.y, normal.y,
						tangent.z,bitangent.z, normal.z
					);

            	float3x3 randRotMatrix = angleAxis3x3(rand01(pos) * UNITY_TWO_PI, float3(0, 0, 1));


            	float3x3 externalRotMatrix = mul(GetSlopeMatrix(pos),randRotMatrix );
                float3x3 baseTransfrmMatrix = mul(tangentToLocal, externalRotMatrix);
                //float3x3 baseTransfrmMatrix = tangentToLocal ;

                float width = _BladeWidth;

            	float t = rand01(pos);
            	float height = lerp(_BladeHeightMin, _BladeHeightMax, t);

				CreateVertexWithSlop(triStream, pos, width, height, baseTransfrmMatrix, color,uv2);

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
            	float t = smoothstep(0,1,i.uv.y);
            	
            	float4 gcol = i.color;
            	float4 tcol = i.uv2;
            	
            	float4 col= lerp(gcol, tcol, t);

            	col.xyz += GetSunshineColor(i.positionWS);
            	col.w = 1;
            	
            	// apply fog
            	col.xyz = MixFog(col, i.fogCoord);

            	// shadow
            	float4 shadowCoord = GetShadowCoord(vertexInput);
            	float shadowAttenuation = saturate(MainLightRealtimeShadow(shadowCoord) + 0.25f);
				float4 shadowColor = lerp(0.5f, 1.0f, shadowAttenuation);

            	//return i.color;
            	return col;
            }
            ENDHLSL
        }		

		Pass
		{
			Name "ShadowCaster"
			Tags {"LightMode" = "ShadowCaster"}
			
			ZWrite On
			ZTest LEqual
			
			HLSLPROGRAM
			#pragma vertex shadowVert
			#pragma hull hull
			#pragma domain domain
			#pragma geometry geom
			#pragma fragment shadowFrag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

			float3 _LightDirection;
			float3 _LightPosition;

			tessControlPoint shadowVert(appdata v)
			{
				tessControlPoint o;

				o.normalWS = TransformObjectToWorldNormal(v.normalOS);
				o.tangentWS = v.tangentOS;
				o.uv = v.uv;
				o.color = v.color;
				o.uv2 = v.uv2;

				float3 positionWS = TransformObjectToWorld(v.positionOS);

				// Code required to account for shadow bias.
				float3 lightDirectionWS = _LightDirection;
				o.positionWS = float4(ApplyShadowBias(positionWS, o.normalWS, lightDirectionWS), 1.0f);

				return o;
			}

			float4 shadowFrag(g2f i) : SV_Target
			{
				//Alpha(SampleAlbedoAlpha(i.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a, float4(1,1,1,1), 0.5);
				return 0;
			}

			ENDHLSL
		}
    }
}
