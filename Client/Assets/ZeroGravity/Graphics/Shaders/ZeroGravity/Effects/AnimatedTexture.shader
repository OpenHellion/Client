// Flipbook animation for electric sparks and currents (Current_MAT, Spark_Mat) and thruster flames
// (ThrustersMat). _MainTex is a sheet of frames, played _Speed times per 20 seconds (_Time.x) and
// looped. The sheet's red channel is the frame's shape: it becomes the alpha, and the colour is
// _Color alone. _Brightness scales both, so above 1 it also makes the edges more opaque.
//
// Despite their names, _Rows is the number of frames across the sheet and _Columns the number
// down it. Frames are played left to right, starting from the top row.
//
// Rebuilt for URP from the original's single unlit pass. It is forward-only, alpha blended and
// has no fog, like the original.
Shader "ZeroGravity/Effects/AnimatedTexture"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Rows ("Number of rows", Float) = 1
        _Columns ("Number of columns", Float) = 1
        _Speed ("Speed", Float) = 1
        _Brightness ("Brightness", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }
        LOD 100

        Pass
        {
            Name "AnimatedTexture"
            Tags { "LightMode" = "UniversalForwardOnly" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex AnimatedTextureVertex
            #pragma fragment AnimatedTextureFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _Rows;
                float _Columns;
                float _Speed;
                float _Brightness;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // Moves uv (0..1 over the whole sheet) into the frame that is showing now.
            float2 GetFrameUV(float2 uv)
            {
                float frameCount = _Columns * _Rows;
                float frame = round(frac(_Speed * _Time.x) * frameCount);

                float frameX = fmod(frame, _Rows);
                float frameY = floor(frame / _Rows);

                // The first row is at the top of the texture. Its offset of 1 lands on the bottom
                // row's place plus one whole sheet, which the texture's wrap mode brings back to the top.
                float2 frameOffset = float2(frameX / _Rows, 1.0 - frameY / _Columns);
                return uv / float2(_Rows, _Columns) + frameOffset;
            }

            Varyings AnimatedTextureVertex(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            float4 AnimatedTextureFragment(Varyings input) : SV_Target
            {
                float shape = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, GetFrameUV(input.uv)).r;

                float alpha = saturate(shape * _Color.a);
                float3 color = saturate(_Color.rgb);
                return float4(color, alpha) * _Brightness;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
