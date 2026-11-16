Shader "Ecos/GhostPresence" {
 Properties {
  _FaceTex("Sebastian face albedo",2D)="white"{}
  _FaceNormal("Face micro normals RGB linear",2D)="bump"{}
  _ClothTex("Aged pediatric gown",2D)="white"{}
  _ClothNormal("Cloth micro normals RGB linear",2D)="bump"{}
  _Presence("Presence",Range(0,1))=1
  _HauntTime("Encounter time",Float)=0
  _Hunt("Hunt intensity",Range(0,1))=0
 }
 SubShader {
  Tags {"RenderType"="TransparentCutout" "Queue"="AlphaTest"}
  CGPROGRAM
  #pragma surface surf Standard fullforwardshadows addshadow vertex:vert
  #pragma target 3.0
  sampler2D _FaceTex,_FaceNormal,_ClothTex,_ClothNormal;
  float _Presence,_HauntTime,_Hunt;
  struct Input {float2 detailUV;float2 category;float3 worldPos;};
  void vert(inout appdata_full v,out Input o) {
   UNITY_INITIALIZE_OUTPUT(Input,o);o.detailUV=v.texcoord.xy;o.category=v.texcoord1.xy;
   // Micro-espasmos viscerales en los vértices cuando Sebastián entra en furia de persecución (_Hunt > 0.5)
   if(_Hunt>.5) {
    float spasm=sin(_HauntTime*52.0+v.vertex.y*14.0)*cos(_HauntTime*37.0+v.vertex.x*19.0);
    v.vertex.xyz+=v.normal*(spasm*0.0045*_Hunt);
   }
  }
  float hashG(float2 p){
   p=frac(p*float2(127.1,311.7));
   p+=dot(p,p+34.2);
   return frac(p.x*p.y);
  }
  float noiseG(float2 p){
   float2 i=floor(p);float2 f=frac(p);f=f*f*(3.0-2.0*f);
   return lerp(lerp(hashG(i),hashG(i+float2(1,0)),f.x),lerp(hashG(i+float2(0,1)),hashG(i+float2(1,1)),f.x),f.y);
  }
  void surf(Input IN,inout SurfaceOutputStandard o) {
   float grain=frac(sin(dot(floor(IN.worldPos*160),float3(12.9898,78.233,45.17)))*43758.5453);
   clip(_Presence-grain-.00001);
   float category=IN.category.x;
   float2 skinUV=frac(IN.detailUV*3)*float2(.18,.12)+float2(.03,.82);
   float3 skin=tex2D(_FaceTex,skinUV).rgb;
   float veinNoise=noiseG(IN.detailUV*14.0+float2(0,_HauntTime*0.4));
   float bloodDrip=noiseG(float2(IN.detailUV.x*9.0,IN.detailUV.y*3.5-_HauntTime*0.25));
   float goreMask=saturate((veinNoise*0.6+bloodDrip*0.4-0.48)*(2.2+_Hunt*1.6));
   float3 bloodColor=lerp(float3(0.18,0.01,0.01),float3(0.52,0.02,0.015),saturate(bloodDrip*1.4));

   o.Normal=float3(0,0,1);o.Smoothness=.19;o.Occlusion=1;
   if(category<.5) {
    float3 face=tex2D(_FaceTex,saturate(IN.detailUV)).rgb;
    float3 baseFace=lerp(skin,face,saturate(IN.category.y));
    // Venas necróticas y lágrimas/boca ensangrentada durante la persecución
    o.Albedo=lerp(baseFace,bloodColor,saturate(goreMask*(0.45+0.48*_Hunt)));
    float3 micro=tex2D(_FaceNormal,saturate(IN.detailUV)).rgb*2-1;
    o.Normal=normalize(lerp(float3(0,0,1),micro,saturate(IN.category.y)*(.85+.35*_Hunt)));
    float eye=saturate(dot(face,float3(.2126,.7152,.0722))*8);
    o.Smoothness=saturate(lerp(.04,.32,eye)+goreMask*0.55);o.Occlusion=lerp(.32,1,eye);
   } else if(category<1.5) {
    float3 clothCol=tex2D(_ClothTex,IN.detailUV*2.8).rgb;
    float hemBlood=saturate(1.0-IN.detailUV.y*1.4)*0.65;
    float clothBlood=saturate(goreMask*(0.55+0.4*_Hunt)+hemBlood*veinNoise);
    o.Albedo=lerp(clothCol*(1.0-_Hunt*0.22),bloodColor,saturate(clothBlood*0.85));
    o.Normal=normalize(tex2D(_ClothNormal,IN.detailUV*2.8).rgb*2-1);
    o.Smoothness=saturate(.08+clothBlood*0.62);
   } else if(category<2.5) {
    o.Albedo=lerp(skin*(1.0-_Hunt*0.18),bloodColor,saturate(goreMask*(0.52+0.42*_Hunt)));
    o.Normal=normalize(tex2D(_FaceNormal,skinUV).rgb*2-1);
    o.Smoothness=saturate(0.22+goreMask*0.55);
   } else if(category<3.5) {
    float strand=.5+.5*sin(IN.detailUV.x*1900+sin(IN.detailUV.y*70)*2);
    o.Albedo=lerp(float3(.012,.011,.010),float3(.035,.018,.016),strand);o.Smoothness=.38;
   } else if(category<4.5) {
    o.Albedo=lerp(float3(.006,.004,.005),float3(.28,.01,.01),_Hunt*0.75);o.Smoothness=.45;o.Occlusion=.35;
   } else {
    o.Albedo=lerp(float3(.30,.28,.23),float3(.42,.04,.03),saturate(goreMask*0.7+_Hunt*0.5));o.Smoothness=.48;
   }
   o.Metallic=0;o.Emission=0;o.Alpha=1;
  }
  ENDCG
 }
 FallBack "Diffuse"
}
