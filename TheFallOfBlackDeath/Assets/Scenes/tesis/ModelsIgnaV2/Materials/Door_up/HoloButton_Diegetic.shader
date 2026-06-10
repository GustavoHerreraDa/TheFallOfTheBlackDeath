// ============================================================
//  HoloButton_Diegetic.shader  — v4 con Parallax Holográfico por Cámara
//  Shader Unlit holográfico para botón de combate — URP / HLSL
//  Compatible con: Unity URP 12+ (Unity 2021.2+)
//  Requiere Post-Processing con Bloom activado en la Camera
//  El estado (Idle/Hover/Press) se inyecta desde HoloButtonFX.cs
// ============================================================

Shader "Custom/HoloButton_Diegetic"
{
    Properties
    {
        // ── Textura ────────────────────────────────────────────────────────
        _MainTex            ("Textura Base (RGBA)", 2D)         = "white" {}

        // ── Colores HDR por estado ─────────────────────────────────────────
        [HDR]
        _HoloColor          ("Color Idle HDR",  Color)          = (0.0,  1.8, 1.4, 1.0)
        [HDR]
        _HoverColor         ("Color Hover HDR", Color)          = (0.2,  2.2, 2.0, 1.0)
        [HDR]
        _PressColor         ("Color Press HDR", Color)          = (1.0,  0.4, 0.1, 1.0)

        // ── Estado actual (0 = Idle, 1 = Hover, 2 = Press) ────────────────
        // Escrito por HoloButtonFX.cs con SetFloat, nunca editado a mano
        _ButtonState        ("Estado (0/1/2)",  Float)          = 0

        // ── Transición suavizada entre estados ────────────────────────────
        // También escrito por el script (lerp en CPU cada frame)
        _StateBlend         ("Blend Estado",    Float)          = 0.0

        // ── Emisión ────────────────────────────────────────────────────────
        _EmitIntensity      ("Intensidad Emisión",      Float)  = 2.5
        _HoverEmitBoost     ("Boost Emisión Hover",     Float)  = 1.4
        _PressEmitBoost     ("Boost Emisión Press",     Float)  = 0.6

        // ── Scanlines ──────────────────────────────────────────────────────
        _ScanlineFrequency  ("Frecuencia Scanlines",    Float)  = 80.0
        _ScanlineSpeed      ("Velocidad Scanlines",     Float)  = 0.6
        _HoverScanBoost     ("Boost Velocidad Hover",   Float)  = 3.0

        // ── Parpadeo ───────────────────────────────────────────────────────
        _FlickerSpeed       ("Velocidad Parpadeo",      Float)  = 8.0

        // ── Escala UV (efecto zoom-out al presionar) ───────────────────────
        _PressUVScale       ("Escala UV Press",         Float)  = 1.05

        // ── Borde de energía (rim) al hacer Hover ─────────────────────────
        // Ancho del glow en los bordes del quad (0 = sin borde, 0.15 = ancho)
        _RimWidth           ("Ancho Borde Hover",       Float)  = 0.08

        // ── Glitch de Aberración Cromática ─────────────────────────────────
        // Separa los canales R y B en direcciones opuestas sobre el eje X.
        // Actúa en ráfagas erráticas: N frames encendido, M frames apagado.

        // Máximo desplazamiento UV de cada canal en estado Idle (en UV space)
        _GlitchOffsetIdle   ("Glitch Offset Idle",      Float)  = 0.008

        // Máximo desplazamiento UV en estado Hover (el hover "activa" el sistema)
        _GlitchOffsetHover  ("Glitch Offset Hover",     Float)  = 0.022

        // Velocidad del ruido que modula las ráfagas de glitch
        _GlitchSpeed        ("Velocidad Glitch",        Float)  = 6.0

        // Umbral de disparo: el glitch solo ocurre cuando el ruido supera este valor.
        // 0.0 = siempre activo, 0.85 = ráfagas muy cortas y raras
        _GlitchThreshold    ("Umbral Ráfaga",           Float)  = 0.75

        // Intensidad del desplazamiento vertical (scanline tear) durante el glitch
        _GlitchTearStrength ("Fuerza Tear Vertical",    Float)  = 0.004

        // ── Parallax Holográfico por Cámara ────────────────────────────────
        // _ParallaxVec es escrito cada frame por HoloParallaxDriver.cs.
        // X = desplazamiento lateral, Y = desplazamiento vertical.
        // NO se edita manualmente — es solo de escritura desde CPU.
        _ParallaxVec        ("Vector Parallax (CPU)",   Vector) = (0,0,0,0)

        // Profundidad de capa: cuánto amplifica el parallax en este material.
        // Capa base (texto/bordes)  → _LayerDepth bajo  (ej: 0.04)
        // Capa media (detalles)     → _LayerDepth medio (ej: 0.09)
        // Capa superior (glow rim)  → _LayerDepth alto  (ej: 0.16)
        _LayerDepth         ("Profundidad de Capa",     Float)  = 0.06
    }

    SubShader
    {
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
            Name "HoloButton_Unlit"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4  _HoloColor;
                half4  _HoverColor;
                half4  _PressColor;
                float  _ButtonState;
                float  _StateBlend;
                float  _EmitIntensity;
                float  _HoverEmitBoost;
                float  _PressEmitBoost;
                float  _ScanlineFrequency;
                float  _ScanlineSpeed;
                float  _HoverScanBoost;
                float  _FlickerSpeed;
                float  _PressUVScale;
                float  _RimWidth;
                // Glitch
                float  _GlitchOffsetIdle;
                float  _GlitchOffsetHover;
                float  _GlitchSpeed;
                float  _GlitchThreshold;
                float  _GlitchTearStrength;
                // Parallax
                float4 _ParallaxVec;
                float  _LayerDepth;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                // UV sin tiling/offset para el cálculo de rim (espacio 0-1 puro)
                float2 uvRaw       : TEXCOORD1;
            };

            // ================================================================
            //  FUNCIONES AUXILIARES
            // ================================================================

            // ── Scanlines ────────────────────────────────────────────────────
            // speedMult escala la velocidad extra en hover
            float ComputeScanlines(float2 uv, float speedMult)
            {
                float speed = _ScanlineSpeed * speedMult;
                float wave  = sin(uv.y * _ScanlineFrequency + _Time.y * speed);
                float mask  = wave * 0.5 + 0.5;
                return pow(mask, 0.6);
            }

            // ── Flicker ───────────────────────────────────────────────────────
            float ComputeFlicker(float speed)
            {
                float t     = _Time.y * speed;
                float hashA = frac(sin(t * 1.7321)          * 43758.5453);
                float hashB = frac(sin(t * 0.6931 + 1.4142) * 22378.1415);
                float noise = frac(hashA + hashB);
                return lerp(0.7, 1.0, noise);
            }

            // ── Rim de energía ────────────────────────────────────────────────
            // Crea un brillo en los 4 bordes del quad midiendo la distancia
            // desde el centro en UVs 0-1.
            // • abs(uv - 0.5) * 2  → remap a [0,1] donde 1 = borde
            // • min(edgeDist.x, edgeDist.y) → borde más cercano global
            // • smoothstep → suaviza la transición para evitar aliasing
            float ComputeRim(float2 uvRaw, float width)
            {
                float2 edgeDist = 1.0 - abs(uvRaw - 0.5) * 2.0; // 0 en borde, 1 en centro
                float  minEdge  = min(edgeDist.x, edgeDist.y);
                return 1.0 - smoothstep(0.0, width, minEdge);
            }

            // ── Pulso de press ────────────────────────────────────────────────
            // Al presionar: flash de energía instantáneo que decae con la curva
            // convexa pow(1-blend, 0.3) — cae rápido al inicio, suave al final.
            float ComputePressFlash(float blend)
            {
                return 1.0 + pow(max(0.0, 1.0 - blend), 0.3) * 2.5;
            }

            // ================================================================
            //  GLITCH — Aberración cromática + Scanline Tear
            // ================================================================
            //
            //  TEORÍA:
            //  La aberración cromática real ocurre cuando una lente no focaliza
            //  todos los colores en el mismo punto. En pantallas holográficas
            //  defectuosas lo simulamos separando los canales de color:
            //
            //    Canal R → muestreado desde uv + offset hacia la derecha
            //    Canal G → muestreado desde uv sin desplazamiento (referencia)
            //    Canal B → muestreado desde uv - offset hacia la izquierda
            //
            //  El offset NO es constante: vive en ráfagas erráticas controladas
            //  por un hash de tiempo que solo supera el umbral _GlitchThreshold
            //  ocasionalmente. Cuando está activo, un segundo hash modula el
            //  desplazamiento vertical (scanline tear) por banda horizontal.
            //
            //  PARÁMETROS DE SALIDA:
            //    glitchActive  — 0.0 si la ráfaga no está disparada, > 0 si sí
            //    offsetX       — desplazamiento horizontal final para R y B
            //    tearY         — desplazamiento vertical de tear por scanline
            // ─────────────────────────────────────────────────────────────────

            // Genera un valor pseudo-aleatorio [0,1] para un tiempo dado.
            // Función separada para poder llamarla con distintas semillas.
            float GlitchHash(float t, float seed)
            {
                return frac(sin(t * seed) * 43758.5453);
            }

            // Estructura de retorno del glitch (HLSL no tiene múltiples returns)
            struct GlitchData
            {
                float active;    // 0 = apagado, 1 = encendido a máxima intensidad
                float offsetX;   // desplazamiento UV en X para R y B
                float tearY;     // desplazamiento UV en Y por scanline tear
            };

            GlitchData ComputeGlitch(float2 uv, float maxOffset)
            {
                GlitchData g;
                float t = _Time.y * _GlitchSpeed;

                // ── 1. Ruido de ráfaga (burst noise) ─────────────────────────
                // Usamos dos hashes de frecuencias muy distintas y los sumamos.
                // Cuando la suma supera _GlitchThreshold el glitch se activa.
                // La diferencia de frecuencias garantiza ráfagas irregulares:
                // algunas cortas y rápidas, otras largas y lentas.
                float burstA = GlitchHash(floor(t * 1.3),  1.7321);  // ~1.3 Hz
                float burstB = GlitchHash(floor(t * 4.7),  0.6931);  // ~4.7 Hz
                float burst  = frac(burstA + burstB);                 // suma caótica

                // step(threshold, x) devuelve 1 si x >= threshold, 0 si no.
                // Cuanto más alto el threshold, más raras las ráfagas.
                g.active = step(_GlitchThreshold, burst);

                // ── 2. Amplitud del offset en este frame ──────────────────────
                // Dentro de cada ráfaga, la amplitud varía con un hash de alta
                // frecuencia para que no sea plana (más orgánico).
                float ampNoise = GlitchHash(t * 17.3, 2.3456);
                // lerp entre 40% y 100% del offset máximo durante la ráfaga
                float amplitude = lerp(0.4, 1.0, ampNoise) * maxOffset;

                g.offsetX = amplitude * g.active;

                // ── 3. Scanline Tear (desplazamiento vertical por banda) ──────
                // Cada banda horizontal del quad (definida por floor(uv.y * N))
                // recibe un desplazamiento Y independiente. Esto simula el
                // "rasgado" de señal analógica donde filas enteras saltan.
                //
                // • floor(uv.y * 8.0) → divide el quad en 8 bandas horizontales
                // • GlitchHash por banda → cada banda tiene su propio offset
                // • * 2.0 - 1.0 → remap [0,1] → [-1,1] para desplazar en ambos sentidos
                float band      = floor(uv.y * 8.0);
                float tearNoise = GlitchHash(floor(t * 6.0) + band * 0.137, 3.7142);
                g.tearY = (tearNoise * 2.0 - 1.0) * _GlitchTearStrength * g.active;

                return g;
            }

            // Muestrea la textura 3 veces con UVs desplazadas para R, G, B
            // y devuelve el color con aberración cromática aplicada.
            // El alpha se toma del canal G (el no desplazado) para referencia.
            half4 SampleWithAberration(float2 uv, GlitchData g)
            {
                // Canal R: desplazado a la derecha en X, con tear en Y
                float2 uvR = uv + float2( g.offsetX, g.tearY);
                // Canal G: solo tear vertical, sin desplazamiento lateral
                float2 uvG = uv + float2( 0.0,       g.tearY * 0.5);
                // Canal B: desplazado a la izquierda (dirección opuesta a R)
                float2 uvB = uv + float2(-g.offsetX,  g.tearY);

                half r = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvR).r;
                half4 gSample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvG);
                half b = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvB).b;

                // Recombinamos: R y B desplazados, G de referencia
                return half4(r, gSample.g, b, gSample.a);
            }

            // ================================================================
            //  VERTEX SHADER
            // ================================================================
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);

                // uvRaw: coordenadas crudas para el rim (antes de tiling)
                OUT.uvRaw = IN.uv;

                // ── Parallax holográfico ──────────────────────────────────────
                // Desplazamos las UVs en dirección opuesta al vector de cámara.
                // "Opuesta" porque cuando la cámara va a la derecha, la capa
                // debe desplazarse a la izquierda para dar sensación de profundidad
                // (igual que un objeto lejano en parallax scrolling clásico).
                //
                // _ParallaxVec.xy: vector cámara en UV space, pasado desde CPU.
                // _LayerDepth: amplificador por capa — capas "más atrás" se mueven más.
                // La negación (-) crea el efecto de profundidad correcto.
                float2 parallaxOffset = -_ParallaxVec.xy * _LayerDepth;
                float2 uvWithParallax = IN.uv + parallaxOffset;

                // Efecto Press: expandimos levemente las UVs desde el centro
                // (aplicado sobre las UVs ya desplazadas por parallax)
                float2 centeredUV = uvWithParallax - 0.5;
                float  pressScale = (_ButtonState >= 2.0)
                                  ? lerp(1.0, _PressUVScale, _StateBlend)
                                  : 1.0;
                float2 scaledUV   = centeredUV * pressScale + 0.5;

                OUT.uv = TRANSFORM_TEX(scaledUV, _MainTex);
                return OUT;
            }

            // ================================================================
            //  FRAGMENT SHADER
            // ================================================================
            half4 frag(Varyings IN) : SV_Target
            {
                // ── 1. Offset máximo según estado ────────────────────────────
                // En Hover el glitch es más intenso: el sistema "reacciona" al
                // contacto. En Press lo suprimimos (el botón se estabiliza).
                float glitchMax;
                if (_ButtonState < 1.5)
                {
                    // Idle → Hover: interpolamos el offset
                    glitchMax = lerp(_GlitchOffsetIdle, _GlitchOffsetHover, _StateBlend);
                }
                else
                {
                    // Hover → Press: suprimimos el glitch al presionar
                    glitchMax = lerp(_GlitchOffsetHover, 0.0, _StateBlend);
                }

                // ── 2. Calcular el glitch y muestrear la textura ─────────────
                // Si glitchMax == 0 (Press completo) usamos el muestreo simple
                // para evitar el costo de las 3 texturas innecesarias.
                GlitchData glitch = ComputeGlitch(IN.uv, glitchMax);
                half4 texColor;
                if (glitch.active > 0.0 && glitchMax > 0.001)
                {
                    // Muestreo con aberración cromática (3 samples)
                    texColor = SampleWithAberration(IN.uv, glitch);
                }
                else
                {
                    // Muestreo normal (1 sample) — la mayoría del tiempo
                    texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                }

                // ── 3. Selección de color según estado ───────────────────────
                half4 activeColor;
                if (_ButtonState < 1.5)
                    activeColor = lerp(_HoloColor, _HoverColor, _StateBlend);
                else
                    activeColor = lerp(_HoverColor, _PressColor, _StateBlend);

                // ── 4. Boost de emisión según estado ─────────────────────────
                float emitBoost = 1.0;
                if (_ButtonState < 1.5)
                {
                    emitBoost = lerp(1.0, _HoverEmitBoost, _StateBlend);
                }
                else
                {
                    float pressFlash = ComputePressFlash(_StateBlend);
                    emitBoost = lerp(_HoverEmitBoost, _PressEmitBoost * pressFlash, _StateBlend);
                }

                // ── 5. Scanlines ─────────────────────────────────────────────
                float scanSpeedMult = lerp(1.0, _HoverScanBoost,
                                          (_ButtonState >= 0.5) ? _StateBlend : 0.0);
                float scanlines = ComputeScanlines(IN.uv, scanSpeedMult);

                // ── 6. Flicker ───────────────────────────────────────────────
                float flicker = ComputeFlicker(_FlickerSpeed);

                // ── 7. Rim ───────────────────────────────────────────────────
                float rimMask    = ComputeRim(IN.uvRaw, _RimWidth);
                float rimBlend   = (_ButtonState >= 0.5) ? _StateBlend : 0.0;
                float rimContrib = rimMask * rimBlend;

                // ── 8. Boost de emisión adicional durante el glitch ──────────
                // Cuando el glitch está activo el brillo sube levemente para
                // simular la sobre-exposición de una señal saturada.
                // 0.3 = +30% de emisión durante la ráfaga (sutil pero legible).
                float glitchEmit = 1.0 + glitch.active * 0.3;

                // ── 9. Composición final ─────────────────────────────────────
                half3 litColor = texColor.rgb * activeColor.rgb;
                litColor += activeColor.rgb * rimContrib;
                litColor *= scanlines;
                litColor *= flicker;
                litColor *= _EmitIntensity * emitBoost * glitchEmit;

                half finalAlpha = texColor.a * activeColor.a * flicker;
                finalAlpha = saturate(finalAlpha + rimContrib * activeColor.a);

                return half4(litColor, finalAlpha);
            }

            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
}
