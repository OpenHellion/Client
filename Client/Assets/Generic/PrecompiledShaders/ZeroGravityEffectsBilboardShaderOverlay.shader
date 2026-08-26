Shader "ZeroGravity/Effects/BilboardShaderOverlay" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Brightness ("Brightness", Float) = 1
		_Tint ("Tint", Vector) = (1,1,1,1)
		_depthMultiplier ("Depth Multiplier", Float) = 5000
		_timeMultiplier ("Time Multiplier", Float) = 1
		_depthPower ("Depth Power", Float) = 1
		_scaleMultiplier ("Scale Multiplier", Float) = 1
		_glowMultiplier ("Glow Multiplier", Float) = 1
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