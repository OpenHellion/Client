Shader "ZeroGravity/Planet/AtmosphereShader" {
	Properties {
		_fresnel ("fresnel", Float) = 1
		_FresnelPower ("FresnelPower", Float) = 1
		_color1 ("color1", Vector) = (1,1,1,1)
		_color2 ("color2", Vector) = (1,1,1,1)
		_AtmosphereNoise ("AtmosphereNoise", 2D) = "white" {}
		_ColorPower ("ColorPower", Float) = 1
		_ColorSlide ("ColorSlide", Range(0, 1)) = 0.5
		_AtmosphereLightPower ("AtmosphereLightPower", Float) = 1
		_Opacity ("Opacity", Float) = 1
		[HideInInspector] _Cutoff ("Alpha cutoff", Range(0, 1)) = 0.5
	}
	SubShader {
		LOD 100
		Tags { "IGNOREPROJECTOR" = "true" "QUEUE" = "Transparent+1" "RenderType" = "Transparent" }
		Pass {
			Name "FORWARD"
			LOD 100
			Tags { "IGNOREPROJECTOR" = "true" "LIGHTMODE" = "FORWARDBASE" "QUEUE" = "Transparent+1" "RenderType" = "Transparent" "SHADOWSUPPORT" = "true" }
			Blend SrcAlpha OneMinusSrcAlpha, SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			Cull Front
			GpuProgramID 13134
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
			float4 _TimeEditor;
			float _fresnel;
			float4 _color1;
			float _FresnelPower;
			float4 _AtmosphereNoise_ST;
			float4 _color2;
			float _ColorPower;
			float _ColorSlide;
			float _AtmosphereLightPower;
			float _Opacity;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _AtmosphereNoise;

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
                tmp0.x = dot(-v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp0.y = dot(-v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp0.z = dot(-v.normal.xyz, unity_WorldToObject._m02_m12_m22);
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
                tmp0.xyz = _WorldSpaceCameraPos - inp.texcoord1.xyz;
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                tmp0.w = dot(inp.texcoord2.xyz, inp.texcoord2.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp1.xyz = tmp0.www * inp.texcoord2.xyz;
                tmp0.x = dot(tmp1.xyz, tmp0.xyz);
                tmp0.x = max(tmp0.x, 0.0);
                tmp0.x = 1.0 - tmp0.x;
                tmp0.x = log(tmp0.x);
                tmp0.y = tmp0.x * _ColorPower;
                tmp0.x = tmp0.x * _fresnel;
                tmp0.x = exp(tmp0.x);
                tmp0.x = min(tmp0.x, 1.0);
                tmp0.x = 1.0 - tmp0.x;
                tmp0.x = log(tmp0.x);
                tmp0.x = tmp0.x * _FresnelPower;
                tmp0.x = exp(tmp0.x);
                tmp0.y = exp(tmp0.y);
                tmp2.xyz = _color2.xyz - _color1.xyz;
                tmp0.yzw = tmp0.yyy * tmp2.xyz;
                tmp0.yzw = tmp0.yzw / _ColorSlide.xxx;
                tmp0.yzw = tmp0.yzw + _color1.xyz;
                tmp1.w = dot(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz);
                tmp1.w = rsqrt(tmp1.w);
                tmp2.xyz = tmp1.www * _WorldSpaceLightPos0.xyz;
                tmp1.x = saturate(dot(tmp1.xyz, tmp2.xyz));
                tmp1.x = 1.0 - tmp1.x;
                tmp1.x = log(tmp1.x);
                tmp1.x = tmp1.x * _AtmosphereLightPower;
                tmp1.x = exp(tmp1.x);
                tmp0.yzw = tmp0.yzw * tmp1.xxx;
                tmp0.yzw = max(tmp0.yzw, float3(0.0, 0.0, 0.0));
                o.sv_target.xyz = min(tmp0.yzw, float3(5.0, 5.0, 5.0));
                tmp0.y = _TimeEditor.y + _Time.y;
                tmp0.yz = tmp0.yy * float2(0.005, 0.005) + inp.texcoord.xy;
                tmp0.yz = tmp0.yz * _AtmosphereNoise_ST.xy + _AtmosphereNoise_ST.zw;
                tmp1 = tex2D(_AtmosphereNoise, tmp0.yz);
                tmp0.yz = tmp1.xy * float2(0.3, 0.3) + float2(0.7, 0.7);
                tmp0.x = tmp0.x * tmp0.y;
                tmp0.x = tmp0.z * tmp0.x;
                o.sv_target.w = saturate(tmp0.x * _Opacity);
                return o;
			}
			ENDCG
		}
		Pass {
			Name "FORWARD_DELTA"
			LOD 100
			Tags { "IGNOREPROJECTOR" = "true" "LIGHTMODE" = "FORWARDADD" "QUEUE" = "Transparent+1" "RenderType" = "Transparent" }
			Blend One One, One One
			ZWrite Off
			Cull Front
			GpuProgramID 87373
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
				float3 texcoord3 : TEXCOORD3;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4x4 unity_WorldToLight;
			// $Globals ConstantBuffers for Fragment Shader
			float4 _TimeEditor;
			float _fresnel;
			float4 _color1;
			float _FresnelPower;
			float4 _AtmosphereNoise_ST;
			float4 _color2;
			float _ColorPower;
			float _ColorSlide;
			float _AtmosphereLightPower;
			float _Opacity;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _AtmosphereNoise;

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
                tmp0 = unity_ObjectToWorld._m03_m13_m23_m33 * v.vertex.wwww + tmp0;
                tmp2 = tmp1.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp2 = unity_MatrixVP._m00_m10_m20_m30 * tmp1.xxxx + tmp2;
                tmp2 = unity_MatrixVP._m02_m12_m22_m32 * tmp1.zzzz + tmp2;
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp1.wwww + tmp2;
                o.texcoord.xy = v.texcoord.xy;
                o.texcoord1 = tmp0;
                tmp1.x = dot(-v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp1.y = dot(-v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp1.z = dot(-v.normal.xyz, unity_WorldToObject._m02_m12_m22);
                tmp1.w = dot(tmp1.xyz, tmp1.xyz);
                tmp1.w = rsqrt(tmp1.w);
                o.texcoord2.xyz = tmp1.www * tmp1.xyz;
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
                tmp0.xyz = _WorldSpaceLightPos0.www * -inp.texcoord1.xyz + _WorldSpaceLightPos0.xyz;
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                tmp0.w = dot(inp.texcoord2.xyz, inp.texcoord2.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp1.xyz = tmp0.www * inp.texcoord2.xyz;
                tmp0.x = saturate(dot(tmp1.xyz, tmp0.xyz));
                tmp0.x = 1.0 - tmp0.x;
                tmp0.x = log(tmp0.x);
                tmp0.x = tmp0.x * _AtmosphereLightPower;
                tmp0.x = exp(tmp0.x);
                tmp0.yzw = _WorldSpaceCameraPos - inp.texcoord1.xyz;
                tmp1.w = dot(tmp0.xyz, tmp0.xyz);
                tmp1.w = rsqrt(tmp1.w);
                tmp0.yzw = tmp0.yzw * tmp1.www;
                tmp0.y = dot(tmp1.xyz, tmp0.xyz);
                tmp0.y = max(tmp0.y, 0.0);
                tmp0.y = 1.0 - tmp0.y;
                tmp0.y = log(tmp0.y);
                tmp0.z = tmp0.y * _ColorPower;
                tmp0.y = tmp0.y * _fresnel;
                tmp0.y = exp(tmp0.y);
                tmp0.y = min(tmp0.y, 1.0);
                tmp0.y = 1.0 - tmp0.y;
                tmp0.y = log(tmp0.y);
                tmp0.y = tmp0.y * _FresnelPower;
                tmp0.y = exp(tmp0.y);
                tmp0.z = exp(tmp0.z);
                tmp1.xyz = _color2.xyz - _color1.xyz;
                tmp1.xyz = tmp0.zzz * tmp1.xyz;
                tmp1.xyz = tmp1.xyz / _ColorSlide.xxx;
                tmp1.xyz = tmp1.xyz + _color1.xyz;
                tmp0.xzw = tmp0.xxx * tmp1.xyz;
                tmp0.xzw = max(tmp0.xzw, float3(0.0, 0.0, 0.0));
                tmp0.xzw = min(tmp0.xzw, float3(5.0, 5.0, 5.0));
                tmp1.x = _TimeEditor.y + _Time.y;
                tmp1.xy = tmp1.xx * float2(0.005, 0.005) + inp.texcoord.xy;
                tmp1.xy = tmp1.xy * _AtmosphereNoise_ST.xy + _AtmosphereNoise_ST.zw;
                tmp1 = tex2D(_AtmosphereNoise, tmp1.xy);
                tmp1.xy = tmp1.xy * float2(0.3, 0.3) + float2(0.7, 0.7);
                tmp0.y = tmp0.y * tmp1.x;
                tmp0.y = tmp1.y * tmp0.y;
                tmp0.y = saturate(tmp0.y * _Opacity);
                o.sv_target.xyz = tmp0.yyy * tmp0.xzw;
                o.sv_target.w = 0.0;
                return o;
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
