Shader "ZeroGravity/Surface/StandardLightMap" {
	Properties {
		[HideInInspector] __dirty ("", Float) = 1
		_MainTex ("MainTex", 2D) = "white" {}
		_Color ("Color", Color) = (0,0,0,0)
		[Normal] _BumpMap ("BumpMap", 2D) = "bump" {}
		_BumpScale ("BumpScale", Float) = 0
		_EmissionMap ("EmissionMap", 2D) = "white" {}
		[HDR] _EmissionColor ("EmissionColor", Color) = (0,0,0,0)
		_MetallicGlossMap ("MetallicGlossMap", 2D) = "white" {}
		_GlossMapScale ("GlossMapScale", Range(0, 1)) = 0
		_OcclusionMap ("OcclusionMap", 2D) = "white" {}
		_Lightmap ("Lightmap", 2D) = "black" {}
		_EmissionControl ("EmissionControl", Range(0, 1)) = 0
		[Toggle] _AlwaysEmit ("AlwaysEmit", Range(0, 1)) = 1
		[IntRange] _EmissionState ("EmissionState", Range(0, 3)) = 0
		[HDR] _ToxicColor ("ToxicColor", Color) = (2,1.034483,0,0)
		[HDR] _LowPressureColor ("LowPressureColor", Color) = (2,0,0,0)
		[HideInInspector] _texcoord ("", 2D) = "white" {}
		[HideInInspector] _texcoord4 ("", 2D) = "white" {}
	}
	SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        sampler2D _MetallicGlossMap;
        sampler2D _BumpMap;
        sampler2D _OcclusionMap;
        sampler2D _EmissionMap;
		half _GlossMapScale;
        fixed4 _Color;
		fixed3 _EmissionColor;
		half _EmissionControl;
		fixed _AlwaysEmit;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
			o.Alpha = c.a;

			fixed4 metallicGloss = tex2D(_MetallicGlossMap, IN.uv_MainTex) * _GlossMapScale;
            o.Metallic = metallicGloss.r;
            o.Smoothness = metallicGloss.a;

			//fixed4 normal = tex2D(_BumpMap, IN.uv_MainTex);
			//o.Normal = normal.rgb;

			fixed4 occlusion = tex2D(_OcclusionMap, IN.uv_MainTex);
			o.Occlusion = occlusion.r;

			// Emission if enabled.
			fixed4 emissionMap = tex2D(_EmissionMap, IN.uv_MainTex);
			o.Emission = _AlwaysEmit == 1 ? emissionMap.rgb * _EmissionColor * _EmissionControl : 0;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
