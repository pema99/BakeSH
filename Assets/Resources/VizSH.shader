Shader "Unlit/VizSH"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white"{}
        _L1Tex ("L1Tex", 2D) = "white"{}
        [ToggleUI] _Toggle ("Toggle", Int) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode"="ForwardBase" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Common.cginc"
            #include "SHTools.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 sh : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = (v.normal);
                o.uv = v.uv;
                return o;
            }

            float4 _MainTex_ST;
            sampler2D _MainTex;
            sampler2D _L1Tex;
            int _Toggle;

            float sampleSHTex(float2 uv, float3 n)
            {
                float4 sh = tex2D(_L1Tex, uv);
                return sh.r * y10(n) + sh.g * y11(n) + sh.b * y12(n) + sh.a * y0();
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 col = tex2D(_MainTex, TRANSFORM_TEX(i.uv, _MainTex));

                float3 res = col;
                if (_Toggle) res *= (1 - sampleSHTex(i.uv, i.normal));
                return float4(res, 1);
            }
            ENDCG
        }
    }
}
