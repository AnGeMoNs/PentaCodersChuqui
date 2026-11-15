"""Revision 10. Recorded CC0 voice sources, subtle processing and mono spatial cues.
Requires numpy, scipy and ffmpeg. Original source previews and credits are included.
No speech synthesis, oscillator vocal imitation, or voice cloning.
"""
from pathlib import Path
import subprocess,wave,hashlib,json
import numpy as np
from scipy.signal import butter,sosfilt
ROOT=Path(__file__).resolve().parents[1];SRC=ROOT/'SourceArt/AudioSources';OUT=ROOT/'Assets/Ecos/Resources/Audio';SR=48000

def read(name):
 raw=subprocess.check_output(['ffmpeg','-v','error','-i',str(SRC/(name+'.mp3')),'-f','f32le','-ac','1','-ar',str(SR),'-'])
 return np.frombuffer(raw,dtype='<f4').astype(float)
def cut(a,start,end):return a[int(start*SR):int(end*SR)].copy()
def rate(a,factor):return np.interp(np.arange(0,len(a)-1,factor),np.arange(len(a)),a)
def clean(a):
 return sosfilt(butter(2,[75,11500],btype='bandpass',fs=SR,output='sos'),a)
def pad(a,size):return np.pad(a,(0,max(0,size-len(a))))[:size]
def save(name,a,wet=.12):
 a=clean(a);a/=max(np.max(abs(a)),1e-8)
 # Gentle gain control retains real vocal attacks without hard clipping.
 a=np.tanh(a*1.12)/np.tanh(1.12)
 a=np.pad(a,(0,int(.55*SR)));dry=a.copy()
 for delay,gain in [(.071,wet),(.139,wet*.55),(.227,wet*.32),(.41,wet*.12)]:
  n=int(delay*SR);a[n:]+=dry[:-n]*gain
 fade=np.minimum(1,np.arange(len(a))/(SR*.018))*np.minimum(1,np.arange(len(a))[::-1]/(SR*.16));a*=fade
 a*=.449/max(np.max(abs(a)),1e-8)
 with wave.open(str(OUT/(name+'.wav')),'wb') as f:
  f.setnchannels(1);f.setsampwidth(2);f.setframerate(SR);f.writeframes((a*32767).astype('<i2').tobytes())
 # Preserve existing Unity GUIDs so existing scene references keep resolving.
 meta=OUT/(name+'.wav.meta')
 if not meta.exists():
  template=(OUT/'SebastianSnarl.wav.meta').read_text()
  import re
  template=re.sub(r'guid: \w+','guid: '+hashlib.md5(('ecos-r10/'+name).encode()).hexdigest(),template)
  meta.write_text(template)
 print(name,round(len(a)/SR,2),'sec',SR,'Hz peak',round(np.max(abs(a)),3))
child=read('ChildScream');monster=read('MonsterScreams');breath=read('MonsterBreaths')
save('SebastianScream',cut(child,.12,4.5),.10)
save('SebastianScreamB',cut(child,4.3,8.05),.12)
# Quiet distant vocalizations are real voice excerpts, with modest slowing.
save('SebastianSob',rate(cut(child,.12,3.1),.88),.18)
save('SebastianSobB',rate(cut(child,4.9,8.05),.92),.18)
for name,start,end,childstart in [('SebastianSnarl',.02,3.25,.4),('SebastianSnarlB',10.3,15.1,4.3)]:
 base=cut(monster,start,end);overlay=rate(cut(child,childstart,childstart+2.8),.94)
 base=base/max(np.max(abs(base)),1e-8);overlay/=max(np.max(abs(overlay)),1e-8)
 save(name,base+.28*pad(overlay,len(base)),.06)
save('SebastianBreath',cut(breath,.18,2.8),.06)
# SebastianStep is the retained original dragging footfall from revision 9.
assert (OUT/'SebastianStep.wav').exists()
print('7 recorded vocal cues replaced/generated; original footfall retained.')
