// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.36 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.36;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,atcv:False,rfrpo:False,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:9356,x:32429,y:32731,varname:node_9356,prsc:2|diff-1189-OUT,normal-3152-RGB;n:type:ShaderForge.SFN_ScreenPos,id:2022,x:30980,y:32516,varname:node_2022,prsc:2,sctp:2;n:type:ShaderForge.SFN_Tex2d,id:422,x:31350,y:32516,ptovrint:False,ptlb:ReflectionTex,ptin:_ReflectionTex,varname:_ReflectionTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-6472-OUT;n:type:ShaderForge.SFN_ComponentMask,id:6472,x:31184,y:32516,varname:node_6472,prsc:2,cc1:0,cc2:1,cc3:-1,cc4:-1|IN-2022-UVOUT;n:type:ShaderForge.SFN_Clamp01,id:2462,x:31515,y:33129,varname:node_2462,prsc:2|IN-1887-OUT;n:type:ShaderForge.SFN_Power,id:1887,x:31317,y:33129,varname:node_1887,prsc:2|VAL-1308-OUT,EXP-4788-OUT;n:type:ShaderForge.SFN_Vector1,id:4788,x:31083,y:33197,varname:node_4788,prsc:2,v1:1.2;n:type:ShaderForge.SFN_Add,id:1308,x:31083,y:33064,varname:node_1308,prsc:2|A-7669-R,B-8009-OUT;n:type:ShaderForge.SFN_Multiply,id:8009,x:30816,y:33080,varname:node_8009,prsc:2|A-7669-R,B-6788-RGB;n:type:ShaderForge.SFN_Lerp,id:6373,x:31847,y:32834,varname:node_6373,prsc:2|A-7278-OUT,B-1663-RGB,T-2462-OUT;n:type:ShaderForge.SFN_VertexColor,id:7669,x:30607,y:32930,varname:node_7669,prsc:2;n:type:ShaderForge.SFN_Tex2d,id:1663,x:31350,y:32761,ptovrint:False,ptlb:MainTex,ptin:_MainTex,varname:_MainTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:65f91997f7060f747a07bc75a804e6c8,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:6788,x:30609,y:33231,ptovrint:False,ptlb:HeightTex,ptin:_HeightTex,varname:_HeightTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:6df4b6a414f2c3744bd58c9707ec8c8a,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:6097,x:31637,y:32519,varname:node_6097,prsc:2|A-5193-OUT,B-4663-OUT;n:type:ShaderForge.SFN_Vector1,id:4663,x:31350,y:32681,varname:node_4663,prsc:2,v1:0.8;n:type:ShaderForge.SFN_Lerp,id:7278,x:31900,y:32562,varname:node_7278,prsc:2|A-6097-OUT,B-1663-RGB,T-293-OUT;n:type:ShaderForge.SFN_Tex2d,id:3152,x:31350,y:32946,ptovrint:False,ptlb:NormalTex,ptin:_NormalTex,varname:_NormalTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:314,x:31544,y:32795,varname:node_314,prsc:2|A-1663-RGB,B-3278-OUT;n:type:ShaderForge.SFN_Lerp,id:1189,x:31847,y:32983,varname:node_1189,prsc:2|A-6373-OUT,B-314-OUT,T-7669-G;n:type:ShaderForge.SFN_Vector1,id:3278,x:31552,y:32984,varname:node_3278,prsc:2,v1:0.2;n:type:ShaderForge.SFN_Slider,id:293,x:31500,y:32711,ptovrint:False,ptlb:Transparency,ptin:_Transparency,varname:_Transparency,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.304796,max:1;n:type:ShaderForge.SFN_Color,id:6067,x:31343,y:32258,ptovrint:False,ptlb:node_6067,ptin:_node_6067,varname:_node_6067,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Blend,id:5193,x:31574,y:32324,varname:node_5193,prsc:2,blmd:6,clmp:True|SRC-6067-RGB,DST-422-RGB;proporder:422-1663-6788-3152-293-6067;pass:END;sub:END;*/

