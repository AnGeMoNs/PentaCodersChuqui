"""Original rigged horror character. Bind positions are absolute model-space metres.
The JSON contains two-weight skinning; the OBJ is the unposed reference mesh.
"""
import math,numpy as np
from scipy.interpolate import PchipInterpolator
BONES=[('Root',-1,(0,0,0)),('Pelvis',0,(0,.66,0)),('Spine',1,(0,.90,.015)),('Head',2,(0,1.155,-.025)),('Jaw',3,(0,1.225,-.07)),('UpperArmL',2,(-.19,1.04,.005)),('ForearmL',5,(-.235,.74,-.035)),('UpperArmR',2,(.19,1.04,.005)),('ForearmR',7,(.235,.74,-.035)),('ThighL',1,(-.085,.66,.015)),('CalfL',9,(-.085,.34,.015)),('ThighR',1,(.085,.66,.015)),('CalfR',11,(.085,.34,.015)),('HandL',6,(-.25,.47,-.06)),('HandR',8,(.25,.47,-.06)),('FootL',10,(-.085,.10,.015)),('FootR',12,(.085,.10,.015))]
def build(m):
 m.bone0=[];m.bone1=[];m.weight0=[];m.skinUV=[];m.skinGroups=[]
 m.rig=[dict(name=n,parent=p,position=dict(zip(['x','y','z'],v))) for n,p,v in BONES]
 def tag(start,bone,second=None,weight=None,kind=None):
  for v in m.v[start:]:
   w=1 if weight is None else float(weight(v));b=bone;s=bone if second is None else second
   if w<.5:b,s=s,b;w=1-w
   m.bone0.append(b);m.bone1.append(s);m.weight0.append(w)
  # Independent character UV/material channels, never the furniture atlas.
  for k in range(start,len(m.v)):
   x,y,z=m.v[k];c=m.c[k]
   slot=kind if kind is not None else 1 if c==(.73,.74,.65) else 3 if c==(.09,.07,.05) else 4 if c==(.065,.085,.09) else 2
   m.skinUV.extend([x/.25+.5,(y-1.19)/.34] if slot==0 else m.uv[k*2:k*2+2])
   m.skinGroups.extend([slot,0])
 def ell(p,s,col,bone):
  start=len(m.v);m.ellipsoid(p,s,col,40 if max(s)>.09 else 24,24 if max(s)>.09 else 14);tag(start,bone)
 def rod(a,b,r,col,bone,r2=None):
  start=len(m.v);m.rod(a,b,r,col,24,r2);tag(start,bone)
 # Gown: folds, collar and ragged hem; pelvis/spine blend keeps the cloth attached.
 start=len(m.v)
 def cloth(j,i):
  t=j/40;a=i*math.tau/80;hem=.54+.010*math.sin(a*9)+.005*math.sin(a*17)
  y=hem+(1.11-hem)*t;fold=(.007*math.sin(a*19+t*3)+.002*math.sin(a*31))*(1-.6*t)*min(1,(1-t)*10)
  rx=.19+.01*t;rz=.12+.02*t
  if t>.82:rx*=1-(t-.82)*3.6;rz*=1-(t-.82)*3.2
  return ((rx+fold)*math.cos(a),y,(rz+fold)*math.sin(a))
 for j in range(40):
  for i in range(80):
   pts=[cloth(j,i),cloth(j+1,i),cloth(j+1,i+1),cloth(j,i+1)]
   m.face(pts,'linen',(0,sum(p[1] for p in pts)/4,0))
 tag(start,2,1,lambda v:np.clip((v[1]-.70)/.30,0,1))
 rod((0,1.06,0),(0,1.175,-.025),.045,'skin',2,r2=.038)
 for side,upper,fore,hand,thigh,calf,foot in [(-1,5,6,13,9,10,15),(1,7,8,14,11,12,16)]:
  # Narrow sleeves, long forearms and articulated hands. No blood or exposed wounds.
  ell((side*.17,1.04,.005),(.054,.045,.055),'linen',upper)
  rod((side*.17,1.04,.005),(side*.235,.74,-.035),.066,'linen',upper,r2=.043)
  ell((side*.235,.74,-.035),(.047,.048,.046),'linen',fore)
  rod((side*.235,.74,-.035),(side*.25,.47,-.06),.036,'skin',fore,r2=.022)
  ell((side*.25,.44,-.065),(.038,.06,.024),'skin',hand)
  for finger in range(4):
   x=side*(.226+finger*.017);length=[.09,.119,.112,.088][finger]
   rod((x,.415,-.073),(x+side*.008,.415-length*.6,-.084),.008,'skin',hand,r2=.006)
   rod((x+side*.008,.415-length*.6,-.084),(x+side*.006,.415-length,-.10),.006,'skin',hand,r2=.004)
  rod((side*.22,.455,-.063),(side*.198,.40,-.10),.011,'skin',hand,r2=.006)
  rod((side*.085,.66,.015),(side*.085,.34,.015),.065,'linen',thigh,r2=.045)
  ell((side*.085,.34,.015),(.045,.047,.044),'linen',calf)
  rod((side*.085,.34,.015),(side*.085,.10,.015),.041,'skin',calf,r2=.025)
  ell((side*.085,.10,.015),(.026,.033,.027),'skin',foot)
  ell((side*.085,.045,-.043),(.045,.045,.095),'skin',foot)
  for toe in range(5):
   ell((side*(.055+toe*.014),.022,-.126+toe*.005),(.009,.020,.027-toe*.002),'skin',foot)
 # A continuous sculpted facial surface, not stacked facial primitives.
 # Head UV uses the dedicated frontal albedo. Jaw vertices deform with the rig.
 start=len(m.v)
 levels=np.array([[1.19,.022,.035],[1.215,.057,.061],[1.25,.078,.078],[1.30,.092,.087],[1.36,.116,.097],[1.42,.119,.104],[1.48,.112,.104],[1.535,.083,.081],[1.565,.005,.008]])
 width=PchipInterpolator(levels[:,0],levels[:,1]);depth=PchipInterpolator(levels[:,0],levels[:,2])
 def gaussian(x,y,cx,cy,sx,sy):return math.exp(-((x-cx)/sx)**2-((y-cy)/sy)**2)
 def head(j,i):
  y=1.19+.375*j/100;a=i*math.tau/128
  x=float(width(y))*math.cos(a)
  z=float(depth(y))*math.sin(a)-.015
  f=max(0,-math.sin(a))**4
  # Brow ridge, concave sockets, cheeks, nasal bridge and tip all in one surface.
  z+=f*(.027*(gaussian(x,y,-.052,1.400,.032,.020)+gaussian(x,y,.052,1.400,.032,.020)))
  z-=f*(.012*(gaussian(x,y,-.05,1.434,.039,.013)+gaussian(x,y,.05,1.434,.039,.013)))
  z-=f*(.020*gaussian(x,y,0,1.365,.015,.050)+.044*gaussian(x,y,0,1.322,.021,.019))
  z-=f*.010*(gaussian(x,y,-.026,1.319,.013,.011)+gaussian(x,y,.026,1.319,.013,.011))
  z-=f*.012*(gaussian(x,y,-.067,1.36,.024,.020)+gaussian(x,y,.067,1.36,.024,.020))
  z-=f*.009*gaussian(x,y,0,1.260,.042,.010)
  z+=f*.010*(gaussian(x,y,-.052,1.30,.022,.03)+gaussian(x,y,.052,1.30,.022,.03))
  return (x,y,z)
 for j in range(100):
  for i in range(128):
   pts=[head(j,i),head(j+1,i),head(j+1,i+1),head(j,i+1)]
   c=np.mean(pts,axis=0)
   # Real narrow mouth opening. It opens wider when the weighted lower face moves.
   if c[2]<-.045 and (c[0]/.036)**2+((c[1]-1.257)/.0045)**2<1:continue
   m.face(pts,'skin',(0,c[1],-.015))
 sums={}
 for k in range(start,len(m.v)):
  key=tuple(round(x,6) for x in m.v[k]);sums[key]=sums.get(key,np.zeros(3))+m.n[k]
 for k in range(start,len(m.v)):
  n=sums[tuple(round(x,6) for x in m.v[k])];m.n[k]=(n/max(np.linalg.norm(n),1e-9)).tolist()
 tag(start,3,4,lambda v:1-np.clip((1.275-v[1])/.024,0,1)*np.clip((-v[2]-.015)/.04,0,1),kind=0)
 for k in range(start,len(m.v)):
  x,y,z=m.v[k];m.skinGroups[k*2+1]=float(np.clip((-z-.012)/.055,0,1))
 # Mouth interior follows the jaw, with small dry teeth rather than a bright grin.
 ell((0,1.247,-.065),(.039,.026,.020),'dark',4)
 for x in np.linspace(-.026,.026,7):
  st=len(m.v);m.ellipsoid((x,1.265,-.078),(.0036,.005,.003),'paper',20,14);tag(st,3,kind=5)
 # Ear helix and concha. Scale corresponds to a child, not a toy's head.
 for side in [-1,1]:
  ell((side*.118,1.373,-.003),(.019,.034,.016),'skin',3)
  ell((side*.130,1.373,-.014),(.007,.020,.004),'dark',3)
  rod((side*.128,1.35,-.017),(side*.133,1.385,-.018),.004,'skin',3,r2=.003)
 # Dense scalp with fine irregular bangs, replacing the old sparse cylindrical hair.
 start=len(m.v)
 def hairpoint(a,y,offset=.006):
  yy=min(1.565,max(1.19,y))
  return ((float(width(yy))+offset)*math.cos(a),y,-.015+(float(depth(yy))+offset)*math.sin(a))
 def scalp(j,i):
  a=i*math.tau/96;t=j/26;front=max(0,-math.sin(a))
  bottom=1.34+.092*front+.015*math.sin(a*7)+.009*math.sin(a*13)+.020*math.cos(a)*front
  y=1.57+(bottom-1.57)*t
  return hairpoint(a,y)
 for j in range(26):
  for i in range(96):
   pts=[scalp(j,i),scalp(j+1,i),scalp(j+1,i+1),scalp(j,i+1)]
   m.face(pts,'hair',(0,1.414,-.010))
 sums={}
 for k in range(start,len(m.v)):
  key=tuple(round(x,6) for x in m.v[k]);sums[key]=sums.get(key,np.zeros(3))+m.n[k]
 for k in range(start,len(m.v)):
  n=sums[tuple(round(x,6) for x in m.v[k])];m.n[k]=(n/max(np.linalg.norm(n),1e-9)).tolist()
 tag(start,3,kind=3)
 rng=np.random.default_rng(198510)
 for strand in range(44):
  a=float(rng.uniform(math.pi,math.tau));bottom=1.415+float(rng.uniform(-.025,.025))+.018*math.cos(a)
  for j in range(5):
   def hair(t):return hairpoint(a+.035*t,1.51+(bottom-1.51)*t,.008-.004*t)
   rod(hair(j/5),hair((j+1)/5),.003*(1-j/6),'hair',3,r2=.0015*(1-j/6))
 # Stitched placket, small buttons, long cuffs and fingernails.
 for y in [.82,.9,.98,1.045]:
  ell((0,y,-.148),(.006,.006,.003),'dark',2)
 for side,hand in [(-1,13),(1,14)]:
  for finger in range(4):
   x=side*(.232+finger*.017);length=[.09,.119,.112,.088][finger]
   st=len(m.v);m.ellipsoid((x,.42-length,-.102),(.004,.010,.0018),'paper',20,14);tag(st,hand,kind=5)
 assert len(m.bone0)==len(m.v)==len(m.skinUV)//2==len(m.skinGroups)//2

 # Bring the head down to a natural short pediatric neck without changing facial UVs.
 for k in range(len(m.v)):
  if m.bone0[k] in (3,4):m.v[k][1]-=.045
 # Remove subpixel needle triangles and weld duplicate render vertices while
 # preserving normals, UV seams, material channels and both skin weights.
 v=np.array(m.v);tri=np.array(m.t).reshape(-1,3)
 area=np.linalg.norm(np.cross(v[tri[:,1]]-v[tri[:,0]],v[tri[:,2]]-v[tri[:,0]]),axis=1)
 keep=area>1.01e-8;tri=tri[keep];materials=np.array(m.material)[keep]
 attributes=np.column_stack([v,np.array(m.n),np.array(m.c),np.array(m.skinUV).reshape(-1,2),np.array(m.skinGroups).reshape(-1,2),m.bone0,m.bone1,m.weight0])
 unique,inverse=np.unique(np.round(attributes,7),axis=0,return_inverse=True)
 tri=inverse[tri];v=unique[:,:3]
 area=np.linalg.norm(np.cross(v[tri[:,1]]-v[tri[:,0]],v[tri[:,2]]-v[tri[:,0]]),axis=1);keep=area>1e-8
 m.v=v.tolist();normal=unique[:,3:6];normal/=np.linalg.norm(normal,axis=1,keepdims=True);m.n=normal.tolist();m.c=unique[:,6:9].tolist()
 m.skinUV=unique[:,9:11].ravel().tolist();m.skinGroups=unique[:,11:13].ravel().tolist();m.uv=m.skinUV.copy()
 m.bone0=unique[:,13].astype(int).tolist();m.bone1=unique[:,14].astype(int).tolist();m.weight0=unique[:,15].tolist()
 m.t=tri[keep].ravel().tolist();m.material=materials[keep].tolist()
