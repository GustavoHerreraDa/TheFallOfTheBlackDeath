// =============================================================================
// GlitchDamage.shader
// Shader de daño tipo "Glitch Digital" para Unity URP
// Autor: Technical Artist - RPG Sci-Fi Oscuro
//
// DESCRIPCIÓN:
// Deforma la geometría de las extremidades dañadas usando Vertex Color (canal R)
// como máscara. Los vértices pintados se desplazan de forma caótica simulando
// corrupción de datos / fragmentación holográfica.
// =============================================================================

Shader "Custom/GlitchDamage"
{
    Properties
    {
        // --- Textura principal del modelo ---
        [MainTexture] _BaseMap("Albedo (Base Map)", 2D) = "white" {}

        // --- Color base (tinte global) ---
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)

        // --- Color de emisión del glitch (rojo digital por defecto) ---
        [HDR] _GlitchColor("Glitch Error Color", Color) = (1, 0.1, 0.05, 1)

        // --- Control maestro del efecto (0 = sin glitch, 1 = máximo caos) ---
        _GlitchIntensity("Glitch Intensity", Range(0.0, 1.0)) = 0.0

        // --- Velocidad de cambio de posición de los vértices ---
        _GlitchSpeed("Glitch Speed", Float) = 8.0

        // --- Distancia máxima de desplazamiento (en unidades de espacio objeto) ---
        _GlitchAmount("Glitch Amount", Float) = 0.15

        // --- Intensidad del color de error en las zonas dañadas ---
        _GlitchColorStrength("Glitch Color Strength", Range(0.0, 1.0)) = 0.6
    }

    SubShader
    {
        // URP Unlit - sin cálculo de iluminación para mantener el aspecto de
        // "pantalla rota" / holográfico oscuro. Cambiar a "UniversalForward" si
        // se necesita Lit con sombras.
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "GlitchDamagePass"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM

            // Directivas de compilación para URP
            #pragma target 3.5
            #pragma vertex   vert
            #pragma fragment frag

            // Incluye las librerías base de URP
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // =========================================================
            // CBUFFER - Variables expuestas al Inspector
            // Deben estar dentro del bloque UnityPerMaterial para que
            // el batching de URP funcione correctamente.
            // =========================================================
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;         // Tiling & Offset de la textura
                float4 _BaseColor;
                float4 _GlitchColor;
                float  _GlitchIntensity;
                float  _GlitchSpeed;
                float  _GlitchAmount;
                float  _GlitchColorStrength;
            CBUFFER_END

            // Sampler de la textura principal
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            // =========================================================
            // ESTRUCTURAS de entrada y salida del Vertex Shader
            // =========================================================
            struct Attributes
            {
                float4 positionOS   : POSITION;    // Posición en Object Space
                float3 normalOS     : NORMAL;      // Normal en Object Space
                float2 uv           : TEXCOORD0;   // Coordenadas UV
                float4 vertexColor  : COLOR;       // Vertex Color pintado en el mesh
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION; // Posición en Clip Space (requerida)
                float2 uv           : TEXCOORD0;
                float4 vertexColor  : TEXCOORD1;   // Pasamos el vertex color al fragment
                float  glitchMask   : TEXCOORD2;   // Máscara calculada (canal R)
            };

            // =========================================================
            // FUNCIONES DE RUIDO / HASH
            //
            // Usamos hash matemáticas en lugar de texturas de ruido para
            // evitar dependencias de assets y maximizar el caos impredecible.
            // =========================================================

            // Hash 1D → [0, 1]: mapea un float a un valor pseudo-aleatorio
            // Basado en la técnica clásica de hash por sin/fract.
            float Hash11(float seed)
            {
                return frac(sin(seed * 127.1) * 43758.5453123);
            }

            // Hash 3D → float: toma una posición 3D y devuelve un escalar caótico.
            // Muy útil para romper la simetría y hacer que cada vértice
            // se desplace de forma independiente al resto.
            float Hash31(float3 p)
            {
                // Combinamos los tres ejes con números irracionales para
                // maximizar la decorrelación entre dimensiones.
                float h = dot(p, float3(127.1, 311.7, 74.7));
                return frac(sin(h) * 43758.5453123);
            }

            // Genera un vector de desplazamiento 3D caótico para un vértice.
            // El truco es "congelar" el ruido en intervalos discretos usando
            // floor() sobre el tiempo → efecto de "salto" brusco tipo glitch,
            // no una animación suave.
            float3 GlitchOffset(float3 posOS, float time, float speed, float amount)
            {
                // --- PASO 1: Discretizar el tiempo ---
                // Multiplicamos el tiempo por la velocidad y luego aplicamos floor().
                // Esto hace que el desplazamiento sea CONSTANTE durante un frame
                // y luego SALTE abruptamente al siguiente intervalo.
                // Es la clave para conseguir el look de "corrupción de datos".
                float timeSlice = floor(time * speed);

                // --- PASO 2: Generar un seed único por vértice y por frame ---
                // Mezclamos la posición del vértice en Object Space con el
                // tiempo discretizado para que cada vértice se mueva diferente.
                float3 seed3 = posOS * 7.3 + float3(timeSlice, timeSlice * 1.3, timeSlice * 0.7);

                // --- PASO 3: Calcular tres desplazamientos independientes (X, Y, Z) ---
                // Usamos seeds distintos por eje para que el movimiento sea
                // verdaderamente tridimensional y no sesgado.
                float dx = Hash31(seed3 + float3(1.0,  0.0,  0.0)) * 2.0 - 1.0;
                float dy = Hash31(seed3 + float3(0.0,  1.0,  0.0)) * 2.0 - 1.0;
                float dz = Hash31(seed3 + float3(0.0,  0.0,  1.0)) * 2.0 - 1.0;

                // --- PASO 4: Añadir una segunda capa de ruido más rápida ---
                // Una segunda "frecuencia" de glitch (más veloz) superpuesta
                // hace que algunos vértices tengan micro-tembleos además del
                // desplazamiento principal → look más orgánico y caótico.
                float timeSliceFast = floor(time * speed * 3.1);
                float3 seedFast = posOS * 13.7 + float3(timeSliceFast * 0.5, timeSliceFast, timeSliceFast * 1.7);
                float dx2 = Hash31(seedFast + float3(2.3, 0.0, 0.0)) * 2.0 - 1.0;
                float dy2 = Hash31(seedFast + float3(0.0, 2.3, 0.0)) * 2.0 - 1.0;
                float dz2 = Hash31(seedFast + float3(0.0, 0.0, 2.3)) * 2.0 - 1.0;

                // Combinamos ambas capas (capa lenta domina, capa rápida añade micro-temblor)
                float3 offset = float3(dx, dy, dz) * 0.7 + float3(dx2, dy2, dz2) * 0.3;

                // --- PASO 5: Escalar por la cantidad de deformación ---
                return offset * amount;
            }

            // =========================================================
            // VERTEX SHADER
            // =========================================================
            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                // --- Leer la máscara del Vertex Color (canal Rojo) ---
                // Solo los vértices pintados en rojo serán afectados.
                // Esto permite al artista pintar exactamente qué zonas
                // del modelo se rompen usando la herramienta de Vertex Paint.
                float mask = IN.vertexColor.r;

                // --- Calcular el desplazamiento de glitch ---
                // Solo procesamos el cálculo si hay intensidad activa para
                // ahorrar costo en GPU cuando el efecto está en 0.
                float3 glitchDisplace = float3(0, 0, 0);

                if (_GlitchIntensity > 0.001)
                {
                    // Generamos el vector de desplazamiento caótico
                    glitchDisplace = GlitchOffset(
                        IN.positionOS.xyz,  // Posición base del vértice (Object Space)
                        _Time.y,            // Tiempo global de Unity en segundos
                        _GlitchSpeed,       // Velocidad del efecto
                        _GlitchAmount       // Distancia máxima de desplazamiento
                    );

                    // --- Combinar: máscara × intensidad global × desplazamiento ---
                    // La máscara (Vertex Color R) restringe el efecto a las zonas pintadas.
                    // _GlitchIntensity es el fade-in/out global del efecto.
                    // El resultado: solo los vértices marcados se mueven,
                    // el resto del cuerpo permanece intacto.
                    glitchDisplace *= mask * _GlitchIntensity;
                }

                // --- Aplicar el desplazamiento a la posición en Object Space ---
                float4 displacedPositionOS = IN.positionOS;
                displacedPositionOS.xyz += glitchDisplace;

                // --- Transformar al Clip Space con la posición deformada ---
                OUT.positionHCS = TransformObjectToHClip(displacedPositionOS.xyz);

                // --- Pasar datos al Fragment Shader ---
                OUT.uv          = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.vertexColor = IN.vertexColor;
                OUT.glitchMask  = mask * _GlitchIntensity; // Máscara efectiva combinada

                return OUT;
            }

            // =========================================================
            // FRAGMENT SHADER
            // =========================================================
            half4 frag(Varyings IN) : SV_Target
            {
                // --- Muestrear la textura base ---
                half4 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;

                // --- Color de error del glitch ---
                // Interpolamos entre el color base y el color de error (rojo digital)
                // en las zonas que están siendo deformadas.
                // Usamos smoothstep para que la transición sea menos abrupta
                // y el efecto tenga más presencia visual.
                float errorBlend = IN.glitchMask * _GlitchColorStrength;

                // Ruido de color: hacemos que el tinte de error también parpadee
                // usando un hash sobre el tiempo para que no sea un color sólido.
                float colorNoise = Hash11(floor(_Time.y * _GlitchSpeed * 2.0) * 0.137);
                errorBlend *= lerp(0.5, 1.0, colorNoise); // Oscila entre 50% y 100%

                // Mezcla final: color base → color de error según la máscara
                half4 finalColor = lerp(baseColor, _GlitchColor, errorBlend);

                // --- Emisión adicional: brillo en los bordes del glitch ---
                // Añadimos un leve bloom multiplicativo en las zonas activas.
                // Como el canal es HDR (_GlitchColor puede ser > 1), el bloom
                // de URP lo recoge automáticamente si está activado en el Volume.
                finalColor.rgb += _GlitchColor.rgb * IN.glitchMask * 0.4 * colorNoise;

                return finalColor;
            }

            ENDHLSL
        }

        // =========================================================
        // SHADOW PASS
        // Necesario para que el modelo siga proyectando sombras
        // correctas incluso con los vértices deformados.
        // =========================================================
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM

            #pragma target 3.5
            #pragma vertex   ShadowVert
            #pragma fragment ShadowFrag

            #pragma multi_compile_shadowcaster

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float4 _GlitchColor;
                float  _GlitchIntensity;
                float  _GlitchSpeed;
                float  _GlitchAmount;
                float  _GlitchColorStrength;
            CBUFFER_END

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            struct AttributesShadow
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float4 vertexColor  : COLOR;
            };

            struct VaryingsShadow
            {
                float4 positionHCS  : SV_POSITION;
            };

            // Reutilizamos las mismas funciones de hash del pass principal
            float Hash31_Shadow(float3 p)
            {
                float h = dot(p, float3(127.1, 311.7, 74.7));
                return frac(sin(h) * 43758.5453123);
            }

            float3 GlitchOffset_Shadow(float3 posOS, float time, float speed, float amount)
            {
                float timeSlice = floor(time * speed);
                float3 seed3 = posOS * 7.3 + float3(timeSlice, timeSlice * 1.3, timeSlice * 0.7);
                float dx = Hash31_Shadow(seed3 + float3(1.0, 0.0, 0.0)) * 2.0 - 1.0;
                float dy = Hash31_Shadow(seed3 + float3(0.0, 1.0, 0.0)) * 2.0 - 1.0;
                float dz = Hash31_Shadow(seed3 + float3(0.0, 0.0, 1.0)) * 2.0 - 1.0;
                float timeSliceFast = floor(time * speed * 3.1);
                float3 seedFast = posOS * 13.7 + float3(timeSliceFast * 0.5, timeSliceFast, timeSliceFast * 1.7);
                float dx2 = Hash31_Shadow(seedFast + float3(2.3, 0.0, 0.0)) * 2.0 - 1.0;
                float dy2 = Hash31_Shadow(seedFast + float3(0.0, 2.3, 0.0)) * 2.0 - 1.0;
                float dz2 = Hash31_Shadow(seedFast + float3(0.0, 0.0, 2.3)) * 2.0 - 1.0;
                float3 offset = float3(dx, dy, dz) * 0.7 + float3(dx2, dy2, dz2) * 0.3;
                return offset * amount;
            }

            VaryingsShadow ShadowVert(AttributesShadow IN)
            {
                VaryingsShadow OUT;

                float mask = IN.vertexColor.r;
                float3 glitchDisplace = float3(0, 0, 0);

                if (_GlitchIntensity > 0.001)
                {
                    glitchDisplace = GlitchOffset_Shadow(
                        IN.positionOS.xyz, _Time.y, _GlitchSpeed, _GlitchAmount
                    );
                    glitchDisplace *= mask * _GlitchIntensity;
                }

                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz + glitchDisplace);
                float3 normalWS   = TransformObjectToWorldNormal(IN.normalOS);

                float4 positionHCS = TransformWorldToHClip(
                    ApplyShadowBias(positionWS, normalWS, float3(0, 0, 0))
                );

                // Evita shadow acne en cascadas de sombra
                #if UNITY_REVERSED_Z
                    positionHCS.z = min(positionHCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionHCS.z = max(positionHCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif

                OUT.positionHCS = positionHCS;
                return OUT;
            }

            half4 ShadowFrag(VaryingsShadow IN) : SV_Target
            {
                return 0; // Shadow pass solo necesita el depth buffer
            }

            ENDHLSL
        }

        // =========================================================
        // DEPTH PREPASS
        // Para efectos de post-proceso (SSAO, depth of field, etc.)
        // que necesitan el depth buffer correcto con la deformación.
        // =========================================================
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask R
            Cull Back

            HLSLPROGRAM

            #pragma target 3.5
            #pragma vertex   DepthVert
            #pragma fragment DepthFrag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float4 _GlitchColor;
                float  _GlitchIntensity;
                float  _GlitchSpeed;
                float  _GlitchAmount;
                float  _GlitchColorStrength;
            CBUFFER_END

            struct AttributesDepth
            {
                float4 positionOS  : POSITION;
                float4 vertexColor : COLOR;
            };

            struct VaryingsDepth
            {
                float4 positionHCS : SV_POSITION;
            };

            float Hash31_Depth(float3 p)
            {
                float h = dot(p, float3(127.1, 311.7, 74.7));
                return frac(sin(h) * 43758.5453123);
            }

            VaryingsDepth DepthVert(AttributesDepth IN)
            {
                VaryingsDepth OUT;

                float mask = IN.vertexColor.r;
                float3 glitchDisplace = float3(0, 0, 0);

                if (_GlitchIntensity > 0.001)
                {
                    float timeSlice = floor(_Time.y * _GlitchSpeed);
                    float3 seed3 = IN.positionOS.xyz * 7.3 + timeSlice;
                    float dx = Hash31_Depth(seed3 + float3(1, 0, 0)) * 2.0 - 1.0;
                    float dy = Hash31_Depth(seed3 + float3(0, 1, 0)) * 2.0 - 1.0;
                    float dz = Hash31_Depth(seed3 + float3(0, 0, 1)) * 2.0 - 1.0;
                    glitchDisplace = float3(dx, dy, dz) * _GlitchAmount * mask * _GlitchIntensity;
                }

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz + glitchDisplace);
                return OUT;
            }

            half DepthFrag(VaryingsDepth IN) : SV_Target
            {
                return IN.positionHCS.z;
            }

            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "UnityEditor.Rendering.Universal.ShaderGUI.UnlitShaderGUI"
}
