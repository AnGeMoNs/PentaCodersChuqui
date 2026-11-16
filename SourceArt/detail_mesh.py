"""Original rounded geometry, anatomical/gore details and high-poly detail pass. Imported by create_assets.py."""
import numpy as np
import math

def install(Mesh):
 original_box=Mesh.box
 original_ellipsoid=Mesh.ellipsoid
 original_rod=Mesh.rod
 def box(self,p,s,col):
  if min(s)<.025 or self.name in ['Document','Rubble','BrokenWindow']:
   return original_box(self,p,s,col)
  p=np.array(p);h=np.array(s)/2;r=min(.035,min(h)*.42);inner=h-r
  for axis in range(3):
   axes=[i for i in range(3) if i!=axis]
   a,b=axes
   uu=[-h[a],-inner[a],inner[a],h[a]];vv=[-h[b],-inner[b],inner[b],h[b]]
   for sign in [-1,1]:
    def pt(u,v):
     q=np.zeros(3);q[axis]=sign*h[axis];q[a]=u;q[b]=v
     c=np.clip(q,-inner,inner);d=q-c;return p+c+d/np.linalg.norm(d)*r
    for i in range(3):
     for j in range(3):
      start=len(self.v)
      self.face([pt(uu[i],vv[j]),pt(uu[i+1],vv[j]),pt(uu[i+1],vv[j+1]),pt(uu[i],vv[j+1])],col,p)
      for k in range(start,len(self.v)):
       q=np.array(self.v[k])-p;c=np.clip(q,-inner,inner);n=q-c;self.n[k]=(n/np.linalg.norm(n)).tolist()
 def ellipsoid(self,p,s,col,segments=10,rings=6):
  start=len(self.v);original_ellipsoid(self,p,s,col,max(24,segments),max(14,rings))
  for k in range(start,len(self.v)):
   n=(np.array(self.v[k])-p)/np.array(s)**2;self.n[k]=(n/np.linalg.norm(n)).tolist()
 def rod(self,a,b,r,col,n=10,r2=None):
  n=max(24,n);start=len(self.v);original_rod(self,a,b,r,col,n,r2)
  axis=np.array(b)-a;length=np.linalg.norm(axis);axis=axis/length
  slope=(r-(r if r2 is None else r2))/length
  for k in range(start+2*n,len(self.v)):
   q=np.array(self.v[k])-a;rad=q-np.dot(q,axis)*axis
   normal=rad/np.linalg.norm(rad)+axis*slope;self.n[k]=(normal/np.linalg.norm(normal)).tolist()
 Mesh.box=box;Mesh.ellipsoid=ellipsoid;Mesh.rod=rod

def torus(m,p,major,minor,col,axis='x',segments=48,sides=8):
 p=np.array(p)
 def pt(a,b):
  q=np.array([minor*math.sin(b),(major+minor*math.cos(b))*math.cos(a),(major+minor*math.cos(b))*math.sin(a)])
  if axis=='y':q=q[[1,0,2]]
  if axis=='z':q=q[[1,2,0]]
  return p+q
 for i in range(segments):
  a=i*math.tau/segments;aa=(i+1)*math.tau/segments
  for j in range(sides):
   b=j*math.tau/sides;bb=(j+1)*math.tau/sides
   center=(pt(a,0)+pt(a,math.pi)+pt(aa,0)+pt(aa,math.pi))/4
   start=len(m.v);m.face([pt(a,b),pt(aa,b),pt(aa,bb),pt(a,bb)],col,center)
   for k in range(start,len(m.v)):
    q=np.array(m.v[k])-p;idx={'x':0,'y':1,'z':2}[axis];c=q.copy();c[idx]=0;c=c/np.linalg.norm(c)*major;n=q-c;m.n[k]=(n/np.linalg.norm(n)).tolist()

