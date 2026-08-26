Shader "ZeroGravity/Planet/GasPlanetSurfaceShader" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_LightRamp ("Light Ramp", 2D) = "white" {}
		_NormalMap ("Normal", 2D) = "bump" {}
		_DispMap ("Displacement Map", 2D) = "gray" {}
		_DispAmount ("Displacement Amount", Range(0, 10)) = 0
		_FlowMap ("Flow Map", 2D) = "gray" {}
		_FlowSpeed ("Flow Speed", Range(-10, 10)) = 0
		_FlowAmount ("Flow Amount", Range(-1, 1)) = 0
		_VignetteMin ("Vignette Min", Range(0, 1)) = 0
		_VignetteMax ("Vignette Max", Range(0, 1)) = 1
		_Tess ("Tessellation", Range(1, 128)) = 1
		_TessPhong ("Tessallation Phong", Range(0, 1)) = 0
		_MinDist ("Min Distance", Float) = 10
		_MaxDist ("Max Distance", Float) = 25
		_NormalDetail ("Normal Detail", 2D) = "bump" {}
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
}