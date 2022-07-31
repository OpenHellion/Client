Shader "ZeroGravity/Effects/FlareShader" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_SecTex ("Sec Tex", 2D) = "black" {}
		_CenterSize ("CenterSize", Float) = 1
		_Sample ("Sample", Float) = 1
		_Brightness ("Brightness", Float) = 1
		_Tint ("Tint", Vector) = (1,1,1,1)
		_Rotate ("Rotate", Float) = 0
		_Rotate2 ("Rotate2", Float) = 0
		_GlobalIntensity ("GlobalIntensity", Float) = 1
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