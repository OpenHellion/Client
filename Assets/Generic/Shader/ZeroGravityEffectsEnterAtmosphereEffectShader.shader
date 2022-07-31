Shader "ZeroGravity/Effects/EnterAtmosphereEffectShader" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_noise ("noise", 2D) = "gray" {}
		_heat ("heat", Vector) = (1,1,1,1)
		_height ("Height", Float) = 0
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