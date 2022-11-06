Shader "ZeroGravity/Surface/StandardLightMap" {
	Properties {
		[HideInInspector] __dirty ("", Float) = 1
		_MainTex ("MainTex", 2D) = "white" {}
		_Color ("Color", Vector) = (0,0,0,0)
		_BumpMap ("BumpMap", 2D) = "bump" {}
		_BumpScale ("BumpScale", Float) = 0
		_EmissionMap ("EmissionMap", 2D) = "white" {}
		[HDR] _EmissionColor ("EmissionColor", Vector) = (0,0,0,0)
		_MetallicGlossMap ("MetallicGlossMap", 2D) = "white" {}
		_GlossMapScale ("GlossMapScale", Range(0, 1)) = 0
		_OcclusionMap ("OcclusionMap", 2D) = "white" {}
		_Lightmap ("Lightmap", 2D) = "black" {}
		_EmissionControl ("EmissionControl", Range(0, 1)) = 0
		[Toggle] _AlwaysEmit ("AlwaysEmit", Range(0, 1)) = 1
		[IntRange] _EmissionState ("EmissionState", Range(0, 3)) = 0
		[HDR] _ToxicColor ("ToxicColor", Vector) = (2,1.034483,0,0)
		[HDR] _LowPressureColor ("LowPressureColor", Vector) = (2,0,0,0)
		[HideInInspector] _texcoord ("", 2D) = "white" {}
		[HideInInspector] _texcoord4 ("", 2D) = "white" {}
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
	Fallback "Diffuse"
	//CustomEditor "ASEMaterialInspector"
}
