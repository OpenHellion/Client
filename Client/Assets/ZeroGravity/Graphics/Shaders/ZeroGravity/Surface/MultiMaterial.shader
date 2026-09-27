// Multi-material surface for station and ship modules. One colour map (_DifuseMap) is shared by
// six materials: _IdMask.r picks the material per texel, and each material colourises the colour
// map with its own colours and adds its own detail normal, roughness and metallic. On top of that:
//  * wear (_Masks.b, _WearAmount) turns a material toward material 6, the bare metal underneath,
//    as far as its _MatNUseWear allows,
//  * soft dirt (_IdMask.a, _SoftDrtAmount) tints each material and darkens ambient light,
//  * rust (_IdMask.g), dirt (_Masks.g) and frost (_IdMask.b) are layered over everything,
//  * _NormalEmision holds the base normal (RGB) and an emission mask (A),
//  * _Glow pulses a colour over the whole surface.
// Texels with _DifuseMap alpha below 0.5 are cut out. LightEmission sets _EmColor at runtime.
//
// The detail maps (_Mat1Normal to _Mat6Normal, _RustNormal, _DirtNormal, _FrostNormal) hold a normal
// in RG, a colour-variation mask in B and roughness in A.
//
// Rebuilt for URP from the Shader Forge shader of the same name.
Shader "ZeroGravity/Surface/MultiMaterial"
{
    Properties
    {
        _IdMask ("IdMask", 2D) = "black" {}
        _Masks ("Masks", 2D) = "white" {}
        _DifuseMap ("DifuseMap", 2D) = "white" {}
        _NormalEmision ("Normal+Emision", 2D) = "white" {}
        _WearAmount ("WearAmount", Range(0, 1)) = 0
        _SoftDrtAmount ("SoftDrtAmount", Range(0, 1)) = 0
        _Mat1Normal ("Mat1Normal", 2D) = "gray" {}
        _Mat1Color1 ("Mat1Color1", Color) = (0, 0, 0, 1)
        _Mat1Color2 ("Mat1Color2", Color) = (0, 0, 0, 1)
        _Mat1BaseMetalic ("Mat1BaseMetalic", Range(0, 1)) = 0.1038745
        _Mat1DmgTint ("Mat1DmgTint", Color) = (1, 1, 1, 1)
        _Mat1DmgRoughnes ("Mat1DmgRoughnes", Range(0, 1)) = 0
        _Mat1DmgMetalic ("Mat1DmgMetalic", Range(0, 1)) = 0
        _Mat1DirtTint ("Mat1DirtTint", Color) = (1, 1, 1, 1)
        _Mat1DirtRoughness ("Mat1DirtRoughness", Range(0, 1)) = 0
        _Mat2Normal ("Mat2Normal", 2D) = "gray" {}
        _Mat2Color1 ("Mat2Color1", Color) = (0, 0, 0, 1)
        _Mat2Color2 ("Mat2Color2", Color) = (0, 0, 0, 1)
        _Mat2BaseMetalic ("Mat2BaseMetalic", Range(0, 1)) = 0
        _Mat2DmgTint ("Mat2DmgTint", Color) = (1, 1, 1, 1)
        _Mat2DmgRoughness ("Mat2DmgRoughness", Range(0, 1)) = 0
        _Mat2DmgMetalic ("Mat2DmgMetalic", Range(0, 1)) = 0
        _Mat2DirtTint ("Mat2DirtTint", Color) = (1, 1, 1, 1)
        _Mat2DirtRoughness ("Mat2DirtRoughness", Range(0, 1)) = 0
        _Mat3Normal ("Mat3Normal", 2D) = "gray" {}
        _Mat3Color1 ("Mat3Color1", Color) = (0, 0, 0, 1)
        _Mat3Color2 ("Mat3Color2", Color) = (0, 0, 0, 1)
        _Mat3BaseMetalic ("Mat3BaseMetalic", Range(0, 1)) = 0
        _Mat3DmgTint ("Mat3DmgTint", Color) = (1, 1, 1, 1)
        _Mat3DmgRoughness ("Mat3DmgRoughness", Range(0, 1)) = 0
        _Mat3DmgMetalic ("Mat3DmgMetalic", Range(0, 1)) = 0
        _Mat3DirtTint ("Mat3DirtTint", Color) = (1, 1, 1, 1)
        _Mat3DirtRoughness ("Mat3DirtRoughness", Range(0, 1)) = 0
        _Mat4Normal ("Mat4Normal", 2D) = "gray" {}
        _Mat4Color1 ("Mat4Color1", Color) = (0, 0, 0, 1)
        _Mat4Color2 ("Mat4Color2", Color) = (0, 0, 0, 1)
        _Mat4BaseMetalic ("Mat4BaseMetalic", Range(0, 1)) = 0
        _Mat4DmgTint ("Mat4DmgTint", Color) = (1, 1, 1, 1)
        _Mat4DirtRouthness ("Mat4DirtRouthness", Range(0, 1)) = 0
        _Mat4DmgMetalic ("Mat4DmgMetalic", Range(0, 1)) = 0
        _Mat4DirtTint ("Mat4DirtTint", Color) = (1, 1, 1, 1)
        _Mat4DmgRoughness ("Mat4DmgRoughness", Range(0, 1)) = 0
        _Mat5Normal ("Mat5Normal", 2D) = "gray" {}
        _Mat5Color1 ("Mat5Color1", Color) = (0, 0, 0, 1)
        _Mat5Color2 ("Mat5Color2", Color) = (0, 0, 0, 1)
        _Mat5BaseMetalic ("Mat5BaseMetalic", Range(0, 1)) = 0
        _Mat5DmgTint ("Mat5DmgTint", Color) = (1, 1, 1, 1)
        _Mat5DmgRoughness ("Mat5DmgRoughness", Range(0, 1)) = 0
        _Mat5DmgMetalic ("Mat5DmgMetalic", Range(0, 1)) = 0
        _Mat5DirtTint ("Mat5DirtTint", Color) = (1, 1, 1, 1)
        _Mat5DirtRoughness ("Mat5DirtRoughness", Range(0, 1)) = 0
        _Mat6Normal ("Mat6Normal", 2D) = "white" {}
        _Mat6Color1 ("Mat6Color1", Color) = (0, 0, 0, 1)
        _Mat6Color2 ("Mat6Color2", Color) = (0, 0, 0, 1)
        _Mat6BaseMetalic ("Mat6BaseMetalic", Range(0, 1)) = 0
        _Mat6DmgTint ("Mat6DmgTint", Color) = (1, 1, 1, 1)
        _Mat6DmgRoughness ("Mat6DmgRoughness", Range(0, 1)) = 0
        _Mat6DmgMetalic ("Mat6DmgMetalic", Range(0, 1)) = 0
        _Mat6DirtColor ("Mat6DirtColor", Color) = (1, 1, 1, 1)
        _Mat6DirtRoughness ("Mat6DirtRoughness", Range(0, 1)) = 0
        _RustNormal ("RustNormal", 2D) = "gray" {}
        _RustAmount ("RustAmount", Range(0, 1)) = 0
        _RustColor1 ("RustColor1", Color) = (0, 0, 0, 1)
        _RustColor2 ("RustColor2", Color) = (0, 0, 0, 1)
        _RustMetaic ("RustMetaic", Range(0, 1)) = 0
        _DirtNormal ("DirtNormal", 2D) = "gray" {}
        _DirtAmount ("DirtAmount", Range(0, 1)) = 0
        _DirtColor1 ("DirtColor1", Color) = (0, 0, 0, 1)
        _DirtColor2 ("DirtColor2", Color) = (0, 0, 0, 1)
        _DirtMetalic ("DirtMetalic", Range(0, 1)) = 0
        _FrostNormal ("FrostNormal", 2D) = "gray" {}
        _FrostAmount ("FrostAmount", Range(0, 1)) = 0
        _FrostColor1 ("FrostColor1", Color) = (1, 1, 1, 1)
        _FrostColor2 ("FrostColor2", Color) = (0.6, 0.9, 0.9, 1)
        _FrostMetalic ("FrostMetalic", Range(0, 1)) = 0
        [MaterialToggle] _IdDebug ("IdDebug", Float) = 0.6
        _EmissionAmount ("EmissionAmount", Float) = 0
        _EmColor ("Emission Color", Color) = (1, 1, 1, 1)
        [MaterialToggle] _Mat1UseRust ("Mat1UseRust", Float) = 0
        [MaterialToggle] _Mat2UseRust ("Mat2UseRust", Float) = 0
        [MaterialToggle] _Mat3UseRust ("Mat3UseRust", Float) = 0
        [MaterialToggle] _Mat4UseRust ("Mat4UseRust", Float) = 0
        [MaterialToggle] _Mat5UseRust ("Mat5UseRust", Float) = 0
        [MaterialToggle] _Mat6UseRust ("Mat6UseRust", Float) = 0
        _Mat1ColorVariationAdj ("Mat1ColorVariationAdj", Range(-0.999, 0.999)) = 0
        _Mat2ColorVariationAdj ("Mat2ColorVariationAdj", Range(-0.999, 0.999)) = 0
        _Mat3ColorVariationAdj ("Mat3ColorVariationAdj", Range(-0.999, 0.999)) = 0
        _Mat4ColorVariationAdj ("Mat4ColorVariationAdj", Range(-0.999, 0.999)) = 0
        _Mat5ColorVariationAdj ("Mat5ColorVariationAdj", Range(-0.999, 0.999)) = 0
        _Mat6ColorVariationAdj ("Mat6ColorVariationAdj", Range(-0.999, 0.999)) = 0
        _RustColorVariationAdj ("RustColorVariationAdj", Range(-0.999, 0.999)) = 0
        _FrostColorVariationAdj ("FrostColorVariationAdj", Range(-0.999, 0.999)) = 0
        _DirtColorVariationAdj ("DirtColorVariationAdj", Range(-0.999, 0.999)) = 0
        _SpecualVariationAdj ("SpecualVariationAdj", Range(-2, 2)) = 0
        _Mat1WearNormalInt ("Mat1WearNormalInt", Range(0, 1)) = 0
        _Mat2WearNormalInt ("Mat2WearNormalInt", Range(0, 1)) = 0
        _Mat3WearNormalInt ("Mat3WearNormalInt", Range(0, 1)) = 0
        _Mat4WearNormalInt ("Mat4WearNormalInt", Range(0, 1)) = 0
        _Mat5WearNormalInt ("Mat5WearNormalInt", Range(0, 1)) = 0
        _Mat6WearNormalInt ("Mat6WearNormalInt", Range(0, 1)) = 0
        _Mat1UseWear ("Mat1UseWear", Float) = 0
        _Mat2UseWear ("Mat2UseWear", Float) = 0
        _Mat3UseWear ("Mat3UseWear", Float) = 0
        _Mat4UseWear ("Mat4UseWear", Float) = 0
        _Mat5UseWear ("Mat5UseWear", Float) = 0
        _Mat6UseWear ("Mat6UseWear", Float) = 0
        _Mat1Tile ("Mat1Tile", Float) = 1
        _Mat2Tile ("Mat2Tile", Float) = 1
        _Mat3Tile ("Mat3Tile", Float) = 1
        _Mat4Tile ("Mat4Tile", Float) = 1
        _Mat5Tile ("Mat5Tile", Float) = 1
        _Mat6Tile ("Mat6Tile", Float) = 1
        _Glow ("Glow", Range(0, 0.02)) = 0
        _GlowColor ("GlowColor", Color) = (1, 1, 1, 1)
        [MaterialToggle] _Mat1UseMatAlbedo ("Mat1UseMatAlbedo", Float) = 0
        [MaterialToggle] _Mat2UseMatAlbedo ("Mat2UseMatAlbedo", Float) = 0
        [MaterialToggle] _Mat3UseMatAlbedo ("Mat3UseMatAlbedo", Float) = 0
        [MaterialToggle] _Mat4UseMatAlbedo ("Mat4UseMatAlbedo", Float) = 0
        [MaterialToggle] _Mat5UseMatAlbedo ("Mat5UseMatAlbedo", Float) = 0
        [MaterialToggle] _Mat6UseMatAlbedo ("Mat6UseMatAlbedo", Float) = 0
        [HideInInspector] _Cutoff ("Alpha cutoff", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "TransparentCutout"
            "Queue" = "AlphaTest"
            "UniversalMaterialType" = "Lit"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
        #endif

        CBUFFER_START(UnityPerMaterial)
            float4 _IdMask_ST;
            float4 _Masks_ST;
            float4 _DifuseMap_ST;
            float4 _NormalEmision_ST;
            float4 _Mat1Normal_ST;
            float4 _Mat2Normal_ST;
            float4 _Mat3Normal_ST;
            float4 _Mat4Normal_ST;
            float4 _Mat5Normal_ST;
            float4 _Mat6Normal_ST;
            float4 _RustNormal_ST;
            float4 _DirtNormal_ST;
            float4 _FrostNormal_ST;
            float4 _Mat1Color1, _Mat1Color2, _Mat1DmgTint, _Mat1DirtTint;
            float4 _Mat2Color1, _Mat2Color2, _Mat2DmgTint, _Mat2DirtTint;
            float4 _Mat3Color1, _Mat3Color2, _Mat3DmgTint, _Mat3DirtTint;
            float4 _Mat4Color1, _Mat4Color2, _Mat4DmgTint, _Mat4DirtTint;
            float4 _Mat5Color1, _Mat5Color2, _Mat5DmgTint, _Mat5DirtTint;
            float4 _Mat6Color1, _Mat6Color2, _Mat6DmgTint, _Mat6DirtColor;
            float4 _RustColor1, _RustColor2;
            float4 _DirtColor1, _DirtColor2;
            float4 _FrostColor1, _FrostColor2;
            float4 _EmColor;
            float4 _GlowColor;
            float _Mat1BaseMetalic, _Mat1DmgRoughnes, _Mat1DmgMetalic, _Mat1DirtRoughness;
            float _Mat2BaseMetalic, _Mat2DmgRoughness, _Mat2DmgMetalic, _Mat2DirtRoughness;
            float _Mat3BaseMetalic, _Mat3DmgRoughness, _Mat3DmgMetalic, _Mat3DirtRoughness;
            float _Mat4BaseMetalic, _Mat4DmgRoughness, _Mat4DmgMetalic, _Mat4DirtRouthness;
            float _Mat5BaseMetalic, _Mat5DmgRoughness, _Mat5DmgMetalic, _Mat5DirtRoughness;
            float _Mat6BaseMetalic, _Mat6DmgRoughness, _Mat6DmgMetalic, _Mat6DirtRoughness;
            float _Mat1ColorVariationAdj, _Mat2ColorVariationAdj, _Mat3ColorVariationAdj;
            float _Mat4ColorVariationAdj, _Mat5ColorVariationAdj, _Mat6ColorVariationAdj;
            float _Mat1WearNormalInt, _Mat2WearNormalInt, _Mat3WearNormalInt;
            float _Mat4WearNormalInt, _Mat5WearNormalInt, _Mat6WearNormalInt;
            float _Mat1UseWear, _Mat2UseWear, _Mat3UseWear, _Mat4UseWear, _Mat5UseWear, _Mat6UseWear;
            float _Mat1UseRust, _Mat2UseRust, _Mat3UseRust, _Mat4UseRust, _Mat5UseRust, _Mat6UseRust;
            float _Mat1UseMatAlbedo, _Mat2UseMatAlbedo, _Mat3UseMatAlbedo;
            float _Mat4UseMatAlbedo, _Mat5UseMatAlbedo, _Mat6UseMatAlbedo;
            float _Mat1Tile, _Mat2Tile, _Mat3Tile, _Mat4Tile, _Mat5Tile, _Mat6Tile;
            float _WearAmount;
            float _SoftDrtAmount;
            float _RustAmount, _RustMetaic, _RustColorVariationAdj;
            float _DirtAmount, _DirtMetalic, _DirtColorVariationAdj;
            float _FrostAmount, _FrostMetalic, _FrostColorVariationAdj;
            float _SpecualVariationAdj;
            float _IdDebug;
            float _EmissionAmount;
            float _Glow;
        CBUFFER_END

        TEXTURE2D(_IdMask);             SAMPLER(sampler_IdMask);
        TEXTURE2D(_Masks);              SAMPLER(sampler_Masks);
        TEXTURE2D(_DifuseMap);          SAMPLER(sampler_DifuseMap);
        TEXTURE2D(_NormalEmision);      SAMPLER(sampler_NormalEmision);
        // D3D11 has 16 sampler slots and URP's lighting takes several, so the nine detail maps share
        // _Mat1Normal's sampler. The game's textures for this shader all import as bilinear and repeat.
        TEXTURE2D(_Mat1Normal);         SAMPLER(sampler_Mat1Normal);
        TEXTURE2D(_Mat2Normal);
        TEXTURE2D(_Mat3Normal);
        TEXTURE2D(_Mat4Normal);
        TEXTURE2D(_Mat5Normal);
        TEXTURE2D(_Mat6Normal);
        TEXTURE2D(_RustNormal);
        TEXTURE2D(_DirtNormal);
        TEXTURE2D(_FrostNormal);

        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float2 uv0 : TEXCOORD0;
            float2 uv1 : TEXCOORD1;     // Unity lightmap UVs, used only by the Meta pass
            float2 uv2 : TEXCOORD2;     // Unity dynamic lightmap UVs, used only by the Meta pass
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        // ---------------------------------------------------------------------
        // Surface
        // ---------------------------------------------------------------------

        // The original clips at a fixed 0.5. _Cutoff only serves the built-in fallback's shadows.
        #define ALPHA_CUTOFF 0.5

        // Flat colours for _IdDebug, one per material.
        static const float3 MaterialDebugColors[6] =
        {
            float3(1.0, 0.0, 0.0), float3(0.0, 1.0, 0.0), float3(0.0, 0.0, 1.0),
            float3(1.0, 0.8, 0.5), float3(0.5, 1.0, 1.0), float3(1.0, 0.3, 1.0)
        };

        struct MaterialSettings
        {
            float3 color1;
            float3 color2;
            float colorVariationAdjust;
            float useMaterialAlbedo;
            float baseMetallic;
            float3 damageTint;
            float damageRoughness;
            float damageMetallic;
            float3 dirtTint;
            float dirtRoughness;
            float useWear;
            float useRust;
        };

        MaterialSettings GetMaterialSettings(uint index)
        {
            MaterialSettings m;
            switch (index)
            {
                case 0:
                    m.color1 = _Mat1Color1.rgb;
                    m.color2 = _Mat1Color2.rgb;
                    m.colorVariationAdjust = _Mat1ColorVariationAdj;
                    m.useMaterialAlbedo = _Mat1UseMatAlbedo;
                    m.baseMetallic = _Mat1BaseMetalic;
                    m.damageTint = _Mat1DmgTint.rgb;
                    m.damageRoughness = _Mat1DmgRoughnes;
                    m.damageMetallic = _Mat1DmgMetalic;
                    m.dirtTint = _Mat1DirtTint.rgb;
                    m.dirtRoughness = _Mat1DirtRoughness;
                    m.useWear = _Mat1UseWear;
                    m.useRust = _Mat1UseRust;
                    break;
                case 1:
                    m.color1 = _Mat2Color1.rgb;
                    m.color2 = _Mat2Color2.rgb;
                    m.colorVariationAdjust = _Mat2ColorVariationAdj;
                    m.useMaterialAlbedo = _Mat2UseMatAlbedo;
                    m.baseMetallic = _Mat2BaseMetalic;
                    m.damageTint = _Mat2DmgTint.rgb;
                    m.damageRoughness = _Mat2DmgRoughness;
                    m.damageMetallic = _Mat2DmgMetalic;
                    m.dirtTint = _Mat2DirtTint.rgb;
                    m.dirtRoughness = _Mat2DirtRoughness;
                    m.useWear = _Mat2UseWear;
                    m.useRust = _Mat2UseRust;
                    break;
                case 2:
                    m.color1 = _Mat3Color1.rgb;
                    m.color2 = _Mat3Color2.rgb;
                    m.colorVariationAdjust = _Mat3ColorVariationAdj;
                    m.useMaterialAlbedo = _Mat3UseMatAlbedo;
                    m.baseMetallic = _Mat3BaseMetalic;
                    m.damageTint = _Mat3DmgTint.rgb;
                    m.damageRoughness = _Mat3DmgRoughness;
                    m.damageMetallic = _Mat3DmgMetalic;
                    m.dirtTint = _Mat3DirtTint.rgb;
                    m.dirtRoughness = _Mat3DirtRoughness;
                    m.useWear = _Mat3UseWear;
                    m.useRust = _Mat3UseRust;
                    break;
                case 3:
                    m.color1 = _Mat4Color1.rgb;
                    m.color2 = _Mat4Color2.rgb;
                    m.colorVariationAdjust = _Mat4ColorVariationAdj;
                    m.useMaterialAlbedo = _Mat4UseMatAlbedo;
                    m.baseMetallic = _Mat4BaseMetalic;
                    m.damageTint = _Mat4DmgTint.rgb;
                    m.damageRoughness = _Mat4DmgRoughness;
                    m.damageMetallic = _Mat4DmgMetalic;
                    m.dirtTint = _Mat4DirtTint.rgb;
                    m.dirtRoughness = _Mat4DirtRouthness;
                    m.useWear = _Mat4UseWear;
                    m.useRust = _Mat4UseRust;
                    break;
                case 4:
                    m.color1 = _Mat5Color1.rgb;
                    m.color2 = _Mat5Color2.rgb;
                    m.colorVariationAdjust = _Mat5ColorVariationAdj;
                    m.useMaterialAlbedo = _Mat5UseMatAlbedo;
                    m.baseMetallic = _Mat5BaseMetalic;
                    m.damageTint = _Mat5DmgTint.rgb;
                    m.damageRoughness = _Mat5DmgRoughness;
                    m.damageMetallic = _Mat5DmgMetalic;
                    m.dirtTint = _Mat5DirtTint.rgb;
                    m.dirtRoughness = _Mat5DirtRoughness;
                    m.useWear = _Mat5UseWear;
                    m.useRust = _Mat5UseRust;
                    break;
                default:
                    m.color1 = _Mat6Color1.rgb;
                    m.color2 = _Mat6Color2.rgb;
                    m.colorVariationAdjust = _Mat6ColorVariationAdj;
                    m.useMaterialAlbedo = _Mat6UseMatAlbedo;
                    m.baseMetallic = _Mat6BaseMetalic;
                    m.damageTint = _Mat6DmgTint.rgb;
                    m.damageRoughness = _Mat6DmgRoughness;
                    m.damageMetallic = _Mat6DmgMetalic;
                    m.dirtTint = _Mat6DirtColor.rgb;
                    m.dirtRoughness = _Mat6DirtRoughness;
                    m.useWear = _Mat6UseWear;
                    m.useRust = _Mat6UseRust;
                    break;
            }
            return m;
        }

        // _IdMask.r picks one of six materials in bands 0.2 wide, centred on 0, 0.2, 0.4, 0.6, 0.8 and 1.
        uint GetMaterialIndex(float id)
        {
            float4 above = step(float4(0.1, 0.3, 0.5, 0.7), id);
            return (uint)(above.x + above.y + above.z + above.w + step(0.9, id));
        }

        // Reveals an effect where its mask is brightest first: amount 0 shows none of it, 1 all of it.
        // max() avoids 0 / 0 at amount 0, which the original relied on D3D's saturate(NaN) = 0 for.
        float RevealMask(float mask, float amount)
        {
            return saturate((mask - (1.0 - amount)) / max(amount, 1e-5));
        }

        // Moves a 0-1 variation mask by adjust (-1 to 1) and stretches what is left back to 0-1.
        float AdjustVariation(float variation, float adjust)
        {
            float low = max(adjust, 0.0);
            float high = min(adjust + 1.0, 1.0);
            return (saturate(variation + adjust) - low) / (high - low);
        }

        // The core library's RgbToHsv with Shader Forge's epsilon (1e-10 instead of 1e-4), which
        // changes saturation and hue noticeably for dark colours.
        float3 RgbToHsvPrecise(float3 c)
        {
            const float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
            float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
            float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
            float d = q.x - min(q.w, q.y);
            const float e = 1.0e-10;
            return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
        }

        // Colours the colour map with a material colour: the map's luminance takes on the colour's hue
        // and saturation, and the darker the colour, the more of the untouched map shows through.
        // A black colour therefore leaves the map as it is.
        float3 ColorizeDiffuse(float3 diffuse, float3 color)
        {
            const float3 lumaWeights = float3(0.3, 0.59, 0.11);
            float3 hsv = RgbToHsvPrecise(color);
            float3 hueColor = saturate(abs(frac(hsv.x + float3(0.0, -1.0 / 3.0, 1.0 / 3.0)) * 6.0 - 3.0) - 1.0);
            float3 colorized = dot(diffuse, lumaWeights) * hueColor;
            colorized = lerp(colorized, dot(colorized, lumaWeights), 1.0 - hsv.y);
            return lerp(colorized, diffuse, 1.0 - hsv.z);
        }

        float3 Overlay(float3 base, float3 blend)
        {
            return saturate(base > 0.5 ? 1.0 - (1.0 - 2.0 * (base - 0.5)) * (1.0 - blend) : 2.0 * base * blend);
        }

        // The textures are imported as sRGB. The original undid that with pow(0.45) wherever it wanted
        // the stored values. abs() only silences FXC's warning: sampled values are never negative.
        float4 ToGamma(float4 value)
        {
            return pow(abs(value), 0.45);
        }

        float2 DecodeDetailNormal(float4 detail)
        {
            return ToGamma(detail).xy * 2.0 - 1.0;
        }

        // A detail normal for BlendNormalRNM. weight 0 makes it flat, then it moves toward wornNormal
        // by wearBlend and back toward flat by flatten. z stays 1: the normal is not rebuilt.
        float3 GetNormalLayer(float2 detailNormal, float weight, float3 wornNormal, float wearBlend, float flatten)
        {
            float3 layer = float3(detailNormal * weight, 1.0);
            layer = lerp(layer, wornNormal, wearBlend);
            return lerp(layer, float3(0.0, 0.0, 1.0), flatten);
        }

        // The pulsing _Glow colour. It is left out of the Meta pass, like in the original.
        half3 GetGlow()
        {
            return (sin(_Time.z * 3.0) * 0.5 + 0.5) * _Glow * _GlowColor.rgb;
        }

        // surfaceData.alpha is the colour map's alpha. Passes other than Meta clip it at ALPHA_CUTOFF.
        // The emission excludes GetGlow().
        void InitializeSurfaceData(float2 uv, out SurfaceData surfaceData)
        {
            float4 diffuse = SAMPLE_TEXTURE2D(_DifuseMap, sampler_DifuseMap, TRANSFORM_TEX(uv, _DifuseMap));
            float4 normalEmission = SAMPLE_TEXTURE2D(_NormalEmision, sampler_NormalEmision, TRANSFORM_TEX(uv, _NormalEmision));
            float4 idMask = ToGamma(SAMPLE_TEXTURE2D(_IdMask, sampler_IdMask, TRANSFORM_TEX(uv, _IdMask)));
            float4 masks = SAMPLE_TEXTURE2D(_Masks, sampler_Masks, TRANSFORM_TEX(uv, _Masks));
            float4 rustDetail = SAMPLE_TEXTURE2D(_RustNormal, sampler_Mat1Normal, TRANSFORM_TEX(uv, _RustNormal));
            float4 dirtDetail = SAMPLE_TEXTURE2D(_DirtNormal, sampler_Mat1Normal, TRANSFORM_TEX(uv, _DirtNormal));
            float4 frostDetail = SAMPLE_TEXTURE2D(_FrostNormal, sampler_Mat1Normal, TRANSFORM_TEX(uv, _FrostNormal));
            float4 materialDetail[6];
            materialDetail[0] = SAMPLE_TEXTURE2D(_Mat1Normal, sampler_Mat1Normal, TRANSFORM_TEX((uv * _Mat1Tile), _Mat1Normal));
            materialDetail[1] = SAMPLE_TEXTURE2D(_Mat2Normal, sampler_Mat1Normal, TRANSFORM_TEX((uv * _Mat2Tile), _Mat2Normal));
            materialDetail[2] = SAMPLE_TEXTURE2D(_Mat3Normal, sampler_Mat1Normal, TRANSFORM_TEX((uv * _Mat3Tile), _Mat3Normal));
            materialDetail[3] = SAMPLE_TEXTURE2D(_Mat4Normal, sampler_Mat1Normal, TRANSFORM_TEX((uv * _Mat4Tile), _Mat4Normal));
            materialDetail[4] = SAMPLE_TEXTURE2D(_Mat5Normal, sampler_Mat1Normal, TRANSFORM_TEX((uv * _Mat5Tile), _Mat5Normal));
            materialDetail[5] = SAMPLE_TEXTURE2D(_Mat6Normal, sampler_Mat1Normal, TRANSFORM_TEX((uv * _Mat6Tile), _Mat6Normal));

            uint materialIndex = GetMaterialIndex(idMask.r);
            MaterialSettings material = GetMaterialSettings(materialIndex);
            float4 detail = materialDetail[materialIndex];
            float4 wornDetail = materialDetail[5];

            float wear = RevealMask(ToGamma(masks).b, _WearAmount);
            float softDirt = RevealMask(idMask.a, _SoftDrtAmount);
            float rust = RevealMask(idMask.g, _RustAmount) * material.useRust;
            float dirt = RevealMask(ToGamma(masks).g, _DirtAmount) * saturate(2.0 * _DirtAmount);
            float frost = RevealMask(idMask.b, _FrostAmount);

            // Material colour. Worn areas turn toward material 6's colour, dirty areas toward the dirt tint.
            float3 wornColor = lerp(_Mat6Color1.rgb, _Mat6Color2.rgb, AdjustVariation(wornDetail.b, _Mat6ColorVariationAdj));
            // Every pass of the original takes material 1's colour variation from _FrostNormal.b, not _Mat1Normal.b.
            float variation = materialIndex == 0 ? frostDetail.b : detail.b;
            float3 color = lerp(material.color1, material.color2, AdjustVariation(variation, material.colorVariationAdjust));
            color = lerp(ColorizeDiffuse(diffuse.rgb, color), color, material.useMaterialAlbedo);
            // Material 6 is the worn surface itself, so its damage tint replaces the colour instead of overlaying it.
            float3 damagedColor = materialIndex == 5 ? material.damageTint : Overlay(color, material.damageTint);
            damagedColor = lerp(damagedColor, wornColor, material.useWear);
            float3 materialColor = lerp(lerp(color, material.dirtTint, softDirt), damagedColor, wear);

            float damagedRoughness = lerp(material.damageRoughness, wornDetail.a, material.useWear);
            float roughness = lerp(lerp(detail.a, material.dirtRoughness, softDirt), damagedRoughness, wear);
            float damagedMetallic = lerp(material.damageMetallic, _Mat6BaseMetalic, material.useWear);
            float metallic = lerp(material.baseMetallic, damagedMetallic, wear);

            // Rust, dirt and frost cover the material.
            float3 rustColor = lerp(_RustColor1.rgb, _RustColor2.rgb, AdjustVariation(rustDetail.b, _RustColorVariationAdj));
            float3 dirtColor = lerp(_DirtColor1.rgb, _DirtColor2.rgb, AdjustVariation(dirtDetail.b, _DirtColorVariationAdj));
            float3 frostColor = lerp(_FrostColor1.rgb, _FrostColor2.rgb, AdjustVariation(frostDetail.b, _FrostColorVariationAdj));
            float3 albedo = lerp(lerp(lerp(materialColor, rustColor, rust), dirtColor, dirt), frostColor, frost);
            roughness = lerp(lerp(lerp(roughness, rustDetail.a, rust), dirtDetail.a, dirt), frostDetail.a, frost);
            metallic = lerp(lerp(lerp(metallic, _RustMetaic, rust), _DirtMetalic, dirt), _FrostMetalic, frost);
            roughness += _SpecualVariationAdj * masks.r;     // masks.r is read without ToGamma

            float3 debugColor = MaterialDebugColors[materialIndex];
            debugColor = lerp(debugColor, float3(0.3, 0.5, 1.0), rust);
            debugColor = lerp(debugColor, float3(1.0, 0.5, 0.3), dirt);
            debugColor = lerp(debugColor, float3(0.8, 1.0, 0.3), frost);
            albedo = lerp(albedo, debugColor, _IdDebug);

            // Normal: the detail normals are stacked over _NormalEmision's normal in the original's order.
            // Material 6's normal doubles as the worn normal. The original's weights are kept exactly,
            // including a quirk: materials 1, 4 and 5 lean toward the worn normal even when not selected,
            // so on material 6 the worn normal is applied up to three extra times.
            float3 isMaterial123 = float3(materialIndex == 0, materialIndex == 1, materialIndex == 2);
            float3 isMaterial456 = float3(materialIndex == 3, materialIndex == 4, materialIndex == 5);
            float3 wornNormal = lerp(float3(DecodeDetailNormal(wornDetail) * isMaterial456.z, 1.0), float3(0.0, 0.0, 1.0),
                                     wear * (1.0 - _Mat6WearNormalInt));
            float3 normalTS = ToGamma(normalEmission).rgb * 2.0 - 1.0;
            normalTS = BlendNormalRNM(normalTS, GetNormalLayer(DecodeDetailNormal(materialDetail[0]), isMaterial123.x, wornNormal,
                                                               wear * _Mat1UseWear, wear * (1.0 - _Mat1WearNormalInt)));
            normalTS = BlendNormalRNM(normalTS, GetNormalLayer(DecodeDetailNormal(materialDetail[1]), isMaterial123.y, wornNormal,
                                                               wear * _Mat2UseWear * isMaterial123.y, wear * (1.0 - _Mat2WearNormalInt)));
            normalTS = BlendNormalRNM(normalTS, GetNormalLayer(DecodeDetailNormal(materialDetail[2]), isMaterial123.z, wornNormal,
                                                               wear * _Mat3UseWear * isMaterial123.z, wear * (1.0 - _Mat3WearNormalInt)));
            normalTS = BlendNormalRNM(normalTS, GetNormalLayer(DecodeDetailNormal(materialDetail[3]), isMaterial456.x, wornNormal,
                                                               wear * _Mat4UseWear, wear * (1.0 - _Mat4WearNormalInt)));
            normalTS = BlendNormalRNM(normalTS, GetNormalLayer(DecodeDetailNormal(materialDetail[4]), isMaterial456.y, wornNormal,
                                                               wear * _Mat5UseWear, wear * (1.0 - _Mat5WearNormalInt)));
            normalTS = BlendNormalRNM(normalTS, wornNormal);
            normalTS = BlendNormalRNM(normalTS, float3(DecodeDetailNormal(rustDetail) * rust, 1.0));
            normalTS = BlendNormalRNM(normalTS, float3(DecodeDetailNormal(dirtDetail) * dirt, 1.0));
            normalTS = BlendNormalRNM(normalTS, float3(DecodeDetailNormal(frostDetail) * frost, 1.0));

            surfaceData = (SurfaceData)0;
            surfaceData.albedo = albedo;
            surfaceData.alpha = diffuse.a;
            surfaceData.metallic = metallic;
            surfaceData.smoothness = saturate(1.0 - roughness);
            surfaceData.occlusion = 1.0 - softDirt;
            surfaceData.normalTS = normalTS;
            // Emission takes the material colour from before rust, dirt and frost, like the original.
            surfaceData.emission = materialColor * normalEmission.a * _EmissionAmount * _EmColor.rgb;
        }

        // normalTS is not unit length. The original also normalised only the result.
        float3 TransformSurfaceNormalToWorld(float3 normalTS, float3 normalWS, float4 tangentWS)
        {
            normalWS = normalize(normalWS);
            float3 bitangentWS = tangentWS.w * cross(normalWS, tangentWS.xyz);
            return NormalizeNormalPerPixel(TransformTangentToWorld(normalTS, float3x3(tangentWS.xyz, bitangentWS, normalWS)));
        }

        // Alpha test for the passes that need no other surface data.
        void ClipDiffuseAlpha(float2 uv)
        {
            clip(SAMPLE_TEXTURE2D(_DifuseMap, sampler_DifuseMap, TRANSFORM_TEX(uv, _DifuseMap)).a - ALPHA_CUTOFF);
        }

        // ---------------------------------------------------------------------
        // Vertex data shared by the ForwardLit and GBuffer passes
        // ---------------------------------------------------------------------

        struct LitVaryings
        {
            float4 positionCS : SV_POSITION;
            float2 uv : TEXCOORD0;
            float3 positionWS : TEXCOORD1;
            float3 normalWS : TEXCOORD2;
            float4 tangentWS : TEXCOORD3;                   // w: bitangent sign
            half4 fogFactorAndVertexLight : TEXCOORD4;      // x: unused (the original had no fog), yzw: per-vertex lights
            half3 vertexSH : TEXCOORD5;
        #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            float4 shadowCoord : TEXCOORD6;
        #endif
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
            output.tangentWS = float4(normalInputs.tangentWS, input.tangentOS.w * GetOddNegativeScale());
            output.uv = input.uv0;
            output.fogFactorAndVertexLight = half4(0.0, VertexLighting(positionInputs.positionWS, normalInputs.normalWS));

            // Ambient light comes from light probes only. No scene lightmapped this shader in the original.
            OUTPUT_SH4(positionInputs.positionWS, output.normalWS, GetWorldSpaceNormalizeViewDir(positionInputs.positionWS),
                       output.vertexSH, output.probeOcclusion);

        #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            output.shadowCoord = GetShadowCoord(positionInputs);
        #endif
            return output;
        }

        void InitializeInputData(LitVaryings input, float3 normalTS, out InputData inputData)
        {
            inputData = (InputData)0;
            inputData.positionWS = input.positionWS;
            inputData.positionCS = input.positionCS;
            inputData.normalWS = TransformSurfaceNormalToWorld(normalTS, input.normalWS, input.tangentWS);
            inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
        #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            inputData.shadowCoord = input.shadowCoord;
        #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
            inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
        #else
            inputData.shadowCoord = float4(0, 0, 0, 0);
        #endif
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

        // Surface data for a lit pass, alpha-tested, with the glow added to the emission.
        SurfaceData GetLitSurfaceData(LitVaryings input)
        {
            SurfaceData surfaceData;
            InitializeSurfaceData(input.uv, surfaceData);
            clip(surfaceData.alpha - ALPHA_CUTOFF);
            surfaceData.alpha = 1.0;
            surfaceData.emission += GetGlow();
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

            // URP Lit's keywords, without static and dynamic lightmaps and without fog.
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

                outColor = half4(UniversalFragmentPBR(inputData, surfaceData).rgb, 1.0);
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
                float2 uv : TEXCOORD0;
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
                ClipDiffuseAlpha(input.uv);
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
                float2 uv : TEXCOORD0;
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
                output.uv = input.uv0;
                return output;
            }

            half DepthOnlyFragment(DepthOnlyVaryings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                ClipDiffuseAlpha(input.uv);
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
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float4 tangentWS : TEXCOORD2;
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
                output.tangentWS = float4(normalInputs.tangentWS, input.tangentOS.w * GetOddNegativeScale());
                output.uv = input.uv0;
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

                // Only the normal and alpha are used. The compiler strips the rest of the surface.
                SurfaceData surfaceData;
                InitializeSurfaceData(input.uv, surfaceData);
                clip(surfaceData.alpha - ALPHA_CUTOFF);
            #if defined(LOD_FADE_CROSSFADE)
                LODFadeCrossFade(input.positionCS);
            #endif

                outNormalWS = half4(TransformSurfaceNormalToWorld(surfaceData.normalTS, input.normalWS, input.tangentWS), 0.0);
            #ifdef _WRITE_RENDERING_LAYERS
                outRenderingLayers = EncodeMeshRenderingLayer();
            #endif
            }
            ENDHLSL
        }

        // Albedo and emission for the lightmapper. Like the original it does not alpha-test.
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
                float2 uv : TEXCOORD0;
            #ifdef EDITOR_VISUALIZATION
                float2 VizUV : TEXCOORD1;
                float4 LightCoord : TEXCOORD2;
            #endif
            };

            MetaVaryings MetaPassVertex(Attributes input)
            {
                MetaVaryings output = (MetaVaryings)0;
                output.positionCS = UnityMetaVertexPosition(input.positionOS.xyz, input.uv1, input.uv2);
                output.uv = input.uv0;
            #ifdef EDITOR_VISUALIZATION
                UnityEditorVizData(input.positionOS.xyz, input.uv0, input.uv1, input.uv2, output.VizUV, output.LightCoord);
            #endif
                return output;
            }

            half4 MetaPassFragment(MetaVaryings input) : SV_Target
            {
                SurfaceData surfaceData;
                InitializeSurfaceData(input.uv, surfaceData);

                // Diffuse plus some specular for rough surfaces, as Unity's lightmapping albedo.
                BRDFData brdfData;
                InitializeBRDFData(surfaceData.albedo, surfaceData.metallic, surfaceData.specular, surfaceData.smoothness,
                                   surfaceData.alpha, brdfData);
                MetaInput metaInput = (MetaInput)0;
                metaInput.Albedo = brdfData.diffuse + brdfData.specular * brdfData.roughness * 0.5;
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
