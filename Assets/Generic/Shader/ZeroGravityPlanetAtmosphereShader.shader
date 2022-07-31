Shader "ZeroGravity/Planet/AtmosphereShader" {
	Properties {
		_fresnel ("fresnel", Float) = 1
		_FresnelPower ("FresnelPower", Float) = 1
		_color1 ("color1", Vector) = (1,1,1,1)
		_color2 ("color2", Vector) = (1,1,1,1)
		_AtmosphereNoise ("AtmosphereNoise", 2D) = "white" {}
		_ColorPower ("ColorPower", Float) = 1
		_ColorSlide ("ColorSlide", Range(0, 1)) = 0.5
		_AtmosphereLightPower ("AtmosphereLightPower", Float) = 1
		_Opacity ("Opacity", Float) = 1
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