// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/First Lit Shader" 
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex("Abedo", 2D) = "white" {}
		[Gamma] _Metallic ("Metallic", Range(0, 1)) = 0
        _Smoothness("Smoothness", Range(0,1)) = 0.5
    }

	SubShader 
	{
    
		Pass 
		{
		    Tags
		    {
		        "LightMode" = "ForwardBase"
		    }
		
		    CGPROGRAM
		    
		    #pragma vertex Vertex
		    #pragma fragment Fragment
		    
		    #pragma target 3.0
		    
			#define FORWARD_BASE_PASS

		    #include "FirstLitLighting.cginc"
		    
		    
		    ENDCG
    	}
		
		Pass 
		{
		    Tags
		    {
		        "LightMode" = "ForwardAdd"
		    }
			Blend One One
			ZWrite Off
		
		    CGPROGRAM
		    
		    #pragma vertex Vertex
		    #pragma fragment Fragment
			#pragma multi_compile _ VERTEXLIGHT_ON
			#pragma multi_compile DIRECTIONAL POINT SPOT
		    #pragma target 3.0
		    
		    #include "FirstLitLighting.cginc"
		    
		    
		    ENDCG
    	}
		
	}
}