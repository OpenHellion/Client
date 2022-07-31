Shader "DamageParallaxShader" {
	Properties {
		[HideInInspector] __dirty ("", Float) = 1
		_MaskClipValue ("Mask Clip Value", Float) = 0.5
		_Texture1 ("Texture1", 2D) = "white" {}
		_Texture1Normal ("Texture1 Normal", 2D) = "bump" {}
		_Texture1Height ("Texture1 Height", Float) = 0
		_Texture1EmissionMask ("Texture1 Emission Mask", 2D) = "white" {}
		_Texture1EmissionColor ("Texture1 Emission Color", Vector) = (0,0,0,0)
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