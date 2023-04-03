Shader "Custom/FuzzyObject" {
	Properties {
		_MainTexture ("MainTexture", 2D) = "white" {}
		_MainTextureTiling ("MainTextureTiling", Float) = 0
		[HDR] _MainTextureColor ("MainTextureColor", Vector) = (0,0,0,0)
		_MainTextureIntensity ("MainTextureIntensity", Range(0, 10)) = 0
		_MainTexturePanningSpeed ("MainTexturePanningSpeed", Float) = 1
		_PannerTexture ("PannerTexture", 2D) = "white" {}
		_PanningSpeed ("PanningSpeed", Float) = 1
		_PannerTextureTiling ("PannerTextureTiling", Float) = 1
		_Mask ("Mask", 2D) = "white" {}
		_MaskTile ("MaskTile", Float) = 0.84
		_MaskOffset ("MaskOffset", Float) = -0.66
		[HDR] _FresnelColor ("FresnelColor", Vector) = (0,0,0,0)
		_FresnelScale ("FresnelScale", Float) = 0
		_FresnelPower ("FresnelPower", Float) = 0
		_GeneralOpacity ("GeneralOpacity", Range(0, 1)) = 0
		[HideInInspector] _texcoord ("", 2D) = "white" {}
		[HideInInspector] __dirty ("", Float) = 1
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
	//CustomEditor "ASEMaterialInspector"
}