// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "Custom/Frost" {
	Properties {
		_Opacity ("Opacity", Range(0.1,1)) = 0.5
		_MainTex ("Texture", 2D) = "white" {}
	}

	SubShader{
	 	Tags { "Queue"="Transparent" "RenderType"="Transparent" } Blend SrcAlpha OneMinusSrcAlpha
		GrabPass{"_GrabTex"}
		Pass{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct v2f{
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 uvgrab : TEXCOORD1;
			};

			float _Opacity;

			sampler2D _MainTex;
			sampler2D _GrabTex;

			//Vertex
			v2f vert(appdata_base IN){
				v2f OUT;
				OUT.position = UnityObjectToClipPos(IN.vertex);
				OUT.uv = IN.texcoord;
				OUT.uvgrab = ComputeGrabScreenPos(OUT.position);

				return OUT;
			}

			half4 blur_grab(float4 proj, float blur){
				half4 col = tex2Dproj(_GrabTex, proj );
				col+= tex2Dproj(_GrabTex, proj + float4(0, -blur, 0,0));
				col+= tex2Dproj(_GrabTex, proj + float4(0, blur, 0,0));
				col+= tex2Dproj(_GrabTex, proj + float4(-blur, 0, 0,0));
				col+= tex2Dproj(_GrabTex, proj + float4(blur,0, 0,0));

				col+= tex2Dproj(_GrabTex, proj + float4(blur,blur, 0,0));
				col+= tex2Dproj(_GrabTex, proj + float4(blur,-blur, 0,0));
				col+= tex2Dproj(_GrabTex, proj + float4(-blur,blur, 0,0));
				col+= tex2Dproj(_GrabTex, proj + float4(-blur,-blur, 0,0));

				return col/9;
			}

			//Fragment
			fixed4 frag(v2f IN) : SV_Target{
				half4 col = tex2D(_MainTex, IN.uv);
      
				half4 grab = blur_grab(IN.uvgrab, 0.01);

				half4 l = lerp(grab, col, _Opacity);
				col.rgb = l * grab.rgb*1.5;

				return col;
			}

			ENDCG
		}
	}

}

