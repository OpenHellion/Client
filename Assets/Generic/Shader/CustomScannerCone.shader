Shader "Custom/ScannerCone" {
	Properties {
		_ScannerTexture ("ScannerTexture", 2D) = "white" {}
		_ScannerTextureTiling ("ScannerTextureTiling", Float) = 1
		_ColorStrength ("ColorStrength", Range(0, 5)) = 0
		[HDR] _ScannerColor ("ScannerColor", Vector) = (0,0.4726329,1,0)
		[Toggle(_SCANNERPANNINGONOFF_ON)] _ScannerPanningONOFF ("ScannerPanning ON/OFF", Float) = 0
		_ScannerPanningSpeed ("ScannerPanningSpeed", Float) = 1
		_GradientMask ("GradientMask", 2D) = "white" {}
		_MaskTile ("MaskTile", Float) = 0.84
		_MaskOffset ("MaskOffset", Float) = -0.66
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