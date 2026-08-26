// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.26 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:2,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:True,fnfb:False;n:type:ShaderForge.SFN_Final,id:1,x:34336,y:32640,varname:node_1,prsc:2|emission-654-OUT;n:type:ShaderForge.SFN_Tex2d,id:3,x:33655,y:32751,ptovrint:False,ptlb:Blue Texture,ptin:_BlueTexture,varname:_BlueTexture,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:26,x:33655,y:32390,ptovrint:False,ptlb:Red Texture,ptin:_RedTexture,varname:_RedTexture,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:409,x:33655,y:32571,ptovrint:False,ptlb:Green Texture,ptin:_GreenTexture,varname:_GreenTexture,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_VertexColor,id:561,x:32673,y:33672,varname:node_561,prsc:2;n:type:ShaderForge.SFN_ChannelBlend,id:654,x:34057,y:32570,varname:node_654,prsc:2,chbt:0|M-561-RGB,R-26-RGB,G-409-RGB,B-3-RGB;proporder:26-409-3;pass:END;sub:END;*/

Shader "Ikari Vertex Painter/Unlit/Vertex Color RGB Blend" {
    Properties {
        _RedTexture ("Red Texture", 2D) = "white" {}
        _GreenTexture ("Green Texture", 2D) = "white" {}
        _BlueTexture ("Blue Texture", 2D) = "white" {}
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
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            #pragma exclude_renderers d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform sampler2D _BlueTexture; uniform float4 _BlueTexture_ST;
            uniform sampler2D _RedTexture; uniform float4 _RedTexture_ST;
            uniform sampler2D _GreenTexture; uniform float4 _GreenTexture_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 vertexColor : COLOR;
                UNITY_FOG_COORDS(1)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
////// Lighting:
////// Emissive:
                float4 _RedTexture_var = tex2D(_RedTexture,TRANSFORM_TEX(i.uv0, _RedTexture));
                float4 _GreenTexture_var = tex2D(_GreenTexture,TRANSFORM_TEX(i.uv0, _GreenTexture));
                float4 _BlueTexture_var = tex2D(_BlueTexture,TRANSFORM_TEX(i.uv0, _BlueTexture));
                float3 emissive = (i.vertexColor.rgb.r*_RedTexture_var.rgb + i.vertexColor.rgb.g*_GreenTexture_var.rgb + i.vertexColor.rgb.b*_BlueTexture_var.rgb);
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
