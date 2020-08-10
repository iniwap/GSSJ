
Shader "Custom/FilterColor" { 
    Properties { 
        _MainTex ("Base (RGB)", 2D) = "white" {} 
        _FilterColor("Ridof (RGB)",Color)=(1,1,1,1) 
    } 
    SubShader { 
        Tags { "RenderType"="Opaque" }
        
        Cull Off  
        ZWrite Off
        
        Blend SrcAlpha OneMinusSrcAlpha
        pass 
        { 
            CGPROGRAM 

            #pragma vertex vertext_convert
            #pragma fragment fragment_convert
            #include "UnityCG.cginc" 

            sampler2D  _MainTex; 
            float4  _FilterColor; 
            struct Inputvrite 
            { 
                float4 vertex : POSITION; 
                float4 texcoord : TEXCOORD0; 
            }; 
            struct Inputfragment 
            { 
                float4 pos : SV_POSITION; 
                float4 uv : TEXCOORD0; 
            }; 

            float ColorLerp(float3 tmp_nowcolor,float3 tmp_FilterColor) 
            { 
                float3 dis = float3(abs(tmp_nowcolor.x - tmp_FilterColor.x),abs(tmp_nowcolor.y - tmp_FilterColor.y),abs(tmp_nowcolor.z - tmp_FilterColor.z)); 
                float dis0 =sqrt(pow(dis.x,2)+pow(dis.y,2)+pow(dis.z,2)); 
                float maxdis = sqrt(3); 
                float dis1 = lerp(0,maxdis,dis0); 
                return dis1; 
            } 

            Inputfragment vertext_convert(Inputvrite i) 
            { 
                Inputfragment o; 
                o.pos = UnityObjectToClipPos(i.vertex); 
                o.uv = float4(i.texcoord.xy,1,1); 
                return o; 
            } 

            float4 fragment_convert(Inputfragment o) : COLOR 
            { 
                float4 c = tex2D(_MainTex,o.uv); 
                c.a *=ColorLerp(c.rgb,_FilterColor.rgb); 
                return c; 
            } 


        ENDCG 
        } 
    } 
    FallBack "Diffuse" 
}