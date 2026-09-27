// Alpha-tested metallic PBR surface for station parts (doors, lights, ceiling frames, cables) with:
//  * cut-out holes where _MainTex alpha is below _MaskClipValue,
//  * pre-baked lighting in _Lightmap on UV4, added as emission,
//  * emission that changes colour with the room's atmosphere state (_Float2 picks _EmissionColor,
//    _Color3 or _Color1), switched by _EmissionControl unless _AlwaysEmit is set,
//  * shadows thinned out by a 4x4 dither pattern: _Cutoff is the shadow's opacity.
//
// Rebuilt for URP from the Amplify Shader Editor surface shader of the same name. Unlike StandardLightMap,
// _EmissionControl here scales the baked lighting too.
Shader "ZeroGravity/Surface/StandardLightMapCutout"
{
    Properties
    {
        [HideInInspector] __dirty ("", Float) = 1
        _MaskClipValue ("Mask Clip Value", Float) = 0.5
        _MainTex ("MainTex", 2D) = "white" {}
        _Color ("Color", Color) = (0, 0, 0, 0)
        _BumpMap ("BumpMap", 2D) = "bump" {}
        _BumpScale ("BumpScale", Float) = 0
        _EmissionMap ("EmissionMap", 2D) = "white" {}
        [HDR] _EmissionColor ("EmissionColor", Color) = (0, 0, 0, 0)
        _MetallicGlossMap ("MetallicGlossMap", 2D) = "white" {}
        _GlossMapScale ("GlossMapScale", Range(0, 1)) = 0
        _OcclusionMap ("OcclusionMap", 2D) = "white" {}
        _Lightmap ("Lightmap", 2D) = "black" {}
        _EmissionControl ("EmissionControl", Range(0, 1)) = 0
        [IntRange] _Float2 ("Float 2", Range(0, 3)) = 0
        [Toggle] _AlwaysEmit ("AlwaysEmit", Range(0, 1)) = 0
        [HDR] _Color3 ("Color 3", Color) = (2, 1.034483, 0, 0)
        _Cutoff ("Cutoff", Range(0, 1)) = 0
        [HDR] _Color1 ("Color 1", Color) = (2, 0, 0, 0)
        [HideInInspector] _texcoord ("", 2D) = "white" {}
        [HideInInspector] _texcoord4 ("", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "TransparentCutout"
            "Queue" = "Geometry+0"
            "UniversalMaterialType" = "Lit"
            "IsEmissive" = "true"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
        #endif

        CBUFFER_START(UnityPerMaterial)
            float4 _texcoord_ST;
            float4 _texcoord4_ST;
            float4 _MainTex_ST;
            float4 _Lightmap_ST;
            float4 _Color;
            float4 _EmissionColor;
            float4 _Color3;
            float4 _Color1;
            float _MaskClipValue;
            float _BumpScale;
            float _GlossMapScale;
            float _EmissionControl;
            float _AlwaysEmit;
            float _Float2;
            float _Cutoff;
        CBUFFER_END

        TEXTURE2D(_MainTex);            SAMPLER(sampler_MainTex);
        TEXTURE2D(_BumpMap);            SAMPLER(sampler_BumpMap);
        TEXTURE2D(_MetallicGlossMap);   SAMPLER(sampler_MetallicGlossMap);
        TEXTURE2D(_OcclusionMap);       SAMPLER(sampler_OcclusionMap);
        TEXTURE2D(_EmissionMap);        SAMPLER(sampler_EmissionMap);
        TEXTURE2D(_Lightmap);           SAMPLER(sampler_Lightmap);

        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float2 uv0 : TEXCOORD0;
            float2 uv1 : TEXCOORD1;     // Unity lightmap UVs, used only by the Meta pass
            float2 uv2 : TEXCOORD2;
            float2 uv3 : TEXCOORD3;     // UVs of the baked _Lightmap
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        // xy: UV0 and zw: UV4 with the hidden Amplify Shader Editor tilings _texcoord and _texcoord4
        // (normally identity). The vertex stage applies only these. _MainTex_ST and _Lightmap_ST are
        // applied per pixel, as in the original: UVs rounded differently shift the GPU's filtering.
        float4 GetVertexUVs(float2 uv0, float2 uv3)
        {
            return float4(TRANSFORM_TEX(uv0, _texcoord), TRANSFORM_TEX(uv3, _texcoord4));
        }

        // The alpha test. uv has _MainTex_ST applied.
        void ClipOpacityMask(float2 uv)
        {
            clip(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).a - _MaskClipValue);
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

        // _Float2: 0 normal, 1 toxic atmosphere (_Color3), 2 low pressure (_Color1), 3 off.
        half3 GetStateEmissionColor()
        {
            if (_Float2 < 1.0)
                return _EmissionColor.rgb;
            if (_Float2 < 2.0)
                return _Color3.rgb;
            if (_Float2 < 3.0)
                return _Color1.rgb;
            return 0.0;
        }

        // uv has _MainTex_ST applied, lightmapUV has _Lightmap_ST applied. Clips cut-out pixels.
        void InitializeSurfaceData(float2 uv, float2 lightmapUV, out SurfaceData surfaceData)
        {
            half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
            clip(mainTex.a - _MaskClipValue);

            half3 albedo = mainTex.rgb * _Color.rgb;
            half4 metallicGloss = SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, uv);
            half metallic = metallicGloss.r;
            half occlusion = SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, uv).r;

            // The pre-baked lighting lights the diffuse part of the surface.
            half3 bakedLight = SAMPLE_TEXTURE2D(_Lightmap, sampler_Lightmap, lightmapUV).rgb;
            half3 emission = bakedLight * albedo * (1.0 - metallic) * occlusion;

            // The state colour, masked by the emission map. Lights switch it off through
            // _EmissionControl, together with the baked lighting, unless _AlwaysEmit is set.
            emission += GetStateEmissionColor() * SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, uv).rgb;
            half emissionStrength = _AlwaysEmit >= 0.5 ? 1.0 : _EmissionControl;

            surfaceData = (SurfaceData)0;
            surfaceData.albedo = albedo;
            surfaceData.alpha = 1.0;
            surfaceData.metallic = metallic;
            surfaceData.smoothness = metallicGloss.a * _GlossMapScale;
            surfaceData.occlusion = occlusion;
            surfaceData.emission = emission * emissionStrength;
            surfaceData.normalTS = UnpackScaledNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uv), _BumpScale);
        }

        // ---------------------------------------------------------------------
        // Vertex data shared by the ForwardLit and GBuffer passes
        // ---------------------------------------------------------------------

        struct LitVaryings
        {
            float4 positionCS : SV_POSITION;
            float4 uv : TEXCOORD0;                          // GetVertexUVs(): xy surface, zw _Lightmap
            float3 positionWS : TEXCOORD1;
            float3 normalWS : TEXCOORD2;
            float3 tangentWS : TEXCOORD3;
            float3 bitangentWS : TEXCOORD4;
            half4 fogFactorAndVertexLight : TEXCOORD5;      // x: fog factor, yzw: per-vertex lights
            half3 vertexSH : TEXCOORD6;
        #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            float4 shadowCoord : TEXCOORD7;
        #endif
        #if defined(USE_APV_PROBE_OCCLUSION)
            float4 probeOcclusion : TEXCOORD8;
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
            output.uv = GetVertexUVs(input.uv0, input.uv3);

            half fogFactor = 0;
        #if !defined(_FOG_FRAGMENT)
            fogFactor = ComputeFogFactor(positionInputs.positionCS.z);
        #endif
            output.fogFactorAndVertexLight = half4(fogFactor, VertexLighting(positionInputs.positionWS, normalInputs.normalWS));

            // Ambient light comes from light probes only. The original was compiled without Unity
            // lightmap support, because its baked lighting is _Lightmap.
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

        #if defined(_SCREEN_SPACE_IRRADIANCE)
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
            SurfaceData surfaceData;
            InitializeSurfaceData(TRANSFORM_TEX(input.uv.xy, _MainTex), TRANSFORM_TEX(input.uv.zw, _Lightmap), surfaceData);
            return surfaceData;
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex LitVertex
            #pragma fragment ForwardFragment

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

                // The original's forward pass wrote _Cutoff to alpha, which nothing reads (the game
                // rendered deferred). Opaque URP passes write 1.
                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                outColor = half4(MixFog(color.rgb, inputData.fogCoord), 1.0);
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

            HLSLPROGRAM
            #pragma target 4.5
            #pragma exclude_renderers gles3 glcore
            #pragma vertex LitVertex
            #pragma fragment GBufferFragment

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma multi_compile_fragment _ _RENDER_PASS_ENABLED
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile_fragment _ REFLECTION_PROBE_ROTATION
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_IRRADIANCE
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
                // takes global illumination plus emission.
                BRDFData brdfData;
                InitializeBRDFData(surfaceData.albedo, surfaceData.metallic, surfaceData.specular, surfaceData.smoothness,
                                   surfaceData.alpha, brdfData);
                Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, inputData.shadowMask);
                MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, inputData.shadowMask);
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

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex ShadowVertex
            #pragma fragment ShadowFragment

            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct ShadowVaryings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;                      // UV0 with _MainTex_ST, without _texcoord_ST as in the original
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // Clips the shadow to a 4x4 screen-door pattern covering about `opacity` of the shadow map:
            // the built-in pipeline's _DitherMaskLOD, which the original's shadow caster sampled with the
            // surface alpha that Amplify Shader Editor took from _Cutoff. The texture has 16 slices, picked
            // by floor(opacity * 15). Slices 0 to 7 cover that many of the 16 pixels, adding them in the
            // order of kDitherRank; slices 8 to 15 cover the pixels that slice 15 - n leaves out.
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
                output.uv = input.uv0;
                return output;
            }

            half4 ShadowFragment(ShadowVaryings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                ClipOpacityMask(TRANSFORM_TEX(input.uv, _MainTex));
                ClipShadowDither(input.positionCS.xy, _Cutoff);
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

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            struct DepthOnlyVaryings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;                      // GetVertexUVs().xy
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
                output.uv = GetVertexUVs(input.uv0, input.uv3).xy;
                return output;
            }

            half DepthOnlyFragment(DepthOnlyVaryings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                ClipOpacityMask(TRANSFORM_TEX(input.uv, _MainTex));
            #if defined(LOD_FADE_CROSSFADE)
                LODFadeCrossFade(input.positionCS);
            #endif
                return input.positionCS.z;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }

            ZWrite On

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            struct DepthNormalsVaryings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;                      // GetVertexUVs().xy
                float3 normalWS : TEXCOORD1;
                float3 tangentWS : TEXCOORD2;
                float3 bitangentWS : TEXCOORD3;
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
                output.uv = GetVertexUVs(input.uv0, input.uv3).xy;
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
                float2 uv = TRANSFORM_TEX(input.uv, _MainTex);
                ClipOpacityMask(uv);
            #if defined(LOD_FADE_CROSSFADE)
                LODFadeCrossFade(input.positionCS);
            #endif

                // Normal-mapped normals for SSAO and other screen-space effects.
                half3 normalTS = UnpackScaledNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uv), _BumpScale);
                outNormalWS = half4(TransformSurfaceNormalToWorld(normalTS, input.tangentWS, input.bitangentWS, input.normalWS), 0.0);
            #ifdef _WRITE_RENDERING_LAYERS
                outRenderingLayers = EncodeMeshRenderingLayer();
            #endif
            }
            ENDHLSL
        }

        // Albedo and emission for the lightmapper, with the cut-out holes.
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
                float4 uv : TEXCOORD0;                      // GetVertexUVs()
            #ifdef EDITOR_VISUALIZATION
                float2 VizUV : TEXCOORD1;
                float4 LightCoord : TEXCOORD2;
            #endif
            };

            MetaVaryings MetaPassVertex(Attributes input)
            {
                MetaVaryings output = (MetaVaryings)0;
                output.positionCS = UnityMetaVertexPosition(input.positionOS.xyz, input.uv1, input.uv2);
                output.uv = GetVertexUVs(input.uv0, input.uv3);
            #ifdef EDITOR_VISUALIZATION
                UnityEditorVizData(input.positionOS.xyz, input.uv0, input.uv1, input.uv2, output.VizUV, output.LightCoord);
            #endif
                return output;
            }

            half4 MetaPassFragment(MetaVaryings input) : SV_Target
            {
                SurfaceData surfaceData;
                InitializeSurfaceData(TRANSFORM_TEX(input.uv.xy, _MainTex), TRANSFORM_TEX(input.uv.zw, _Lightmap), surfaceData);

                MetaInput metaInput = (MetaInput)0;
                metaInput.Albedo = surfaceData.albedo;
                metaInput.Emission = surfaceData.emission;
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
