// Upgrade NOTE: replaced 'glstate_matrix_projection' with 'UNITY_MATRIX_P'

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
	SubShader {
		Tags { "DisableBatching" = "true" "IGNOREPROJECTOR" = "true" "QUEUE" = "Overlay" "RenderType" = "Transparent" }
		Pass {
			Tags { "DisableBatching" = "true" "IGNOREPROJECTOR" = "true" "QUEUE" = "Overlay" "RenderType" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha, SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			GpuProgramID 12462
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			struct v2f
			{
				float2 texcoord : TEXCOORD0;
				float depth : DEPTH0;
				float4 position : SV_POSITION0;
				float4 normal : NORMAL0;
				float4 texcoord1 : TEXCOORD1;
				float4 color : COLOR0;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _MainTex_ST;
			// $Globals ConstantBuffers for Fragment Shader
			float4 _Tint;
			float _Brightness;
			float _timeMultiplier;
			float _depthPower;
			float _highlight;
			float _click;
			float4 _clickedColor;
			float4 _Color;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _MainTex;

			// Keywords:
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                tmp0.x = dot(unity_ObjectToWorld._m00_m10_m20_m30, unity_ObjectToWorld._m00_m10_m20_m30);
                tmp0.x = sqrt(tmp0.x);
                tmp0.x = tmp0.x * v.vertex.x;
                tmp1 = unity_ObjectToWorld._m13_m13_m13_m13 * unity_MatrixV._m01_m11_m21_m31;
                tmp1 = unity_MatrixV._m00_m10_m20_m30 * unity_ObjectToWorld._m03_m03_m03_m03 + tmp1;
                tmp1 = unity_MatrixV._m02_m12_m22_m32 * unity_ObjectToWorld._m23_m23_m23_m23 + tmp1;
                tmp1 = unity_MatrixV._m03_m13_m23_m33 * unity_ObjectToWorld._m33_m33_m33_m33 + tmp1;
                tmp2.x = dot(unity_ObjectToWorld._m01_m11_m21_m31, unity_ObjectToWorld._m01_m11_m21_m31);
                tmp2.x = sqrt(tmp2.x);
                tmp0.yz = tmp2.xx * v.vertex.yx;
                tmp0.w = 0.0;
                tmp2 = tmp1 - tmp0.xyww;
                tmp0 = tmp1 - tmp0.zyww;
                tmp1.x = tmp2.y * UNITY_MATRIX_P._m31;
                tmp1.x = UNITY_MATRIX_P._m30 * tmp2.x + tmp1.x;
                tmp1.x = UNITY_MATRIX_P._m32 * tmp2.z + tmp1.x;
                tmp1.x = UNITY_MATRIX_P._m33 * tmp2.w + tmp1.x;
                o.depth.x = tmp1.x * _ProjectionParams.w;
                o.texcoord.xy = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                tmp1 = tmp0.yyyy * UNITY_MATRIX_P._m01_m11_m21_m31;
                tmp1 = UNITY_MATRIX_P._m00_m10_m20_m30 * tmp0.xxxx + tmp1;
                tmp1 = UNITY_MATRIX_P._m02_m12_m22_m32 * tmp0.zzzz + tmp1;
                o.position = UNITY_MATRIX_P._m03_m13_m23_m33 * tmp0.wwww + tmp1;
                o.normal = float4(0.0, 0.0, 0.0, 0.0);
                o.texcoord1 = float4(0.0, 0.0, 0.0, 0.0);
                o.color = float4(0.0, 0.0, 0.0, 0.0);
                return o;
			}
			// Keywords:
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                tmp0.x = _timeMultiplier * _Time.y;
                tmp0.x = sin(tmp0.x);
                tmp0.x = tmp0.x + 1.0;
                tmp0.x = tmp0.x * 0.5;
                tmp0.x = log(tmp0.x);
                tmp0.x = tmp0.x * _depthPower;
                tmp0.x = exp(tmp0.x);
                tmp0.x = tmp0.x * _highlight + 1.0;
                tmp0.yz = float2(1.0, 1.0) - inp.texcoord.xy;
                tmp1 = tex2D(_MainTex, tmp0.yz);
                tmp0.yzw = tmp1.xyz * _Brightness.xxx;
                o.sv_target.w = tmp1.w * _Tint.w;
                tmp0.yzw = tmp0.yzw * _Tint.xyz;
                tmp0.yzw = tmp0.yzw * _Color.xyz;
                tmp0.xyz = tmp0.xxx * tmp0.yzw;
                tmp1.xyz = _clickedColor.xyz - float3(1.0, 1.0, 1.0);
                tmp1.xyz = _click.xxx * tmp1.xyz + float3(1.0, 1.0, 1.0);
                o.sv_target.xyz = tmp0.xyz * tmp1.xyz;
                return o;
			}
			ENDCG
		}
	}
}
