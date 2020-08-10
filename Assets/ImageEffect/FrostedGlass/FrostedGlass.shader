﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/FrostedGlass" {

	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_IntensityX ("IntensityX", float) = 500.0
		_IntensityY ("IntensityY", float) = 500.0
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

			uniform sampler2D _MainTex;
			uniform float _IntensityX;
			uniform float _IntensityY;
            float _Radius;

			float rand(float2 co)
			{
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}

			float4 frag (v2f i) : SV_Target
			{
				float x = (i.uv.x * _IntensityX) + rand(i.uv) * _Radius * 2.0 - _Radius;
				float y = (i.uv.y * _IntensityY) + rand(float2(i.uv.y, i.uv.x)) * _Radius * 2.0 - _Radius;
 
				return tex2D(_MainTex, float2( x / _IntensityX, y / _IntensityY) );
			}
			ENDCG
		}
	}
}