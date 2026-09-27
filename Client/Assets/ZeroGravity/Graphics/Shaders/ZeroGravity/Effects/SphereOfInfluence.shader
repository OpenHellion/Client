// Sphere of influence around a celestial body on the navigation map. Drawn additively from the
// inside (front faces culled) and unlit. Three layers make up the colour:
//  * the texture's red x blue, tinted with _Color0,
//  * the texture's green, tinted with _Color2,
//  * a rim, (1 - N.V)^5 against the inward-facing normal, tinted with _Color1.
// Bands of |sin| that scroll along V over time modulate them: raised to _Float0 for the first layer
// and the rim, to _Float1 for the second. _Appear (clamped to 0..1) and _Fade scale the result.
//
// Rebuilt for URP from the Amplify Shader Editor surface shader of the same name. Its surface is
// black with this emission, and its FORWARDADD pass adds nothing, so it is unlit and forward-only.
// The original's ShadowCaster is left out: the map's layer is outside every shadow-casting light's
// culling mask.
Shader "ZeroGravity/Effects/SphereOfInfluence"
{
    Properties
    {
        [HideInInspector] __dirty ("", Float) = 1
        _MaskClipValue ("Mask Clip Value", Float) = 0.5
        _TextureSample0 ("Texture Sample 0", 2D) = "white" {}
        _Color0 ("Color 0", Color) = (0, 0, 0, 0)
        _Float0 ("Float 0", Float) = 1
        _Color1 ("Color 1", Color) = (0, 0, 0, 0)
        _Float1 ("Float 1", Float) = 0
        _Color2 ("Color 2", Color) = (0, 0, 0, 0)
        _Appear ("Appear", Range(-0.01, 1)) = 0
        _Fade ("Fade", Range(0, 1)) = 0
        [HideInInspector] _texcoord ("", 2D) = "white" {}
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
            float4 _texcoord_ST;
            float4 _TextureSample0_ST;
            float4 _Color0;
            float4 _Color1;
            float4 _Color2;
            float _Float0;
            float _Float1;
            float _Appear;
            float _Fade;
            float _MaskClipValue;
            float __dirty;
        CBUFFER_END

        TEXTURE2D(_TextureSample0);     SAMPLER(sampler_TextureSample0);

        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float2 uv0 : TEXCOORD0;
            float2 uv1 : TEXCOORD1;
            float2 uv2 : TEXCOORD2;
        };

        // Phase of the scrolling bands: V spans about half a turn (3.14 radians, so one band),
        // and they move at half a radian per second.
        float GetBandPhase(float2 uv0)
        {
            return _Time.y * 0.5 + uv0.y * 3.14;
        }

        float3 GetSphereColor(float2 uv, float bandPhase, float3 positionWS, float3 normalWS)
        {
            // The sphere is seen from inside, so the rim uses the inward-facing normal.
            float3 viewDirWS = normalize(_WorldSpaceCameraPos - positionWS);
            float rim = 1.0 - dot(-normalWS, viewDirWS);
            float rim2 = rim * rim;
            float3 rimColor = (rim2 * rim2 * rim) * _Color1.rgb;

            float4 texel = SAMPLE_TEXTURE2D(_TextureSample0, sampler_TextureSample0,
                uv * _TextureSample0_ST.xy + _TextureSample0_ST.zw);
            float2 bands = pow(abs(sin(bandPhase)), float2(_Float0, _Float1));

            float3 color = (texel.r * texel.b) * _Color0.rgb * bands.x + (texel.g * bands.y) * _Color2.rgb;
            color = rimColor * (bands.x * 0.5 + 0.5) + color;
            color = saturate(_Appear) * color;
            return color * _Fade;
        }
        ENDHLSL

        Pass
        {
            Name "Unlit"
            Tags { "LightMode" = "UniversalForwardOnly" }

            Cull Front
            Blend One One

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex SphereOfInfluenceVertex
            #pragma fragment SphereOfInfluenceFragment

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float bandPhase : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 positionWS : TEXCOORD3;
            };

            Varyings SphereOfInfluenceVertex(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.uv = TRANSFORM_TEX(input.uv0, _texcoord);
                output.bandPhase = GetBandPhase(input.uv0);
                // Normalized per vertex only, as in the original.
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            float4 SphereOfInfluenceFragment(Varyings input) : SV_Target
            {
                return float4(GetSphereColor(input.uv, input.bandPhase, input.positionWS, input.normalWS), 1.0);
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

            struct MetaVaryings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float bandPhase : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 positionWS : TEXCOORD3;
            #ifdef EDITOR_VISUALIZATION
                float2 VizUV : TEXCOORD4;
                float4 LightCoord : TEXCOORD5;
            #endif
            };

            MetaVaryings MetaPassVertex(Attributes input)
            {
                MetaVaryings output = (MetaVaryings)0;
                output.positionCS = UnityMetaVertexPosition(input.positionOS.xyz, input.uv1, input.uv2);
                output.uv = TRANSFORM_TEX(input.uv0, _texcoord);
                output.bandPhase = GetBandPhase(input.uv0);
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
                metaInput.Emission = GetSphereColor(input.uv, input.bandPhase, input.positionWS, input.normalWS);
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
