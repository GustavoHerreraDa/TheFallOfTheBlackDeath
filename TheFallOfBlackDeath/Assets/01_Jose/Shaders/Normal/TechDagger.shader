Shader "Custom/TechDagger"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.08,0.08,0.08,1)
        _EnergyColor ("Energy Color", Color) = (1,0,0,1)

        _Emission ("Emission", Range(0,50)) = 15

        _GridScale ("Grid Scale", Range(1,100)) = 30
        _GridThickness ("Grid Thickness", Range(0.001,0.1)) = 0.02

        _FlowSpeed ("Flow Speed", Range(0,20)) = 5

        _Direction ("Direction", Vector) = (0,0,1,0)

        _GridHeight ("Grid Height", Range(0,1)) = 0.25
        _GridFade ("Grid Fade", Range(0.01,1)) = 0.1

        _CavityPower ("Cavity Power", Range(1,20)) = 8
        _CavityBoost ("Cavity Boost", Range(0,10)) = 3

        _RimPower ("Rim Power", Range(0.1,10)) = 4
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float3 localPos : TEXCOORD3;
            };

            float4 _BaseColor;
            float4 _EnergyColor;

            float _Emission;

            float _GridScale;
            float _GridThickness;

            float _FlowSpeed;

            float4 _Direction;

            float _GridHeight;
            float _GridFade;

            float _CavityPower;
            float _CavityBoost;

            float _RimPower;

            v2f vert(appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                o.worldNormal =
                    UnityObjectToWorldNormal(v.normal);

                o.worldPos =
                    mul(unity_ObjectToWorld, v.vertex).xyz;

                o.localPos =
                    v.vertex.xyz;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 N =
                    normalize(i.worldNormal);

                float3 V =
                    normalize(
                        _WorldSpaceCameraPos -
                        i.worldPos
                    );

                float3 dir =
                    normalize(_Direction.xyz);

                float projected =
                    dot(i.localPos, dir);

                float flow =
                    sin(
                        projected * 20 +
                        _Time.y * _FlowSpeed
                    );

                flow =
                    flow * 0.5 + 0.5;

                //----------------------------------
                // GRID
                //----------------------------------

                float gx =
                    abs(frac(i.uv.x * _GridScale) - 0.5);

                float gy =
                    abs(frac(i.uv.y * _GridScale) - 0.5);

                float grid =
                    step(gx, _GridThickness) +
                    step(gy, _GridThickness);

                grid =
                    saturate(grid);

                //----------------------------------
                // LOWER PART MASK
                //----------------------------------

                float lowerMask =
                    1.0 -
                    smoothstep(
                        _GridHeight,
                        _GridHeight + _GridFade,
                        i.uv.y
                    );

                //----------------------------------
                // INDENTATION DETECTION
                //----------------------------------

                float cavity =
                    pow(
                        1.0 - abs(N.y),
                        _CavityPower
                    );

                cavity *= _CavityBoost;

                //----------------------------------
                // RIM LIGHT
                //----------------------------------

                float rim =
                    pow(
                        1.0 -
                        saturate(dot(N, V)),
                        _RimPower
                    );

                //----------------------------------
                // ENERGY CHANNELS
                //----------------------------------

                float channels =
                    max(
                        cavity,
                        rim * 0.5
                    );

                //----------------------------------
                // LOWER GRID
                //----------------------------------

                float lowerGrid =
                    grid *
                    lowerMask *
                    flow;

                //----------------------------------
                // FINAL ENERGY
                //----------------------------------

                float energy =
                    max(
                        channels,
                        lowerGrid
                    );

                float3 finalColor =
                    _BaseColor.rgb +
                    _EnergyColor.rgb *
                    energy *
                    _Emission;

                return float4(finalColor, 1);
            }

            ENDCG
        }
    }
}