def cloth(m,curtain=False,body=False,child=False):
 nx,nz=(64,32) if curtain else (28,52)
 def pt(i,j,back=0):
  u=i/nx;v=j/nz
  if curtain:return (-.85+1.7*u,.34+1.64*v,.055*math.sin(u*math.tau*12)+.012*math.sin(v*12+u*19)+back)
  x=(u-.5)*1.02;z=(v-.5)*1.94
  y=.991+.016*math.sin(u*63+v*11)+.008*math.sin(v*75-u*12)
  y-=.12*(abs(2*u-1)**12)
  if body:y+=.3*math.exp(-(x/.24)**4-(z/(.58 if child else .82))**4)
  return (x,y-back,z)
 for back in [0,.008]:
  for i in range(nx):
   for j in range(nz):
    a=[pt(i,j,back),pt(i+1,j,back),pt(i+1,j+1,back),pt(i,j+1,back)]
    center=np.mean(a,axis=0)+np.array([0,0,1 if back==0 else -1]) if curtain else np.mean(a,axis=0)+np.array([0,-1 if back==0 else 1,0])
    # Add visceral blood-soaked patches on sheets and privacy curtains
    dist_center=math.hypot((i/nx)-0.48,(j/nz)-0.52)
    is_bloody=(not curtain and body and dist_center<0.22) or (not curtain and not body and 0.14<dist_center<0.26 and (i+j)%3==0) or (curtain and j<10 and (i*7+j)%5==0)
    m.face(a,'red' if is_bloody else 'linen',center)

