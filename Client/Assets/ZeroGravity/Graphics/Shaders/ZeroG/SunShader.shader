// The sun (SunPrefab and the Sun proxy planet): an unlit surface whose silhouette is eaten away by noise.
//  * colour: _Albedo × _ColorTint plus a screen blend of that colour with a fresnel rim, so the disc
//    shows twice the texture colour in the middle and brightens towards the edge,
//  * cut-out: near the silhouette, pixels are clipped where two layers of noise are bright. The
//    threshold is a fixed 0.5; _Cutoff is only there for the fallback shader, as in the original.
//
// Rebuilt for URP from the Shader Forge shader of the same name.
Shader "ZeroG/sunShader"
{
    Properties
    {
        _Albedo ("Albedo", 2D) = "white" {}
        _ColorTint ("Color Tint", Color) = (1, 1, 1, 1)
        _NoiseTileScale ("Noise Tile/Scale", Range(0, 50)) = 2.555199
        _FresnelColor ("Fresnel Color", Color) = (1, 1, 1, 1)
        _FresnelSpread ("Fresnel Spread", Range(1, 5)) = 2
        _EdgeOpacity ("Edge Opacity", Range(0, 10)) = 10
        [HideInInspector] _Cutoff ("Alpha cutoff", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "TransparentCutout"
            "Queue" = "AlphaTest"
            "UniversalMaterialType" = "Unlit"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
        #endif

        CBUFFER_START(UnityPerMaterial)
            float4 _Albedo_ST;
            float4 _ColorTint;
            float4 _FresnelColor;
            float _NoiseTileScale;
            float _FresnelSpread;
            float _EdgeOpacity;
            float _Cutoff;
        CBUFFER_END

        TEXTURE2D(_Albedo);     SAMPLER(sampler_Albedo);

        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float2 uv0 : TEXCOORD0;
            float2 uv1 : TEXCOORD1;     // lightmap UVs, used only by the Meta pass
            float2 uv2 : TEXCOORD2;     // dynamic lightmap UVs, used only by the Meta pass
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float2 uv : TEXCOORD0;
            float3 positionWS : TEXCOORD1;
            float3 normalWS : TEXCOORD2;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
        };

        Varyings SunVertex(Attributes input)
        {
            Varyings output = (Varyings)0;
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_TRANSFER_INSTANCE_ID(input, output);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

            output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
            output.positionCS = TransformWorldToHClip(output.positionWS);
            output.normalWS = TransformObjectToWorldNormal(input.normalOS);
            output.uv = input.uv0;
            return output;
        }

        // Shader Forge's Noise node: a hash of the coordinate in [0, 1).
        float ShaderForgeNoise(float2 coord)
        {
            float2 skewed = coord + 0.2127 + coord.x * 0.3713 * coord.y;
            float2 random = 4.789 * sin(489.123 * skewed);
            return frac(random.x * random.y * (1.0 + skewed.x));
        }

        float2 RotateAroundCenter(float2 uv, float angle)
        {
            float sine, cosine;
            sincos(angle, sine, cosine);
            float2 centered = uv - 0.5;
            return float2(dot(centered, float2(cosine, sine)), dot(centered, float2(-sine, cosine))) + 0.5;
        }

        // 0 where the surface faces the camera, 1 at the silhouette. The original always measures from
        // the camera position, also for orthographic cameras and in the shadow pass.
        // The original clamps with max(dot, 0). saturate also keeps a dot product a rounding error
        // above 1 from making the result negative, which would turn pow() into NaN.
        float GetEdgeFactor(float3 positionWS, float3 normalWS)
        {
            float3 viewDirectionWS = normalize(GetCameraPositionWS() - positionWS);
            return 1.0 - saturate(dot(normalize(normalWS), viewDirectionWS));
        }

        // Clips where the noise, faded out towards the middle of the disc by pow(edge, _EdgeOpacity),
        // exceeds 0.5. The noise is the sum of two layers: one of the UVs and one of the UVs rotated by
        // _NoiseTileScale radians, both shifted up a tile and scaled down by _NoiseTileScale.
        // The hash magnifies rounding differences, so this multiplies by the reciprocal as the
        // original does; dividing instead can change which pixels are clipped.
        void ClipEdge(float2 uv, float edge)
        {
            float noiseScale = 1.0 / _NoiseTileScale;
            float noise = ShaderForgeNoise((RotateAroundCenter(uv, _NoiseTileScale) + float2(0.0, 1.0)) * noiseScale)
                        + ShaderForgeNoise((uv + float2(0.0, 1.0)) * noiseScale);
            clip(0.5 - pow(edge, _EdgeOpacity) * noise);
        }

        half3 SampleAlbedo(float2 uv)
        {
            return SAMPLE_TEXTURE2D(_Albedo, sampler_Albedo, TRANSFORM_TEX(uv, _Albedo)).rgb * _ColorTint.rgb;
        }

        // Pixel shader shared by the passes that only need the cut-out.
        void ClipFragment(Varyings input)
        {
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

            ClipEdge(input.uv, GetEdgeFactor(input.positionWS, input.normalWS));
        #if defined(LOD_FADE_CROSSFADE)
            LODFadeCrossFade(input.positionCS);
        #endif
        }
        ENDHLSL

        // UniversalForwardOnly rather than UniversalForward: the deferred renderer skips UniversalForward
        // passes of shaders without a GBuffer pass, but draws forward-only ones.
        Pass
        {
            Name "Unlit"
            Tags { "LightMode" = "UniversalForwardOnly" }

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex SunVertex
            #pragma fragment UnlitFragment

            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            void UnlitFragment(
                Varyings input
                , out half4 outColor : SV_Target0
            #ifdef _WRITE_RENDERING_LAYERS
                , out uint outRenderingLayers : SV_Target1
            #endif
            )
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float edge = GetEdgeFactor(input.positionWS, input.normalWS);
                ClipEdge(input.uv, edge);
            #if defined(LOD_FADE_CROSSFADE)
                LODFadeCrossFade(input.positionCS);
            #endif

                half3 albedo = SampleAlbedo(input.uv);
                half3 fresnel = _FresnelColor.rgb * pow(edge, _FresnelSpread);
                half3 screen = 1.0 - (1.0 - albedo) * (1.0 - fresnel);
                // No fog: the original was compiled without it.
                outColor = half4(albedo + screen, 1.0);
            #ifdef _WRITE_RENDERING_LAYERS
                outRenderingLayers = EncodeMeshRenderingLayer();
            #endif
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex ShadowVertex
            #pragma fragment ShadowFragment

            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            Varyings ShadowVertex(Attributes input)
            {
                Varyings output = SunVertex(input);
            #if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW)
                float3 lightDirectionWS = normalize(_LightPosition - output.positionWS);
            #else
                float3 lightDirectionWS = _LightDirection;
            #endif
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(output.positionWS, output.normalWS, lightDirectionWS));
                output.positionCS = ApplyShadowClamping(positionCS);
                return output;
            }

            half4 ShadowFragment(Varyings input) : SV_Target
            {
                ClipFragment(input);
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex SunVertex
            #pragma fragment DepthOnlyFragment

            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            half DepthOnlyFragment(Varyings input) : SV_Target
            {
                ClipFragment(input);
                return input.positionCS.z;
            }
            ENDHLSL
        }

        // DepthNormalsOnly, the depth-normals pass of forward-only shaders: the forward renderer draws it
        // as DepthNormals, and the deferred renderer uses it to add this surface's normals to the G-buffer.
        Pass
        {
            Name "DepthNormalsOnly"
            Tags { "LightMode" = "DepthNormalsOnly" }

            ZWrite On

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex SunVertex
            #pragma fragment DepthNormalsFragment

            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            void DepthNormalsFragment(
                Varyings input
                , out half4 outNormalWS : SV_Target0
            #ifdef _WRITE_RENDERING_LAYERS
                , out uint outRenderingLayers : SV_Target1
            #endif
            )
            {
                ClipFragment(input);

                float3 normalWS = normalize(input.normalWS);
            #if defined(_GBUFFER_NORMALS_OCT)
                float2 octNormalWS = PackNormalOctQuadEncode(normalWS);
                outNormalWS = half4(PackFloat2To888(saturate(octNormalWS * 0.5 + 0.5)), 0.0);
            #else
                outNormalWS = half4(normalWS, 0.0);
            #endif
            #ifdef _WRITE_RENDERING_LAYERS
                outRenderingLayers = EncodeMeshRenderingLayer();
            #endif
            }
            ENDHLSL
        }

        // Emission for the lightmapper: the tinted texture without the rim, and black albedo.
        // Nothing is clipped here.
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
            #ifdef EDITOR_VISUALIZATION
                float2 VizUV : TEXCOORD1;
                float4 LightCoord : TEXCOORD2;
            #endif
            };

            MetaVaryings MetaPassVertex(Attributes input)
            {
                MetaVaryings output = (MetaVaryings)0;
                output.positionCS = UnityMetaVertexPosition(input.positionOS.xyz, input.uv1, input.uv2);
                output.uv = input.uv0;
            #ifdef EDITOR_VISUALIZATION
                UnityEditorVizData(input.positionOS.xyz, input.uv0, input.uv1, input.uv2, output.VizUV, output.LightCoord);
            #endif
                return output;
            }

            half4 MetaPassFragment(MetaVaryings input) : SV_Target
            {
                MetaInput metaInput = (MetaInput)0;
                metaInput.Albedo = 0.0;
                metaInput.Emission = SampleAlbedo(input.uv);
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
