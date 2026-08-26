// Upgrade NOTE: replaced 'glstate_matrix_projection' with 'UNITY_MATRIX_P'

Shader "ZeroGravity/Effects/FlareShader" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_SecTex ("Sec Tex", 2D) = "black" {}
		_CenterSize ("CenterSize", Float) = 1
		_Sample ("Sample", Float) = 1
		_Brightness ("Brightness", Float) = 1
		_Tint ("Tint", Vector) = (1,1,1,1)
		_Rotate ("Rotate", Float) = 0
		_Rotate2 ("Rotate2", Float) = 0
		_GlobalIntensity ("GlobalIntensity", Float) = 1
	}
	SubShader {
		Tags { "DisableBatching" = "true" "IGNOREPROJECTOR" = "true" "QUEUE" = "Overlay" "RenderType" = "Transparent" }
		Pass {
			Tags { "DisableBatching" = "true" "IGNOREPROJECTOR" = "true" "QUEUE" = "Overlay" "RenderType" = "Transparent" }
			Blend One One, One One
			ZTest Always
			ZWrite Off
			GpuProgramID 19888
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			struct v2f
			{
				float2 texcoord : TEXCOORD0;
				float2 texcoord4 : TEXCOORD4;
				float4 position : SV_POSITION0;
				float4 normal : NORMAL0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float depth : DEPTH0;
				float4 texcoord3 : TEXCOORD3;
				float4 color : COLOR0;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _MainTex_ST;
			float4 _SecTex_ST;
			float _Sample;
			float _CenterSize;
			float _Rotate;
			float _Rotate2;
			// $Globals ConstantBuffers for Fragment Shader
			float4 _Tint;
			float _Brightness;
			float _GlobalIntensity;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			sampler2D _CameraDepthTexture;
			// Texture params for Fragment Shader
			sampler2D _MainTex;
			sampler2D _SecTex;

			// Keywords:
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                float4 tmp3;
                float4 tmp4;
                float4 tmp5;
                float4 tmp6;
                tmp0 = unity_ObjectToWorld._m13_m13_m13_m13 * unity_MatrixV._m01_m11_m21_m31;
                tmp0 = unity_MatrixV._m00_m10_m20_m30 * unity_ObjectToWorld._m03_m03_m03_m03 + tmp0;
                tmp0 = unity_MatrixV._m02_m12_m22_m32 * unity_ObjectToWorld._m23_m23_m23_m23 + tmp0;
                tmp0 = unity_MatrixV._m03_m13_m23_m33 * unity_ObjectToWorld._m33_m33_m33_m33 + tmp0;
                tmp1.x = dot(unity_ObjectToWorld._m00_m10_m20_m30, unity_ObjectToWorld._m00_m10_m20_m30);
                tmp1.y = dot(unity_ObjectToWorld._m01_m11_m21_m31, unity_ObjectToWorld._m01_m11_m21_m31);
                tmp1.xy = sqrt(tmp1.xy);
                tmp2.xy = tmp1.xy * v.vertex.xy;
                tmp2.zw = float2(0.0, 0.0);
                tmp0 = tmp0 - tmp2;
                tmp1 = tmp0.yyyy * UNITY_MATRIX_P._m01_m11_m21_m31;
                tmp1 = UNITY_MATRIX_P._m00_m10_m20_m30 * tmp0.xxxx + tmp1;
                tmp1 = UNITY_MATRIX_P._m02_m12_m22_m32 * tmp0.zzzz + tmp1;
                tmp0 = UNITY_MATRIX_P._m03_m13_m23_m33 * tmp0.wwww + tmp1;
                tmp1 = unity_ObjectToWorld._m13_m13_m13_m13 * unity_MatrixVP._m01_m11_m21_m31;
                tmp1 = unity_MatrixVP._m00_m10_m20_m30 * unity_ObjectToWorld._m03_m03_m03_m03 + tmp1;
                tmp1 = unity_MatrixVP._m02_m12_m22_m32 * unity_ObjectToWorld._m23_m23_m23_m23 + tmp1;
                tmp1 = unity_MatrixVP._m03_m13_m23_m33 * unity_ObjectToWorld._m33_m33_m33_m33 + tmp1;
                tmp1 = tmp1 - tmp0;
                tmp2.x = 1.0 / _Sample;
                tmp3 = float4(0.0, 0.0, 0.0, 0.0);
                tmp4 = float4(0.0, 0.0, 0.0, 0.0);
                tmp2.yw = float2(0.0, 0.0);
                tmp5.x = 0.0;
                for (float i = tmp5.x; i < _Sample; i += 1) {
                    tmp5.y = i / _Sample;
                    tmp5.y = -tmp5.y * _CenterSize + 1.0;
                    tmp3 = tmp5.yyyy * tmp1 + tmp0;
                    tmp6.xz = tmp3.xw * float2(0.5, 0.5);
                    tmp2.yz = tmp3.wy * _ProjectionParams.wx;
                    tmp6.w = tmp2.z * 0.5;
                    tmp3.xy = tmp6.zz + tmp6.xw;
                    tmp5.yz = tmp3.xy / tmp3.ww;
                    tmp6 = tex2Dlod(_CameraDepthTexture, float4(tmp5.yz, 0, 0.0));
                    tmp2.z = _ZBufferParams.x * tmp6.x + _ZBufferParams.y;
                    tmp4 = float4(1.0, 1.0, 1.0, 1.0) / tmp2.zzzz;
                    tmp2.z = tmp4.w < tmp2.y;
                    tmp6.xy = tmp3.xy < float2(0.0, 0.0);
                    tmp2.z = uint1(tmp2.z) | uint1(tmp6.x);
                    tmp2.z = uint1(tmp6.y) | uint1(tmp2.z);
                    tmp5.yz = tmp5.yz > float2(1.0, 1.0);
                    tmp2.z = uint1(tmp2.z) | uint1(tmp5.y);
                    tmp2.z = uint1(tmp5.z) | uint1(tmp2.z);
                    tmp5.y = tmp2.x + tmp2.w;
                    tmp2.w = tmp2.z ? tmp5.y : tmp2.w;
                }
                o.texcoord1 = tmp3;
                o.texcoord2 = tmp4;
                o.depth.x = tmp2.y;
                o.color.x = tmp2.w;
                tmp1.x = _Rotate * _Time.y;
                tmp1.x = sin(tmp1.x);
                tmp2.x = cos(tmp1.x);
                tmp3.x = -tmp1.x;
                tmp1.yz = v.texcoord.xy - float2(0.5, 0.5);
                tmp3.y = tmp2.x;
                tmp2.x = dot(tmp3.xy, tmp1.xy);
                tmp3.z = tmp1.x;
                tmp2.y = dot(tmp3.xy, tmp1.xy);
                tmp1.xw = tmp2.xy + float2(0.5, 0.5);
                o.texcoord.xy = tmp1.xw * _MainTex_ST.xy + _MainTex_ST.zw;
                tmp1.x = _Rotate2 * _Time.y;
                tmp1.x = sin(tmp1.x);
                tmp2.x = cos(tmp1.x);
                tmp3.x = -tmp1.x;
                tmp3.y = tmp2.x;
                tmp2.z = dot(tmp3.xy, tmp1.xy);
                tmp3.z = tmp1.x;
                tmp2.w = dot(tmp3.xy, tmp1.xy);
                tmp1.xy = tmp2.zw + float2(0.5, 0.5);
                o.texcoord4.xy = tmp1.xy * _SecTex_ST.xy + _SecTex_ST.zw;
                o.position = tmp0;
                o.normal = float4(0.0, 0.0, 0.0, 0.0);
                o.texcoord3 = float4(0.0, 0.0, 0.0, 0.0);
                o.color.yzw = float3(0.0, 0.0, 0.0);
                return o;
			}
			// Keywords:
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                tmp0 = tex2D(_MainTex, inp.texcoord.xy);
                tmp1 = tex2D(_SecTex, inp.texcoord4.xy);
                tmp0.w = 1.0 - inp.color.x;
                tmp1.xyz = tmp0.www * tmp1.xyz;
                tmp0.xyz = tmp0.xyz * tmp0.www + tmp1.xyz;
                tmp0.xyz = tmp0.xyz * _Brightness.xxx;
                tmp0.xyz = tmp0.xyz * _Tint.xyz;
                tmp1.x = max(_GlobalIntensity, 0.0);
                tmp0.w = 1.0;
                o.sv_target = tmp0 * tmp1.xxxx;
                return o;
			}
			ENDCG
		}
	}
}
