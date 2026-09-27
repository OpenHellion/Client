// Metallic PBR glass (cockpit and cage windows, container lids) that fades with distance to the camera:
// transparent up close (_Transparency at _MinDistance and nearer) and opaque from _MaxDistance on, with a
// smoothstep in between. The texture and colour alpha are not used.
//
// Rebuilt for URP from the Amplify Shader Editor surface shader of the same name. The original was an
// alpha-blended forward shader (no DEFERRED pass): built-in deferred drew it with forward rendering
// after the deferred lighting, as URP Deferred+ does with UniversalForwardOnly passes. Like the original
// it receives no shadows and casts none.
Shader "ZeroGravity/Surface/StandardTransparentClose"
{
    Properties
    {
        [HideInInspector] __dirty ("", Float) = 1
        _MainTex ("MainTex", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _BumpMap ("BumpMap", 2D) = "bump" {}
        _BumpScale ("BumpScale", Float) = 0
        _MetallicGlossMap ("MetallicGlossMap", 2D) = "white" {}
        _GlossMapScale ("GlossMapScale", Float) = 0
        _Transparency ("Transparency", Float) = 0
        _Metallic ("Metallic", Range(0, 1)) = 0
        _MinDistance ("MinDistance", Float) = 0
        _MaxDistance ("MaxDistance", Float) = 0
        [HideInInspector] _texcoord ("", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent+0"
            "IgnoreProjector" = "True"
        }

        HLSLINCLUDE
        // An alpha-blended surface: no SSAO, decals or screen-space shadows from the opaque depth
        // behind it, and, as in the original, no shadows at all. Both are read by URP's includes.
        #define _SURFACE_TYPE_TRANSPARENT 1
        #define _RECEIVE_SHADOWS_OFF 1

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
        #endif

        CBUFFER_START(UnityPerMaterial)
            float4 _texcoord_ST;
            float4 _MainTex_ST;
            float4 _BumpMap_ST;
            float4 _MetallicGlossMap_ST;
            float4 _Color;
            float _BumpScale;
            float _GlossMapScale;
            float _Transparency;
            float _Metallic;
            float _MinDistance;
            float _MaxDistance;
        CBUFFER_END

        TEXTURE2D(_MainTex);            SAMPLER(sampler_MainTex);
        TEXTURE2D(_BumpMap);            SAMPLER(sampler_BumpMap);
        TEXTURE2D(_MetallicGlossMap);   SAMPLER(sampler_MetallicGlossMap);

        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float2 uv0 : TEXCOORD0;
            float2 uv1 : TEXCOORD1;     // Unity lightmap UVs, used only by the Meta pass
            float2 uv2 : TEXCOORD2;     // Realtime GI UVs, used only by the Meta pass
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        // UV0 with the hidden Amplify Shader Editor tiling _texcoord (normally identity). Each texture's
        // own tiling is applied per pixel, as in the original.
        float2 GetVertexUV(float2 uv0)
        {
            return TRANSFORM_TEX(uv0, _texcoord);
        }

        // Unpacks like the built-in pipeline's UnpackScaleNormal. It scales xy before rebuilding z,
        // so the result differs from URP's UnpackNormalScale when _BumpScale is not 1.
        half3 UnpackScaledNormal(half4 packedNormal, half scale)
        {
            packedNormal.x *= packedNormal.w;   // DXT5nm keeps x in alpha; alpha is 1 for BC5 and RGB maps
            half3 normal;
            normal.xy = (packedNormal.xy * 2.0 - 1.0) * scale;
            normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
            return normal;
        }

        // The bitangent is URP's per-vertex one, interpolated like the original's.
        float3 TransformSurfaceNormalToWorld(half3 normalTS, float3 tangentWS, float3 bitangentWS, float3 normalWS)
        {
            return NormalizeNormalPerPixel(TransformTangentToWorld(normalTS, half3x3(tangentWS, bitangentWS, normalWS)));
        }

        // Opacity from the distance to the camera: lerp(_Transparency, 1, t) with t going from 0 at
        // _MinDistance to 1 at _MaxDistance, clamped and smoothed. Operations in the original's order.
        float GetDistanceAlpha(float3 positionWS)
        {
            float distanceToCamera = length(GetCameraPositionWS() - positionWS);
            float fade = saturate((distanceToCamera - _MinDistance) * (1.0 - _Transparency) / (_MaxDistance - _MinDistance)
                                  + _Transparency);
            return fade * fade * (3.0 - 2.0 * fade);
        }

        // uv is the vertex UV (GetVertexUV).
        void InitializeSurfaceData(float2 uv, float3 positionWS, out SurfaceData surfaceData)
        {
            half4 metallicGloss = SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, TRANSFORM_TEX(uv, _MetallicGlossMap));

            surfaceData = (SurfaceData)0;
            surfaceData.albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, TRANSFORM_TEX(uv, _MainTex)).rgb * _Color.rgb;
            surfaceData.alpha = GetDistanceAlpha(positionWS);
            surfaceData.metallic = metallicGloss.r * _Metallic;
            surfaceData.smoothness = metallicGloss.a * _GlossMapScale;
            surfaceData.occlusion = 1.0;
            surfaceData.normalTS = UnpackScaledNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, TRANSFORM_TEX(uv, _BumpMap)), _BumpScale);
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForwardOnly" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ColorMask RGB

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex LitVertex
            #pragma fragment ForwardFragment

            // URP Lit's keywords, without static and dynamic lightmaps (the original had none), without
            // realtime shadows (it received none), and without what URP skips for transparent surfaces:
            // SSAO, decals, and screen-space irradiance, which holds the opaque surface behind.
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_ATLAS
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile _ _LIGHT_LAYERS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile_fragment _ REFLECTION_PROBE_ROTATION
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ProbeVolumeVariants.hlsl"
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            struct LitVaryings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;                          // GetVertexUV()
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 tangentWS : TEXCOORD3;
                float3 bitangentWS : TEXCOORD4;
                half4 fogFactorAndVertexLight : TEXCOORD5;      // x: fog factor, yzw: per-vertex lights
                half3 vertexSH : TEXCOORD6;
            #if defined(USE_APV_PROBE_OCCLUSION)
                float4 probeOcclusion : TEXCOORD7;
            #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            LitVaryings LitVertex(Attributes input)
            {
                LitVaryings output = (LitVaryings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = normalInputs.normalWS;
                output.tangentWS = normalInputs.tangentWS;
                output.bitangentWS = normalInputs.bitangentWS;
                output.uv = GetVertexUV(input.uv0);

                half fogFactor = 0;
            #if !defined(_FOG_FRAGMENT)
                fogFactor = ComputeFogFactor(positionInputs.positionCS.z);
            #endif
                output.fogFactorAndVertexLight = half4(fogFactor, VertexLighting(positionInputs.positionWS, normalInputs.normalWS));

                // Ambient light comes from light probes only.
                OUTPUT_SH4(positionInputs.positionWS, output.normalWS, GetWorldSpaceNormalizeViewDir(positionInputs.positionWS),
                           output.vertexSH, output.probeOcclusion);
                return output;
            }

            void InitializeInputData(LitVaryings input, half3 normalTS, out InputData inputData)
            {
                inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.positionCS = input.positionCS;
                inputData.normalWS = TransformSurfaceNormalToWorld(normalTS, input.tangentWS, input.bitangentWS, input.normalWS);
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                inputData.shadowCoord = float4(0, 0, 0, 0);
                inputData.fogCoord = InitializeInputDataFog(float4(input.positionWS, 1.0), input.fogFactorAndVertexLight.x);
                inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);

            #if defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2)
                inputData.bakedGI = SAMPLE_GI(input.vertexSH, GetAbsolutePositionWS(inputData.positionWS), inputData.normalWS,
                                              inputData.viewDirectionWS, input.positionCS.xy, input.probeOcclusion, inputData.shadowMask);
            #else
                inputData.bakedGI = SampleSHPixel(input.vertexSH, inputData.normalWS);
                inputData.shadowMask = SAMPLE_SHADOWMASK(0);
            #endif
            }

            void ForwardFragment(
                LitVaryings input
                , out half4 outColor : SV_Target0
            #ifdef _WRITE_RENDERING_LAYERS
                , out uint outRenderingLayers : SV_Target1
            #endif
            )
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                SurfaceData surfaceData;
                InitializeSurfaceData(input.uv, input.positionWS, surfaceData);
            #if defined(LOD_FADE_CROSSFADE)
                LODFadeCrossFade(input.positionCS);
            #endif

                InputData inputData;
                InitializeInputData(input, surfaceData.normalTS, inputData);

                // Blended with SrcAlpha, so the colour is not premultiplied (as the original's alpha:fade).
                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                outColor = half4(MixFog(color.rgb, inputData.fogCoord), color.a);
            #ifdef _WRITE_RENDERING_LAYERS
                outRenderingLayers = EncodeMeshRenderingLayer();
            #endif
            }
            ENDHLSL
        }

        // Albedo for the lightmapper. The surface has no emission.
        Pass
        {
            Name "Meta"
            Tags { "LightMode" = "Meta" }

            Cull Off
            ZWrite Off
            ColorMask RGB

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex MetaPassVertex
            #pragma fragment MetaPassFragment
            #pragma shader_feature EDITOR_VISUALIZATION

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"

            struct MetaVaryings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;                          // GetVertexUV()
            #ifdef EDITOR_VISUALIZATION
                float2 VizUV : TEXCOORD1;
                float4 LightCoord : TEXCOORD2;
            #endif
            };

            MetaVaryings MetaPassVertex(Attributes input)
            {
                MetaVaryings output = (MetaVaryings)0;
                output.positionCS = UnityMetaVertexPosition(input.positionOS.xyz, input.uv1, input.uv2);
                output.uv = GetVertexUV(input.uv0);
            #ifdef EDITOR_VISUALIZATION
                UnityEditorVizData(input.positionOS.xyz, input.uv0, input.uv1, input.uv2, output.VizUV, output.LightCoord);
            #endif
                return output;
            }

            half4 MetaPassFragment(MetaVaryings input) : SV_Target
            {
                MetaInput metaInput = (MetaInput)0;
                metaInput.Albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, TRANSFORM_TEX(input.uv, _MainTex)).rgb * _Color.rgb;
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
