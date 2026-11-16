"""Original hospital set dressing. Rebuild all 60 models, keeping gameplay ids."""
import runpy,math
from pathlib import Path
import numpy as np
b=runpy.run_path(str(Path(__file__).with_name('extend_assets.py')));Mesh=b['Mesh']
from detail_mesh import torus

def bolts(m,xs,ys,z):
 for x in xs:
  for y in ys:m.rod((x,y,z),(x,y,z-.012),.012,'steel')

def caster(m,x,z,y=.1):
 torus(m,(x,y,z),.06,.025,'rubber',segments=24)
 m.box((x,y+.075,z),(.04,.12,.08),'steel')

m=Mesh('BedsideTable');m.box((0,.43,0),(.65,.78,.55),'paint');m.box((0,.85,0),(.72,.07,.62),'porcelain')
for y in [.25,.59]:
 m.box((0,y,-.29),(.57,.28,.035),'paint');m.rod((-.12,y,-.33),(.12,y,-.33),.016,'steel')
for x in [-.25,.25]:
 for z in [-.2,.2]:m.rod((x,0,z),(x,.16,z),.025,'steel')
bolts(m,[-.26,.26],[.12,.73],-.315);m.save()

m=Mesh('MedicineShelf')
for x in [-.65,.65]:m.box((x,1.1,0),(.05,2.2,.32),'paint')
for y in [.16,.7,1.24,1.78,2.15]:m.box((0,y,0),(1.35,.04,.4),'paint')
for row in range(3):
 for i in range(6):
  x=-.5+i*.2;y=.73+row*.54
  m.rod((x,y,0),(x,y+.23,0),.06,'glass');m.rod((x,y+.23,0),(x,y+.28,0),.042,'rubber');m.box((x,y+.12,-.058),(.074,.12,.008),'paper')
for i in range(3):m.box((-.4+i*.4,.36,0),(.3,.34,.25),'cardboard')
m.save()

m=Mesh('ArchiveBoxes')
for i in range(3):
 x=(i-1)*.58;h=.36+(i%2)*.18
 m.box((x,h/2,0),(.54,h,.46),'cardboard');m.box((x,h+.015,0),(.56,.04,.48),'cardboard')
 m.box((x,h+.04,0),(.09,.006,.47),'paper');m.box((x,h*.5,-.235),(.25,.12,.006),'paper')
 for j in range(4):m.box((x-.01,h*.5-.035+j*.018,-.24),(.18-j*.02,.003,.004),'wood')
m.save()

m=Mesh('Defibrillator');m.box((0,.25,0),(.7,.48,.4),'porcelain');m.box((-.1,.3,-.21),(.35,.25,.02),'dark')
for i in range(14):m.box((-.25+i*.024,.3+(.07 if i==7 else -.06 if i==8 else .005),-.224),(.024,.008,.005),'green')
for x in [.15,.25]:
 for y in [.16,.27,.38]:m.ellipsoid((x,y,-.225),(.025,.025,.012),'red' if y==.16 else 'steel')
for x in [-.28,.28]:m.rod((x,.47,0),(x,.62,0),.02,'steel')
m.rod((-.28,.62,0),(.28,.62,0),.025,'rubber')
for side in [-1,1]:
 m.box((side*.4,.13,0),(.15,.06,.22),'steel');m.box((side*.4,.19,0),(.08,.09,.15),'rubber')
 for i in range(12):torus(m,(side*.38,.1,-.25-i*.018),.028,.006,'rubber','z',16,6)
m.save()

m=Mesh('JanitorCart')
for x in [-.4,.4]:
 for z in [-.3,.3]:caster(m,x,z);m.rod((x,.15,z),(x,1.05,z),.023,'steel')
for y in [.25,.8]:m.box((0,y,0),(.9,.05,.72),'steel')
m.rod((-.4,1.08,.3),(.4,1.08,.3),.025,'rubber')
for x in [-.22,.22]:
 m.rod((x,.83,0),(x,1.13,0),.16,'paint');torus(m,(x,1.14,0),.16,.016,'steel','y')
m.rod((.38,.5,.28),(.38,1.85,.28),.018,'wood')
for i in range(18):
 a=i*math.tau/18;m.rod((.38,.5,.28),(.38+.13*math.cos(a),.27,.28+.13*math.sin(a)),.01,'linen')
m.save()

m=Mesh('DeskLamp');m.rod((0,.02,0),(0,.07,0),.19,'steel')
for a,bp in [((0,.07,0),(0,.45,.12)),((0,.45,.12),(0,.68,-.1))]:
 for x in [-.025,.025]:m.rod((x,a[1],a[2]),(x,bp[1],bp[2]),.012,'steel')
 m.ellipsoid(bp,(.035,.035,.035),'steel')
m.rod((0,.68,-.1),(0,.59,-.23),.065,'paint',r2=.15);m.ellipsoid((0,.58,-.24),(.07,.02,.07),'light');m.save()

