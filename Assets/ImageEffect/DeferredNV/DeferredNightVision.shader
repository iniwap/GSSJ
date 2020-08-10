// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

///A night vision device (NVD) is an optoelectronic device that allows 
///images to be produced in levels of light approaching total darkness. 
///The image may be a conversion to visible light of both visible light and near-infrared, 
///while by convention detection of thermal infrared is denoted thermal imaging. 
///The image produced is typically monochrome, e.g. shades of green. 
///NVDs are most often used by the military and law enforcement agencies, 
///Many NVDs also include optical components such as a sacrificial lens,[1] or telescopic lenses or mirrors. 
///An NVD may have an IR illuminator, making it an active as opposed to passive night vision device.
///

Shader "Custom/DeferredNightVision" {

	Properties {
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_NVColor ("NV Color", Color) = (0,1,0.1724138,0)
		_LightSensitivityMultiplier ("SensitivityMultiplier", Range(0,128)) = 90
        _Scope("Scope", Range(0.05,1)) = 0.5
	}
	
SubShader {
	Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="Transparent"}

	Cull Off Lighting Off ZWrite Off

	Pass {  
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"
			#pragma multi_compile _ USE_VIGNETTE

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
			};

			sampler2D _MainTex;
			sampler2D _CameraGBufferTexture0;
			//NOTE: If you're super keen to optimize, change all these floats to fixeds.
			float4 _MainTex_ST;
			float4 _NVColor;
			float4 _TargetWhiteColor;
			float _BaseLightingContribution;
			float _LightSensitivityMultiplier;
            float _Scope;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}
			
			//So, we want to achieve a few things here:
			fixed4 frag (v2f i) : SV_Target
			{
				//fixed4 is cheapest, half4 second, float4 expensive (but really doesn't matter here)
				float4 col = tex2D(_MainTex, i.texcoord);
				float4 dfse = tex2D(_CameraGBufferTexture0, i.texcoord);			
				
				//Get the luminance of the pixel				
				float lumc = Luminance (col.rgb);
				
				//Desat + green the image
				col = dot(col, _NVColor);	
				
				//Make bright areas/lights too bright
				col.rgb = lerp(col.rgb, _TargetWhiteColor, lumc * _LightSensitivityMultiplier);
				
				//Add some of the regular diffuse texture based off how bright each pixel is
				col.rgb = lerp(col.rgb, dfse.rgb, lumc+_BaseLightingContribution);
				
				#if USE_VIGNETTE
				//Add vignette
				float dist = distance(i.texcoord, float2(0.5,0.5));
				col *= smoothstep(_Scope,_Scope - 0.01,dist);
				#endif				
				
				//Increase the brightness of all normal areas by a certain amount
				col.rb = max (col.r - 0.75, 0)*4;
				
				return col;
			}
		ENDCG
	}
	}
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	FallBack "Diffuse"
}
