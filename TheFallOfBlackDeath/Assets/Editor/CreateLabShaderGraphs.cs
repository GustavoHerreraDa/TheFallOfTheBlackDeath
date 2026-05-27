using UnityEditor;
using UnityEngine;
using System.IO;

public static class CreateLabShaders
{
    [MenuItem("Tools/Create Underground Lab Shaders")]
    public static void CreateShaders()
    {
        CreateFolder();

        CreateMonitorShader();
        CreateTankShader();
        CreateServerShader();

        AssetDatabase.Refresh();

        Debug.Log(
            "Shaders created in Assets/ShaderJose/"
        );
    }

    static void CreateFolder()
    {
        if (
            !AssetDatabase.IsValidFolder(
                "Assets/ShaderJose"
            )
        )
        {
            AssetDatabase.CreateFolder(
                "Assets",
                "ShaderJose"
            );
        }
    }

    static void CreateMonitorShader()
    {
        string path =
            "Assets/ShaderJose/MonitorFlicker.shader";

        string shader =
@"Shader ""Custom/MonitorFlicker""
{
    Properties
    {
        _BaseMap (""Texture"", 2D) = ""white"" {}
        _FlickerSpeed (""Flicker Speed"", Float) = 8
        _Intensity (""Intensity"", Float) = 0.25
        _EnableFlicker (""Enable Flicker"", Float) = 1
    }

    SubShader
    {
        Tags
        {
            ""RenderPipeline""=""UniversalPipeline""
            ""RenderType""=""Opaque""
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include ""Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl""

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            float _FlickerSpeed;
            float _Intensity;
            float _EnableFlicker;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(
                Attributes IN
            )
            {
                Varyings OUT;

                OUT.positionHCS =
                    TransformObjectToHClip(
                        IN.positionOS.xyz
                    );

                OUT.uv =
                    IN.uv;

                return OUT;
            }

            half4 frag(
                Varyings IN
            ) : SV_Target
            {
                half4 col =
                    SAMPLE_TEXTURE2D(
                        _BaseMap,
                        sampler_BaseMap,
                        IN.uv
                    );

                float flicker =
                    1 +
                    sin(
                        _Time.y *
                        _FlickerSpeed
                    ) *
                    _Intensity;

                float finalValue =
                    lerp(
                        1,
                        flicker,
                        _EnableFlicker
                    );

                col.rgb *=
                    finalValue;

                return col;
            }
            ENDHLSL
        }
    }
}";
        File.WriteAllText(
            path,
            shader
        );
    }

    static void CreateTankShader()
    {
        string path =
            "Assets/ShaderJose/TankBubbling.shader";

        string shader =
@"Shader ""Custom/TankBubbling""
{
    Properties
    {
        _Tint (""Tint"", Color) =
        (0,0.8,1,0.35)
    }

    SubShader
    {
        Tags
        {
            ""RenderPipeline""=""UniversalPipeline""
            ""Queue""=""Transparent""
        }

        Blend
            SrcAlpha
            OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include ""Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl""

            float4 _Tint;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(
                Attributes IN
            )
            {
                Varyings OUT;

                OUT.positionHCS =
                    TransformObjectToHClip(
                        IN.positionOS.xyz
                    );

                OUT.uv =
                    IN.uv;

                return OUT;
            }

            half4 frag(
                Varyings IN
            ) : SV_Target
            {
                float n =
                    frac(
                        sin(
                            dot(
                                IN.uv *
                                _Time.y,
                                float2(
                                    12.98,
                                    78.23
                                )
                            )
                        )
                        * 43758
                    );

                float bubble =
                    step(
                        0.97,
                        n
                    );

                return
                    half4(
                        _Tint.rgb
                        + bubble,
                        _Tint.a
                    );
            }

            ENDHLSL
        }
    }
}";
        File.WriteAllText(
            path,
            shader
        );
    }

    static void CreateServerShader()
    {
        string path =
            "Assets/ShaderJose/ServerElectricity.shader";

        string shader =
@"Shader ""Custom/ServerElectricity""
{
    Properties
    {
        _GlowColor
        (
            ""Glow Color"",
            Color
        ) =
        (0.3,0.8,1,1)

        _PulseSpeed
        (
            ""Pulse Speed"",
            Float
        ) = 12
    }

    SubShader
    {
        Tags
        {
            ""RenderPipeline""=""UniversalPipeline""
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include ""Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl""

            float4 _GlowColor;
            float _PulseSpeed;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(
                Attributes IN
            )
            {
                Varyings OUT;

                OUT.positionHCS =
                    TransformObjectToHClip(
                        IN.positionOS.xyz
                    );

                OUT.uv =
                    IN.uv;

                return OUT;
            }

            half4 frag(
                Varyings IN
            ) : SV_Target
            {
                float pulse =
                    abs(
                        sin(
                            _Time.y *
                            _PulseSpeed
                            +
                            IN.uv.x *
                            20
                        )
                    );

                return
                    _GlowColor
                    *
                    pulse;
            }

            ENDHLSL
        }
    }
}";
        File.WriteAllText(
            path,
            shader
        );
    }
}