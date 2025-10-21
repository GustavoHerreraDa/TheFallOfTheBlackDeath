Shader "Hidden/Ditther"
{
    Shader "Hidden/DitherURP"
    {
        Properties
        {
            _NoiseTex("Dither Texture", 2D) = "white" {}
            _ColorRampTex("Ramp Texture", 2D) = "white" {}
            _XOffset("X Offset", Float) = 0
            _YOffset("Y Offset", Float) = 0
        }
            SubShader
            {
                Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
                Pass
                {
                    Name "DitherPass"
                    ZTest Always ZWrite Off Cull Off
                    HLSLPROGRAM
                    #pragma vertex Vert
                    #pragma fragment Frag
                    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

                    TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
                    TEXTURE2D(_NoiseTex); SAMPLER(sampler_NoiseTex);
                    TEXTURE2D(_ColorRampTex); SAMPLER(sampler_ColorRampTex);
                    float _XOffset, _YOffset;

                    struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
                    struct Varyings { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; };

                    Varyings Vert(Attributes IN)
                    {
                        Varyings OUT;
                        OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                        OUT.uv = IN.uv;
                        return OUT;
                    }

                    half4 Frag(Varyings IN) : SV_Target
                    {
                        float2 uv = IN.uv;
                        float3 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).rgb;
                        float noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, uv + float2(_XOffset, _YOffset)).r;
                        float3 ramp = SAMPLE_TEXTURE2D(_ColorRampTex, sampler_ColorRampTex, float2(noise, 0.5)).rgb;
                        return half4(color * ramp, 1.0);
                    }
                    ENDHLSL
                }
            }
    }