def decorate(m):
 # Record original bounding box so gore/detail additions never alter outer collision/layout bounds
 orig_v=np.array(m.v) if len(m.v)>0 else None
 orig_min=orig_v.min(axis=0) if orig_v is not None else None
 orig_max=orig_v.max(axis=0) if orig_v is not None else None

 if m.name in ['Bed','MotherBody','ChildBody']:
  cloth(m,body=m.name!='Bed',child=m.name=='ChildBody')
  for x in [-.48,.48]:
   for y in [1.06,1.17]:m.rod((x,y,-.75),(x,y,.75),.016,'steel')
   for z in np.linspace(-.72,.72,7):m.rod((x,.88,z),(x,1.19,z),.012,'steel')
  for z in [-1.05,1.05]:
   for x in [-.43,.43]:m.rod((x,1.1,z-.04),(x,1.1,z+.04),.023,'rust')
  if m.name=='Bed':
   m.ellipsoid((0,1.09,-.68),(.37,.09,.23),'linen')
   # Visceral blood pool and torn restraints on the mattress
   m.ellipsoid((.06,1.01,.12),(.28,.022,.36),'red')
   for z in [-.35,.28]:m.box((0,1.005,z),(.92,.018,.055),'rubber')
 if m.name=='AutopsyTable':
  # Visceral anatomical dissection cadaver organs, exposed ribcage & surgical saws on autopsy table
  m.ellipsoid((-.05,.915,0),(.52,.018,.28),'red')
  for i in range(6):
   rx=-.32+i*.11
   torus(m,(rx,.93,0),.14,.014,'white','x',20,6)
  m.ellipsoid((-.12,.94,.04),(.14,.055,.11),'red')
  m.ellipsoid((.14,.935,-.05),(.16,.05,.12),'red')
  m.rod((.45,.915,.18),(.68,.915,.22),.010,'steel',16)
  m.box((.56,.92,.20),(.14,.008,.045),'steel')
 if m.name=='Stretcher':
  # Blood-soaked trauma sheet, coagulated pool and leather restraint straps
  m.ellipsoid((0,.875,.08),(.29,.020,.58),'red')
  for z in [-.22,.22]:m.box((0,.874,z),(.71,.015,.055),'rubber')
 if m.name=='MedicalTray':
  # Bloodied surgical gauze, scalpel blades and specimen vials inside the tray
  m.ellipsoid((-.12,.052,-.04),(.09,.012,.08),'red')
  m.ellipsoid((.14,.052,.05),(.07,.015,.06),'red')
  for x in [-.06,.04]:m.rod((x,.055,-.10),(x,.055,.12),.006,'steel',14)
 if m.name=='BioBin':
  for y in [.15,.35,.55]:
   torus(m,(0,y,0),.235,.010,'yellow','y',32,8)
  torus(m,(0,.61,0),.248,.014,'dark','y',32,8)
  for a in [0,2.094,4.188]:
   torus(m,(.04*math.cos(a),.34+.04*math.sin(a),-.235),.035,.007,'dark','z',20,6)
  for x in [-.23,.23]:
   torus(m,(x,.52,0),.035,.008,'dark','x',20,6)
  # Overflowing bloody biohazard bag inside bin rim
  m.ellipsoid((0,.615,0),(.19,.028,.19),'red')
 if m.name=='Wheelchair':
  m.v=[];m.n=[];m.c=[];m.t=[];m.material=[];m.uv=[]
  m.box((0,.55,0),(.6,.07,.6),'dark');m.box((0,.88,.27),(.62,.63,.08),'dark')
  m.ellipsoid((0,.59,0),(.22,.015,.22),'red')
  for x in [-.39,.39]:
   torus(m,(x,.37,.12),.33,.035,'dark');torus(m,(x*1.13,.37,.12),.29,.012,'steel')
   m.rod((x-.05,.37,.12),(x+.05,.37,.12),.045,'steel')
   for i in range(24):
    a=math.tau*i/24;m.rod((x,.37,.12),(x,.37+.315*math.cos(a),.12+.315*math.sin(a)),.004,'steel')
   m.rod((x,.2,-.3),(x,1.2,.3),.024,'steel');m.rod((x,.85,-.3),(x,.85,.3),.025,'steel')
   m.box((x,.87,0),(.09,.05,.5),'dark');m.box((x*.7,.15,-.55),(.22,.04,.3),'steel')
   torus(m,(x,.1,-.36),.07,.025,'dark',segments=24)
   m.rod((x,.1,-.36),(x,.4,-.3),.02,'steel')
 if m.name=='PrivacyScreen':
  m.v=[];m.n=[];m.c=[];m.t=[];m.material=[];m.uv=[]
  for x in [-.9,.9]:m.rod((x,.1,0),(x,2.1,0),.025,'steel');m.rod((x,.1,-.3),(x,.1,.3),.025,'steel')
  m.rod((-.9,2.05,0),(.9,2.05,0),.025,'steel');cloth(m,True)
  for x in np.linspace(-.8,.8,12):torus(m,(x,2.04,0),.035,.006,'steel','x',24,6)
 if m.name in ['Cabinet','Locker','MorgueDrawers','Generator','Monitor']:
  v=np.array(m.v);low=v.min(axis=0);high=v.max(axis=0)
  for x in [low[0]+.08,high[0]-.08]:
   for y in [low[1]+.08,high[1]-.08]:m.rod((x,y,low[2]-.005),(x,y,low[2]-.016),.012,'steel',6)
 if m.name=='Camcorder':
  for z in np.linspace(-.26,-.34,7):torus(m,(0,.16,z),.112,.006,'dark','z',32,6)
  for x in [-.07,-.03,.01,.05]:m.box((x,.299,.12),(.022,.012,.026),'steel')
  for z in np.linspace(-.1,.16,9):m.box((.126,.19,z),(.006,.06,.007),'steel')
  m.rod((-.05,.36,-.03),(-.05,.36,.18),.018,'dark');m.rod((.05,.36,-.03),(.05,.36,.18),.018,'dark')
 if m.name=='OxygenTank':
  torus(m,(.12,1.4,-.09),.073,.008,'steel','z',32)
  m.rod((.12,1.4,-.1),(.15,1.44,-.1),.003,'dark')
  for i in range(10):
   a=i*math.tau/10;m.rod((.12+.052*math.cos(a),1.4+.052*math.sin(a),-.1),(.12+.06*math.cos(a),1.4+.06*math.sin(a),-.1),.002,'dark')
 if m.name=='Recorder':
  for x in [-.036,.036]:
   torus(m,(x,.23,-.041),.024,.005,'steel','z',28,8)
   m.rod((x,.23,-.036),(x,.23,-.044),.009,'paper',16)
  for row in range(4):
   for col in range(6):
    m.rod((-.055+col*.022,.145+row*.014,-.033),(-.055+col*.022,.145+row*.014,-.042),.004,'steel',12)
  m.rod((.07,.30,0),(.07,.318,0),.012,'red',16)
  m.rod((-.07,.30,0),(-.07,.318,0),.016,'steel',16)
 if m.name=='Battery':
  for y in [.05,.12,.19,.26]:
   torus(m,(0,y,0),.092,.005,'rubber','y',32,8)
  torus(m,(0,.355,0),.088,.006,'steel','y',32,8)
  torus(m,(0,.365,0),.042,.005,'red','y',24,8)
 if m.name=='Key':
  torus(m,(0,.42,0),.068,.011,'yellow','z',36,10)
  torus(m,(0,.33,0),.028,.007,'steel','y',24,8)
  for y in [.04,.09,.14,.19]:
   m.rod((0,y,0),(.075,y,0),.011,'yellow',14)
 if m.name=='Door':
  for y in [.65,1.25]:
   m.box((0,y,-.062),(1.95,.42,.028),'steel')
  m.rod((-.85,1.42,-.10),(.72,1.42,-.10),.022,'steel',24)
  for x in [-.22,0,.22]:
   m.rod((x,1.78,-.09),(x,2.52,-.09),.012,'steel',16)
  for y in [1.95,2.15,2.35]:
   m.rod((-.34,y,-.09),(.34,y,-.09),.012,'steel',16)
  torus(m,(.8,1.45,-.08),.038,.008,'steel','z',24,8)
  # Bloody handprint smear near door pushbar
  m.ellipsoid((.48,1.32,-.068),(.16,.22,.008),'red')
 if m.name=='Desk':
  for y in [.25,.55,.8]:
   torus(m,(1,y+.05,-.44),.018,.005,'steel','z',20,6)
  for x in [-1.25,-.65,0,.65,1.25]:
   m.rod((x,.12,.38),(x,.92,.38),.016,'steel',16)
  m.box((-.35,1.045,-.05),(.85,.03,.55),'vinyl')
 if m.name=='Chair':
  m.box((0,.515,0),(.50,.05,.50),'vinyl')
  for x in [-.24,.24]:
   m.rod((x,.48,.24),(x,1.18,.25),.022,'steel',20)
   m.rod((x,.22,-.24),(x,.22,.24),.016,'steel',16)
  for x in [-.18,0,.18]:
   torus(m,(x,.92,.215),.016,.005,'steel','z',18,6)
 if m.name=='Bench':
  for x in [-.8,0,.8]:
   m.rod((x,.04,-.22),(x,.44,.22),.024,'steel',20)
   m.rod((x,.44,.22),(x,1.08,.22),.024,'steel',20)
  for x in [-.92,.92]:
   m.rod((x,.62,-.20),(x,.62,.22),.026,'steel',20)
   m.rod((x,.45,-.20),(x,.62,-.20),.024,'steel',20)
 if m.name=='Locker':
  for x in [-.25,.25]:
   for y in [.35,1.1,1.85]:
    m.rod((x-.22,y-.04,-.32),(x-.22,y+.04,-.32),.012,'steel',14)
   torus(m,(x+.14,1.05,-.345),.024,.006,'steel','z',20,6)
 if m.name=='Cabinet':
  for y in [.25,.75,1.25,1.75]:
   torus(m,(.42,y+.12,-.33),.020,.005,'steel','z',20,6)
   m.rod((-.14,y,-.355),(.14,y,-.355),.014,'steel',18)
 if m.name=='Generator':
  torus(m,(-.82,.6,0),.34,.025,'steel','x',32,8)
  for x in [-.35,-.05,.25]:
   torus(m,(x,.95,-.49),.065,.010,'steel','z',24,8)
   m.rod((x,.95,-.48),(x+.03,.98,-.50),.006,'red',12)
  for y in [.35,.65,.95]:
   torus(m,(.7,1.1+y*.4,0),.082,.014,'steel','y',24,8)
 if m.name=='XRay':
  torus(m,(0,1.6,-.11),.28,.014,'steel','z',32,8)
  for i in range(5):
   yy=1.45+i*.075
   m.rod((-.26,yy,-.135),(-.04,yy+.015,-.135),.010,'white',14)
   m.rod((.26,yy,-.135),(.04,yy+.015,-.135),.010,'white',14)
  m.rod((0,1.38,-.135),(0,1.82,-.135),.014,'white',16)
 if m.name=='Sink':
  for x in [-.14,.14]:
   m.rod((x,1.22,.20),(x,1.31,.20),.022,'steel',18)
   m.rod((x-.045,1.30,.20),(x+.045,1.30,.20),.012,'steel',14)
  torus(m,(0,1.22,-.05),.038,.007,'steel','y',24,6)
  m.box((.26,1.24,.18),(.12,.035,.09),'porcelain')
  # Coagulated blood pooled in the sink basin
  m.ellipsoid((0,1.235,-.05),(.22,.018,.15),'red')
 if m.name=='Radiator':
  for i in range(9):
   x=-.6+i*.15
   m.rod((x,.15,-.07),(x,.75,-.07),.028,'steel',18)
   m.rod((x,.15,.07),(x,.75,.07),.028,'steel',18)
  torus(m,(-.68,.72,0),.055,.012,'red','x',20,6)
 if m.name=='Vent':
  for x in [-.44,.44]:
   for y in [-.26,.26]:
    torus(m,(x,y,-.042),.018,.005,'steel','z',16,6)
  for x in [-.28,0,.28]:
   m.rod((x,-.28,-.045),(x,.28,-.045),.009,'steel',14)
 if m.name=='Extinguisher':
  torus(m,(0,.64,-.06),.028,.006,'steel','z',20,6)
  torus(m,(-.06,.66,0),.022,.005,'yellow','z',20,6)
  for y in [.20,.46]:
   torus(m,(0,y,0),.122,.008,'dark','y',28,6)
  m.rod((.18,.40,0),(.20,.18,0),.022,'dark',18,r2=.036)
 if m.name=='Lamp':
  for z in [-.055,.055]:
   m.rod((-.56,-.045,z),(.56,-.045,z),.024,'light',24)
   for x in [-.57,.57]:
    m.box((x,-.035,z),(.04,.05,.05),'steel')
  for x in [-.35,0,.35]:
   m.rod((x,-.02,-.12),(x,-.02,.12),.010,'steel',14)
 if m.name=='Pipe':
  for z in [-.88,-.30,.30,.88]:
   torus(m,(0,0,z),.115,.016,'steel','z',28,8)
  for a in range(6):
   ang=a*math.tau/6
   for z in [-.88,.88]:
    m.rod((.125*math.cos(ang),.125*math.sin(ang),z-.03),(.125*math.cos(ang),.125*math.sin(ang),z+.03),.009,'rust',10)
 if m.name=='MorgueDrawers':
  for x in [-.9,0,.9]:
   for y in [.4,1.1,1.8]:
    torus(m,(x+.28,y+.16,-.44),.038,.007,'steel','z',20,6)
    m.rod((x+.28,y+.16,-.44),(x+.30,y+.18,-.45),.005,'red',10)
    m.rod((x-.34,y-.22,-.44),(x-.34,y+.22,-.44),.012,'steel',14)
   # Dried blood seepage along the lower mortuary chamber seals
   m.ellipsoid((x,.12,-.425),(.28,.035,.012),'red')

 if m.name=='Sebastian':
  def gown(row,col):
   t=row/20;a=col*math.tau/48
   hem=.45+.018*math.sin(a*7)+.012*math.sin(a*13)
   y=hem+(1.035-hem)*t
   fold=.009*math.sin(a*18+t*3)*(1-t*.6)
   return ((.29-.10*t+fold)*math.cos(a),y,(.21-.06*t+fold)*math.sin(a))
  for row in range(20):
   for col in range(48):
    pts=[gown(row,col),gown(row+1,col),gown(row+1,col+1),gown(row,col+1)]
    m.face(pts,'linen',(0,sum(p[1] for p in pts)/4,0))
  for y in [.68,.78,.88,.98]:m.ellipsoid((0,y,-.137),(.015,.015,.009),'dark')
  for side in [-1,1]:
   for i in range(4):m.rod((side*(.235+i*.015),.49,-.045),(side*(.235+i*.015),.415,-.06),.009,'skin')

