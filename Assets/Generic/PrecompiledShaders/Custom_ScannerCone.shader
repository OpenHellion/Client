Shader "Custom/ScannerCone" {
	Properties {
		_ScannerTexture ("ScannerTexture", 2D) = "white" {}
		_ScannerTextureTiling ("ScannerTextureTiling", Float) = 1
		_ColorStrength ("ColorStrength", Range(0, 5)) = 0
		[HDR] _ScannerColor ("ScannerColor", Vector) = (0,0.4726329,1,0)
		[Toggle(_SCANNERPANNINGONOFF_ON)] _ScannerPanningONOFF ("ScannerPanning ON/OFF", Float) = 0
		_ScannerPanningSpeed ("ScannerPanningSpeed", Float) = 1
		_GradientMask ("GradientMask", 2D) = "white" {}
		_MaskTile ("MaskTile", Float) = 0.84
		_MaskOffset ("MaskOffset", Float) = -0.66
		[HideInInspector] _texcoord ("", 2D) = "white" {}
		[HideInInspector] __dirty ("", Float) = 1
	}
	SubShader {
		Tags { "IGNOREPROJECTOR" = "true" "IsEmissive" = "true" "QUEUE" = "Transparent+0" "RenderType" = "Transparent" }
		Pass {
			Name "FORWARD"
			Tags { "IGNOREPROJECTOR" = "true" "IsEmissive" = "true" "LIGHTMODE" = "FORWARDBASE" "QUEUE" = "Transparent+0" "RenderType" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha, SrcAlpha OneMinusSrcAlpha
			ColorMask RGB -1
			ZWrite Off
			Cull Off
			GpuProgramID 45660
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float3 texcoord1 : TEXCOORD1;
				float3 texcoord2 : TEXCOORD2;
				float4 texcoord6 : TEXCOORD6;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _texcoord_ST;
			// $Globals ConstantBuffers for Fragment Shader
			float4 _LightColor0;
			float4 _ScannerColor;
			float _ColorStrength;
			float _ScannerTextureTiling;
			float _MaskTile;
			float _MaskOffset;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _ScannerTexture;
			sampler2D _GradientMask;
			
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
                tmp0.x = dot(v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp0.y = dot(v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp0.z = dot(v.normal.xyz, unity_WorldToObject._m02_m12_m22);
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                o.texcoord1.xyz = tmp0.www * tmp0.xyz;
                o.texcoord6 = float4(0.0, 0.0, 0.0, 0.0);
                return o;
			}
			// Keywords: DIRECTIONAL
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                float4 tmp3;
                float4 tmp4;
                float4 tmp5;
                float4 tmp6;
                float4 tmp7;
                float4 tmp8;
                float4 tmp9;
                tmp0.xyz = _WorldSpaceCameraPos - inp.texcoord2.xyz;
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp1.xyz = tmp0.www * tmp0.xyz;
                tmp1.w = inp.texcoord.y * _ScannerTextureTiling;
                tmp2.xy = inp.texcoord.xy * _MaskTile.xx + _MaskOffset.xx;
                tmp3 = tex2D(_ScannerTexture, tmp1.ww);
                tmp2 = tex2D(_GradientMask, tmp2.xy);
                o.sv_target.w = tmp2.x * tmp3.x;
                tmp1.w = unity_ProbeVolumeParams.x == 1.0;
                if (tmp1.w) {
                    tmp1.w = unity_ProbeVolumeParams.y == 1.0;
                    tmp2.xyz = inp.texcoord2.yyy * unity_ProbeVolumeWorldToObject._m01_m11_m21;
                    tmp2.xyz = unity_ProbeVolumeWorldToObject._m00_m10_m20 * inp.texcoord2.xxx + tmp2.xyz;
                    tmp2.xyz = unity_ProbeVolumeWorldToObject._m02_m12_m22 * inp.texcoord2.zzz + tmp2.xyz;
                    tmp2.xyz = tmp2.xyz + unity_ProbeVolumeWorldToObject._m03_m13_m23;
                    tmp2.xyz = tmp1.www ? tmp2.xyz : inp.texcoord2.xyz;
                    tmp2.xyz = tmp2.xyz - unity_ProbeVolumeMin;
                    tmp2.yzw = tmp2.xyz * unity_ProbeVolumeSizeInv;
                    tmp1.w = tmp2.y * 0.25 + 0.75;
                    tmp2.y = unity_ProbeVolumeParams.z * 0.5 + 0.75;
                    tmp2.x = max(tmp1.w, tmp2.y);
                    tmp2 = UNITY_SAMPLE_TEX3D_SAMPLER(unity_ProbeVolumeSH, unity_ProbeVolumeSH, tmp2.xzw);
                } else {
                    tmp2 = float4(1.0, 1.0, 1.0, 1.0);
                }
                tmp1.w = saturate(dot(tmp2, unity_OcclusionMaskSelector));
                tmp2.x = dot(-tmp1.xyz, inp.texcoord1.xyz);
                tmp2.x = tmp2.x + tmp2.x;
                tmp2.xyz = inp.texcoord1.xyz * -tmp2.xxx + -tmp1.xyz;
                tmp3.xyz = tmp1.www * _LightColor0.xyz;
                tmp1.w = unity_SpecCube0_ProbePosition.w > 0.0;
                if (tmp1.w) {
                    tmp1.w = dot(tmp2.xyz, tmp2.xyz);
                    tmp1.w = rsqrt(tmp1.w);
                    tmp4.xyz = tmp1.www * tmp2.xyz;
                    tmp5.xyz = unity_SpecCube0_BoxMax.xyz - inp.texcoord2.xyz;
                    tmp5.xyz = tmp5.xyz / tmp4.xyz;
                    tmp6.xyz = unity_SpecCube0_BoxMin.xyz - inp.texcoord2.xyz;
                    tmp6.xyz = tmp6.xyz / tmp4.xyz;
                    tmp7.xyz = tmp4.xyz > float3(0.0, 0.0, 0.0);
                    tmp5.xyz = tmp7.xyz ? tmp5.xyz : tmp6.xyz;
                    tmp1.w = min(tmp5.y, tmp5.x);
                    tmp1.w = min(tmp5.z, tmp1.w);
                    tmp5.xyz = inp.texcoord2.xyz - unity_SpecCube0_ProbePosition.xyz;
                    tmp4.xyz = tmp4.xyz * tmp1.www + tmp5.xyz;
                } else {
                    tmp4.xyz = tmp2.xyz;
                }
                tmp4 = UNITY_SAMPLE_TEXCUBE_SAMPLER(unity_SpecCube0, unity_SpecCube0, float4(tmp4.xyz, 6.0));
                tmp1.w = tmp4.w - 1.0;
                tmp1.w = unity_SpecCube0_HDR.w * tmp1.w + 1.0;
                tmp1.w = log(tmp1.w);
                tmp1.w = tmp1.w * unity_SpecCube0_HDR.y;
                tmp1.w = exp(tmp1.w);
                tmp1.w = tmp1.w * unity_SpecCube0_HDR.x;
                tmp5.xyz = tmp4.xyz * tmp1.www;
                tmp2.w = unity_SpecCube0_BoxMin.w < 0.99999;
                if (tmp2.w) {
                    tmp2.w = unity_SpecCube1_ProbePosition.w > 0.0;
                    if (tmp2.w) {
                        tmp2.w = dot(tmp2.xyz, tmp2.xyz);
                        tmp2.w = rsqrt(tmp2.w);
                        tmp6.xyz = tmp2.www * tmp2.xyz;
                        tmp7.xyz = unity_SpecCube1_BoxMax.xyz - inp.texcoord2.xyz;
                        tmp7.xyz = tmp7.xyz / tmp6.xyz;
                        tmp8.xyz = unity_SpecCube1_BoxMin.xyz - inp.texcoord2.xyz;
                        tmp8.xyz = tmp8.xyz / tmp6.xyz;
                        tmp9.xyz = tmp6.xyz > float3(0.0, 0.0, 0.0);
                        tmp7.xyz = tmp9.xyz ? tmp7.xyz : tmp8.xyz;
                        tmp2.w = min(tmp7.y, tmp7.x);
                        tmp2.w = min(tmp7.z, tmp2.w);
                        tmp7.xyz = inp.texcoord2.xyz - unity_SpecCube1_ProbePosition.xyz;
                        tmp2.xyz = tmp6.xyz * tmp2.www + tmp7.xyz;
                    }
                    tmp2 = UNITY_SAMPLE_TEXCUBE_SAMPLER(unity_SpecCube0, unity_SpecCube0, float4(tmp2.xyz, 6.0));
                    tmp2.w = tmp2.w - 1.0;
                    tmp2.w = unity_SpecCube1_HDR.w * tmp2.w + 1.0;
                    tmp2.w = log(tmp2.w);
                    tmp2.w = tmp2.w * unity_SpecCube1_HDR.y;
                    tmp2.w = exp(tmp2.w);
                    tmp2.w = tmp2.w * unity_SpecCube1_HDR.x;
                    tmp2.xyz = tmp2.xyz * tmp2.www;
                    tmp4.xyz = tmp1.www * tmp4.xyz + -tmp2.xyz;
                    tmp5.xyz = unity_SpecCube0_BoxMin.www * tmp4.xyz + tmp2.xyz;
                }
                tmp1.w = dot(inp.texcoord1.xyz, inp.texcoord1.xyz);
                tmp1.w = rsqrt(tmp1.w);
                tmp2.xyz = tmp1.www * inp.texcoord1.xyz;
                tmp0.xyz = tmp0.xyz * tmp0.www + _WorldSpaceLightPos0.xyz;
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = max(tmp0.w, 0.001);
                tmp0.w = rsqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                tmp0.w = dot(tmp2.xyz, tmp1.xyz);
                tmp1.x = saturate(dot(tmp2.xyz, _WorldSpaceLightPos0.xyz));
                tmp0.x = saturate(dot(_WorldSpaceLightPos0.xyz, tmp0.xyz));
                tmp0.y = abs(tmp0.w) + tmp1.x;
                tmp0.y = tmp0.y + 0.00001;
                tmp0.y = 0.5 / tmp0.y;
                tmp0.y = tmp1.x * tmp0.y;
                tmp0.y = tmp0.y * 0.9999999;
                tmp1.xyz = tmp3.xyz * tmp0.yyy;
                tmp0.x = 1.0 - tmp0.x;
                tmp0.y = tmp0.x * tmp0.x;
                tmp0.y = tmp0.y * tmp0.y;
                tmp0.x = tmp0.x * tmp0.y;
                tmp0.x = tmp0.x * 0.96 + 0.04;
                tmp2.xyz = tmp5.xyz * float3(0.5, 0.5, 0.5);
                tmp0.y = 1.0 - abs(tmp0.w);
                tmp0.z = tmp0.y * tmp0.y;
                tmp0.z = tmp0.z * tmp0.z;
                tmp0.y = tmp0.y * tmp0.z;
                tmp0.y = tmp0.y * 0.0 + 0.04;
                tmp0.yzw = tmp0.yyy * tmp2.xyz;
                tmp0.xyz = tmp1.xyz * tmp0.xxx + tmp0.yzw;
                o.sv_target.xyz = _ScannerColor.xyz * _ColorStrength.xxx + tmp0.xyz;
                return o;
			}
			ENDCG
		}
		Pass {
			Name "FORWARD"
			Tags { "IGNOREPROJECTOR" = "true" "IsEmissive" = "true" "LIGHTMODE" = "FORWARDADD" "QUEUE" = "Transparent+0" "RenderType" = "Transparent" }
			Blend SrcAlpha One, SrcAlpha One
			ColorMask RGB -1
			ZWrite Off
			Cull Off
			GpuProgramID 75814
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float3 texcoord1 : TEXCOORD1;
				float3 texcoord2 : TEXCOORD2;
				float3 texcoord3 : TEXCOORD3;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4x4 unity_WorldToLight;
			float4 _texcoord_ST;
			// $Globals ConstantBuffers for Fragment Shader
			float4 _LightColor0;
			float _ScannerTextureTiling;
			float _MaskTile;
			float _MaskOffset;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _ScannerTexture;
			sampler2D _GradientMask;
			sampler2D _LightTexture0;
			
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
                o.texcoord.xy = v.texcoord.xy * _texcoord_ST.xy + _texcoord_ST.zw;
                tmp1.x = dot(v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp1.y = dot(v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp1.z = dot(v.normal.xyz, unity_WorldToObject._m02_m12_m22);
                tmp1.w = dot(tmp1.xyz, tmp1.xyz);
                tmp1.w = rsqrt(tmp1.w);
                o.texcoord1.xyz = tmp1.www * tmp1.xyz;
                o.texcoord2.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp0.xyz;
                tmp0 = unity_ObjectToWorld._m03_m13_m23_m33 * v.vertex.wwww + tmp0;
                tmp1.xyz = tmp0.yyy * unity_WorldToLight._m01_m11_m21;
                tmp1.xyz = unity_WorldToLight._m00_m10_m20 * tmp0.xxx + tmp1.xyz;
                tmp0.xyz = unity_WorldToLight._m02_m12_m22 * tmp0.zzz + tmp1.xyz;
                o.texcoord3.xyz = unity_WorldToLight._m03_m13_m23 * tmp0.www + tmp0.xyz;
                return o;
			}
			// Keywords: POINT
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                float4 tmp3;
                float4 tmp4;
                tmp0.xyz = _WorldSpaceLightPos0.xyz - inp.texcoord2.xyz;
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp1.xyz = tmp0.www * tmp0.xyz;
                tmp2.xyz = _WorldSpaceCameraPos - inp.texcoord2.xyz;
                tmp1.w = dot(tmp2.xyz, tmp2.xyz);
                tmp1.w = rsqrt(tmp1.w);
                tmp2.xyz = tmp1.www * tmp2.xyz;
                tmp1.w = inp.texcoord.y * _ScannerTextureTiling;
                tmp3.xy = inp.texcoord.xy * _MaskTile.xx + _MaskOffset.xx;
                tmp4 = tex2D(_ScannerTexture, tmp1.ww);
                tmp3 = tex2D(_GradientMask, tmp3.xy);
                o.sv_target.w = tmp3.x * tmp4.x;
                tmp3.xyz = inp.texcoord2.yyy * unity_WorldToLight._m01_m11_m21;
                tmp3.xyz = unity_WorldToLight._m00_m10_m20 * inp.texcoord2.xxx + tmp3.xyz;
                tmp3.xyz = unity_WorldToLight._m02_m12_m22 * inp.texcoord2.zzz + tmp3.xyz;
                tmp3.xyz = tmp3.xyz + unity_WorldToLight._m03_m13_m23;
                tmp1.w = unity_ProbeVolumeParams.x == 1.0;
                if (tmp1.w) {
                    tmp1.w = unity_ProbeVolumeParams.y == 1.0;
                    tmp4.xyz = inp.texcoord2.yyy * unity_ProbeVolumeWorldToObject._m01_m11_m21;
                    tmp4.xyz = unity_ProbeVolumeWorldToObject._m00_m10_m20 * inp.texcoord2.xxx + tmp4.xyz;
                    tmp4.xyz = unity_ProbeVolumeWorldToObject._m02_m12_m22 * inp.texcoord2.zzz + tmp4.xyz;
                    tmp4.xyz = tmp4.xyz + unity_ProbeVolumeWorldToObject._m03_m13_m23;
                    tmp4.xyz = tmp1.www ? tmp4.xyz : inp.texcoord2.xyz;
                    tmp4.xyz = tmp4.xyz - unity_ProbeVolumeMin;
                    tmp4.yzw = tmp4.xyz * unity_ProbeVolumeSizeInv;
                    tmp1.w = tmp4.y * 0.25 + 0.75;
                    tmp2.w = unity_ProbeVolumeParams.z * 0.5 + 0.75;
                    tmp4.x = max(tmp1.w, tmp2.w);
                    tmp4 = UNITY_SAMPLE_TEX3D_SAMPLER(unity_ProbeVolumeSH, unity_ProbeVolumeSH, tmp4.xzw);
                } else {
                    tmp4 = float4(1.0, 1.0, 1.0, 1.0);
                }
                tmp1.w = saturate(dot(tmp4, unity_OcclusionMaskSelector));
                tmp2.w = dot(tmp3.xyz, tmp3.xyz);
                tmp3 = tex2D(_LightTexture0, tmp2.ww);
                tmp1.w = tmp1.w * tmp3.x;
                tmp3.xyz = tmp1.www * _LightColor0.xyz;
                tmp1.w = dot(inp.texcoord1.xyz, inp.texcoord1.xyz);
                tmp1.w = rsqrt(tmp1.w);
                tmp4.xyz = tmp1.www * inp.texcoord1.xyz;
                tmp0.xyz = tmp0.xyz * tmp0.www + tmp2.xyz;
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = max(tmp0.w, 0.001);
                tmp0.w = rsqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                tmp0.w = dot(tmp4.xyz, tmp2.xyz);
                tmp1.w = saturate(dot(tmp4.xyz, tmp1.xyz));
                tmp0.x = saturate(dot(tmp1.xyz, tmp0.xyz));
                tmp0.y = abs(tmp0.w) + tmp1.w;
                tmp0.y = tmp0.y + 0.00001;
                tmp0.y = 0.5 / tmp0.y;
                tmp0.y = tmp1.w * tmp0.y;
                tmp0.y = tmp0.y * 0.9999999;
                tmp0.yzw = tmp3.xyz * tmp0.yyy;
                tmp0.x = 1.0 - tmp0.x;
                tmp1.x = tmp0.x * tmp0.x;
                tmp1.x = tmp1.x * tmp1.x;
                tmp0.x = tmp0.x * tmp1.x;
                tmp0.x = tmp0.x * 0.96 + 0.04;
                o.sv_target.xyz = tmp0.xxx * tmp0.yzw;
                return o;
			}
			ENDCG
		}
		Pass {
			Name "Meta"
			Tags { "IGNOREPROJECTOR" = "true" "IsEmissive" = "true" "LIGHTMODE" = "META" "QUEUE" = "Transparent+0" "RenderType" = "Transparent" }
			ColorMask RGB -1
			ZWrite Off
			Cull Off
			GpuProgramID 137370
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float3 texcoord : TEXCOORD0;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			// $Globals ConstantBuffers for Fragment Shader
			float4 _ScannerColor;
			float _ColorStrength;
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
                tmp0.xyz = v.vertex.yyy * unity_ObjectToWorld._m01_m11_m21;
                tmp0.xyz = unity_ObjectToWorld._m00_m10_m20 * v.vertex.xxx + tmp0.xyz;
                tmp0.xyz = unity_ObjectToWorld._m02_m12_m22 * v.vertex.zzz + tmp0.xyz;
                o.texcoord.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp0.xyz;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                tmp0.xyz = _ScannerColor.xyz * _ColorStrength.xxx;
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
			Tags { "IGNOREPROJECTOR" = "true" "IsEmissive" = "true" "LIGHTMODE" = "SHADOWCASTER" "QUEUE" = "Transparent+0" "RenderType" = "Transparent" "SHADOWSUPPORT" = "true" }
			ColorMask RGB -1
			Cull Off
			GpuProgramID 224178
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord1 : TEXCOORD1;
				float3 texcoord2 : TEXCOORD2;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			// $Globals ConstantBuffers for Fragment Shader
			float _ScannerTextureTiling;
			float _MaskTile;
			float _MaskOffset;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _ScannerTexture;
			sampler2D _GradientMask;
			sampler3D _DitherMaskLOD;
			
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
                o.texcoord1.xy = v.texcoord.xy;
                tmp0.xyz = v.vertex.yyy * unity_ObjectToWorld._m01_m11_m21;
                tmp0.xyz = unity_ObjectToWorld._m00_m10_m20 * v.vertex.xxx + tmp0.xyz;
                tmp0.xyz = unity_ObjectToWorld._m02_m12_m22 * v.vertex.zzz + tmp0.xyz;
                o.texcoord2.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp0.xyz;
                return o;
			}
			// Keywords: SHADOWS_DEPTH UNITY_PASS_SHADOWCASTER
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                tmp0.x = inp.texcoord1.y * _ScannerTextureTiling;
                tmp0 = tex2D(_ScannerTexture, tmp0.xx);
                tmp0.yz = inp.texcoord1.xy * _MaskTile.xx + _MaskOffset.xx;
                tmp1 = tex2D(_GradientMask, tmp0.yz);
                tmp0.x = tmp0.x * tmp1.x;
                tmp0.z = tmp0.x * 0.9375;
                tmp0.xy = inp.position.xy * float2(0.25, 0.25);
                tmp0 = tex3D(_DitherMaskLOD, tmp0.xyz);
                tmp0.x = tmp0.w - 0.01;
                tmp0.x = tmp0.x < 0.0;
                if (tmp0.x) {
                    discard;
                }
                o.sv_target = float4(0.0, 0.0, 0.0, 0.0);
                return o;
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
