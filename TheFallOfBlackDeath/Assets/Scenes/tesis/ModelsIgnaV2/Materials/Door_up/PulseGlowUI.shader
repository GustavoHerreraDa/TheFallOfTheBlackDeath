Shader "Custom/UI/PulseGlowUI"
{
    Properties
    {
        [PerRendererData] _MainTex   ("Sprite Texture", 2D) = "white" {}
        _Color              ("Tint",            Color)  = (1,1,1,1)

        _PulseSpeed         ("Pulse Speed",     Float)  = 1.2
        _PulseMinAlpha      ("Pulse Min Alpha", Range(0,1)) = 0.55
        _PulseMaxAlpha      ("Pulse Max Alpha", Range(0,1)) = 1.0

        _GlowColor          ("Glow Color",      Color)  = (0.6,0.2,1.0,1.0)
        _GlowIntensity      ("Glow Intensity",  Float)  = 2.0
        _GlowRadius         ("Glow Softness",   Range(0,1)) = 0.35
        _PulseThreshold     ("Glow Threshold",  Range(0,1)) = 0.85

        _FlickerSpeed       ("Flicker Speed",   Float)  = 8.0
        _FlickerAmount      ("Flicker Amount",  Range(0,0.3)) = 0.08

        _ForcePulse         ("Force Pulse t",   Range(0,1)) = 0.0

        [HideInInspector] _StencilComp      ("Stencil Comparison",  Float) = 8
        [HideInInspector] _Stencil          ("Stencil ID",          Float) = 0
        [HideInInspector] _StencilOp        ("Stencil Operation",   Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask",  Float) = 255
        [HideInInspector] _StencilReadMask  ("Stencil Read Mask",   Float) = 255
        [HideInInspector] _ColorMask        ("Color Mask",          Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"             = "Transparent"
            "IgnoreProjector"   = "True"
            "RenderType"        = "Transparent"
            "PreviewType"       = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref       [_Stencil]
            Comp      [_StencilComp]
            Pass      [_StencilOp]
            ReadMask  [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull     Off
        Lighting Off
        ZWrite   Off
        ZTest    [unity_GUIZTestMode]
        Blend    SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "PulseGlow"

            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            sampler2D _MainTex;

            fixed4  _Color;
            fixed4  _GlowColor;
            float4  _ClipRect;

            float   _PulseSpeed;
            fixed   _PulseMinAlpha;
            fixed   _PulseMaxAlpha;

            fixed   _GlowIntensity;
            fixed   _GlowRadius;
            fixed   _PulseThreshold;

            float   _FlickerSpeed;
            fixed   _FlickerAmount;

            fixed   _ForcePulse;

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex      : SV_POSITION;
                fixed4 color       : COLOR;
                float2 texcoord    : TEXCOORD0;
                float4 worldPos    : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPos    = v.vertex;
                OUT.vertex      = UnityObjectToClipPos(v.vertex);
                OUT.texcoord    = v.texcoord;
                OUT.color       = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 col = tex2D(_MainTex, IN.texcoord) * IN.color;

                // --- Pulso ---
                float pulseSin = sin(_Time.y * _PulseSpeed * UNITY_TWO_PI) * 0.5 + 0.5;
                float pulseT   = max(pulseSin, (float)_ForcePulse);

                // Flicker cerca del peak
                float aboveThr = saturate((pulseT - _PulseThreshold) / (1.0 - _PulseThreshold + 0.001));
                float flicker  = sin(_Time.y * _FlickerSpeed * UNITY_TWO_PI) * _FlickerAmount * aboveThr;
                pulseT = saturate(pulseT + flicker);

                // Alpha modulado
                fixed alphaRange = _PulseMaxAlpha - _PulseMinAlpha;
                fixed pulseAlpha = _PulseMinAlpha + alphaRange * pulseT;

                // --- Glow additive ---
                float glowFactor = saturate((pulseT - _PulseThreshold) / max(0.001, (float)_GlowRadius));
                col.rgb = saturate(col.rgb + _GlowColor.rgb * _GlowIntensity * glowFactor * col.a);

                col.a *= pulseAlpha;

                // --- Canvas clip rect ---
                #ifdef UNITY_UI_CLIP_RECT
                    col.a *= UnityGet2DClipping(IN.worldPos.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                    clip(col.a - 0.001);
                #endif

                return col;
            }
            ENDCG
        }
    }
}
