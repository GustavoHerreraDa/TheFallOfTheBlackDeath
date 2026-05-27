Shader "Custom/ServerElectricity"
{
    Properties
    {
        _GlowColor
        (
            "Glow Color",
            Color
        ) =
        (0.3,0.8,1,1)

        _PulseSpeed
        (
            "Pulse Speed",
            Float
        ) = 12
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _GlowColor;
            float _PulseSpeed;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(
                Attributes IN
            )
            {
                Varyings OUT;

                OUT.positionHCS =
                    TransformObjectToHClip(
                        IN.positionOS.xyz
                    );

                OUT.uv =
                    IN.uv;

                return OUT;
            }

            half4 frag(
                Varyings IN
            ) : SV_Target
            {
                float pulse =
                    abs(
                        sin(
                            _Time.y *
                            _PulseSpeed
                            +
                            IN.uv.x *
                            20
                        )
                    );

                return
                    _GlowColor
                    *
                    pulse;
            }

            ENDHLSL
        }
    }
}