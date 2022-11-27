Shader "ZeroGravity/UI/JitterEffect" {
	Properties {
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Vector) = (1,1,1,1)
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255
		_ColorMask ("Color Mask", Float) = 15
		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
		[Toggle] _UseOnText ("UseOnText", Range(0, 1)) = 0
		_JitterTexture ("JitterTexture", 2D) = "white" {}
		_JitterUV ("JitterUV", Vector) = (0,0,0,0)
		_JitterDensity ("JitterDensity", Float) = 0
		_Seed ("Seed", Float) = 0
		_JitterStrength ("JitterStrength", Float) = 0
		[Toggle] _RandomJitter ("RandomJitter", Range(0, 1)) = 0
		_InterlaceCount ("InterlaceCount", Float) = 0
	}
	SubShader {
		Tags { "CanUseSpriteAtlas" = "true" "IGNOREPROJECTOR" = "true" "PreviewType" = "Plane" "QUEUE" = "Transparent" "RenderType" = "Transparent" }
		Pass {
			Name "Default"
			Tags { "CanUseSpriteAtlas" = "true" "IGNOREPROJECTOR" = "true" "PreviewType" = "Plane" "QUEUE" = "Transparent" "RenderType" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha, SrcAlpha OneMinusSrcAlpha
			ColorMask 0 -1
			ZWrite Off
			Cull Off
			Stencil {
				ReadMask 255
				WriteMask 255
				Comp Always
				Pass Keep
				Fail Keep
				ZFail Keep
			}
			GpuProgramID 41325
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float4 color : COLOR0;
				float2 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _Color;
			// $Globals ConstantBuffers for Fragment Shader
			float4 _ClipRect;
			float _UseOnText;
			float2 _JitterUV;
			float _JitterDensity;
			float _Seed;
			float _JitterStrength;
			float _RandomJitter;
			float _InterlaceCount;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _JitterTexture;
			sampler2D _MainTex;

			// Keywords:
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
                tmp0 = unity_MatrixVP._m03_m13_m23_m33 * tmp0.wwww + tmp1;
                o.position = tmp0;
                o.color = v.color * _Color;
                o.texcoord.xy = v.texcoord.xy;
                o.texcoord1 = v.vertex;
                tmp0.y = tmp0.y * _ProjectionParams.x;
                tmp1.xzw = tmp0.xwy * float3(0.5, 0.5, 0.5);
                o.texcoord2.zw = tmp0.zw;
                o.texcoord2.xy = tmp1.zz + tmp1.xw;
                return o;
			}
			// Keywords:
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                tmp0.x = _JitterDensity * 1.5;
                tmp0.yz = inp.texcoord2.xy / inp.texcoord2.ww;
                tmp0.yw = tmp0.yz * _JitterUV;
                tmp1.xy = tmp0.xx * tmp0.yw;
                tmp0.yw = tmp0.yw * _JitterDensity.xx;
                tmp0.yw = floor(tmp0.yw);
                tmp1.xy = floor(tmp1.xy);
                tmp1.z = _Time.y * 5.0 + _Seed;
                tmp1.xy = tmp1.xy + tmp1.zz;
                tmp1.xy = tmp1.xy / tmp0.xx;
                tmp0.xy = tmp0.yw + tmp1.zz;
                tmp0.xy = tmp0.xy / _JitterDensity.xx;
                tmp0.xy = tmp1.xy + tmp0.xy;
                tmp2 = tex2D(_JitterTexture, tmp0.xy);
                tmp0.x = 1.0 - tmp2.x;
                tmp0.x = inp.color.w * tmp0.x + tmp2.x;
                tmp0.y = inp.color.w < tmp2.x;
                tmp0.w = tmp2.x * 2.5 + -0.5;
                tmp1.x = tmp0.w * _JitterStrength;
                tmp0.y = tmp0.y ? 0.0 : 1.0;
                tmp0.x = tmp0.x * tmp0.y;
                tmp0.y = tmp1.z * 0.01 + tmp0.z;
                tmp0.z = tmp0.z * 50.0 + tmp1.z;
                tmp0.w = tmp1.z * 0.5;
                tmp0.zw = sin(tmp0.zw);
                tmp0.zw = tmp0.zw + float2(1.0, -0.7);
                tmp0.w = max(tmp0.w, 0.0);
                tmp0.z = tmp0.z * 0.1 + 0.8;
                tmp0.y = tmp0.y * _InterlaceCount;
                tmp0.y = sin(tmp0.y);
                tmp0.y = tmp0.y + 1.0;
                tmp0.y = tmp0.y * 0.1 + 0.8;
                tmp0.y = tmp0.z * tmp0.y;
                tmp0.x = tmp0.y * tmp0.x;
                tmp0.y = _SinTime.w + _SinTime.w;
                tmp0.z = _SinTime.w > 0.0;
                tmp0.y = tmp0.y ? tmp0.z : 0.0;
                tmp0.y = tmp0.w * tmp0.y;
                tmp0.y = tmp0.y * _RandomJitter;
                tmp0.y = -tmp0.y * 0.3 + 1.0;
                tmp0.y = tmp0.y * inp.color.w;
                tmp1.y = 0.0;
                tmp0.zw = tmp1.xy + inp.texcoord.xy;
                tmp1.xy = inp.texcoord.xy - tmp0.zw;
                tmp0.yz = tmp0.yy * tmp1.xy + tmp0.zw;
                tmp1 = tex2D(_MainTex, tmp0.yz);
                tmp0.x = tmp0.x * tmp1.w;
                tmp0.yz = inp.texcoord1.xy >= _ClipRect.xy;
                tmp0.yz = tmp0.yz ? 1.0 : 0.0;
                tmp2.xy = _ClipRect.zw >= inp.texcoord1.xy;
                tmp2.xy = tmp2.xy ? 1.0 : 0.0;
                tmp0.yz = tmp0.yz * tmp2.xy;
                tmp0.y = tmp0.z * tmp0.y;
                o.sv_target.w = tmp0.y * tmp0.x;
                tmp0.x = _UseOnText == 0.0;
                tmp0.y = _UseOnText < 0.0;
                tmp0.x = uint1(tmp0.x) | uint1(tmp0.y);
                tmp0.xyz = tmp0.xxx ? tmp1.xyz : 0.0;
                tmp0.w = _UseOnText > 0.0;
                tmp0.xyz = tmp0.www ? float3(1.0, 1.0, 1.0) : tmp0.xyz;
                o.sv_target.xyz = tmp0.xyz * inp.color.xyz;
                return o;
			}
			ENDCG
		}
	}
}