Shader "Ikari Vertex Painter/Advanced/Water Blend" {
    Properties {
        _ReflectionTex ("ReflectionTex", 2D) = "white" {}
        _MainTex ("MainTex", 2D) = "white" {}
        _HeightTex ("HeightTex", 2D) = "white" {}
        _NormalTex ("NormalTex", 2D) = "white" {}
        _Transparency ("Transparency", Range(0, 1)) = 0.304796
        _node_6067 ("node_6067", Color) = (0.5,0.5,0.5,1)
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
            #pragma only_renderers d3d9 d3d11 glcore gles n3ds wiiu 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform sampler2D _ReflectionTex; uniform float4 _ReflectionTex_ST;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform sampler2D _HeightTex; uniform float4 _HeightTex_ST;
            uniform sampler2D _NormalTex; uniform float4 _NormalTex_ST;
            uniform float _Transparency;
            uniform float4 _node_6067;
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
                float4 screenPos : TEXCOORD5;
                float4 vertexColor : COLOR;
                LIGHTING_COORDS(6,7)
                UNITY_FOG_COORDS(8)
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
                o.screenPos = o.pos;
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                #if UNITY_UV_STARTS_AT_TOP
                    float grabSign = -_ProjectionParams.x;
                #else
                    float grabSign = _ProjectionParams.x;
                #endif
                i.normalDir = normalize(i.normalDir);
                i.screenPos = float4( i.screenPos.xy / i.screenPos.w, 0, 0 );
                i.screenPos.y *= _ProjectionParams.x;
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float4 _NormalTex_var = tex2D(_NormalTex,TRANSFORM_TEX(i.uv0, _NormalTex));
                float3 normalLocal = _NormalTex_var.rgb;
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float2 sceneUVs = float2(1,grabSign)*i.screenPos.xy*0.5+0.5;
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
                float NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                float3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += UNITY_LIGHTMODEL_AMBIENT.rgb; // Ambient Light
                float2 node_6472 = sceneUVs.rg.rg;
                float4 _ReflectionTex_var = tex2D(_ReflectionTex,TRANSFORM_TEX(node_6472, _ReflectionTex));
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float4 _HeightTex_var = tex2D(_HeightTex,TRANSFORM_TEX(i.uv0, _HeightTex));
                float3 diffuseColor = lerp(lerp(lerp((saturate((1.0-(1.0-_node_6067.rgb)*(1.0-_ReflectionTex_var.rgb)))*0.8),_MainTex_var.rgb,_Transparency),_MainTex_var.rgb,saturate(pow((i.vertexColor.r+(i.vertexColor.r*_HeightTex_var.rgb)),1.2))),(_MainTex_var.rgb*0.2),i.vertexColor.g);
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse;
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
            #pragma only_renderers d3d9 d3d11 glcore gles n3ds wiiu 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform sampler2D _ReflectionTex; uniform float4 _ReflectionTex_ST;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform sampler2D _HeightTex; uniform float4 _HeightTex_ST;
            uniform sampler2D _NormalTex; uniform float4 _NormalTex_ST;
            uniform float _Transparency;
            uniform float4 _node_6067;
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
                float4 screenPos : TEXCOORD5;
                float4 vertexColor : COLOR;
                LIGHTING_COORDS(6,7)
                UNITY_FOG_COORDS(8)
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
                o.screenPos = o.pos;
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                #if UNITY_UV_STARTS_AT_TOP
                    float grabSign = -_ProjectionParams.x;
                #else
                    float grabSign = _ProjectionParams.x;
                #endif
                i.normalDir = normalize(i.normalDir);
                i.screenPos = float4( i.screenPos.xy / i.screenPos.w, 0, 0 );
                i.screenPos.y *= _ProjectionParams.x;
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float4 _NormalTex_var = tex2D(_NormalTex,TRANSFORM_TEX(i.uv0, _NormalTex));
                float3 normalLocal = _NormalTex_var.rgb;
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float2 sceneUVs = float2(1,grabSign)*i.screenPos.xy*0.5+0.5;
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
                float NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                float2 node_6472 = sceneUVs.rg.rg;
                float4 _ReflectionTex_var = tex2D(_ReflectionTex,TRANSFORM_TEX(node_6472, _ReflectionTex));
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float4 _HeightTex_var = tex2D(_HeightTex,TRANSFORM_TEX(i.uv0, _HeightTex));
                float3 diffuseColor = lerp(lerp(lerp((saturate((1.0-(1.0-_node_6067.rgb)*(1.0-_ReflectionTex_var.rgb)))*0.8),_MainTex_var.rgb,_Transparency),_MainTex_var.rgb,saturate(pow((i.vertexColor.r+(i.vertexColor.r*_HeightTex_var.rgb)),1.2))),(_MainTex_var.rgb*0.2),i.vertexColor.g);
                float3 diffuse = directDiffuse * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse;
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
