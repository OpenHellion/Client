Shader "ZeroG/sunShader" {
	Properties {
		_Albedo ("Albedo", 2D) = "white" {}
		_ColorTint ("Color Tint", Vector) = (1,1,1,1)
		_NoiseTileScale ("Noise Tile/Scale", Range(0, 50)) = 2.555199
		_FresnelColor ("Fresnel Color", Vector) = (1,1,1,1)
		_FresnelSpread ("Fresnel Spread", Range(1, 5)) = 2
		_EdgeOpacity ("Edge Opacity", Range(0, 10)) = 10
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
	//CustomEditor "ShaderForgeMaterialInspector"
}