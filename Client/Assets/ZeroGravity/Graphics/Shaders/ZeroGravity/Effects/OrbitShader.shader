// Orbit lines on the navigation map (and the warp line). The texture scrolls along the line's U axis,
// and the line renderer's vertex colours tint it. Alpha-blended, unlit.
//
// Rebuilt for URP from the original's single unlit pass. Forward-only, as in the original.
Shader "ZeroGravity/Effects/OrbitShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [HDR] _Color ("Color", Color) = (1, 1, 1, 1)
        _Speed ("Speed", Float) = 1
        _AnimationOffset ("Animation Offset", Float) = 0
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
            Name "Unlit"
            Tags { "LightMode" = "UniversalForwardOnly" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex OrbitVertex
            #pragma fragment OrbitFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _Speed;
                float _AnimationOffset;
            CBUFFER_END

            TEXTURE2D(_MainTex);    SAMPLER(sampler_MainTex);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            Varyings OrbitVertex(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;
                return output;
            }

            float4 OrbitFragment(Varyings input) : SV_Target
            {
                // _Time.x is time / 20.
                float2 uv = input.uv;
                uv.x -= (_AnimationOffset + _Time.x) * _Speed;

                float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) * _Color;
                return color * input.color;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
