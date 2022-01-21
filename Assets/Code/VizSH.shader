Shader "Unlit/VizSH"
{
    Properties
    {
        [Enum(L0, 0, L1, 1, L2, 2)] _SHBand("SH Band", Int) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Common.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            int _SHBand;
            float4 _L1r;
            float4 _L1g;
            float4 _L1b;
            float4 _L2r;
            float4 _L2g;
            float4 _L2b;
            float4 _L2c;

            float3 sampleSHL0(float3 n)
            {
                return float3(
                    _L1r.w * y0(),
                    _L1g.w * y0(), 
                    _L1b.w * y0()
                );
            }

            float3 sampleSHL1(float3 n)
            {
                return float3(
                    _L1r.r * y10(n) + _L1r.g * y11(n) + _L1r.b * y12(n) + _L1r.w * y0(),
                    _L1g.r * y10(n) + _L1g.g * y11(n) + _L1g.b * y12(n) + _L1g.w * y0(),
                    _L1b.r * y10(n) + _L1b.g * y11(n) + _L1b.b * y12(n) + _L1b.w * y0()
                );
            }

            float3 sampleSHL2(float3 n)
            {
                float3 l1 = sampleSHL1(n);
                return l1 + float3(
                    _L2r.r * y20(n) + _L2r.g * y21(n) + _L2r.b * y22(n) + _L2r.w * y23(n) + _L2c.r * y24(n),
                    _L2g.r * y20(n) + _L2g.g * y21(n) + _L2g.b * y22(n) + _L2g.w * y23(n) + _L2c.g * y24(n),
                    _L2b.r * y20(n) + _L2b.g * y21(n) + _L2b.b * y22(n) + _L2b.w * y23(n) + _L2c.b * y24(n)
                );
            }

            fixed4 frag (v2f i) : SV_Target
            {
                switch (_SHBand)
                {
                    case 0: return float4(sampleSHL0(i.normal), 1);
                    case 1: return float4(sampleSHL1(i.normal), 1);
                    case 2: return float4(sampleSHL2(i.normal), 1);
                }
                return 0;
            }
            ENDCG
        }
    }
}
