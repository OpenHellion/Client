// Glowing rim around a celestial body on the navigation map. Additive and unlit: a Fresnel term
// (1 - N.V)^5 scaled by _Float0 and tinted with _Color.
//
// Rebuilt for URP from the Amplify Shader Editor surface shader of the same name. Its surface is
// black with this emission, and its FORWARDADD pass adds nothing, so it is unlit and forward-only.
// The original's ShadowCaster is left out: the map's layer is outside every shadow-casting light's
// culling mask.
Shader "ZeroGravity/Effects/PlanetHighlight"
{
    Properties
    {
        [HideInInspector] __dirty ("", Float) = 1
        _MaskClipValue ("Mask Clip Value", Float) = 0.5
        [HDR] _Color ("Color", Color) = (0.6387868, 0.6890086, 1, 0)
        _Float0 ("Float 0", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Overlay"
            "Queue" = "Overlay+0"
            "IsEmissive" = "true"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _Color;
            float _Float0;
            float _MaskClipValue;
            float __dirty;
        CBUFFER_END

        float3 GetRimColor(float3 positionWS, float3 normalWS)
        {
            float3 viewDirWS = normalize(_WorldSpaceCameraPos - positionWS);
            float rim = 1.0 - dot(normalWS, viewDirWS);
            float rim2 = rim * rim;
            float fresnel = rim2 * rim2 * rim;
            return (fresnel * _Float0) * _Color.rgb;
        }
        ENDHLSL

        Pass
        {
            Name "Unlit"
            Tags { "LightMode" = "UniversalForwardOnly" }

            Blend One One

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex PlanetHighlightVertex
            #pragma fragment PlanetHighlightFragment

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
            };

            Varyings PlanetHighlightVertex(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                // Normalized per vertex only, as in the original.
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            float4 PlanetHighlightFragment(Varyings input) : SV_Target
            {
                return float4(GetRimColor(input.positionWS, input.normalWS), 1.0);
            }
            ENDHLSL
        }

        // Emission for the lightmapper; the albedo is black.
        Pass
        {
            Name "Meta"
            Tags { "LightMode" = "Meta" }

            Cull Off

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex MetaPassVertex
            #pragma fragment MetaPassFragment
            #pragma shader_feature EDITOR_VISUALIZATION

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"

            struct MetaAttributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
            };

            struct MetaVaryings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
            #ifdef EDITOR_VISUALIZATION
                float2 VizUV : TEXCOORD2;
                float4 LightCoord : TEXCOORD3;
            #endif
            };

            MetaVaryings MetaPassVertex(MetaAttributes input)
            {
                MetaVaryings output = (MetaVaryings)0;
                output.positionCS = UnityMetaVertexPosition(input.positionOS.xyz, input.uv1, input.uv2);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
            #ifdef EDITOR_VISUALIZATION
                UnityEditorVizData(input.positionOS.xyz, input.uv0, input.uv1, input.uv2, output.VizUV, output.LightCoord);
            #endif
                return output;
            }

            half4 MetaPassFragment(MetaVaryings input) : SV_Target
            {
                MetaInput metaInput = (MetaInput)0;
                metaInput.Albedo = 0.0;
                metaInput.Emission = GetRimColor(input.positionWS, input.normalWS);
            #ifdef EDITOR_VISUALIZATION
                metaInput.VizUV = input.VizUV;
                metaInput.LightCoord = input.LightCoord;
            #endif
                return UnityMetaFragment(metaInput);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
