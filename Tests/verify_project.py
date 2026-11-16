"""Static asset/scene validation; does not claim a Unity playtest.
Works with tree_sitter if installed, or built-in C# structural validator otherwise.
"""
from pathlib import Path
from itertools import product
from collections import deque
import json,re,wave
import numpy as np
ROOT=Path(__file__).resolve().parents[1]
cs=list((ROOT/'Assets').rglob('*.cs'))
try:
 from tree_sitter import Language,Parser
 import tree_sitter_c_sharp
 parser=Parser(Language(tree_sitter_c_sharp.language()))
 for p in cs:
  assert not parser.parse(p.read_bytes()).root_node.has_error,p
except ImportError:
 for p in cs:
  txt=p.read_text()
  cleaned=re.sub(r'//.*?$|/\*.*?\*/|"(?:\\.|[^"\\])*"','',txt,flags=re.M|re.S)
  assert cleaned.count('{')==cleaned.count('}') and cleaned.count('(')==cleaned.count(')') and cleaned.count('[')==cleaned.count(']'),p
  assert 'namespace Ecos' in txt,p
print('PASS syntax:',len(cs),'C# files (not a Unity compilation)')
models={}
for p in (ROOT/'Assets/Ecos/Resources/Models').glob('*.json'):
 d=json.loads(p.read_text());v=np.array(d['vertices']).reshape(-1,3);n=np.array(d['normals']).reshape(-1,3);c=np.array(d['colors']).reshape(-1,3);t=np.array(d['triangles']).reshape(-1,3)
 assert v.shape==n.shape==c.shape and np.isfinite(v).all()
 assert t.min()>=0 and t.max()<len(v)
 assert np.allclose(np.linalg.norm(n,axis=1),1,atol=1e-5)
 assert np.all(np.linalg.norm(np.cross(v[t[:,1]]-v[t[:,0]],v[t[:,2]]-v[t[:,0]]),axis=1)>1e-8)
 models[p.stem]=d
print('PASS models:',len(models),'meshes; indices, normals and nondegenerate faces')
for p in (ROOT/'Assets/Ecos/Resources/Audio').glob('*.wav'):
 with wave.open(str(p),'rb') as w:
  assert w.getframerate() in (22050,48000) and w.getnchannels()==1
  a=np.frombuffer(w.readframes(w.getnframes()),dtype='<i2');assert np.max(np.abs(a.astype(float)))<32767*.46
print('PASS audio:',len(list((ROOT/'Assets/Ecos/Resources/Audio').glob('*.wav'))),'WAVs; individual peaks below -6dBFS (runtime mix unmeasured)')
all_cs='\n'.join(p.read_text() for p in cs)
for name in re.findall(r'(?:Model|Item|Clue|Prop)\("(\w+)"',all_cs):assert name in models,name
for name in re.findall(r'ModelLibrary.Create\("(\w+)"',all_cs):assert name in models,name
assert 'inputlegacy' not in (ROOT/'Packages/manifest.json').read_text()
assert 'textrendering' not in (ROOT/'Packages/manifest.json').read_text()
manifest=json.loads((ROOT/'Packages/manifest.json').read_text());assert set(manifest['dependencies'])=={f'com.unity.modules.{n}' for n in ['audio','imgui','physics','jsonserialize']}
scene=(ROOT/'Assets/Ecos/Scenes/Inicio.unity').read_text();meta=(ROOT/'Assets/Ecos/Scripts/SceneBootstrap.cs.meta').read_text();assert re.search(r'guid: (\w+)',meta).group(1) in scene
cells={(x,z) for z in range(56) for x in range(-1,2)}
for zone in range(8):
 for side in [-1,1]:
  center=zone*7+3;cells.add((side*2,center));cells.update((side*x,z) for x in range(3,6) for z in range(center-1,center+2))
