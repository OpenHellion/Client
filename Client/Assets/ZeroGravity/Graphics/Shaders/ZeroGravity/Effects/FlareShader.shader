// Additive lens flare for the sun and for thruster and headlight flares. The mesh is drawn as a
// camera-facing billboard around the object's origin, sized by the object's X and Y scale. Two
// textures, each spinning at its own rate, are added together, tinted and faded by how much of the
// flare's centre the scene hides.
//
// The occlusion is found per vertex: _Sample points from the flare's centre a little towards the
// vertex (_CenterSize is how far, as a fraction of the way) are tested against the camera depth
// texture. Each point that is behind the scene or off screen dims the vertex by 1 / _Sample.
// _GlobalIntensity is set by scripts (SunFlareEffect, EngineThrusters).
//
// Rebuilt for URP from the original's single unlit pass. It is forward-only and drawn in the
// Overlay queue, over everything, with ZTest Always. It needs the camera depth texture
// (the pipeline asset's Depth Texture setting, or the camera's override).
Shader "ZeroGravity/Effects/FlareShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SecTex ("Sec Tex", 2D) = "black" {}
        _CenterSize ("CenterSize", Float) = 1
        _Sample ("Sample", Float) = 1
        _Brightness ("Brightness", Float) = 1
        _Tint ("Tint", Color) = (1, 1, 1, 1)
        _Rotate ("Rotate", Float) = 0
        _Rotate2 ("Rotate2", Float) = 0
        _GlobalIntensity ("GlobalIntensity", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Overlay"
            "IgnoreProjector" = "True"
            "DisableBatching" = "True"
        }

        Pass
        {
            Name "Flare"
            Tags { "LightMode" = "UniversalForwardOnly" }

            Blend One One
            ZTest Always
            ZWrite Off

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex FlareVertex
            #pragma fragment FlareFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_SecTex);
            SAMPLER(sampler_SecTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _SecTex_ST;
                float4 _Tint;
                float _Sample;
                float _CenterSize;
                float _Brightness;
                float _Rotate;
                float _Rotate2;
                float _GlobalIntensity;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 mainUV : TEXCOORD0;
                float2 secondUV : TEXCOORD1;
                float occlusion : TEXCOORD2;
            };

            // SampleSceneDepth for the vertex stage, which has no derivatives to pick a mip.
            float SampleSceneDepthLod(float2 uv)
            {
                uv = ClampAndScaleUVForBilinear(UnityStereoTransformScreenSpaceTex(uv), _CameraDepthTexture_TexelSize.xy);
                return SAMPLE_TEXTURE2D_X_LOD(_CameraDepthTexture, sampler_PointClamp, uv, 0).r;
            }

            // Fraction of the samples between the flare's centre and this vertex that the scene hides.
            float GetOcclusion(float4 positionCS, float4 centerCS)
            {
                float4 toCenterCS = centerCS - positionCS;
                float occlusion = 0.0;

                [loop]
                for (float i = 0.0; i < _Sample; i += 1.0)
                {
                    float t = 1.0 - (i / _Sample) * _CenterSize;
                    float4 samplePositionCS = positionCS + t * toCenterCS;

                    // ComputeScreenPos: 0..w across the screen.
                    float2 screenPos = float2(samplePositionCS.x, samplePositionCS.y * _ProjectionParams.x) * 0.5
                        + samplePositionCS.w * 0.5;
                    float2 screenUV = screenPos / samplePositionCS.w;

                    float sampleDepth01 = samplePositionCS.w * _ProjectionParams.w;
                    float sceneDepth01 = Linear01Depth(SampleSceneDepthLod(screenUV), _ZBufferParams);

                    bool hidden = sceneDepth01 < sampleDepth01
                        || screenPos.x < 0.0 || screenPos.y < 0.0
                        || screenUV.x > 1.0 || screenUV.y > 1.0;
                    if (hidden)
                    {
                        occlusion += 1.0 / _Sample;
                    }
                }

                return occlusion;
            }

            // Turns the UV about the texture's centre by angle radians.
            float2 RotateUV(float2 uv, float angle)
            {
                float s, c;
                sincos(angle, s, c);
                float2 p = uv - 0.5;
                return float2(c * p.x - s * p.y, s * p.x + c * p.y) + 0.5;
            }

            Varyings FlareVertex(Attributes input)
            {
                Varyings output;

                // Billboard: the vertex is offset from the object's origin in view space, so the
                // flare always faces the camera. The offset is subtracted, which turns the mesh half
                // a turn compared to its object space.
                float4 originWS = UNITY_MATRIX_M._m03_m13_m23_m33;
                float2 objectScale = float2(length(UNITY_MATRIX_M._m00_m10_m20_m30), length(UNITY_MATRIX_M._m01_m11_m21_m31));
                float4 positionVS = mul(UNITY_MATRIX_V, originWS) - float4(objectScale * input.positionOS.xy, 0.0, 0.0);
                output.positionCS = mul(UNITY_MATRIX_P, positionVS);

                float4 centerCS = mul(UNITY_MATRIX_VP, originWS);
                output.occlusion = GetOcclusion(output.positionCS, centerCS);

                output.mainUV = TRANSFORM_TEX(RotateUV(input.uv, _Rotate * _Time.y), _MainTex);
                output.secondUV = TRANSFORM_TEX(RotateUV(input.uv, _Rotate2 * _Time.y), _SecTex);
                return output;
            }

            float4 FlareFragment(Varyings input) : SV_Target
            {
                float3 mainColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.mainUV).rgb;
                float3 secondColor = SAMPLE_TEXTURE2D(_SecTex, sampler_SecTex, input.secondUV).rgb;

                float visibility = 1.0 - input.occlusion;
                float3 color = mainColor * visibility + secondColor * visibility;
                color = color * _Brightness * _Tint.rgb;
                return float4(color, 1.0) * max(_GlobalIntensity, 0.0);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