def uv_data(m):
 slots={'paint':0,'red':0,'yellow':0,'green':0,'steel':1,'rust':1,'linen':2,'white':2,'blue':2,'wood':3,'paper':6,'dark':6,'glass':6,'light':6,'skin':7,'hair':7}
 palette={'paint':(.31,.46,.42),'steel':(.33,.39,.4),'rust':(.37,.19,.1),'linen':(.73,.74,.65),'dark':(.065,.085,.09),'wood':(.28,.17,.1),'paper':(.82,.73,.52),'glass':(.2,.46,.5),'red':(.47,.08,.05),'yellow':(.8,.61,.19),'blue':(.16,.33,.48),'skin':(.65,.55,.45),'hair':(.09,.07,.05),'white':(.86,.87,.79),'light':(.72,.82,.65),'green':(.23,.5,.3)}
 palette.update({'vinyl':(.24,.25,.26),'cardboard':(.62,.47,.29),'porcelain':(.88,.86,.8),'rubber':(.12,.13,.14)})
 slots.update({'vinyl':8,'cardboard':9,'porcelain':10,'rubber':11})
 lookup={tuple(v):slots[k] for k,v in palette.items()};uv=[];groups=[]
 v=np.array(m.v);norm=np.array(m.n);axes=np.argmax(abs(norm),axis=1)
 for p,n,a,c in zip(v,norm,axes,m.c):
  pair=[p[2],p[1]] if a==0 else [p[0],p[2]] if a==1 else [p[0],p[1]]
  uv.extend(pair);groups.extend([lookup[tuple(c)],0])
 return m.uv,groups
