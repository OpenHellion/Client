Shader "ZeroGravity/Surface/MultiMaterialDoubleSided" {
	Properties {
		_IdMask ("IdMask", 2D) = "black" {}
		_Masks ("Masks", 2D) = "white" {}
		_DifuseMap ("DifuseMap", 2D) = "white" {}
		_NormalEmision ("Normal+Emision", 2D) = "white" {}
		_WearAmount ("WearAmount", Range(0, 1)) = 0
		_SoftDrtAmount ("SoftDrtAmount", Range(0, 1)) = 0
		_Mat1Normal ("Mat1Normal", 2D) = "gray" {}
		_Mat1Color1 ("Mat1Color1", Vector) = (0,0,0,1)
		_Mat1Color2 ("Mat1Color2", Vector) = (0,0,0,1)
		_Mat1BaseMetalic ("Mat1BaseMetalic", Range(0, 1)) = 0.1038745
		_Mat1DmgTint ("Mat1DmgTint", Vector) = (1,1,1,1)
		_Mat1DmgRoughnes ("Mat1DmgRoughnes", Range(0, 1)) = 0
		_Mat1DmgMetalic ("Mat1DmgMetalic", Range(0, 1)) = 0
		_Mat1DirtTint ("Mat1DirtTint", Vector) = (1,1,1,1)
		_Mat1DirtRoughness ("Mat1DirtRoughness", Range(0, 1)) = 0
		_Mat2Normal ("Mat2Normal", 2D) = "gray" {}
		_Mat2Color1 ("Mat2Color1", Vector) = (0,0,0,1)
		_Mat2Color2 ("Mat2Color2", Vector) = (0,0,0,1)
		_Mat2BaseMetalic ("Mat2BaseMetalic", Range(0, 1)) = 0
		_Mat2DmgTint ("Mat2DmgTint", Vector) = (1,1,1,1)
		_Mat2DmgRoughness ("Mat2DmgRoughness", Range(0, 1)) = 0
		_Mat2DmgMetalic ("Mat2DmgMetalic", Range(0, 1)) = 0
		_Mat2DirtTint ("Mat2DirtTint", Vector) = (1,1,1,1)
		_Mat2DirtRoughness ("Mat2DirtRoughness", Range(0, 1)) = 0
		_Mat3Normal ("Mat3Normal", 2D) = "gray" {}
		_Mat3Color1 ("Mat3Color1", Vector) = (0,0,0,1)
		_Mat3Color2 ("Mat3Color2", Vector) = (0,0,0,1)
		_Mat3BaseMetalic ("Mat3BaseMetalic", Range(0, 1)) = 0
		_Mat3DmgTint ("Mat3DmgTint", Vector) = (1,1,1,1)
		_Mat3DmgRoughness ("Mat3DmgRoughness", Range(0, 1)) = 0
		_Mat3DmgMetalic ("Mat3DmgMetalic", Range(0, 1)) = 0
		_Mat3DirtTint ("Mat3DirtTint", Vector) = (1,1,1,1)
		_Mat3DirtRoughness ("Mat3DirtRoughness", Range(0, 1)) = 0
		_Mat4Normal ("Mat4Normal", 2D) = "gray" {}
		_Mat4Color1 ("Mat4Color1", Vector) = (0,0,0,1)
		_Mat4Color2 ("Mat4Color2", Vector) = (0,0,0,1)
		_Mat4BaseMetalic ("Mat4BaseMetalic", Range(0, 1)) = 0
		_Mat4DmgTint ("Mat4DmgTint", Vector) = (1,1,1,1)
		_Mat4DirtRouthness ("Mat4DirtRouthness", Range(0, 1)) = 0
		_Mat4DmgMetalic ("Mat4DmgMetalic", Range(0, 1)) = 0
		_Mat4DirtTint ("Mat4DirtTint", Vector) = (1,1,1,1)
		_Mat4DmgRoughness ("Mat4DmgRoughness", Range(0, 1)) = 0
		_Mat5Normal ("Mat5Normal", 2D) = "gray" {}
		_Mat5Color1 ("Mat5Color1", Vector) = (0,0,0,1)
		_Mat5Color2 ("Mat5Color2", Vector) = (0,0,0,1)
		_Mat5BaseMetalic ("Mat5BaseMetalic", Range(0, 1)) = 0
		_Mat5DmgTint ("Mat5DmgTint", Vector) = (1,1,1,1)
		_Mat5DmgRoughness ("Mat5DmgRoughness", Range(0, 1)) = 0
		_Mat5DmgMetalic ("Mat5DmgMetalic", Range(0, 1)) = 0
		_Mat5DirtTint ("Mat5DirtTint", Vector) = (1,1,1,1)
		_Mat5DirtRoughness ("Mat5DirtRoughness", Range(0, 1)) = 0
		_Mat6Normal ("Mat6Normal", 2D) = "white" {}
		_Mat6Color1 ("Mat6Color1", Vector) = (0,0,0,1)
		_Mat6Color2 ("Mat6Color2", Vector) = (0,0,0,1)
		_Mat6BaseMetalic ("Mat6BaseMetalic", Range(0, 1)) = 0
		_Mat6DmgTint ("Mat6DmgTint", Vector) = (1,1,1,1)
		_Mat6DmgRoughness ("Mat6DmgRoughness", Range(0, 1)) = 0
		_Mat6DmgMetalic ("Mat6DmgMetalic", Range(0, 1)) = 0
		_Mat6DirtColor ("Mat6DirtColor", Vector) = (1,1,1,1)
		_Mat6DirtRoughness ("Mat6DirtRoughness", Range(0, 1)) = 0
		_RustNormal ("RustNormal", 2D) = "gray" {}
		_RustAmount ("RustAmount", Range(0, 1)) = 0
		_RustColor1 ("RustColor1", Vector) = (0,0,0,1)
		_RustColor2 ("RustColor2", Vector) = (0,0,0,1)
		_RustMetaic ("RustMetaic", Range(0, 1)) = 0
		_DirtNormal ("DirtNormal", 2D) = "gray" {}
		_DirtAmount ("DirtAmount", Range(0, 1)) = 0
		_DirtColor1 ("DirtColor1", Vector) = (0,0,0,1)
		_DirtColor2 ("DirtColor2", Vector) = (0,0,0,1)
		_DirtMetalic ("DirtMetalic", Range(0, 1)) = 0
		_FrostNormal ("FrostNormal", 2D) = "gray" {}
		_FrostAmount ("FrostAmount", Range(0, 1)) = 0
		_FrostColor1 ("FrostColor1", Vector) = (1,1,1,1)
		_FrostColor2 ("FrostColor2", Vector) = (0.6,0.9,0.9,1)
		_FrostMetalic ("FrostMetalic", Range(0, 1)) = 0
		[MaterialToggle] _IdDebug ("IdDebug", Float) = 0.6
		_EmissionAmount ("EmissionAmount", Float) = 0
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
		[MaterialToggle] _Mat1UseMatAlbedo ("Mat1UseMatAlbedo", Float) = 0
		[MaterialToggle] _Mat2UseMatAlbedo ("Mat2UseMatAlbedo", Float) = 0
		[MaterialToggle] _Mat3UseMatAlbedo ("Mat3UseMatAlbedo", Float) = 0
		[MaterialToggle] _Mat4UseMatAlbedo ("Mat4UseMatAlbedo", Float) = 0
		[MaterialToggle] _Mat5UseMatAlbedo ("Mat5UseMatAlbedo", Float) = 0
		[MaterialToggle] _Mat6UseMatAlbedo ("Mat6UseMatAlbedo", Float) = 0
		[HideInInspector] _Cutoff ("Alpha cutoff", Range(0, 1)) = 0.5
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = 1;
		}
		ENDCG
	}
	Fallback "Diffuse"
	//CustomEditor "MultiMaterialInspector"
}
