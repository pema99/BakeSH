Shader "Hidden/BlitVertPos"
{
    SubShader
    {
        CGINCLUDE
        #include "UnityCG.cginc"
        #pragma vertex vert
        #pragma fragment frag

        struct v2f
        {
            float3 vpos : TEXCOORD0;
            float3 normal : TEXCOORD1;
            float4 vertex : SV_POSITION;
        };

        v2f vert(appdata_base v)
        {
            v2f o;
            o.vertex = float4(float2(1, -1) * ((v.texcoord) * 2 - 1), 0, 1);
            o.vpos = v.vertex;
            o.normal = normalize(v.normal);
            return o;
        }
        ENDCG

        Pass
        {
            CGPROGRAM
            float4 frag (v2f i) : SV_Target
            {
                return float4(i.vpos, 1);
            }
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            float4 frag(v2f i) : SV_Target
            {
                return float4(i.normal, 1);
            }
            ENDCG
        }
    }
}
