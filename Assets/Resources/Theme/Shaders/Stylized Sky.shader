// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Stylized/Sky"
{
	Properties
	{
        //用于来存储方位，shader里并不使用
        [Header(Pos)]_CameraRX("Camera RX", float) = -22
        _CameraRZ("Camera RZ", Float) = 0
        _LightRX("Light RX", Float) = 15
        _LightRY("Light RY", Float) = 168

		[Header(Sun Disc)]_SunDiscColor("Sun Disc Color", Color) = (1,1,1,1)
		_SunDiscMultiplier("Sun Disc Multiplier", Float) = 100000
		_SunDiscExponent("Sun Disc Exponent", Float) = 100000
		[Header(Sun Halo)]_SunHaloColor("Sun Halo Color", Color) = (1,0,0,1)
		_SunHaloExponent("Sun Halo Exponent", Float) = 500
		_SunHaloContribution("Sun Halo Contribution", Range( 0 , 1)) = 1
		[Header(Horizon Line)]_HorizonLineColor("Horizon Line Color", Color) = (0,0,0,0)
		_HorizonLineExponent("Horizon Line Exponent", Float) = 1
		_HorizonLineContribution("Horizon Line Contribution", Range( 0 , 1)) = 1
		[Header(Sky Gradient)]_SkyGradientTop("Sky Gradient Top", Color) = (0,0,0,0)
		_SkyGradientBottom("Sky Gradient Bottom", Color) = (0,0,0,0)
		_SkyGradientExponent("Sky Gradient Exponent", Float) = 1
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Background"  "Queue" = "Background+0" "IgnoreProjector" = "True" "ForceNoShadowCasting" = "True" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#include "UnityCG.cginc"
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha noshadow noinstancing noambient nolightmap  nodynlightmap nodirlightmap nofog nometa noforwardadd 
		struct Input
		{
			float3 worldPos;
		};

		uniform half _SunHaloExponent;
		uniform half4 _SunHaloColor;
		uniform half _SunHaloContribution;
		uniform half4 _SkyGradientTop;
		uniform half4 _SkyGradientBottom;
		uniform half _SkyGradientExponent;
		uniform half _HorizonLineExponent;
		uniform half4 _HorizonLineColor;
		uniform half _HorizonLineContribution;
		uniform half4 _SunDiscColor;
		uniform half _SunDiscMultiplier;
		uniform half _SunDiscExponent;

        //方位
        uniform half _CameraRX;
        uniform half _CameraRZ;
        uniform half _LightRX;
        uniform half _LightRY;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float3 ase_worldPos = i.worldPos;
			half3 ase_worldViewDir = Unity_SafeNormalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 normalizeResult31 = normalize( ase_worldViewDir );
			float3 ase_worldlightDir = Unity_SafeNormalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			float3 normalizeResult30 = normalize( -ase_worldlightDir );
			float dotResult35 = dot( normalizeResult31 , normalizeResult30 );
			float mask_sunDirection62 = dotResult35;
			float dotResult32 = dot( half3(0,-1,0) , normalizeResult31 );
			float mask_horizon58 = dotResult32;
			float4 color_sunHalo49 = ( saturate( ( pow( saturate( mask_sunDirection62 ) , ( saturate( abs( mask_horizon58 ) ) * _SunHaloExponent ) ) * ( 1.0 - saturate( pow( ( 1.0 - saturate( mask_horizon58 ) ) , 50.0 ) ) ) ) ) * _SunHaloColor * _SunHaloContribution );
			float4 lerpResult118 = lerp( _SkyGradientTop , _SkyGradientBottom , saturate( pow( ( 1.0 - saturate( mask_horizon58 ) ) , _SkyGradientExponent ) ));
			float4 color_skyGradient121 = lerpResult118;
			float3 lerpResult212 = lerp( float3( 0,0,0 ) , ( saturate( pow( ( 1.0 - abs( mask_horizon58 ) ) , _HorizonLineExponent ) ) * (_HorizonLineColor).rgb ) , _HorizonLineContribution);
			float3 color_horizonLine157 = lerpResult212;

			//float mask_sunDisc214 = saturate( ( _SunDiscMultiplier * saturate( pow( saturate( mask_sunDirection62 ) , _SunDiscExponent ) ) ) );
            float mask_sunDisc214 = saturate( ( _SunDiscMultiplier * saturate( pow( abs(dot(ase_worldViewDir, -ase_worldlightDir)) , _SunDiscExponent ) ) ) );

			float4 lerpResult245 = lerp( saturate( ( color_sunHalo49 + color_skyGradient121 + half4( color_horizonLine157 , 0.0 ) ) ) , _SunDiscColor , mask_sunDisc214);
			
            o.Emission = lerpResult245.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Skybox/Procedural"
	CustomEditor "ASEMaterialInspector"
}