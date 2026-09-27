// The sun and planets behind the world. SpaceBackground.cs renders SunCamera's stack (the sun and the
// planets) into _SpaceBackgroundTex before the world camera renders; the world camera draws this as its
// skybox, through a Skybox component whose material uses this shader.
//
// The skybox is drawn after the opaque objects and only where none of them are, like the original's
// world cameras, which cleared only depth and drew over the sun and planets cameras.
Shader "OpenHellion/SpaceBackground"
{
    Properties
    {
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Background"
            "RenderType" = "Background"
            "PreviewType" = "Skybox"
        }

        Pass
        {
            Name "SpaceBackground"

            Cull Off
            ZWrite Off

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex SpaceBackgroundVertex
            #pragma fragment SpaceBackgroundFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_SpaceBackgroundTex);

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings SpaceBackgroundVertex(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            float4 SpaceBackgroundFragment(Varyings input) : SV_Target
            {
                // The same screen UV URP uses to sample its own camera textures, so the flip for render
                // textures is handled the same way.
                float2 uv = GetNormalizedScreenSpaceUV(input.positionCS);
                return float4(SAMPLE_TEXTURE2D_LOD(_SpaceBackgroundTex, sampler_LinearClamp, uv, 0).rgb, 1.0);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
