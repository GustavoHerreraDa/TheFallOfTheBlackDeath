// ============================================================
//  HoloButton_Diegetic.shader  — v2 con feedback Hover/Press
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
            //  VERTEX SHADER
            // ================================================================
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);

                // uvRaw: coordenadas crudas para el rim (antes de tiling)
                OUT.uvRaw = IN.uv;

                // Efecto Press: expandimos levemente las UVs desde el centro
                // para dar sensación de "empuje" en el quad
                float2 centeredUV = IN.uv - 0.5;
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
                // ── 1. Textura base ──────────────────────────────────────────
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                // ── 2. Selección de color según estado ───────────────────────
                // _ButtonState: 0=Idle, 1=Hover, 2=Press
                // _StateBlend:  valor 0→1 interpolado en CPU por el script
                //
                // Encadenamos dos lerps para cubrir las tres transiciones:
                //   Idle→Hover con blend cuando state=1
                //   Hover→Press con blend cuando state=2
                half4 activeColor;
                if (_ButtonState < 1.5)
                {
                    // Transición Idle ↔ Hover
                    activeColor = lerp(_HoloColor, _HoverColor, _StateBlend);
                }
                else
                {
                    // Transición Hover ↔ Press
                    activeColor = lerp(_HoverColor, _PressColor, _StateBlend);
                }

                // ── 3. Boost de emisión según estado ─────────────────────────
                // Hover: más brillo. Press: destello inicial luego más oscuro.
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

                // ── 4. Scanlines (más rápidas en hover) ──────────────────────
                float scanSpeedMult = lerp(1.0, _HoverScanBoost,
                                          (_ButtonState >= 0.5) ? _StateBlend : 0.0);
                float scanlines = ComputeScanlines(IN.uv, scanSpeedMult);

                // ── 5. Flicker ───────────────────────────────────────────────
                float flicker = ComputeFlicker(_FlickerSpeed);

                // ── 6. Rim de energía (solo en Hover y Press) ────────────────
                float rimMask    = ComputeRim(IN.uvRaw, _RimWidth);
                float rimBlend   = (_ButtonState >= 0.5) ? _StateBlend : 0.0;
                float rimContrib = rimMask * rimBlend;

                // ── 7. Composición final ─────────────────────────────────────
                half3 litColor = texColor.rgb * activeColor.rgb;
                litColor += activeColor.rgb * rimContrib;   // añade el rim encima
                litColor *= scanlines;
                litColor *= flicker;
                litColor *= _EmitIntensity * emitBoost;

                half finalAlpha = texColor.a * activeColor.a * flicker;
                finalAlpha = saturate(finalAlpha + rimContrib * activeColor.a);

                return half4(litColor, finalAlpha);
            }

            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
}
