Shader "Decal/DecalShader" {
	Properties {
		_Alpha ("Alpha", Range(0, 1)) = 1
		_Color ("Color + Diffuse Multiplier", Vector) = (1,1,1,1)
		_MainTex ("Diffuse", 2D) = "white" {}
		_NormalMultiplier ("Normals Strength", Range(0, 2)) = 1
		_BumpMap ("Normals", 2D) = "bump" {}
		_NormalClipAngle ("Normals Clip", Float) = 0.1
		_AttributeMultiplier ("Attribute Tex Multiplier", Range(0, 1)) = 1
		__MetallicGlossMap ("SpecularColor (RGB) Roughness(A)", 2D) = "black" {}
		[Toggle(_ApplyRoughness)] _ApplyRoughness ("Apply roughness", Float) = 0
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