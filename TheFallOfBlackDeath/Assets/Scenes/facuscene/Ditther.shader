Shader "Hidden/URP/DitherEffect"
{
    Properties
    {
        _MainTex("MainTex", 2D) = "white" {}
        _BlueNoiseTex("Blue Noise Texture", 2D) = "gray" {}
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl" 

            // Variables uniformes
            TEXTURE2D(_BlueNoiseTex); SAMPLER(sampler_BlueNoiseTex);
            float4 _BlueNoiseTex_TexelSize;

            float _NoiseScale;
            int _BitDepth;
            float _Contrast;
            float _Brightness;
            float _Threshold;
            float _EdgeStrength;
            float _EdgeThreshold;
            float _DitherTime;

            // Función para reconstruir World Position desde el Depth
            float3 ReconstructWorldPos(float2 uv, float depth)
            {
                float4 ndc = float4(uv * 2.0 - 1.0, depth, 1.0);
                #if UNITY_REVERSED_Z
                    ndc.z = depth;
                #else
                    ndc.z = depth * 2.0 - 1.0;
                #endif
                float4 worldPos = mul(UNITY_MATRIX_I_VP, ndc);
                return worldPos.xyz / worldPos.w;
            }

            // Sobel Edge Detection
            float GetEdge(float2 uv)
            {
                float2 texelSize = 1.0 / _ScreenParams.xy;
                
                // Muestras de Depth
                float d = SampleSceneDepth(uv);
                float d_up = SampleSceneDepth(uv + float2(0, texelSize.y));
                float d_down = SampleSceneDepth(uv - float2(0, texelSize.y));
                float d_left = SampleSceneDepth(uv - float2(texelSize.x, 0));
                float d_right = SampleSceneDepth(uv + float2(texelSize.x, 0));

                float depthEdge = (abs(d - d_up) + abs(d - d_down) + abs(d - d_left) + abs(d - d_right)) * 10.0;

                // Muestras de Normals
                float3 n = SampleSceneNormals(uv);
                float3 n_up = SampleSceneNormals(uv + float2(0, texelSize.y));
                float3 n_left = SampleSceneNormals(uv - float2(texelSize.x, 0));

                float normalEdge = (1.0 - dot(n, n_up)) + (1.0 - dot(n, n_left));
                
                float edge = saturate(depthEdge + normalEdge * 5.0);
                return edge > _EdgeThreshold ? _EdgeStrength : 0.0;
            }

            float4 Frag(Varyings i) : SV_Target
            {
                float2 uv = i.texcoord;
                
                // 1. Color Base y Luminancia
                float3 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv).rgb;
                float lum = dot(col, float3(0.299, 0.587, 0.114));

                // 2. Curva de Luminancia (Contraste y Brillo)
                lum = saturate((lum - 0.5) * (_Contrast * 4.0 + 1.0) + 0.5 + (_Brightness - 0.5));
                
                // 3. Reconstrucción de posición para Dithering Estable
                float rawDepth = SampleSceneDepth(uv);
                float3 worldPos = ReconstructWorldPos(uv, rawDepth);
                
                // Proyección triplanar simple para evitar distorsión
                float2 ditherUV = worldPos.xz + worldPos.y; 
                ditherUV *= (_NoiseScale * 0.01);
                
                // Añadir offset temporal si el ruido es animado
                ditherUV += frac(_DitherTime * 0.1);

                // 4. Blue Noise Dithering
                float dither = SAMPLE_TEXTURE2D(_BlueNoiseTex, sampler_BlueNoiseTex, ditherUV).r;
                
                // 5. Cuantización y Thresholding
                float levels = pow(2.0, (float)_BitDepth);
                float rawLum = lum * (levels - 1.0);
                float integerLum = floor(rawLum);
                float fractionalLum = frac(rawLum);
                
                float quantized = (fractionalLum > dither) ? (integerLum + 1.0) : integerLum;
                float finalLum = quantized / (levels - 1.0);

                // 6. Edge Detection
                float edge = GetEdge(uv);
                finalLum = saturate(finalLum - edge);

                return float4(finalLum.xxx, 1.0);
            }
            ENDHLSL
        }
    }
}
