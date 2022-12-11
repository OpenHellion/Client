Shader "ZeroGravity/Effects/SunShader" {
	Properties {
		_StarTexture ("StarTexture", 2D) = "black" {}
		_GlowTexure ("GlowTexure", 2D) = "black" {}
		_node_4639 ("node_4639", Range(0, 1)) = 0
		_node_4131 ("node_4131", Range(0, 1)) = 1
		_node_8031 ("node_8031", Float) = 1
		_node_2310 ("node_2310", Float) = 1
		_node_6920 ("node_6920", Vector) = (0.5,0.5,0.5,1)
	}
	SubShader {
		LOD 100
		Tags { "IGNOREPROJECTOR" = "true" "QUEUE" = "Transparent" "RenderType" = "Transparent" }
		Pass {
			Name "FORWARD"
			LOD 100
			Tags { "IGNOREPROJECTOR" = "true" "LIGHTMODE" = "FORWARDBASE" "QUEUE" = "Transparent" "RenderType" = "Transparent" "SHADOWSUPPORT" = "true" }
			Blend One One, One One
			ZWrite Off
			GpuProgramID 63218
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			// $Globals ConstantBuffers for Fragment Shader
			float4 _StarTexture_ST;
			float4 _GlowTexure_ST;
			float _node_4639;
			float _node_4131;
			float _node_8031;
			float _node_2310;
			float4 _node_6920;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _GlowTexure;
			sampler2D _StarTexture;

			// Keywords: DIRECTIONAL
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                tmp0 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                tmp0 = tmp0 + unity_ObjectToWorld._m03_m13_m23_m33;
                tmp1 = tmp0.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp1 = unity_MatrixVP._m00_m10_m20_m30 * tmp0.xxxx + tmp1;
                tmp1 = unity_MatrixVP._m02_m12_m22_m32 * tmp0.zzzz + tmp1;
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp0.wwww + tmp1;
                o.texcoord.xy = v.texcoord.xy;
                return o;
			}
			// Keywords: DIRECTIONAL
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                tmp0.xy = inp.texcoord.xy * float2(2.0, 2.0) + float2(-1.0, -1.0);
                tmp0.x = dot(tmp0.xy, tmp0.xy);
                tmp0.x = sqrt(tmp0.x);
                tmp0.x = 1.0 - tmp0.x;
                tmp0.x = tmp0.x - _node_4639;
                tmp0.y = _node_4131 - _node_4639;
                tmp0.x = saturate(tmp0.x / tmp0.y);
                tmp0.x = log(tmp0.x);
                tmp0.x = tmp0.x * _node_8031;
                tmp0.x = exp(tmp0.x);
                tmp0.yz = inp.texcoord.xy * _GlowTexure_ST.xy + _GlowTexure_ST.zw;
                tmp1 = tex2D(_GlowTexure, tmp0.yz);
                tmp0.yzw = log(tmp1.xyz);
                tmp0.yzw = tmp0.yzw * _node_2310.xxx;
                tmp0.yzw = exp(tmp0.yzw);
                tmp1.xy = inp.texcoord.xy * _StarTexture_ST.xy + _StarTexture_ST.zw;
                tmp1 = tex2D(_StarTexture, tmp1.xy);
                tmp0.yzw = tmp0.yzw + tmp1.xyz;
                o.sv_target.xyz = _node_6920.xyz * tmp0.yzw + tmp0.xxx;
                o.sv_target.w = 1.0;
                return o;
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
