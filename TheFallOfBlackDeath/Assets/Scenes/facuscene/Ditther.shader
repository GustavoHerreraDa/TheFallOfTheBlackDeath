Shader "Hidden/URP/DitherEffect"
{
    Properties
    {
        _MainTex("MainTex", 2D) = "white" {}
        _NoiseTex("Noise Texture", 2D) = "gray" {} 
        _ColorRampTex("Color Ramp", 2D) = "white" {}
        _NoiseScale("Noise Scale", Float) = 256
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "DitherPass"
            
            // Mantenemos tu Stencil para proteger la UI
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
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl" 

            // Variables uniformes
            TEXTURE2D(_NoiseTex); SAMPLER(sampler_NoiseTex);
            TEXTURE2D(_ColorRampTex); SAMPLER(sampler_ColorRampTex);
            float4x4 _InverseView; // Matriz que pasamos desde C#
            float _NoiseScale;

            float4 Frag(Varyings i) : SV_Target
            {
                // 1. Muestrear color original
                float3 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.texcoord).rgb;
                float lum = dot(col, float3(0.299, 0.587, 0.114));

                // 2. MAGIA: Calcular UVs basadas en la dirección de la vista en el mundo
                // Convertimos la coordenada UV de pantalla a coordenadas de clip (-1 a 1)
                float2 p11 = i.texcoord * 2.0 - 1.0;
                
                // Calculamos el vector de dirección desde la cámara hacia ese píxel en el mundo
                // (Usamos un valor Z arbitrario para la proyección)
                float4 viewPos = mul(unity_CameraInvProjection, float4(p11, 0.0, 1.0)); // Clip -> View
                viewPos.xyz /= viewPos.w;
                float3 worldDir = mul((float3x3)_InverseView, viewPos.xyz); // View -> World direction
                worldDir = normalize(worldDir);

                // Mapeo esférico/cilíndrico para el ruido
                // atan2 nos da el ángulo horizontal, asin el vertical.
                float2 noiseUV = float2(
                    atan2(worldDir.x, worldDir.z), 
                    asin(worldDir.y)
                ) * (_NoiseScale / 3.14159);

                // 3. Dither (Blue Noise es mejor aquí)
                float threshold = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, noiseUV).r;

                // Tu lógica de rampa (que estaba muy bien)
                float rampVal = lum < threshold ? threshold - lum : 1.0;
                float3 finalCol = SAMPLE_TEXTURE2D(_ColorRampTex, sampler_ColorRampTex, float2(rampVal, 0.5)).rgb;

                return float4(finalCol, 1);
            }
            ENDHLSL
        }
    }
}