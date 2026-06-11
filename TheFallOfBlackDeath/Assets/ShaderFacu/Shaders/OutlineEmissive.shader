Shader "Custom/OutlineEmissive"
{
    Properties
    {
        // --- Material principal ---
        _BaseColor          ("Base Color",          Color)  = (1,1,1,1)
        _BaseMap            ("Base Texture",         2D)    = "white" {}

        // --- Outline ---
        [HDR]
        _OutlineColor       ("Outline Color",        Color)  = (0,1,1,1)
        _OutlineWidth       ("Outline Width",        Float)  = 0.02
        _EmissionStrength   ("Emission Strength",    Range(0,8)) = 2.0
        _OutlineZOffset     ("Outline Z Offset",     Float)  = 0.0001
    }

    SubShader
    {
        Tags
        {
            "RenderType"  = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue"       = "Geometry"
        }

        // ─────────────────────────────────────────────────────
        // PASS 1 – Objeto original (lit, Forward URP)
        // ─────────────────────────────────────────────────────
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex   vert_base
            #pragma fragment frag_base
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _BaseMap_ST;
                float4 _OutlineColor;
                float  _OutlineWidth;
                float  _EmissionStrength;
                float  _OutlineZOffset;
            CBUFFER_END

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float3 positionWS  : TEXCOORD2;
                float  fogFactor   : TEXCOORD3;
            };

            Varyings vert_base(Attributes IN)
            {
                Varyings OUT;
                OUT.positionWS  = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformWorldToHClip(OUT.positionWS);
                OUT.normalWS    = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv          = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.fogFactor   = ComputeFogFactor(OUT.positionHCS.z);
                return OUT;
            }

            half4 frag_base(Varyings IN) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                half4 color    = texColor * _BaseColor;

                // Luz principal básica (diffuse)
                InputData  lightInput  = (InputData)0;
                lightInput.positionWS  = IN.positionWS;
                lightInput.normalWS    = normalize(IN.normalWS);
                lightInput.viewDirectionWS = GetWorldSpaceNormalizeViewDir(IN.positionWS);
                lightInput.shadowCoord = TransformWorldToShadowCoord(IN.positionWS);

                SurfaceData surface = (SurfaceData)0;
                surface.albedo      = color.rgb;
                surface.alpha       = color.a;

                half4 litColor = UniversalFragmentPBR(lightInput, surface);
                litColor.rgb   = MixFog(litColor.rgb, IN.fogFactor);
                return litColor;
            }
            ENDHLSL
        }

        // ─────────────────────────────────────────────────────
        // PASS 2 – Invert Hull Outline (emisivo)
        // ─────────────────────────────────────────────────────
        Pass
        {
            Name "OutlineEmissive"
            // No usa LightMode estándar → se fuerza con RenderObjects (ver notas)
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Cull  Front   // ← clave de la técnica: descarta caras exteriores
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex   vert_outline
            #pragma fragment frag_outline

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _BaseMap_ST;
                float4 _OutlineColor;
                float  _OutlineWidth;
                float  _EmissionStrength;
                float  _OutlineZOffset;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            Varyings vert_outline(Attributes IN)
            {
                Varyings OUT;

                // 1. Convertir a world space
                float3 posWS    = TransformObjectToWorld(IN.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);

                // 2. Expandir el mesh a lo largo de la normal (world units)
                posWS += normalize(normalWS) * _OutlineWidth;

                // 3. Pequeño Z offset para evitar z-fighting en meshes planos
                float4 posHCS    = TransformWorldToHClip(posWS);
                posHCS.z        -= _OutlineZOffset * posHCS.w;
                OUT.positionHCS  = posHCS;

                return OUT;
            }

            half4 frag_outline(Varyings IN) : SV_Target
            {
                // Color HDR: la intensidad extra activa el Bloom en post-process
                half3 emission = _OutlineColor.rgb * _EmissionStrength;
                return half4(emission, _OutlineColor.a);
            }
            ENDHLSL
        }

        // Shadow caster (necesario para que el objeto proyecte sombras)
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }

    FallBack "Universal Render Pipeline/Lit"
    CustomEditor "UnityEditor.ShaderGUI"
}
