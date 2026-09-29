Shader "Hidden/SwipeClean/SurfaceComposite"
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

            fixed4 frag(v2f_img input) : SV_Target
            {
                float2 uv = input.uv;
                float4 state = tex2D(_MainTex, uv);

                float vignette = 1.0 - 0.15 * dot(uv - 0.5, uv - 0.5);
                float3 baseColor = lerp(float3(0.72, 0.88, 0.91), float3(0.91, 0.96, 0.94), uv.y);
                baseColor *= vignette;

                float highlight = smoothstep(0.035, 0.0, abs(uv.x + uv.y * 0.35 - 0.56));
                baseColor += highlight * 0.08;

                float3 stainColor = lerp(float3(0.23, 0.11, 0.055), float3(0.48, 0.25, 0.08), uv.y);
                float stainAlpha = saturate(state.r * 0.88);
                float3 color = lerp(baseColor, stainColor, stainAlpha);
                color += state.b * 0.025;
                return fixed4(saturate(color), 1.0);
            }
            ENDHLSL
        }
    }
}

