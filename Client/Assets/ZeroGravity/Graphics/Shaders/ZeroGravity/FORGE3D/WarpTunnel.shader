// FORGE3D warp tunnel: a scrolling texture tinted with _node_6523, drawn additively on both sides
// of the tunnel mesh. Unlit.
//
// Rebuilt for URP from the Shader Forge original. Forward-only, as in the original.
// Left out of the original:
//  * Shader Forge's _TimeEditor offset, which only its editor sets (0 in the game).
//  * The ShadowCaster from Fallback "Diffuse". The tunnel is on the Planets layer, which no
//    shadow-casting light includes in its culling mask.
Shader "FORGE3D/WarpTunnel"
{
    Properties
    {
        _color ("color", 2D) = "white" {}
        _U_TileAnimFactor ("U_TileAnimFactor", Range(-5, 5)) = 0
        _V_TileAnimFactor ("V_TileAnimFactor", Range(-5, 5)) = 0
        _Opacity ("Opacity", Range(0, 1)) = 0
        _node_6523 ("node_6523", Color) = (0.5, 0.5, 0.5, 1)
        [HideInInspector] _Cutoff ("Alpha cutoff", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
        }
        LOD 200

        Pass
        {
            Name "Unlit"
            Tags { "LightMode" = "UniversalForwardOnly" }

            Cull Off
            ZWrite Off
            Blend One One

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex WarpTunnelVertex
            #pragma fragment WarpTunnelFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _color_ST;
                float4 _node_6523;
                float _U_TileAnimFactor;
                float _V_TileAnimFactor;
                float _Opacity;
                float _Cutoff;
            CBUFFER_END

            TEXTURE2D(_color);  SAMPLER(sampler_color);

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

            Varyings WarpTunnelVertex(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            float4 WarpTunnelFragment(Varyings input) : SV_Target
            {
                float2 uv = _Time.y * float2(_U_TileAnimFactor, _V_TileAnimFactor) + input.uv;
                float3 color = SAMPLE_TEXTURE2D(_color, sampler_color, TRANSFORM_TEX(uv, _color)).rgb;
                // Blend One One ignores the alpha.
                return float4(color * _node_6523.rgb, _Opacity);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
