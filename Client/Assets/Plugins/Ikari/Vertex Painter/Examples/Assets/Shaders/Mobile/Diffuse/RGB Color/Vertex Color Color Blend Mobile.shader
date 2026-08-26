// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Shader created with Shader Forge v1.26 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:2,rfrpo:False,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:1,x:34046,y:32902,varname:node_1,prsc:2|diff-1850-OUT;n:type:ShaderForge.SFN_Tex2d,id:2,x:32751,y:32724,ptovrint:False,ptlb:MainTex,ptin:_MainTex,varname:_MainTex,prsc:0,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Color,id:17,x:32751,y:32509,ptovrint:False,ptlb:Color,ptin:_Color,varname:_Color,prsc:0,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Multiply,id:18,x:32939,y:32617,varname:node_18,prsc:0|A-17-RGB,B-2-RGB;n:type:ShaderForge.SFN_Color,id:177,x:32667,y:32987,ptovrint:False,ptlb:Red Channel Color,ptin:_RedChannelColor,varname:_RedChannelColor,prsc:0,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_VertexColor,id:1380,x:32422,y:32781,varname:node_1380,prsc:2;n:type:ShaderForge.SFN_Multiply,id:1498,x:33138,y:32983,varname:node_1498,prsc:2|A-1499-OUT,B-1682-OUT;n:type:ShaderForge.SFN_Blend,id:1499,x:32939,y:32983,varname:node_1499,prsc:2,blmd:1,clmp:True|SRC-18-OUT,DST-177-RGB;n:type:ShaderForge.SFN_Vector1,id:1682,x:32469,y:33172,varname:node_1682,prsc:0,v1:2;n:type:ShaderForge.SFN_Color,id:1690,x:32667,y:33220,ptovrint:False,ptlb:Green Channel Color,ptin:_GreenChannelColor,varname:_GreenChannelColor,prsc:0,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Multiply,id:1716,x:33138,y:33147,varname:node_1716,prsc:2|A-1718-OUT,B-1682-OUT;n:type:ShaderForge.SFN_Blend,id:1718,x:32937,y:33147,varname:node_1718,prsc:2,blmd:1,clmp:True|SRC-18-OUT,DST-1690-RGB;n:type:ShaderForge.SFN_Multiply,id:1738,x:33138,y:33337,varname:node_1738,prsc:2|A-1740-OUT,B-1682-OUT;n:type:ShaderForge.SFN_Blend,id:1740,x:32937,y:33337,varname:node_1740,prsc:2,blmd:1,clmp:True|SRC-18-OUT,DST-1742-RGB;n:type:ShaderForge.SFN_Color,id:1742,x:32667,y:33435,ptovrint:False,ptlb:Blue Channel Color,ptin:_BlueChannelColor,varname:_BlueChannelColor,prsc:0,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_ChannelBlend,id:1850,x:33561,y:33031,varname:node_1850,prsc:2,chbt:1|M-1906-OUT,R-1498-OUT,G-1716-OUT,B-1738-OUT,A-18-OUT,BTM-2299-OUT;n:type:ShaderForge.SFN_Append,id:1906,x:32939,y:32815,varname:node_1906,prsc:0|A-1380-RGB,B-2075-OUT;n:type:ShaderForge.SFN_OneMinus,id:2075,x:32309,y:33100,varname:node_2075,prsc:2|IN-1380-A;n:type:ShaderForge.SFN_Vector1,id:2299,x:33376,y:33181,varname:node_2299,prsc:2,v1:0;proporder:17-2-177-1690-1742;pass:END;sub:END;*/

Shader "Ikari Vertex Painter/Mobile/Diffuse/Vertex Color Color Blend" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("MainTex", 2D) = "white" {}
        _RedChannelColor ("Red Channel Color", Color) = (1,1,1,1)
        _GreenChannelColor ("Green Channel Color", Color) = (1,1,1,1)
        _BlueChannelColor ("Blue Channel Color", Color) = (1,1,1,1)
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
            #pragma exclude_renderers d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform fixed4 _Color;
            uniform fixed4 _RedChannelColor;
            uniform fixed4 _GreenChannelColor;
            uniform fixed4 _BlueChannelColor;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float4 vertexColor : COLOR;
                LIGHTING_COORDS(3,4)
                UNITY_FOG_COORDS(5)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 normalDirection = i.normalDir;
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
                fixed4 node_1906 = float4(i.vertexColor.rgb,(1.0 - i.vertexColor.a));
                float node_2299 = 0.0;
                fixed4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                fixed3 node_18 = (_Color.rgb*_MainTex_var.rgb);
                fixed node_1682 = 2.0;
                float3 diffuseColor = (lerp( lerp( lerp( lerp( float3(node_2299,node_2299,node_2299), (saturate((node_18*_RedChannelColor.rgb))*node_1682), node_1906.r ), (saturate((node_18*_GreenChannelColor.rgb))*node_1682), node_1906.g ), (saturate((node_18*_BlueChannelColor.rgb))*node_1682), node_1906.b ), node_18, node_1906.a ));
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
            #pragma exclude_renderers d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform fixed4 _Color;
            uniform fixed4 _RedChannelColor;
            uniform fixed4 _GreenChannelColor;
            uniform fixed4 _BlueChannelColor;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float4 vertexColor : COLOR;
                LIGHTING_COORDS(3,4)
                UNITY_FOG_COORDS(5)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 normalDirection = i.normalDir;
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
                float NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                fixed4 node_1906 = float4(i.vertexColor.rgb,(1.0 - i.vertexColor.a));
                float node_2299 = 0.0;
                fixed4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                fixed3 node_18 = (_Color.rgb*_MainTex_var.rgb);
                fixed node_1682 = 2.0;
                float3 diffuseColor = (lerp( lerp( lerp( lerp( float3(node_2299,node_2299,node_2299), (saturate((node_18*_RedChannelColor.rgb))*node_1682), node_1906.r ), (saturate((node_18*_GreenChannelColor.rgb))*node_1682), node_1906.g ), (saturate((node_18*_BlueChannelColor.rgb))*node_1682), node_1906.b ), node_18, node_1906.a ));
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
