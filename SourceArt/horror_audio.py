"""Original synthetic vocal and mechanical horror cues. No sampled performances.
Deterministic mono 22050 Hz, peak .42, fades and diffuse echo tails.
"""
from pathlib import Path
import numpy as np, wave
sr=22050
out=Path(__file__).resolve().parents[1]/'Assets/Ecos/Resources/Audio'
rng=np.random.default_rng(20261103)
def band(a,centers):
    f=np.fft.rfftfreq(len(a),1/sr);response=np.zeros(len(f))
    for center,width,gain in centers: response+=gain*np.exp(-.5*((f-center)/width)**2)
    return np.fft.irfft(np.fft.rfft(a)*response,n=len(a))
def save(name,a):
    tail=np.pad(a,(0,int(sr*1.7)));wet=tail.copy()
    for delay,gain in [(.117,.18),(.263,.15),(.431,.11),(.677,.08),(1.13,.04)]:
        offset=int(delay*sr);wet[offset:]+=tail[:-offset]*gain
    wet-=wet.mean();wet*=np.minimum(1,np.arange(len(wet))/(sr*.03))*np.minimum(1,np.arange(len(wet))[::-1]/(sr*.2))
    wet*=.42/max(np.abs(wet).max(),1e-9)
    with wave.open(str(out/(name+'.wav')),'wb') as w:
        w.setnchannels(1);w.setsampwidth(2);w.setframerate(sr);w.writeframes((wet*32767).astype('<i2').tobytes())
t=np.arange(int(sr*3.4))/sr
pitch=310+150*np.sin(np.minimum(t/3.4,1)*np.pi)+9*np.sin(2*np.pi*5.6*t)
phase=2*np.pi*np.cumsum(pitch)/sr
voice=sum(np.sin(phase*k)/k for k in range(1,22))
vowel=band(voice,[(750,210,1),(1350,260,.7),(2600,450,.35)])
breath=band(rng.normal(size=len(t)),[(1800,900,.16)])
env=np.sin(np.pi*t/3.4)**1.5*(.8+.2*np.sin(2*np.pi*4.5*t))
save('DistantScream',(vowel+breath)*env)
t=np.arange(int(sr*4.6))/sr
noise=band(rng.normal(size=len(t)),[(650,230,.5),(1800,450,1),(3500,650,.3)])
syllables=sum(np.exp(-((t-c)/width)**2) for c,width in [(.5,.18),(1,.24),(1.9,.35),(2.8,.17),(3.4,.35)])
save('GhostWhisper',noise*syllables)
t=np.arange(int(sr*2.7))/sr
scrape=band(rng.normal(size=len(t)),[(1200,350,.6),(2800,600,.3)])
save('MetalDrag',(scrape*(.5+.5*np.sin(t*29)**2)+.15*np.sin(t*2*np.pi*137))*np.sin(np.pi*t/2.7)**.7)
t=np.arange(int(sr*2.5))/sr
chord=sum(np.sin(2*np.pi*f*t+2*np.sin(t*1.3)) for f in [83,117,171,247])*.2
save('GhostArrival',(chord+band(rng.normal(size=len(t)),[(900,500,.12)]))*np.sin(np.pi*t/2.5)**1.4)
print('PASS: 4 original synthetic horror cues generated with fades, echo tails and controlled peaks.')
