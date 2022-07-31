Shader "ZeroGravity/Skybox/SkyboxShader" {
	Properties {
		_node_3136 ("node_3136", Cube) = "_Skybox" {}
		_node_5129 ("node_5129", Range(0, 1)) = 0
		_node_390 ("node_390", 2D) = "white" {}
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