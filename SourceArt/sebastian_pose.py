"""Offline reference poses for rig inspection. Not Unity runtime verification."""
import numpy as np,math,json
from pathlib import Path
from scipy.spatial.transform import Rotation,Slerp
MOTION=json.loads((Path(__file__).resolve().parents[1]/'Assets/Ecos/Resources/Sebastian/Motion.json').read_text())

def rotations(chase,motion,phase,clock,voice=False,reach=1,search=False,turn=0):
 s=math.sin(phase)*motion;breathe=math.sin(clock*2.4)
 offset=-motion*(.05+.015*chase)+.006*math.sin(phase*2)*motion
 r=np.zeros((17,3));r[2]=[-9-chase*23+breathe*1.3,s*5,2+s*5-turn*8*motion]
 tp=clock%4.7;twitch=math.sin(tp/.28*math.pi)*10 if tp<.28 else 0
 r[3]=[8+chase*9,math.sin(clock*1.7)*28 if search else 0,14-18*chase+breathe*2+twitch]
 r[4]=[-(2+20*float(voice)),0,0]
 r[5]=[(chase*30-s*38)*(1-reach)+(chase*92-s*10)*reach,0,-7-chase*8];r[7]=[(chase*24+s*38)*(1-reach)+(chase*86+s*12)*reach,0,10+chase*6]
 r[6]=[8+chase*8+max(0,s)*8,0,-5];r[8]=[12+chase*10+max(0,-s)*8,0,8]
 r[13]=[-6-chase*9,chase*140,-12];r[14]=[-9-chase*6,-chase*140,9]
 for thigh,calf,foot,angle in [(9,10,15,phase),(11,12,16,phase+math.pi)]:
  z=-math.sin(angle)*(.10+.11*chase)*motion;y=.10+max(0,math.cos(angle))*.095*motion
  down=.66+offset-y;length=np.clip(math.hypot(down,z),.10,.559)
  aim=math.degrees(math.atan2(-z,down));hip=math.degrees(math.acos(np.clip((.32**2+length**2-.24**2)/(.64*length),-1,1)))
  knee=math.degrees(math.acos(np.clip((length**2-.32**2-.24**2)/.1536,-1,1)))
  r[thigh,0]=aim+hip;r[calf,0]=-knee;r[foot,0]=-aim-hip+knee
 return r,offset

def sample_pose(keys,time):
 a=keys[-1];b=a;blend=0
 for i in range(1,len(keys)):
  if time<=keys[i]['time']:
   a,b=keys[i-1],keys[i];blend=np.clip((time-a['time'])/(b['time']-a['time']),0,1);blend=blend*blend*(3-2*blend);break
 result={}
 for k in a:
  av=np.array(list(a[k].values())) if isinstance(a[k],dict) else a[k]
  bv=np.array(list(b[k].values())) if isinstance(b[k],dict) else b[k]
  result[k]=av*(1-blend)+bv*blend
 return result

def pose(d,chase=0,motion=0,phase=0,clock=1,voice=False,bind=False,reach=1,search=False,turn=0,attack=None,attack_time=0,alert=None):
 piv=np.array([[b['position'][a] for a in ['x','y','z']] for b in d['rig']]);angles,offset=rotations(chase,motion,phase,clock,voice,reach,search,turn)
 if bind:angles[:]=0;offset=0
 extra={};blend=0
 if not bind and (attack is not None or alert is not None):
  if attack is not None:
   frame=sample_pose(MOTION['attacks'][attack]['keys'],attack_time);blend=np.clip(attack_time/.10,0,1)*np.clip((1.2-attack_time)/.28,0,1)
  else:
   frame=sample_pose(MOTION['alert'],alert);blend=np.clip(alert/.10,0,1)*np.clip((.75-alert)/.15,0,1)
  offset=offset*(1-blend)+frame['pelvis']*blend
  for i,key in [(2,'spine'),(3,'head'),(5,'leftArm'),(7,'rightArm'),(6,'leftForearm'),(8,'rightForearm'),(13,'leftHand'),(14,'rightHand')]:extra[i]=frame[key]
  extra[4]=np.array([-frame['jaw'],0,0])
  # Recompute leg IK with the pose's lower pelvis, while keeping ground contact.
  for thigh,calf,foot,angle in [(9,10,15,phase),(11,12,16,phase+math.pi)]:
   z=-math.sin(angle)*(.10+.11*chase)*motion;y=.10+max(0,math.cos(angle))*.095*motion
   down=.66+offset-y;length=np.clip(math.hypot(down,z),.10,.559)
   aim=math.degrees(math.atan2(-z,down));hip=math.degrees(math.acos(np.clip((.32**2+length**2-.24**2)/(.64*length),-1,1)))
   knee=math.degrees(math.acos(np.clip((length**2-.32**2-.24**2)/.1536,-1,1)))
   angles[thigh,0]=aim+hip;angles[calf,0]=-knee;angles[foot,0]=-aim-hip+knee
 world=[];skin=[]
 for i,b in enumerate(d['rig']):
  x,y,z=np.radians(angles[i]);cx,sx=math.cos(x),math.sin(x);cy,sy=math.cos(y),math.sin(y);cz,sz=math.cos(z),math.sin(z)
  rx=np.array([[1,0,0],[0,cx,-sx],[0,sx,cx]]);ry=np.array([[cy,0,sy],[0,1,0],[-sy,0,cy]]);rz=np.array([[cz,-sz,0],[sz,cz,0],[0,0,1]])
  m=np.eye(4);m[:3,:3]=ry@rx@rz
  if i in extra:
   e=extra[i];target=Rotation.from_euler('zxy',[e[2],e[0],e[1]],degrees=True)
   interpolator=Slerp([0,1],Rotation.from_quat([Rotation.from_matrix(m[:3,:3]).as_quat(),target.as_quat()]))
   m[:3,:3]=interpolator([blend]).as_matrix()[0]
  m[:3,3]=piv[i]-(piv[b['parent']] if b['parent']>=0 else 0)
  if i==1:m[1,3]+=offset
  w=world[b['parent']]@m if b['parent']>=0 else m;world.append(w)
  inv=np.eye(4);inv[:3,3]=-piv[i];skin.append(w@inv)
 skin=np.array(skin);v=np.array(d['vertices']).reshape(-1,3);n=np.array(d['normals']).reshape(-1,3);v4=np.column_stack([v,np.ones(len(v))]);w=np.array(d['weight0'])[:,None]
 a=np.array(d['bone0']);b=np.array(d['bone1']);out=(np.einsum('nij,nj->ni',skin[a],v4)*w+np.einsum('nij,nj->ni',skin[b],v4)*(1-w))[:,:3]
 normals=np.einsum('nij,nj->ni',skin[a,:3,:3],n)*w+np.einsum('nij,nj->ni',skin[b,:3,:3],n)*(1-w)
 normals/=np.maximum(np.linalg.norm(normals,axis=1)[:,None],1e-12)
 return out,normals
