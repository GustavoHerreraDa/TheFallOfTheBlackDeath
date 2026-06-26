Shader "Custom/FuturisticLaser"
{
    Properties
    {
        _Color ("Core Color", Color) = (1,0,0,1)
        _GlowColor ("Glow Color", Color) = (1,0.2,0.2,1)
        _Intensity ("Intensity", Range(0,20)) = 5
        _ScrollSpeed ("Scroll Speed", Float) = 8
        _PulseSpeed ("Pulse Speed", Float) = 6
        _Direction ("Direction", Vector) = (1,0,0,0)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Blend One One
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 localPos : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            float4 _Color;
            float4 _GlowColor;
            float _Intensity;
            float _ScrollSpeed;
            float _PulseSpeed;
            float4 _Direction;

            v2f vert(appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.localPos = v.vertex.xyz;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float pulse =
                    sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;

                float beam =
                    smoothstep(
                        0.4,
                        0.5,
                        1.0 - abs(i.uv.y - 0.5) * 2.0
                    );

                float3 dir =
                    normalize(_Direction.xyz);

                float projected =
                    dot(i.localPos, dir);

                float scan =
                    sin(
                        (projected - _Time.y * _ScrollSpeed) * 30.0
                    ) * 0.5 + 0.5;

                float glow =
                    beam * (scan * 0.5 + 0.5);

                float3 col =
                    _Color.rgb * beam +
                    _GlowColor.rgb *
                    glow *
                    pulse *
                    _Intensity;

                return float4(col, beam);
            }
            ENDCG
        }
    }
}