// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Shader created with Shader Forge v1.26 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:1,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:2,rfrpo:False,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:1,x:37064,y:32532,varname:node_1,prsc:2|diff-654-OUT,spec-678-OUT,gloss-374-OUT,normal-662-OUT;n:type:ShaderForge.SFN_Tex2d,id:3,x:35927,y:32751,ptovrint:False,ptlb:Blue Texture,ptin:_BlueTexture,varname:_BlueTexture,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:26,x:35927,y:32390,ptovrint:False,ptlb:Red Texture,ptin:_RedTexture,varname:_RedTexture,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:311,x:35663,y:33734,ptovrint:False,ptlb:Blue Normal,ptin:_BlueNormal,varname:_BlueNormal,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:3,isnm:True;n:type:ShaderForge.SFN_Tex2d,id:313,x:35663,y:33350,ptovrint:False,ptlb:Red Normal,ptin:_RedNormal,varname:_RedNormal,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:3,isnm:True;n:type:ShaderForge.SFN_Slider,id:374,x:36186,y:32831,ptovrint:False,ptlb:Roughness,ptin:_Roughness,varname:_Gloss,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_Tex2d,id:409,x:35927,y:32571,ptovrint:False,ptlb:Green Texture,ptin:_GreenTexture,varname:_GreenTexture,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:602,x:35663,y:33541,ptovrint:False,ptlb:Green Normal,ptin:_GreenNormal,varname:_GreenNormal,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:3,isnm:True;n:type:ShaderForge.SFN_Tex2d,id:636,x:35655,y:34577,ptovrint:False,ptlb:Blue Specular,ptin:_BlueSpecular,varname:_BlueSpecular,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:638,x:35655,y:34197,ptovrint:False,ptlb:Red Specular,ptin:_RedSpecular,varname:_RedSpecular,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:642,x:35655,y:34386,ptovrint:False,ptlb:Green Specular,ptin:_GreenSpecular,varname:_GreenSpecular,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_ChannelBlend,id:654,x:36329,y:32570,varname:node_654,prsc:2,chbt:1|M-691-OUT,R-26-RGB,G-409-RGB,B-3-RGB,BTM-773-OUT;n:type:ShaderForge.SFN_ChannelBlend,id:662,x:36052,y:33592,varname:node_662,prsc:2,chbt:1|M-691-OUT,R-313-RGB,G-602-RGB,B-311-RGB,BTM-773-OUT;n:type:ShaderForge.SFN_ChannelBlend,id:678,x:36092,y:34419,varname:node_678,prsc:2,chbt:1|M-691-OUT,R-638-RGB,G-642-RGB,B-636-RGB,BTM-773-OUT;n:type:ShaderForge.SFN_Clamp01,id:691,x:34946,y:33268,varname:node_691,prsc:2|IN-692-OUT;n:type:ShaderForge.SFN_Power,id:692,x:34763,y:33268,varname:node_692,prsc:2|VAL-693-OUT,EXP-694-OUT;n:type:ShaderForge.SFN_Add,id:693,x:34383,y:33213,varname:node_693,prsc:2|A-698-RGB,B-695-OUT;n:type:ShaderForge.SFN_Vector1,id:694,x:34431,y:33456,varname:node_694,prsc:2,v1:32;n:type:ShaderForge.SFN_Add,id:695,x:34215,y:33255,varname:node_695,prsc:2|A-719-OUT,B-701-OUT;n:type:ShaderForge.SFN_VertexColor,id:698,x:33078,y:33142,varname:node_698,prsc:2;n:type:ShaderForge.SFN_OneMinus,id:699,x:33728,y:33234,varname:node_699,prsc:2|IN-698-RGB;n:type:ShaderForge.SFN_Multiply,id:701,x:33909,y:33343,varname:node_701,prsc:2|A-699-OUT,B-722-RGB;n:type:ShaderForge.SFN_Tex2dAsset,id:709,x:33000,y:32684,ptovrint:False,ptlb:Noise Texture,ptin:_NoiseTexture,varname:_NoiseTexture,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:9f21b5f7fdcd53b4fac39152ccc95123,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:717,x:33251,y:32754,varname:node_9468,prsc:2,tex:9f21b5f7fdcd53b4fac39152ccc95123,ntxv:0,isnm:False|TEX-709-TEX;n:type:ShaderForge.SFN_Multiply,id:719,x:33909,y:33091,varname:node_719,prsc:2|A-739-OUT,B-825-OUT;n:type:ShaderForge.SFN_Tex2d,id:722,x:33257,y:33365,varname:node_2164,prsc:2,tex:9f21b5f7fdcd53b4fac39152ccc95123,ntxv:0,isnm:False|TEX-709-TEX;n:type:ShaderForge.SFN_Multiply,id:739,x:33515,y:32776,varname:node_739,prsc:2|A-717-RGB,B-698-RGB;n:type:ShaderForge.SFN_Vector1,id:773,x:34893,y:33723,varname:node_773,prsc:2,v1:0;n:type:ShaderForge.SFN_Slider,id:825,x:33358,y:32955,ptovrint:False,ptlb:Blend Amount,ptin:_BlendAmount,varname:_BlendAmount,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:3.5,cur:20,max:20;proporder:26-409-3-313-602-311-638-642-636-709-374-825;pass:END;sub:END;*/

