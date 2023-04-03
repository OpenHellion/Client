Shader "ZeroGravity/Effects/OrbitShader" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		[HDR] _Color ("Color", Vector) = (1,1,1,1)
		_Speed ("Speed", Float) = 1
		_AnimationOffset ("Animation Offset", Float) = 0
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