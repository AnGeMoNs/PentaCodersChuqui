"""Physical pose and audio checks for revision 11. No Unity execution claimed."""
from pathlib import Path
import json,sys,wave,numpy as np
ROOT=Path(__file__).resolve().parents[1];sys.path.insert(0,str(ROOT/'SourceArt'))
from sebastian_pose import pose
spec=json.loads((ROOT/'Assets/Ecos/Resources/Sebastian/Motion.json').read_text())
d=json.loads((ROOT/'Assets/Ecos/Resources/Models/Sebastian.json').read_text());feet=np.isin(d['bone0'],[15,16])
assert len(spec['attacks'])==3
count=0;global_min=np.full(3,np.inf);global_max=-global_min
for variant,attack in enumerate(spec['attacks']):
 times=[k['time'] for k in attack['keys']];assert times==sorted(set(times)) and times[0]==0 and times[-1]==1.2
 for time in np.linspace(0,1.2,31):
  for moving in [0,1]:
   v,n=pose(d,chase=1,motion=moving,phase=time*8,attack=variant,attack_time=time)
   assert np.isfinite(v).all() and np.isfinite(n).all()
   # Match the actual SkinnedMeshRenderer conservative local bounds.
   assert np.all(v.min(0)>[-.9,-.25,-1.3]) and np.all(v.max(0)<[.9,1.95,1.1]),(variant,time,v.min(0),v.max(0))
   for foot in [15,16]:assert -.005<=v[np.array(d['bone0'])==foot,1].min()<=.105,(variant,time,'foot support')
   global_min=np.minimum(global_min,v.min(0));global_max=np.maximum(global_max,v.max(0));count+=1
for time in np.linspace(0,.75,20):
 v,n=pose(d,chase=1,alert=time)
 assert np.all(v.min(0)>[-.9,-.25,-1.3]) and np.all(v.max(0)<[.9,1.95,1.1]);count+=1
print('PASS:',count,'attack/recognition reference poses, finite skinning, feet supported and geometry inside renderer bounds.')
# Prove the three impact silhouettes differ, not merely their labels.
impacts=[pose(d,chase=1,attack=i,attack_time=.56)[0] for i in range(3)]
for i,j in [(0,1),(0,2),(1,2)]:assert np.max(np.linalg.norm(impacts[i]-impacts[j],axis=1))>.15
print('PASS: three distinct attack silhouettes at impact.')
clips=[]
for name in ['DreadLayer','ChaseLayer','PanicLayer']:
 with wave.open(str(ROOT/f'Assets/Ecos/Resources/Audio/{name}.wav'),'rb') as f:
  assert f.getframerate()==48000 and f.getnchannels()==1 and f.getnframes()==768000
  a=np.frombuffer(f.readframes(f.getnframes()),dtype='<i2').astype(float)/32767
  assert max(abs(a))<.45
  seam=abs(a[0]-a[-1]);allowed=max(.002,np.quantile(abs(np.diff(a)),.999)*1.5)
  assert seam<=allowed,(name,seam,allowed)
  clips.append(a)
assert max(abs(clips[0]*.15+clips[1]*.25+clips[2]*.18))<.30
print('PASS: three synchronized 16s/48kHz score layers, loop boundaries and conservative full-layer peak sum.')
for name in ['ThreatSting','AttackWhoosh','AttackHit']:
 with wave.open(str(ROOT/f'Assets/Ecos/Resources/Audio/{name}.wav'),'rb') as f:
  a=np.frombuffer(f.readframes(f.getnframes()),dtype='<i2').astype(float)/32767
  assert max(abs(a))<.45 and abs(a[0])<.001 and abs(a[-1])<.001
print('PASS: recognition, swing and impact cues have bounded peaks and faded endpoints.')
print('PENDING: Unity combat timing tests, real colliders, audio transitions, shader compilation and playtest.')
