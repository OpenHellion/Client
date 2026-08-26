Shader "ZeroG/sunShader" {
	Properties {
		_Albedo ("Albedo", 2D) = "white" {}
		_ColorTint ("Color Tint", Vector) = (1,1,1,1)
		_NoiseTileScale ("Noise Tile/Scale", Range(0, 50)) = 2.555199
		_FresnelColor ("Fresnel Color", Vector) = (1,1,1,1)
		_FresnelSpread ("Fresnel Spread", Range(1, 5)) = 2
		_EdgeOpacity ("Edge Opacity", Range(0, 10)) = 10
		[HideInInspector] _Cutoff ("Alpha cutoff", Range(0, 1)) = 0.5
	}
	SubShader {
		Tags { "QUEUE" = "AlphaTest" "RenderType" = "TransparentCutout" }
		Pass {
			Name "FORWARD"
			Tags { "LIGHTMODE" = "FORWARDBASE" "QUEUE" = "AlphaTest" "RenderType" = "TransparentCutout" "SHADOWSUPPORT" = "true" }
			GpuProgramID 13181
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float3 texcoord2 : TEXCOORD2;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			// $Globals ConstantBuffers for Fragment Shader
			float _FresnelSpread;
			float _NoiseTileScale;
			float4 _ColorTint;
			float4 _FresnelColor;
			float _EdgeOpacity;
			float4 _Albedo_ST;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _Albedo;

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
                o.texcoord1 = unity_ObjectToWorld._m03_m13_m23_m33 * v.vertex.wwww + tmp0;
                tmp0 = tmp1.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp0 = unity_MatrixVP._m00_m10_m20_m30 * tmp1.xxxx + tmp0;
                tmp0 = unity_MatrixVP._m02_m12_m22_m32 * tmp1.zzzz + tmp0;
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp1.wwww + tmp0;
                o.texcoord.xy = v.texcoord.xy;
                tmp0.x = dot(v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp0.y = dot(v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp0.z = dot(v.normal.xyz, unity_WorldToObject._m02_m12_m22);
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                o.texcoord2.xyz = tmp0.www * tmp0.xyz;
                return o;
			}
			// Keywords: DIRECTIONAL
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                tmp0.x = sin(_NoiseTileScale);
                tmp1.x = cos(_NoiseTileScale);
                tmp2.z = tmp0.x;
                tmp2.y = tmp1.x;
                tmp2.x = -tmp0.x;
                tmp0 = inp.texcoord.xyxy + float4(-0.5, -0.5, 0.0, 1.0);
                tmp1.x = dot(tmp0.xy, tmp2.xy);
                tmp1.y = dot(tmp0.xy, tmp2.xy);
                tmp0.xy = tmp1.xy + float2(0.5, 1.5);
                tmp1.x = 1.0 / _NoiseTileScale;
                tmp1.yz = tmp0.xy * tmp1.xx;
                tmp0.xy = tmp0.xy * tmp1.xx + float2(0.2127, 0.2127);
                tmp1.y = tmp1.z * tmp1.y;
                tmp0.xy = tmp1.yy * float2(0.3713, 0.3713) + tmp0.xy;
                tmp1.yz = tmp0.xy * float2(489.123, 489.123);
                tmp0.x = tmp0.x + 1.0;
                tmp1.yz = sin(tmp1.yz);
                tmp1.yz = tmp1.yz * float2(4.789, 4.789);
                tmp0.y = tmp1.z * tmp1.y;
                tmp0.x = tmp0.x * tmp0.y;
                tmp1.yz = tmp0.zw * tmp1.xx;
                tmp0.yz = tmp0.zw * tmp1.xx + float2(0.2127, 0.2127);
                tmp0.w = tmp1.z * tmp1.y;
                tmp0.yz = tmp0.ww * float2(0.3713, 0.3713) + tmp0.yz;
                tmp0.w = tmp0.y + 1.0;
                tmp0.yz = tmp0.yz * float2(489.123, 489.123);
                tmp0.yz = sin(tmp0.yz);
                tmp0.yz = tmp0.yz * float2(4.789, 4.789);
                tmp0.y = tmp0.z * tmp0.y;
                tmp0.y = tmp0.w * tmp0.y;
                tmp0.xy = frac(tmp0.xy);
                tmp0.x = tmp0.y + tmp0.x;
                tmp0.yzw = _WorldSpaceCameraPos - inp.texcoord1.xyz;
                tmp1.x = dot(tmp0.xyz, tmp0.xyz);
                tmp1.x = rsqrt(tmp1.x);
                tmp0.yzw = tmp0.yzw * tmp1.xxx;
                tmp1.x = dot(inp.texcoord2.xyz, inp.texcoord2.xyz);
                tmp1.x = rsqrt(tmp1.x);
                tmp1.xyz = tmp1.xxx * inp.texcoord2.xyz;
                tmp0.y = dot(tmp1.xyz, tmp0.xyz);
                tmp0.y = max(tmp0.y, 0.0);
                tmp0.y = 1.0 - tmp0.y;
                tmp0.y = log(tmp0.y);
                tmp0.z = tmp0.y * _EdgeOpacity;
                tmp0.y = tmp0.y * _FresnelSpread;
                tmp0.y = exp(tmp0.y);
                tmp1.xyz = -_FresnelColor.xyz * tmp0.yyy + float3(1.0, 1.0, 1.0);
                tmp0.y = exp(tmp0.z);
                tmp0.x = -tmp0.y * tmp0.x + 0.5;
                tmp0.x = tmp0.x < 0.0;
                if (tmp0.x) {
                    discard;
                }
                tmp0.xy = inp.texcoord.xy * _Albedo_ST.xy + _Albedo_ST.zw;
                tmp0 = tex2D(_Albedo, tmp0.xy);
                tmp2.xyz = tmp0.xyz * _ColorTint.xyz;
                tmp0.xyz = -tmp0.xyz * _ColorTint.xyz + float3(1.0, 1.0, 1.0);
                tmp0.xyz = -tmp0.xyz * tmp1.xyz + tmp2.xyz;
                o.sv_target.xyz = tmp0.xyz + float3(1.0, 1.0, 1.0);
                o.sv_target.w = 1.0;
                return o;
			}
			ENDCG
		}
		Pass {
			Name "ShadowCaster"
			Tags { "LIGHTMODE" = "SHADOWCASTER" "QUEUE" = "AlphaTest" "RenderType" = "TransparentCutout" "SHADOWSUPPORT" = "true" }
			Offset 1, 1
			GpuProgramID 116417
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float3 texcoord3 : TEXCOORD3;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			// $Globals ConstantBuffers for Fragment Shader
			float _NoiseTileScale;
			float _EdgeOpacity;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader

			// Keywords: SHADOWS_DEPTH
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                tmp0 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                tmp1 = tmp0 + unity_ObjectToWorld._m03_m13_m23_m33;
                o.texcoord2 = unity_ObjectToWorld._m03_m13_m23_m33 * v.vertex.wwww + tmp0;
                tmp0 = tmp1.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp0 = unity_MatrixVP._m00_m10_m20_m30 * tmp1.xxxx + tmp0;
                tmp0 = unity_MatrixVP._m02_m12_m22_m32 * tmp1.zzzz + tmp0;
                tmp0 = unity_MatrixVP._m03_m13_m23_m33 * tmp1.wwww + tmp0;
                tmp1.x = unity_LightShadowBias.x / tmp0.w;
                tmp1.x = min(tmp1.x, 0.0);
                tmp1.x = max(tmp1.x, -1.0);
                tmp0.z = tmp0.z + tmp1.x;
                tmp1.x = min(tmp0.w, tmp0.z);
                o.position.xyw = tmp0.xyw;
                tmp0.x = tmp1.x - tmp0.z;
                o.position.z = unity_LightShadowBias.y * tmp0.x + tmp0.z;
                o.texcoord1.xy = v.texcoord.xy;
                tmp0.x = dot(v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp0.y = dot(v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp0.z = dot(v.normal.xyz, unity_WorldToObject._m02_m12_m22);
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                o.texcoord3.xyz = tmp0.www * tmp0.xyz;
                return o;
			}
			// Keywords: SHADOWS_DEPTH
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                tmp0.x = sin(_NoiseTileScale);
                tmp1.x = cos(_NoiseTileScale);
                tmp2.z = tmp0.x;
                tmp2.y = tmp1.x;
                tmp2.x = -tmp0.x;
                tmp0 = inp.texcoord1.xyxy + float4(-0.5, -0.5, 0.0, 1.0);
                tmp1.x = dot(tmp0.xy, tmp2.xy);
                tmp1.y = dot(tmp0.xy, tmp2.xy);
                tmp0.xy = tmp1.xy + float2(0.5, 1.5);
                tmp1.x = 1.0 / _NoiseTileScale;
                tmp1.yz = tmp0.xy * tmp1.xx;
                tmp0.xy = tmp0.xy * tmp1.xx + float2(0.2127, 0.2127);
                tmp1.y = tmp1.z * tmp1.y;
                tmp0.xy = tmp1.yy * float2(0.3713, 0.3713) + tmp0.xy;
                tmp1.yz = tmp0.xy * float2(489.123, 489.123);
                tmp0.x = tmp0.x + 1.0;
                tmp1.yz = sin(tmp1.yz);
                tmp1.yz = tmp1.yz * float2(4.789, 4.789);
                tmp0.y = tmp1.z * tmp1.y;
                tmp0.x = tmp0.x * tmp0.y;
                tmp1.yz = tmp0.zw * tmp1.xx;
                tmp0.yz = tmp0.zw * tmp1.xx + float2(0.2127, 0.2127);
                tmp0.w = tmp1.z * tmp1.y;
                tmp0.yz = tmp0.ww * float2(0.3713, 0.3713) + tmp0.yz;
                tmp0.w = tmp0.y + 1.0;
                tmp0.yz = tmp0.yz * float2(489.123, 489.123);
                tmp0.yz = sin(tmp0.yz);
                tmp0.yz = tmp0.yz * float2(4.789, 4.789);
                tmp0.y = tmp0.z * tmp0.y;
                tmp0.y = tmp0.w * tmp0.y;
                tmp0.xy = frac(tmp0.xy);
                tmp0.x = tmp0.y + tmp0.x;
                tmp0.yzw = _WorldSpaceCameraPos - inp.texcoord2.xyz;
                tmp1.x = dot(tmp0.xyz, tmp0.xyz);
                tmp1.x = rsqrt(tmp1.x);
                tmp0.yzw = tmp0.yzw * tmp1.xxx;
                tmp1.x = dot(inp.texcoord3.xyz, inp.texcoord3.xyz);
                tmp1.x = rsqrt(tmp1.x);
                tmp1.xyz = tmp1.xxx * inp.texcoord3.xyz;
                tmp0.y = dot(tmp1.xyz, tmp0.xyz);
                tmp0.y = max(tmp0.y, 0.0);
                tmp0.y = 1.0 - tmp0.y;
                tmp0.y = log(tmp0.y);
                tmp0.y = tmp0.y * _EdgeOpacity;
                tmp0.y = exp(tmp0.y);
                tmp0.x = -tmp0.y * tmp0.x + 0.5;
                tmp0.x = tmp0.x < 0.0;
                if (tmp0.x) {
                    discard;
                }
                o.sv_target = float4(0.0, 0.0, 0.0, 0.0);
                return o;
			}
			ENDCG
		}
		Pass {
			Name "Meta"
			Tags { "LIGHTMODE" = "META" "QUEUE" = "AlphaTest" "RenderType" = "TransparentCutout" "SHADOWSUPPORT" = "true" }
			Cull Off
			GpuProgramID 156495
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
			float unity_MaxOutputValue;
			float unity_UseLinearSpace;
			float4 _ColorTint;
			float4 _Albedo_ST;
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
			sampler2D _Albedo;

			// Keywords: SHADOWS_DEPTH
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
                o.texcoord.xy = v.texcoord.xy;
                return o;
			}
			// Keywords: SHADOWS_DEPTH
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                tmp0.xy = inp.texcoord.xy * _Albedo_ST.xy + _Albedo_ST.zw;
                tmp0 = tex2D(_Albedo, tmp0.xy);
                tmp0.xyz = tmp0.xyz * _ColorTint.xyz;
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
	}
	Fallback "Diffuse"
}
