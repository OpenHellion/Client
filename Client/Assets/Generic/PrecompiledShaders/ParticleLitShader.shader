Shader "ParticleLitShader" {
	Properties {
		[HideInInspector] __dirty ("", Float) = 1
		_MainTex ("MainTex", 2D) = "white" {}
		_Color ("Color", Vector) = (0,0,0,0)
		_Float0 ("Float 0", Float) = 0
		_UnlitColor ("UnlitColor", Vector) = (0,0,0,0)
		[HideInInspector] _texcoord ("", 2D) = "white" {}
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
	//CustomEditor "ASEMaterialInspector"
}