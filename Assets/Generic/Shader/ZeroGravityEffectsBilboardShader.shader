// Upgrade NOTE: replaced 'glstate_matrix_projection' with 'UNITY_MATRIX_P'

Shader "ZeroGravity/Effects/BilboardShader" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Brightness ("Brightness", Float) = 1
		_Tint ("Tint", Vector) = (1,1,1,1)
	}
	SubShader {
		Tags { "DisableBatching" = "true" "IGNOREPROJECTOR" = "true" "QUEUE" = "Transparent" "RenderType" = "Transparent" }
		Pass {
			Tags { "DisableBatching" = "true" "IGNOREPROJECTOR" = "true" "QUEUE" = "Transparent" "RenderType" = "Transparent" }
			Blend One One, One One
			GpuProgramID 60387
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			struct v2f
			{
				float2 texcoord : TEXCOORD0;
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
                o.texcoord.xy = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                tmp0 = unity_ObjectToWorld._m13_m13_m13_m13 * unity_MatrixV._m01_m11_m21_m31;
                tmp0 = unity_MatrixV._m00_m10_m20_m30 * unity_ObjectToWorld._m03_m03_m03_m03 + tmp0;
                tmp0 = unity_MatrixV._m02_m12_m22_m32 * unity_ObjectToWorld._m23_m23_m23_m23 + tmp0;
                tmp0 = unity_MatrixV._m03_m13_m23_m33 * unity_ObjectToWorld._m33_m33_m33_m33 + tmp0;
                tmp1.x = dot(unity_ObjectToWorld._m00_m10_m20_m30, unity_ObjectToWorld._m00_m10_m20_m30);
                tmp1.x = sqrt(tmp1.x);
                tmp1.x = tmp1.x * v.vertex.x;
                tmp2.x = dot(unity_ObjectToWorld._m01_m11_m21_m31, unity_ObjectToWorld._m01_m11_m21_m31);
                tmp2.x = sqrt(tmp2.x);
                tmp1.y = tmp2.x * v.vertex.y;
                tmp1.zw = float2(0.0, 0.0);
                tmp0 = tmp0 - tmp1;
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
                tmp0 = tex2D(_MainTex, inp.texcoord.xy);
                tmp0.xyz = tmp0.xyz * _Brightness.xxx;
                o.sv_target.xyz = tmp0.xyz * _Tint.xyz;
                o.sv_target.w = 1.0;
                return o;
			}
			ENDCG
		}
	}
}
