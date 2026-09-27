// Icons of celestial bodies, stations, ships and markers on the navigation map. A camera-facing
// billboard, alpha-blended and unlit. When the map highlights the object (_highlight) its brightness
// pulses over time, and when it's clicked (_click) it's tinted with _clickedColor.
//
// _highlightScaleMultiplier, _clickedScaleMultiplier and _glowMultiplier are unused, as in the
// original.
//
// Rebuilt for URP from the original's single unlit pass. Forward-only, as in the original.
Shader "ZeroGravity/Effects/NavMapObject"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Brightness ("Brightness", Float) = 1
        _Tint ("Tint", Color) = (1, 1, 1, 1)
        _Color ("Color", Color) = (1, 1, 1, 1)
        _timeMultiplier ("Time Multiplier", Float) = 1
        _depthPower ("Power", Float) = 1
        _highlightScaleMultiplier ("Highlight Scale Multiplier", Float) = 1
        _clickedScaleMultiplier ("Clicked Scale Multiplier", Float) = 1
        _glowMultiplier ("Glow Multiplier", Float) = 1
        _highlight ("Highlight", Range(0, 1)) = 0
        _click ("Click", Range(0, 1)) = 0
        _clickedColor ("Clicked Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Overlay"
            "IgnoreProjector" = "True"
            // The billboard is built from the object's own transform.
            "DisableBatching" = "True"
        }

        Pass
        {
            Name "Unlit"
            Tags { "LightMode" = "UniversalForwardOnly" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex NavMapObjectVertex
            #pragma fragment NavMapObjectFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Tint;
                float4 _Color;
                float4 _clickedColor;
                float _Brightness;
                float _timeMultiplier;
                float _depthPower;
                float _highlightScaleMultiplier;
                float _clickedScaleMultiplier;
                float _glowMultiplier;
                float _highlight;
                float _click;
            CBUFFER_END

            TEXTURE2D(_MainTex);    SAMPLER(sampler_MainTex);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings NavMapObjectVertex(Attributes input)
            {
                Varyings output;

                // Place the mesh's XY plane facing the camera at the object's position, scaled by the
                // object's Y scale on both axes, as the original does. The offset is subtracted, which
                // mirrors the quad; the fragment stage flips the UVs back.
                float4 centerVS = mul(UNITY_MATRIX_V, UNITY_MATRIX_M._m03_m13_m23_m33);
                float scale = length(UNITY_MATRIX_M._m01_m11_m21_m31);
                float4 positionVS = centerVS - float4(input.positionOS.xy * scale, 0.0, 0.0);
                output.positionCS = mul(UNITY_MATRIX_P, positionVS);

                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            float4 NavMapObjectFragment(Varyings input) : SV_Target
            {
                // Brightness pulse while highlighted: 1 when _highlight is 0, up to 2 when it is 1.
                float wave = (sin(_timeMultiplier * _Time.y) + 1.0) * 0.5;
                float pulse = pow(wave, _depthPower) * _highlight + 1.0;

                float4 texel = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, 1.0 - input.uv);
                float3 color = texel.rgb * _Brightness * _Tint.rgb * _Color.rgb;
                color = pulse * color;
                color *= lerp(1.0, _clickedColor.rgb, _click);

                return float4(color, texel.a * _Tint.a);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
