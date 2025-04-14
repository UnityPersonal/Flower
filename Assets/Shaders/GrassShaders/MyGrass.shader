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
        _BladeHeight ("Blade Height", Range(0, 2)) = 0.5       
        
        _WindMap("Wind Offset Map", 2D) = "bump" {}        
        _WindStrength("Wind Strength", Range(0, 1)) = 0.5    	
        _WindFrequency("Wind Frequency", Range(0, 1)) = 0.01
    	_WindVelocity("Wind Velocity", Vector) = (1, 0, 0, 0)
    	
    	_GrassMap("Grass Color Range Map", 2D) = "grayscale" {}
        
        _TessMaxDistance("Tess Max Distance", Range(0,200)) = 100
        _TessAmount("Tess Amount", Range(0,20)) = 10
    	
    	_InteractionMap("InteractionMap",2D) = "black" {}  	
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
                float _BladeHeight;
                float _WindStrength;
                float _WindFrequency;
				float4 _WindVelocity;

                sampler2D _WindMap;
                float4 _WindMap_ST;

                float _TessMaxDistance;
                float _TessAmount;

				sampler2D _GrassMap;
				float4 _GrassMap_ST;

				sampler2D _InteractionMap;
				float4 _InteractionMap_ST;

            CBUFFER_END

            struct appdata
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct tessControlPoint
			{
				float4 positionWS : INTERNALTESSPOS;
				float3 normalWS : NORMAL;
				float4 tangentWS : TANGENT;
				float2 uv : TEXCOORD0;
			};

            struct v2g
            {
                float4 positionWS : SV_POSITION;
                float3 normalWS  : NORMAL;
                float4 tangentWS : TANGENT;
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

            tessControlPoint vert(appdata v)
            {
                tessControlPoint o;
                o.positionWS = float4(TransformObjectToWorld(v.positionOS), 1.0f);
				o.normalWS = TransformObjectToWorldNormal(v.normalOS);
				o.tangentWS = v.tangentOS;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex) ;
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

				return i;
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
            void geom(triangle v2g input[3], inout TriangleStream<g2f> triStream)
            {
                float3 pos = (input[0].positionWS + input[1].positionWS + input[2].positionWS) / 3.0f;

            	float3 normal = float3(0,1,0);
				float4 tangent = (input[0].tangentWS + input[1].tangentWS + input[2].tangentWS) / 3.0f;
				float3 bitangent = cross(normal, tangent.xyz) * tangent.w;

            	float3x3 tangentToLocal = float3x3
					(
						tangent.x,bitangent.x, normal.x,
						tangent.y,bitangent.y, normal.y,
						tangent.z,bitangent.z, normal.z
					);

            	float3x3 randRotMatrix = angleAxis3x3(rand01(pos) * UNITY_TWO_PI, float3(0, 0, 1));
                float3x3 baseTransfrmMatrix = mul(tangentToLocal, randRotMatrix);

            	float2 windUV = pos.xz * _WindMap_ST.xy + _WindMap_ST.zw + normalize(_WindVelocity.xz) * _WindFrequency * _Time.y;
            	float2 windSample = tex2Dlod(_WindMap, float4(windUV, 0, 0)).x * _WindVelocity * 2.0f;

                float width = _BladeWidth;

				float2 iuv = pos.xz - _Position.xz;
            	iuv  = iuv  / (_OrthographicCamSize * 2);
				iuv  += 0.5;
            	float bRipple = 1;
            	//bRipple = tex2D(_InteractionMap,float4(iuv,0,0)).b;
                float height = _BladeHeight * bRipple ;//

                for (int i = 0 ; i <= BLADE_SEGMENTS; ++i)
                {
                    float t = i / (float)BLADE_SEGMENTS;
                    float3 offset = float3(width,0, height * t); // tangent space up is z-axis
                    float3x3 transformMatrix = baseTransfrmMatrix;

                    triStream.Append(worldToClip(pos, float3(offset.x ,  offset.y, offset.z), transformMatrix, float2(0,t)));                    
                    triStream.Append(worldToClip(pos, float3(-offset.x,  offset.y, offset.z), transformMatrix, float2(1,t)));                                       
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
                // sample the texture
            	float t = smoothstep(0,1,i.uv.y);
            	float4 col = float4(1,1,1,1);
            	// apply fog

            	float2 grassUV = i.positionWS.xz * _GrassMap_ST.xy + _GrassMap_ST.zw;
            	float grassSample = tex2Dlod(_GrassMap, float4(grassUV, 0, 0)).x;

            	float r = grassSample;
            	float4 gcol = lerp(_GroundColor, _GroundColor1, r);
            	float4 tcol = lerp(_TipColor, _TipColor1, r);


            	
                return col * lerp(gcol, tcol, t);
            }
            ENDHLSL
        }		
    }
}