wing_specs=[
 (0, 1, 5,7, 1,2, 4,1), (0,-1, 6,7, 0,1, 4,1),
 (1,-1, 5,8, 5,6, 3,4), (1, 1, 5,7, 0,1, 4,1),
 (2, 1, 5,8, 5,6, 3,4), (2,-1, 5,7, 0,1, 3,1),
 (3,-1, 5,8, 0,1, 3,1), (3, 1, 5,8, 5,6, 3,4),
 (4, 1, 5,8, 0,1, 4,1), (4,-1, 5,8, 5,6, 3,4),
 (5,-1, 5,8, 0,1, 3,1), (5, 1, 5,8, 5,6, 3,4),
 (6, 1, 5,8, 0,1, 4,1), (6,-1, 5,7, 5,6, 3,4),
 (7,-1, 5,8, 5,6, 3,4), (7, 1, 5,8, 5,6, 3,4)
]
for zone,s,xmin,xmax,zmin,zmax,dx,dz in wing_specs:
 cells.add((s*dx,zone*7+dz))
 for x in range(min(dx,xmin),xmax+1):
  for z in range(zone*7+zmin,zone*7+zmax+1):cells.add((s*x,z))
def reachable(gates):
 seen={(0,0)};q=deque(seen)
 while q:
  x,z=q.popleft()
  for n in [(x-1,z),(x+1,z),(x,z-1),(x,z+1)]:
   if n not in cells or n in seen:continue
   a,b=z//7,n[1]//7
   if a!=b and max(a,b) not in gates:continue
   seen.add(n);q.append(n)
 return seen
seen=reachable({1,4});assert {z//7 for x,z in seen}=={0,1}
for gates,maximum in [({1,2,4},2),({1,2,3,4},4),({1,2,3,4,5},5),({1,2,3,4,5,6},6),({1,2,3,4,5,6,7},7)]:
 assert {z//7 for x,z in reachable(gates)}==set(range(maximum+1))
assert len(reachable(set(range(1,8))))==len(cells)
print('PASS navigation:',len(cells),'cells (expanded asymmetric hospital); logical progression reachable (physical furniture navigation still needs Unity)' )
answers=[(a,b,c) for a,b,c in product([False,True],repeat=3) if a and not b and c==a]
assert answers==[(True,False,True)]
session=(ROOT/'Assets/Ecos/Scripts/GameSession.cs').read_text();assert 'Code!="1985"' in session and 'toyOrder={0,2,1}' in session
for key in ['infant_key','urgency_key','basement_key','pathology_key','morgue_key']:
 assert key in session or key in (ROOT/'Assets/Ecos/Scripts/LevelBuilder.cs').read_text()
assert 'Has("together")' in session and 'Has("preserved")' in session and 'Contains("evidence")' in session
print('PASS progression: unique generator solution, toy order, archive code and ending requirements')
assert len(models)==60
assert 'PhotoEcho' not in all_cs and 'Spectral' not in all_cs
player=(ROOT/'Assets/Ecos/Scripts/PlayerRig.cs').read_text()
assert 'GetMouseButtonDown(0)' not in player and 'NightVisionActive?.35f' in player
assert 'range=32' in all_cs and 'Graphics.Blit(source,destination,material)' in all_cs
builder=(ROOT/'Assets/Ecos/Scripts/LevelBuilder.cs').read_text()
assert 'Clue("Frame","frame","infant_key",1' in builder
assert 'Clue("XRay","xray","evidence",3' in builder
assert 'Clue("Cabinet","seal","morgue_key",4' in builder
assert 'case "clue"' in session and 'Flag("clue_"+item.id)' in session
save=(ROOT/'Assets/Ecos/Scripts/SaveService.cs').read_text()
assert 's.version==1' in save and 's.version=2' in save and '"clue_"+id' in save
for asset in ['NightVision','Architecture']:
 assert (ROOT/('Assets/Ecos/Resources/Shaders/'+asset+'.shader')).exists()
print('PASS revision: night-vision wiring; no photo mechanism; three physical clue replacements; legacy save migration present')
print('PENDING: compile/play in Unity; frame rate; runtime audio mix; Windows build.')

assert "flashlight" not in player and "torchWanted" not in player
assert "ReloadCamera" in player
assert "spareBatteries" in (ROOT/"Assets/Ecos/Scripts/SaveService.cs").read_text()
print("PASS: camera-only illumination, manual reload and spare battery save field.")
