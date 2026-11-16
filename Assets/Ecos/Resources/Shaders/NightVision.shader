Shader "Hidden/Ecos/NightVision" {
 Properties {
  _MainTex ("Camera image",2D)="white" {}
  _Interference ("Paranormal interference",Range(0,1))=0
  _NightVisionOn ("Night vision active",Float)=0
  _Brightness ("User brightness",Range(0,1))=0.62
 }
 SubShader {
  Cull Off ZWrite Off ZTest Always
  Pass {
   CGPROGRAM
   #pragma vertex vert_img
   #pragma fragment frag
   #include "UnityCG.cginc"
   sampler2D _MainTex; float _NoiseTime,_NoiseAmount,_Interference,_NightVisionOn,_Brightness;
   fixed4 frag(v2f_img i):SV_Target {
    float2 uv=i.uv;
    float distCenter=length((uv-.5)*float2(1.22,1));
    float grain=frac(sin(dot(uv*float2(1920,1080)+floor(_NoiseTime*24),float2(12.9898,78.233)))*43758.5453)-.5;
    if(_NightVisionOn<.5) {
     // Modo sin cámara mejorado: penumbra atmosférica estilo Resident Evil / Silent Hill
     // Permite apreciar texturas, manchas de sangre, reflejos húmedos y sombras dinámicas cercanas
     // sin perder la atmósfera opresiva ni la utilidad de la visión nocturna IR a larga distancia.
     float3 raw=tex2D(_MainTex,uv).rgb;
     float2 texel=float2(1.0/1920.0,1.0/1080.0);
     float3 neigh=tex2D(_MainTex,uv+float2(texel.x,0)).rgb+tex2D(_MainTex,uv-float2(texel.x,0)).rgb
                 +tex2D(_MainTex,uv+float2(0,texel.y)).rgb+tex2D(_MainTex,uv-float2(0,texel.y)).rgb;
     float3 sharp=max(0,raw+(raw-neigh*0.25)*0.55);
     float userGain=lerp(0.68,1.55,saturate(_Brightness));
     float3 shadowLift=float3(0.018,0.022,0.027)*lerp(0.65,1.45,saturate(_Brightness));
     float3 mapped=1.0-exp(-(sharp*userGain+shadowLift)*2.15);
     mapped=pow(saturate(mapped),0.88);
     float luma=dot(mapped,float3(0.2126,0.7152,0.0722));
     // Realce de contraste entre sombras frías y tonos cálidos/rojos de sangre y óxido
     float3 coldShadow=mapped*float3(0.92,0.97,1.08);
     float3 bloodWarm=mapped*float3(1.12,0.94,0.90);
     float redDom=saturate((mapped.r-mapped.g)*2.8);
     float3 graded=lerp(coldShadow,bloodWarm,saturate(luma*1.4+redDom*0.65));
     float darkVignette=1.0-smoothstep(0.34,0.95,distCenter)*0.44;
     float subtleGrain=grain*0.016*_NoiseAmount;
     float3 finalNoCam=saturate(graded*darkVignette+subtleGrain);
     return fixed4(finalNoCam,1);
    }
    float glitchBand=sin(uv.y*95.0+_NoiseTime*38.0)*sin(uv.y*19.0-_NoiseTime*14.0);
    float tear=step(.62,abs(glitchBand))*_Interference*.022*sin(_NoiseTime*60.0);
    uv.x=saturate(uv.x+tear);
    float chroma=.0015+_Interference*.0055;
    float rCh=tex2D(_MainTex,saturate(uv+float2(chroma,0))).r;
    float gCh=tex2D(_MainTex,uv).g;
    float bCh=tex2D(_MainTex,saturate(uv-float2(chroma,0))).b;
    float3 source=float3(rCh,gCh,bCh);
    float luminance=dot(source,float3(.2126,.7152,.0722));
    float exposure=1-exp(-luminance*(6.8+1.8*saturate(_Brightness)));
    float vignette=1-smoothstep(.24-.08*_Interference,.76,distCenter)*(.64+.20*_Interference);
    float scan=1-(_NoiseAmount*.022+_Interference*.05)*sin(i.uv.y*1800+_NoiseTime*12);
    float staticBurst=grain*(.05*_NoiseAmount+.18*_Interference);
    float flicker=1-_Interference*.20*abs(sin(_NoiseTime*47.0));
    float value=saturate(exposure+staticBurst)*vignette*scan*flicker;
    float3 tint=lerp(float3(.24,1,.44),float3(.42,.92,.52),_Interference*.4);
    return fixed4(value*tint,1);
   }
   ENDCG
  }
 }
}
