Shader "ZeroGravity/Effects/SphereOfInfluence" {
	Properties {
		[HideInInspector] __dirty ("", Float) = 1
		_MaskClipValue ("Mask Clip Value", Float) = 0.5
		_TextureSample0 ("Texture Sample 0", 2D) = "white" {}
		_Color0 ("Color 0", Vector) = (0,0,0,0)
		_Float0 ("Float 0", Float) = 1
		_Color1 ("Color 1", Vector) = (0,0,0,0)
		_Float1 ("Float 1", Float) = 0
		_Color2 ("Color 2", Vector) = (0,0,0,0)
		_Appear ("Appear", Range(-0.01, 1)) = 0
		_Fade ("Fade", Range(0, 1)) = 0
		[HideInInspector] _texcoord ("", 2D) = "white" {}
	}
	SubShader {
		Tags { "IsEmissive" = "true" "QUEUE" = "Overlay+0" "RenderType" = "Overlay" }
		Pass {
			Name "FORWARD"
			Tags { "IsEmissive" = "true" "LIGHTMODE" = "FORWARDBASE" "QUEUE" = "Overlay+0" "RenderType" = "Overlay" "SHADOWSUPPORT" = "true" }
			Blend One One, One One
			Cull Front
			GpuProgramID 3101
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float2 texcoord3 : TEXCOORD3;
				float3 texcoord1 : TEXCOORD1;
				float3 texcoord2 : TEXCOORD2;
				float3 texcoord4 : TEXCOORD4;
				float4 texcoord6 : TEXCOORD6;
				float4 texcoord7 : TEXCOORD7;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _texcoord_ST;
			// $Globals ConstantBuffers for Fragment Shader
			float4 _Color0;
			float4 _TextureSample0_ST;
			float _Float0;
			float _Float1;
			float4 _Color2;
			float4 _Color1;
			float _Appear;
			float _Fade;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _TextureSample0;

			// Keywords: DIRECTIONAL
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                tmp0 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                tmp1 = tmp0 + unity_ObjectToWorld._m03_m13_m23_m33;
                o.texcoord2.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp0.xyz;
                tmp0 = tmp1.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp0 = unity_MatrixVP._m00_m10_m20_m30 * tmp1.xxxx + tmp0;
                tmp0 = unity_MatrixVP._m02_m12_m22_m32 * tmp1.zzzz + tmp0;
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp1.wwww + tmp0;
                o.texcoord.xy = v.texcoord.xy * _texcoord_ST.xy + _texcoord_ST.zw;
                tmp0.xy = v.texcoord.xy * float2(1.0, 3.14);
                o.texcoord3.x = tmp0.x;
                o.texcoord3.y = _Time.y * 0.5 + tmp0.y;
                tmp0.x = dot(v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp0.y = dot(v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp0.z = dot(v.normal.xyz, unity_WorldToObject._m02_m12_m22);
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                o.texcoord1.xyz = tmp0.www * tmp0.xyz;
                o.texcoord4.xyz = float3(0.0, 0.0, 0.0);
                o.texcoord6 = float4(0.0, 0.0, 0.0, 0.0);
                o.texcoord7 = float4(0.0, 0.0, 0.0, 0.0);
                return o;
			}
			// Keywords: DIRECTIONAL
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                tmp0.xyz = _WorldSpaceCameraPos - inp.texcoord2.xyz;
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                tmp0.x = dot(-inp.texcoord1.xyz, tmp0.xyz);
                tmp0.x = 1.0 - tmp0.x;
                tmp0.y = tmp0.x * tmp0.x;
                tmp0.y = tmp0.y * tmp0.y;
                tmp0.x = tmp0.y * tmp0.x;
                tmp0.xyz = tmp0.xxx * _Color1.xyz;
                tmp1.xy = inp.texcoord.xy * _TextureSample0_ST.xy + _TextureSample0_ST.zw;
                tmp1 = tex2D(_TextureSample0, tmp1.xy);
                tmp0.w = tmp1.x * tmp1.z;
                tmp1.xzw = tmp0.www * _Color0.xyz;
                tmp0.w = sin(inp.texcoord3.y);
                tmp0.w = log(abs(tmp0.w));
                tmp2.xy = tmp0.ww * float2(_Float0.x, _Float1.x);
                tmp2.xy = exp(tmp2.xy);
                tmp0.w = tmp1.y * tmp2.y;
                tmp2.yzw = tmp0.www * _Color2.xyz;
                tmp1.xyz = tmp1.xzw * tmp2.xxx + tmp2.yzw;
                tmp0.w = tmp2.x * 0.5 + 0.5;
                tmp0.xyz = tmp0.xyz * tmp0.www + tmp1.xyz;
                tmp0.w = saturate(_Appear);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                o.sv_target.xyz = tmp0.xyz * _Fade.xxx;
                o.sv_target.w = 1.0;
                return o;
			}
			ENDCG
		}
		Pass {
			Name "FORWARD"
			Tags { "IsEmissive" = "true" "LIGHTMODE" = "FORWARDADD" "QUEUE" = "Overlay+0" "RenderType" = "Overlay" "SHADOWSUPPORT" = "true" }
			Blend One One, One One
			ZWrite Off
			Cull Front
			GpuProgramID 107644
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float3 texcoord : TEXCOORD0;
				float3 texcoord1 : TEXCOORD1;
				float2 texcoord2 : TEXCOORD2;
				float3 texcoord3 : TEXCOORD3;
				float4 texcoord4 : TEXCOORD4;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4x4 unity_WorldToLight;
			// $Globals ConstantBuffers for Fragment Shader
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader

			// Keywords: POINT
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                tmp0 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                tmp1 = tmp0 + unity_ObjectToWorld._m03_m13_m23_m33;
                tmp2 = tmp1.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp2 = unity_MatrixVP._m00_m10_m20_m30 * tmp1.xxxx + tmp2;
                tmp2 = unity_MatrixVP._m02_m12_m22_m32 * tmp1.zzzz + tmp2;
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp1.wwww + tmp2;
                tmp1.x = dot(v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp1.y = dot(v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp1.z = dot(v.normal.xyz, unity_WorldToObject._m02_m12_m22);
                tmp1.w = dot(tmp1.xyz, tmp1.xyz);
                tmp1.w = rsqrt(tmp1.w);
                o.texcoord.xyz = tmp1.www * tmp1.xyz;
                o.texcoord1.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp0.xyz;
                tmp0 = unity_ObjectToWorld._m03_m13_m23_m33 * v.vertex.wwww + tmp0;
                tmp1.xy = v.texcoord.xy * float2(1.0, 3.14);
                o.texcoord2.x = tmp1.x;
                o.texcoord2.y = _Time.y * 0.5 + tmp1.y;
                tmp1.xyz = tmp0.yyy * unity_WorldToLight._m01_m11_m21;
                tmp1.xyz = unity_WorldToLight._m00_m10_m20 * tmp0.xxx + tmp1.xyz;
                tmp0.xyz = unity_WorldToLight._m02_m12_m22 * tmp0.zzz + tmp1.xyz;
                o.texcoord3.xyz = unity_WorldToLight._m03_m13_m23 * tmp0.www + tmp0.xyz;
                o.texcoord4 = float4(0.0, 0.0, 0.0, 0.0);
                return o;
			}
			// Keywords: POINT
			fout frag(v2f inp)
			{
                fout o;
                o.sv_target = float4(0.0, 0.0, 0.0, 1.0);
                return o;
			}
			ENDCG
		}
		Pass {
			Name "Meta"
			Tags { "IsEmissive" = "true" "LIGHTMODE" = "META" "QUEUE" = "Overlay+0" "RenderType" = "Overlay" }
			Blend One One, One One
			Cull Off
			GpuProgramID 179149
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float2 texcoord3 : TEXCOORD3;
				float3 texcoord1 : TEXCOORD1;
				float3 texcoord2 : TEXCOORD2;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _texcoord_ST;
			// $Globals ConstantBuffers for Fragment Shader
			float4 _Color0;
			float4 _TextureSample0_ST;
			float _Float0;
			float _Float1;
			float4 _Color2;
			float4 _Color1;
			float _Appear;
			float _Fade;
			float unity_MaxOutputValue;
			float unity_UseLinearSpace;
			// Custom ConstantBuffers for Vertex Shader
			CBUFFER_START(UnityMetaPass)
				bool4 unity_MetaVertexControl;
			CBUFFER_END
			// Custom ConstantBuffers for Fragment Shader
			CBUFFER_START(UnityMetaPass)
				bool4 unity_MetaFragmentControl;
			CBUFFER_END
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _TextureSample0;

			// Keywords:
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                tmp0.x = v.vertex.z > 0.0;
                tmp0.z = tmp0.x ? 0.0001 : 0.0;
                tmp0.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                tmp0.xyz = unity_MetaVertexControl.xxx ? tmp0.xyz : v.vertex.xyz;
                tmp0.w = tmp0.z > 0.0;
                tmp1.z = tmp0.w ? 0.0001 : 0.0;
                tmp1.xy = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                tmp0.xyz = unity_MetaVertexControl.yyy ? tmp1.xyz : tmp0.xyz;
                tmp1 = tmp0.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp1 = unity_MatrixVP._m00_m10_m20_m30 * tmp0.xxxx + tmp1;
                tmp0 = unity_MatrixVP._m02_m12_m22_m32 * tmp0.zzzz + tmp1;
                o.position = tmp0 + unity_MatrixVP._m03_m13_m23_m33;
                o.texcoord.xy = v.texcoord.xy * _texcoord_ST.xy + _texcoord_ST.zw;
                tmp0.xy = v.texcoord.xy * float2(1.0, 3.14);
                o.texcoord3.x = tmp0.x;
                o.texcoord3.y = _Time.y * 0.5 + tmp0.y;
                tmp0.x = dot(v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp0.y = dot(v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp0.z = dot(v.normal.xyz, unity_WorldToObject._m02_m12_m22);
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                o.texcoord1.xyz = tmp0.www * tmp0.xyz;
                tmp0.xyz = v.vertex.yyy * unity_ObjectToWorld._m01_m11_m21;
                tmp0.xyz = unity_ObjectToWorld._m00_m10_m20 * v.vertex.xxx + tmp0.xyz;
                tmp0.xyz = unity_ObjectToWorld._m02_m12_m22 * v.vertex.zzz + tmp0.xyz;
                o.texcoord2.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp0.xyz;
                return o;
			}
			// Keywords:
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                tmp0.xyz = _WorldSpaceCameraPos - inp.texcoord2.xyz;
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                tmp0.x = dot(-inp.texcoord1.xyz, tmp0.xyz);
                tmp0.x = 1.0 - tmp0.x;
                tmp0.y = tmp0.x * tmp0.x;
                tmp0.y = tmp0.y * tmp0.y;
                tmp0.x = tmp0.y * tmp0.x;
                tmp0.xyz = tmp0.xxx * _Color1.xyz;
                tmp1.xy = inp.texcoord.xy * _TextureSample0_ST.xy + _TextureSample0_ST.zw;
                tmp1 = tex2D(_TextureSample0, tmp1.xy);
                tmp0.w = tmp1.x * tmp1.z;
                tmp1.xzw = tmp0.www * _Color0.xyz;
                tmp0.w = sin(inp.texcoord3.y);
                tmp0.w = log(abs(tmp0.w));
                tmp2.xy = tmp0.ww * float2(_Float0.x, _Float1.x);
                tmp2.xy = exp(tmp2.xy);
                tmp0.w = tmp1.y * tmp2.y;
                tmp2.yzw = tmp0.www * _Color2.xyz;
                tmp1.xyz = tmp1.xzw * tmp2.xxx + tmp2.yzw;
                tmp0.w = tmp2.x * 0.5 + 0.5;
                tmp0.xyz = tmp0.xyz * tmp0.www + tmp1.xyz;
                tmp0.w = saturate(_Appear);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                tmp0.xyz = tmp0.xyz * _Fade.xxx;
                tmp1.xyz = tmp0.xyz * float3(0.305306, 0.305306, 0.305306) + float3(0.6821711, 0.6821711, 0.6821711);
                tmp1.xyz = tmp0.xyz * tmp1.xyz + float3(0.0125229, 0.0125229, 0.0125229);
                tmp1.xyz = tmp0.xyz * tmp1.xyz;
                tmp0.w = unity_UseLinearSpace != 0.0;
                tmp0.xyz = tmp0.www ? tmp0.xyz : tmp1.xyz;
                tmp1.xyz = min(unity_MaxOutputValue.xxx, float3(0.0, 0.0, 0.0));
                tmp1.w = 1.0;
                tmp1 = unity_MetaFragmentControl ? tmp1 : float4(0.0, 0.0, 0.0, 0.0);
                tmp0.w = 1.0;
                o.sv_target = unity_MetaFragmentControl ? tmp0 : tmp1;
                return o;
			}
			ENDCG
		}
		Pass {
			Name "ShadowCaster"
			Tags { "IsEmissive" = "true" "LIGHTMODE" = "SHADOWCASTER" "QUEUE" = "Overlay+0" "RenderType" = "Overlay" "SHADOWSUPPORT" = "true" }
			Blend One One, One One
			Cull Front
			GpuProgramID 248205
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float3 texcoord6 : TEXCOORD6;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 texcoord3 : TEXCOORD3;
				float4 texcoord4 : TEXCOORD4;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			// $Globals ConstantBuffers for Fragment Shader
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader

			// Keywords: SHADOWS_DEPTH UNITY_PASS_SHADOWCASTER
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                tmp0.x = dot(v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp0.y = dot(v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp0.z = dot(v.normal.xyz, unity_WorldToObject._m02_m12_m22);
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                tmp1 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp1 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp1;
                tmp1 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp1;
                tmp1 = unity_ObjectToWorld._m03_m13_m23_m33 * v.vertex.wwww + tmp1;
                tmp2.xyz = -tmp1.xyz * _WorldSpaceLightPos0.www + _WorldSpaceLightPos0.xyz;
                tmp0.w = dot(tmp2.xyz, tmp2.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp2.xyz = tmp0.www * tmp2.xyz;
                tmp0.w = dot(tmp0.xyz, tmp2.xyz);
                tmp0.w = -tmp0.w * tmp0.w + 1.0;
                tmp0.w = sqrt(tmp0.w);
                tmp0.w = tmp0.w * unity_LightShadowBias.z;
                tmp0.xyz = -tmp0.xyz * tmp0.www + tmp1.xyz;
                tmp0.w = unity_LightShadowBias.z != 0.0;
                tmp0.xyz = tmp0.www ? tmp0.xyz : tmp1.xyz;
                tmp2 = tmp0.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp2 = unity_MatrixVP._m00_m10_m20_m30 * tmp0.xxxx + tmp2;
                tmp0 = unity_MatrixVP._m02_m12_m22_m32 * tmp0.zzzz + tmp2;
                tmp0 = unity_MatrixVP._m03_m13_m23_m33 * tmp1.wwww + tmp0;
                tmp1.x = unity_LightShadowBias.x / tmp0.w;
                tmp1.x = min(tmp1.x, 0.0);
                tmp1.x = max(tmp1.x, -1.0);
                tmp0.z = tmp0.z + tmp1.x;
                tmp1.x = min(tmp0.w, tmp0.z);
                o.position.xyw = tmp0.xyw;
                tmp0.x = tmp1.x - tmp0.z;
                o.position.z = unity_LightShadowBias.y * tmp0.x + tmp0.z;
                tmp0.xyz = v.vertex.yyy * unity_ObjectToWorld._m01_m11_m21;
                tmp0.xyz = unity_ObjectToWorld._m00_m10_m20 * v.vertex.xxx + tmp0.xyz;
                tmp0.xyz = unity_ObjectToWorld._m02_m12_m22 * v.vertex.zzz + tmp0.xyz;
                o.texcoord6.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp0.xyz;
                o.texcoord1 = float4(0.0, 0.0, 0.0, 0.0);
                o.texcoord2 = float4(0.0, 0.0, 0.0, 0.0);
                o.texcoord3 = float4(0.0, 0.0, 0.0, 0.0);
                o.texcoord4.xy = v.texcoord.xy;
                o.texcoord4.zw = v.texcoord1.xy;
                return o;
			}
			// Keywords: SHADOWS_DEPTH UNITY_PASS_SHADOWCASTER
			fout frag(v2f inp)
			{
                fout o;
                o.sv_target = float4(0.0, 0.0, 0.0, 0.0);
                return o;
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
