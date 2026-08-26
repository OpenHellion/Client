Shader "ZeroGravity/Effects/SunShaderAdditive" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_NoiseTex ("Noise", 2D) = "black" {}
		_DispAmount ("Displacement", Float) = 0
		_Emission ("Emission", Float) = 1
		_Speed ("Speed", Float) = 1
		_Tint ("Tint", Vector) = (1,1,1,1)
		_RimAmount ("Rim Amount", Float) = 1
		_RimPower ("Rim Power", Float) = 1
	}
	SubShader {
		LOD 100
		Tags { "QUEUE" = "Transparent" "RenderType" = "Opaque" }
		Pass {
			LOD 100
			Tags { "QUEUE" = "Transparent" "RenderType" = "Opaque" }
			Blend One One, One One
			ZWrite Off
			Cull Off
			GpuProgramID 28437
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			struct v2f
			{
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
				float4 position : SV_POSITION0;
				float4 color : COLOR0;
				float3 texcoord2 : TEXCOORD2;
				float3 texcoord3 : TEXCOORD3;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _MainTex_ST;
			float4 _NoiseTex_ST;
			// $Globals ConstantBuffers for Fragment Shader
			float _DispAmount;
			float _Emission;
			float _Speed;
			float4 _Tint;
			float _RimAmount;
			float _RimPower;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _NoiseTex;
			sampler2D _MainTex;

			// Keywords:
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                o.texcoord.xy = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                o.texcoord1.xy = v.texcoord1.xy * _NoiseTex_ST.xy + _NoiseTex_ST.zw;
                tmp0 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                tmp1 = tmp0 + unity_ObjectToWorld._m03_m13_m23_m33;
                o.texcoord3.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp0.xyz;
                tmp0 = tmp1.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp0 = unity_MatrixVP._m00_m10_m20_m30 * tmp1.xxxx + tmp0;
                tmp0 = unity_MatrixVP._m02_m12_m22_m32 * tmp1.zzzz + tmp0;
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp1.wwww + tmp0;
                o.color = v.color;
                tmp0.x = dot(v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp0.y = dot(v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp0.z = dot(v.normal.xyz, unity_WorldToObject._m02_m12_m22);
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                o.texcoord2.xyz = tmp0.www * tmp0.xyz;
                return o;
			}
			// Keywords:
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                tmp0.y = _Time.x * _Speed + inp.texcoord1.y;
                tmp0.x = inp.texcoord1.x;
                tmp0 = tex2D(_NoiseTex, tmp0.xy);
                tmp0.xy = tmp0.xy - float2(0.5, 0.5);
                tmp0.xy = tmp0.xy * _DispAmount.xx + inp.texcoord.xy;
                tmp0 = tex2D(_MainTex, tmp0.xy);
                tmp0 = tmp0 * _Emission.xxxx;
                tmp0 = tmp0 * _Tint;
                tmp0.w = saturate(tmp0.w);
                tmp0 = tmp0 * inp.color;
                tmp1.xyz = _WorldSpaceCameraPos - inp.texcoord3.xyz;
                tmp1.w = dot(tmp1.xyz, tmp1.xyz);
                tmp1.w = rsqrt(tmp1.w);
                tmp1.xyz = tmp1.www * tmp1.xyz;
                tmp1.x = dot(tmp1.xyz, inp.texcoord2.xyz);
                tmp1.x = log(abs(tmp1.x));
                tmp1.x = tmp1.x * _RimPower;
                tmp1.x = exp(tmp1.x);
                tmp1.x = tmp1.x * _RimAmount;
                tmp1.xyz = saturate(tmp1.xxx);
                tmp1.w = 1.0;
                o.sv_target = tmp0 * tmp1;
                return o;
			}
			ENDCG
		}
	}
}
