#if !defined(MY_LIGHTING_INCLUDED)
#define MY_LIGHTING_INCLUDED

#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

float4 _Color;
float _Metallic;
float _Smoothness;
sampler2D _MainTex;
float4 _MainTex_ST;


struct i2v {
    float4 position : POSITION;
    float2 uv : TEXCOORD0;
    float3 normal : NORMAL;
};

struct v2f
{
    float4 position  : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 normal : TEXCOORD1;
    float3 worldPos : TEXCOORD2;
    #if defined(VERTEXLIGHT_ON)
        float3 vertexLightColor : TEXCOORD3;
    #endif
};

void ComputeVertexLightColor(inout v2f v)
{
    #if defined(VERTEXLIGHT_ON)
        v.vertexLightColor = Shade4PointLights(
            unity_4LightPosX0,unity_4LightPosY0,unity_4LightPosZ0,
            unity_LightColor[0].rgb,
            unity_LightColor[1].rgb,
            unity_LightColor[2].rgb,
            unity_LightColor[3].rgb,
            unity_4LightAtten0,
            v.worldPos,
            v.normal);
            
	#endif
}
UnityIndirect  CreateIndirectLight(v2f v)
{

    UnityIndirect indirectLight;
    indirectLight.diffuse = 0;
    indirectLight.specular = 0;
    #if defined(VERTEXLIGHT_ON)
        indirectLight.diffuse = v.vertexLightColor;
    #endif    
    #if defined(FORWARD_BASE_PASS)
        indirectLight.diffuse += max(0, ShadeSH9(float4(v.normal, 1)));
    #endif
    return indirectLight;
}
UnityLight CreateLight(v2f v)
{

    UnityLight light;
    #if defined(POINT) || defined(SPOT)
        light.dir = normalize(_WorldSpaceLightPos0.xyz - v.worldPos);
	#else
        light.dir = _WorldSpaceLightPos0.xyz;
	#endif
	UNITY_LIGHT_ATTENUATION(attenuation, 0, v.worldPos);
    light.color =  _LightColor0.rgb * attenuation;
    light.ndotl = DotClamped(v.normal, light.dir);
    return light;
}


v2f Vertex(i2v i)
{
    v2f v;
    v.position = UnityObjectToClipPos(i.position);
    v.uv = TRANSFORM_TEX(i.uv,_MainTex);
    v.normal = UnityObjectToWorldNormal(i.normal);
    v.worldPos = mul(unity_ObjectToWorld, i.position);
    ComputeVertexLightColor(v);
    return v;
}


float4 Fragment(v2f v) : SV_TARGET
{
    
    //Normalize normals, if we want
    //v.normal = normalize(v.normal);
    float3 viewDir = normalize(_WorldSpaceCameraPos - v.worldPos);

    float3 albedo = tex2D(_MainTex, v.uv).rgb * _Color.rgb;

    float3 specularTint;
    float oneMinusReflectivity;
    albedo = DiffuseAndSpecularFromMetallic(
        albedo, _Metallic, specularTint, oneMinusReflectivity
    );


    UnityLight light;


    return UNITY_BRDF_PBS(
        albedo, specularTint,
        oneMinusReflectivity, _Smoothness,
        v.normal, viewDir,
        CreateLight(v), CreateIndirectLight(v)
    );


    
}

#endif