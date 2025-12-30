Shader "UI/GlitchImage"
{
    Properties
    {
        _MainTex("Sprite", 2D) = "white" {}
        _GlitchStrength("Glitch Strength", Range(0,1)) = 0
        _GlitchSpeed("Glitch Speed", Float) = 10
    }

        SubShader
        {
            Tags
            {
                "Queue" = "Transparent"
                "IgnoreProjector" = "True"
                "RenderType" = "Transparent"
                "PreviewType" = "Plane"
                "CanUseSpriteAtlas" = "True"
            }

            Cull Off
            Lighting Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                sampler2D _MainTex;
                float4 _MainTex_ST;
                float _GlitchStrength;
                float _GlitchSpeed;

                struct appdata_t
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    float2 uv : TEXCOORD0;
                };

                v2f vert(appdata_t v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    return o;
                }

                float rand(float2 co)
                {
                    return frac(sin(dot(co.xy, float2(12.9898,78.233))) * 43758.5453);
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    float glitch = rand(float2(_Time.y * _GlitchSpeed, i.uv.y));
                    float offset = (glitch - 0.5) * _GlitchStrength * 0.1;

                    float2 uv = i.uv;
                    uv.x += offset;

                    fixed4 col = tex2D(_MainTex, uv);
                    return col;
                }
                ENDCG
            }
        }
}
