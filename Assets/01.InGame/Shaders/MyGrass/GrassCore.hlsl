#ifndef GRASS_CORE_INCLUDED
#define GRASS_CORE_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"


uniform Texture2D _GlobalEffectRT;
uniform Texture2D _GlobalPaintRT;
uniform Texture2D _GlobalForceRT;
uniform Texture2D _GlobalGloryRT;
uniform float3 _Position;
uniform float _OrthographicCamSize;
uniform float _HasRT;
uniform float4 _MapSize_Offset;
uniform float _InteractionDistance;

SamplerState my_linear_clamp_sampler;

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
    wp += _MapSize_Offset.zw;
    wp /= _MapSize_Offset.xy;
    return wp;
}

float4 GetInteractionData(in float3 pos)
{
    float2 iuv = pos.xz - _Position.xz;
    iuv  = iuv  / (_OrthographicCamSize * 2);
    iuv  += 0.5;
    return _GlobalEffectRT.SampleLevel(my_linear_clamp_sampler, iuv, 0);
}

float4 PaintColor(float3 posWS)
{
    return _GlobalPaintRT.SampleLevel(my_linear_clamp_sampler, MapUV(posWS), 0);
}

float4 GloryColor(float3 posWS)
{
    return _GlobalGloryRT.SampleLevel(my_linear_clamp_sampler, MapUV(posWS), 0);
}

float3x3 ExternalForceMatrix(float3 posWS, float3 terrainNormal)
{
    float2 uv =  MapUV(posWS);

    float4 force = _GlobalForceRT.SampleLevel(my_linear_clamp_sampler,uv, 0);

    if (force.w < 0.01)
    {
        return identity3x3();
    }

    // unpack force vector
    force.xyz -= 0.5;
    force.xyz *= 2;            	
    float3 dir = -force.xyz;
    //dir = float3(1,0,0);
    
    float3 axis = normalize(cross( dir.xyz, float3(0,1,0)));
    float angle =  lerp(0,70,force.w);

    return angleAxis3x3(DegToRad(angle), axis);            	
}

float3x3 GetSlopeMatrix(float3 pos, float3 terrainNormal)
{
    float4 data = GetInteractionData(pos);

    float maxDistance = _InteractionDistance;
    float distance = maxDistance;
    float3 dir = float3(0,0,1); 
    /*if (data.w > 0.5)
    {
        distance = length(pos - data.xyz);
        dir = pos - _Position.xyz;
        dir = float3(dir.x,0, dir.z);
        
        dir = distance < 0.05f ? float3(0,1,0) : normalize(dir);          		
    }
    else*/
    {
        distance = length(pos - _Position.xyz);
        dir = _Position.xyz - pos;
        dir = float3(dir.x,0, dir.z);
        
        dir = distance < 0.05f ? float3(0,1,0) : normalize(dir);    
    }
    
    float3 axis = normalize(cross( -dir.xyz, float3(0,1,0)));
    axis = float3(1,0,0);

    float3 terrainSlopeWeight = dot(float3(0,1,0), terrainNormal);
    float t = 1 - saturate(distance / maxDistance);
    t = pow(t,0.5f) * terrainSlopeWeight; 
    float angle =  lerp(5,90,t);
    return angleAxis3x3(DegToRad(angle), axis);
}





#endif // GRASS_CORE_INCLUDED