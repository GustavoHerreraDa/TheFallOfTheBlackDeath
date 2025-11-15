Shader "Hidden/URP/DitherEffect"
{
    Properties
    {
        _MainTex("MainTex", 2D) = "white" {}
        _NoiseTex("Noise Texture", 2D) = "white" {}
        _ColorRampTex("Color Ramp", 2D) = "white" {}
        _NoiseScale("Noise Scale", Float) = 512
        _XOffset("X Offset", Float) = 0
        _YOffset("Y Offset", Float) = 0
    }

        SubShader
        {
            Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" }
            Cull Off ZWrite Off ZTest Always

            Pass
            {
                Name "DitherPass"

                Stencil
                {
                    Ref 1
                    Comp NotEqual
                    Pass Keep
                }

                HLSLPROGRAM
                #pragma vertex Vert
                #pragma fragment Frag
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

                TEXTURE2D(_MainTex);
                SAMPLER(sampler_MainTex);

                TEXTURE2D(_NoiseTex);
                SAMPLER(sampler_NoiseTex);

                TEXTURE2D(_ColorRampTex);
                SAMPLER(sampler_ColorRampTex);

                float _NoiseScale;
                float _XOffset;
                float _YOffset;

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

                Varyings Vert(Attributes v)
                {
                    Varyings o;
                    o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                    o.uv = v.uv;
                    return o;
                }

                float4 Frag(Varyings i) : SV_Target
                {
                    float3 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv).rgb;
                    float lum = dot(col, float3(0.299, 0.587, 0.114));

                    float2 noiseUV = frac(i.uv * _NoiseScale + float2(_XOffset, _YOffset));
                    float3 threshold = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, noiseUV).rgb;
                    float thresholdLum = dot(threshold, float3(0.299, 0.587, 0.114));

                    float rampVal = lum < thresholdLum ? thresholdLum - lum : 1.0;
                    float3 rgb = SAMPLE_TEXTURE2D(_ColorRampTex, sampler_ColorRampTex, float2(rampVal, 0.5)).rgb;

                    // 🔹 Suavizar el dithering en zonas muy brillantes (como el fondo)
                    float ditherIntensity = saturate(1.0 - lum * 1.5);
                    rgb = lerp(col, rgb, ditherIntensity);

                    return float4(rgb, 1);
                }
                ENDHLSL
            }
        }
}
