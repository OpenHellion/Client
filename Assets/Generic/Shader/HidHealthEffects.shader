Shader "Hid/HealthEffects" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_BloodTex ("Blood Texture", 2D) = "white" {}
		_BloodMask ("Blood Mask", 2D) = "black" {}
		_HitFront ("HitFront", Range(0, 1)) = 0
		_HitBack ("HitBack", Range(0, 1)) = 0
		_HitLeft ("HitLeft", Range(0, 1)) = 0
		_HitRight ("HitRight", Range(0, 1)) = 0
		_Health ("Health", Range(0, 1)) = 1
		_Vignette ("Vignette", 2D) = "white" {}
		_Veins ("Veins", 2D) = "white" {}
		_VeinsMask ("Veins Mask", 2D) = "white" {}
		_VeinsAppear ("VeinsAppear", Range(0, 1)) = 1
		_BlurIterations ("BlurIterations", Float) = 1
		_BlurAmount ("BlurAmount", Float) = 1
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