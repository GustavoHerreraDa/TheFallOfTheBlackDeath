// ============================================================
//  HoloButton_Diegetic.shader
//  Shader Unlit holográfico para botón de combate — URP / HLSL
//  Compatible con: Unity URP 12+ (Unity 2021.2+)
//  Requiere Post-Processing con Bloom activado en la Camera
// ============================================================

Shader "Custom/HoloButton_Diegetic"
{
    Properties
    {
        // Textura base: texto y bordes del botón con fondo transparente (RGBA)
        _MainTex        ("Textura Base (RGBA)", 2D)             = "white" {}

        // Color HDR: permite valores > 1 para saturar el Color Buffer y activar Bloom
        [HDR]
        _HoloColor      ("Color Neón HDR", Color)               = (0.0, 1.8, 1.4, 1.0)

        // Multiplicador final de emisión — escalar > 1 dispara el Bloom de URP
        _EmitIntensity  ("Intensidad de Emisión", Float)        = 2.5

        // Densidad de las líneas horizontales (scanlines)
        _ScanlineFrequency ("Frecuencia de Scanlines", Float)   = 80.0

        // Velocidad de desplazamiento de las scanlines (positivo = hacia arriba)
        _ScanlineSpeed  ("Velocidad de Scanlines", Float)       = 0.6

        // Velocidad del parpadeo errático tipo tubo fluorescente
        _FlickerSpeed   ("Velocidad de Parpadeo", Float)        = 8.0
    }

    SubShader
    {
        // ── Tags ────────────────────────────────────────────────────────────
        // RenderType "Transparent" + Queue "Transparent" para respetar el alpha
        // de la textura base. RenderPipeline "UniversalPipeline" obliga a URP.
        Tags
        {
            "RenderType"        = "Transparent"
            "Queue"             = "Transparent"
            "RenderPipeline"    = "UniversalPipeline"
            "IgnoreProjector"   = "True"
        }

        LOD 100

        Pass
        {
            // ── Estado del pipeline ──────────────────────────────────────────
            Name "HoloButton_Unlit"

            // Blend estándar alfa-premultiplicado (SrcAlpha / OneMinusSrcAlpha)
            Blend SrcAlpha OneMinusSrcAlpha

            // Desactivamos escritura de profundidad para transparentes
            ZWrite Off

            // El botón siempre visible (sin culling, puede ser coplanar con UI)
            Cull Off

            HLSLPROGRAM
            // ── Pragmas ──────────────────────────────────────────────────────
            #pragma vertex   vert
            #pragma fragment frag
            // target 3.0 para tener acceso a funciones trigonométricas completas
            #pragma target 3.0

            // ── Includes URP ─────────────────────────────────────────────────
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // ── Declaración de texturas y samplers ───────────────────────────
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            // ── Bloque CBUFFER (requerido por el SRP Batcher de URP) ─────────
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;         // Tiling & Offset de la textura
                half4  _HoloColor;          // Color HDR (r,g,b,a)
                float  _EmitIntensity;
                float  _ScanlineFrequency;
                float  _ScanlineSpeed;
                float  _FlickerSpeed;
            CBUFFER_END

            // ── Estructuras de vértice ───────────────────────────────────────
            struct Attributes
            {
                float4 positionOS : POSITION;   // Posición en Object Space
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION; // Posición en Clip Space
                float2 uv          : TEXCOORD0;
            };

            // ================================================================
            //  VERTEX SHADER
            // ================================================================
            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                // TransformObjectToHClip: MVP matrix automática de URP Core
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);

                // Aplica Tiling y Offset definidos en el Inspector
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);

                return OUT;
            }

            // ================================================================
            //  FUNCIONES AUXILIARES
            // ================================================================

            // ── Scanlines ────────────────────────────────────────────────────
            // Genera una máscara de líneas horizontales animadas.
            // • uv.y * _ScanlineFrequency → escala el eje Y para crear N bandas
            // • + _Time.y * _ScanlineSpeed → desplaza las bandas con el tiempo
            // • sin(x) → onda senoidal [-1, 1]
            // • * 0.5 + 0.5 → reescalada a [0, 1]  (sin balanceado)
            // • pow(x, 0.6) → suaviza el contraste para que no sean muy duras
            float ComputeScanlines(float2 uv)
            {
                float wave = sin(uv.y * _ScanlineFrequency + _Time.y * _ScanlineSpeed);
                float mask = wave * 0.5 + 0.5;           // rango [0, 1]
                return pow(mask, 0.6);                    // suavizado de contraste
            }

            // ── Parpadeo Errático (Flicker) ──────────────────────────────────
            // Simula el fallo técnico de un tubo fluorescente viejo.
            // La clave está en combinar varias frecuencias irracionales de sin()
            // y extraer la parte fraccionaria (frac) para romper la periodicidad.
            //
            // • frac(sin(t * F) * 43758.5453) → hash numérico pseudo-aleatorio
            //   (43758.5453 es una constante de hash clásica en shaders GLSL/HLSL)
            // • Mezclar dos hashes a distintas velocidades → ruido temporal
            // • lerp(0.7, 1.0, noise) → limitamos el rango mínimo al 70%
            //   para que el botón nunca quede completamente apagado
            float ComputeFlicker(float speed)
            {
                float t = _Time.y * speed;

                // Hash A: frecuencia alta → parpadeos rápidos y cortos
                float hashA = frac(sin(t * 1.7321) * 43758.5453);

                // Hash B: frecuencia media → intermitencias más lentas
                float hashB = frac(sin(t * 0.6931 + 1.4142) * 22378.1415);

                // Mezclamos ambos hashes para obtener ruido no periódico
                float noise = frac(hashA + hashB);

                // Remapeamos para que el botón no quede totalmente negro
                return lerp(0.7, 1.0, noise);
            }

            // ================================================================
            //  FRAGMENT SHADER
            // ================================================================
            half4 frag(Varyings IN) : SV_Target
            {
                // ── 1. Muestreo de textura base ──────────────────────────────
                // Obtenemos el color RGBA de la textura (texto + bordes del botón)
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                // ── 2. Cálculo de Scanlines ──────────────────────────────────
                float scanlines = ComputeScanlines(IN.uv);

                // ── 3. Cálculo de Parpadeo ───────────────────────────────────
                float flicker = ComputeFlicker(_FlickerSpeed);

                // ── 4. Composición final ─────────────────────────────────────
                // Paso a: Textura × Color HDR → tinta el sprite con el neón
                half3 litColor = texColor.rgb * _HoloColor.rgb;

                // Paso b: × scanlines → imprime las bandas horizontales
                litColor *= scanlines;

                // Paso c: × flicker → aplica el parpadeo errático
                litColor *= flicker;

                // Paso d: × _EmitIntensity → escala los valores por encima de 1
                //         para saturar el Color Buffer y activar el Bloom en URP
                litColor *= _EmitIntensity;

                // ── 5. Alpha final ───────────────────────────────────────────
                // Preservamos el alpha original de la textura (transparencia del fondo)
                // Atenuamos también el alpha con el parpadeo para que el fade sea total
                half finalAlpha = texColor.a * _HoloColor.a * flicker;

                return half4(litColor, finalAlpha);
            }

            ENDHLSL
        }
    }

    // Fallback mínimo si URP no está disponible
    FallBack "Hidden/InternalErrorShader"
}
