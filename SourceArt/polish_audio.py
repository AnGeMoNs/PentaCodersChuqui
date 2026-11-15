"""Original mechanical sound design; no external recordings. Python + numpy."""
import numpy as np,wave
from pathlib import Path
out=Path(__file__).resolve().parents[1]/'Assets/Ecos/Resources/Audio';sr=22050;rng=np.random.default_rng(456)
def write(name,a):
 a=a/(max(np.max(np.abs(a)),1e-8))*.42
 with wave.open(str(out/(name+'.wav')),'wb') as w:w.setnchannels(1);w.setsampwidth(2);w.setframerate(sr);w.writeframes((a*32767).astype('<i2').tobytes())
t=np.arange(int(sr*1.4))/sr;n=rng.normal(0,1,len(t));rough=n-np.convolve(n,np.ones(35)/35,mode='same');envelope=np.clip(t/.09,0,1)*np.clip((1.4-t)/.22,0,1)
a=(rough*.12+.08*np.sin(t*2*np.pi*(90+16*np.sin(t*4)))+.035*np.sin(t*2*np.pi*677))*envelope
write('DoorSlide',a)
t=np.arange(int(sr*1.8))/sr;a=np.zeros(len(t))
for start in [0,.18]:
 u=np.maximum(0,t-start);mask=t>=start
 a+=mask*(np.sin(u*2*np.pi*193)*np.exp(-u*8)+.4*np.sin(u*2*np.pi*483)*np.exp(-u*12)+rng.normal(0,.2,len(t))*np.exp(-u*65))
write('PipeKnock',a)
