Shader "DamageParallaxShader" {
	Properties {
		[HideInInspector] __dirty ("", Float) = 1
		_MaskClipValue ("Mask Clip Value", Float) = 0.5
		_Texture1 ("Texture1", 2D) = "white" {}
		_Texture1Normal ("Texture1 Normal", 2D) = "bump" {}
		_Texture1Height ("Texture1 Height", Float) = 0
		_Texture1EmissionMask ("Texture1 Emission Mask", 2D) = "white" {}
		_Texture1EmissionColor ("Texture1 Emission Color", Vector) = (0,0,0,0)
		_Texture1Emission ("Texture1 Emission", Float) = 0
		_Texture2 ("Texture2", 2D) = "white" {}
		_Texture2Normal ("Texture2 Normal", 2D) = "bump" {}
		_Texture2Height ("Texture2 Height", Float) = 0
		_Metallic ("Metallic", Range(0, 1)) = 0
		_Smoothness ("Smoothness", Range(0, 1)) = 0
		_Crack ("Crack", 2D) = "white" {}
		_Noise ("Noise", 2D) = "white" {}
		_NoiseMax ("NoiseMax", Float) = 0
		_Health ("Health", Range(0, 1)) = 0
		_Lightning ("Lightning", 2D) = "white" {}
		_LightningBrightness ("LightningBrightness", Float) = 0
		_SystemDamage ("SystemDamage", Range(0, 1)) = 0
		[HideInInspector] _texcoord ("", 2D) = "white" {}
	}
	SubShader {
		Tags { "IGNOREPROJECTOR" = "true" "IsEmissive" = "true" "QUEUE" = "AlphaTest+0" "RenderType" = "TransparentCutout" }
		Pass {
			Name "FORWARD"
			Tags { "IGNOREPROJECTOR" = "true" "IsEmissive" = "true" "LIGHTMODE" = "FORWARDBASE" "QUEUE" = "AlphaTest+0" "RenderType" = "TransparentCutout" "SHADOWSUPPORT" = "true" }
			GpuProgramID 4889
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float2 texcoord4 : TEXCOORD4;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 texcoord3 : TEXCOORD3;
				float4 texcoord7 : TEXCOORD7;
				float4 texcoord8 : TEXCOORD8;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _texcoord_ST;
			// $Globals ConstantBuffers for Fragment Shader
			float4 _LightColor0;
			float4 _Texture2_ST;
			float _Texture2Height;
			float4 _Texture1_ST;
			float _Texture1Height;
			float _Health;
			float _NoiseMax;
			float _SystemDamage;
			float _LightningBrightness;
			float _Texture1Emission;
			float4 _Texture1EmissionColor;
			float _Metallic;
			float _Smoothness;
			float4 _Noise_ST;
			float4 _Crack_ST;
			float _MaskClipValue;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _Texture2;
			sampler2D _Texture2Normal;
			sampler2D _Texture1Normal;
			sampler2D _Noise;
			sampler2D _Texture1;
			sampler2D _Crack;
			sampler2D _Lightning;
			sampler2D _Texture1EmissionMask;

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
                o.texcoord4.xy = v.texcoord.xy;
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
                o.texcoord7 = float4(0.0, 0.0, 0.0, 0.0);
                o.texcoord8 = float4(0.0, 0.0, 0.0, 0.0);
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
                tmp0.x = inp.texcoord1.w;
                tmp0.y = inp.texcoord2.w;
                tmp0.z = inp.texcoord3.w;
                tmp1.xyz = _WorldSpaceCameraPos - tmp0.xyz;
                tmp0.w = dot(tmp1.xyz, tmp1.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp2.xyz = tmp0.www * tmp1.xyz;
                tmp3.xy = tmp2.yy * inp.texcoord2.xy;
                tmp3.xy = inp.texcoord1.xy * tmp2.xx + tmp3.xy;
                tmp3.xy = inp.texcoord3.xy * tmp2.zz + tmp3.xy;
                tmp3.zw = inp.texcoord.xy * _Texture2_ST.xy + _Texture2_ST.zw;
                tmp1.w = _Texture2Height - 1.0;
                tmp3.zw = tmp1.ww * tmp3.xy + tmp3.zw;
                tmp4 = tex2D(_Texture2, tmp3.zw);
                tmp5.xy = inp.texcoord.xy * _Texture1_ST.xy + _Texture1_ST.zw;
                tmp1.w = _Texture1Height - 1.0;
                tmp3.xy = tmp1.ww * tmp3.xy + tmp5.xy;
                tmp5 = tex2D(_Texture2Normal, tmp3.zw);
                tmp5.x = tmp5.w * tmp5.x;
                tmp5.xy = tmp5.xy * float2(2.0, 2.0) + float2(-1.0, -1.0);
                tmp1.w = dot(tmp5.xy, tmp5.xy);
                tmp1.w = min(tmp1.w, 1.0);
                tmp1.w = 1.0 - tmp1.w;
                tmp5.z = sqrt(tmp1.w);
                tmp1.w = 1.0 - tmp4.w;
                tmp6 = tex2D(_Texture1Normal, tmp3.xy);
                tmp6.x = tmp6.w * tmp6.x;
                tmp6.xy = tmp6.xy * float2(2.0, 2.0) + float2(-1.0, -1.0);
                tmp2.w = dot(tmp6.xy, tmp6.xy);
                tmp2.w = min(tmp2.w, 1.0);
                tmp2.w = 1.0 - tmp2.w;
                tmp6.z = sqrt(tmp2.w);
                tmp6.xyz = tmp1.www * tmp6.xyz;
                tmp5.xyz = tmp5.xyz * tmp4.www + tmp6.xyz;
                tmp6 = tex2D(_Noise, tmp3.zw);
                tmp2.w = tmp6.x + 0.1;
                tmp2.w = saturate(tmp2.w * 0.9090909);
                tmp6 = tex2D(_Texture1, tmp3.xy);
                tmp7 = tex2D(_Noise, tmp3.xy);
                tmp7.yz = _Health.xx * _NoiseMax.xx + float2(-0.1, -0.05);
                tmp5.w = tmp7.x - tmp7.y;
                tmp7.xy = _Health.xx * _NoiseMax.xx + -tmp7.yz;
                tmp5.w = saturate(tmp5.w / tmp7.x);
                tmp4.xyz = tmp4.www * tmp4.xyz;
                tmp8 = tex2D(_Crack, tmp3.zw);
                tmp8.xyz = float3(1.0, 1.0, 1.0) - tmp8.xyz;
                tmp4.xyz = tmp4.xyz * tmp8.xyz;
                tmp8 = tex2D(_Crack, tmp3.xy);
                tmp8.xyz = float3(1.0, 1.0, 1.0) - tmp8.xyz;
                tmp8.xyz = tmp6.xyz * tmp8.xyz;
                tmp8.xyz = tmp5.www * tmp8.xyz;
                tmp8.xyz = tmp1.www * tmp8.xyz;
                tmp4.xyz = tmp4.xyz * tmp2.www + tmp8.xyz;
                tmp3.zw = _SystemDamage.xx * float2(0.8, 0.5) + float2(0.2, 0.5);
                tmp1.w = tmp3.z * _Time.y;
                tmp1.w = tmp1.w * 0.78125;
                tmp2.w = tmp1.w >= -tmp1.w;
                tmp1.w = frac(abs(tmp1.w));
                tmp1.w = tmp2.w ? tmp1.w : -tmp1.w;
                tmp1.w = tmp1.w * 64.0;
                tmp1.w = round(tmp1.w);
                tmp2.w = tmp1.w < 0.0;
                tmp2.w = tmp2.w ? 64.0 : 0.0;
                tmp1.w = tmp1.w + tmp2.w;
                tmp2.w = tmp1.w * 0.125;
                tmp3.z = tmp2.w >= -tmp2.w;
                tmp2.w = frac(abs(tmp2.w));
                tmp2.w = tmp3.z ? tmp2.w : -tmp2.w;
                tmp2.w = tmp2.w * 8.0;
                tmp2.w = round(tmp2.w);
                tmp8.x = tmp2.w * 0.125;
                tmp1.w = tmp1.w - tmp2.w;
                tmp1.w = tmp1.w * 0.015625;
                tmp2.w = tmp1.w >= -tmp1.w;
                tmp1.w = frac(abs(tmp1.w));
                tmp1.w = tmp2.w ? tmp1.w : -tmp1.w;
                tmp1.w = tmp1.w * 8.0;
                tmp1.w = round(tmp1.w);
                tmp1.w = 7.0 - tmp1.w;
                tmp8.y = tmp1.w * 0.125;
                tmp7.xw = inp.texcoord4.xy * float2(0.125, 0.125) + tmp8.xy;
                tmp1.w = _SystemDamage > 0.2;
                tmp1.w = tmp1.w ? tmp3.w : 0.0;
                tmp8 = tex2D(_Lightning, tmp7.xw);
                tmp8.xyz = tmp8.xyz * _LightningBrightness.xxx;
                tmp3 = tex2D(_Texture1EmissionMask, tmp3.xy);
                tmp3.xyz = tmp3.xxx * tmp6.xyz;
                tmp3.xyz = tmp3.xyz * _Texture1Emission.xxx;
                tmp3.xyz = tmp3.xyz * _Texture1EmissionColor.xyz;
                tmp3.xyz = tmp8.xyz * tmp1.www + tmp3.xyz;
                tmp6.xy = inp.texcoord.xy * _Noise_ST.xy + _Noise_ST.zw;
                tmp6 = tex2D(_Noise, tmp6.xy);
                tmp1.w = tmp6.x - tmp7.z;
                tmp1.w = saturate(tmp1.w / tmp7.y);
                tmp6.xy = inp.texcoord.xy * _Crack_ST.xy + _Crack_ST.zw;
                tmp6 = tex2D(_Crack, tmp6.xy);
                tmp1.w = saturate(tmp1.w - tmp6.x);
                tmp1.w = tmp1.w - _MaskClipValue;
                tmp1.w = tmp1.w < 0.0;
                if (tmp1.w) {
                    discard;
                }
                tmp1.w = unity_ProbeVolumeParams.x == 1.0;
                if (tmp1.w) {
                    tmp1.w = unity_ProbeVolumeParams.y == 1.0;
                    tmp6.xyz = inp.texcoord2.www * unity_ProbeVolumeWorldToObject._m01_m11_m21;
                    tmp6.xyz = unity_ProbeVolumeWorldToObject._m00_m10_m20 * inp.texcoord1.www + tmp6.xyz;
                    tmp6.xyz = unity_ProbeVolumeWorldToObject._m02_m12_m22 * inp.texcoord3.www + tmp6.xyz;
                    tmp6.xyz = tmp6.xyz + unity_ProbeVolumeWorldToObject._m03_m13_m23;
                    tmp6.xyz = tmp1.www ? tmp6.xyz : tmp0.xyz;
                    tmp6.xyz = tmp6.xyz - unity_ProbeVolumeMin;
                    tmp6.yzw = tmp6.xyz * unity_ProbeVolumeSizeInv;
                    tmp1.w = tmp6.y * 0.25 + 0.75;
                    tmp2.w = unity_ProbeVolumeParams.z * 0.5 + 0.75;
                    tmp6.x = max(tmp1.w, tmp2.w);
                    tmp6 = UNITY_SAMPLE_TEX3D_SAMPLER(unity_ProbeVolumeSH, unity_ProbeVolumeSH, tmp6.xzw);
                } else {
                    tmp6 = float4(1.0, 1.0, 1.0, 1.0);
                }
                tmp1.w = saturate(dot(tmp6, unity_OcclusionMaskSelector));
                tmp6.x = dot(inp.texcoord1.xyz, tmp5.xyz);
                tmp6.y = dot(inp.texcoord2.xyz, tmp5.xyz);
                tmp6.z = dot(inp.texcoord3.xyz, tmp5.xyz);
                tmp2.w = dot(tmp6.xyz, tmp6.xyz);
                tmp2.w = rsqrt(tmp2.w);
                tmp5.xyz = tmp2.www * tmp6.xyz;
                tmp2.w = 1.0 - _Smoothness;
                tmp3.w = dot(-tmp2.xyz, tmp5.xyz);
                tmp3.w = tmp3.w + tmp3.w;
                tmp6.xyz = tmp5.xyz * -tmp3.www + -tmp2.xyz;
                tmp7.xyz = tmp1.www * _LightColor0.xyz;
                tmp1.w = unity_SpecCube0_ProbePosition.w > 0.0;
                if (tmp1.w) {
                    tmp1.w = dot(tmp6.xyz, tmp6.xyz);
                    tmp1.w = rsqrt(tmp1.w);
                    tmp8.xyz = tmp1.www * tmp6.xyz;
                    tmp9.xyz = unity_SpecCube0_BoxMax.xyz - tmp0.xyz;
                    tmp9.xyz = tmp9.xyz / tmp8.xyz;
                    tmp10.xyz = unity_SpecCube0_BoxMin.xyz - tmp0.xyz;
                    tmp10.xyz = tmp10.xyz / tmp8.xyz;
                    tmp11.xyz = tmp8.xyz > float3(0.0, 0.0, 0.0);
                    tmp9.xyz = tmp11.xyz ? tmp9.xyz : tmp10.xyz;
                    tmp1.w = min(tmp9.y, tmp9.x);
                    tmp1.w = min(tmp9.z, tmp1.w);
                    tmp9.xyz = tmp0.xyz - unity_SpecCube0_ProbePosition.xyz;
                    tmp8.xyz = tmp8.xyz * tmp1.www + tmp9.xyz;
                } else {
                    tmp8.xyz = tmp6.xyz;
                }
                tmp1.w = -tmp2.w * 0.7 + 1.7;
                tmp1.w = tmp1.w * tmp2.w;
                tmp1.w = tmp1.w * 6.0;
                tmp8 = UNITY_SAMPLE_TEXCUBE_SAMPLER(unity_SpecCube0, unity_SpecCube0, float4(tmp8.xyz, tmp1.w));
                tmp3.w = tmp8.w - 1.0;
                tmp3.w = unity_SpecCube0_HDR.w * tmp3.w + 1.0;
                tmp3.w = log(tmp3.w);
                tmp3.w = tmp3.w * unity_SpecCube0_HDR.y;
                tmp3.w = exp(tmp3.w);
                tmp3.w = tmp3.w * unity_SpecCube0_HDR.x;
                tmp9.xyz = tmp8.xyz * tmp3.www;
                tmp4.w = unity_SpecCube0_BoxMin.w < 0.99999;
                if (tmp4.w) {
                    tmp4.w = unity_SpecCube1_ProbePosition.w > 0.0;
                    if (tmp4.w) {
                        tmp4.w = dot(tmp6.xyz, tmp6.xyz);
                        tmp4.w = rsqrt(tmp4.w);
                        tmp10.xyz = tmp4.www * tmp6.xyz;
                        tmp11.xyz = unity_SpecCube1_BoxMax.xyz - tmp0.xyz;
                        tmp11.xyz = tmp11.xyz / tmp10.xyz;
                        tmp12.xyz = unity_SpecCube1_BoxMin.xyz - tmp0.xyz;
                        tmp12.xyz = tmp12.xyz / tmp10.xyz;
                        tmp13.xyz = tmp10.xyz > float3(0.0, 0.0, 0.0);
                        tmp11.xyz = tmp13.xyz ? tmp11.xyz : tmp12.xyz;
                        tmp4.w = min(tmp11.y, tmp11.x);
                        tmp4.w = min(tmp11.z, tmp4.w);
                        tmp0.xyz = tmp0.xyz - unity_SpecCube1_ProbePosition.xyz;
                        tmp6.xyz = tmp10.xyz * tmp4.www + tmp0.xyz;
                    }
                    tmp6 = UNITY_SAMPLE_TEXCUBE_SAMPLER(unity_SpecCube0, unity_SpecCube0, float4(tmp6.xyz, tmp1.w));
                    tmp0.x = tmp6.w - 1.0;
                    tmp0.x = unity_SpecCube1_HDR.w * tmp0.x + 1.0;
                    tmp0.x = log(tmp0.x);
                    tmp0.x = tmp0.x * unity_SpecCube1_HDR.y;
                    tmp0.x = exp(tmp0.x);
                    tmp0.x = tmp0.x * unity_SpecCube1_HDR.x;
                    tmp0.xyz = tmp6.xyz * tmp0.xxx;
                    tmp6.xyz = tmp3.www * tmp8.xyz + -tmp0.xyz;
                    tmp9.xyz = unity_SpecCube0_BoxMin.www * tmp6.xyz + tmp0.xyz;
                }
                tmp0.xyz = tmp4.xyz - float3(0.04, 0.04, 0.04);
                tmp0.xyz = _Metallic.xxx * tmp0.xyz + float3(0.04, 0.04, 0.04);
                tmp1.w = -_Metallic * 0.96 + 0.96;
                tmp4.xyz = tmp1.www * tmp4.xyz;
                tmp1.xyz = tmp1.xyz * tmp0.www + _WorldSpaceLightPos0.xyz;
                tmp0.w = dot(tmp1.xyz, tmp1.xyz);
                tmp0.w = max(tmp0.w, 0.001);
                tmp0.w = rsqrt(tmp0.w);
                tmp1.xyz = tmp0.www * tmp1.xyz;
                tmp0.w = dot(tmp5.xyz, tmp2.xyz);
                tmp2.x = saturate(dot(tmp5.xyz, _WorldSpaceLightPos0.xyz));
                tmp2.y = saturate(dot(tmp5.xyz, tmp1.xyz));
                tmp1.x = saturate(dot(_WorldSpaceLightPos0.xyz, tmp1.xyz));
                tmp1.y = tmp1.x * tmp1.x;
                tmp1.y = dot(tmp1.xy, tmp2.xy);
                tmp1.y = tmp1.y - 0.5;
                tmp1.z = 1.0 - tmp2.x;
                tmp2.z = tmp1.z * tmp1.z;
                tmp2.z = tmp2.z * tmp2.z;
                tmp1.z = tmp1.z * tmp2.z;
                tmp1.z = tmp1.y * tmp1.z + 1.0;
                tmp2.z = 1.0 - abs(tmp0.w);
                tmp3.w = tmp2.z * tmp2.z;
                tmp3.w = tmp3.w * tmp3.w;
                tmp2.z = tmp2.z * tmp3.w;
                tmp1.y = tmp1.y * tmp2.z + 1.0;
                tmp1.y = tmp1.y * tmp1.z;
                tmp1.y = tmp2.x * tmp1.y;
                tmp1.z = tmp2.w * tmp2.w;
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
                tmp2.x = dot(tmp0.xyz, tmp0.xyz);
                tmp2.x = tmp2.x != 0.0;
                tmp2.x = tmp2.x ? 1.0 : 0.0;
                tmp0.w = tmp0.w * tmp2.x;
                tmp1.w = 1.0 - tmp1.w;
                tmp1.w = saturate(tmp1.w + _Smoothness);
                tmp2.xyw = tmp1.yyy * tmp7.xyz;
                tmp5.xyz = tmp7.xyz * tmp0.www;
                tmp0.w = 1.0 - tmp1.x;
                tmp1.x = tmp0.w * tmp0.w;
                tmp1.x = tmp1.x * tmp1.x;
                tmp0.w = tmp0.w * tmp1.x;
                tmp6.xyz = float3(1.0, 1.0, 1.0) - tmp0.xyz;
                tmp6.xyz = tmp6.xyz * tmp0.www + tmp0.xyz;
                tmp5.xyz = tmp5.xyz * tmp6.xyz;
                tmp2.xyw = tmp4.xyz * tmp2.xyw + tmp5.xyz;
                tmp1.xyz = tmp9.xyz * tmp1.zzz;
                tmp4.xyz = tmp1.www - tmp0.xyz;
                tmp0.xyz = tmp2.zzz * tmp4.xyz + tmp0.xyz;
                tmp0.xyz = tmp1.xyz * tmp0.xyz + tmp2.xyw;
                o.sv_target.xyz = tmp3.xyz + tmp0.xyz;
                o.sv_target.w = 1.0;
                return o;
			}
			ENDCG
		}
		Pass {
			Name "FORWARD"
			Tags { "IGNOREPROJECTOR" = "true" "IsEmissive" = "true" "LIGHTMODE" = "FORWARDADD" "QUEUE" = "AlphaTest+0" "RenderType" = "TransparentCutout" "SHADOWSUPPORT" = "true" }
			Blend One One, One One
			ZWrite Off
			GpuProgramID 104126
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float2 texcoord5 : TEXCOORD5;
				float3 texcoord1 : TEXCOORD1;
				float3 texcoord2 : TEXCOORD2;
				float3 texcoord3 : TEXCOORD3;
				float3 texcoord4 : TEXCOORD4;
				float3 texcoord6 : TEXCOORD6;
				float4 texcoord7 : TEXCOORD7;
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
			float4 _Texture2_ST;
			float _Texture2Height;
			float4 _Texture1_ST;
			float _Texture1Height;
			float _Health;
			float _NoiseMax;
			float _Metallic;
			float _Smoothness;
			float4 _Noise_ST;
			float4 _Crack_ST;
			float _MaskClipValue;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _Texture2;
			sampler2D _Texture2Normal;
			sampler2D _Texture1Normal;
			sampler2D _Noise;
			sampler2D _Texture1;
			sampler2D _Crack;
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
                o.texcoord5.xy = v.texcoord.xy;
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
                o.texcoord6.xyz = unity_WorldToLight._m03_m13_m23 * tmp0.www + tmp0.xyz;
                o.texcoord7 = float4(0.0, 0.0, 0.0, 0.0);
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
                tmp3.xy = tmp2.yy * inp.texcoord2.xy;
                tmp3.xy = inp.texcoord1.xy * tmp2.xx + tmp3.xy;
                tmp3.xy = inp.texcoord3.xy * tmp2.zz + tmp3.xy;
                tmp3.zw = inp.texcoord.xy * _Texture2_ST.xy + _Texture2_ST.zw;
                tmp1.w = _Texture2Height - 1.0;
                tmp3.zw = tmp1.ww * tmp3.xy + tmp3.zw;
                tmp4 = tex2D(_Texture2, tmp3.zw);
                tmp5.xy = inp.texcoord.xy * _Texture1_ST.xy + _Texture1_ST.zw;
                tmp1.w = _Texture1Height - 1.0;
                tmp3.xy = tmp1.ww * tmp3.xy + tmp5.xy;
                tmp5 = tex2D(_Texture2Normal, tmp3.zw);
                tmp5.x = tmp5.w * tmp5.x;
                tmp5.xy = tmp5.xy * float2(2.0, 2.0) + float2(-1.0, -1.0);
                tmp1.w = dot(tmp5.xy, tmp5.xy);
                tmp1.w = min(tmp1.w, 1.0);
                tmp1.w = 1.0 - tmp1.w;
                tmp5.z = sqrt(tmp1.w);
                tmp1.w = 1.0 - tmp4.w;
                tmp6 = tex2D(_Texture1Normal, tmp3.xy);
                tmp6.x = tmp6.w * tmp6.x;
                tmp6.xy = tmp6.xy * float2(2.0, 2.0) + float2(-1.0, -1.0);
                tmp2.w = dot(tmp6.xy, tmp6.xy);
                tmp2.w = min(tmp2.w, 1.0);
                tmp2.w = 1.0 - tmp2.w;
                tmp6.z = sqrt(tmp2.w);
                tmp6.xyz = tmp1.www * tmp6.xyz;
                tmp5.xyz = tmp5.xyz * tmp4.www + tmp6.xyz;
                tmp6 = tex2D(_Noise, tmp3.zw);
                tmp2.w = tmp6.x + 0.1;
                tmp2.w = saturate(tmp2.w * 0.9090909);
                tmp6 = tex2D(_Texture1, tmp3.xy);
                tmp7 = tex2D(_Noise, tmp3.xy);
                tmp7.yz = _Health.xx * _NoiseMax.xx + float2(-0.1, -0.05);
                tmp5.w = tmp7.x - tmp7.y;
                tmp7.xy = _Health.xx * _NoiseMax.xx + -tmp7.yz;
                tmp5.w = saturate(tmp5.w / tmp7.x);
                tmp4.xyz = tmp4.www * tmp4.xyz;
                tmp8 = tex2D(_Crack, tmp3.zw);
                tmp8.xyz = float3(1.0, 1.0, 1.0) - tmp8.xyz;
                tmp4.xyz = tmp4.xyz * tmp8.xyz;
                tmp3 = tex2D(_Crack, tmp3.xy);
                tmp3.xyz = float3(1.0, 1.0, 1.0) - tmp3.xyz;
                tmp3.xyz = tmp3.xyz * tmp6.xyz;
                tmp3.xyz = tmp5.www * tmp3.xyz;
                tmp3.xyz = tmp1.www * tmp3.xyz;
                tmp3.xyz = tmp4.xyz * tmp2.www + tmp3.xyz;
                tmp4.xy = inp.texcoord.xy * _Noise_ST.xy + _Noise_ST.zw;
                tmp4 = tex2D(_Noise, tmp4.xy);
                tmp1.w = tmp4.x - tmp7.z;
                tmp1.w = saturate(tmp1.w / tmp7.y);
                tmp4.xy = inp.texcoord.xy * _Crack_ST.xy + _Crack_ST.zw;
                tmp4 = tex2D(_Crack, tmp4.xy);
                tmp1.w = saturate(tmp1.w - tmp4.x);
                tmp1.w = tmp1.w - _MaskClipValue;
                tmp1.w = tmp1.w < 0.0;
                if (tmp1.w) {
                    discard;
                }
                tmp4.xyz = inp.texcoord4.yyy * unity_WorldToLight._m01_m11_m21;
                tmp4.xyz = unity_WorldToLight._m00_m10_m20 * inp.texcoord4.xxx + tmp4.xyz;
                tmp4.xyz = unity_WorldToLight._m02_m12_m22 * inp.texcoord4.zzz + tmp4.xyz;
                tmp4.xyz = tmp4.xyz + unity_WorldToLight._m03_m13_m23;
                tmp1.w = unity_ProbeVolumeParams.x == 1.0;
                if (tmp1.w) {
                    tmp1.w = unity_ProbeVolumeParams.y == 1.0;
                    tmp6.xyz = inp.texcoord4.yyy * unity_ProbeVolumeWorldToObject._m01_m11_m21;
                    tmp6.xyz = unity_ProbeVolumeWorldToObject._m00_m10_m20 * inp.texcoord4.xxx + tmp6.xyz;
                    tmp6.xyz = unity_ProbeVolumeWorldToObject._m02_m12_m22 * inp.texcoord4.zzz + tmp6.xyz;
                    tmp6.xyz = tmp6.xyz + unity_ProbeVolumeWorldToObject._m03_m13_m23;
                    tmp6.xyz = tmp1.www ? tmp6.xyz : inp.texcoord4.xyz;
                    tmp6.xyz = tmp6.xyz - unity_ProbeVolumeMin;
                    tmp6.yzw = tmp6.xyz * unity_ProbeVolumeSizeInv;
                    tmp1.w = tmp6.y * 0.25 + 0.75;
                    tmp2.w = unity_ProbeVolumeParams.z * 0.5 + 0.75;
                    tmp6.x = max(tmp1.w, tmp2.w);
                    tmp6 = UNITY_SAMPLE_TEX3D_SAMPLER(unity_ProbeVolumeSH, unity_ProbeVolumeSH, tmp6.xzw);
                } else {
                    tmp6 = float4(1.0, 1.0, 1.0, 1.0);
                }
                tmp1.w = saturate(dot(tmp6, unity_OcclusionMaskSelector));
                tmp2.w = dot(tmp4.xyz, tmp4.xyz);
                tmp4 = tex2D(_LightTexture0, tmp2.ww);
                tmp1.w = tmp1.w * tmp4.x;
                tmp4.x = dot(inp.texcoord1.xyz, tmp5.xyz);
                tmp4.y = dot(inp.texcoord2.xyz, tmp5.xyz);
                tmp4.z = dot(inp.texcoord3.xyz, tmp5.xyz);
                tmp2.w = dot(tmp4.xyz, tmp4.xyz);
                tmp2.w = rsqrt(tmp2.w);
                tmp4.xyz = tmp2.www * tmp4.xyz;
                tmp5.xyz = tmp1.www * _LightColor0.xyz;
                tmp6.xyz = tmp3.xyz - float3(0.04, 0.04, 0.04);
                tmp6.xyz = _Metallic.xxx * tmp6.xyz + float3(0.04, 0.04, 0.04);
                tmp1.w = -_Metallic * 0.96 + 0.96;
                tmp3.xyz = tmp1.www * tmp3.xyz;
                tmp1.w = 1.0 - _Smoothness;
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
                tmp0.w = dot(tmp6.xyz, tmp6.xyz);
                tmp0.w = tmp0.w != 0.0;
                tmp0.w = tmp0.w ? 1.0 : 0.0;
                tmp0.z = tmp0.w * tmp0.z;
                tmp1.xyz = tmp0.yyy * tmp5.xyz;
                tmp0.yzw = tmp5.xyz * tmp0.zzz;
                tmp0.x = 1.0 - tmp0.x;
                tmp1.w = tmp0.x * tmp0.x;
                tmp1.w = tmp1.w * tmp1.w;
                tmp0.x = tmp0.x * tmp1.w;
                tmp2.xyz = float3(1.0, 1.0, 1.0) - tmp6.xyz;
                tmp2.xyz = tmp2.xyz * tmp0.xxx + tmp6.xyz;
                tmp0.xyz = tmp0.yzw * tmp2.xyz;
                o.sv_target.xyz = tmp3.xyz * tmp1.xyz + tmp0.xyz;
                o.sv_target.w = 1.0;
                return o;
			}
			ENDCG
		}
		Pass {
			Name "DEFERRED"
			Tags { "IGNOREPROJECTOR" = "true" "IsEmissive" = "true" "LIGHTMODE" = "DEFERRED" "QUEUE" = "AlphaTest+0" "RenderType" = "TransparentCutout" }
			GpuProgramID 133463
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float2 texcoord4 : TEXCOORD4;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 texcoord3 : TEXCOORD3;
				float3 texcoord5 : TEXCOORD5;
				float4 texcoord6 : TEXCOORD6;
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
			// $Globals ConstantBuffers for Fragment Shader
			float4 _Texture2_ST;
			float _Texture2Height;
			float4 _Texture1_ST;
			float _Texture1Height;
			float _Health;
			float _NoiseMax;
			float _SystemDamage;
			float _LightningBrightness;
			float _Texture1Emission;
			float4 _Texture1EmissionColor;
			float _Metallic;
			float _Smoothness;
			float4 _Noise_ST;
			float4 _Crack_ST;
			float _MaskClipValue;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _Texture2;
			sampler2D _Texture2Normal;
			sampler2D _Texture1Normal;
			sampler2D _Noise;
			sampler2D _Texture1;
			sampler2D _Crack;
			sampler2D _Lightning;
			sampler2D _Texture1EmissionMask;

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
                o.texcoord4.xy = v.texcoord.xy;
                o.texcoord1.w = tmp0.x;
                tmp0.w = v.tangent.w * unity_WorldTransformParams.w;
                tmp1.x = dot(v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp1.y = dot(v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp1.z = dot(v.normal.xyz, unity_WorldToObject._m02_m12_m22);
                tmp1.w = dot(tmp1.xyz, tmp1.xyz);
                tmp1.w = rsqrt(tmp1.w);
                tmp1.xyz = tmp1.www * tmp1.xyz;
                tmp2.xyz = v.tangent.yyy * unity_ObjectToWorld._m01_m11_m21;
                tmp2.xyz = unity_ObjectToWorld._m00_m10_m20 * v.tangent.xxx + tmp2.xyz;
                tmp2.xyz = unity_ObjectToWorld._m02_m12_m22 * v.tangent.zzz + tmp2.xyz;
                tmp1.w = dot(tmp2.xyz, tmp2.xyz);
                tmp1.w = rsqrt(tmp1.w);
                tmp2.xyz = tmp1.www * tmp2.xyz;
                tmp3.xyz = tmp1.zxy * tmp2.yzx;
                tmp3.xyz = tmp1.yzx * tmp2.zxy + -tmp3.xyz;
                tmp3.xyz = tmp0.www * tmp3.xyz;
                o.texcoord1.y = tmp3.x;
                o.texcoord1.x = tmp2.x;
                o.texcoord1.z = tmp1.x;
                o.texcoord2.x = tmp2.y;
                o.texcoord2.z = tmp1.y;
                o.texcoord2.w = tmp0.y;
                o.texcoord2.y = tmp3.y;
                o.texcoord3.x = tmp2.z;
                o.texcoord3.z = tmp1.z;
                o.texcoord3.w = tmp0.z;
                tmp0.xyz = _WorldSpaceCameraPos - tmp0.xyz;
                o.texcoord3.y = tmp3.z;
                o.texcoord5.y = dot(tmp0.xyz, tmp3.xyz);
                o.texcoord5.x = dot(tmp0.xyz, tmp2.xyz);
                o.texcoord5.z = dot(tmp0.xyz, tmp1.xyz);
                o.texcoord6 = float4(0.0, 0.0, 0.0, 0.0);
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
                tmp0.xy = inp.texcoord.xy * _Crack_ST.xy + _Crack_ST.zw;
                tmp0 = tex2D(_Crack, tmp0.xy);
                tmp0.yz = inp.texcoord.xy * _Noise_ST.xy + _Noise_ST.zw;
                tmp1 = tex2D(_Noise, tmp0.yz);
                tmp0.yz = _Health.xx * _NoiseMax.xx + float2(-0.1, -0.05);
                tmp0.w = tmp1.x - tmp0.z;
                tmp1.xy = _Health.xx * _NoiseMax.xx + -tmp0.yz;
                tmp0.z = saturate(tmp0.w / tmp1.y);
                tmp0.x = saturate(tmp0.z - tmp0.x);
                tmp0.x = tmp0.x - _MaskClipValue;
                tmp0.x = tmp0.x < 0.0;
                if (tmp0.x) {
                    discard;
                }
                tmp0.x = dot(inp.texcoord5.xyz, inp.texcoord5.xyz);
                tmp0.x = rsqrt(tmp0.x);
                tmp0.xz = tmp0.xx * inp.texcoord5.xy;
                tmp1.yz = inp.texcoord.xy * _Texture1_ST.xy + _Texture1_ST.zw;
                tmp0.w = _Texture1Height - 1.0;
                tmp1.yz = tmp0.ww * tmp0.xz + tmp1.yz;
                tmp2 = tex2D(_Noise, tmp1.yz);
                tmp0.y = tmp2.x - tmp0.y;
                tmp0.y = saturate(tmp0.y / tmp1.x);
                tmp2 = tex2D(_Crack, tmp1.yz);
                tmp2.xyz = float3(1.0, 1.0, 1.0) - tmp2.xyz;
                tmp3 = tex2D(_Texture1, tmp1.yz);
                tmp2.xyz = tmp2.xyz * tmp3.xyz;
                tmp2.xyz = tmp0.yyy * tmp2.xyz;
                tmp0.yw = inp.texcoord.xy * _Texture2_ST.xy + _Texture2_ST.zw;
                tmp1.x = _Texture2Height - 1.0;
                tmp0.xy = tmp1.xx * tmp0.xz + tmp0.yw;
                tmp4 = tex2D(_Texture2, tmp0.xy);
                tmp0.z = 1.0 - tmp4.w;
                tmp2.xyz = tmp2.xyz * tmp0.zzz;
                tmp5 = tex2D(_Noise, tmp0.xy);
                tmp0.w = tmp5.x + 0.1;
                tmp0.w = saturate(tmp0.w * 0.9090909);
                tmp5 = tex2D(_Crack, tmp0.xy);
                tmp6 = tex2D(_Texture2Normal, tmp0.xy);
                tmp5.xyz = float3(1.0, 1.0, 1.0) - tmp5.xyz;
                tmp4.xyz = tmp4.www * tmp4.xyz;
                tmp4.xyz = tmp5.xyz * tmp4.xyz;
                tmp0.xyw = tmp4.xyz * tmp0.www + tmp2.xyz;
                tmp1.x = -_Metallic * 0.96 + 0.96;
                o.sv_target.xyz = tmp0.xyw * tmp1.xxx;
                tmp0.xyw = tmp0.xyw - float3(0.04, 0.04, 0.04);
                o.sv_target1.xyz = _Metallic.xxx * tmp0.xyw + float3(0.04, 0.04, 0.04);
                o.sv_target.w = 1.0;
                o.sv_target1.w = _Smoothness;
                tmp6.x = tmp6.w * tmp6.x;
                tmp2.xy = tmp6.xy * float2(2.0, 2.0) + float2(-1.0, -1.0);
                tmp0.x = dot(tmp2.xy, tmp2.xy);
                tmp0.x = min(tmp0.x, 1.0);
                tmp0.x = 1.0 - tmp0.x;
                tmp2.z = sqrt(tmp0.x);
                tmp5 = tex2D(_Texture1Normal, tmp1.yz);
                tmp1 = tex2D(_Texture1EmissionMask, tmp1.yz);
                tmp0.xyw = tmp1.xxx * tmp3.xyz;
                tmp0.xyw = tmp0.xyw * _Texture1Emission.xxx;
                tmp0.xyw = tmp0.xyw * _Texture1EmissionColor.xyz;
                tmp5.x = tmp5.w * tmp5.x;
                tmp1.xy = tmp5.xy * float2(2.0, 2.0) + float2(-1.0, -1.0);
                tmp1.w = dot(tmp1.xy, tmp1.xy);
                tmp1.w = min(tmp1.w, 1.0);
                tmp1.w = 1.0 - tmp1.w;
                tmp1.z = sqrt(tmp1.w);
                tmp1.xyz = tmp0.zzz * tmp1.xyz;
                tmp1.xyz = tmp2.xyz * tmp4.www + tmp1.xyz;
                tmp2.x = dot(inp.texcoord1.xyz, tmp1.xyz);
                tmp2.y = dot(inp.texcoord2.xyz, tmp1.xyz);
                tmp2.z = dot(inp.texcoord3.xyz, tmp1.xyz);
                tmp0.z = dot(tmp2.xyz, tmp2.xyz);
                tmp0.z = rsqrt(tmp0.z);
                tmp1.xyz = tmp0.zzz * tmp2.xyz;
                o.sv_target2.xyz = tmp1.xyz * float3(0.5, 0.5, 0.5) + float3(0.5, 0.5, 0.5);
                o.sv_target2.w = 1.0;
                tmp1.xy = _SystemDamage.xx * float2(0.8, 0.5) + float2(0.2, 0.5);
                tmp0.z = tmp1.x * _Time.y;
                tmp0.z = tmp0.z * 0.78125;
                tmp1.x = tmp0.z >= -tmp0.z;
                tmp0.z = frac(abs(tmp0.z));
                tmp0.z = tmp1.x ? tmp0.z : -tmp0.z;
                tmp0.z = tmp0.z * 64.0;
                tmp0.z = round(tmp0.z);
                tmp1.x = tmp0.z < 0.0;
                tmp1.x = tmp1.x ? 64.0 : 0.0;
                tmp0.z = tmp0.z + tmp1.x;
                tmp1.x = tmp0.z * 0.125;
                tmp1.z = tmp1.x >= -tmp1.x;
                tmp1.x = frac(abs(tmp1.x));
                tmp1.x = tmp1.z ? tmp1.x : -tmp1.x;
                tmp1.x = tmp1.x * 8.0;
                tmp1.x = round(tmp1.x);
                tmp0.z = tmp0.z - tmp1.x;
                tmp2.x = tmp1.x * 0.125;
                tmp0.z = tmp0.z * 0.015625;
                tmp1.x = tmp0.z >= -tmp0.z;
                tmp0.z = frac(abs(tmp0.z));
                tmp0.z = tmp1.x ? tmp0.z : -tmp0.z;
                tmp0.z = tmp0.z * 8.0;
                tmp0.z = round(tmp0.z);
                tmp0.z = 7.0 - tmp0.z;
                tmp2.y = tmp0.z * 0.125;
                tmp1.xz = inp.texcoord4.xy * float2(0.125, 0.125) + tmp2.xy;
                tmp2 = tex2D(_Lightning, tmp1.xz);
                tmp1.xzw = tmp2.xyz * _LightningBrightness.xxx;
                tmp0.z = _SystemDamage > 0.2;
                tmp0.z = tmp0.z ? tmp1.y : 0.0;
                tmp0.xyz = tmp1.xzw * tmp0.zzz + tmp0.xyw;
                o.sv_target3.xyz = exp(-tmp0.xyz);
                o.sv_target3.w = 1.0;
                return o;
			}
			ENDCG
		}
		Pass {
			Name "Meta"
			Tags { "IGNOREPROJECTOR" = "true" "IsEmissive" = "true" "LIGHTMODE" = "META" "QUEUE" = "AlphaTest+0" "RenderType" = "TransparentCutout" }
			Cull Off
			GpuProgramID 229507
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float2 texcoord4 : TEXCOORD4;
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
			// $Globals ConstantBuffers for Fragment Shader
			float4 _Texture2_ST;
			float _Texture2Height;
			float4 _Texture1_ST;
			float _Texture1Height;
			float _Health;
			float _NoiseMax;
			float _SystemDamage;
			float _LightningBrightness;
			float _Texture1Emission;
			float4 _Texture1EmissionColor;
			float4 _Noise_ST;
			float4 _Crack_ST;
			float _MaskClipValue;
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
			sampler2D _Texture2;
			sampler2D _Noise;
			sampler2D _Texture1;
			sampler2D _Crack;
			sampler2D _Lightning;
			sampler2D _Texture1EmissionMask;

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
                o.texcoord4.xy = v.texcoord.xy;
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
                tmp0.xy = inp.texcoord.xy * _Crack_ST.xy + _Crack_ST.zw;
                tmp0 = tex2D(_Crack, tmp0.xy);
                tmp0.yz = inp.texcoord.xy * _Noise_ST.xy + _Noise_ST.zw;
                tmp1 = tex2D(_Noise, tmp0.yz);
                tmp0.yz = _Health.xx * _NoiseMax.xx + float2(-0.1, -0.05);
                tmp0.w = tmp1.x - tmp0.z;
                tmp1.xy = _Health.xx * _NoiseMax.xx + -tmp0.yz;
                tmp0.z = saturate(tmp0.w / tmp1.y);
                tmp0.x = saturate(tmp0.z - tmp0.x);
                tmp0.x = tmp0.x - _MaskClipValue;
                tmp0.x = tmp0.x < 0.0;
                if (tmp0.x) {
                    discard;
                }
                tmp2.x = inp.texcoord1.w;
                tmp2.y = inp.texcoord2.w;
                tmp2.z = inp.texcoord3.w;
                tmp0.xzw = _WorldSpaceCameraPos - tmp2.xyz;
                tmp1.y = dot(tmp0.xyz, tmp0.xyz);
                tmp1.y = rsqrt(tmp1.y);
                tmp0.xzw = tmp0.xzw * tmp1.yyy;
                tmp1.yz = tmp0.zz * inp.texcoord2.xy;
                tmp0.xz = inp.texcoord1.xy * tmp0.xx + tmp1.yz;
                tmp0.xz = inp.texcoord3.xy * tmp0.ww + tmp0.xz;
                tmp1.yz = inp.texcoord.xy * _Texture1_ST.xy + _Texture1_ST.zw;
                tmp0.w = _Texture1Height - 1.0;
                tmp1.yz = tmp0.ww * tmp0.xz + tmp1.yz;
                tmp2 = tex2D(_Noise, tmp1.yz);
                tmp0.y = tmp2.x - tmp0.y;
                tmp0.y = saturate(tmp0.y / tmp1.x);
                tmp2 = tex2D(_Crack, tmp1.yz);
                tmp2.xyz = float3(1.0, 1.0, 1.0) - tmp2.xyz;
                tmp3 = tex2D(_Texture1, tmp1.yz);
                tmp1 = tex2D(_Texture1EmissionMask, tmp1.yz);
                tmp1.xyz = tmp1.xxx * tmp3.xyz;
                tmp2.xyz = tmp2.xyz * tmp3.xyz;
                tmp2.xyz = tmp0.yyy * tmp2.xyz;
                tmp1.xyz = tmp1.xyz * _Texture1Emission.xxx;
                tmp1.xyz = tmp1.xyz * _Texture1EmissionColor.xyz;
                tmp0.yw = inp.texcoord.xy * _Texture2_ST.xy + _Texture2_ST.zw;
                tmp1.w = _Texture2Height - 1.0;
                tmp0.xy = tmp1.ww * tmp0.xz + tmp0.yw;
                tmp3 = tex2D(_Texture2, tmp0.xy);
                tmp0.z = 1.0 - tmp3.w;
                tmp3.xyz = tmp3.www * tmp3.xyz;
                tmp2.xyz = tmp2.xyz * tmp0.zzz;
                tmp4 = tex2D(_Crack, tmp0.xy);
                tmp0 = tex2D(_Noise, tmp0.xy);
                tmp0.x = tmp0.x + 0.1;
                tmp0.x = saturate(tmp0.x * 0.9090909);
                tmp0.yzw = float3(1.0, 1.0, 1.0) - tmp4.xyz;
                tmp0.yzw = tmp0.yzw * tmp3.xyz;
                tmp0.xyz = tmp0.yzw * tmp0.xxx + tmp2.xyz;
                tmp0.xyz = log(tmp0.xyz);
                tmp0.w = saturate(unity_OneOverOutputBoost);
                tmp0.xyz = tmp0.xyz * tmp0.www;
                tmp0.xyz = exp(tmp0.xyz);
                tmp0.xyz = min(tmp0.xyz, unity_MaxOutputValue.xxx);
                tmp0.w = 1.0;
                tmp0 = unity_MetaFragmentControl ? tmp0 : float4(0.0, 0.0, 0.0, 0.0);
                tmp2.xy = _SystemDamage.xx * float2(0.8, 0.5) + float2(0.2, 0.5);
                tmp1.w = tmp2.x * _Time.y;
                tmp1.w = tmp1.w * 0.78125;
                tmp2.x = tmp1.w >= -tmp1.w;
                tmp1.w = frac(abs(tmp1.w));
                tmp1.w = tmp2.x ? tmp1.w : -tmp1.w;
                tmp1.w = tmp1.w * 64.0;
                tmp1.w = round(tmp1.w);
                tmp2.x = tmp1.w < 0.0;
                tmp2.x = tmp2.x ? 64.0 : 0.0;
                tmp1.w = tmp1.w + tmp2.x;
                tmp2.x = tmp1.w * 0.125;
                tmp2.z = tmp2.x >= -tmp2.x;
                tmp2.x = frac(abs(tmp2.x));
                tmp2.x = tmp2.z ? tmp2.x : -tmp2.x;
                tmp2.x = tmp2.x * 8.0;
                tmp2.x = round(tmp2.x);
                tmp1.w = tmp1.w - tmp2.x;
                tmp3.x = tmp2.x * 0.125;
                tmp1.w = tmp1.w * 0.015625;
                tmp2.x = tmp1.w >= -tmp1.w;
                tmp1.w = frac(abs(tmp1.w));
                tmp1.w = tmp2.x ? tmp1.w : -tmp1.w;
                tmp1.w = tmp1.w * 8.0;
                tmp1.w = round(tmp1.w);
                tmp1.w = 7.0 - tmp1.w;
                tmp3.y = tmp1.w * 0.125;
                tmp2.xz = inp.texcoord4.xy * float2(0.125, 0.125) + tmp3.xy;
                tmp3 = tex2D(_Lightning, tmp2.xz);
                tmp2.xzw = tmp3.xyz * _LightningBrightness.xxx;
                tmp1.w = _SystemDamage > 0.2;
                tmp1.w = tmp1.w ? tmp2.y : 0.0;
                tmp1.xyz = tmp2.xzw * tmp1.www + tmp1.xyz;
                tmp2.xyz = tmp1.xyz * float3(0.305306, 0.305306, 0.305306) + float3(0.6821711, 0.6821711, 0.6821711);
                tmp2.xyz = tmp1.xyz * tmp2.xyz + float3(0.0125229, 0.0125229, 0.0125229);
                tmp2.xyz = tmp1.xyz * tmp2.xyz;
                tmp1.w = unity_UseLinearSpace != 0.0;
                tmp1.xyz = tmp1.www ? tmp1.xyz : tmp2.xyz;
                tmp1.w = 1.0;
                o.sv_target = unity_MetaFragmentControl ? tmp1 : tmp0;
                return o;
			}
			ENDCG
		}
		Pass {
			Name "ShadowCaster"
			Tags { "IGNOREPROJECTOR" = "true" "IsEmissive" = "true" "LIGHTMODE" = "SHADOWCASTER" "QUEUE" = "AlphaTest+0" "RenderType" = "TransparentCutout" "SHADOWSUPPORT" = "true" }
			GpuProgramID 311464
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
			float _Health;
			float _NoiseMax;
			float4 _Noise_ST;
			float4 _Crack_ST;
			float _MaskClipValue;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _Noise;
			sampler2D _Crack;

			// Keywords: SHADOWS_DEPTH UNITY_PASS_SHADOWCASTER
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
                tmp0 = unity_ObjectToWorld._m03_m13_m23_m33 * v.vertex.wwww + tmp0;
                tmp1.xyz = -tmp0.xyz * _WorldSpaceLightPos0.www + _WorldSpaceLightPos0.xyz;
                tmp1.w = dot(tmp1.xyz, tmp1.xyz);
                tmp1.w = rsqrt(tmp1.w);
                tmp1.xyz = tmp1.www * tmp1.xyz;
                tmp2.x = dot(v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp2.y = dot(v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp2.z = dot(v.normal.xyz, unity_WorldToObject._m02_m12_m22);
                tmp1.w = dot(tmp2.xyz, tmp2.xyz);
                tmp1.w = rsqrt(tmp1.w);
                tmp2.xyz = tmp1.www * tmp2.xyz;
                tmp1.x = dot(tmp2.xyz, tmp1.xyz);
                tmp1.x = -tmp1.x * tmp1.x + 1.0;
                tmp1.x = sqrt(tmp1.x);
                tmp1.x = tmp1.x * unity_LightShadowBias.z;
                tmp1.xyz = -tmp2.xyz * tmp1.xxx + tmp0.xyz;
                tmp1.w = unity_LightShadowBias.z != 0.0;
                tmp0.xyz = tmp1.www ? tmp1.xyz : tmp0.xyz;
                tmp1 = tmp0.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp1 = unity_MatrixVP._m00_m10_m20_m30 * tmp0.xxxx + tmp1;
                tmp1 = unity_MatrixVP._m02_m12_m22_m32 * tmp0.zzzz + tmp1;
                tmp0 = unity_MatrixVP._m03_m13_m23_m33 * tmp0.wwww + tmp1;
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
                tmp0.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp0.xyz;
                o.texcoord6.xyz = tmp0.xyz;
                o.texcoord1.w = tmp0.x;
                tmp1.xyz = v.tangent.yyy * unity_ObjectToWorld._m11_m21_m01;
                tmp1.xyz = unity_ObjectToWorld._m10_m20_m00 * v.tangent.xxx + tmp1.xyz;
                tmp1.xyz = unity_ObjectToWorld._m12_m22_m02 * v.tangent.zzz + tmp1.xyz;
                tmp0.x = dot(tmp1.xyz, tmp1.xyz);
                tmp0.x = rsqrt(tmp0.x);
                tmp1.xyz = tmp0.xxx * tmp1.xyz;
                tmp3.xyz = tmp1.xyz * tmp2.zxy;
                tmp3.xyz = tmp2.yzx * tmp1.yzx + -tmp3.xyz;
                tmp0.x = v.tangent.w * unity_WorldTransformParams.w;
                tmp3.xyz = tmp0.xxx * tmp3.xyz;
                o.texcoord1.y = tmp3.x;
                o.texcoord1.z = tmp2.x;
                o.texcoord1.x = tmp1.z;
                o.texcoord2.x = tmp1.x;
                o.texcoord3.x = tmp1.y;
                o.texcoord2.z = tmp2.y;
                o.texcoord3.z = tmp2.z;
                o.texcoord2.w = tmp0.y;
                o.texcoord3.w = tmp0.z;
                o.texcoord2.y = tmp3.y;
                o.texcoord3.y = tmp3.z;
                o.texcoord4.xy = v.texcoord.xy;
                o.texcoord4.zw = v.texcoord1.xy;
                return o;
			}
			// Keywords: SHADOWS_DEPTH UNITY_PASS_SHADOWCASTER
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                tmp0.xy = inp.texcoord4.xy * _Noise_ST.xy + _Noise_ST.zw;
                tmp0 = tex2D(_Noise, tmp0.xy);
                tmp0.y = _Health * _NoiseMax + -0.05;
                tmp0.x = tmp0.x - tmp0.y;
                tmp0.y = _Health * _NoiseMax + -tmp0.y;
                tmp0.x = saturate(tmp0.x / tmp0.y);
                tmp0.yz = inp.texcoord4.xy * _Crack_ST.xy + _Crack_ST.zw;
                tmp1 = tex2D(_Crack, tmp0.yz);
                tmp0.x = saturate(tmp0.x - tmp1.x);
                tmp0.x = tmp0.x - _MaskClipValue;
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
	CustomEditor "ASEMaterialInspector"
}
