Shader "Custom/MyFirstToonTriPlanar"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_RampTex ("Ramp", 2D) = "white" {}
		_Sharpness("Sharpness",Range(-10,100)) =1
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
		    
			#include "MyFirstToonLitTriPlanar.cginc"
			
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
		    
			#include "MyFirstToonLitTriPlanar.cginc"
			
			ENDCG
		}
	}
}
