Shader "ZeroGravity/Surface/StandardTransparentClose" {
	Properties {
		[HideInInspector] __dirty ("", Float) = 1
		_MainTex ("MainTex", 2D) = "white" {}
		_Color ("Color", Vector) = (1,1,1,1)
		_BumpMap ("BumpMap", 2D) = "bump" {}
		_BumpScale ("BumpScale", Float) = 0
		_MetallicGlossMap ("MetallicGlossMap", 2D) = "white" {}
		_GlossMapScale ("GlossMapScale", Float) = 0
		_Transparency ("Transparency", Float) = 0
		_Metallic ("Metallic", Range(0, 1)) = 0
		_MinDistance ("MinDistance", Float) = 0
		_MaxDistance ("MaxDistance", Float) = 0
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