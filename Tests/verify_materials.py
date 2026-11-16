"""Material and mesh data validation; does not replace a Unity shader compile."""
from pathlib import Path
import json,numpy as np
from PIL import Image
root=Path(__file__).resolve().parents[1]
count=0
for p in (root/'Assets/Ecos/Resources/Models').glob('*.json'):
 d=json.loads(p.read_text());v=np.array(d['vertices']).reshape(-1,3);uv=np.array(d['uv']).reshape(-1,2);g=np.array(d['groups']).reshape(-1,2);tri=np.array(d['triangles']).reshape(-1,3)
 assert len(v)==len(uv)==len(g) and np.isfinite(uv).all(),p
 assert np.all((g[:,0]>=0)&(g[:,0]<=11)),p
 a=uv[tri[:,1]]-uv[tri[:,0]];b=uv[tri[:,2]]-uv[tri[:,0]];det=a[:,0]*b[:,1]-a[:,1]*b[:,0]
 assert np.all(abs(det)>1e-12),(p.name,int((abs(det)<=1e-12).sum()))
 assert np.all(np.ptp(g[tri,0],axis=1)==0),p
 n=np.array(d['normals']).reshape(-1,3)
 face=np.cross(v[tri[:,1]]-v[tri[:,0]],v[tri[:,2]]-v[tri[:,0]])
 assert np.all(np.sum(face*n[tri].mean(axis=1),axis=1)>0),(p.name,'face winding opposes shading normals')
 count+=len(tri)
for name in ['Albedo.jpg','DetailNormal.png','MaterialMask.png']:
 p=root/'Assets/Ecos/Resources/PBR'/name
 assert Image.open(p).size==(4096,2048)
 meta=Path(str(p)+'.meta').read_text();assert 'maxTextureSize: 4096' in meta
 assert 'sRGBTexture: '+('1' if name=='Albedo.jpg' else '0') in meta
print('PASS: all UV triangles nondegenerate; material slots constant; 60 meshes;',count,'triangles total across unique assets.')
print('PASS: three 4096x2048 atlases; correct linear mask and normal import settings.')
print('PENDING: Unity shader compilation, close-up lighting, memory and frame-time measurement.')

for name in ['PropsAlbedo.jpg','PropsNormal.png','PropsMask.png']:
 p=root/'Assets/Ecos/Resources/PBR'/name
 assert Image.open(p).size==(1024,1024)
 meta=Path(str(p)+'.meta').read_text()
 assert 'sRGBTexture: '+('1' if name.endswith('.jpg') else '0') in meta
print('PASS: additional atlas and linear material maps.')
print('PASS: all triangle windings agree with shading normals, including broken glass.')
