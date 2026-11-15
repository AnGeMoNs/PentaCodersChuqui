"""Additional original hospital props. Run after create_assets.py; numpy required."""
import runpy,math
from pathlib import Path
base=runpy.run_path(str(Path(__file__).with_name('create_assets.py')))
Mesh=base['Mesh']
def wheels(m,x=.32,z=.32):
 for a in [-x,x]:
  for b in [-z,z]:m.ellipsoid((a,.09,b),(.06,.09,.09),'dark',8,4)
def pole(m,a,b,r=.025,col='steel'):m.rod(a,b,r,col)
m=Mesh('IVStand');pole(m,(0,.12,0),(0,2,0));
for i in range(5):
 a=i*math.pi*2/5;x,z=.4*math.cos(a),.4*math.sin(a);pole(m,(0,.15,0),(x,.12,z));m.ellipsoid((x,.07,z),(.065,.065,.04),'dark',8,4)
pole(m,(-.3,1.9,0),(.3,1.9,0));m.box((.23,1.64,0),(.15,.35,.07),'linen');pole(m,(.23,1.45,0),(.14,.5,.1),.008,'glass');m.save()
m=Mesh('Monitor');m.box((0,.24,0),(.62,.48,.34),'linen');m.box((-.04,.27,-.18),(.42,.3,.025),'dark')
for i in range(12):
 x=-.23+i*.034;y=.25+(.09 if i==5 else -.08 if i==6 else .014*math.sin(i));m.box((x,y,-.2),(.035,.012,.005),'green')
for y in [.12,.23,.34]:m.ellipsoid((.23,y,-.19),(.027,.027,.015),'yellow',8,4)
m.save()
m=Mesh('Trolley');wheels(m,.37,.25)
for x in [-.4,.4]:
 for z in [-.28,.28]:pole(m,(x,.1,z),(x,.95,z))
for y in [.3,.8]:m.box((0,y,0),(.9,.06,.65),'steel')
pole(m,(-.4,1,-.28),(-.4,1,.28));m.save() # Keep shelf free for separately placed equipment.
m=Mesh('OxygenTank');m.rod((0,.15,0),(0,1.2,0),.21,'green',16);m.ellipsoid((0,1.2,0),(.21,.18,.21),'green');m.rod((0,1.35,0),(0,1.5,0),.045,'steel');m.box((0,1.5,0),(.25,.04,.04),'yellow');m.ellipsoid((.12,1.4,-.05),(.08,.08,.04),'linen');m.box((0,.65,-.214),(.22,.2,.02),'white');m.save()
m=Mesh('Locker');m.box((0,1.1,0),(1,2.2,.6),'paint')
for x in [-.25,.25]:
 m.box((x,1.1,-.316),(.46,2.08,.03),'steel');m.box((x+.14,1.05,-.35),(.035,.18,.04),'dark')
 for y in [1.65,1.72,1.79,.3,.37,.44]:m.box((x,y,-.337),(.25,.02,.009),'dark')
m.save()
m=Mesh('Extinguisher');m.rod((0,.06,0),(0,.56,0),.12,'red',14);m.ellipsoid((0,.56,0),(.12,.1,.12),'red');m.box((0,.68,0),(.2,.04,.05),'dark');pole(m,(.1,.64,0),(.2,.35,0),.018,'dark');m.box((0,.34,-.122),(.14,.22,.01),'paper');m.save()
m=Mesh('Radiator')
for i in range(9):m.box((-.6+i*.15,.45,0),(.1,.7,.22),'linen')
for y in [.15,.72]:pole(m,(-.7,y,0),(.7,y,0),.05)
m.save()
m=Mesh('Vent');m.box((0,0,0),(1,.65,.08),'steel')
for y in [-.25,-.15,-.05,.05,.15,.25]:m.box((0,y,-.05),(.85,.055,.035),'dark')
m.save()
m=Mesh('Bench');m.box((0,.45,0),(2,.1,.55),'wood');m.box((0,.83,.23),(2,.62,.08),'paint')
for x in [-.8,.8]:pole(m,(x,0,0),(x,.45,0),.045)
m.save()
m=Mesh('Sink');m.box((0,.58,0),(.7,1.16,.5),'paint');m.box((0,1.17,0),(.85,.12,.65),'white');m.ellipsoid((0,1.23,-.05),(.28,.025,.2),'dark');pole(m,(0,1.2,.23),(0,1.5,.23),.024);pole(m,(0,1.5,.23),(0,1.5,0),.024);m.save()
m=Mesh('Fan');pole(m,(0,0,0),(0,-.25,0),.04);m.ellipsoid((0,-.3,0),(.17,.1,.17),'steel')
for i in range(4):
 a=i*math.pi/2;c,s=math.cos(a),math.sin(a);m.face([(c*.1,-.29,s*.1),(c*.95-s*.12,-.29,s*.95+c*.12),(c*.95+s*.12,-.29,s*.95-c*.12)],'wood',(0,-.4,0));m.face([(c*.1,-.31,s*.1),(c*.95-s*.12,-.31,s*.95+c*.12),(c*.95+s*.12,-.31,s*.95-c*.12)],'wood',(0,-.2,0))
