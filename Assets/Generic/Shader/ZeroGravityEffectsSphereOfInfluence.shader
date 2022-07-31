Shader "ZeroGravity/Effects/SphereOfInfluence" {
	Properties {
		[HideInInspector] __dirty ("", Float) = 1
		_MaskClipValue ("Mask Clip Value", Float) = 0.5
		_TextureSample0 ("Texture Sample 0", 2D) = "white" {}
		_Color0 ("Color 0", Vector) = (0,0,0,0)
		_Float0 ("Float 0", Float) = 1
		_Color1 ("Color 1", Vector) = (0,0,0,0)
		_Float1 ("Float 1", Float) = 0
		_Color2 ("Color 2", Vector) = (0,0,0,0)
		_Appear ("Appear", Range(-0.01, 1)) = 0
		_Fade ("Fade", Range(0, 1)) = 0
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