Shader "Ikari Vertex Painter/Diffuse/Vertex Color RGB Advanced Blend" {
    Properties {
        _RedTexture ("Red Texture", 2D) = "white" {}
        _GreenTexture ("Green Texture", 2D) = "white" {}
        _BlueTexture ("Blue Texture", 2D) = "white" {}
        _RedNormal ("Red Normal", 2D) = "bump" {}
        _GreenNormal ("Green Normal", 2D) = "bump" {}
        _BlueNormal ("Blue Normal", 2D) = "bump" {}
        _RedSpecular ("Red Specular", 2D) = "white" {}
        _GreenSpecular ("Green Specular", 2D) = "white" {}
        _BlueSpecular ("Blue Specular", 2D) = "white" {}
        _NoiseTexture ("Noise Texture", 2D) = "white" {}
        _Roughness ("Roughness", Range(0, 1)) = 0
        _BlendAmount ("Blend Amount", Range(3.5, 20)) = 20
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        LOD 200
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform sampler2D _BlueTexture; uniform float4 _BlueTexture_ST;
            uniform sampler2D _RedTexture; uniform float4 _RedTexture_ST;
            uniform sampler2D _BlueNormal; uniform float4 _BlueNormal_ST;
            uniform sampler2D _RedNormal; uniform float4 _RedNormal_ST;
            uniform float _Roughness;
            uniform sampler2D _GreenTexture; uniform float4 _GreenTexture_ST;
            uniform sampler2D _GreenNormal; uniform float4 _GreenNormal_ST;
            uniform sampler2D _BlueSpecular; uniform float4 _BlueSpecular_ST;
            uniform sampler2D _RedSpecular; uniform float4 _RedSpecular_ST;
            uniform sampler2D _GreenSpecular; uniform float4 _GreenSpecular_ST;
            uniform sampler2D _NoiseTexture; uniform float4 _NoiseTexture_ST;
            uniform float _BlendAmount;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 bitangentDir : TEXCOORD4;
                float4 vertexColor : COLOR;
                LIGHTING_COORDS(5,6)
                UNITY_FOG_COORDS(7)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float4 node_9468 = tex2D(_NoiseTexture,TRANSFORM_TEX(i.uv0, _NoiseTexture));
                float4 node_2164 = tex2D(_NoiseTexture,TRANSFORM_TEX(i.uv0, _NoiseTexture));
                float3 node_691 = saturate(pow((i.vertexColor.rgb+(((node_9468.rgb*i.vertexColor.rgb)*_BlendAmount)+((1.0 - i.vertexColor.rgb)*node_2164.rgb))),32.0));
                float node_773 = 0.0;
                float3 _RedNormal_var = UnpackNormal(tex2D(_RedNormal,TRANSFORM_TEX(i.uv0, _RedNormal)));
                float3 _GreenNormal_var = UnpackNormal(tex2D(_GreenNormal,TRANSFORM_TEX(i.uv0, _GreenNormal)));
                float3 _BlueNormal_var = UnpackNormal(tex2D(_BlueNormal,TRANSFORM_TEX(i.uv0, _BlueNormal)));
                float3 normalLocal = (lerp( lerp( lerp( float3(node_773,node_773,node_773), _RedNormal_var.rgb, node_691.r ), _GreenNormal_var.rgb, node_691.g ), _BlueNormal_var.rgb, node_691.b ));
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
///////// Gloss:
                float gloss = 1.0 - _Roughness; // Convert roughness to gloss
                float specPow = exp2( gloss * 10.0+1.0);
////// Specular:
                float NdotL = max(0, dot( normalDirection, lightDirection ));
                float4 _RedSpecular_var = tex2D(_RedSpecular,TRANSFORM_TEX(i.uv0, _RedSpecular));
                float4 _GreenSpecular_var = tex2D(_GreenSpecular,TRANSFORM_TEX(i.uv0, _GreenSpecular));
                float4 _BlueSpecular_var = tex2D(_BlueSpecular,TRANSFORM_TEX(i.uv0, _BlueSpecular));
                float3 specularColor = (lerp( lerp( lerp( float3(node_773,node_773,node_773), _RedSpecular_var.rgb, node_691.r ), _GreenSpecular_var.rgb, node_691.g ), _BlueSpecular_var.rgb, node_691.b ));
                float3 directSpecular = (floor(attenuation) * _LightColor0.xyz) * pow(max(0,dot(halfDirection,normalDirection)),specPow)*specularColor;
                float3 specular = directSpecular;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                float3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += UNITY_LIGHTMODEL_AMBIENT.rgb; // Ambient Light
                float4 _RedTexture_var = tex2D(_RedTexture,TRANSFORM_TEX(i.uv0, _RedTexture));
                float4 _GreenTexture_var = tex2D(_GreenTexture,TRANSFORM_TEX(i.uv0, _GreenTexture));
                float4 _BlueTexture_var = tex2D(_BlueTexture,TRANSFORM_TEX(i.uv0, _BlueTexture));
                float3 diffuseColor = (lerp( lerp( lerp( float3(node_773,node_773,node_773), _RedTexture_var.rgb, node_691.r ), _GreenTexture_var.rgb, node_691.g ), _BlueTexture_var.rgb, node_691.b ));
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse + specular;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "FORWARD_DELTA"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDADD
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform sampler2D _BlueTexture; uniform float4 _BlueTexture_ST;
            uniform sampler2D _RedTexture; uniform float4 _RedTexture_ST;
            uniform sampler2D _BlueNormal; uniform float4 _BlueNormal_ST;
            uniform sampler2D _RedNormal; uniform float4 _RedNormal_ST;
            uniform float _Roughness;
            uniform sampler2D _GreenTexture; uniform float4 _GreenTexture_ST;
            uniform sampler2D _GreenNormal; uniform float4 _GreenNormal_ST;
            uniform sampler2D _BlueSpecular; uniform float4 _BlueSpecular_ST;
            uniform sampler2D _RedSpecular; uniform float4 _RedSpecular_ST;
            uniform sampler2D _GreenSpecular; uniform float4 _GreenSpecular_ST;
            uniform sampler2D _NoiseTexture; uniform float4 _NoiseTexture_ST;
            uniform float _BlendAmount;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 bitangentDir : TEXCOORD4;
                float4 vertexColor : COLOR;
                LIGHTING_COORDS(5,6)
                UNITY_FOG_COORDS(7)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float4 node_9468 = tex2D(_NoiseTexture,TRANSFORM_TEX(i.uv0, _NoiseTexture));
                float4 node_2164 = tex2D(_NoiseTexture,TRANSFORM_TEX(i.uv0, _NoiseTexture));
                float3 node_691 = saturate(pow((i.vertexColor.rgb+(((node_9468.rgb*i.vertexColor.rgb)*_BlendAmount)+((1.0 - i.vertexColor.rgb)*node_2164.rgb))),32.0));
                float node_773 = 0.0;
                float3 _RedNormal_var = UnpackNormal(tex2D(_RedNormal,TRANSFORM_TEX(i.uv0, _RedNormal)));
                float3 _GreenNormal_var = UnpackNormal(tex2D(_GreenNormal,TRANSFORM_TEX(i.uv0, _GreenNormal)));
                float3 _BlueNormal_var = UnpackNormal(tex2D(_BlueNormal,TRANSFORM_TEX(i.uv0, _BlueNormal)));
                float3 normalLocal = (lerp( lerp( lerp( float3(node_773,node_773,node_773), _RedNormal_var.rgb, node_691.r ), _GreenNormal_var.rgb, node_691.g ), _BlueNormal_var.rgb, node_691.b ));
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
///////// Gloss:
                float gloss = 1.0 - _Roughness; // Convert roughness to gloss
                float specPow = exp2( gloss * 10.0+1.0);
////// Specular:
                float NdotL = max(0, dot( normalDirection, lightDirection ));
                float4 _RedSpecular_var = tex2D(_RedSpecular,TRANSFORM_TEX(i.uv0, _RedSpecular));
                float4 _GreenSpecular_var = tex2D(_GreenSpecular,TRANSFORM_TEX(i.uv0, _GreenSpecular));
                float4 _BlueSpecular_var = tex2D(_BlueSpecular,TRANSFORM_TEX(i.uv0, _BlueSpecular));
                float3 specularColor = (lerp( lerp( lerp( float3(node_773,node_773,node_773), _RedSpecular_var.rgb, node_691.r ), _GreenSpecular_var.rgb, node_691.g ), _BlueSpecular_var.rgb, node_691.b ));
                float3 directSpecular = attenColor * pow(max(0,dot(halfDirection,normalDirection)),specPow)*specularColor;
                float3 specular = directSpecular;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                float4 _RedTexture_var = tex2D(_RedTexture,TRANSFORM_TEX(i.uv0, _RedTexture));
                float4 _GreenTexture_var = tex2D(_GreenTexture,TRANSFORM_TEX(i.uv0, _GreenTexture));
                float4 _BlueTexture_var = tex2D(_BlueTexture,TRANSFORM_TEX(i.uv0, _BlueTexture));
                float3 diffuseColor = (lerp( lerp( lerp( float3(node_773,node_773,node_773), _RedTexture_var.rgb, node_691.r ), _GreenTexture_var.rgb, node_691.g ), _BlueTexture_var.rgb, node_691.b ));
                float3 diffuse = directDiffuse * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse + specular;
                fixed4 finalRGBA = fixed4(finalColor * 1,0);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
