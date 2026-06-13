Shader "Custom/MonitorFlicker"
{
    Properties
    {
        _BaseMap ("Texture", 2D) = "white" {}
        _TintColor ("Tint Color", Color) = (1,1,1,1)
        _FlickerSpeed ("Flicker Speed", Float) = 8
        _Intensity ("Intensity", Float) = 0.25
        _EnableFlicker ("Enable Flicker", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            float4 _TintColor;
            float _FlickerSpeed;
            float _Intensity;
            float _EnableFlicker;

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
                half4 col =
                    SAMPLE_TEXTURE2D(
                        _BaseMap,
                        sampler_BaseMap,
                        IN.uv
                    );

                col *= _TintColor;

                float flicker =
                    1 +
                    sin(
                        _Time.y *
                        _FlickerSpeed
                    ) *
                    _Intensity;

                float finalValue =
                    lerp(
                        1,
                        flicker,
                        _EnableFlicker
                    );

                col.rgb *=
                    finalValue;

                return col;
            }
            ENDHLSL
        }
    }
}