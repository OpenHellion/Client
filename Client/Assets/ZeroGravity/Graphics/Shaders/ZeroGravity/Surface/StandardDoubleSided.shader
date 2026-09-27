// Unity's Standard (metallic) shader drawn without back-face culling, for leaves and glass panes. Back
// faces are lit with the front face's normal, as in the original.
//
// Rebuilt for URP from the original, a copy of the Unity 5.x Standard shader. It keeps Standard's
// properties, material keywords and inspector (StandardShaderGUI, still part of the editor), which sets
// the keywords, _SrcBlend, _DstBlend, _ZWrite and the render queue from the Rendering Mode (_Mode):
//   Opaque, Cutout (_ALPHATEST_ON), Fade (_ALPHABLEND_ON) and Transparent (_ALPHAPREMULTIPLY_ON).
// Opaque and Cutout materials are lit through the G-buffer under Deferred+; Fade and Transparent ones
// are in a transparent queue and drawn by the ForwardLit pass.
//
// The original's DEFERRED pass never added light-probe ambient light: its Unity 5 source checks
// LIGHTMAP_OFF, which later engines no longer define. The game rendered deferred, so the GBuffer pass
// leaves it out too. The forward pass adds it, as the original's FORWARDBASE pass did.
Shader "ZeroGravity/Surface/StandardDoubleSided"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("Albedo", 2D) = "white" {}
        _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5
        _Glossiness ("Smoothness", Range(0, 1)) = 0.5
        _GlossMapScale ("Smoothness Scale", Range(0, 1)) = 1
        [Enum(Metallic Alpha,0,Albedo Alpha,1)] _SmoothnessTextureChannel ("Smoothness texture channel", Float) = 0
        [Gamma] _Metallic ("Metallic", Range(0, 1)) = 0
        _MetallicGlossMap ("Metallic", 2D) = "white" {}
        [ToggleOff] _SpecularHighlights ("Specular Highlights", Float) = 1
        [ToggleOff] _GlossyReflections ("Glossy Reflections", Float) = 1
        _BumpScale ("Scale", Float) = 1
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _Parallax ("Height Scale", Range(0.005, 0.08)) = 0.02
        _ParallaxMap ("Height Map", 2D) = "black" {}
        _OcclusionStrength ("Strength", Range(0, 1)) = 1
        _OcclusionMap ("Occlusion", 2D) = "white" {}
        _EmissionColor ("Color", Color) = (0, 0, 0, 1)
        _EmissionMap ("Emission", 2D) = "white" {}
        _DetailMask ("Detail Mask", 2D) = "white" {}
        _DetailAlbedoMap ("Detail Albedo x2", 2D) = "grey" {}
        _DetailNormalMapScale ("Scale", Float) = 1
        _DetailNormalMap ("Normal Map", 2D) = "bump" {}
        [Enum(UV0,0,UV1,1)] _UVSec ("UV Set for secondary textures", Float) = 0
        [HideInInspector] _Mode ("__mode", Float) = 0
        [HideInInspector] _SrcBlend ("__src", Float) = 1
        [HideInInspector] _DstBlend ("__dst", Float) = 0
        [HideInInspector] _ZWrite ("__zw", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "PerformanceChecks" = "False"
            "UniversalMaterialType" = "Lit"
        }
        LOD 300

        HLSLINCLUDE
        // Fade and Transparent: no SSAO, decals or screen-space shadows from the opaque depth behind.
        #if defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
            #define _SURFACE_TYPE_TRANSPARENT 1
        #endif
        // Standard's "Glossy Reflections" off is URP's environment reflections off.
        #if defined(_GLOSSYREFLECTIONS_OFF)
            #define _ENVIRONMENTREFLECTIONS_OFF 1
        #endif

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ParallaxMapping.hlsl"
        #if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
        #endif

        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _DetailAlbedoMap_ST;
            float4 _Color;
            float4 _EmissionColor;
            float _Cutoff;
            float _Glossiness;
            float _GlossMapScale;
            float _Metallic;
            float _BumpScale;
            float _Parallax;
            float _OcclusionStrength;
            float _DetailNormalMapScale;
            float _UVSec;
        CBUFFER_END

        TEXTURE2D(_MainTex);            SAMPLER(sampler_MainTex);
        TEXTURE2D(_MetallicGlossMap);   SAMPLER(sampler_MetallicGlossMap);
        TEXTURE2D(_BumpMap);            SAMPLER(sampler_BumpMap);
        TEXTURE2D(_ParallaxMap);        SAMPLER(sampler_ParallaxMap);
        TEXTURE2D(_OcclusionMap);       SAMPLER(sampler_OcclusionMap);
        TEXTURE2D(_EmissionMap);        SAMPLER(sampler_EmissionMap);
        TEXTURE2D(_DetailMask);         SAMPLER(sampler_DetailMask);
        TEXTURE2D(_DetailAlbedoMap);    SAMPLER(sampler_DetailAlbedoMap);
        TEXTURE2D(_DetailNormalMap);    SAMPLER(sampler_DetailNormalMap);

        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float2 uv0 : TEXCOORD0;
            float2 uv1 : TEXCOORD1;     // detail UVs when _UVSec is 1, and Unity lightmap UVs for the Meta pass
            float2 uv2 : TEXCOORD2;     // Realtime GI UVs, used only by the Meta pass
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        // xy: _MainTex UV (UV0), zw: detail UV (UV0 or UV1 by _UVSec), each with its tiling. Per vertex,
        // as in the original.
        float4 GetTexCoords(float2 uv0, float2 uv1)
        {
            float2 detailUV = _UVSec == 0 ? uv0 : uv1;
            return float4(TRANSFORM_TEX(uv0, _MainTex), TRANSFORM_TEX(detailUV, _DetailAlbedoMap));
        }

        // Standard's per-vertex parallax view direction: the direction to the camera in object space,
        // rotated into tangent space by the object-space tangent frame (TANGENT_SPACE_ROTATION).
        float3 GetViewDirForParallax(float3 positionOS, float3 normalOS, float4 tangentOS)
        {
            float3 bitangentOS = cross(normalize(normalOS), normalize(tangentOS.xyz)) * tangentOS.w;
            float3x3 rotation = float3x3(tangentOS.xyz, bitangentOS, normalOS);
            return mul(rotation, TransformWorldToObject(GetCameraPositionWS()) - positionOS);
        }

        // Shifts both UV pairs by the height in _ParallaxMap's green channel.
        float4 ApplyParallax(float4 texCoords, float3 viewDirForParallax)
        {
        #if defined(_PARALLAXMAP)
            half height = SAMPLE_TEXTURE2D(_ParallaxMap, sampler_ParallaxMap, texCoords.xy).g;
            float2 offset = ParallaxOffset1Step(height, _Parallax, normalize(viewDirForParallax));
            texCoords += offset.xyxy;
        #endif
            return texCoords;
        }

        // The surface alpha: from _Color alone when the albedo alpha holds smoothness.
        half GetAlpha(half albedoAlpha)
        {
        #if defined(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A)
            return _Color.a;
        #else
            return albedoAlpha * _Color.a;
        #endif
        }

        // Cutout's alpha test (Standard's shadow pass always uses the albedo alpha).
        void ClipCutout(half alpha)
        {
        #if defined(_ALPHATEST_ON)
            clip(alpha - _Cutoff);
        #endif
        }

        // Unpacks like the built-in pipeline's UnpackScaleNormal. It scales xy before rebuilding z,
        // so the result differs from URP's UnpackNormalScale when the scale is not 1.
        half3 UnpackScaledNormal(half4 packedNormal, half scale)
        {
            packedNormal.x *= packedNormal.w;   // DXT5nm keeps x in alpha; alpha is 1 for BC5 and RGB maps
            half3 normal;
            normal.xy = (packedNormal.xy * 2.0 - 1.0) * scale;
            normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
            return normal;
        }

        // Tangent-space normal from _BumpMap, blended with _DetailNormalMap where _DetailMask allows.
        half3 GetNormalTS(float4 texCoords)
        {
        #if defined(_NORMALMAP)
            half3 normalTS = UnpackScaledNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, texCoords.xy), _BumpScale);
            #if defined(_DETAIL_MULX2)
                half mask = SAMPLE_TEXTURE2D(_DetailMask, sampler_DetailMask, texCoords.xy).a;
                half3 detailNormalTS = UnpackScaledNormal(SAMPLE_TEXTURE2D(_DetailNormalMap, sampler_DetailNormalMap, texCoords.zw),
                                                          _DetailNormalMapScale);
                half3 blended = normalize(half3(normalTS.xy + detailNormalTS.xy, normalTS.z * detailNormalTS.z));
                normalTS = lerp(normalTS, blended, mask);
            #endif
            return normalTS;
        #else
            return half3(0.0, 0.0, 1.0);
        #endif
        }

        // texCoords come from GetTexCoords, with the parallax offset applied. Clips Cutout pixels.
        void InitializeSurfaceData(float4 texCoords, out SurfaceData surfaceData)
        {
            half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, texCoords.xy);
            half alpha = GetAlpha(mainTex.a);
            ClipCutout(alpha);

            half3 albedo = _Color.rgb * mainTex.rgb;
        #if defined(_DETAIL_MULX2)
            // "Detail Albedo x2": 2 in gamma space, which is 4.59 in linear space.
            #if defined(UNITY_COLORSPACE_GAMMA)
                const half colorSpaceDouble = 2.0;
            #else
                const half colorSpaceDouble = 4.59479380;
            #endif
            half mask = SAMPLE_TEXTURE2D(_DetailMask, sampler_DetailMask, texCoords.xy).a;
            half3 detailAlbedo = SAMPLE_TEXTURE2D(_DetailAlbedoMap, sampler_DetailAlbedoMap, texCoords.zw).rgb;
            albedo *= LerpWhiteTo(detailAlbedo * colorSpaceDouble, mask);
        #endif

            half2 metallicGloss;
        #if defined(_METALLICGLOSSMAP)
            half4 metallicGlossMap = SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, texCoords.xy);
            #if defined(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A)
                metallicGloss = half2(metallicGlossMap.r, mainTex.a);
            #else
                metallicGloss = metallicGlossMap.ra;
            #endif
            metallicGloss.y *= _GlossMapScale;
        #else
            #if defined(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A)
                metallicGloss = half2(_Metallic, mainTex.a * _GlossMapScale);
            #else
                metallicGloss = half2(_Metallic, _Glossiness);
            #endif
        #endif

            half occlusion = SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, texCoords.xy).g;

            surfaceData = (SurfaceData)0;
            surfaceData.albedo = albedo;
            surfaceData.alpha = alpha;
            surfaceData.metallic = metallicGloss.x;
            surfaceData.smoothness = metallicGloss.y;
            surfaceData.occlusion = (1.0 - _OcclusionStrength) + occlusion * _OcclusionStrength;
        #if defined(_EMISSION)
            surfaceData.emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, texCoords.xy).rgb * _EmissionColor.rgb;
        #endif
            surfaceData.normalTS = GetNormalTS(texCoords);
        }

        // The bitangent is URP's per-vertex one, interpolated like the original's.
        float3 TransformSurfaceNormalToWorld(half3 normalTS, float3 tangentWS, float3 bitangentWS, float3 normalWS)
        {
            return NormalizeNormalPerPixel(TransformTangentToWorld(normalTS, half3x3(tangentWS, bitangentWS, normalWS)));
        }

        // ---------------------------------------------------------------------
        // Vertex data shared by the ForwardLit and GBuffer passes
        // ---------------------------------------------------------------------

        struct LitVaryings
        {
            float4 positionCS : SV_POSITION;
            float4 texCoords : TEXCOORD0;                   // GetTexCoords()
            float3 positionWS : TEXCOORD1;
            float3 normalWS : TEXCOORD2;
            float3 tangentWS : TEXCOORD3;
            float3 bitangentWS : TEXCOORD4;
            half4 fogFactorAndVertexLight : TEXCOORD5;      // x: fog factor, yzw: per-vertex lights
            half3 vertexSH : TEXCOORD6;
        #if defined(_PARALLAXMAP)
            float3 viewDirForParallax : TEXCOORD7;
        #endif
        #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            float4 shadowCoord : TEXCOORD8;
        #endif
        #if defined(USE_APV_PROBE_OCCLUSION)
            float4 probeOcclusion : TEXCOORD9;
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
            output.texCoords = GetTexCoords(input.uv0, input.uv1);
        #if defined(_PARALLAXMAP)
            output.viewDirForParallax = GetViewDirForParallax(input.positionOS.xyz, input.normalOS, input.tangentOS);
        #endif

            half fogFactor = 0;
        #if !defined(_FOG_FRAGMENT)
            fogFactor = ComputeFogFactor(positionInputs.positionCS.z);
        #endif
            output.fogFactorAndVertexLight = half4(fogFactor, VertexLighting(positionInputs.positionWS, normalInputs.normalWS));

            // Ambient light comes from light probes only; no material using this shader was lightmapped.
            OUTPUT_SH4(positionInputs.positionWS, output.normalWS, GetWorldSpaceNormalizeViewDir(positionInputs.positionWS),
                       output.vertexSH, output.probeOcclusion);

        #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            output.shadowCoord = GetShadowCoord(positionInputs);
        #endif
            return output;
        }

        void InitializeInputData(LitVaryings input, half3 normalTS, out InputData inputData)
        {
            inputData = (InputData)0;
            inputData.positionWS = input.positionWS;
            inputData.positionCS = input.positionCS;
            inputData.normalWS = TransformSurfaceNormalToWorld(normalTS, input.tangentWS, input.bitangentWS, input.normalWS);
            inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
        #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            inputData.shadowCoord = input.shadowCoord;
        #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
            inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
        #else
            inputData.shadowCoord = float4(0, 0, 0, 0);
        #endif
            inputData.fogCoord = InitializeInputDataFog(float4(input.positionWS, 1.0), input.fogFactorAndVertexLight.x);
            inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
            inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);

            // Screen-space irradiance belongs to the opaque surface in each pixel, not to Fade and
            // Transparent surfaces in front of it.
        #if defined(_SCREEN_SPACE_IRRADIANCE) && !defined(_SURFACE_TYPE_TRANSPARENT)
            inputData.bakedGI = SAMPLE_GI(_ScreenSpaceIrradiance, input.positionCS.xy);
        #elif defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2)
            inputData.bakedGI = SAMPLE_GI(input.vertexSH, GetAbsolutePositionWS(inputData.positionWS), inputData.normalWS,
                                          inputData.viewDirectionWS, input.positionCS.xy, input.probeOcclusion, inputData.shadowMask);
        #else
            inputData.bakedGI = SampleSHPixel(input.vertexSH, inputData.normalWS);
            inputData.shadowMask = SAMPLE_SHADOWMASK(0);
        #endif
        }

        SurfaceData GetLitSurfaceData(LitVaryings input)
        {
            float4 texCoords = input.texCoords;
        #if defined(_PARALLAXMAP)
            texCoords = ApplyParallax(texCoords, input.viewDirForParallax);
        #endif
            SurfaceData surfaceData;
            InitializeSurfaceData(texCoords, surfaceData);
            return surfaceData;
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            Cull Off

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex LitVertex
            #pragma fragment ForwardFragment

            // Standard's material keywords.
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _PARALLAXMAP
            // Both stages: the transparent modes change which interpolators URP needs.
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _METALLICGLOSSMAP
            #pragma shader_feature_local_fragment _DETAIL_MULX2
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _GLOSSYREFLECTIONS_OFF

            // URP Lit's keywords, without static and dynamic lightmaps.
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_ATLAS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _SCREEN_SPACE_IRRADIANCE
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
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

                SurfaceData surfaceData = GetLitSurfaceData(input);
            #if defined(LOD_FADE_CROSSFADE)
                LODFadeCrossFade(input.positionCS);
            #endif

                InputData inputData;
                InitializeInputData(input, surfaceData.normalTS, inputData);
            #if defined(_DBUFFER)
                ApplyDecalToSurfaceData(input.positionCS, surfaceData, inputData);
            #endif

                // Transparent premultiplies the diffuse part by alpha (URP's BRDF does that under
                // _ALPHAPREMULTIPLY_ON) and, as Standard does, lets reflectivity hide the background.
                half4 color = UniversalFragmentPBR(inputData, surfaceData);
            #if defined(_ALPHAPREMULTIPLY_ON)
                half oneMinusReflectivity = OneMinusReflectivityMetallic(surfaceData.metallic);
                color.a = 1.0 - oneMinusReflectivity + surfaceData.alpha * oneMinusReflectivity;
            #elif !defined(_ALPHABLEND_ON)
                color.a = 1.0;
            #endif
                outColor = half4(MixFog(color.rgb, inputData.fogCoord), color.a);
            #ifdef _WRITE_RENDERING_LAYERS
                outRenderingLayers = EncodeMeshRenderingLayer();
            #endif
            }
            ENDHLSL
        }

        Pass
        {
            Name "GBuffer"
            Tags { "LightMode" = "UniversalGBuffer" }

            Cull Off

            HLSLPROGRAM
            #pragma target 4.5
            #pragma exclude_renderers gles3 glcore
            #pragma vertex LitVertex
            #pragma fragment GBufferFragment

            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _PARALLAXMAP
            // Both stages: the transparent modes change which interpolators URP needs.
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _METALLICGLOSSMAP
            #pragma shader_feature_local_fragment _DETAIL_MULX2
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _GLOSSYREFLECTIONS_OFF

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma multi_compile_fragment _ _RENDER_PASS_ENABLED
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile_fragment _ REFLECTION_PROBE_ROTATION
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ProbeVolumeVariants.hlsl"
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GBufferOutput.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GBufferOutputFormat.hlsl"

            GBufferFragOutput GBufferFragment(LitVaryings input)
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                SurfaceData surfaceData = GetLitSurfaceData(input);
            #if defined(LOD_FADE_CROSSFADE)
                LODFadeCrossFade(input.positionCS);
            #endif

                InputData inputData;
                InitializeInputData(input, surfaceData.normalTS, inputData);
            #if defined(_DBUFFER)
                ApplyDecalToSurfaceData(input.positionCS, surfaceData, inputData);
            #endif

                // Direct lights are applied by the deferred lighting pass. The GBuffer's colour target
                // takes emission plus reflections, without the light-probe ambient light that the
                // original's DEFERRED pass never added (see the top of the file).
                BRDFData brdfData;
                InitializeBRDFData(surfaceData.albedo, surfaceData.metallic, surfaceData.specular, surfaceData.smoothness,
                                   surfaceData.alpha, brdfData);
                inputData.bakedGI = 0.0;
                half3 globalIllumination = GlobalIllumination(brdfData, (BRDFData)0, 0, inputData.bakedGI, surfaceData.occlusion,
                                                              inputData.positionWS, inputData.normalWS, inputData.viewDirectionWS,
                                                              inputData.normalizedScreenSpaceUV);

                return PackGBuffersBRDFData(brdfData, inputData, surfaceData.smoothness,
                                            surfaceData.emission + globalIllumination, surfaceData.occlusion);
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
            Cull Off

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex ShadowVertex
            #pragma fragment ShadowFragment

            #pragma shader_feature_local_fragment _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local_fragment _METALLICGLOSSMAP

            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct ShadowVaryings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;                      // UV0 with _MainTex_ST
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // Fade and Transparent shadows: clips the shadow to a 4x4 screen-door pattern covering about
            // `opacity` of the shadow map, as Standard's shadow caster does with the built-in pipeline's
            // _DitherMaskLOD. The texture has 16 slices, picked by floor(opacity * 15). Slices 0 to 7
            // cover that many of the 16 pixels, adding them in the order of kDitherRank; slices 8 to 15
            // cover the pixels that slice 15 - n leaves out.
            void ClipShadowDither(float2 positionSS, float opacity)
            {
                static const uint kDitherRank[16] = { 0, 7, 3, 7,  7, 4, 7, 7,  2, 7, 1, 7,  7, 6, 7, 5 };
                uint2 pixel = (uint2)((int2)floor(positionSS) & 3);
                uint rank = kDitherRank[pixel.y * 4 + pixel.x];
                int slice = clamp((int)floor(opacity * 0.9375 * 16.0), 0, 15);
                bool covered = slice < 8 ? (int)rank < slice : (int)rank >= 15 - slice;
                clip(covered ? 1.0 : -1.0);
            }

            ShadowVaryings ShadowVertex(Attributes input)
            {
                ShadowVaryings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
            #if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW)
                float3 lightDirectionWS = normalize(_LightPosition - positionWS);
            #else
                float3 lightDirectionWS = _LightDirection;
            #endif
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
                output.positionCS = ApplyShadowClamping(positionCS);
                output.uv = TRANSFORM_TEX(input.uv0, _MainTex);
                return output;
            }

            half4 ShadowFragment(ShadowVaryings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
            #if defined(_ALPHATEST_ON) || defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
                // The original's shadow pass predates _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A: the albedo
                // alpha always counts here.
                half alpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).a * _Color.a;
                #if defined(_ALPHATEST_ON)
                    clip(alpha - _Cutoff);
                #else
                    #if defined(_ALPHAPREMULTIPLY_ON)
                        // Transparent: reflectivity makes the surface more opaque, as in the forward pass.
                        #if defined(_METALLICGLOSSMAP)
                            half metallic = SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, input.uv).r;
                        #else
                            half metallic = _Metallic;
                        #endif
                        half oneMinusReflectivity = OneMinusReflectivityMetallic(metallic);
                        alpha = 1.0 - oneMinusReflectivity + alpha * oneMinusReflectivity;
                    #endif
                    ClipShadowDither(input.positionCS.xy, alpha);
                #endif
            #endif
            #if defined(LOD_FADE_CROSSFADE)
                LODFadeCrossFade(input.positionCS);
            #endif
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
            Cull Off

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            struct DepthOnlyVaryings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;                      // GetTexCoords().xy
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            DepthOnlyVaryings DepthOnlyVertex(Attributes input)
            {
                DepthOnlyVaryings output = (DepthOnlyVaryings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv0, _MainTex);
                return output;
            }

            half DepthOnlyFragment(DepthOnlyVaryings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            #if defined(_ALPHATEST_ON)
                ClipCutout(GetAlpha(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).a));
            #endif
            #if defined(LOD_FADE_CROSSFADE)
                LODFadeCrossFade(input.positionCS);
            #endif
                return input.positionCS.z;
            }
            ENDHLSL
        }

        // Normals for SSAO and other screen-space effects in the forward renderers.
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }

            ZWrite On
            Cull Off

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _PARALLAXMAP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _DETAIL_MULX2
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            struct DepthNormalsVaryings
            {
                float4 positionCS : SV_POSITION;
                float4 texCoords : TEXCOORD0;               // GetTexCoords()
                float3 normalWS : TEXCOORD1;
                float3 tangentWS : TEXCOORD2;
                float3 bitangentWS : TEXCOORD3;
            #if defined(_PARALLAXMAP)
                float3 viewDirForParallax : TEXCOORD4;
            #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            DepthNormalsVaryings DepthNormalsVertex(Attributes input)
            {
                DepthNormalsVaryings output = (DepthNormalsVaryings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = normalInputs.normalWS;
                output.tangentWS = normalInputs.tangentWS;
                output.bitangentWS = normalInputs.bitangentWS;
                output.texCoords = GetTexCoords(input.uv0, input.uv1);
            #if defined(_PARALLAXMAP)
                output.viewDirForParallax = GetViewDirForParallax(input.positionOS.xyz, input.normalOS, input.tangentOS);
            #endif
                return output;
            }

            void DepthNormalsFragment(
                DepthNormalsVaryings input
                , out half4 outNormalWS : SV_Target0
            #ifdef _WRITE_RENDERING_LAYERS
                , out uint outRenderingLayers : SV_Target1
            #endif
            )
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float4 texCoords = input.texCoords;
            #if defined(_PARALLAXMAP)
                texCoords = ApplyParallax(texCoords, input.viewDirForParallax);
            #endif
            #if defined(_ALPHATEST_ON)
                ClipCutout(GetAlpha(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, texCoords.xy).a));
            #endif
            #if defined(LOD_FADE_CROSSFADE)
                LODFadeCrossFade(input.positionCS);
            #endif

                outNormalWS = half4(TransformSurfaceNormalToWorld(GetNormalTS(texCoords), input.tangentWS, input.bitangentWS,
                                                                  input.normalWS), 0.0);
            #ifdef _WRITE_RENDERING_LAYERS
                outRenderingLayers = EncodeMeshRenderingLayer();
            #endif
            }
            ENDHLSL
        }

        // Albedo and emission for the lightmapper, with Standard's allowance for rough specular surfaces.
        Pass
        {
            Name "Meta"
            Tags { "LightMode" = "Meta" }

            Cull Off

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex MetaPassVertex
            #pragma fragment MetaPassFragment

            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _METALLICGLOSSMAP
            #pragma shader_feature_local_fragment _DETAIL_MULX2
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature EDITOR_VISUALIZATION

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"

            struct MetaVaryings
            {
                float4 positionCS : SV_POSITION;
                float4 texCoords : TEXCOORD0;               // GetTexCoords()
            #ifdef EDITOR_VISUALIZATION
                float2 VizUV : TEXCOORD1;
                float4 LightCoord : TEXCOORD2;
            #endif
            };

            MetaVaryings MetaPassVertex(Attributes input)
            {
                MetaVaryings output = (MetaVaryings)0;
                output.positionCS = UnityMetaVertexPosition(input.positionOS.xyz, input.uv1, input.uv2);
                output.texCoords = GetTexCoords(input.uv0, input.uv1);
            #ifdef EDITOR_VISUALIZATION
                UnityEditorVizData(input.positionOS.xyz, input.uv0, input.uv1, input.uv2, output.VizUV, output.LightCoord);
            #endif
                return output;
            }

            half4 MetaPassFragment(MetaVaryings input) : SV_Target
            {
                SurfaceData surfaceData;
                InitializeSurfaceData(input.texCoords, surfaceData);

                // UnityLightmappingAlbedo: the diffuse colour plus half the specular colour, weighted by roughness.
                half oneMinusReflectivity = OneMinusReflectivityMetallic(surfaceData.metallic);
                half3 diffuse = surfaceData.albedo * oneMinusReflectivity;
                half3 specular = lerp(kDielectricSpec.rgb, surfaceData.albedo, surfaceData.metallic);
                half roughness = (1.0 - surfaceData.smoothness) * (1.0 - surfaceData.smoothness);

                MetaInput metaInput = (MetaInput)0;
            #ifdef EDITOR_VISUALIZATION
                metaInput.Albedo = diffuse;
                metaInput.VizUV = input.VizUV;
                metaInput.LightCoord = input.LightCoord;
            #else
                metaInput.Albedo = diffuse + specular * roughness * 0.5;
            #endif
                metaInput.Emission = surfaceData.emission;
                return UnityMetaFragment(metaInput);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "StandardShaderGUI"
}
