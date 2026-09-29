Shader "Hidden/SwipeClean/StainUpdate"
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
            #pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _Stamp0[16];
            float4 _Stamp1[16];
            int _StampCount;

            fixed4 frag(v2f_img input) : SV_Target
            {
                float4 state = tex2D(_MainTex, input.uv);

                [loop]
                for (int index = 0; index < _StampCount; index++)
                {
                    float2 center = _Stamp0[index].xy;
                    float radius = max(_Stamp0[index].z, 0.0001);
                    float hardness = saturate(_Stamp0[index].w);
                    float distanceToStamp = distance(input.uv, center);
                    float innerRadius = radius * lerp(0.15, 0.9, hardness);
                    float influence = 1.0 - smoothstep(innerRadius, radius, distanceToStamp);
                    float strength = max(0.0, _Stamp1[index].z);
                    float deltaTime = clamp(_Stamp1[index].w, 0.0, 0.0666667);
                    state.r = max(0.0, state.r - strength * influence * deltaTime);
                    state.b = saturate(state.b + influence * 0.04);
                }

                return saturate(state);
            }
            ENDHLSL
        }
    }
}

