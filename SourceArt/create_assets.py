"""Original procedural low-poly meshes and original synthesized audio. Python 3 + numpy.
Run from this project root. OBJ/MTL exports are editable in Blender; JSON is read by Unity.
"""
import math,json,wave,sys
sys.path.insert(0,str(__import__("pathlib").Path(__file__).resolve().parent))
from detail_mesh import install,decorate,uv_data
from pathlib import Path
import numpy as np
ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Assets/Ecos/Resources/Models';OUT.mkdir(parents=True,exist_ok=True)
PALETTE={'paint':(.31,.46,.42),'steel':(.33,.39,.4),'rust':(.37,.19,.1),'linen':(.73,.74,.65),'dark':(.065,.085,.09),'wood':(.28,.17,.1),'paper':(.82,.73,.52),'glass':(.2,.46,.5),'red':(.47,.08,.05),'yellow':(.8,.61,.19),'blue':(.16,.33,.48),'skin':(.65,.55,.45),'hair':(.09,.07,.05),'white':(.86,.87,.79),'light':(.72,.82,.65),'green':(.23,.5,.3)}
PALETTE.update({'vinyl':(.24,.25,.26),'cardboard':(.62,.47,.29),'porcelain':(.88,.86,.8),'rubber':(.12,.13,.14)})
class Mesh:
 def __init__(self,name):self.name=name;self.v=[];self.n=[];self.c=[];self.t=[];self.material=[];self.uv=[]
 def face(self,verts,col,center):
  if col=='dark' and self.name in ['Wheelchair','Chair','VinylSofa']:col='vinyl'
  if col in ['dark','wood'] and self.name=='Camcorder':col='rubber'
  if col=='white' and self.name=='Sink':col='porcelain'
  a=np.array(verts,dtype=float);n=np.cross(a[1]-a[0],a[2]-a[0]);length=np.linalg.norm(n)
  if length<1e-9:return
  if np.dot(n,a.mean(axis=0)-np.array(center))<0:a=a[::-1];n=-n
  n/=np.linalg.norm(n);start=len(self.v)
  axis=int(np.argmax(abs(n)))
  for p in a:
   self.v.append(p.tolist());self.n.append(n.tolist());self.c.append(PALETTE[col]);self.uv.extend([p[2],p[1]] if axis==0 else [p[0],p[2]] if axis==1 else [p[0],p[1]])
  for j in range(1,len(a)-1):self.t.extend([start,start+j,start+j+1]);self.material.append(col)
 def box(self,p,s,col):
  p=np.array(p);s=np.array(s)/2
  v=[p+s*np.array(a) for a in [(-1,-1,-1),(1,-1,-1),(1,1,-1),(-1,1,-1),(-1,-1,1),(1,-1,1),(1,1,1),(-1,1,1)]]
  for f in [(0,1,2,3),(4,5,6,7),(0,1,5,4),(3,2,6,7),(0,3,7,4),(1,2,6,5)]:self.face([v[i] for i in f],col,p)
 def ellipsoid(self,p,s,col,segments=10,rings=6):
  p=np.array(p);s=np.array(s)
  def pt(j,i):
   a=math.pi*j/rings;b=2*math.pi*i/segments
   return p+s*np.array([math.sin(a)*math.cos(b),math.cos(a),math.sin(a)*math.sin(b)])
  for j in range(rings):
   for i in range(segments):
    a,b,c,d=pt(j,i),pt(j+1,i),pt(j+1,i+1),pt(j,i+1)
    self.face([a,b,c],col,p);self.face([a,c,d],col,p)
 def rod(self,a,b,r,col,n=10,r2=None):
  a=np.array(a,dtype=float);b=np.array(b,dtype=float);axis=b-a;axis/=np.linalg.norm(axis)
  u=np.cross(axis,[0,1,0] if abs(axis[1])<.9 else [1,0,0]);u/=np.linalg.norm(u);v=np.cross(axis,u);r2=r if r2 is None else r2
  aa=[a+r*(u*math.cos(i*2*math.pi/n)+v*math.sin(i*2*math.pi/n)) for i in range(n)]
  bb=[b+r2*(u*math.cos(i*2*math.pi/n)+v*math.sin(i*2*math.pi/n)) for i in range(n)]
  mid=(a+b)/2;self.face(aa,col,mid);self.face(bb,col,mid)
  for i in range(n):self.face([aa[i],aa[(i+1)%n],bb[(i+1)%n],bb[i]],col,mid)
 def save(self):
  if self.name!='Sebastian':decorate(self)
  uv,groups=(self.skinUV,self.skinGroups) if hasattr(self,'skinUV') else uv_data(self)
  obj={'vertices':[a for v in self.v for a in v],'normals':[a for v in self.n for a in v],'colors':[a for v in self.c for a in v],'triangles':self.t,'uv':uv,'groups':groups}
  if hasattr(self,'rig'):obj.update(rig=self.rig,bone0=self.bone0,bone1=self.bone1,weight0=self.weight0)
  (OUT/(self.name+'.json')).write_text(json.dumps(obj,separators=(',',':')))
  lines=['# Original Ecos mesh. Units: metres. Y up.','mtllib Palette.mtl','o '+self.name]
  lines+=['v '+' '.join(f'{a:.6f}' for a in v) for v in self.v]
  lines+=['vt '+str(uv[i])+' '+str(uv[i+1]) for i in range(0,len(uv),2)]
  lines+=['vn '+' '.join(f'{a:.6f}' for a in n) for n in self.n]
  material=None
  for j in range(0,len(self.t),3):
   col=self.material[j//3]
   if col!=material:lines+=['usemtl '+col];material=col
   lines+=['f '+' '.join(f'{i+1}/{i+1}/{i+1}' for i in self.t[j:j+3])]
  (ROOT/'SourceArt'/(self.name+'.obj')).write_text('\n'.join(lines)+'\n')
  print(self.name,len(self.v),'vertices',len(self.t)//3,'triangles')
install(Mesh)
(ROOT/'SourceArt/Palette.mtl').write_text('\n'.join('newmtl '+k+'\nKd '+' '.join(map(str,c))+'\nKa 0.1 0.1 0.1\nKs 0.1 0.1 0.1\nNs 10\n' for k,c in PALETTE.items()))
def bed(name,body=False,child=False):
 m=Mesh(name);m.box((0,.75,0),(1.05,.13,2.15),'steel');m.box((0,.88,0),(.98,.18,2),'linen')
 for x in [-.43,.43]:
  for z in [-.85,.85]:m.rod((x,.16,z),(x,.72,z),.035,'steel');m.ellipsoid((x,.12,z),(.09,.09,.05),'dark',8,4)
  for z in [-1.05,1.05]:m.rod((x,.7,z),(x,1.25,z),.035,'steel')
 for z in [-1.05,1.05]:m.rod((-.43,1.25,z),(.43,1.25,z),.035,'steel')
 if body:
  m.ellipsoid((0,1.04,0),(.28,.2,.57 if child else .82),'white');m.ellipsoid((0,1.07,-.62 if child else -.8),(.15,.16,.19),'white')
 else:m.box((0,1,-.73),(.7,.12,.38),'white')
 return m
bed('Bed').save();bed('MotherBody',True).save();bed('ChildBody',True,True).save()
m=Mesh('Crib');m.box((0,.6,0),(.95,.15,1.4),'wood');m.box((0,.72,0),(.85,.12,1.3),'linen')
for x in [-.45,.45]:
 for z in [-.65,.65]:m.rod((x,0,z),(x,1.3,z),.04,'wood')
 for z in np.linspace(-.65,.65,8):m.rod((x,.65,z),(x,1.2,z),.018,'white')
 m.rod((x,1.25,-.68),(x,1.25,.68),.035,'white')
for z in [-.65,.65]:m.box((0,1,z),(.9,.65,.06),'paint')
m.save()
m=Mesh('Desk');m.box((0,1,0),(2.8,.13,1.1),'wood');m.box((0,.55,.4),(2.6,.95,.12),'paint')
for x in [-1,1]:m.box((x,.48,0),(.5,.95,.8),'wood')
for y in [.25,.55,.8]:m.box((1,y,-.43),(.43,.2,.04),'paint');m.rod((.9,y,-.47),(1.1,y,-.47),.015,'steel')
m.save()
m=Mesh('Phone');m.box((0,.1,0),(.42,.2,.3),'dark');m.rod((-.2,.3,0),(.2,.3,0),.065,'dark')
for x in [-.18,.18]:m.ellipsoid((x,.24,0),(.09,.09,.1),'dark')
for x in [-.09,0,.09]:
 for z in [-.09,0,.09]:m.box((x,.21,z),(.05,.018,.05),'linen')
m.save()
m=Mesh('Chair');m.box((0,.48,0),(.58,.07,.58),'wood');m.box((0,.92,.25),(.58,.7,.07),'paint')
for x in [-.25,.25]:
 for z in [-.25,.25]:m.rod((x,0,z),(x,.5,z),.025,'steel')
m.save()
m=Mesh('Wheelchair');m.box((0,.55,0),(.64,.09,.6),'dark');m.box((0,.92,.28),(.66,.7,.07),'dark')
for x in [-.4,.4]:
 m.rod((x,.38,.12),(x+.045,.38,.12),.38,'steel',16);m.rod((x-.015,.38,.12),(x+.06,.38,.12),.3,'dark',16)
 m.rod((x,.2,-.3),(x,.9,.3),.025,'steel');m.rod((x,.88,-.25),(x,.88,.25),.04,'steel');m.ellipsoid((x,.1,-.35),(.09,.1,.09),'dark')
 m.box((x*.65,.14,-.55),(.22,.035,.3),'steel')
m.save()
m=Mesh('Cabinet');m.box((0,1,0),(1.2,2,.6),'paint')
for y in [.25,.75,1.25,1.75]:m.box((0,y,-.32),(1.08,.44,.035),'steel');m.box((0,y,-.36),(.26,.045,.04),'dark');m.box((0,y+.1,-.35),(.2,.065,.01),'paper')
m.save()
m=Mesh('XRay');m.box((0,.18,0),(1,.36,.7),'paint');m.rod((0,.2,0),(0,1.8,0),.09,'steel');m.box((0,1.6,0),(1.2,.85,.2),'dark');m.box((0,1.6,-.12),(1,.66,.025),'glass')
for x in [-.25,.25]:m.ellipsoid((x,1.6,-.15),(.17,.24,.015),'linen',8,4)
m.save()
m=Mesh('Generator');m.box((0,.6,0),(1.8,1.2,1),'paint');m.rod((-.8,.6,0),(.6,.6,0),.38,'dark',12)
for x in np.linspace(-.7,.6,8):m.box((x,.62,-.48),(.035,.8,.05),'steel')
m.rod((.7,1,0),(.7,1.65,0),.07,'rust');m.rod((.7,1.65,0),(1,1.65,0),.07,'rust');m.save()
m=Mesh('Pipe');m.rod((0,0,-1),(0,0,1),.1,'rust');
for z in [-.9,.9]:m.rod((0,0,z-.04),(0,0,z+.04),.15,'steel')
m.save()
m=Mesh('Door');m.box((0,1.6,0),(2.5,3.2,.14),'paint');m.box((0,2.15,-.08),(.7,.75,.025),'glass');m.box((0,2.15,-.1),(.06,.8,.025),'steel');m.rod((.8,1.3,-.12),(.8,1.6,-.12),.025,'steel');m.save()
m=Mesh('Teddy');m.ellipsoid((0,.35,0),(.21,.27,.16),'wood');m.ellipsoid((0,.67,0),(.2,.18,.17),'wood')
for x in [-.16,.16]:m.ellipsoid((x,.79,0),(.085,.09,.06),'wood');m.ellipsoid((x,.1,-.03),(.1,.11,.14),'wood');m.rod((x,.48,0),(x*1.8,.28,0),.07,'wood')
for x in [-.07,.07]:m.ellipsoid((x,.7,-.16),(.025,.025,.015),'dark')
m.ellipsoid((0,.6,-.16),(.08,.06,.05),'paper');m.save()
m=Mesh('Battery');m.rod((0,0,0),(0,.36,0),.1,'yellow',12);m.rod((0,.3,0),(0,.36,0),.102,'dark',12);m.rod((0,.36,0),(0,.39,0),.04,'steel');m.save()
m=Mesh('Key');m.rod((0,0,0),(0,.35,0),.025,'yellow');m.rod((0,.26,0),(.1,.26,0),.02,'yellow');m.ellipsoid((0,.42,0),(.09,.08,.025),'yellow');m.save()
m=Mesh('Camera');m.box((0,.12,0),(.48,.29,.17),'dark');m.rod((0,.12,-.07),(0,.12,-.22),.11,'steel');m.rod((0,.12,-.22),(0,.12,-.24),.085,'glass');m.box((-.16,.2,-.1),(.1,.07,.02),'white');m.box((.16,.28,0),(.08,.035,.06),'red');m.save()
m=Mesh('Recorder');m.box((0,.16,0),(.2,.32,.07),'dark');m.box((0,.23,-.04),(.14,.08,.01),'glass');
for x in [-.05,0,.05]:m.box((x,.1,-.04),(.025,.03,.01),'white')
m.save()
from sebastian_model import build as build_sebastian
m=Mesh('Sebastian');build_sebastian(m);m.save()
m=Mesh('Frame');m.box((0,0,0),(1.2,.85,.08),'wood');m.box((0,0,-.05),(1.05,.7,.02),'paper');m.ellipsoid((-.18,-.1,-.07),(.3,.18,.01),'blue');m.ellipsoid((.2,.1,-.07),(.17,.17,.01),'yellow');m.save()
m=Mesh('Document');m.box((0,0,0),(.42,.58,.02),'paper')
for y in np.linspace(-.18,.2,7):m.box((-.02,y,-.015),(.3,.009,.006),'wood')
m.save()
m=Mesh('Lamp');m.box((0,0,0),(1.3,.09,.3),'steel');m.box((0,-.06,0),(1.15,.04,.22),'light');m.save()
m=Mesh('Rubble');rng=np.random.default_rng(85)
for i in range(9):m.ellipsoid((rng.uniform(-.9,.9),rng.uniform(.1,.4),rng.uniform(-.5,.5)),(rng.uniform(.15,.45),rng.uniform(.15,.4),rng.uniform(.2,.5)),'steel',6,3)
m.save()
m=Mesh('Valve');m.rod((0,0,0),(0,0,.25),.06,'steel')
for i in range(8):
 a=i*math.pi/4;b=(i+1)*math.pi/4;m.rod((.25*math.cos(a),.25*math.sin(a),0),(.25*math.cos(b),.25*math.sin(b),0),.035,'red',6)
for i in range(4):a=i*math.pi/2;m.rod((0,0,0),(.25*math.cos(a),.25*math.sin(a),0),.025,'red',6)
m.save()
# 22.05kHz mono audio. Every file is peak-normalized below -6dBFS.
AUD=ROOT/'Assets/Ecos/Resources/Audio';AUD.mkdir(parents=True,exist_ok=True);SR=22050
rng=np.random.default_rng(1985)
def save(name,a):
 a=np.asarray(a);peak=max(1e-6,np.max(np.abs(a)));a=a/peak*.45
 with wave.open(str(AUD/(name+'.wav')),'wb') as f:f.setnchannels(1);f.setsampwidth(2);f.setframerate(SR);f.writeframes((a*32767).astype('<i2').tobytes())
def tone(hz,dur):
 t=np.arange(int(SR*dur))/SR;return np.sin(2*np.pi*hz*t)*np.minimum(t*30,1)*np.exp(-t*4/dur)
for name,hz,d in [('Collect',740,.45),('Error',110,.4),('Unlock',523,.8),('Heartbeat',55,.45),('Bell1',261.63,1.4),('Bell2',329.63,1.4),('Bell3',392,1.4),('Memory',220,4),('Death',75,1.8)]:save(name,tone(hz,d)+.2*tone(hz*2.01,d))
t=np.arange(SR//3)/SR;save('Shutter',rng.normal(0,1,len(t))*np.exp(-t*35)+.3*np.sin(t*1800)*np.exp(-t*14))
t=np.arange(SR//5)/SR;save('Step',rng.normal(0,1,len(t))*np.exp(-t*45)+np.sin(t*2*np.pi*85)*np.exp(-t*30))
for n,hz in [('Wind',42),('Hospital',56),('Basement',37),('Tension',69)]:
 t=np.arange(SR*8)/SR;noise=rng.normal(0,1,len(t));noise=np.convolve(noise,np.ones(180)/180,mode='same');a=.15*np.sin(t*2*np.pi*hz)+.09*np.sin(t*2*np.pi*(hz+1))+noise*2
 envelope=np.minimum(t,1)*np.minimum(8-t,1);save(n,a*envelope)
t=np.arange(SR*32)/SR;a=np.zeros(len(t))
for j,h in enumerate([146.83,174.61,130.81,164.81]*2):
 start=j*SR*4;note=tone(h,4);a[start:start+len(note)]+=note*.25+tone(h*1.5,4)*.1
save('Theme',a)
print('Created original audio WAVs.')
