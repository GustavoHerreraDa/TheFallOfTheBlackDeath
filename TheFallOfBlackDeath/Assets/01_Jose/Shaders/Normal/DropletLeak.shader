Shader "Custom/WaterDrip"
{
    Properties
    {
        [MainTexture] _MainTex ("Main Texture", 2D) = "white" {}

        [Header(Color)]
        _BaseColor ("Base Color", Color) = (0.7,0.8,1,0.5)

        [Header(Fresnel)]
        _FresnelPower ("Fresnel Power", Range(0.01,20)) = 4
        _FresnelIntensity ("Fresnel Intensity", Range(0,5)) = 0.6
        _FresnelAlpha ("Fresnel Alpha", Range(0,5)) = 0.2

        [Header(Transparency)]
        _AlphaMultiplier ("Alpha Multiplier", Range(0,5)) = 1

        [Header(Specular)]
        _Smoothness ("Smoothness", Range(0,1)) = 1

        [Header(Rendering)]
        [Toggle] _ZWrite ("Z Write", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite [_ZWrite]
        Cull Back

        Pass
        {
            Name "Forward"

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)

            float4 _BaseColor;
            float4 _MainTex_ST;

            float _FresnelPower;
            float _FresnelIntensity;
            float _FresnelAlpha;

            float _AlphaMultiplier;
            float _Smoothness;

            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                VertexPositionInputs positionInputs =
                    GetVertexPositionInputs(IN.positionOS.xyz);

                VertexNormalInputs normalInputs =
                    GetVertexNormalInputs(IN.normalOS);

                OUT.positionCS = positionInputs.positionCS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);

                OUT.normalWS =
                    normalize(normalInputs.normalWS);

                OUT.viewDirWS =
                    normalize(
                        GetWorldSpaceViewDir(
                            positionInputs.positionWS
                        )
                    );

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float4 tex =
                    SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        IN.uv
                    );

                float fresnel =
                    pow(
                        1.0 -
                        saturate(
                            dot(
                                normalize(IN.normalWS),
                                normalize(IN.viewDirWS)
                            )
                        ),
                        _FresnelPower
                    );

                float3 finalColor =
                    lerp(
                        _BaseColor.rgb,
                        float3(1,1,1),
                        fresnel * _FresnelIntensity
                    );

                float finalAlpha =
                    (_BaseColor.a * tex.a * _AlphaMultiplier) +
                    (fresnel * _FresnelAlpha);

                return float4(finalColor, saturate(finalAlpha));
            }

            ENDHLSL
        }
    }
}