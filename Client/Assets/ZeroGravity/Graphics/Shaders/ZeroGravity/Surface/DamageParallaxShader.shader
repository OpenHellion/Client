// Damage overlay meshes at repair and scrap points (FriedElectronics_MAT, RepairPoint_MAT,
// ScrapPoint_MAT), with:
//  * a cut-out that eats into the mesh along a noise texture: VesselRepairPoint lowers _Health as
//    the damage grows, which moves the edge (_Health * _NoiseMax) and uncovers more of the mesh,
//  * two texture layers seen through the surface with parallax, 1 - _Texture1Height and
//    1 - _Texture2Height below it in UV units (1 is on the surface), blended by _Texture2's alpha and
//    darkened by cracks,
//  * emission from _Texture1 through its mask, plus animated lightning (an 8 x 8 flipbook) once
//    VesselRepairPoint sets _SystemDamage above 0.2.
//
// Rebuilt for URP from the Amplify Shader Editor surface shader of the same name, with one deliberate
// change: the parallax (see ParallaxOffset). Everything else matches the original; to check that with
// the verify tools, pin both heights to 1, where neither version shifts the layers:
//   --set _Texture1Height=1 --set _Texture2Height=1
Shader "DamageParallaxShader"
{
    Properties
    {
        [HideInInspector] __dirty ("", Float) = 1
        _MaskClipValue ("Mask Clip Value", Float) = 0.5
        _Texture1 ("Texture1", 2D) = "white" {}
        _Texture1Normal ("Texture1 Normal", 2D) = "bump" {}
        _Texture1Height ("Texture1 Height", Float) = 0
        _Texture1EmissionMask ("Texture1 Emission Mask", 2D) = "white" {}
        _Texture1EmissionColor ("Texture1 Emission Color", Color) = (0, 0, 0, 0)
        _Texture1Emission ("Texture1 Emission", Float) = 0
        _Texture2 ("Texture2", 2D) = "white" {}
        _Texture2Normal ("Texture2 Normal", 2D) = "bump" {}
        _Texture2Height ("Texture2 Height", Float) = 0
        _Metallic ("Metallic", Range(0, 1)) = 0
        _Smoothness ("Smoothness", Range(0, 1)) = 0
        _Crack ("Crack", 2D) = "white" {}
        _Noise ("Noise", 2D) = "white" {}
        _NoiseMax ("NoiseMax", Float) = 0
        _Health ("Health", Range(0, 1)) = 0
        _Lightning ("Lightning", 2D) = "white" {}
        _LightningBrightness ("LightningBrightness", Float) = 0
        _SystemDamage ("SystemDamage", Range(0, 1)) = 0
        [HideInInspector] _texcoord ("", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "TransparentCutout"
            "Queue" = "AlphaTest+0"
            "UniversalMaterialType" = "Lit"
            "IgnoreProjector" = "True"
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
            float4 _Texture1_ST;
            float4 _Texture2_ST;
            float4 _Noise_ST;
            float4 _Crack_ST;
            float4 _Texture1EmissionColor;
            float _MaskClipValue;
            float _Texture1Height;
            float _Texture1Emission;
            float _Texture2Height;
            float _Metallic;
            float _Smoothness;
            float _NoiseMax;
            float _Health;
            float _LightningBrightness;
            float _SystemDamage;
        CBUFFER_END

        TEXTURE2D(_Texture1);               SAMPLER(sampler_Texture1);
        TEXTURE2D(_Texture1Normal);         SAMPLER(sampler_Texture1Normal);
        TEXTURE2D(_Texture1EmissionMask);   SAMPLER(sampler_Texture1EmissionMask);
        TEXTURE2D(_Texture2);               SAMPLER(sampler_Texture2);
        TEXTURE2D(_Texture2Normal);         SAMPLER(sampler_Texture2Normal);
        TEXTURE2D(_Crack);                  SAMPLER(sampler_Crack);
        TEXTURE2D(_Noise);                  SAMPLER(sampler_Noise);
        TEXTURE2D(_Lightning);              SAMPLER(sampler_Lightning);

        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float2 uv0 : TEXCOORD0;
            float2 uv1 : TEXCOORD1;     // Unity lightmap UVs, used only by the Meta pass
            float2 uv2 : TEXCOORD2;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        // xy: UV0 with the hidden Amplify Shader Editor tiling _texcoord (normally identity).
        // zw: UV0 as it is, for the lightning flipbook. The textures' own tilings are applied per pixel,
        // as in the original.
        float4 GetVertexUVs(float2 uv0)
        {
            return float4(TRANSFORM_TEX(uv0, _texcoord), uv0);
        }

        // The DEFERRED pass's tangent-space view direction, computed per vertex and not normalized.
        float3 GetVertexViewDirectionTS(float3 positionWS, VertexNormalInputs normalInputs)
        {
            float3 toCamera = GetCameraPositionWS() - positionWS;
            return float3(dot(toCamera, normalInputs.tangentWS), dot(toCamera, normalInputs.bitangentWS),
                          dot(toCamera, normalInputs.normalWS));
        }

        // The FORWARD passes' tangent-space view direction: the per-pixel direction to the camera in the
        // interpolated tangent frame, which isn't renormalized. The original uses the camera position
        // for orthographic cameras too, where URP's GetWorldSpaceNormalizeViewDir uses the view axis.
        float3 GetPixelViewDirectionTS(float3 positionWS, float3 tangentWS, float3 bitangentWS, float3 normalWS)
        {
            float3 viewDirWS = normalize(GetCameraPositionWS() - positionWS);
            return mul(float3x3(tangentWS, bitangentWS, normalWS), viewDirWS);
        }

        // Where the view ray meets a flat layer lying (1 - height) below the surface, in UV units. The
        // original used Amplify Shader Editor's Bump Offset node, (height - 1) * viewDir.xy + uv, which
        // leaves out the division by viewDir.z: its shift grows with the sine of the viewing angle
        // instead of the tangent and never exceeds the depth, so the layers seemed to turn towards the
        // player rather than lie below the surface. Changed on request. Beyond kMinViewCosine (about
        // 78 degrees from the normal) the shift stops growing, so it stays finite at grazing angles.
        // viewDirTS need not be normalized.
        float2 ParallaxOffset(float2 uv, float height, float3 viewDirTS)
        {
            const float kMinViewCosine = 0.2;
            float3 viewDir = normalize(viewDirTS);
            return (height - 1.0) * viewDir.xy / max(viewDir.z, kMinViewCosine) + uv;
        }

        // 0 where the noise is `width` or more below the damage edge at _Health * _NoiseMax, rising to 1
        // at the edge. Written as the original computes it, so it rounds the same.
        float GetDamageMask(float noise, float width)
        {
            float edge = _Health * _NoiseMax;
            float start = edge - width;
            return saturate((noise - start) / (edge - start));
        }

        // The alpha test: the mesh shows past the damage edge, except in the cracks. The surface passes
        // pass GetVertexUVs().xy; the shadow caster passes UV0 without _texcoord_ST, as in the original.
        void ClipDamage(float2 uv)
        {
            float noise = SAMPLE_TEXTURE2D(_Noise, sampler_Noise, TRANSFORM_TEX(uv, _Noise)).r;
            float crack = SAMPLE_TEXTURE2D(_Crack, sampler_Crack, TRANSFORM_TEX(uv, _Crack)).r;
            clip(saturate(GetDamageMask(noise, 0.05) - crack) - _MaskClipValue);
        }

        // Amplify Shader Editor's Flipbook UV Animation node on an 8 x 8 sheet, played from the top left
        // at 10 frames per second, rising to 50 with _SystemDamage.
        float2 GetLightningFlipbookUV(float2 uv)
        {
            const float columns = 8.0;
            const float rows = 8.0;
            float framesPerSecond = (_SystemDamage * 0.8 + 0.2) * 50.0;
            float frame = round(fmod(_Time.y * framesPerSecond, columns * rows));
            frame += frame < 0.0 ? columns * rows : 0.0;
            float column = round(fmod(frame, columns));
            float row = (rows - 1.0) - round(fmod((frame - column) / columns, rows));
            return uv / float2(columns, rows) + float2(column / columns, row / rows);
        }

        // The lightning, shown only while _SystemDamage is above 0.2 and brighter with more damage.
        // uv is GetVertexUVs().zw.
        half3 GetLightningEmission(float2 uv)
        {
            half3 lightning = SAMPLE_TEXTURE2D(_Lightning, sampler_Lightning, GetLightningFlipbookUV(uv)).rgb * _LightningBrightness;
            half strength = _SystemDamage > 0.2 ? _SystemDamage * 0.5 + 0.5 : 0.0;
            return lightning * strength;
        }

        // uv and flipbookUV are GetVertexUVs().xy and .zw; viewDirTS is the tangent-space direction to
        // the camera, of any length. Clips the damaged-away pixels.
        void InitializeSurfaceData(float2 uv, float2 flipbookUV, float3 viewDirTS, out SurfaceData surfaceData)
        {
            ClipDamage(uv);

            float2 uv1 = ParallaxOffset(TRANSFORM_TEX(uv, _Texture1), _Texture1Height, viewDirTS);
            float2 uv2 = ParallaxOffset(TRANSFORM_TEX(uv, _Texture2), _Texture2Height, viewDirTS);

            // Layer 1 fades in over the last 0.1 of noise before the damage edge. The noise and cracks
            // are sampled with each layer's UVs.
            half3 texture1 = SAMPLE_TEXTURE2D(_Texture1, sampler_Texture1, uv1).rgb;
            half noise1 = SAMPLE_TEXTURE2D(_Noise, sampler_Noise, uv1).r;
            half3 crack1 = SAMPLE_TEXTURE2D(_Crack, sampler_Crack, uv1).rgb;
            half3 layer1 = (1.0 - crack1) * texture1 * GetDamageMask(noise1, 0.1);

            // Layer 2 covers layer 1 by its alpha, and is darkened where the noise is low.
            half4 texture2 = SAMPLE_TEXTURE2D(_Texture2, sampler_Texture2, uv2);
            half noise2 = SAMPLE_TEXTURE2D(_Noise, sampler_Noise, uv2).r;
            half3 crack2 = SAMPLE_TEXTURE2D(_Crack, sampler_Crack, uv2).rgb;
            half3 layer2 = texture2.a * texture2.rgb * (1.0 - crack2);
            half3 albedo = layer2 * saturate((noise2 + 0.1) / 1.1) + layer1 * (1.0 - texture2.a);

            half3 normal1 = UnpackNormal(SAMPLE_TEXTURE2D(_Texture1Normal, sampler_Texture1Normal, uv1));
            half3 normal2 = UnpackNormal(SAMPLE_TEXTURE2D(_Texture2Normal, sampler_Texture2Normal, uv2));

            // Layer 1's emission shows even where layer 2 or the damage mask hides its colour.
            half emissionMask = SAMPLE_TEXTURE2D(_Texture1EmissionMask, sampler_Texture1EmissionMask, uv1).r;
            half3 emission = emissionMask * texture1 * _Texture1Emission * _Texture1EmissionColor.rgb;
            emission += GetLightningEmission(flipbookUV);

            surfaceData = (SurfaceData)0;
            surfaceData.albedo = albedo;
            surfaceData.alpha = 1.0;
            surfaceData.metallic = _Metallic;
            surfaceData.smoothness = _Smoothness;
            surfaceData.occlusion = 1.0;
            surfaceData.emission = emission;
            // Blended unnormalized; the world-space normal is normalized.
            surfaceData.normalTS = normal2 * texture2.a + normal1 * (1.0 - texture2.a);
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
            float4 uv : TEXCOORD0;                          // GetVertexUVs(): xy surface, zw lightning
            float3 positionWS : TEXCOORD1;
            float3 normalWS : TEXCOORD2;
            float3 tangentWS : TEXCOORD3;
            float3 bitangentWS : TEXCOORD4;
            float3 viewDirTS : TEXCOORD5;                   // per vertex, for the GBuffer pass
            half4 fogFactorAndVertexLight : TEXCOORD6;      // x: fog factor, yzw: per-vertex lights
            half3 vertexSH : TEXCOORD7;
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
            output.viewDirTS = GetVertexViewDirectionTS(positionInputs.positionWS, normalInputs);
            output.uv = GetVertexUVs(input.uv0);

            half fogFactor = 0;
        #if !defined(_FOG_FRAGMENT)
            fogFactor = ComputeFogFactor(positionInputs.positionCS.z);
        #endif
            output.fogFactorAndVertexLight = half4(fogFactor, VertexLighting(positionInputs.positionWS, normalInputs.normalWS));

            // Ambient light comes from light probes only: the game's build had no lightmap variants.
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

        // The original's passes get the parallax view direction differently (Unity's surface shader
        // generator did that). This is the GBuffer pass's, as in the original's DEFERRED pass: the
        // per-vertex direction, interpolated. The game rendered with this one.
        SurfaceData GetLitSurfaceData(LitVaryings input)
        {
            SurfaceData surfaceData;
            InitializeSurfaceData(input.uv.xy, input.uv.zw, input.viewDirTS, surfaceData);
            return surfaceData;
        }

        // The ForwardLit pass's, as in the original's FORWARD and FORWARD_DELTA passes: per pixel.
        SurfaceData GetForwardLitSurfaceData(LitVaryings input)
        {
            float3 viewDirTS = GetPixelViewDirectionTS(input.positionWS, input.tangentWS, input.bitangentWS, input.normalWS);
            SurfaceData surfaceData;
            InitializeSurfaceData(input.uv.xy, input.uv.zw, viewDirTS, surfaceData);
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

                SurfaceData surfaceData = GetForwardLitSurfaceData(input);
            #if defined(LOD_FADE_CROSSFADE)
                LODFadeCrossFade(input.positionCS);
            #endif

                InputData inputData;
                InitializeInputData(input, surfaceData.normalTS, inputData);
            #if defined(_DBUFFER)
                ApplyDecalToSurfaceData(input.positionCS, surfaceData, inputData);
            #endif

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
                float2 uv : TEXCOORD0;                      // UV0 without _texcoord_ST, as in the original
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

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
                ClipDamage(input.uv);
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
                output.uv = GetVertexUVs(input.uv0).xy;
                return output;
            }

            half DepthOnlyFragment(DepthOnlyVaryings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                ClipDamage(input.uv);
            #if defined(LOD_FADE_CROSSFADE)
                LODFadeCrossFade(input.positionCS);
            #endif
                return input.positionCS.z;
            }
            ENDHLSL
        }

        // Normals for SSAO and other screen-space effects in the forward renderers, so they follow
        // the ForwardLit pass's parallax.
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
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 tangentWS : TEXCOORD3;
                float3 bitangentWS : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            DepthNormalsVaryings DepthNormalsVertex(Attributes input)
            {
                DepthNormalsVaryings output = (DepthNormalsVaryings)0;
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
                output.uv = GetVertexUVs(input.uv0).xy;
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

                // Only the normal is used; the compiler drops the colour and emission samples.
                float3 viewDirTS = GetPixelViewDirectionTS(input.positionWS, input.tangentWS, input.bitangentWS, input.normalWS);
                SurfaceData surfaceData;
                InitializeSurfaceData(input.uv, 0.0, viewDirTS, surfaceData);
            #if defined(LOD_FADE_CROSSFADE)
                LODFadeCrossFade(input.positionCS);
            #endif

                outNormalWS = half4(TransformSurfaceNormalToWorld(surfaceData.normalTS, input.tangentWS, input.bitangentWS,
                                                                  input.normalWS), 0.0);
            #ifdef _WRITE_RENDERING_LAYERS
                outRenderingLayers = EncodeMeshRenderingLayer();
            #endif
            }
            ENDHLSL
        }

        // Albedo and emission for the lightmapper, with the damaged-away holes. There is no parallax
        // here; the view-dependent offset means nothing to the lightmapper.
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
                output.uv = GetVertexUVs(input.uv0);
            #ifdef EDITOR_VISUALIZATION
                UnityEditorVizData(input.positionOS.xyz, input.uv0, input.uv1, input.uv2, output.VizUV, output.LightCoord);
            #endif
                return output;
            }

            half4 MetaPassFragment(MetaVaryings input) : SV_Target
            {
                // Looking straight down the normal: no parallax offset.
                SurfaceData surfaceData;
                InitializeSurfaceData(input.uv.xy, input.uv.zw, float3(0.0, 0.0, 1.0), surfaceData);

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
