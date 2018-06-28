// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/First Shader" 
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex("Main Texture", 2D) = "white" {}
    }

	SubShader 
	{
    
		Pass 
		{
		    CGPROGRAM
		    
		    #pragma vertex Vertex
		    #pragma fragment Fragment
		    
		    #include "UnityCG.cginc"
		    
            float4 _Color;
            sampler2D _MainTex;
			float4 _MainTex_ST;

            
		    struct i2v {
                float4 position : POSITION;
                float2 uv : TEXCOORD0;
		    };
		    
		    struct v2f
		    {
		        float4 position  : SV_POSITION;
		        float2 uv : TEXCOORD0;
		    };
		    
		    
		    v2f Vertex(i2v i)
		    {
		        v2f v;
		        v.position = UnityObjectToClipPos(i.position);
		        v.uv = TRANSFORM_TEX(i.uv,_MainTex);
		        return v;
		    }
		    
		    float4 Fragment(v2f v) : SV_TARGET
		    {
		        return tex2D(_MainTex, v.uv) * _Color;
		    }
		    
		    ENDCG
    	}
		
	}
}