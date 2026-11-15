Shader "Ecos/AgedArchitecture" {
 Properties {
  _Color("Paint",Color)=(.3,.4,.35,1) _Tiled("Tiled floor",Float)=0
  _MainTex("Surface atlas",2D)="white"{}
  _DetailNormal("Detail normals",2D)="bump"{}
  _MaterialMask("Metal Smoothness AO",2D)="white"{}
  _NormalStrength("Surface relief",Range(0,2))=1.65
 }
 SubShader {
  Tags {"RenderType"="Opaque"}
  CGPROGRAM
  #pragma surface surf Standard fullforwardshadows
  #pragma target 3.0
  sampler2D _MainTex,_MaterialMask,_DetailNormal;fixed4 _Color;float _Tiled,_NormalStrength;
  struct Input {float3 worldPos;float3 worldNormal;float3 viewDir; INTERNAL_DATA};

  float hash21(float2 p){
   p=frac(p*float2(123.34,456.21));
   p+=dot(p,p+45.32);
   return frac(p.x*p.y);
  }
  float noise2D(float2 p){
   float2 i=floor(p);float2 f=frac(p);
   f=f*f*(3.0-2.0*f);
   float a=hash21(i);
   float b=hash21(i+float2(1,0));
   float c=hash21(i+float2(0,1));
   float d=hash21(i+float2(1,1));
   return lerp(lerp(a,b,f.x),lerp(c,d,f.x),f.y);
  }
  float fbm(float2 p){
   float v=0.0;float amp=0.5;
   v+=noise2D(p)*amp; p*=2.03; amp*=0.5;
   v+=noise2D(p)*amp; p*=2.01; amp*=0.5;
   v+=noise2D(p)*amp;
   return v;
  }
  float3 sampleAtlasCell(sampler2D map,float2 p,float2 cell){
   float2 scale=float2(.984375/4,.984375/2);
   float2 uv=(cell+frac(p)*.984375+.0078125)/float2(4,2);
   return tex2Dgrad(map,uv,ddx(p)*scale,ddy(p)*scale).rgb;
  }
  void surf(Input IN,inout SurfaceOutputStandard o){
   float3 p=IN.worldPos*.52;
   float3 baseNormal=normalize(WorldNormalVector(IN,float3(0,0,1)));
   float3 w=pow(abs(baseNormal),8);w/=max(.001,w.x+w.y+w.z);
   bool isFloor=_Tiled>.5;
   // Cell (0,0)=Plaster, (1,0)=Tiles, (0,1)=Enamel, (1,1)=Steel, (2,0)=Blood/Grime, (3,0)=Rust/Corrosion
   float2 baseCell=isFloor?float2(1,0): (IN.worldPos.y<1.12?float2(0,1):float2(0,0));
   float2 wearCell=isFloor?float2(3,0):float2(2,0);

   float3 aBase=sampleAtlasCell(_MainTex,p.yz,baseCell)*w.x+sampleAtlasCell(_MainTex,p.xz,baseCell)*w.y+sampleAtlasCell(_MainTex,p.xy,baseCell)*w.z;
   float3 aWear=sampleAtlasCell(_MainTex,p.yz*.65,wearCell)*w.x+sampleAtlasCell(_MainTex,p.xz*.65,wearCell)*w.y+sampleAtlasCell(_MainTex,p.xy*.65,wearCell)*w.z;
   float3 mask=sampleAtlasCell(_MaterialMask,p.yz,baseCell)*w.x+sampleAtlasCell(_MaterialMask,p.xz,baseCell)*w.y+sampleAtlasCell(_MaterialMask,p.xy,baseCell)*w.z;

   float2 nx=sampleAtlasCell(_DetailNormal,p.yz,baseCell).rg*2-1;
   float2 ny=sampleAtlasCell(_DetailNormal,p.xz,baseCell).rg*2-1;
   float2 nz=sampleAtlasCell(_DetailNormal,p.xy,baseCell).rg*2-1;

   // Juntas de baldosas cerámicas y grietas estructurales para mayor realismo en sombras rasantes
   float2 tileGrid=abs(frac(IN.worldPos.xz*1.25)-0.5);
   float groutLine=isFloor?smoothstep(0.465,0.495,max(tileGrid.x,tileGrid.y)):0.0;
   float2 wallTileGrid=abs(frac(float2(IN.worldPos.x+IN.worldPos.z,IN.worldPos.y)*1.6)-0.5);
   float wallGrout=(!isFloor&&IN.worldPos.y<1.12)?smoothstep(0.46,0.492,max(wallTileGrid.x,wallTileGrid.y))*0.65:0.0;

   // Suciedad, humedad ascendente, moho negro y filtraciones del techo
   float damp=1.0-smoothstep(0.0,1.45,IN.worldPos.y);
   float ceilingLeak=smoothstep(2.25,3.22,IN.worldPos.y);
   float leakStreaks=fbm(float2((IN.worldPos.x+IN.worldPos.z)*2.4,IN.worldPos.y*0.45));
   float grimeNoise=fbm(IN.worldPos.xz*0.55+float2(IN.worldPos.y*0.35,0));
   float rustNoise=fbm(IN.worldPos.xz*1.15-float2(0,IN.worldPos.y*0.85));

   // Progresión de deterioro Silent Hill / Resident Evil según la profundidad del sector (Z)
   float hospitalDepth=saturate(IN.worldPos.z/105.0);
   float rustMask=saturate((rustNoise-0.46)*2.4)*(0.32+0.55*hospitalDepth+0.35*damp);

   // Manchas realistas de sangre coagulada, salpicaduras arteriales y rastros de arrastre
   float2 bloodUV=isFloor?IN.worldPos.xz*0.42:float2((IN.worldPos.x+IN.worldPos.z)*0.55,IN.worldPos.y*0.75);
   float bloodMacro=fbm(bloodUV);
   float bloodDetail=fbm(bloodUV*3.1+float2(4.2,1.7));
   // Chorretones verticales de sangre en las paredes
   float wallDrip=(!isFloor&&IN.worldPos.y<2.1)?saturate(sin((IN.worldPos.x+IN.worldPos.z)*6.5)*cos((IN.worldPos.x-IN.worldPos.z)*4.3))*saturate(1.0-IN.worldPos.y/2.1):0.0;
   float bloodRaw=bloodMacro*0.68+bloodDetail*0.32+(isFloor?0.08:wallDrip*0.24-0.06);
   float bloodThreshold=lerp(0.64,0.52,hospitalDepth)-damp*0.05;
   float bloodPatch=smoothstep(bloodThreshold,bloodThreshold+0.14,bloodRaw);
   float bloodCore=smoothstep(bloodThreshold+0.09,bloodThreshold+0.22,bloodRaw);

   // Si el material fue creado explícitamente como decal de sangre/óxido (por tinte rojizo), potenciar el efecto
   float isBloodDecal=saturate((_Color.r-_Color.g)*3.2);
   bloodPatch=saturate(bloodPatch+isBloodDecal*0.85);
   bloodCore=saturate(bloodCore+isBloodDecal*0.65);

   // Charcos húmedos de agua y sangre en el suelo
   float puddle=isFloor?saturate(sin(IN.worldPos.x*1.35+grimeNoise*2.0)*cos(IN.worldPos.z*1.15-grimeNoise*1.8)-0.28):0.0;
   float wetBlood=max(puddle*0.5,bloodCore);

   // Perturbación de normales tridimensionales (micro-cavidades + grietas + menisco de sangre)
   float2 groutGrad=isFloor?(tileGrid-0.5)*groutLine*2.2:float2(0,0);
   float2 bloodGrad=float2(ddx(bloodPatch),ddy(bloodPatch))*14.0;
   float3 perturb=float3(0,nx.x,nx.y)*w.x+float3(ny.x+groutGrad.x+bloodGrad.x,0,ny.y+groutGrad.y+bloodGrad.y)*w.y+float3(nz.x,nz.y,0)*w.z;
   float microCavity=saturate(1.0-dot(perturb,perturb)*.46)*(1.0-groutLine*0.45-wallGrout*0.35);
   perturb-=baseNormal*dot(baseNormal,perturb);
   float3 wn=normalize(baseNormal+perturb*(_NormalStrength*(1.0+rustMask*0.35)));
   o.Normal=normalize(float3(dot(wn,normalize(WorldNormalVector(IN,float3(1,0,0)))),dot(wn,normalize(WorldNormalVector(IN,float3(0,1,0)))),dot(wn,baseNormal)));

   // Composición de color PBR: pintura descascarada + óxido Silent Hill + hollín + sangre arterial y coagulada
   float3 baseCol=lerp(aBase,aWear,saturate(rustMask*0.65+grimeNoise*0.25))*saturate(_Color.rgb*1.72+.10);
   float3 rustCol=float3(0.32,0.13,0.065)*aWear;
   float3 moldCol=float3(0.05,0.07,0.055);
   float3 freshBloodCol=float3(0.46,0.024,0.018);
   float3 driedBloodCol=float3(0.16,0.012,0.010);

   float3 surfaceCol=lerp(baseCol,rustCol,saturate(rustMask*0.72));
   surfaceCol=lerp(surfaceCol,moldCol,saturate((damp*0.55+ceilingLeak*0.48)*leakStreaks));
   surfaceCol*=(1.0-groutLine*0.52-wallGrout*0.38)*lerp(0.72,1.05,grimeNoise);

   float3 bloodCol=lerp(driedBloodCol,freshBloodCol,bloodCore);
   surfaceCol=lerp(surfaceCol,bloodCol,saturate(bloodPatch*0.92));

   o.Albedo=surfaceCol*lerp(.74,1.0,microCavity);
   o.Metallic=saturate(mask.r+rustMask*0.28+puddle*0.18+bloodCore*0.08);
   o.Smoothness=saturate(mask.g*(1.0-rustMask*0.45)+damp*0.18+puddle*0.52+bloodPatch*0.35+bloodCore*0.48-groutLine*0.35);
   o.Occlusion=saturate(mask.b*microCavity*(1.0-damp*0.16)*(1.0-rustMask*0.12));
   o.Alpha=1;
  }
  ENDCG
 }
 FallBack "Diffuse"
}
