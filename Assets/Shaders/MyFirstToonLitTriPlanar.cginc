#if !defined(MY_FIRST_TOON_LIT_INCLUDED)
#define MY_FIRST_TOON_LIT_INCLUDED



#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

struct appdata
{
    float4 position : POSITION;
    float3 normal : NORMAL;
//    float2 uv : TEXCOORD0;
};

struct v2f
{
    float4 position : SV_POSITION;
//    float2 worldNormal : TEXCOORD0;
    float3 normal : TEXCOORD0;
    float3 worldPos : TEXCOORD1;
};

float _Sharpness;
sampler2D _MainTex;
sampler2D _RampTex;
float4 _MainTex_ST;

v2f vert (appdata v)
{
    v2f o;
    o.position = UnityObjectToClipPos(v.position);
    //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    o.normal = UnityObjectToWorldNormal(v.normal);
    o.worldPos =  mul(unity_ObjectToWorld, v.position);   
    return o;
}


//	#ifndef USING_DIRECTIONAL_LIGHT
//	lightDir = normalize(lightDir);
//	#endif
//	
//	half d = dot (s.Normal, lightDir)*0.5 + 0.5;
//	half3 ramp = tex2D (_Ramp, float2(d,d)).rgb;
//	
//	half4 c;
//	c.rgb = s.Albedo * _LightColor0.rgb * ramp * (atten * 2);
//	c.a = 0;
//	return c;
			
	
fixed4 toonLighting(float3 normal, float3 lightDir, float atten)
{
    #if defined(SPOT) || defined(POINT)
        lightDir = normalize(lightDir);
    #endif
    
    float d = dot(normal, lightDir) * 0.5 + 0.5;
    float4 ramp = tex2D(_RampTex, float2(d,d));
    
    fixed4 c;
    c = _LightColor0 * ramp * (atten * 2); 
    return c;


   /* float3 normal = normalize(i.normal);
    
    #if defined(POINT) || defined(SPOT)
        float3 lightDir = normalize(_WorldSpaceLightPos0 - i.worldPos);
	#else
        float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
	#endif
    float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
	float3 halfDir = normalize(viewDir + lightDir);
	
    float u = DotClamped(normal, halfDir);
    float3 ramp = tex2D(_RampTex, float2(u,u));
    fixed4 c;
    c.rgb = ramp * _LightColor0.rgb;
    c.a = _LightColor0.a;
    return c;*/
}		
fixed4 toonLightingHelper(v2f v)
{
    UNITY_LIGHT_ATTENUATION(attenuation, 0, v.worldPos)
    float3 lightDir;
    #if defined(SPOT) || defined(POINT)
        lightDir = _WorldSpaceLightPos0.xyz - v.worldPos;   
    #else        
        lightDir = _WorldSpaceLightPos0.xyz;
    #endif
    return toonLighting(v.normal, lightDir, attenuation);
}

fixed4 frag (v2f i) : SV_Target
{
    // sample the texture
    fixed4 colY = tex2D(_MainTex, TRANSFORM_TEX(i.worldPos.xz,_MainTex));
    fixed4 colX = tex2D(_MainTex, TRANSFORM_TEX(i.worldPos.yz,_MainTex));
    fixed4 colZ = tex2D(_MainTex, TRANSFORM_TEX(i.worldPos.xy,_MainTex));
    fixed4 lit = toonLightingHelper(i);
    
    float3 blendWeights = pow(abs(i.normal), _Sharpness);
    blendWeights /= blendWeights.x + blendWeights.y + blendWeights.z;
    
    fixed4 col = colY * blendWeights.y + colX * blendWeights.x + colZ * blendWeights.z;
       
    return col * lit;
}

#endif