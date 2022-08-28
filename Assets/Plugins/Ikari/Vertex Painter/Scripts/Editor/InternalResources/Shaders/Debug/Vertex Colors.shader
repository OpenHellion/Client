// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.26 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,rfrpo:False,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:1,x:33369,y:32668,varname:node_1,prsc:2|emission-8123-OUT;n:type:ShaderForge.SFN_VertexColor,id:861,x:32345,y:32595,varname:node_861,prsc:2;n:type:ShaderForge.SFN_SwitchProperty,id:4299,x:32643,y:32422,ptovrint:False,ptlb:Red Channel,ptin:_RedChannel,varname:node_4299,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:True|A-540-OUT,B-861-R;n:type:ShaderForge.SFN_SwitchProperty,id:735,x:32643,y:32579,ptovrint:False,ptlb:Green Channel,ptin:_GreenChannel,varname:node_735,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:True|A-540-OUT,B-861-G;n:type:ShaderForge.SFN_SwitchProperty,id:2159,x:32643,y:32743,ptovrint:False,ptlb:Blue Channel,ptin:_BlueChannel,varname:node_2159,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:True|A-540-OUT,B-861-B;n:type:ShaderForge.SFN_SwitchProperty,id:7865,x:32643,y:32899,ptovrint:False,ptlb:Alpha Channel,ptin:_AlphaChannel,varname:node_7865,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:True|A-540-OUT,B-861-A;n:type:ShaderForge.SFN_Append,id:8123,x:32926,y:32645,varname:node_8123,prsc:2|A-4299-OUT,B-735-OUT,C-2159-OUT,D-7865-OUT;n:type:ShaderForge.SFN_Vector1,id:540,x:32345,y:32516,varname:node_540,prsc:2,v1:0;proporder:4299-735-2159-7865;pass:END;sub:END;*/

Shader "Ikari Vertex Painter/Debug/Vertex Colors" {
    Properties {
        [MaterialToggle] _RedChannel ("Red Channel", Float ) = 1
        [MaterialToggle] _GreenChannel ("Green Channel", Float ) = 1
        [MaterialToggle] _BlueChannel ("Blue Channel", Float ) = 1
        [MaterialToggle] _AlphaChannel ("Alpha Channel", Float ) = 1
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
            #pragma exclude_renderers metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform fixed _RedChannel;
            uniform fixed _GreenChannel;
            uniform fixed _BlueChannel;
            uniform fixed _AlphaChannel;
            struct VertexInput {
                float4 vertex : POSITION;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float4 vertexColor : COLOR;
                UNITY_FOG_COORDS(0)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.vertexColor = v.vertexColor;
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
////// Lighting:
////// Emissive:
                float node_540 = 0.0;
                float3 emissive = float4(lerp( node_540, i.vertexColor.r, _RedChannel ),lerp( node_540, i.vertexColor.g, _GreenChannel ),lerp( node_540, i.vertexColor.b, _BlueChannel ),lerp( node_540, i.vertexColor.a, _AlphaChannel )).rgb;
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
