// The atmosphere shell around a planet (Athnar, Bethyr and Teiora atmosphere materials): a sphere a
// little larger than the planet, drawn from the inside (Cull Front) and alpha-blended over it.
//  * Its opacity is a Fresnel rim: thin where the shell faces the camera, thick towards the limb,
//    broken up by a slowly scrolling noise texture.
//  * Its colour runs from _color1 to _color2 towards the limb and is strongest where the shell faces
//    away from the light, so the atmosphere glows around the lit edge of the planet.
// The light's colour and falloff are ignored, as in the original: every light that reaches the shell
// only decides where it glows.
//
// Rebuilt for URP from the Shader Forge shader of the same name. The property names are the original
// ones, because the materials store their values under them. Two things are left out on purpose:
//  * _TimeEditor, the Shader Forge editor's time offset. It isn't a property and was 0 in the game.
//  * The shadow caster that the original borrowed from Fallback "Diffuse". Only SunLight lights the
//    Planets layer, and it casts no shadows (ShipSunLight excludes the layer); an opaque shadow of the
//    shell would also darken the planet inside it.
Shader "ZeroGravity/Planet/AtmosphereShader"
{
    Properties
    {
        _fresnel ("Rim Fresnel Exponent", Float) = 1
        _FresnelPower ("Rim Opacity Exponent", Float) = 1
        _color1 ("Inner Colour", Color) = (1, 1, 1, 1)
        _color2 ("Outer Colour", Color) = (1, 1, 1, 1)
        _AtmosphereNoise ("Noise", 2D) = "white" {}
        _ColorPower ("Colour Gradient Exponent", Float) = 1
        _ColorSlide ("Colour Gradient Scale", Range(0, 1)) = 0.5
        _AtmosphereLightPower ("Light Falloff Exponent", Float) = 1
        _Opacity ("Opacity", Float) = 1
        [HideInInspector] _Cutoff ("Alpha cutoff", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent+1"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }
        LOD 100

        Pass
        {
            Name "Atmosphere"
            // The original had no DEFERRED pass, so it stays forward-only: drawn by both the Forward and
            // the Deferred renderers, whatever queue a material puts it in. The original's FORWARDADD
            // pass (one additive draw per extra light) is the additional-light loop below.
            Tags { "LightMode" = "UniversalForwardOnly" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Front

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex AtmosphereVertex
            #pragma fragment AtmosphereFragment

            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile _ _LIGHT_LAYERS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_AtmosphereNoise);
            SAMPLER(sampler_AtmosphereNoise);

            CBUFFER_START(UnityPerMaterial)
                float4 _AtmosphereNoise_ST;
                float4 _color1;
                float4 _color2;
                float _fresnel;
                float _FresnelPower;
                float _ColorPower;
                float _ColorSlide;
                float _AtmosphereLightPower;
                float _Opacity;
                float _Cutoff;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 inwardNormalWS : TEXCOORD2;
            };

            Varyings AtmosphereVertex(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.uv = input.uv;
                // The shell is seen from the inside, so its normals are flipped to face the camera.
                output.inwardNormalWS = TransformObjectToWorldNormal(-input.normalOS);
                return output;
            }

            // How much one light makes the shell glow: most where the shell faces away from it.
            float3 AtmosphereGlow(float3 colour, float3 normalWS, float3 lightDirectionWS)
            {
                float facingLight = saturate(dot(normalWS, lightDirectionWS));
                float glow = pow(1.0 - facingLight, _AtmosphereLightPower);
                return clamp(colour * glow, 0.0, 5.0);
            }

            float4 AtmosphereFragment(Varyings input) : SV_Target
            {
                float3 normalWS = normalize(input.inwardNormalWS);
                float3 viewDirectionWS = normalize(GetCameraPositionWS() - input.positionWS);
                float rim = 1.0 - max(dot(normalWS, viewDirectionWS), 0.0);

                float3 colour = _color1.rgb + (_color2.rgb - _color1.rgb) * pow(rim, _ColorPower) / _ColorSlide;

                // Glow from the main light, then from every additional light. The original added the
                // additional lights in an additive pass, scaled by this alpha; adding them here, before
                // the SrcAlpha blend, gives the same result.
                float3 glow = AtmosphereGlow(colour, normalWS, normalize(_MainLightPosition.xyz));

            #if defined(_ADDITIONAL_LIGHTS)
                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                uint meshRenderingLayers = GetMeshRenderingLayer();

                #if USE_CLUSTER_LIGHT_LOOP
                [loop] for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
                {
                    CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK
                    Light light = GetAdditionalLight(lightIndex, input.positionWS);
                #ifdef _LIGHT_LAYERS
                    if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
                #endif
                        glow += AtmosphereGlow(colour, normalWS, light.direction);
                }
                #endif

                uint pixelLightCount = GetAdditionalLightsCount();
                LIGHT_LOOP_BEGIN(pixelLightCount)
                    Light light = GetAdditionalLight(lightIndex, input.positionWS);
                #ifdef _LIGHT_LAYERS
                    if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
                #endif
                        glow += AtmosphereGlow(colour, normalWS, light.direction);
                LIGHT_LOOP_END
            #endif

                float rimOpacity = pow(1.0 - min(pow(rim, _fresnel), 1.0), _FresnelPower);
                float2 scrolledUV = input.uv + _Time.y * 0.005;
                float2 noiseUV = TRANSFORM_TEX(scrolledUV, _AtmosphereNoise);
                float2 noise = SAMPLE_TEXTURE2D(_AtmosphereNoise, sampler_AtmosphereNoise, noiseUV).rg * 0.3 + 0.7;
                float alpha = saturate(rimOpacity * noise.r * noise.g * _Opacity);

                return float4(glow, alpha);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
