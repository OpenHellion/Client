Shader "ZeroGravity/Effects/HoopHoloShader" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_min ("Min", Float) = 0
		_max ("Max", Float) = 1
		_multiplier ("Multiplier", Float) = 1
		_power ("Power", Float) = 1
		_glowTex ("Glow Texture", 2D) = "white" {}
		_animTex ("Anim Texture", 2D) = "white" {}
		_animSpeed ("Anim Speed", Float) = 1
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