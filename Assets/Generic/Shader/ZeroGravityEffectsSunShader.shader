Shader "ZeroGravity/Effects/SunShader" {
	Properties {
		_StarTexture ("StarTexture", 2D) = "black" {}
		_GlowTexure ("GlowTexure", 2D) = "black" {}
		_node_4639 ("node_4639", Range(0, 1)) = 0
		_node_4131 ("node_4131", Range(0, 1)) = 1
		_node_8031 ("node_8031", Float) = 1
		_node_2310 ("node_2310", Float) = 1
		_node_6920 ("node_6920", Vector) = (0.5,0.5,0.5,1)
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