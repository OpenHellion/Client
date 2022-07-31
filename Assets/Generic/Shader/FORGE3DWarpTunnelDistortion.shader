Shader "FORGE3D/WarpTunnelDistortion" {
	Properties {
		_Refraction ("Refraction", 2D) = "" {}
		_Opacity ("Opacity", Range(0, 1)) = 0
		_RefractionFactor ("Refraction Factor", Range(0, 2)) = 0
		_U_TileAnimFactor ("U_TileAnimFactor", Range(-5, 5)) = 0
		_V_TileAnimFactor ("V_TileAnimFactor", Range(-5, 5)) = 0
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
}