m.save()
m=Mesh('BioBin');m.rod((0,0,0),(0,.6,0),.24,'yellow',12);m.rod((0,.6,0),(0,.65,0),.26,'dark',12);m.box((0,.32,-.242),(.2,.18,.01),'red');m.save()
m=Mesh('PrivacyScreen');
for x in [-.9,.9]:pole(m,(x,.1,0),(x,2.1,0));pole(m,(x,.1,-.3),(x,.1,.3))
for y in [.3,2.05]:pole(m,(-.9,y,0),(.9,y,0))
for i in range(14):m.box((-.84+i*.13,1.18,.025*math.sin(i*math.pi/2)),(.14,1.67,.035),'paint')
m.save()
m=Mesh('MorgueDrawers');m.box((0,1.1,0),(2.7,2.2,.8),'dark')
for x in [-.9,0,.9]:
 for y in [.4,1.1,1.8]:m.box((x,y,-.42),(.84,.64,.05),'steel');m.box((x,y,-.46),(.28,.055,.055),'dark');m.box((x-.2,y+.16,-.454),(.13,.07,.01),'paper')
m.save()
m=Mesh('Camcorder');m.box((0,.16,0),(.24,.26,.48),'dark');m.rod((0,.16,-.23),(0,.16,-.34),.12,'steel',14);m.rod((0,.16,-.34),(0,.16,-.35),.085,'glass',14)
m.box((-.25,.19,.03),(.28,.21,.035),'dark');m.box((-.25,.19,.008),(.23,.16,.008),'glass');m.box((.13,.16,.04),(.035,.2,.25),'wood');m.box((0,.32,.04),(.13,.07,.2),'steel');m.ellipsoid((.08,.28,-.1),(.025,.02,.02),'red');m.save()
m=Mesh('BrokenWindow')
for x in [-.65,.65]:m.box((x,0,0),(.07,1.35,.08),'wood')
for y in [-.65,0,.65]:m.box((0,y,0),(1.35,.07,.08),'wood')
m.box((0,0,0),(.06,1.35,.08),'wood')
for sx in [-1,1]:
 for sy in [-1,1]:
  # Concave shard: split along the internal diagonal, not a triangle fan.
  a,b,c,d=[(sx*.04,sy*.04,0),(sx*.6,sy*.04,0),(sx*.6,sy*.55,0),(sx*.34,sy*.18,0)]
  m.face([a,b,d],'glass',(0,0,.1));m.face([b,c,d],'glass',(0,0,.1))
m.save()
m=Mesh('CableBundle')
for j in range(3):
 for i in range(14):
  x=-1+i*.15;nx=x+.15;z=.07*j+.12*math.sin(i*.4);nz=.07*j+.12*math.sin((i+1)*.4);pole(m,(x,.025,z),(nx,.025,nz),.014,'dark')
m.save()
m=Mesh('SurgicalLamp');pole(m,(0,0,0),(0,-.45,0),.04);pole(m,(0,-.45,0),(.6,-.65,0),.035);m.rod((.6,-.65,0),(.6,-.8,0),.4,'steel',14)
for i in range(6):
 a=i*math.pi/3;m.ellipsoid((.6+.24*math.cos(a),-.82,.24*math.sin(a)),(.085,.025,.085),'light',8,4)
m.save()
print('18 additional original models generated: 42 total.')
