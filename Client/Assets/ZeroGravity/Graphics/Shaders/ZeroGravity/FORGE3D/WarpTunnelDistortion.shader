// FORGE3D warp tunnel distortion: redraws what is behind the tunnel mesh, offset by a scrolling
// refraction map (red x alpha and green, remapped to -1..1, times _RefractionFactor), and darkened
// towards black by _Opacity. Drawn on both sides, replacing the colour behind it.
//
// Rebuilt for URP from the Shader Forge original, which read the screen through a GrabPass. URP
// reads the camera's opaque texture instead. PlanetsCamera, which draws this, is an overlay camera,
// so it gets that texture from OverlayOpaqueTextureFeature on its renderer. Transparent objects
// drawn before this (the warp tunnel itself) aren't in the texture, so this pass draws over them;
// its material's queue (2999) puts it before the tunnel.
// Left out of the original:
//  * Shader Forge's _TimeEditor offset, which only its editor sets (0 in the game).
//  * The ShadowCaster from Fallback "Diffuse". The tunnel is on the Planets layer, which no
//    shadow-casting light includes in its culling mask.
Shader "FORGE3D/WarpTunnelDistortion"
{
    Properties
    {
        _Refraction ("Refraction", 2D) = "" {}
        _Opacity ("Opacity", Range(0, 1)) = 0
        _RefractionFactor ("Refraction Factor", Range(0, 2)) = 0
        _U_TileAnimFactor ("U_TileAnimFactor", Range(-5, 5)) = 0
        _V_TileAnimFactor ("V_TileAnimFactor", Range(-5, 5)) = 0
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

        Pass
        {
            Name "Unlit"
            Tags { "LightMode" = "UniversalForwardOnly" }

            Cull Off
            ZWrite Off

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex WarpTunnelDistortionVertex
            #pragma fragment WarpTunnelDistortionFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Refraction_ST;
                float _Opacity;
                float _RefractionFactor;
                float _U_TileAnimFactor;
                float _V_TileAnimFactor;
                float _Cutoff;
            CBUFFER_END

            TEXTURE2D(_Refraction);     SAMPLER(sampler_Refraction);

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

            Varyings WarpTunnelDistortionVertex(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            float4 WarpTunnelDistortionFragment(Varyings input) : SV_Target
            {
                float2 uv = float2(_U_TileAnimFactor, _V_TileAnimFactor) * _Time.y + input.uv;
                float4 refraction = SAMPLE_TEXTURE2D(_Refraction, sampler_Refraction, TRANSFORM_TEX(uv, _Refraction));
                float2 offset = float2(refraction.r * refraction.a, refraction.g) * 2.0 - 1.0;

                float2 screenUV = offset * _RefractionFactor + GetNormalizedScreenSpaceUV(input.positionCS);
                float3 sceneColor = SampleSceneColor(screenUV);
                return float4(lerp(sceneColor, 0.0, _Opacity), 1.0);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
