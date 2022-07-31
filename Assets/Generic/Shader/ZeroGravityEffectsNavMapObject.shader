Shader "ZeroGravity/Effects/NavMapObject" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Brightness ("Brightness", Float) = 1
		_Tint ("Tint", Vector) = (1,1,1,1)
		_Color ("Color", Vector) = (1,1,1,1)
		_timeMultiplier ("Time Multiplier", Float) = 1
		_depthPower ("Power", Float) = 1
		_highlightScaleMultiplier ("Highlight Scale Multiplier", Float) = 1
		_clickedScaleMultiplier ("Clicked Scale Multiplier", Float) = 1
		_glowMultiplier ("Glow Multiplier", Float) = 1
		_highlight ("Highlight", Range(0, 1)) = 0
		_click ("Click", Range(0, 1)) = 0
		_clickedColor ("Clicked Color", Vector) = (1,1,1,1)
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
}