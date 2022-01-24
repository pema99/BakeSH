Shader "Unlit/VizSH"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white"{}
        _L1Tex ("L1Tex", 2D) = "white"{}
        [ToggleUI] _Toggle("Toggle", Int) = 0
        _Rot("Rot", Vector) = (0, 0, 0)
        _Cube ("Cube", CUBE) = "white"{}
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
            float3 _Rot;
            samplerCUBE _Cube;

            float2x2 rot(float theta)
            {
                float s, c;
                sincos(theta, s, c);
                return float2x2(c, -s, s, c);
            }

            float sampleSHTex(float2 uv, float3 n)
            {
                float3 up = float3(0, 1, 0);
                up.yz = mul(rot(_Rot.x), up.yz);
                up.xz = mul(rot(_Rot.y), up.xz);
                up.xy = mul(rot(_Rot.z), up.xy);
                float4 sh = tex2D(_L1Tex, uv);
                return dot(sh.xyz, up);
                //float eval = sh.r * y10(n) + sh.g * y11(n) + sh.b * y12(n) + sh.a * y0();
               // return eval;
            }

            float4 frag(v2f i) : SV_Target
            {
                if (_Toggle) return texCUBElod(_Cube, float4(i.normal, 4));
                else return texCUBElod(_Cube, float4(tex2D(_L1Tex, i.uv).xyz, 4));
                float3 col = tex2D(_MainTex, TRANSFORM_TEX(i.uv, _MainTex));
                #ifdef UNITY_COLORSPACE_GAMMA
                col = GammaToLinearSpace(col);
                if (_Toggle) col *= (1 - sampleSHTex(i.uv, i.normal));
                return float4(LinearToGammaSpace(col), 1);
                #else
                if (_Toggle) col *= (1 - sampleSHTex(i.uv, i.normal));
                return float4(col, 1);
                #endif
            }
            ENDCG
        }
    }
}
