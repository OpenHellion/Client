Shader "ZeroGravity/Surface/StandardDoubleSided" {
	Properties {
		_Color ("Color", Vector) = (1,1,1,1)
		_MainTex ("Albedo", 2D) = "white" {}
		_Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5
		_Glossiness ("Smoothness", Range(0, 1)) = 0.5
		_GlossMapScale ("Smoothness Scale", Range(0, 1)) = 1
		[Enum(Metallic Alpha,0,Albedo Alpha,1)] _SmoothnessTextureChannel ("Smoothness texture channel", Float) = 0
		[Gamma] _Metallic ("Metallic", Range(0, 1)) = 0
		_MetallicGlossMap ("Metallic", 2D) = "white" {}
		[ToggleOff] _SpecularHighlights ("Specular Highlights", Float) = 1
		[ToggleOff] _GlossyReflections ("Glossy Reflections", Float) = 1
		_BumpScale ("Scale", Float) = 1
		_BumpMap ("Normal Map", 2D) = "bump" {}
		_Parallax ("Height Scale", Range(0.005, 0.08)) = 0.02
		_ParallaxMap ("Height Map", 2D) = "black" {}
		_OcclusionStrength ("Strength", Range(0, 1)) = 1
		_OcclusionMap ("Occlusion", 2D) = "white" {}
		_EmissionColor ("Color", Vector) = (0,0,0,1)
		_EmissionMap ("Emission", 2D) = "white" {}
		_DetailMask ("Detail Mask", 2D) = "white" {}
		_DetailAlbedoMap ("Detail Albedo x2", 2D) = "grey" {}
		_DetailNormalMapScale ("Scale", Float) = 1
		_DetailNormalMap ("Normal Map", 2D) = "bump" {}
		[Enum(UV0,0,UV1,1)] _UVSec ("UV Set for secondary textures", Float) = 0
		[HideInInspector] _Mode ("__mode", Float) = 0
		[HideInInspector] _SrcBlend ("__src", Float) = 1
		[HideInInspector] _DstBlend ("__dst", Float) = 0
		[HideInInspector] _ZWrite ("__zw", Float) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		Cull Off

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows alphatest:_Cutoff addshadow

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input
		{
			float2 uv_MainTex;
		};

		sampler2D _EmissionMap;
		sampler2D _MetallicGlossMap;
		sampler2D _OcclusionMap;
		sampler2D _BumpMap;

		sampler2D _DetailMask;
		sampler2D _DetailAlbedoMap;
		sampler2D _DetailNormalMap;

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		half _GlossMapScale;
		half _OcclusionStrength;
		half _BumpScale;

		fixed3 _EmissionColor;
		half _EmissionControl;

		half _DetailNormalMapScale;



		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			// Colour and detail.
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			fixed4 albedoDetail = tex2D(_DetailAlbedoMap, IN.uv_MainTex) * tex2D(_DetailMask, IN.uv_MainTex);
			o.Albedo = c.rgb * albedoDetail.rgb * 4;

			fixed4 metallicGloss = tex2D(_MetallicGlossMap, IN.uv_MainTex) * _GlossMapScale;
			o.Metallic = metallicGloss.r * _Metallic;
			o.Smoothness = metallicGloss.a * _Glossiness;
			o.Alpha = c.a;

			fixed4 normal = tex2D(_BumpMap, IN.uv_MainTex) * _BumpScale;
			fixed4 normalDetail = tex2D(_DetailNormalMap, IN.uv_MainTex) * tex2D(_DetailMask, IN.uv_MainTex) * _DetailNormalMapScale;
			o.Normal = normal * normalDetail;

			// Emission if enabled.
			fixed4 emissionMap = tex2D(_EmissionMap, IN.uv_MainTex);
			o.Emission = emissionMap.rgb * _EmissionColor * (_EmissionControl / 8);

			fixed4 occlusion = tex2D(_OcclusionMap, IN.uv_MainTex) * _OcclusionStrength;
			o.Occlusion = occlusion;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
