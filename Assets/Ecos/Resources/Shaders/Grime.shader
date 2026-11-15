Shader "Ecos/OriginalVertexMaterial" {
 Properties {
  _Color("Tint",Color)=(1,1,1,1)
  _MainTex("Original surface atlas",2D)="white"{}
  _DetailNormal("Linear RGB detail normals",2D)="bump"{}
  _MaterialMask("R Metal G Smoothness B AO",2D)="white"{}
  _PropsTex("Additional surfaces",2D)="white"{}
  _PropsNormal("Additional normals",2D)="bump"{}
  _PropsMask("Additional masks",2D)="white"{}
  _NormalStrength("Relief",Range(0,2))=1.48
 }
 SubShader {
  Tags {"RenderType"="Opaque"}
  LOD 300
  CGPROGRAM
  #pragma surface surf Standard fullforwardshadows vertex:vert addshadow
  #pragma target 3.0
  sampler2D _MainTex,_DetailNormal,_MaterialMask,_PropsTex,_PropsNormal,_PropsMask;
  fixed4 _Color;half _NormalStrength;
  struct Input {float4 tint:COLOR;float2 detailUV;float slot;float3 worldPos;};
  void vert(inout appdata_full v,out Input o){UNITY_INITIALIZE_OUTPUT(Input,o);o.tint=v.color;o.detailUV=v.texcoord.xy;o.slot=v.texcoord1.x;}

  float hashP(float2 p){
   p=frac(p*float2(234.12,567.89));
   p+=dot(p,p+34.56);
   return frac(p.x*p.y);
  }
  float noiseP(float2 p){
   float2 i=floor(p);float2 f=frac(p);f=f*f*(3.0-2.0*f);
   return lerp(lerp(hashP(i),hashP(i+float2(1,0)),f.x),lerp(hashP(i+float2(0,1)),hashP(i+float2(1,1)),f.x),f.y);
  }
  void surf(Input IN,inout SurfaceOutputStandard o){
   float id=floor(IN.slot+.5);
   float2 cell=float2(fmod(id,4),1-floor(id/4));
   float2 tile=(frac(IN.detailUV)*.984375+.0078125);
   float2 uv=(cell+tile)/float2(4,2);
   float2 dx=ddx(IN.detailUV)*.984375/float2(4,2),dy=ddy(IN.detailUV)*.984375/float2(4,2);
   float3 albedo=tex2Dgrad(_MainTex,uv,dx,dy).rgb;
   float3 packed=tex2Dgrad(_MaterialMask,uv,dx,dy).rgb;
   float3 n=tex2Dgrad(_DetailNormal,uv,dx,dy).xyz*2-1;
   float3 tint=IN.tint.rgb;
   if(id<.5)tint=saturate(tint*1.65+.08);
   else if(id<1.5)tint=float3(.9,.93,.94);
   else if(id<2.5)tint=saturate(tint*1.3);
   else if(id<3.5)tint=float3(.9,.86,.8);
   if(id>5.5&&id<7.5){albedo=1;n=float3(0,0,1);packed=float3(0,.24,1);}
   if(id>7.5){
    float sid=id-8;float2 puv=(float2(fmod(sid,2),1-floor(sid/2))+frac(IN.detailUV)*.96875+.015625)/2;
    float2 pdx=ddx(IN.detailUV)*.484375,pdy=ddy(IN.detailUV)*.484375;
    albedo=tex2Dgrad(_PropsTex,puv,pdx,pdy).rgb;
    n=tex2Dgrad(_PropsNormal,puv,pdx,pdy).rgb*2-1;packed=tex2Dgrad(_PropsMask,puv,pdx,pdy).rgb;
    tint=float3(.9,.9,.9);
   }
   // Desgaste hospitalario realista: manchas de óxido, suciedad y salpicaduras de sangre en camillas, mesas y puertas
   float wNoise=noiseP(IN.worldPos.xz*2.4+IN.detailUV*1.8);
   float bNoise=noiseP(IN.worldPos.xz*3.7-IN.detailUV*2.9+float2(IN.worldPos.y*2.1,0));
   float hospitalDepth=saturate(IN.worldPos.z/105.0);
   float propBlood=smoothstep(0.68-hospitalDepth*0.10,0.82-hospitalDepth*0.08,bNoise)*(id<3.5?0.85:0.45);
   float propGrime=saturate((1.0-wNoise)*0.35+(1.0-saturate(IN.worldPos.y*1.4))*0.28);

   float cavity=saturate(1.0-dot(n.xy,n.xy)*.42);
   float groundAO=lerp(.68,1.0,saturate(IN.worldPos.y*2.2));
   float3 baseSurface=albedo*tint*_Color.rgb*lerp(.76,1.0,cavity)*groundAO*(1.0-propGrime*0.28);
   float3 bloodStain=lerp(float3(0.18,0.014,0.012),float3(0.44,0.025,0.018),saturate(bNoise*1.3-0.3));
   o.Albedo=lerp(baseSurface,bloodStain,saturate(propBlood*0.78));
   o.Normal=normalize(float3(n.xy*_NormalStrength,max(.1,n.z)));
   o.Metallic=saturate(packed.r+(id>0.5&&id<1.5?0.12:0.0));
   o.Smoothness=saturate(packed.g*1.15+propBlood*0.32-propGrime*0.18);
   o.Occlusion=saturate(packed.b*cavity*groundAO);
   o.Alpha=1;
  }
  ENDCG
 }
 FallBack "Diffuse"
}
