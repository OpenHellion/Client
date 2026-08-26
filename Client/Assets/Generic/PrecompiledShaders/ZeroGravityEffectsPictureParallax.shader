Shader "ZeroGravity/Effects/PictureParallax" {
	Properties {
		_level5 ("level5", 2D) = "white" {}
		_Level5Height ("Level5 Height", Float) = 0
		_Level4 ("Level4", 2D) = "white" {}
		_Level4Height ("Level4 Height", Float) = 0
		_Level3 ("Level3", 2D) = "white" {}
		_Level3Height ("Level3 Height", Float) = 0
		_Level2 ("Level2", 2D) = "white" {}
		_Level2Height ("Level2 Height", Float) = 0
		_Level1 ("Level1", 2D) = "white" {}
		_Level1Height ("Level1 Height", Float) = 0
		_Metallic ("Metallic", Range(0, 1)) = 0
		_Roughness ("Roughness", 2D) = "white" {}
		_Normal ("Normal", 2D) = "bump" {}
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