m=Mesh('WallClock');m.rod((0,0,.025),(0,0,-.025),.31,'paint',48);m.rod((0,0,-.026),(0,0,-.033),.282,'porcelain',48)
torus(m,(0,0,-.035),.292,.012,'steel','z')
for i in range(60):
 a=i*math.tau/60;r=.235 if i%5==0 else .25
 m.rod((r*math.sin(a),r*math.cos(a),-.04),(.266*math.sin(a),.266*math.cos(a),-.04),.005 if i%5==0 else .002,'dark')
m.rod((0,0,-.05),(-.13,.08,-.05),.008,'dark');m.rod((0,0,-.054),(.05,.22,-.054),.005,'dark');m.ellipsoid((0,0,-.06),(.02,.02,.01),'steel');m.save()

m=Mesh('Stretcher');m.box((0,.8,0),(.72,.12,1.95),'vinyl')
for x in [-.4,.4]:
 m.rod((x,.72,-1.15),(x,.72,1.15),.028,'steel')
 for z in [-.8,.8]:caster(m,x,z);m.rod((x,.18,z),(x,.72,z),.025,'steel')
 m.rod((x,.25,-.85),(x,.69,.85),.02,'steel');m.rod((x,.69,-.85),(x,.25,.85),.02,'steel')
for z in [-.48,.45]:m.box((0,.872,z),(.73,.012,.07),'rubber')
m.ellipsoid((0,.92,-.68),(.3,.07,.2),'linen');m.save()

m=Mesh('VinylSofa');m.box((0,.37,0),(1.9,.2,.7),'wood')
for x in [-.65,0,.65]:m.box((x,.53,-.02),(.61,.16,.65),'vinyl');m.box((x,.87,.3),(.61,.6,.15),'vinyl')
for x in [-1,1]:m.box((x,.65,0),(.15,.5,.8),'vinyl')
for x in [-.8,.8]:
 for z in [-.25,.25]:m.rod((x,0,z),(x,.3,z),.035,'steel')
m.save()

m=Mesh('ElectricalPanel');m.box((0,.6,0),(.85,1.2,.18),'paint');m.box((0,.6,-.11),(.75,1.1,.04),'steel')
for x in [-.22,0,.22]:
 for y in [.3,.55,.8]:m.box((x,y,-.15),(.13,.16,.04),'rubber');m.box((x,y,-.185),(.05,.07,.025),'white')
bolts(m,[-.35,.35],[.12,1.08],-.145)
for x in [-.23,.23]:m.rod((x,1.2,.02),(x,1.6,.02),.025,'steel')
m.box((0,1,-.14),(.24,.09,.01),'yellow');m.save()

m=Mesh('MedicalTray');m.box((0,.03,0),(.65,.035,.4),'steel')
for x in [-.32,.32]:m.box((x,.055,0),(.02,.07,.4),'steel')
for z in [-.2,.2]:m.box((0,.055,z),(.65,.07,.02),'steel')
for x in [-.18,0,.18]:
 torus(m,(x,.08,.1),.035,.006,'steel','y',24,6);m.rod((x,.08,.07),(x,.08,-.12),.009,'steel')
m.rod((-.21,.08,-.12),(-.15,.08,-.12),.008,'steel');m.save()

m=Mesh('WashBasin');m.box((0,.45,.15),(.32,.9,.3),'porcelain')
# Continuous lathed bowl with an inner and outer shell, smooth normals and a drain.
for outer in [False,True]:
 for j in range(18):
  for i in range(64):
   def pt(jj,ii):
    t=jj/18;a=ii*math.tau/64;radius=.07+.32*t
    return np.array([radius*math.cos(a),.90+.14*t*t-(.025 if outer else 0),radius*math.sin(a)])
   points=[pt(j,i),pt(j+1,i),pt(j+1,i+1),pt(j,i+1)]
   middle=(i+.5)*math.tau/64;t=(j+.5)/18
   desired=np.array([-.28*t/.32*math.cos(middle),1,-.28*t/.32*math.sin(middle)])*(-1 if outer else 1)
   start=len(m.v);m.face(points,'porcelain',np.mean(points,axis=0)-desired)
   for k in range(start,len(m.v)):
    q=np.array(m.v[k]);radius=np.linalg.norm(q[[0,2]]);t=(radius-.07)/.32
    n=np.array([-.28*t/.32*q[0]/radius,1,-.28*t/.32*q[2]/radius])*(-1 if outer else 1)
    m.n[k]=(n/np.linalg.norm(n)).tolist()
torus(m,(0,1.0275,0),.39,.014,'porcelain','y',64,8)
m.rod((0,.873,0),(0,.901,0),.07,'steel');m.rod((0,1.04,.3),(0,1.3,.3),.018,'steel');m.rod((0,1.3,.3),(0,1.3,.08),.018,'steel')
for x in [-.15,.15]:m.rod((x,1.04,.28),(x,1.1,.28),.025,'steel');m.box((x,1.11,.28),(.09,.02,.03),'steel')
m.save()

