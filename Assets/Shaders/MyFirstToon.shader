Shader "Custom/MyFirstToon"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_RampTex ("Ramp", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
		    Tags
		    {
		        "LightMode" = "ForwardBase"
		    }
			CGPROGRAM
			
			
		    #pragma target 3.0
                        
            #pragma vertex vert
            #pragma fragment frag
		    
			#include "MyFirstToonLit.cginc"
			
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
			
			
		    #pragma target 3.0
                        
            #pragma vertex vert
            #pragma fragment frag
			#pragma multi_compile _ VERTEXLIGHT_ON
			#pragma multi_compile DIRECTIONAL POINT SPOT
		    
			#include "MyFirstToonLit.cginc"
			
			ENDCG
		}
	}
}
