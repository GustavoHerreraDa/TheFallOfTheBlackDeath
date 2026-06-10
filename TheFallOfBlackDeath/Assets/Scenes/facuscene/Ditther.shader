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
            
            // NOTA: El stencil excluye píxeles marcados con Ref=1
            // Usalo para proteger UI u objetos que no deben recibir dither.
            // Si no usás stencil en tu pipeline, podés eliminar este bloque.
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

            // ─────────────────────────────────────────────
            //  Texturas y samplers
            // ─────────────────────────────────────────────
            TEXTURE2D(_BlueNoiseTex);
            SAMPLER(sampler_BlueNoiseTex);
            float4 _BlueNoiseTex_TexelSize;

            // ─────────────────────────────────────────────
            //  Parámetros expuestos
            // ─────────────────────────────────────────────
            float  _NoiseScale;
            int    _BitDepth;
            float  _Contrast;
            float  _Brightness;
            float  _LumThreshold;   // Corregido: antes "_Threshold" declarado pero no usado
            float  _EdgeStrength;
            float  _EdgeThreshold;
            float  _DitherTime;
            float  _TriplanarSharpness; // Controla el blend entre planos (1=suave, 8=nítido)

            // Paleta de dos colores estilo Obra Dinn
            float3 _ColorDark;
            float3 _ColorLight;

            // ─────────────────────────────────────────────
            //  Reconstrucción de World Position desde Depth
            // ─────────────────────────────────────────────
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

            // ─────────────────────────────────────────────
            //  Triplanar UV real con blend por normal
            //
            //  Combina las tres proyecciones (XY, XZ, YZ)
            //  ponderadas por cuánto mira la normal hacia
            //  cada eje. Elimina el seam en superficies
            //  diagonales que tenía el código anterior.
            // ─────────────────────────────────────────────
            float SampleBlueNoiseTriplanar(float3 worldPos, float3 worldNormal, float scale)
            {
                float2 uvX = worldPos.yz * scale; // Plano YZ (caras que miran en X)
                float2 uvY = worldPos.xz * scale; // Plano XZ (suelos/techos)
                float2 uvZ = worldPos.xy * scale; // Plano XY (caras que miran en Z)

                float noiseX = SAMPLE_TEXTURE2D(_BlueNoiseTex, sampler_BlueNoiseTex, uvX).r;
                float noiseY = SAMPLE_TEXTURE2D(_BlueNoiseTex, sampler_BlueNoiseTex, uvY).r;
                float noiseZ = SAMPLE_TEXTURE2D(_BlueNoiseTex, sampler_BlueNoiseTex, uvZ).r;

                // Pesos: normal al cuadrado elevado a la sharpness
                // pow con abs evita artefactos en normales negativas
                float3 blend = pow(abs(worldNormal), _TriplanarSharpness);
                blend /= (blend.x + blend.y + blend.z + 0.0001); // Normalizar a suma 1

                return noiseX * blend.x + noiseY * blend.y + noiseZ * blend.z;
            }

            // ─────────────────────────────────────────────
            //  Sobel 3x3 completo (isotrópico)
            //
            //  El código anterior usaba 5 samples en cruz (+),
            //  que es asimétrico con las diagonales.
            //  Este kernel 3x3 estándar da bordes más limpios.
            // ─────────────────────────────────────────────
            float GetEdge(float2 uv)
            {
                float2 t = 1.0 / _ScreenParams.xy;

                // ── Kernel Sobel de Depth ──
                float d00 = SampleSceneDepth(uv + float2(-t.x,  t.y));
                float d10 = SampleSceneDepth(uv + float2( 0.0,  t.y));
                float d20 = SampleSceneDepth(uv + float2( t.x,  t.y));
                float d01 = SampleSceneDepth(uv + float2(-t.x,  0.0));
                float d21 = SampleSceneDepth(uv + float2( t.x,  0.0));
                float d02 = SampleSceneDepth(uv + float2(-t.x, -t.y));
                float d12 = SampleSceneDepth(uv + float2( 0.0, -t.y));
                float d22 = SampleSceneDepth(uv + float2( t.x, -t.y));

                float gxD = -d00 + d20 - 2.0*d01 + 2.0*d21 - d02 + d22;
                float gyD = -d00 - 2.0*d10 - d20 + d02 + 2.0*d12 + d22;
                float depthEdge = sqrt(gxD * gxD + gyD * gyD) * 5.0;

                // ── Kernel Sobel de Normals ──
                float3 n00 = SampleSceneNormals(uv + float2(-t.x,  t.y));
                float3 n20 = SampleSceneNormals(uv + float2( t.x,  t.y));
                float3 n01 = SampleSceneNormals(uv + float2(-t.x,  0.0));
                float3 n21 = SampleSceneNormals(uv + float2( t.x,  0.0));
                float3 n02 = SampleSceneNormals(uv + float2(-t.x, -t.y));
                float3 n22 = SampleSceneNormals(uv + float2( t.x, -t.y));

                float3 gxN = -n00 + n20 - 2.0*n01 + 2.0*n21 - n02 + n22;
                float3 gyN = -n00 - 2.0 * SampleSceneNormals(uv + float2(0, t.y))
                             - n20
                             + n02 + 2.0 * SampleSceneNormals(uv + float2(0, -t.y)) + n22;
                float normalEdge = sqrt(dot(gxN, gxN) + dot(gyN, gyN)) * 2.0;

                float edge = saturate(depthEdge + normalEdge);
                return edge > _EdgeThreshold ? _EdgeStrength : 0.0;
            }

            // ─────────────────────────────────────────────
            //  Fragment
            // ─────────────────────────────────────────────
            float4 Frag(Varyings i) : SV_Target
            {
                float2 uv = i.texcoord;

                // 1. Color base → Luminancia perceptual
                float3 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv).rgb;
                float lum = dot(col, float3(0.299, 0.587, 0.114));

                // 2. Umbral de luminancia mínima (antes "_Threshold" sin usar)
                //    Valores bajo el umbral se fuerzan a negro puro.
                lum = lum < _LumThreshold ? 0.0 : lum;

                // 3. Curva de contraste y brillo
                lum = saturate((lum - 0.5) * (_Contrast * 4.0 + 1.0) + 0.5 + (_Brightness - 0.5));

                // 4. Reconstrucción de World Position y Normal para dithering estable
                float rawDepth   = SampleSceneDepth(uv);
                float3 worldPos  = ReconstructWorldPos(uv, rawDepth);
                float3 worldNorm = SampleSceneNormals(uv); // Normal en world space

                // Offset temporal por número de frame (menos crawling que Time continuo)
                // _DitherTime = 0 si animatedNoise está apagado (sin movimiento)
                float scale = _NoiseScale * 0.01;
                float3 animatedPos = worldPos;
                animatedPos.xz += floor(_DitherTime) * 0.137; // desplazamiento por frame, no suave

                // 5. Blue Noise triplanar real
                float dither = SampleBlueNoiseTriplanar(animatedPos, worldNorm, scale);

                // 6. Cuantización + Thresholding
                float levels    = pow(2.0, (float)_BitDepth);
                float rawLum    = lum * (levels - 1.0);
                float intLum    = floor(rawLum);
                float fracLum   = frac(rawLum);

                float quantized = (fracLum > dither) ? (intLum + 1.0) : intLum;
                float finalLum  = quantized / (levels - 1.0);

                // 7. Edge Detection → oscurece bordes
                float edge = GetEdge(uv);
                finalLum = saturate(finalLum - edge);

                // 8. Paleta de dos colores (estilo Obra Dinn)
                //    _ColorDark  = negro/tinta  (ej: float3(0.08, 0.06, 0.05))
                //    _ColorLight = blanco/papel (ej: float3(0.95, 0.92, 0.86))
                float3 finalColor = lerp(_ColorDark, _ColorLight, finalLum);

                return float4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}
