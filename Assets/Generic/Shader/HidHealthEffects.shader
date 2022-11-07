Shader "Hid/HealthEffects" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_BloodTex ("Blood Texture", 2D) = "white" {}
		_BloodMask ("Blood Mask", 2D) = "black" {}
		_HitFront ("HitFront", Range(0, 1)) = 0
		_HitBack ("HitBack", Range(0, 1)) = 0
		_HitLeft ("HitLeft", Range(0, 1)) = 0
		_HitRight ("HitRight", Range(0, 1)) = 0
		_Health ("Health", Range(0, 1)) = 1
		_Vignette ("Vignette", 2D) = "white" {}
		_Veins ("Veins", 2D) = "white" {}
		_VeinsMask ("Veins Mask", 2D) = "white" {}
		_VeinsAppear ("VeinsAppear", Range(0, 1)) = 1
		_BlurIterations ("BlurIterations", Float) = 1
		_BlurAmount ("BlurAmount", Float) = 1
	}
	SubShader {
		Pass {
			ZTest Always
			ZWrite Off
			Cull Off
			GpuProgramID 31572
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			struct v2f
			{
				float2 texcoord : TEXCOORD0;
				float4 position : SV_POSITION0;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			// $Globals ConstantBuffers for Fragment Shader
			float _HitFront;
			float _HitBack;
			float _HitLeft;
			float _HitRight;
			float _Health;
			float _VeinsAppear;
			float _BlurAmount;
			float _BlurIterations;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _Vignette;
			sampler2D _MainTex;
			sampler2D _BloodTex;
			sampler2D _BloodMask;
			sampler2D _Veins;

			// Keywords:
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                o.texcoord.xy = v.texcoord.xy;
                tmp0 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                tmp0 = tmp0 + unity_ObjectToWorld._m03_m13_m23_m33;
                tmp1 = tmp0.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp1 = unity_MatrixVP._m00_m10_m20_m30 * tmp0.xxxx + tmp1;
                tmp1 = unity_MatrixVP._m02_m12_m22_m32 * tmp0.zzzz + tmp1;
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp0.wwww + tmp1;
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
                tmp0 = tex2D(_Vignette, inp.texcoord.xy);
                tmp0.y = _BlurAmount > 0.0;
                if (tmp0.y) {
                    tmp0.y = asint(_BlurIterations);
                    tmp0.z = _BlurAmount * 0.2;
                    tmp0.w = 1.0 - tmp0.x;
                    tmp1.x = tmp0.w * tmp0.z;
                    tmp1.y = trunc(_BlurIterations);
                    tmp2.y = inp.texcoord.y + inp.texcoord.x;
                    tmp3.y = tmp2.y + 24.0;
                    tmp4 = float4(0.0, 0.0, 0.0, 0.0);
                    tmp1.z = 0.0;

					int i = 0;
                    while (true) {
                        tmp2.z = i >= tmp0.y;
                        if (tmp2.z) {
                            break;
                        }
						i++;
                        tmp2.x = floor(i);
                        tmp2.z = dot(tmp2.xy, float2(12.9898, 78.233));
                        tmp2.z = sin(tmp2.z);
                        tmp2.z = tmp2.z * 43758.55;
                        tmp2.z = frac(tmp2.z);
                        tmp2.z = tmp2.z / tmp1.y;
                        tmp2.z = tmp2.z * 20626.48;
                        tmp5.x = sin(tmp2.z);
                        tmp6.x = cos(tmp2.z);
                        tmp2.x = dot(tmp2.xy, float2(12.9898, 78.233));
                        tmp2.x = sin(tmp2.x);
                        tmp2.x = tmp2.x * 43758.55;
                        tmp2.x = frac(tmp2.x);
                        tmp2.x = tmp0.z * tmp0.w + tmp2.x;
                        tmp6.y = tmp5.x;
                        tmp2.xz = tmp2.xx * tmp6.xy;
                        tmp2.xz = -tmp2.xz * tmp1.xx + inp.texcoord.xy;
                        tmp5 = tex2D(_MainTex, tmp2.xz);
                        tmp5 = tmp5 * float4(0.5, 0.5, 0.5, 0.5) + tmp4;
                        tmp1.zw = tmp1.zz + int2(1, 2);
                        tmp3.x = floor(tmp1.w);
                        tmp1.w = dot(tmp3.xy, float2(12.9898, 78.233));
                        tmp1.w = sin(tmp1.w);
                        tmp1.w = tmp1.w * 43758.55;
                        tmp1.w = frac(tmp1.w);
                        tmp1.w = tmp0.z * tmp0.w + tmp1.w;
                        tmp2.xz = tmp1.ww * tmp6.xy;
                        tmp2.xz = tmp2.xz * tmp1.xx + inp.texcoord.xy;
                        tmp6 = tex2D(_MainTex, tmp2.xz);
                        tmp4 = tmp6 * float4(0.5, 0.5, 0.5, 0.5) + tmp5;
                    }
                    tmp1 = tmp4 / tmp1.yyyy;
                } else {
                    tmp1 = tex2D(_MainTex, inp.texcoord.xy);
                }
                tmp2 = tex2D(_BloodTex, inp.texcoord.xy);
                tmp3 = tex2D(_BloodMask, inp.texcoord.xy);
                tmp3 = tmp3 * float4(_HitFront.x, _HitBack.x, _HitLeft.x, _HitRight.x);
                tmp0.yz = max(tmp3.yw, tmp3.xz);
                tmp0.y = max(tmp0.z, tmp0.y);
                tmp0.z = dot(tmp1.xyz, float3(0.25, 0.25, 0.5));
                tmp1 = tmp1 - tmp0.zzzz;
                tmp1 = _Health.xxxx * tmp1 + tmp0.zzzz;
                tmp3 = tex2D(_Veins, inp.texcoord.xy);
                tmp0.z = sin(_Time.z);
                tmp0.w = tmp0.z + 0.5;
                tmp0.w = tmp0.w * _VeinsAppear;
                tmp0.w = tmp0.w * 0.5 + -0.2;
                tmp0.z = tmp0.z * 0.5 + 0.5;
                tmp0.z = _VeinsAppear * tmp0.z + 0.2;
                tmp4.x = tmp3.x - tmp0.w;
                tmp0.z = tmp0.z - tmp0.w;
                tmp0.z = saturate(tmp4.x / tmp0.z);
                tmp4 = float4(1.0, 1.0, 1.0, 1.0) - tmp3;
                tmp3 = tmp0.zzzz * tmp4 + tmp3;
                tmp0.y = 1.0 - tmp0.y;
                tmp4 = float4(1.0, 1.0, 1.0, 1.0) - tmp2;
                tmp2 = tmp0.yyyy * tmp4 + tmp2;
                tmp1 = tmp1 * tmp2;
                tmp0.y = 1.0 - tmp0.x;
                tmp0.x = _Health * tmp0.y + tmp0.x;
                tmp0 = tmp0.xxxx * tmp1;
                o.sv_target = tmp3 * tmp0;
                return o;
			}
			ENDCG
		}
	}
}