# 6 New High-Detail Hospital 3D Models (bringing total from 54 to 60):
m=Mesh('Incubator')
for x in [-.38,.38]:
 for z in [-.26,.26]:caster(m,x,z);m.rod((x,.15,z),(x,.78,z),.025,'steel')
m.box((0,.68,0),(.88,.24,.62),'paint')
m.box((0,.83,0),(.82,.06,.56),'linen')
m.box((0,1.05,0),(.84,.40,.58),'glass')
for x in [-.41,.41]:
 for z in [-.28,.28]:m.rod((x,.84,z),(x,1.25,z),.016,'steel')
for x in [-.20,.20]:
 torus(m,(x,1.04,-.295),.085,.014,'rubber','z',28,8)
 torus(m,(x,1.04,.295),.085,.014,'rubber','z',28,8)
m.box((-.22,.68,-.32),(.28,.14,.03),'dark')
for x in [.12,.22,.32]:
 torus(m,(x,.68,-.32),.028,.006,'steel','z',18,6)
m.save()

m=Mesh('Microscope')
m.box((0,.025,0),(.24,.05,.28),'dark')
m.rod((0,.04,.08),(0,.22,.08),.025,'steel')
torus(m,(0,.24,.03),.09,.018,'steel','x',28,8)
m.box((0,.17,-.02),(.16,.025,.16),'dark')
m.rod((0,.06,-.02),(0,.15,-.02),.028,'glass')
m.rod((0,.27,-.01),(0,.39,-.06),.024,'steel')
for a in [0,2.094,4.188]:
 m.rod((.025*math.cos(a),.25,-.02+.025*math.sin(a)),(.035*math.cos(a),.19,-.02+.035*math.sin(a)),.012,'steel')
for x in [-.022,.022]:
 m.rod((x,.38,-.06),(x,.44,-.11),.014,'dark')
 torus(m,(x,.44,-.11),.015,.004,'rubber','z',18,6)
for side in [-1,1]:
 torus(m,(side*.035,.14,.08),.024,.008,'steel','x',20,6)
m.save()

m=Mesh('AutopsyTable')
m.box((0,.42,0),(.76,.80,.62),'steel')
m.box((0,.85,0),(2.05,.09,.82),'steel')
for x in [-.98,.98]:m.box((x,.90,0),(.05,.06,.80),'steel')
for z in [-.38,.38]:m.box((0,.90,z),(2.02,.06,.05),'steel')
for z in [-.22,-.07,.07,.22]:
 m.rod((-.85,.898,z),(.75,.898,z),.008,'dark')
m.box((-.72,.92,0),(.22,.06,.32),'rubber')
m.rod((.88,.89,.24),(.88,1.18,.24),.018,'steel')
m.rod((.88,1.18,.24),(.74,1.18,.08),.016,'steel')
for y in [.90,1.02]:
 torus(m,(.88,y,-.24),.055,.009,'steel','y',24,6)
m.save()

m=Mesh('FirstAidKit')
m.box((0,0,0),(.42,.48,.16),'porcelain')
m.box((0,0,-.082),(.38,.44,.025),'white')
m.box((0,0,-.098),(.18,.055,.015),'red')
m.box((0,0,-.098),(.055,.18,.015),'red')
for y in [-.15,.15]:
 m.rod((-.19,y,-.082),(-.19,y-.03,-.082),.010,'steel')
m.rod((.17,0,-.095),(.17,.04,-.095),.012,'steel')
m.save()

m=Mesh('FuseBox')
m.box((0,.45,0),(.62,.86,.22),'paint')
m.box((0,.45,-.09),(.54,.76,.04),'dark')
for row in range(3):
 for col in range(2):
  x=-.12+col*.24;y=.24+row*.21
  m.rod((x,y,-.11),(x,y,-.16),.036,'porcelain',20)
  torus(m,(x,y,-.155),.038,.007,'steel','z',20,6)
m.rod((.33,.45,-.04),(.38,.45,-.04),.022,'steel')
m.rod((.38,.45,-.04),(.38,.64,-.12),.018,'red')
bolts(m,[-.26,.26],[.08,.82],-.115)
m.save()

m=Mesh('AudioTape')
m.box((0,.012,0),(.16,.024,.10),'dark')
m.box((0,.025,0),(.13,.006,.065),'paper')
for x in [-.038,.038]:
 torus(m,(x,.026,0),.018,.004,'white','y',24,6)
 m.rod((x,.001,0),(x,.028,0),.008,'dark',16)
m.box((0,.012,-.046),(.11,.025,.018),'rubber')
m.save()

print('60 models total: 54 upgraded original models + 6 new detailed clinical props.')
