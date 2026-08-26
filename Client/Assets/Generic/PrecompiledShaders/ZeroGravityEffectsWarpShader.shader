Shader "ZeroGravity/Effects/WarpShader" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_WarpEffectTex ("Texture", 2D) = "black" {}
		_WarpIntensity ("Warp Intensity", Range(0, 0.1)) = 0
		_WarpSpeed ("Warp Speed", Float) = 0
		_WarpColor ("Warp Color", Vector) = (1,1,1,1)
		_WarpColorIntensity ("Warp Color Intensity", Float) = 1
		_WarpPower ("Warp Power", Float) = 1
		_EndsTex ("EndsTex", 2D) = "black" {}
		_Warp ("Warp", Range(0, 1)) = 1
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