Shader "Hidden/SwipeClean/CoverageReduce"
{
    Properties
    {
        _MainTex ("State", 2D) = "black" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            fixed4 frag(v2f_img input) : SV_Target
            {
                float2 offset = _MainTex_TexelSize.xy * 0.5;
                float4 sum = tex2D(_MainTex, input.uv + float2(-offset.x, -offset.y));
                sum += tex2D(_MainTex, input.uv + float2(offset.x, -offset.y));
                sum += tex2D(_MainTex, input.uv + float2(-offset.x, offset.y));
                sum += tex2D(_MainTex, input.uv + float2(offset.x, offset.y));
                return sum * 0.25;
            }
            ENDHLSL
        }
    }
}

