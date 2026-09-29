Shader "Hidden/SwipeClean/StainInitialize"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            float Hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float SoftCircle(float2 uv, float2 center, float radius, float feather)
            {
                return 1.0 - smoothstep(radius - feather, radius, distance(uv, center));
            }

            fixed4 frag(v2f_img input) : SV_Target
            {
                float2 uv = input.uv;
                float stain = 0.0;
                stain = max(stain, SoftCircle(uv, float2(0.34, 0.65), 0.23, 0.06));
                stain = max(stain, SoftCircle(uv, float2(0.67, 0.42), 0.27, 0.07));
                stain = max(stain, SoftCircle(uv, float2(0.49, 0.27), 0.14, 0.05));
                stain *= lerp(0.68, 1.0, Hash(floor(uv * 96.0)));
                return fixed4(saturate(stain), 0.0, 0.0, 0.0);
            }
            ENDHLSL
        }
    }
}

