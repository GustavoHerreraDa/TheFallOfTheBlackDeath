Shader "Custom/DitherEffect"
{
    Properties
    {
        _DitherTex("Dither Pattern", 2D) = "white" {}
        _Intensity("Intensity", Range(0,1)) = 1
    }

        SubShader
        {
            Tags { "RenderType" = "Opaque" }

            Pass
            {
                Name "DITHER"
                ZTest Always
                ZWrite Off
                Cull Off

                HLSLPROGRAM
                #pragma vertex Vert
                #pragma fragment Frag

                sampler2D _CameraOpaqueTexture;
                sampler2D _DitherTex;

                float4 _CameraOpaqueTexture_TexelSize;
                float _Intensity;

                struct Attributes
                {
                    float4 position : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct Varyings
                {
                    float4 position : SV_POSITION;
                    float2 uv : TEXCOORD0;
                };

                Varyings Vert(Attributes v)
                {
                    Varyings o;
                    o.position = TransformVertex(v.position);
                    o.uv = v.uv;
                    return o;
                }

                float4 Frag(Varyings i) : SV_Target
                {
                    float4 col = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, i.uv);

                    float2 screenUV = i.position.xy * _CameraOpaqueTexture_TexelSize.xy;
                    float ditherValue = SAMPLE_TEXTURE2D(_DitherTex, sampler_DitherTex, screenUV * 8).r;

                    float lumin = dot(col.rgb, float3(0.299, 0.587, 0.114));

                    float threshold = ditherValue * _Intensity;
                    float outVal = lumin < threshold ? 0 : 1;

                    return float4(outVal, outVal, outVal, 1);
                }
                ENDHLSL
            }
        }
}
