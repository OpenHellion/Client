// OpenHellion/SpecularDecal
//
// Projected decal for URP's DecalProjector, replacing the original game's Decalicious and DeferredDecal decals.
// URP's stock "Shader Graphs/Decal" cannot reproduce them: in the G-buffer it only changes albedo and normals, blends
// with a fixed alpha blend and fades by angle softly. Hellion's decals also change specular colour and smoothness
// (the game uses the specular workflow), blend per material (premultiplied, additive or multiply), use a separate
// coverage mask with an animatable clip reveal and a scrolling emission pulse, and cut off hard at an angle.
//
// Setup: put a DecalProjector on the object with Scale Mode "Inherit From Hierarchy", size (1, 1, 1) and pivot 0,
// so the transform alone defines the projection box, exactly like the old decals. The texture is mapped onto the
// box's local X/Z plane and projected along local Y (the projector's forward axis). SpecularDecal does its own
// angle cut-off (Angle Limit below), so the projector's Start/End Angle Fade settings have no effect.
//
// Passes: DecalGBufferProjector writes straight into the G-buffer under Deferred/Deferred+ (like Decalicious did).
// DecalScreenSpaceProjector is the fallback for the forward renderers and draws the decal fully lit on top.
Shader "OpenHellion/SpecularDecal"
{
	Properties
	{
		[Header(Albedo)]
		[MainTexture] _BaseMap ("Albedo", 2D) = "white" {}
		[MainColor] _BaseColor ("Albedo Tint", Color) = (1, 1, 1, 1)
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Albedo Source Blend", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Albedo Destination Blend", Float) = 10

		[Header(Coverage)]
		[NoScaleOffset] _MaskMap ("Coverage Mask (R)", 2D) = "white" {}
		_AngleLimit ("Angle Limit (Cosine)", Range(-1, 1)) = 0.5
		[Toggle(_MASK_CLIP)] _MaskClip ("Mask Clip Reveal", Float) = 0
		[ShowIfToggle(_MaskClip)] _ClipThreshold ("Clip Threshold", Range(0, 1)) = 1

		[Header(Specular)]
		[NoScaleOffset] _SpecularMap ("Specular (RGB) Coverage (A)", 2D) = "white" {}
		_SpecularColor ("Specular Tint (RGB) Coverage (A)", Color) = (0.2, 0.2, 0.2, 1)
		_Smoothness ("Smoothness", Range(0, 1)) = 0.5

		[Header(Normal)]
		[Enum(Nothing, 0, Smoothness, 1, Normal, 14)] _NormalBufferWrites ("Normal Buffer Writes", Float) = 1
		[Normal][NoScaleOffset] _NormalMap ("Normal", 2D) = "bump" {}
		_NormalStrength ("Normal Strength", Float) = 1

		[Header(Emission)]
		[NoScaleOffset] _EmissionMap ("Emission", 2D) = "white" {}
		[HDR] _EmissionColor ("Emission Tint", Color) = (0, 0, 0, 1)
		[Toggle(_EMISSION_FLOW)] _EmissionFlow ("Emission Flow", Float) = 0
		[ShowIfToggle(_EmissionFlow)][NoScaleOffset] _EmissionFlowMap ("Emission Flow Gradient (R)", 2D) = "white" {}
		[ShowIfToggle(_EmissionFlow)] _EmissionFlowTimeOffset ("Emission Flow Time Offset", Float) = 0

		[Header(Sorting)]
		_DrawOrder ("Draw Order", Range(-50, 50)) = 0
	}

	SubShader
	{
		Tags { "RenderPipeline" = "UniversalPipeline" "PreviewType" = "Plane" }

		HLSLINCLUDE
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/NormalReconstruction.hlsl"
		#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderVariablesDecal.hlsl"
		// With native render passes (the render graph default) URP gives the G-buffer decal pass the depth and rendering
		// layers as framebuffer inputs instead of binding _CameraDepthTexture. Same declarations as URP's DecalPass.template.
		#if defined(_RENDER_PASS_ENABLED)
			#define GBUFFER3 0
			#define GBUFFER4 1
			FRAMEBUFFER_INPUT_X_FLOAT(GBUFFER3);
			FRAMEBUFFER_INPUT_X_UINT(GBUFFER4);
		#elif defined(_DECAL_LAYERS)
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareRenderingLayerTexture.hlsl"
		#endif

		TEXTURE2D(_BaseMap);
		TEXTURE2D(_MaskMap);
		TEXTURE2D(_EmissionMap);
		TEXTURE2D(_SpecularMap);
		TEXTURE2D(_NormalMap);
		TEXTURE2D(_EmissionFlowMap);
		SAMPLER(sampler_BaseMap);

		CBUFFER_START(UnityPerMaterial)
			float4 _BaseMap_ST;
			half4 _BaseColor;
			half4 _EmissionColor;
			half4 _SpecularColor;
			half _Smoothness;
			half _NormalStrength;
			half _NormalBufferWrites;
			half _AngleLimit;
			half _SrcBlend;
			half _ClipThreshold;
			half _EmissionFlowTimeOffset;
		CBUFFER_END

		struct Attributes
		{
			float3 positionOS : POSITION;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct Varyings
		{
			float4 positionCS : SV_POSITION;
			UNITY_VERTEX_INPUT_INSTANCE_ID
			UNITY_VERTEX_OUTPUT_STEREO
		};

		struct DecalSurface
		{
			float3 positionWS;
			half3 albedo;
			half albedoWeight;
			half3 specular;
			half specularWeight;
			half smoothness;
			half3 normalWS;
			half normalWeight;
			half3 emission;
			half emissionFlow;
		};

		Varyings DecalVertex(Attributes input)
		{
			Varyings output;
			UNITY_SETUP_INSTANCE_ID(input);
			UNITY_TRANSFER_INSTANCE_ID(input, output);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
			output.positionCS = TransformObjectToHClip(input.positionOS);
			return output;
		}

		// Shared by both passes: finds the surface under this pixel, rejects it if it is outside the decal box, on another
		// rendering layer or facing away, and returns what the decal wants to write there.
		DecalSurface ProjectDecal(float2 positionCS)
		{
			#if defined(_RENDER_PASS_ENABLED)
				float deviceDepth = LOAD_FRAMEBUFFER_X_INPUT(GBUFFER3, positionCS).x;
				uint surfaceLayers = LOAD_FRAMEBUFFER_X_INPUT(GBUFFER4, positionCS).r;
			#else
				float deviceDepth = LoadSceneDepth(positionCS);
				uint surfaceLayers = 0;
				#if defined(_DECAL_LAYERS)
					surfaceLayers = LoadSceneRenderingLayer(positionCS);
				#endif
			#endif
			#if defined(_DECAL_LAYERS)
				clip((surfaceLayers & asuint(UNITY_ACCESS_INSTANCED_PROP(Decal, _DecalLayerMaskFromDecal))) - 0.1);
			#endif
			#if !UNITY_REVERSED_Z
				deviceDepth = lerp(UNITY_NEAR_CLIP_VALUE, 1, deviceDepth);
			#endif
			// UNITY_MATRIX_M is the projector's decal-to-world matrix, so object space is the decal box (a unit cube).
			float3 positionWS = ComputeWorldSpacePosition(positionCS * _ScreenSize.zw, deviceDepth, UNITY_MATRIX_I_VP);
			float3 positionDS = TransformWorldToObject(positionWS);
			clip(0.5 - Max3(abs(positionDS.x), abs(positionDS.y), abs(positionDS.z)));

			// The G-buffer normal cannot be read while this pass writes it, so the surface normal is rebuilt from depth,
			// like URP's own decals. The render target may be flipped, so the result is turned to face the camera.
			#if defined(_RENDER_PASS_ENABLED)
				half3 surfaceNormalWS = ReconstructNormalDerivative(positionCS, deviceDepth);
			#elif defined(_DECAL_NORMAL_BLEND_HIGH)
				half3 surfaceNormalWS = ReconstructNormalTap9(positionCS);
			#elif defined(_DECAL_NORMAL_BLEND_MEDIUM)
				half3 surfaceNormalWS = ReconstructNormalTap5(positionCS);
			#else
				half3 surfaceNormalWS = ReconstructNormalDerivative(positionCS);
			#endif
			surfaceNormalWS *= FastSign(dot(surfaceNormalWS, GetWorldSpaceViewDir(positionWS)));
			half3 decalRightWS = normalize(UNITY_MATRIX_M._m00_m10_m20);
			half3 decalUpWS = normalize(UNITY_MATRIX_M._m01_m11_m21);
			clip(dot(surfaceNormalWS, decalUpWS) - _AngleLimit);

			// The decal projects along its local Y axis, so the texture lies on the box's X/Z plane.
			float2 uv = (positionDS.xz + 0.5) * _BaseMap_ST.xy + _BaseMap_ST.zw;
			half mask = SAMPLE_TEXTURE2D(_MaskMap, sampler_BaseMap, uv).r;
			// Clip reveal: as the threshold rises, the mask shows from its brightest texels down. The curve (x2.5, -0.4)
			// comes from Decalicious' clip shader and gives the revealed area a soft edge that is at most 60% opaque.
			#if defined(_MASK_CLIP)
				half coverage = max(saturate((mask + _ClipThreshold - 1.0) * 2.5) - 0.4, 0.0);
			#else
				half coverage = mask;
			#endif
			// URP packs the projector's fade factor (including its draw distance fade) into the last column of _NormalToWorld.
			coverage *= UNITY_ACCESS_INSTANCED_PROP(Decal, _NormalToWorld)[0][3];
			clip(coverage - 0.0005);

			half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv) * _BaseColor;
			half4 emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_BaseMap, uv) * _EmissionColor;
			half4 specularMap = SAMPLE_TEXTURE2D(_SpecularMap, sampler_BaseMap, uv);

			DecalSurface surface;
			surface.positionWS = positionWS;
			surface.albedo = albedo.rgb;
			surface.albedoWeight = albedo.a * coverage;
			surface.specular = specularMap.rgb * _SpecularColor.rgb;
			surface.specularWeight = specularMap.a * _SpecularColor.a * coverage;
			surface.smoothness = specularMap.r * _Smoothness;
			surface.emission = emission.rgb * emission.a * coverage;
			#if defined(_EMISSION_FLOW)
				// A sawtooth that runs along the flow gradient: 0.3 cycles per second, as in Decalicious' Emission Flow shader.
				half flow = SAMPLE_TEXTURE2D(_EmissionFlowMap, sampler_BaseMap, uv).r;
				surface.emissionFlow = frac(flow - 6.0 * (_Time.x + _EmissionFlowTimeOffset));
			#else
				surface.emissionFlow = 1.0;
			#endif
			surface.normalWS = surfaceNormalWS;
			surface.normalWeight = 0.0;

			// The normal map is laid onto the surface under the decal: its tangent follows the decal's X axis projected onto
			// the surface, and its bitangent is cross(surface normal, tangent), matching how Decalicious oriented it.
			UNITY_BRANCH
			if (_NormalBufferWrites > 1.0)
			{
				half3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_BaseMap, uv), _NormalStrength);
				half3 tangentWS = normalize(decalRightWS - surfaceNormalWS * dot(decalRightWS, surfaceNormalWS));
				half3 bitangentWS = cross(surfaceNormalWS, tangentWS);
				surface.normalWS = normalize(normalTS.x * tangentWS + normalTS.y * bitangentWS + normalTS.z * surfaceNormalWS);
				surface.normalWeight = coverage;
			}
			return surface;
		}
		ENDHLSL

		Pass
		{
			Name "DecalGBufferProjector"
			Tags { "LightMode" = "DecalGBufferProjector" }

			// The box is drawn with ZTest Greater so only pixels where scene geometry lies inside it are shaded. Cull Off
			// keeps mirrored (negative scale) decals working; camera-facing faces never pass the depth test anyway.
			Cull Off
			ZTest Greater
			ZWrite Off
			// Targets: 0 albedo, 1 specular (alpha is occlusion, left alone), 2 normal + smoothness, 3 lighting (GI and emission).
			// Albedo and lighting use the material's blend. Specular and normal lerp by their own alpha. The alpha op Min only
			// matters for target 2 when it writes smoothness only: it lets a decal make a surface rougher but never shinier.
			BlendOp Add, Min
			Blend 0 [_SrcBlend] [_DstBlend]
			Blend 1 SrcAlpha OneMinusSrcAlpha
			Blend 2 SrcAlpha OneMinusSrcAlpha
			Blend 3 [_SrcBlend] [_DstBlend]
			ColorMask RGB 0
			ColorMask RGB 1
			ColorMask [_NormalBufferWrites] 2
			ColorMask RGB 3

			HLSLPROGRAM
			#pragma target 4.5
			#pragma exclude_renderers gles3 glcore
			#pragma vertex DecalVertex
			#pragma fragment GBufferFragment
			#pragma multi_compile_instancing
			#pragma editor_sync_compilation
			#pragma multi_compile_fragment _DECAL_NORMAL_BLEND_LOW _DECAL_NORMAL_BLEND_MEDIUM _DECAL_NORMAL_BLEND_HIGH
			#pragma multi_compile_fragment _ _DECAL_LAYERS
			#pragma multi_compile_fragment _ _RENDER_PASS_ENABLED
			#pragma shader_feature_local_fragment _MASK_CLIP
			#pragma shader_feature_local_fragment _EMISSION_FLOW

			void GBufferFragment(Varyings input,
				out half4 albedoBuffer : SV_Target0,
				out half4 specularBuffer : SV_Target1,
				out half4 normalBuffer : SV_Target2,
				out half4 lightingBuffer : SV_Target3)
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
				DecalSurface surface = ProjectDecal(input.positionCS.xy);

				// The receiving object already wrote its own ambient light into the lighting buffer. Blending the decal's
				// ambient term with the same weight as its albedo keeps the ambient light matching the new albedo.
				half3 premultipliedAlbedo = surface.albedo * surface.albedoWeight;
				albedoBuffer = half4(premultipliedAlbedo, surface.albedoWeight);
				specularBuffer = half4(surface.specular, surface.specularWeight);
				// Smoothness only: 1 keeps the surface value (Min blend); the decal's smoothness pulls it down by coverage.
				normalBuffer = _NormalBufferWrites > 1.0
					? half4(surface.normalWS, surface.normalWeight)
					: half4(0.0, 0.0, 0.0, lerp(1.0, surface.smoothness, surface.specularWeight));
				lightingBuffer = half4(SampleSH(surface.normalWS) * premultipliedAlbedo + surface.emission, surface.albedoWeight) * surface.emissionFlow;
			}
			ENDHLSL
		}

		Pass
		{
			Name "DecalScreenSpaceProjector"
			Tags { "LightMode" = "DecalScreenSpaceProjector" }

			Cull Off
			ZTest Greater
			ZWrite Off
			Blend [_SrcBlend] [_DstBlend]

			HLSLPROGRAM
			#pragma target 3.0
			#pragma vertex DecalVertex
			#pragma fragment ScreenSpaceFragment
			#pragma multi_compile_instancing
			#pragma editor_sync_compilation
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			#pragma multi_compile _ _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _CLUSTER_LIGHT_LOOP
			#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
			#pragma multi_compile_fragment _DECAL_NORMAL_BLEND_LOW _DECAL_NORMAL_BLEND_MEDIUM _DECAL_NORMAL_BLEND_HIGH
			#pragma multi_compile_fragment _ _DECAL_LAYERS
			#pragma multi_compile_fog
			#pragma shader_feature_local_fragment _MASK_CLIP
			#pragma shader_feature_local_fragment _EMISSION_FLOW
			#define _SPECULAR_SETUP

			half4 ScreenSpaceFragment(Varyings input) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
				float2 positionCS = input.positionCS.xy;
				TransformScreenUV(positionCS, _ScreenSize.y);
				DecalSurface surface = ProjectDecal(positionCS);

				// Forward fallback: the decal replaces the lit surface colour under it. Multiply decals (DstColor) only tint,
				// so they output their unlit albedo.
				UNITY_BRANCH
				if (_SrcBlend == 2.0)
					return half4(surface.albedo * surface.albedoWeight, surface.albedoWeight);

				InputData inputData = (InputData)0;
				inputData.positionWS = surface.positionWS;
				inputData.positionCS = input.positionCS;
				inputData.normalWS = surface.normalWS;
				inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(surface.positionWS);
				inputData.shadowCoord = TransformWorldToShadowCoord(surface.positionWS);
				inputData.fogCoord = ComputeFogFactor(TransformWorldToHClip(surface.positionWS).z);
				inputData.bakedGI = SampleSH(surface.normalWS);
				inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
				inputData.shadowMask = half4(1.0, 1.0, 1.0, 1.0);

				SurfaceData surfaceData = (SurfaceData)0;
				surfaceData.albedo = surface.albedo;
				surfaceData.specular = surface.specular;
				surfaceData.smoothness = surface.smoothness;
				surfaceData.occlusion = 1.0;
				surfaceData.alpha = 1.0;

				half3 color = UniversalFragmentPBR(inputData, surfaceData).rgb * surface.albedoWeight + surface.emission * surface.emissionFlow;
				return half4(MixFog(color, inputData.fogCoord), surface.albedoWeight);
			}
			ENDHLSL
		}
	}
}
