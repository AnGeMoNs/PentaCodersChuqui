"""Validate rig topology, skin weights, bind reconstruction and offline reference poses."""
from pathlib import Path
import json,sys,numpy as np,wave
root=Path(__file__).resolve().parents[1];sys.path.insert(0,str(root/'SourceArt'))
from sebastian_pose import pose
p=root/'Assets/Ecos/Resources/Models/Sebastian.json';d=json.loads(p.read_text());v=np.array(d['vertices']).reshape(-1,3);count=len(v)
assert len(d['rig'])==17
for i,b in enumerate(d['rig']):assert -1<=b['parent']<i
for key in ['bone0','bone1','weight0']:assert len(d[key])==count
ids=np.array([d['bone0'],d['bone1']]);weights=np.array(d['weight0']);assert ids.min()>=0 and ids.max()<17
assert np.isfinite(weights).all() and np.all((weights>=.5)&(weights<=1))
assert np.allclose(pose(d,bind=True)[0],v,atol=1e-7)
minimum=1.;maximum=-1.
for phase in np.linspace(0,np.pi*2,25):
 posed,normals=pose(d,chase=1,motion=1,phase=phase,clock=1,voice=True)
 assert np.isfinite(posed).all() and np.isfinite(normals).all()
 assert np.all(posed.min(0)>[-.9,-.25,-1.3]) and np.all(posed.max(0)<[.9,1.95,1.1]),'Rig exceeds renderer bounds'
 for foot in [15,16]:
  sole=posed[np.array(d['bone0'])==foot,1].min();minimum=min(minimum,sole);maximum=max(maximum,sole)
  assert -.005<=sole<=.105,('Foot support error',phase,foot,sole)
assert np.linalg.norm(pose(d,chase=1,motion=1,phase=1)[0]-v,axis=1).max()>.25
jaw=np.array(d['bone0'])==4
assert np.max(np.linalg.norm(pose(d,voice=True)[0][jaw]-pose(d,voice=False)[0][jaw],axis=1))>.005
for name in ['SebastianSob','SebastianSobB','SebastianSnarl','SebastianSnarlB','SebastianScream','SebastianScreamB','SebastianBreath','SebastianStep']:
 with wave.open(str(root/f'Assets/Ecos/Resources/Audio/{name}.wav'),'rb') as f:
  assert f.getnchannels()==1 and f.getframerate()==(22050 if name=='SebastianStep' else 48000)
  audio=np.frombuffer(f.readframes(f.getnframes()),dtype='<i2')/32767
  assert np.isfinite(audio).all() and max(abs(audio))<.46
  assert abs(audio[0])<.001 and abs(audio[-1])<.001
print('PASS: 17-bone hierarchy, valid normalized skin weights, bind-pose reconstruction and 25 finite chase poses.')
print('PASS: offline reference foot height range:',round(minimum,4),'to',round(maximum,4),'m; jaw and limbs move.')
print('PASS: 7 recorded voice derivatives at 48kHz and 1 original footstep, bounded peaks and quiet endpoints.')
print('PENDING: Unity compilation, in-game rig motion, listening and frame-time measurement.')

from PIL import Image
channels=np.array(d['groups']).reshape(-1,2)
assert set(channels[:,0].astype(int))==set(range(6))
assert np.all((channels[:,1]>=0)&(channels[:,1]<=1))
for name in ['FaceAlbedo.jpg','FaceNormal.png','ClothAlbedo.jpg','ClothNormal.png']:
 p=root/'Assets/Ecos/Resources/Sebastian'/name
 image=Image.open(p);image.load();assert image.size==(1254,1254)
 meta=Path(str(p)+'.meta').read_text()
 assert 'sRGBTexture: '+('1' if 'Albedo' in name else '0') in meta
 assert 'maxTextureSize: 2048' in meta and 'nPOTScale: 0' in meta
shader=(root/'Assets/Ecos/Resources/Shaders/Ghost.shader').read_text()
assert 'addshadow' in shader and 'o.Emission=0' in shader
assert len(json.loads((root/'SourceArt/AudioSources/SOURCES.json').read_text()))==3
print('PASS: character material channels, four native 1254px maps, linear normal data and recorded-source credits.')
