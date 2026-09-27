// The gas giant Bethyr (BethyrTop_MAT, BethyrBottom_MAT, BethyrBelt01-04_MAT, MiniBethyr1):
//  * Tessellated by distance from the camera (_Tess triangles per edge up close, from _MaxDist down to
//    _MinDist), optionally rounded with Phong tessellation and displaced along the normal by _DispMap.
//  * Its clouds flow: _MainTex and _NormalMap are sampled twice, pushed along _FlowMap by two
//    offsets half a cycle apart, and cross-faded so the restart of each cycle is never seen. The flow
//    and the normal map fade out towards the UV edges (_VignetteMin/_VignetteMax), so the seams of the
//    separate cloud-band meshes line up.
//  * Two tilings of _NormalDetail (x100 and x10, flowing with the clouds) add detail close to the
//    camera and fade out with distance.
//  * Lit through _LightRamp: the ramp is looked up with N.L plus a rim highlight where the surface
//    reflects the light at grazing angles, and scaled by 4, so it can brighten past the texture.
//    Ambient light is the light-probe SH at the geometric normal.
//
// Rebuilt for URP from the tessellated surface shader of the same name (custom lighting, forward
// only). As in the original there is no shadow caster.
Shader "ZeroGravity/Planet/GasPlanetSurfaceShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _LightRamp ("Light Ramp", 2D) = "white" {}
        _NormalMap ("Normal", 2D) = "bump" {}
        _DispMap ("Displacement Map", 2D) = "gray" {}
        _DispAmount ("Displacement Amount", Range(0, 10)) = 0
        _FlowMap ("Flow Map", 2D) = "gray" {}
        _FlowSpeed ("Flow Speed", Range(-10, 10)) = 0
        _FlowAmount ("Flow Amount", Range(-1, 1)) = 0
        _VignetteMin ("Vignette Min", Range(0, 1)) = 0
        _VignetteMax ("Vignette Max", Range(0, 1)) = 1
        _Tess ("Tessellation", Range(1, 128)) = 1
        _TessPhong ("Tessallation Phong", Range(0, 1)) = 0
        _MinDist ("Min Distance", Float) = 10
        _MaxDist ("Max Distance", Float) = 25
        _NormalDetail ("Normal Detail", 2D) = "bump" {}
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
        }
        LOD 100

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        TEXTURE2D(_MainTex);        SAMPLER(sampler_MainTex);
        TEXTURE2D(_LightRamp);      SAMPLER(sampler_LightRamp);
        TEXTURE2D(_NormalMap);      SAMPLER(sampler_NormalMap);
        TEXTURE2D(_DispMap);        SAMPLER(sampler_DispMap);
        TEXTURE2D(_FlowMap);        SAMPLER(sampler_FlowMap);
        TEXTURE2D(_NormalDetail);   SAMPLER(sampler_NormalDetail);

        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _NormalMap_ST;
            float4 _FlowMap_ST;
            float4 _NormalDetail_ST;
            float _DispAmount;
            float _FlowSpeed;
            float _FlowAmount;
            float _VignetteMin;
            float _VignetteMax;
            float _Tess;
            float _TessPhong;
            float _MinDist;
            float _MaxDist;
        CBUFFER_END

        // ------------------------------------------------------------------------------------------
        // Tessellation: the vertex stage passes the mesh through, the hull stage picks the factors,
        // and the domain stage places, displaces and transforms the new vertices.
        // ------------------------------------------------------------------------------------------

        struct Attributes
        {
            float4 positionOS : POSITION;
            float4 tangentOS : TANGENT;
            float3 normalOS : NORMAL;
            float2 uv : TEXCOORD0;
        };

        struct ControlPoint
        {
            float4 positionOS : INTERNALTESSPOS;
            float4 tangentOS : TANGENT;
            float3 normalOS : NORMAL;
            float2 uv : TEXCOORD0;
        };

        struct TessellationFactors
        {
            float edge[3] : SV_TessFactor;
            float inside : SV_InsideTessFactor;
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float4 uvMainAndNormal : TEXCOORD0;     // xy: _MainTex, zw: _NormalMap
            float4 uvFlowAndDetail : TEXCOORD1;     // xy: _FlowMap, zw: _NormalDetail
            float3 positionWS : TEXCOORD2;
            float3 normalWS : TEXCOORD3;
            float3 tangentWS : TEXCOORD4;
            float3 bitangentWS : TEXCOORD5;
            float3 ambient : TEXCOORD6;             // light-probe SH at the geometric normal
        };

        ControlPoint GasPlanetVertex(Attributes input)
        {
            ControlPoint output;
            output.positionOS = input.positionOS;
            output.tangentOS = input.tangentOS;
            output.normalOS = input.normalOS;
            output.uv = input.uv;
            return output;
        }

        // _Tess at _MinDist from the camera and closer, falling to 1% of it at _MaxDist and beyond.
        float DistanceTessellation(float4 positionOS)
        {
            float3 positionWS = mul(GetObjectToWorldMatrix(), positionOS).xyz;
            float distance = length(positionWS - GetCameraPositionWS());
            float closeness = clamp(1.0 - (distance - _MinDist) / (_MaxDist - _MinDist), 0.01, 1.0);
            return closeness * _Tess;
        }

        TessellationFactors GasPlanetPatchConstants(InputPatch<ControlPoint, 3> patch)
        {
            float3 factors = float3(DistanceTessellation(patch[0].positionOS),
                                    DistanceTessellation(patch[1].positionOS),
                                    DistanceTessellation(patch[2].positionOS));
            TessellationFactors output;
            output.edge[0] = (factors.y + factors.z) * 0.5;
            output.edge[1] = (factors.z + factors.x) * 0.5;
            output.edge[2] = (factors.x + factors.y) * 0.5;
            output.inside = (factors.x + factors.y + factors.z) * (1.0 / 3.0);
            return output;
        }

        [domain("tri")]
        [partitioning("fractional_odd")]
        [outputtopology("triangle_cw")]
        [patchconstantfunc("GasPlanetPatchConstants")]
        [outputcontrolpoints(3)]
        ControlPoint GasPlanetHull(InputPatch<ControlPoint, 3> patch, uint id : SV_OutputControlPointID)
        {
            return patch[id];
        }

        // A point projected onto the tangent plane of one corner of the triangle (Phong tessellation).
        float3 ProjectOntoCornerPlane(float3 position, ControlPoint corner)
        {
            return position - corner.normalOS * (dot(position, corner.normalOS) - dot(corner.positionOS.xyz, corner.normalOS));
        }

        [domain("tri")]
        Varyings GasPlanetDomain(TessellationFactors factors, OutputPatch<ControlPoint, 3> patch, float3 barycentric : SV_DomainLocation)
        {
            #define INTERPOLATE(field) (patch[0].field * barycentric.x + patch[1].field * barycentric.y + patch[2].field * barycentric.z)
            float4 positionOS = INTERPOLATE(positionOS);
            float4 tangentOS = INTERPOLATE(tangentOS);
            float3 normalOS = INTERPOLATE(normalOS);
            float2 uv = INTERPOLATE(uv);
            #undef INTERPOLATE

            float3 phongPosition = ProjectOntoCornerPlane(positionOS.xyz, patch[0]) * barycentric.x
                                 + ProjectOntoCornerPlane(positionOS.xyz, patch[1]) * barycentric.y
                                 + ProjectOntoCornerPlane(positionOS.xyz, patch[2]) * barycentric.z;
            positionOS.xyz = lerp(positionOS.xyz, phongPosition, _TessPhong);

            // Displacement around the middle of the map: grey (0.5) leaves the surface where it is.
            float height = SAMPLE_TEXTURE2D_LOD(_DispMap, sampler_DispMap, uv, 0).r;
            positionOS.xyz += normalOS * (height * _DispAmount - _DispAmount * 0.5);

            Varyings output;
            output.positionWS = TransformObjectToWorld(positionOS.xyz);
            output.positionCS = TransformWorldToHClip(output.positionWS);
            output.uvMainAndNormal = float4(TRANSFORM_TEX(uv, _MainTex), TRANSFORM_TEX(uv, _NormalMap));
            output.uvFlowAndDetail = float4(TRANSFORM_TEX(uv, _FlowMap), TRANSFORM_TEX(uv, _NormalDetail));

            VertexNormalInputs normalInputs = GetVertexNormalInputs(normalOS, tangentOS);
            output.normalWS = normalInputs.normalWS;
            output.tangentWS = normalInputs.tangentWS;
            output.bitangentWS = normalInputs.bitangentWS;
            output.ambient = SampleSH(normalInputs.normalWS);
            return output;
        }

        // ------------------------------------------------------------------------------------------
        // The surface: flowing clouds and normals
        // ------------------------------------------------------------------------------------------

        // x and y of a normal-map texel in either packing (DXT5nm .ag, BC5 .rg). The two flow samples
        // are blended as raw texels first, as in the original.
        float2 UnpackNormalXY(float4 packedNormal)
        {
            return float2(packedNormal.a * packedNormal.r, packedNormal.g) * 2.0 - 1.0;
        }

        struct GasPlanetSurface
        {
            float3 albedo;
            float3 normalWS;
        };

        GasPlanetSurface GetGasPlanetSurface(Varyings input)
        {
            float2 uvMain = input.uvMainAndNormal.xy;
            float2 uvNormal = input.uvMainAndNormal.zw;
            float2 uvDetail = input.uvFlowAndDetail.zw;

            // 0 inside the UV square, rising to 1 across the band between _VignetteMin and _VignetteMax
            // from each edge. It turns the flow and the normal map off at the mesh's edges.
            float2 fromLowEdge = saturate((uvMain - _VignetteMin) / (_VignetteMax - _VignetteMin));
            float2 fromHighEdge = saturate((uvMain - (1.0 - _VignetteMin)) / ((1.0 - _VignetteMax) - (1.0 - _VignetteMin)));
            float edgeMask = 1.0 - fromLowEdge.x * fromHighEdge.x * fromLowEdge.y * fromHighEdge.y;

            // Two flow cycles half a period apart, each running its offset from -1 to 1 times the flow.
            float cycleA = frac(_FlowSpeed * _Time.x);
            float cycleB = frac(_FlowSpeed * _Time.x + 0.5);
            float2 flow = pow(SAMPLE_TEXTURE2D(_FlowMap, sampler_FlowMap, input.uvFlowAndDetail.xy).rg, 0.45) * 2.0 - 1.0;
            float2 unmaskedOffsetA = (cycleA * 2.0 - 1.0) * flow * _FlowAmount;
            float2 unmaskedOffsetB = (cycleB * 2.0 - 1.0) * flow * _FlowAmount;
            float2 offsetA = unmaskedOffsetA - edgeMask * unmaskedOffsetA;
            float2 offsetB = unmaskedOffsetB - edgeMask * unmaskedOffsetB;

            // Weight of cycle A: 1 halfway through it (no offset), 0 as it wraps around.
            float easedCycleA = smoothstep(0.0, 1.0, cycleA);
            float weightA = saturate(2.0 - 2.0 * easedCycleA) * min(2.0 * easedCycleA, 1.0);

            GasPlanetSurface surface;
            float3 colourA = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvMain - offsetA).rgb;
            float3 colourB = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvMain - offsetB).rgb;
            surface.albedo = lerp(colourB, colourA, weightA);

            float4 packedA = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uvNormal - offsetA);
            float4 packedB = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uvNormal - offsetB);
            float3 baseNormal;
            baseNormal.xy = UnpackNormalXY(lerp(packedB, packedA, weightA));
            baseNormal.z = sqrt(1.0 - min(dot(baseNormal.xy, baseNormal.xy), 1.0));

            // The detail normal map at two tilings, the flow offsets scaled with them. The fine one is
            // flattened less (z = 1.5) than the coarse one (z = 6).
            float2 fineUVA = uvDetail * 100.0 - (unmaskedOffsetA * 100.0 - edgeMask * unmaskedOffsetA * 100.0);
            float2 fineUVB = uvDetail * 100.0 - (unmaskedOffsetB * 100.0 - edgeMask * unmaskedOffsetB * 100.0);
            float2 coarseUVA = uvDetail * 10.0 - (unmaskedOffsetA * 10.0 - edgeMask * unmaskedOffsetA * 10.0);
            float2 coarseUVB = uvDetail * 10.0 - (unmaskedOffsetB * 10.0 - edgeMask * unmaskedOffsetB * 10.0);
            float4 packedFine = lerp(SAMPLE_TEXTURE2D(_NormalDetail, sampler_NormalDetail, fineUVB),
                                     SAMPLE_TEXTURE2D(_NormalDetail, sampler_NormalDetail, fineUVA), weightA);
            float4 packedCoarse = lerp(SAMPLE_TEXTURE2D(_NormalDetail, sampler_NormalDetail, coarseUVB),
                                       SAMPLE_TEXTURE2D(_NormalDetail, sampler_NormalDetail, coarseUVA), weightA);
            float3 fineDetail = normalize(float3(UnpackNormalXY(packedFine), 1.5));
            float3 coarseDetail = normalize(float3(UnpackNormalXY(packedCoarse), 6.0));

            // Fine detail up to 1 unit from the camera, coarse by 5, none by 50.
            float cameraDistance = length(GetCameraPositionWS() - input.positionWS);
            float2 detailFade = clamp((cameraDistance - float2(1.0, 5.0)) * float2(1.0 / 4.0, 1.0 / 45.0), 0.001, 0.999);
            float3 detail = lerp(fineDetail, coarseDetail, detailFade.x);
            detail = lerp(detail, float3(0.0, 0.0, 1.0), detailFade.y);

            // Reoriented normal mapping: the detail normal rotated onto the base normal.
            float3 t = baseNormal + float3(0.0, 0.0, 1.0);
            float3 u = detail * float3(-1.0, -1.0, 1.0);
            float3 normalTS = t * dot(t, u) / t.z - u;
            normalTS = lerp(normalTS, float3(0.0, 0.0, 1.0), edgeMask);

            surface.normalWS = normalize(mul(normalTS, float3x3(input.tangentWS, input.bitangentWS, input.normalWS)));
            return surface;
        }
        ENDHLSL

        Pass
        {
            Name "GasPlanet"
            // The original had no DEFERRED pass (custom lighting), so it stays forward-only: drawn by
            // both the Forward and the Deferred renderers. Its FORWARDADD pass is the additional-light
            // loop below.
            Tags { "LightMode" = "UniversalForwardOnly" }

            HLSLPROGRAM
            #pragma target 5.0
            #pragma require tessellation tessHW
            #pragma vertex GasPlanetVertex
            #pragma hull GasPlanetHull
            #pragma domain GasPlanetDomain
            #pragma fragment GasPlanetFragment

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile _ _LIGHT_LAYERS

            // Where one light reads the ramp: N.L, plus a highlight where the reflected view ray points
            // at the light, strongest at grazing angles (where N.V is below 0.7).
            float2 RampUV(float3 normalWS, float3 viewDirectionWS, float3 lightDirectionWS)
            {
                float grazing = 1.0 - saturate(dot(normalWS, viewDirectionWS) * (1.0 / 0.7));
                float3 reflectedWS = reflect(-viewDirectionWS, normalWS);
                float highlight = saturate(grazing * ((dot(reflectedWS, lightDirectionWS) - 0.5) * (2.0 / 3.0)));
                return float2((dot(normalWS, lightDirectionWS) + highlight * 2.0) * 0.5 + 0.5, 0.0);
            }

            float3 RampLight(float3 albedo, Light light, float3 rampSample)
            {
                float attenuation = light.distanceAttenuation * light.shadowAttenuation;
                return clamp(rampSample * 4.0 * (albedo * light.color) * attenuation, 0.0, 10.0);
            }

            // Additional lights read the ramp's top mip: inside the light loops, whose length varies
            // from pixel to pixel, the derivatives a mip level is chosen from are undefined.
            float3 AdditionalRampLight(float3 albedo, float3 normalWS, float3 viewDirectionWS, Light light)
            {
                float2 rampUV = RampUV(normalWS, viewDirectionWS, light.direction);
                float3 ramp = SAMPLE_TEXTURE2D_LOD(_LightRamp, sampler_LightRamp, rampUV, 0).rgb;
                return RampLight(albedo, light, ramp);
            }

            float4 GasPlanetFragment(Varyings input) : SV_Target
            {
                GasPlanetSurface surface = GetGasPlanetSurface(input);
                float3 viewDirectionWS = normalize(GetCameraPositionWS() - input.positionWS);

                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                Light mainLight = GetMainLight(shadowCoord, input.positionWS, half4(1.0, 1.0, 1.0, 1.0));
                uint meshRenderingLayers = GetMeshRenderingLayer();

                float3 colour = surface.albedo * input.ambient;
            #ifdef _LIGHT_LAYERS
                if (IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers))
            #endif
                {
                    float2 rampUV = RampUV(surface.normalWS, viewDirectionWS, mainLight.direction);
                    float3 ramp = SAMPLE_TEXTURE2D(_LightRamp, sampler_LightRamp, rampUV).rgb;
                    colour += RampLight(surface.albedo, mainLight, ramp);
                }

            #if defined(_ADDITIONAL_LIGHTS)
                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);

                #if USE_CLUSTER_LIGHT_LOOP
                [loop] for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
                {
                    CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK
                    Light light = GetAdditionalLight(lightIndex, input.positionWS);
                #ifdef _LIGHT_LAYERS
                    if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
                #endif
                        colour += AdditionalRampLight(surface.albedo, surface.normalWS, viewDirectionWS, light);
                }
                #endif

                uint pixelLightCount = GetAdditionalLightsCount();
                LIGHT_LOOP_BEGIN(pixelLightCount)
                    Light light = GetAdditionalLight(lightIndex, input.positionWS);
                #ifdef _LIGHT_LAYERS
                    if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
                #endif
                        colour += AdditionalRampLight(surface.albedo, surface.normalWS, viewDirectionWS, light);
                LIGHT_LOOP_END
            #endif

                return float4(colour, 1.0);
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
            #pragma target 5.0
            #pragma require tessellation tessHW
            #pragma vertex GasPlanetVertex
            #pragma hull GasPlanetHull
            #pragma domain GasPlanetDomain
            #pragma fragment DepthOnlyFragment

            half DepthOnlyFragment(Varyings input) : SV_Target
            {
                return input.positionCS.z;
            }
            ENDHLSL
        }

        // Forward-only, so the depth-normals prepass (SSAO and other screen-space effects) needs this
        // in every renderer, Deferred+ included. The normal is the flowing, normal-mapped one.
        Pass
        {
            Name "DepthNormalsOnly"
            Tags { "LightMode" = "DepthNormalsOnly" }

            ZWrite On

            HLSLPROGRAM
            #pragma target 5.0
            #pragma require tessellation tessHW
            #pragma vertex GasPlanetVertex
            #pragma hull GasPlanetHull
            #pragma domain GasPlanetDomain
            #pragma fragment DepthNormalsFragment

            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            void DepthNormalsFragment(
                Varyings input
                , out half4 outNormalWS : SV_Target0
            #ifdef _WRITE_RENDERING_LAYERS
                , out uint outRenderingLayers : SV_Target1
            #endif
            )
            {
                // Only the normal is used; the compiler drops the colour samples.
                GasPlanetSurface surface = GetGasPlanetSurface(input);
                outNormalWS = half4(surface.normalWS, 0.0);
            #ifdef _WRITE_RENDERING_LAYERS
                outRenderingLayers = EncodeMeshRenderingLayer();
            #endif
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
