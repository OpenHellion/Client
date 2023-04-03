Shader "ZeroGravity/Effects/GlassEffect" {
	Properties {
		_MainTex ("Screen", 2D) = "black" {}
		_GlassTint ("GlassTint", Vector) = (0,0,0,0)
		_RefractionIntensity ("RefractionIntensity", Float) = 0
		_DisplacementTexture ("DisplacementTexture", 2D) = "white" {}
		_RefractionStrength ("RefractionStrength", Float) = 0
		_ReflectionCap ("ReflectionCap", Float) = 0
		_ReflectionIntensity ("ReflectionIntensity", Float) = 0
		_DirtTexture ("DirtTexture", 2D) = "white" {}
		_DirtIntensity ("DirtIntensity", Float) = 0
		_ReflectionSaturation ("ReflectionSaturation", Float) = 0
		_LowerHelmet ("LowerHelmet", Range(0, 1)) = 0
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