"""Original adaptive horror score. Three 16-second synchronized layers, 120 BPM.
Procedural bowed-string timbres, low percussion, metal resonance and granular noise.
No external music, vocals, or commercial-game samples. numpy + scipy only.
"""
from pathlib import Path
import numpy as np,wave,hashlib,io,json
from scipy.signal import butter,sosfilt
ROOT=Path(__file__).resolve().parents[1];OUT=ROOT/'Assets/Ecos/Resources/Audio';SR=48000;DUR=16;N=SR*DUR
rng=np.random.default_rng(111985);t=np.arange(N)/SR

def periodic_noise(lo,hi):
 f=np.fft.rfftfreq(N,1/SR);spec=rng.normal(size=len(f))+1j*rng.normal(size=len(f));shape=np.zeros_like(f)
 gate=(f>lo)&(f<hi);shape[gate]=(np.sin(np.pi*(f[gate]-lo)/(hi-lo))**2)/(np.maximum(f[gate],1)**.5)
 x=np.fft.irfft(spec*shape,n=N);return x/(np.std(x)+1e-9)
def bowed(hz,seconds,seed,bright=.5):
 rr=np.random.default_rng(seed);tt=np.arange(int(seconds*SR))/SR;out=np.zeros(len(tt))
 for detune in [-.004,0,.003]:
  phase=2*np.pi*hz*(1+detune)*tt+.025*np.sin(2*np.pi*5.2*tt)
  for k in range(1,20):out+=np.sin(phase*k+rr.uniform(-.15,.15))*np.exp(-k/(3+bright*10))/k
 return out/3

def add(buffer,sound,when,gain=1):
 indices=(np.arange(len(sound))+int(when*SR))%len(buffer);np.add.at(buffer,indices,sound*gain)
def note(hz,duration,seed):
 a=bowed(hz,duration,seed,.65);u=np.arange(len(a))/SR
 return a*np.minimum(u/.025,1)*np.exp(-u*8)*np.minimum((duration-u)/.05,1)
def drum(duration=.65,seed=1):
 rr=np.random.default_rng(seed);u=np.arange(int(duration*SR))/SR
 a=np.sin(2*np.pi*(46*u+16*(1-np.exp(-u*16))/16))*np.exp(-u*9)
 for hz,gain in [(93,.35),(153,.18),(241,.08)]:a+=np.sin(2*np.pi*hz*u)*np.exp(-u*15)*gain
 noise=sosfilt(butter(2,2600,fs=SR,output='sos'),rr.normal(size=len(u)))
 return (a+noise*np.exp(-u*45)*.2)*np.minimum(u/.004,1)*np.minimum((duration-u)/.04,1)
# Stationary periodic strings, with exact-cycle frequencies for seamless boundaries.
dread=np.zeros(N)
for hz,gain in [(41.20,.7),(55,.24),(58.27,.18),(82.41,.12),(110,.10)]:
 for detune in [0,.003,-.004]:
  f=round(hz*(1+detune)*DUR)/DUR;phase=rng.uniform(0,2*np.pi)
  for k in range(1,9):
   dread+=np.sin(2*np.pi*f*k*t+phase)*gain*np.exp(-k/5)/k*.3
texture=periodic_noise(130,2400);dread+=texture*(.018+.01*np.sin(2*np.pi*t/8))
dread*=.70+.18*np.sin(2*np.pi*t/16)+.10*np.sin(2*np.pi*t/4)
chase=np.zeros(N);notes=[110,110,116.54,110,164.81,110,116.54,103.83]
for i in range(64):
 add(chase,note(notes[i%8],.46,110+i),i*.25,.65 if i%4==0 else .42)
 if i%2==0:add(chase,drum(seed=50+i),i*.25,.7 if i%8==0 else .38)
 if i%8 in [3,6]:add(chase,note(55,.35,900+i),i*.25,.35)
