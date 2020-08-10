// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Drunk" {

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
            float _Magnitude;
            float _Speed;

			float4 frag (v2f i) : SV_Target
			{
				float2 pos = i.uv;

				float sq = sin( _Time.y * _Speed) * _Magnitude;
                float ssq = sin(sq);
				float4 tc = tex2D(_MainTex, i.uv);
				float4 tl = tex2D(_MainTex, i.uv - float2(ssq,0.));
				float4 tR = tex2D(_MainTex, i.uv + float2(ssq,0.));
				float4 tD = tex2D(_MainTex, i.uv - float2(0.,ssq));
				float4 tU = tex2D(_MainTex, i.uv + float2(0.,ssq));

				return (tc + tl + tR + tD + tU) / 5.;
			}
			ENDCG
		}
	}
}