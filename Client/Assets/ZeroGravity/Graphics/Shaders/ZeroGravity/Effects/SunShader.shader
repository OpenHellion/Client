// The distant sun seen from space: an unlit, camera-facing plane drawn additively. A white core that
// fades out towards the rim, plus a tinted glow texture and star-flare texture on top.
//
// The property names are the original Shader Forge ones (including the "_GlowTexure" typo), because
// SunMaterial stores its values under them. Only the display names are new.
Shader "ZeroGravity/Effects/SunShader"
{
    Properties
    {
        _StarTexture ("Star Texture", 2D) = "black" {}
        _GlowTexure ("Glow Texture", 2D) = "black" {}
        // The core is measured as closeness to the centre: 1 - (distance from the UV centre / 0.5).
        // It is 0 at or below the start and fully bright at or above the end.
        _node_4639 ("Core Falloff Start", Range(0, 1)) = 0
        _node_4131 ("Core Falloff End", Range(0, 1)) = 1
        _node_8031 ("Core Falloff Exponent", Float) = 1
        _node_2310 ("Glow Exponent", Float) = 1
        _node_6920 ("Glow and Star Tint", Color) = (0.5, 0.5, 0.5, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }
        LOD 100

        Pass
        {
            Name "Unlit"
            // Unlit, so it can't be lit through the G-buffer: forward-only is drawn by both the Forward
            // and the Deferred renderer, whatever queue the material is in.
            Tags { "LightMode" = "UniversalForwardOnly" }

            Blend One One
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex SunVertex
            #pragma fragment SunFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_StarTexture);
            SAMPLER(sampler_StarTexture);
            TEXTURE2D(_GlowTexure);
            SAMPLER(sampler_GlowTexure);

            CBUFFER_START(UnityPerMaterial)
                float4 _StarTexture_ST;
                float4 _GlowTexure_ST;
                float _node_4639;   // core falloff start
                float _node_4131;   // core falloff end
                float _node_8031;   // core falloff exponent
                float _node_2310;   // glow exponent
                float4 _node_6920;  // glow and star tint
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

            Varyings SunVertex(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            float4 SunFragment(Varyings input) : SV_Target
            {
                float coreFalloffStart = _node_4639;
                float coreFalloffEnd = _node_4131;
                float coreFalloffExponent = _node_8031;
                float glowExponent = _node_2310;
                float3 tint = _node_6920.rgb;

                float closenessToCentre = 1.0 - length(input.uv * 2.0 - 1.0);
                float core = saturate((closenessToCentre - coreFalloffStart) / (coreFalloffEnd - coreFalloffStart));
                core = pow(core, coreFalloffExponent);

                float2 glowUV = TRANSFORM_TEX(input.uv, _GlowTexure);
                float3 glowSample = SAMPLE_TEXTURE2D(_GlowTexure, sampler_GlowTexure, glowUV).rgb;
                float3 glow = pow(max(glowSample, 0.0), glowExponent);
                float2 starUV = TRANSFORM_TEX(input.uv, _StarTexture);
                float3 star = SAMPLE_TEXTURE2D(_StarTexture, sampler_StarTexture, starUV).rgb;

                return float4(tint * (glow + star) + core, 1.0);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
