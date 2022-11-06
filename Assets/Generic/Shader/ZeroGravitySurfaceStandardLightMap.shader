Shader "ZeroGravity/Surface/StandardLightMap" {
	Properties {
		[HideInInspector] __dirty ("", Float) = 1
		_MainTex ("MainTex", 2D) = "white" {}
		_Color ("Color", Vector) = (0,0,0,0)
		_BumpMap ("BumpMap", 2D) = "bump" {}
		_BumpScale ("BumpScale", Float) = 0
		_EmissionMap ("EmissionMap", 2D) = "white" {}
		[HDR] _EmissionColor ("EmissionColor", Vector) = (0,0,0,0)
		_MetallicGlossMap ("MetallicGlossMap", 2D) = "white" {}
		_GlossMapScale ("GlossMapScale", Range(0, 1)) = 0
		_OcclusionMap ("OcclusionMap", 2D) = "white" {}
		_Lightmap ("Lightmap", 2D) = "black" {}
		_EmissionControl ("EmissionControl", Range(0, 1)) = 0
		[Toggle] _AlwaysEmit ("AlwaysEmit", Range(0, 1)) = 1
		[IntRange] _EmissionState ("EmissionState", Range(0, 3)) = 0
		[HDR] _ToxicColor ("ToxicColor", Vector) = (2,1.034483,0,0)
		[HDR] _LowPressureColor ("LowPressureColor", Vector) = (2,0,0,0)
		[HideInInspector] _texcoord ("", 2D) = "white" {}
		[HideInInspector] _texcoord4 ("", 2D) = "white" {}
	}
	SubShader {
		Tags { "IsEmissive" = "true" "QUEUE" = "Geometry+0" "RenderType" = "Opaque" }
		Pass {
			Name "FORWARD"
			Tags { "IsEmissive" = "true" "LIGHTMODE" = "FORWARDBASE" "QUEUE" = "Geometry+0" "RenderType" = "Opaque" "SHADOWSUPPORT" = "true" }
			GpuProgramID 6765
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 texcoord3 : TEXCOORD3;
				float4 texcoord6 : TEXCOORD6;
				float4 texcoord7 : TEXCOORD7;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _texcoord_ST;
			float4 _texcoord4_ST;
			// $Globals ConstantBuffers for Fragment Shader
			float4 _LightColor0;
			float _BumpScale;
			float4 _MainTex_ST;
			float4 _Color;
			float _EmissionState;
			float4 _LowPressureColor;
			float4 _ToxicColor;
			float4 _EmissionColor;
			float _AlwaysEmit;
			float _EmissionControl;
			float4 _Lightmap_ST;
			float _GlossMapScale;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _BumpMap;
			sampler2D _MainTex;
			sampler2D _MetallicGlossMap;
			sampler2D _OcclusionMap;
			sampler2D _EmissionMap;
			sampler2D _Lightmap;

			// Keywords: DIRECTIONAL
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                float4 tmp3;
                tmp0 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                tmp1 = tmp0 + unity_ObjectToWorld._m03_m13_m23_m33;
                tmp0.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp0.xyz;
                tmp2 = tmp1.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp2 = unity_MatrixVP._m00_m10_m20_m30 * tmp1.xxxx + tmp2;
                tmp2 = unity_MatrixVP._m02_m12_m22_m32 * tmp1.zzzz + tmp2;
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp1.wwww + tmp2;
                o.texcoord.xy = v.texcoord.xy * _texcoord_ST.xy + _texcoord_ST.zw;
                o.texcoord.zw = v.texcoord3.xy * _texcoord4_ST.xy + _texcoord4_ST.zw;
                o.texcoord1.w = tmp0.x;
                tmp1.y = dot(v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp1.z = dot(v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp1.x = dot(v.normal.xyz, unity_WorldToObject._m02_m12_m22);
                tmp0.x = dot(tmp1.xyz, tmp1.xyz);
                tmp0.x = rsqrt(tmp0.x);
                tmp1.xyz = tmp0.xxx * tmp1.xyz;
                tmp2.xyz = v.tangent.yyy * unity_ObjectToWorld._m11_m21_m01;
                tmp2.xyz = unity_ObjectToWorld._m10_m20_m00 * v.tangent.xxx + tmp2.xyz;
                tmp2.xyz = unity_ObjectToWorld._m12_m22_m02 * v.tangent.zzz + tmp2.xyz;
                tmp0.x = dot(tmp2.xyz, tmp2.xyz);
                tmp0.x = rsqrt(tmp0.x);
                tmp2.xyz = tmp0.xxx * tmp2.xyz;
                tmp3.xyz = tmp1.xyz * tmp2.xyz;
                tmp3.xyz = tmp1.zxy * tmp2.yzx + -tmp3.xyz;
                tmp0.x = v.tangent.w * unity_WorldTransformParams.w;
                tmp3.xyz = tmp0.xxx * tmp3.xyz;
                o.texcoord1.y = tmp3.x;
                o.texcoord1.x = tmp2.z;
                o.texcoord1.z = tmp1.y;
                o.texcoord2.x = tmp2.x;
                o.texcoord3.x = tmp2.y;
                o.texcoord2.z = tmp1.z;
                o.texcoord3.z = tmp1.x;
                o.texcoord2.w = tmp0.y;
                o.texcoord3.w = tmp0.z;
                o.texcoord2.y = tmp3.y;
                o.texcoord3.y = tmp3.z;
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
                float4 tmp3;
                float4 tmp4;
                float4 tmp5;
                float4 tmp6;
                float4 tmp7;
                float4 tmp8;
                float4 tmp9;
                float4 tmp10;
                float4 tmp11;
                float4 tmp12;
                float4 tmp13;
                float4 tmp14;
                float4 tmp15;
                tmp0.x = inp.texcoord1.w;
                tmp0.y = inp.texcoord2.w;
                tmp0.z = inp.texcoord3.w;
                tmp1.xyz = _WorldSpaceCameraPos - tmp0.xyz;
                tmp0.w = dot(tmp1.xyz, tmp1.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp2.xyz = tmp0.www * tmp1.xyz;
                tmp3.xy = inp.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                tmp4 = tex2D(_BumpMap, tmp3.xy);
                tmp4.x = tmp4.w * tmp4.x;
                tmp3.zw = tmp4.xy * float2(2.0, 2.0) + float2(-1.0, -1.0);
                tmp4.xy = tmp3.zw * _BumpScale.xx;
                tmp1.w = dot(tmp4.xy, tmp4.xy);
                tmp1.w = min(tmp1.w, 1.0);
                tmp1.w = 1.0 - tmp1.w;
                tmp4.z = sqrt(tmp1.w);
                tmp5 = tex2D(_MainTex, tmp3.xy);
                tmp6.xyz = tmp5.xyz * _Color.xyz;
                tmp7.xyz = _EmissionState.xxx < float3(3.0, 2.0, 1.0);
                tmp8.xyz = tmp7.xxx ? _LowPressureColor.xyz : 0.0;
                tmp3.zw = _EmissionState.xx > float2(2.0, 1.0);
                tmp7.xw = _EmissionState.xx == float2(2.0, 1.0);
                tmp9.xyz = tmp7.yyy ? _ToxicColor.xyz : 0.0;
                tmp3.zw = uint2(tmp3.zw) | uint2(tmp7.xw);
                tmp7.xyw = tmp3.zzz ? tmp8.xyz : tmp9.xyz;
                tmp8.xyz = tmp7.zzz ? _EmissionColor.xyz : 0.0;
                tmp7.xyz = tmp3.www ? tmp7.xyw : tmp8.xyz;
                tmp1.w = _AlwaysEmit > 0.5;
                tmp2.w = _AlwaysEmit == 0.5;
                tmp3.z = _AlwaysEmit < 0.5;
                tmp3.z = tmp3.z ? _EmissionControl : 0.0;
                tmp1.w = uint1(tmp1.w) | uint1(tmp2.w);
                tmp1.w = tmp1.w ? 1.0 : tmp3.z;
                tmp3.zw = inp.texcoord.zw * _Lightmap_ST.xy + _Lightmap_ST.zw;
                tmp8 = tex2D(_MetallicGlossMap, tmp3.xy);
                tmp9 = tex2D(_OcclusionMap, tmp3.xy);
                tmp2.w = 1.0 - tmp9.x;
                tmp2.w = 1.0 - tmp2.w;
                tmp9 = tex2D(_EmissionMap, tmp3.xy);
                tmp7.xyz = tmp7.xyz * tmp9.xyz;
                tmp3 = tex2D(_Lightmap, tmp3.zw);
                tmp3.w = 1.0 - tmp8.x;
                tmp3.xyz = tmp3.www * tmp3.xyz;
                tmp3.xyz = tmp6.xyz * tmp3.xyz;
                tmp3.xyz = tmp2.www * tmp3.xyz;
                tmp3.xyz = tmp7.xyz * tmp1.www + tmp3.xyz;
                tmp1.w = unity_ProbeVolumeParams.x == 1.0;
                if (tmp1.w) {
                    tmp1.w = unity_ProbeVolumeParams.y == 1.0;
                    tmp7.xyz = inp.texcoord2.www * unity_ProbeVolumeWorldToObject._m01_m11_m21;
                    tmp7.xyz = unity_ProbeVolumeWorldToObject._m00_m10_m20 * inp.texcoord1.www + tmp7.xyz;
                    tmp7.xyz = unity_ProbeVolumeWorldToObject._m02_m12_m22 * inp.texcoord3.www + tmp7.xyz;
                    tmp7.xyz = tmp7.xyz + unity_ProbeVolumeWorldToObject._m03_m13_m23;
                    tmp7.xyz = tmp1.www ? tmp7.xyz : tmp0.xyz;
                    tmp7.xyz = tmp7.xyz - unity_ProbeVolumeMin;
                    tmp7.yzw = tmp7.xyz * unity_ProbeVolumeSizeInv;
                    tmp1.w = tmp7.y * 0.25 + 0.75;
                    tmp3.w = unity_ProbeVolumeParams.z * 0.5 + 0.75;
                    tmp7.x = max(tmp1.w, tmp3.w);
                    tmp7 = UNITY_SAMPLE_TEX3D_SAMPLER(unity_ProbeVolumeSH, unity_ProbeVolumeSH, tmp7.xzw);
                } else {
                    tmp7 = float4(1.0, 1.0, 1.0, 1.0);
                }
                tmp1.w = saturate(dot(tmp7, unity_OcclusionMaskSelector));
                tmp7.x = dot(inp.texcoord1.xyz, tmp4.xyz);
                tmp7.y = dot(inp.texcoord2.xyz, tmp4.xyz);
                tmp7.z = dot(inp.texcoord3.xyz, tmp4.xyz);
                tmp3.w = dot(tmp7.xyz, tmp7.xyz);
                tmp3.w = rsqrt(tmp3.w);
                tmp4.xyz = tmp3.www * tmp7.xyz;
                tmp3.w = -tmp8.w * _GlossMapScale + 1.0;
                tmp4.w = dot(-tmp2.xyz, tmp4.xyz);
                tmp4.w = tmp4.w + tmp4.w;
                tmp7.xyz = tmp4.xyz * -tmp4.www + -tmp2.xyz;
                tmp9.xyz = tmp1.www * _LightColor0.xyz;
                tmp1.w = unity_SpecCube0_ProbePosition.w > 0.0;
                if (tmp1.w) {
                    tmp1.w = dot(tmp7.xyz, tmp7.xyz);
                    tmp1.w = rsqrt(tmp1.w);
                    tmp10.xyz = tmp1.www * tmp7.xyz;
                    tmp11.xyz = unity_SpecCube0_BoxMax.xyz - tmp0.xyz;
                    tmp11.xyz = tmp11.xyz / tmp10.xyz;
                    tmp12.xyz = unity_SpecCube0_BoxMin.xyz - tmp0.xyz;
                    tmp12.xyz = tmp12.xyz / tmp10.xyz;
                    tmp13.xyz = tmp10.xyz > float3(0.0, 0.0, 0.0);
                    tmp11.xyz = tmp13.xyz ? tmp11.xyz : tmp12.xyz;
                    tmp1.w = min(tmp11.y, tmp11.x);
                    tmp1.w = min(tmp11.z, tmp1.w);
                    tmp11.xyz = tmp0.xyz - unity_SpecCube0_ProbePosition.xyz;
                    tmp10.xyz = tmp10.xyz * tmp1.www + tmp11.xyz;
                } else {
                    tmp10.xyz = tmp7.xyz;
                }
                tmp1.w = -tmp3.w * 0.7 + 1.7;
                tmp1.w = tmp1.w * tmp3.w;
                tmp1.w = tmp1.w * 6.0;
                tmp10 = UNITY_SAMPLE_TEXCUBE_SAMPLER(unity_SpecCube0, unity_SpecCube0, float4(tmp10.xyz, tmp1.w));
                tmp4.w = tmp10.w - 1.0;
                tmp4.w = unity_SpecCube0_HDR.w * tmp4.w + 1.0;
                tmp4.w = log(tmp4.w);
                tmp4.w = tmp4.w * unity_SpecCube0_HDR.y;
                tmp4.w = exp(tmp4.w);
                tmp4.w = tmp4.w * unity_SpecCube0_HDR.x;
                tmp11.xyz = tmp10.xyz * tmp4.www;
                tmp5.w = unity_SpecCube0_BoxMin.w < 0.99999;
                if (tmp5.w) {
                    tmp5.w = unity_SpecCube1_ProbePosition.w > 0.0;
                    if (tmp5.w) {
                        tmp5.w = dot(tmp7.xyz, tmp7.xyz);
                        tmp5.w = rsqrt(tmp5.w);
                        tmp12.xyz = tmp5.www * tmp7.xyz;
                        tmp13.xyz = unity_SpecCube1_BoxMax.xyz - tmp0.xyz;
                        tmp13.xyz = tmp13.xyz / tmp12.xyz;
                        tmp14.xyz = unity_SpecCube1_BoxMin.xyz - tmp0.xyz;
                        tmp14.xyz = tmp14.xyz / tmp12.xyz;
                        tmp15.xyz = tmp12.xyz > float3(0.0, 0.0, 0.0);
                        tmp13.xyz = tmp15.xyz ? tmp13.xyz : tmp14.xyz;
                        tmp5.w = min(tmp13.y, tmp13.x);
                        tmp5.w = min(tmp13.z, tmp5.w);
                        tmp0.xyz = tmp0.xyz - unity_SpecCube1_ProbePosition.xyz;
                        tmp7.xyz = tmp12.xyz * tmp5.www + tmp0.xyz;
                    }
                    tmp7 = UNITY_SAMPLE_TEXCUBE_SAMPLER(unity_SpecCube0, unity_SpecCube0, float4(tmp7.xyz, tmp1.w));
                    tmp0.x = tmp7.w - 1.0;
                    tmp0.x = unity_SpecCube1_HDR.w * tmp0.x + 1.0;
                    tmp0.x = log(tmp0.x);
                    tmp0.x = tmp0.x * unity_SpecCube1_HDR.y;
                    tmp0.x = exp(tmp0.x);
                    tmp0.x = tmp0.x * unity_SpecCube1_HDR.x;
                    tmp0.xyz = tmp7.xyz * tmp0.xxx;
                    tmp7.xyz = tmp4.www * tmp10.xyz + -tmp0.xyz;
                    tmp11.xyz = unity_SpecCube0_BoxMin.www * tmp7.xyz + tmp0.xyz;
                }
                tmp0.xyz = tmp2.www * tmp11.xyz;
                tmp5.xyz = tmp5.xyz * _Color.xyz + float3(-0.04, -0.04, -0.04);
                tmp5.xyz = tmp8.xxx * tmp5.xyz + float3(0.04, 0.04, 0.04);
                tmp1.w = -tmp8.x * 0.96 + 0.96;
                tmp6.xyz = tmp1.www * tmp6.xyz;
                tmp1.xyz = tmp1.xyz * tmp0.www + _WorldSpaceLightPos0.xyz;
                tmp0.w = dot(tmp1.xyz, tmp1.xyz);
                tmp0.w = max(tmp0.w, 0.001);
                tmp0.w = rsqrt(tmp0.w);
                tmp1.xyz = tmp0.www * tmp1.xyz;
                tmp0.w = dot(tmp4.xyz, tmp2.xyz);
                tmp2.x = saturate(dot(tmp4.xyz, _WorldSpaceLightPos0.xyz));
                tmp2.y = saturate(dot(tmp4.xyz, tmp1.xyz));
                tmp1.x = saturate(dot(_WorldSpaceLightPos0.xyz, tmp1.xyz));
                tmp1.y = tmp1.x * tmp1.x;
                tmp1.y = dot(tmp1.xy, tmp3.xy);
                tmp1.y = tmp1.y - 0.5;
                tmp1.z = 1.0 - tmp2.x;
                tmp2.z = tmp1.z * tmp1.z;
                tmp2.z = tmp2.z * tmp2.z;
                tmp1.z = tmp1.z * tmp2.z;
                tmp1.z = tmp1.y * tmp1.z + 1.0;
                tmp2.z = 1.0 - abs(tmp0.w);
                tmp2.w = tmp2.z * tmp2.z;
                tmp2.w = tmp2.w * tmp2.w;
                tmp2.z = tmp2.z * tmp2.w;
                tmp1.y = tmp1.y * tmp2.z + 1.0;
                tmp1.y = tmp1.y * tmp1.z;
                tmp1.y = tmp2.x * tmp1.y;
                tmp1.z = tmp3.w * tmp3.w;
                tmp1.z = max(tmp1.z, 0.002);
                tmp2.w = 1.0 - tmp1.z;
                tmp3.w = abs(tmp0.w) * tmp2.w + tmp1.z;
                tmp2.w = tmp2.x * tmp2.w + tmp1.z;
                tmp0.w = abs(tmp0.w) * tmp2.w;
                tmp0.w = tmp2.x * tmp3.w + tmp0.w;
                tmp0.w = tmp0.w + 0.00001;
                tmp0.w = 0.5 / tmp0.w;
                tmp2.w = tmp1.z * tmp1.z;
                tmp3.w = tmp2.y * tmp2.w + -tmp2.y;
                tmp2.y = tmp3.w * tmp2.y + 1.0;
                tmp2.w = tmp2.w * 0.3183099;
                tmp2.y = tmp2.y * tmp2.y + 0.0000001;
                tmp2.y = tmp2.w / tmp2.y;
                tmp0.w = tmp0.w * tmp2.y;
                tmp0.w = tmp0.w * 3.141593;
                tmp0.w = tmp2.x * tmp0.w;
                tmp0.w = max(tmp0.w, 0.0);
                tmp1.z = tmp1.z * tmp1.z + 1.0;
                tmp1.z = 1.0 / tmp1.z;
                tmp2.x = dot(tmp5.xyz, tmp5.xyz);
                tmp2.x = tmp2.x != 0.0;
                tmp2.x = tmp2.x ? 1.0 : 0.0;
                tmp0.w = tmp0.w * tmp2.x;
                tmp1.w = tmp8.w * _GlossMapScale + -tmp1.w;
                tmp1.w = saturate(tmp1.w + 1.0);
                tmp2.xyw = tmp1.yyy * tmp9.xyz;
                tmp4.xyz = tmp9.xyz * tmp0.www;
                tmp0.w = 1.0 - tmp1.x;
                tmp1.x = tmp0.w * tmp0.w;
                tmp1.x = tmp1.x * tmp1.x;
                tmp0 = tmp0 * tmp1.zzzx;
                tmp7.xyz = float3(1.0, 1.0, 1.0) - tmp5.xyz;
                tmp7.xyz = tmp7.xyz * tmp0.www + tmp5.xyz;
                tmp4.xyz = tmp4.xyz * tmp7.xyz;
                tmp2.xyw = tmp6.xyz * tmp2.xyw + tmp4.xyz;
                tmp1.xyz = tmp1.www - tmp5.xyz;
                tmp1.xyz = tmp2.zzz * tmp1.xyz + tmp5.xyz;
                tmp0.xyz = tmp0.xyz * tmp1.xyz + tmp2.xyw;
                o.sv_target.xyz = tmp3.xyz + tmp0.xyz;
                o.sv_target.w = 1.0;
                return o;
			}
			ENDCG
		}
		Pass {
			Name "FORWARD"
			Tags { "IsEmissive" = "true" "LIGHTMODE" = "FORWARDADD" "QUEUE" = "Geometry+0" "RenderType" = "Opaque" "SHADOWSUPPORT" = "true" }
			Blend One One, One One
			ZWrite Off
			GpuProgramID 116983
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
				float3 texcoord4 : TEXCOORD4;
				float3 texcoord5 : TEXCOORD5;
				float4 texcoord6 : TEXCOORD6;
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
			float _BumpScale;
			float4 _MainTex_ST;
			float4 _Color;
			float _GlossMapScale;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _BumpMap;
			sampler2D _MainTex;
			sampler2D _MetallicGlossMap;
			sampler2D _LightTexture0;

			// Keywords: POINT
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                float4 tmp3;
                tmp0 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                tmp1 = tmp0 + unity_ObjectToWorld._m03_m13_m23_m33;
                tmp2 = tmp1.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp2 = unity_MatrixVP._m00_m10_m20_m30 * tmp1.xxxx + tmp2;
                tmp2 = unity_MatrixVP._m02_m12_m22_m32 * tmp1.zzzz + tmp2;
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp1.wwww + tmp2;
                o.texcoord.xy = v.texcoord.xy * _texcoord_ST.xy + _texcoord_ST.zw;
                tmp1.y = dot(v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp1.z = dot(v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp1.x = dot(v.normal.xyz, unity_WorldToObject._m02_m12_m22);
                tmp1.w = dot(tmp1.xyz, tmp1.xyz);
                tmp1.w = rsqrt(tmp1.w);
                tmp1.xyz = tmp1.www * tmp1.xyz;
                tmp2.xyz = v.tangent.yyy * unity_ObjectToWorld._m11_m21_m01;
                tmp2.xyz = unity_ObjectToWorld._m10_m20_m00 * v.tangent.xxx + tmp2.xyz;
                tmp2.xyz = unity_ObjectToWorld._m12_m22_m02 * v.tangent.zzz + tmp2.xyz;
                tmp1.w = dot(tmp2.xyz, tmp2.xyz);
                tmp1.w = rsqrt(tmp1.w);
                tmp2.xyz = tmp1.www * tmp2.xyz;
                tmp3.xyz = tmp1.xyz * tmp2.xyz;
                tmp3.xyz = tmp1.zxy * tmp2.yzx + -tmp3.xyz;
                tmp1.w = v.tangent.w * unity_WorldTransformParams.w;
                tmp3.xyz = tmp1.www * tmp3.xyz;
                o.texcoord1.y = tmp3.x;
                o.texcoord1.x = tmp2.z;
                o.texcoord1.z = tmp1.y;
                o.texcoord2.x = tmp2.x;
                o.texcoord3.x = tmp2.y;
                o.texcoord2.z = tmp1.z;
                o.texcoord3.z = tmp1.x;
                o.texcoord2.y = tmp3.y;
                o.texcoord3.y = tmp3.z;
                o.texcoord4.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp0.xyz;
                tmp0 = unity_ObjectToWorld._m03_m13_m23_m33 * v.vertex.wwww + tmp0;
                tmp1.xyz = tmp0.yyy * unity_WorldToLight._m01_m11_m21;
                tmp1.xyz = unity_WorldToLight._m00_m10_m20 * tmp0.xxx + tmp1.xyz;
                tmp0.xyz = unity_WorldToLight._m02_m12_m22 * tmp0.zzz + tmp1.xyz;
                o.texcoord5.xyz = unity_WorldToLight._m03_m13_m23 * tmp0.www + tmp0.xyz;
                o.texcoord6 = float4(0.0, 0.0, 0.0, 0.0);
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
                float4 tmp5;
                float4 tmp6;
                float4 tmp7;
                float4 tmp8;
                tmp0.xyz = _WorldSpaceLightPos0.xyz - inp.texcoord4.xyz;
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp1.xyz = tmp0.www * tmp0.xyz;
                tmp2.xyz = _WorldSpaceCameraPos - inp.texcoord4.xyz;
                tmp1.w = dot(tmp2.xyz, tmp2.xyz);
                tmp1.w = rsqrt(tmp1.w);
                tmp2.xyz = tmp1.www * tmp2.xyz;
                tmp3.xy = inp.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                tmp4 = tex2D(_BumpMap, tmp3.xy);
                tmp4.x = tmp4.w * tmp4.x;
                tmp3.zw = tmp4.xy * float2(2.0, 2.0) + float2(-1.0, -1.0);
                tmp4.xy = tmp3.zw * _BumpScale.xx;
                tmp1.w = dot(tmp4.xy, tmp4.xy);
                tmp1.w = min(tmp1.w, 1.0);
                tmp1.w = 1.0 - tmp1.w;
                tmp4.z = sqrt(tmp1.w);
                tmp5 = tex2D(_MainTex, tmp3.xy);
                tmp6.xyz = tmp5.xyz * _Color.xyz;
                tmp3 = tex2D(_MetallicGlossMap, tmp3.xy);
                tmp7.xyz = inp.texcoord4.yyy * unity_WorldToLight._m01_m11_m21;
                tmp7.xyz = unity_WorldToLight._m00_m10_m20 * inp.texcoord4.xxx + tmp7.xyz;
                tmp7.xyz = unity_WorldToLight._m02_m12_m22 * inp.texcoord4.zzz + tmp7.xyz;
                tmp7.xyz = tmp7.xyz + unity_WorldToLight._m03_m13_m23;
                tmp1.w = unity_ProbeVolumeParams.x == 1.0;
                if (tmp1.w) {
                    tmp1.w = unity_ProbeVolumeParams.y == 1.0;
                    tmp8.xyz = inp.texcoord4.yyy * unity_ProbeVolumeWorldToObject._m01_m11_m21;
                    tmp8.xyz = unity_ProbeVolumeWorldToObject._m00_m10_m20 * inp.texcoord4.xxx + tmp8.xyz;
                    tmp8.xyz = unity_ProbeVolumeWorldToObject._m02_m12_m22 * inp.texcoord4.zzz + tmp8.xyz;
                    tmp8.xyz = tmp8.xyz + unity_ProbeVolumeWorldToObject._m03_m13_m23;
                    tmp8.xyz = tmp1.www ? tmp8.xyz : inp.texcoord4.xyz;
                    tmp8.xyz = tmp8.xyz - unity_ProbeVolumeMin;
                    tmp8.yzw = tmp8.xyz * unity_ProbeVolumeSizeInv;
                    tmp1.w = tmp8.y * 0.25 + 0.75;
                    tmp2.w = unity_ProbeVolumeParams.z * 0.5 + 0.75;
                    tmp8.x = max(tmp1.w, tmp2.w);
                    tmp8 = UNITY_SAMPLE_TEX3D_SAMPLER(unity_ProbeVolumeSH, unity_ProbeVolumeSH, tmp8.xzw);
                } else {
                    tmp8 = float4(1.0, 1.0, 1.0, 1.0);
                }
                tmp1.w = saturate(dot(tmp8, unity_OcclusionMaskSelector));
                tmp2.w = dot(tmp7.xyz, tmp7.xyz);
                tmp7 = tex2D(_LightTexture0, tmp2.ww);
                tmp1.w = tmp1.w * tmp7.x;
                tmp7.x = dot(inp.texcoord1.xyz, tmp4.xyz);
                tmp7.y = dot(inp.texcoord2.xyz, tmp4.xyz);
                tmp7.z = dot(inp.texcoord3.xyz, tmp4.xyz);
                tmp2.w = dot(tmp7.xyz, tmp7.xyz);
                tmp2.w = rsqrt(tmp2.w);
                tmp4.xyz = tmp2.www * tmp7.xyz;
                tmp7.xyz = tmp1.www * _LightColor0.xyz;
                tmp5.xyz = tmp5.xyz * _Color.xyz + float3(-0.04, -0.04, -0.04);
                tmp5.xyz = tmp3.xxx * tmp5.xyz + float3(0.04, 0.04, 0.04);
                tmp1.w = -tmp3.x * 0.96 + 0.96;
                tmp3.xyz = tmp1.www * tmp6.xyz;
                tmp1.w = -tmp3.w * _GlossMapScale + 1.0;
                tmp0.xyz = tmp0.xyz * tmp0.www + tmp2.xyz;
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = max(tmp0.w, 0.001);
                tmp0.w = rsqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                tmp0.w = dot(tmp4.xyz, tmp2.xyz);
                tmp2.x = saturate(dot(tmp4.xyz, tmp1.xyz));
                tmp2.y = saturate(dot(tmp4.xyz, tmp0.xyz));
                tmp0.x = saturate(dot(tmp1.xyz, tmp0.xyz));
                tmp0.y = tmp0.x * tmp0.x;
                tmp0.y = dot(tmp0.xy, tmp1.xy);
                tmp0.y = tmp0.y - 0.5;
                tmp0.z = 1.0 - tmp2.x;
                tmp1.x = tmp0.z * tmp0.z;
                tmp1.x = tmp1.x * tmp1.x;
                tmp0.z = tmp0.z * tmp1.x;
                tmp0.z = tmp0.y * tmp0.z + 1.0;
                tmp1.x = 1.0 - abs(tmp0.w);
                tmp1.y = tmp1.x * tmp1.x;
                tmp1.y = tmp1.y * tmp1.y;
                tmp1.x = tmp1.x * tmp1.y;
                tmp0.y = tmp0.y * tmp1.x + 1.0;
                tmp0.y = tmp0.y * tmp0.z;
                tmp0.z = tmp1.w * tmp1.w;
                tmp0.z = max(tmp0.z, 0.002);
                tmp1.x = 1.0 - tmp0.z;
                tmp1.y = abs(tmp0.w) * tmp1.x + tmp0.z;
                tmp1.x = tmp2.x * tmp1.x + tmp0.z;
                tmp0.w = abs(tmp0.w) * tmp1.x;
                tmp0.w = tmp2.x * tmp1.y + tmp0.w;
                tmp0.w = tmp0.w + 0.00001;
                tmp0.w = 0.5 / tmp0.w;
                tmp0.z = tmp0.z * tmp0.z;
                tmp1.x = tmp2.y * tmp0.z + -tmp2.y;
                tmp1.x = tmp1.x * tmp2.y + 1.0;
                tmp0.z = tmp0.z * 0.3183099;
                tmp1.x = tmp1.x * tmp1.x + 0.0000001;
                tmp0.z = tmp0.z / tmp1.x;
                tmp0.z = tmp0.z * tmp0.w;
                tmp0.z = tmp0.z * 3.141593;
                tmp0.yz = tmp2.xx * tmp0.yz;
                tmp0.z = max(tmp0.z, 0.0);
                tmp0.w = dot(tmp5.xyz, tmp5.xyz);
                tmp0.w = tmp0.w != 0.0;
                tmp0.w = tmp0.w ? 1.0 : 0.0;
                tmp0.z = tmp0.w * tmp0.z;
                tmp1.xyz = tmp0.yyy * tmp7.xyz;
                tmp0.yzw = tmp7.xyz * tmp0.zzz;
                tmp0.x = 1.0 - tmp0.x;
                tmp1.w = tmp0.x * tmp0.x;
                tmp1.w = tmp1.w * tmp1.w;
                tmp0.x = tmp0.x * tmp1.w;
                tmp2.xyz = float3(1.0, 1.0, 1.0) - tmp5.xyz;
                tmp2.xyz = tmp2.xyz * tmp0.xxx + tmp5.xyz;
                tmp0.xyz = tmp0.yzw * tmp2.xyz;
                o.sv_target.xyz = tmp3.xyz * tmp1.xyz + tmp0.xyz;
                o.sv_target.w = 1.0;
                return o;
			}
			ENDCG
		}
		Pass {
			Name "DEFERRED"
			Tags { "IsEmissive" = "true" "LIGHTMODE" = "DEFERRED" "QUEUE" = "Geometry+0" "RenderType" = "Opaque" }
			GpuProgramID 151636
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 texcoord3 : TEXCOORD3;
				float4 texcoord5 : TEXCOORD5;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
				float4 sv_target1 : SV_Target1;
				float4 sv_target2 : SV_Target2;
				float4 sv_target3 : SV_Target3;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _texcoord_ST;
			float4 _texcoord4_ST;
			// $Globals ConstantBuffers for Fragment Shader
			float _BumpScale;
			float4 _MainTex_ST;
			float4 _Color;
			float _EmissionState;
			float4 _LowPressureColor;
			float4 _ToxicColor;
			float4 _EmissionColor;
			float _AlwaysEmit;
			float _EmissionControl;
			float4 _Lightmap_ST;
			float _GlossMapScale;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _BumpMap;
			sampler2D _MainTex;
			sampler2D _MetallicGlossMap;
			sampler2D _OcclusionMap;
			sampler2D _EmissionMap;
			sampler2D _Lightmap;

			// Keywords:
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                float4 tmp3;
                tmp0 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                tmp1 = tmp0 + unity_ObjectToWorld._m03_m13_m23_m33;
                tmp0.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp0.xyz;
                tmp2 = tmp1.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp2 = unity_MatrixVP._m00_m10_m20_m30 * tmp1.xxxx + tmp2;
                tmp2 = unity_MatrixVP._m02_m12_m22_m32 * tmp1.zzzz + tmp2;
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp1.wwww + tmp2;
                o.texcoord.xy = v.texcoord.xy * _texcoord_ST.xy + _texcoord_ST.zw;
                o.texcoord.zw = v.texcoord3.xy * _texcoord4_ST.xy + _texcoord4_ST.zw;
                o.texcoord1.w = tmp0.x;
                tmp1.y = dot(v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp1.z = dot(v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp1.x = dot(v.normal.xyz, unity_WorldToObject._m02_m12_m22);
                tmp0.x = dot(tmp1.xyz, tmp1.xyz);
                tmp0.x = rsqrt(tmp0.x);
                tmp1.xyz = tmp0.xxx * tmp1.xyz;
                tmp2.xyz = v.tangent.yyy * unity_ObjectToWorld._m11_m21_m01;
                tmp2.xyz = unity_ObjectToWorld._m10_m20_m00 * v.tangent.xxx + tmp2.xyz;
                tmp2.xyz = unity_ObjectToWorld._m12_m22_m02 * v.tangent.zzz + tmp2.xyz;
                tmp0.x = dot(tmp2.xyz, tmp2.xyz);
                tmp0.x = rsqrt(tmp0.x);
                tmp2.xyz = tmp0.xxx * tmp2.xyz;
                tmp3.xyz = tmp1.xyz * tmp2.xyz;
                tmp3.xyz = tmp1.zxy * tmp2.yzx + -tmp3.xyz;
                tmp0.x = v.tangent.w * unity_WorldTransformParams.w;
                tmp3.xyz = tmp0.xxx * tmp3.xyz;
                o.texcoord1.y = tmp3.x;
                o.texcoord1.x = tmp2.z;
                o.texcoord1.z = tmp1.y;
                o.texcoord2.x = tmp2.x;
                o.texcoord3.x = tmp2.y;
                o.texcoord2.z = tmp1.z;
                o.texcoord3.z = tmp1.x;
                o.texcoord2.w = tmp0.y;
                o.texcoord3.w = tmp0.z;
                o.texcoord2.y = tmp3.y;
                o.texcoord3.y = tmp3.z;
                o.texcoord5 = float4(0.0, 0.0, 0.0, 0.0);
                return o;
			}
			// Keywords:
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                float4 tmp3;
                float4 tmp4;
                float4 tmp5;
                tmp0.xy = inp.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                tmp1 = tex2D(_OcclusionMap, tmp0.xy);
                tmp0.z = 1.0 - tmp1.x;
                tmp0.z = 1.0 - tmp0.z;
                o.sv_target.w = tmp0.z;
                tmp1 = tex2D(_MainTex, tmp0.xy);
                tmp2.xyz = tmp1.xyz * _Color.xyz;
                tmp1.xyz = tmp1.xyz * _Color.xyz + float3(-0.04, -0.04, -0.04);
                tmp3 = tex2D(_MetallicGlossMap, tmp0.xy);
                tmp0.w = -tmp3.x * 0.96 + 0.96;
                o.sv_target.xyz = tmp0.www * tmp2.xyz;
                o.sv_target1.xyz = tmp3.xxx * tmp1.xyz + float3(0.04, 0.04, 0.04);
                o.sv_target1.w = tmp3.w * _GlossMapScale;
                tmp0.w = 1.0 - tmp3.x;
                tmp1 = tex2D(_BumpMap, tmp0.xy);
                tmp3 = tex2D(_EmissionMap, tmp0.xy);
                tmp1.x = tmp1.w * tmp1.x;
                tmp0.xy = tmp1.xy * float2(2.0, 2.0) + float2(-1.0, -1.0);
                tmp1.xy = tmp0.xy * _BumpScale.xx;
                tmp0.x = dot(tmp1.xy, tmp1.xy);
                tmp0.x = min(tmp0.x, 1.0);
                tmp0.x = 1.0 - tmp0.x;
                tmp1.z = sqrt(tmp0.x);
                tmp4.x = dot(inp.texcoord1.xyz, tmp1.xyz);
                tmp4.y = dot(inp.texcoord2.xyz, tmp1.xyz);
                tmp4.z = dot(inp.texcoord3.xyz, tmp1.xyz);
                tmp0.x = dot(tmp4.xyz, tmp4.xyz);
                tmp0.x = rsqrt(tmp0.x);
                tmp1.xyz = tmp0.xxx * tmp4.xyz;
                o.sv_target2.xyz = tmp1.xyz * float3(0.5, 0.5, 0.5) + float3(0.5, 0.5, 0.5);
                o.sv_target2.w = 1.0;
                tmp0.xy = inp.texcoord.zw * _Lightmap_ST.xy + _Lightmap_ST.zw;
                tmp1 = tex2D(_Lightmap, tmp0.xy);
                tmp0.xyw = tmp0.www * tmp1.xyz;
                tmp0.xyw = tmp2.xyz * tmp0.xyw;
                tmp0.xyz = tmp0.zzz * tmp0.xyw;
                tmp1.xy = _EmissionState.xx > float2(2.0, 1.0);
                tmp1.zw = _EmissionState.xx == float2(2.0, 1.0);
                tmp1.xy = uint2(tmp1.xy) | uint2(tmp1.zw);
                tmp2.xyz = _EmissionState.xxx < float3(3.0, 2.0, 1.0);
                tmp4.xyz = tmp2.xxx ? _LowPressureColor.xyz : 0.0;
                tmp2.xyw = tmp2.yyy ? _ToxicColor.xyz : 0.0;
                tmp5.xyz = tmp2.zzz ? _EmissionColor.xyz : 0.0;
                tmp1.xzw = tmp1.xxx ? tmp4.xyz : tmp2.xyw;
                tmp1.xyz = tmp1.yyy ? tmp1.xzw : tmp5.xyz;
                tmp1.xyz = tmp1.xyz * tmp3.xyz;
                tmp0.w = _AlwaysEmit > 0.5;
                tmp1.w = _AlwaysEmit == 0.5;
                tmp0.w = uint1(tmp0.w) | uint1(tmp1.w);
                tmp1.w = _AlwaysEmit < 0.5;
                tmp1.w = tmp1.w ? _EmissionControl : 0.0;
                tmp0.w = tmp0.w ? 1.0 : tmp1.w;
                tmp0.xyz = tmp1.xyz * tmp0.www + tmp0.xyz;
                o.sv_target3.xyz = exp(-tmp0.xyz);
                o.sv_target3.w = 1.0;
                return o;
			}
			ENDCG
		}
		Pass {
			Name "ShadowCaster"
			Tags { "IsEmissive" = "true" "LIGHTMODE" = "SHADOWCASTER" "QUEUE" = "Geometry+0" "RenderType" = "Opaque" "SHADOWSUPPORT" = "true" }
			GpuProgramID 260875
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float3 texcoord1 : TEXCOORD1;
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

			// Keywords: SHADOWS_DEPTH
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
                o.texcoord1.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp0.xyz;
                return o;
			}
			// Keywords: SHADOWS_DEPTH
			fout frag(v2f inp)
			{
                fout o;
                o.sv_target = float4(0.0, 0.0, 0.0, 0.0);
                return o;
			}
			ENDCG
		}
		Pass {
			Name "Meta"
			Tags { "IsEmissive" = "true" "LIGHTMODE" = "META" "QUEUE" = "Geometry+0" "RenderType" = "Opaque" }
			Cull Off
			GpuProgramID 314398
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 texcoord3 : TEXCOORD3;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _texcoord_ST;
			float4 _texcoord4_ST;
			// $Globals ConstantBuffers for Fragment Shader
			float4 _MainTex_ST;
			float4 _Color;
			float _EmissionState;
			float4 _LowPressureColor;
			float4 _ToxicColor;
			float4 _EmissionColor;
			float _AlwaysEmit;
			float _EmissionControl;
			float4 _Lightmap_ST;
			float unity_OneOverOutputBoost;
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
			sampler2D _MainTex;
			sampler2D _MetallicGlossMap;
			sampler2D _OcclusionMap;
			sampler2D _EmissionMap;
			sampler2D _Lightmap;

			// Keywords:
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                float4 tmp3;
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
                o.texcoord.zw = v.texcoord3.xy * _texcoord4_ST.xy + _texcoord4_ST.zw;
                tmp0.y = dot(v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp0.z = dot(v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp0.x = dot(v.normal.xyz, unity_WorldToObject._m02_m12_m22);
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                tmp1.xyz = v.tangent.yyy * unity_ObjectToWorld._m11_m21_m01;
                tmp1.xyz = unity_ObjectToWorld._m10_m20_m00 * v.tangent.xxx + tmp1.xyz;
                tmp1.xyz = unity_ObjectToWorld._m12_m22_m02 * v.tangent.zzz + tmp1.xyz;
                tmp0.w = dot(tmp1.xyz, tmp1.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp1.xyz = tmp0.www * tmp1.xyz;
                tmp2.xyz = tmp0.xyz * tmp1.xyz;
                tmp2.xyz = tmp0.zxy * tmp1.yzx + -tmp2.xyz;
                tmp0.w = v.tangent.w * unity_WorldTransformParams.w;
                tmp2.xyz = tmp0.www * tmp2.xyz;
                o.texcoord1.y = tmp2.x;
                tmp3.xyz = v.vertex.yyy * unity_ObjectToWorld._m01_m11_m21;
                tmp3.xyz = unity_ObjectToWorld._m00_m10_m20 * v.vertex.xxx + tmp3.xyz;
                tmp3.xyz = unity_ObjectToWorld._m02_m12_m22 * v.vertex.zzz + tmp3.xyz;
                tmp3.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp3.xyz;
                o.texcoord1.w = tmp3.x;
                o.texcoord1.x = tmp1.z;
                o.texcoord1.z = tmp0.y;
                o.texcoord2.x = tmp1.x;
                o.texcoord3.x = tmp1.y;
                o.texcoord2.z = tmp0.z;
                o.texcoord3.z = tmp0.x;
                o.texcoord2.w = tmp3.y;
                o.texcoord3.w = tmp3.z;
                o.texcoord2.y = tmp2.y;
                o.texcoord3.y = tmp2.z;
                return o;
			}
			// Keywords:
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
                tmp0.x = _AlwaysEmit > 0.5;
                tmp0.y = _AlwaysEmit == 0.5;
                tmp0.x = uint1(tmp0.x) | uint1(tmp0.y);
                tmp0.y = _AlwaysEmit < 0.5;
                tmp0.y = tmp0.y ? _EmissionControl : 0.0;
                tmp0.x = tmp0.x ? 1.0 : tmp0.y;
                tmp0.yz = inp.texcoord.zw * _Lightmap_ST.xy + _Lightmap_ST.zw;
                tmp1 = tex2D(_Lightmap, tmp0.yz);
                tmp0.yz = inp.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                tmp2 = tex2D(_MetallicGlossMap, tmp0.yz);
                tmp0.w = 1.0 - tmp2.x;
                tmp1.xyz = tmp0.www * tmp1.xyz;
                tmp2 = tex2D(_MainTex, tmp0.yz);
                tmp2.xyz = tmp2.xyz * _Color.xyz;
                tmp1.xyz = tmp1.xyz * tmp2.xyz;
                tmp2.xyz = log(tmp2.xyz);
                tmp3 = tex2D(_OcclusionMap, tmp0.yz);
                tmp4 = tex2D(_EmissionMap, tmp0.yz);
                tmp0.y = 1.0 - tmp3.x;
                tmp0.y = 1.0 - tmp0.y;
                tmp0.yzw = tmp0.yyy * tmp1.xyz;
                tmp1.xy = _EmissionState.xx > float2(2.0, 1.0);
                tmp1.zw = _EmissionState.xx == float2(2.0, 1.0);
                tmp1.xy = uint2(tmp1.xy) | uint2(tmp1.zw);
                tmp3.xyz = _EmissionState.xxx < float3(3.0, 2.0, 1.0);
                tmp5.xyz = tmp3.xxx ? _LowPressureColor.xyz : 0.0;
                tmp3.xyw = tmp3.yyy ? _ToxicColor.xyz : 0.0;
                tmp6.xyz = tmp3.zzz ? _EmissionColor.xyz : 0.0;
                tmp1.xzw = tmp1.xxx ? tmp5.xyz : tmp3.xyw;
                tmp1.xyz = tmp1.yyy ? tmp1.xzw : tmp6.xyz;
                tmp1.xyz = tmp1.xyz * tmp4.xyz;
                tmp0.xyz = tmp1.xyz * tmp0.xxx + tmp0.yzw;
                tmp1.xyz = tmp0.xyz * float3(0.305306, 0.305306, 0.305306) + float3(0.6821711, 0.6821711, 0.6821711);
                tmp1.xyz = tmp0.xyz * tmp1.xyz + float3(0.0125229, 0.0125229, 0.0125229);
                tmp1.xyz = tmp0.xyz * tmp1.xyz;
                tmp0.w = unity_UseLinearSpace != 0.0;
                tmp0.xyz = tmp0.www ? tmp0.xyz : tmp1.xyz;
                tmp1.x = saturate(unity_OneOverOutputBoost);
                tmp1.xyz = tmp2.xyz * tmp1.xxx;
                tmp1.xyz = exp(tmp1.xyz);
                tmp1.xyz = min(tmp1.xyz, unity_MaxOutputValue.xxx);
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
	CustomEditor "ASEMaterialInspector"
}
