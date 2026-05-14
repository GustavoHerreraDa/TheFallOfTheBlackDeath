Shader "UI/CRT_Overlay_Improved"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.5
        _ScanlineCount ("Scanline Count", Float) = 800
        _ScanlineSpeed ("Scanline Speed", Float) = 2.0
        
        _VignetteAmount ("Vignette Amount", Range(0, 2)) = 1.0
        _Curvature ("Screen Curvature", Range(0, 0.5)) = 0.05
        
        _ChromaticAberration ("Chromatic Aberration", Range(0, 0.05)) = 0.005
        _NoiseIntensity ("Noise Intensity", Range(0, 0.1)) = 0.02
        _Flicker ("Flicker Intensity", Range(0, 0.1)) = 0.03

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            float _ScanlineIntensity, _ScanlineCount, _ScanlineSpeed;
            float _VignetteAmount, _Curvature;
            float _ChromaticAberration, _NoiseIntensity, _Flicker;

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(o.worldPosition);

                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                o.color = v.color * _Color;
                return o;
            }

            sampler2D _MainTex;

            float2 curve(float2 uv)
            {
                uv = uv * 2.0 - 1.0;
                float2 offset = abs(uv.yx) / 2.5;
                uv = uv + uv * offset * offset * _Curvature;
                uv = uv * 0.5 + 0.5;
                return uv;
            }

            float rand(float2 co)
            {
                return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = curve(i.texcoord);
                
                // Bordes de la curvatura
                if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
                    return fixed4(0,0,0,0);

                // Chromatic Aberration
                float2 uvR = uv + float2(_ChromaticAberration, 0);
                float2 uvG = uv;
                float2 uvB = uv - float2(_ChromaticAberration, 0);

                fixed4 col;
                col.r = (tex2D(_MainTex, uvR) + _TextureSampleAdd).r;
                col.g = (tex2D(_MainTex, uvG) + _TextureSampleAdd).g;
                col.b = (tex2D(_MainTex, uvB) + _TextureSampleAdd).b;
                col.a = tex2D(_MainTex, uv).a;

                col *= i.color;

                // Noise y Flicker
                float noise = rand(uv + _Time.y) * _NoiseIntensity;
                float flicker = sin(_Time.y * 50.0) * _Flicker;
                col.rgb += noise + flicker;

                // Scanlines animadas
                float scanline = sin((uv.y + _Time.y * _ScanlineSpeed * 0.01) * _ScanlineCount) * _ScanlineIntensity;
                col.rgb -= scanline;

                // Vignette
                float vignette = uv.x * uv.y * (1.0 - uv.x) * (1.0 - uv.y);
                vignette = clamp(pow(16.0 * vignette, _VignetteAmount), 0.0, 1.0);
                col.rgb *= vignette;

                #ifdef UNITY_UI_ALPHACLIP
                clip (col.a - 0.001);
                #endif

                // Aplicar clipping rectangular de UI
                col.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);

                return col;
            }
            ENDCG
        }
    }
}