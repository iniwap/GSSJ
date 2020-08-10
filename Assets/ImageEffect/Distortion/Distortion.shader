﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Distortion" {

	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}

	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;
            float _PosX;
            float _PosY;
            float _Distortion;

			float length(float3 v)
			{
				return sqrt(dot(v,v));
			}

			float4 frag (v2f i) : SV_Target
			{
				float2 pos = i.uv;
				pos -= float2(_PosX,_PosY);

				float3 v3 = float3(pos.x, pos.y, 0);
				pos *= float2( pow( float2(length(v3), length(v3)), _Distortion) );
				pos += float2(_PosX, _PosY);
				return tex2D(_MainTex, pos);
			}
			ENDCG
		}
	}
}