chase+=periodic_noise(900,4200)*(.012+.012*np.sin(2*np.pi*4*t)**2)
panic=np.zeros(N)
for hz,gain in [(440,.17),(466.16,.14),(622.25,.10),(659.25,.06)]:
 f=round(hz*16)/16;phase=2*np.pi*f*t+.09*np.sin(2*np.pi*6*t)
 panic+=(np.sin(phase)+.3*np.sin(phase*2)+.12*np.sin(phase*3))*gain*(.3+.7*np.sin(2*np.pi*4*t)**2)
panic+=periodic_noise(1700,7800)*.035*(.3+.7*np.sin(2*np.pi*8*t)**8)

def save(name,a,loop=False):
 a=np.asarray(a,dtype=float);a-=a.mean()
 if not loop:
  u=np.arange(len(a))/SR;a*=np.minimum(u/.007,1)*np.minimum((len(a)/SR-u)/.12,1)
 a*=.449/max(np.max(abs(a)),1e-9)
 b=io.BytesIO()
 with wave.open(b,'wb') as f:f.setnchannels(1);f.setsampwidth(2);f.setframerate(SR);f.writeframes(np.int16(a*32767).tobytes())
 (OUT/(name+'.wav')).write_bytes(b.getvalue())
 meta=OUT/(name+'.wav.meta')
 if not meta.exists():
  meta.write_text('''fileFormatVersion: 2
guid: '''+hashlib.md5(('ecos-r11-'+name).encode()).hexdigest()+'''
AudioImporter:
  externalObjects: {}
  serializedVersion: 7
  defaultSettings:
    serializedVersion: 2
    loadType: 0
    sampleRateSetting: 0
    sampleRateOverride: 48000
    compressionFormat: 0
    quality: 1
    conversionMode: 0
  forceToMono: 1
  normalize: 0
  preloadAudioData: 1
  loadInBackground: 0
  ambisonic: 0
  3D: 0
''')
 print(name,round(len(a)/SR,3),'seconds')
 return a
stems=[save(n,a,True) for n,a in [('DreadLayer',dread),('ChaseLayer',chase),('PanicLayer',panic)]]
u=np.arange(int(SR*2))/SR
sting=bowed(77.8,2,985,.95)*np.exp(-u*1.8)+bowed(82.4,2,1985,.9)*np.exp(-u*2)*.7
sting[:int(.65*SR)]+=drum(seed=97)*1.3
save('ThreatSting',sting)
u=np.arange(int(SR*.48))/SR;noise=rng.normal(size=len(u));noise=sosfilt(butter(2,[350,5800],btype='bandpass',fs=SR,output='sos'),noise)
save('AttackWhoosh',noise*np.sin(np.pi*u/.48)**2)
save('AttackHit',drum(.9,1985))
# Review mix: discovery 0-4s, pursuit 4-10s, immediate danger 10-13s, escape fade.
x=np.arange(N)/SR
threat=np.interp(x,[0,3,5,9,11,13,16],[0,.3,.65,.75,1,.7,0])
def smooth(a,b,z):
 q=np.clip((z-a)/(b-a),0,1);return q*q*(3-2*q)
mix=stems[0]*(.15*np.clip(threat*3,0,1))+stems[1]*(.25*smooth(.3,.8,threat))+stems[2]*(.18*smooth(.75,1,threat))
mix*=np.minimum(x/.05,1)*np.minimum((16-x)/.18,1)
b=io.BytesIO()
with wave.open(b,'wb') as f:f.setnchannels(1);f.setsampwidth(2);f.setframerate(SR);f.writeframes(np.int16(np.clip(mix*1.8,-1,1)*32767).tobytes())
(ROOT/'Docs/Musica_Persecucion_11.wav').write_bytes(b.getvalue())
(ROOT/'Docs/MUSIC_11.json').write_text(json.dumps({'bpm':120,'seconds':16,'sample_rate':SR,'samples':N,'stems':['DreadLayer','ChaseLayer','PanicLayer'],'provenance':'Original deterministic DSP; no external music samples'},indent=2))
