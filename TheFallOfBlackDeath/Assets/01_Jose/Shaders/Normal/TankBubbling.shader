Shader "Custom/TankBubbling"
{
    Properties
    {
        _Tint ("Tint", Color) =
        (0,0.8,1,0.35)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Transparent"
        }

        Blend
            SrcAlpha
            OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _Tint;

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
                float n =
                    frac(
                        sin(
                            dot(
                                IN.uv *
                                _Time.y,
                                float2(
                                    12.98,
                                    78.23
                                )
                            )
                        )
                        * 43758
                    );

                float bubble =
                    step(
                        0.97,
                        n
                    );

                return
                    half4(
                        _Tint.rgb
                        + bubble,
                        _Tint.a
                    );
            }

            ENDHLSL
        }
    }
}