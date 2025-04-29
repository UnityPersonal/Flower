Shader "Custom/MyGrassFlower"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}
    	[NoScaleOffset] _FlowerColorMap("Flower Map", 2D) = "white" {}
    	[NoScaleOffset] _ForceMap("Force Map", 2D) = "white" {}
    	[NoScaleOffset] _GloryMap("Glory Map", 2D) = "white" {}
    	
    	_NoiseMap("Noise Map", 2D) = "white" {}
        
    	_GloryParticleSizeMin ("Glory Particle Size Min", Float) = 1
    	_GloryParticleSizeMax ("Glory Particle Size Max", Float) = 1
    	_GloryParticleColor ("Glory Particle Color", Color) = (1,1,1,1)
    	
    	_MapSize_Offset ("Map Size Offset", Vector) = (1,1,0,0)
    	
        _BladeHeightMin ("Blade Height", Range(0, 10)) = 0.5       
        _BladeHeightMax ("Blade Height", Range(0, 10)) = 0.5
    	
        _VisibleDistance ("Visible Distance", Float) = 100
    	
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

            uniform Texture2D _GlobalEffectRT;
			uniform float3 _Position;
			uniform float _OrthographicCamSize;
            uniform float _PaintOrthoSize;
            uniform float _ForceOrthoSize;
            uniform float _GloryOrthoSize;
			uniform float _InteractionDistance;
			SamplerState my_linear_clamp_sampler;

            CBUFFER_START(UnityPerMaterial)
                sampler2D _MainTex;
				sampler2D _FlowerColorMap;

				sampler2D _NoiseMap;
				float4 _NoiseMap_ST;
            
				float4 _MapSize_Offset;

				float _GloryParticleSizeMin;
				float _GloryParticleSizeMax;
				float4 _GloryParticleColor;
            
                float _BladeHeightMin;
                float _BladeHeightMax;

				float _VisibleDistance;
            
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
            	float4 color : TEXCOORD3;
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

            float2 MapUV(float3 posWS)
			{
			    float2 wp =  posWS.xz;
			    wp /= _MapSize_Offset.xy;
			    wp += _MapSize_Offset.zw;
			    return wp;
			}

            float4 Externalforce(float3 pos)
            {
	            float2 uv = pos.xz - _Position.xz;
			    uv  = uv  / (_ForceOrthoSize * 2);
			    uv  += 0.5;
            	if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
				{
					return float4(0,0,0,0);
				}
            	return tex2Dlod(_ForceMap,float4(uv,0,0));
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
            	float2 uv,
            	float4 color
            	)
			{
				g2f o;           	
				o.positionWS = pivotPosWS.xyz + mul(localMatrix,posOS);
				o.positionCS = TransformWorldToHClip(o.positionWS);
				o.uv = float4(uv,0,0);
            	o.color = color;
				return o;
			}

            float4 SampleGloryColor(float3 pos)
            {
            	float2 uv = pos.xz - _Position.xz;
			    uv  = uv  / (_GloryOrthoSize * 2);
			    uv  += 0.5;
            	if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
				{
					return float4(0,0,0,0);
				}
            	return tex2Dlod(_GloryMap, float4(uv, 0, 0));
            }

            float4 ExternalForceVectror(float3 pos)
            {
                float4 force = Externalforce(pos);
                if (force.w < 0.01)
                {
                    return float4(0,0,0,0);
                }
                // unpack force vector
                force.xyz -= 0.5;
                force.xyz *= 2;
            	float3 dir = float3(force.x,0, force.z);
                return force;
            }

            float4 GetInteractionData(in float3 pos)
            {
                float2 iuv = pos.xz - _Position.xz;
                iuv  = iuv  / (_OrthographicCamSize * 2);
                iuv  += 0.5;
                return _GlobalEffectRT.SampleLevel(my_linear_clamp_sampler, iuv, 0);
            }

            float4 GetInteractionVector(float3 pos)
            {
                float maxDistance = _InteractionDistance;
            	float3 dir =  _Position.xyz - pos;
                float distance = length(dir);
                dir = float3(dir.x,0, dir.z);
                dir = distance < 0.05f ? float3(0,1,0) : normalize(dir);

                float t = 1 - saturate(distance / maxDistance);
                float4 interaction = float4(dir, t);
                return interaction;
            }

            float3x3 GetTotalForceMatrix(float3 pos, float3 terrainNormal)
            {
                float4 external = ExternalForceVectror(pos);
                float4 interaction = GetInteractionVector(pos);

                float3 dir = normalize(external + interaction);
                float t = max(external.w, interaction.w);

                if (t <0.01)
                {
                    return identity3x3();
                }
                float3 terrainSlopeWeight = dot(float3(0,1,0), terrainNormal);
                t = pow(t,0.5f) * terrainSlopeWeight; 
                float angle =  lerp(0,60,t);

                
                float3 axis = normalize(cross( float3(0,1,0), dir));
                return angleAxis3x3(DegToRad(angle), -axis);
            }

			            
            [maxvertexcount(4)]
            void geom(triangle v2g input[3], inout TriangleStream<g2f> triStream)
            {
                float3 pivotPosWS = (input[0].positionWS + input[1].positionWS + input[2].positionWS) / 3.0f;
            	float3 terrainNormal = (input[0].normalWS + input[1].normalWS + input[2].normalWS) / 3.0f;

            	float r01 = rand01(pivotPosWS);
            	float grassHeight = lerp(_BladeHeightMin, _BladeHeightMax, r01);
            	
				float2 mapuv = MapUV(pivotPosWS);

            	float2 noiseUV = TRANSFORM_TEX(pivotPosWS.xz, _NoiseMap);
            	float noise = tex2Dlod(_NoiseMap, float4(noiseUV,0,0)).x;
            	/*if (noise < 0.5)
				{
					return;
				}*/
            	
                float3x3 localMatrix = GetTotalForceMatrix(pivotPosWS, terrainNormal);
            	
            	float3 wind = GetWindVector(pivotPosWS);
            	 
            	float3 terrainPivotPosWS = pivotPosWS + wind;
            	float3 gloryPivotPosOS = float3(0,grassHeight + 0.5,0);


            	float4 glory = SampleGloryColor(pivotPosWS);
				float3 diff = _Position - (gloryPivotPosOS + terrainPivotPosWS);
            	float distance = length(diff);
            	float maxDistance = _InteractionDistance;
            	float distanceWieght = 1 - saturate(distance / maxDistance);
            	
            	float visibleDistanceWeight = 1 - saturate(distance / _VisibleDistance);
				visibleDistanceWeight = pow(visibleDistanceWeight, 2);            	
            	distanceWieght = pow(distanceWieght, 3.f);

            	float sizeWeight = max(distanceWieght, glory.w);
            	float4 color = _GloryParticleColor;

            	float4 flowerColor = tex2Dlod(_FlowerColorMap, float4(mapuv,0,0));
            	sizeWeight = max(sizeWeight, flowerColor.w);
            	color.xyz = lerp(color.xyz, flowerColor.xyz, flowerColor.w);

            	if (sizeWeight < 0.01)
					return;


            	float dz[2] = {-0.5,0.5};
            	
            	float particleSize = lerp(_GloryParticleSizeMin, _GloryParticleSizeMax, r01);
            	particleSize *= distanceWieght;

            	// rotation make grass Lookat() camera just like a bilboard;
            	float3 right = UNITY_MATRIX_V[0].xyz;//UNITY_MATRIX_V[0].xyz == world space camera Right unit vector
                float3 up = UNITY_MATRIX_V[1].xyz;//UNITY_MATRIX_V[1].xyz == world space camera Up unit vector
                //float3 cameraTransformForwardWS = -UNITY_MATRIX_V[2].xyz;//UNITY_MATRIX_V[2].xyz == -1 * world space camera Forward unit vector


	            for (int i = 0 ; i <= 1; ++i)
                {
                    float t = i;
	            	
	            	float3 offset0 =  (right * -0.5) + (up*dz[i]);
	            	offset0 *= particleSize;
	            	float3 offset1 =  (right * 0.5) + (up*dz[i]);
	            	offset1 *= particleSize;

	            	float3 posOS = gloryPivotPosOS + offset0;
	            	float3 posOS2 = gloryPivotPosOS + offset1;

                    triStream.Append(make_g2_f(terrainPivotPosWS, posOS,  localMatrix, float2(0,t), color));                    
                    triStream.Append(make_g2_f(terrainPivotPosWS, posOS2, localMatrix, float2(1,t), color));                                       
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
            	
            	return i.color * mask.a;
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
