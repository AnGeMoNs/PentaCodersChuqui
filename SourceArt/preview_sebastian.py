"""Orthographic depth-buffer render of project meshes; not a Unity screenshot.
Requires numpy and Pillow; no modification of game models.
"""
from pathlib import Path
import json,io,numpy as np
from PIL import Image,ImageDraw,ImageFont
root=Path(__file__).resolve().parents[1]
from sebastian_pose import pose
models=[('Sebastian','CUERPO Y VESTUARIO'),('Sebastian','ROSTRO · MALLA TEXTURIZADA'),('Sebastian','PERSECUCIÓN')]

w,h=720,760
canvas=Image.new('RGB',(2160,960),(16,26,27));draw=ImageDraw.Draw(canvas)
font=ImageFont.truetype('DejaVuSans.ttf',18);big=ImageFont.truetype('DejaVuSans.ttf',30)
def label(text,x,y,font):draw.text((x,y),text,font=font,fill=(220,212,181),anchor='mt')
label('SEBASTIÁN · ROSTRO Y MATERIALES / REVISIÓN 10',1080,25,big)
cam=np.array([3,2,-5.]);cam/=np.linalg.norm(cam);right=np.array([5.,0,3]);right/=np.linalg.norm(right);up=np.cross(right,cam)
light=np.array([.4,.7,-.6]);light/=np.linalg.norm(light)
face=np.asarray(Image.open(root/'Assets/Ecos/Resources/Sebastian/FaceAlbedo.jpg'),dtype=float)/255
cloth=np.asarray(Image.open(root/'Assets/Ecos/Resources/Sebastian/ClothAlbedo.jpg'),dtype=float)/255
def sample(texture,uv,repeat=False):
 uv=np.mod(uv,1) if repeat else np.clip(uv,0,1)
 tx=(uv[...,0]*(texture.shape[1]-1)).astype(int);ty=((1-uv[...,1])*(texture.shape[0]-1)).astype(int)
 return texture[ty,tx]

for idx,(name,title) in enumerate(models):
 d=json.loads((root/f'Assets/Ecos/Resources/Models/{name}.json').read_text());v=np.array(d['vertices']).reshape(-1,3);n=np.array(d['normals']).reshape(-1,3);c=np.array(d['colors']).reshape(-1,3);t=np.array(d['triangles']).reshape(-1,3)
 uv=np.array(d["uv"]).reshape(-1,2);channels=np.array(d["groups"]).reshape(-1,2);slots=channels[:,0]
 v,n=pose(d,bind=idx==1,chase=1 if idx==2 else 0,motion=1 if idx==2 else 0,phase=.7,clock=1+idx*.7,voice=idx>0)
 cam=np.array([.03,.01,-1.] if idx==1 else [2.,1,-7.]);cam/=np.linalg.norm(cam);right=np.array([-cam[2],0,cam[0]]);right/=np.linalg.norm(right);up=np.cross(right,cam)
 proj=np.column_stack([v@right,-v@up,v@cam]);mid=(proj[:,:2].min(0)+proj[:,:2].max(0))/2;extent=np.ptp(proj[:,:2],axis=0);scale=min((w-80)/max(extent[0],.001),(h-40)/max(extent[1],.001));proj[:,:2]=(proj[:,:2]-mid)*scale+[w/2,h/2]
 if idx==1:
  center=np.array([0,1.345,-.02]);mid=np.array([center@right,-center@up]);scale=1800;proj[:,:2]=(np.column_stack([v@right,-v@up])-mid)*scale+[w/2,h/2]
 rgb=np.empty((h,w,3),dtype=np.uint8);rgb[:]=(16,26,27);depth=np.full((h,w),-np.inf)
 for tri in t:
  if n[tri[0]]@cam<=0:continue
  p=proj[tri];lo=np.maximum(np.floor(p[:,:2].min(0)).astype(int),[0,0]);hi=np.minimum(np.ceil(p[:,:2].max(0)).astype(int),[w-1,h-1])
  if np.any(hi<lo):continue
  xx,yy=np.meshgrid(np.arange(lo[0],hi[0]+1)+.5,np.arange(lo[1],hi[1]+1)+.5)
  a,b,q=p;den=(b[1]-q[1])*(a[0]-q[0])+(q[0]-b[0])*(a[1]-q[1])
  if abs(den)<1e-9:continue
  u=((b[1]-q[1])*(xx-q[0])+(q[0]-b[0])*(yy-q[1]))/den;vv=((q[1]-a[1])*(xx-q[0])+(a[0]-q[0])*(yy-q[1]))/den;ww=1-u-vv;z=u*a[2]+vv*b[2]+ww*q[2]
  region=depth[lo[1]:hi[1]+1,lo[0]:hi[0]+1];mask=(u>=-1e-6)&(vv>=-1e-6)&(ww>=-1e-6)&(z>region)
  region[mask]=z[mask]
  nn=u[...,None]*n[tri[0]]+vv[...,None]*n[tri[1]]+ww[...,None]*n[tri[2]];nn/=np.maximum(np.linalg.norm(nn,axis=-1)[...,None],1e-8)
  shade=.4+.6*np.maximum(0,nn@light)
  texuv=u[...,None]*uv[tri[0]]+vv[...,None]*uv[tri[1]]+ww[...,None]*uv[tri[2]]
  slot=int(slots[tri[0]])
  skinuv=np.mod(texuv*3,1)*[.18,.12]+[.03,.82];skin=sample(face,skinuv)
  if slot==0:
   blend=channels[tri,1]
   amount=u*blend[0]+vv*blend[1]+ww*blend[2]
   color=skin*(1-amount[...,None])+sample(face,texuv)*amount[...,None]
  elif slot==1:color=sample(cloth,texuv*2.8,True)
  elif slot==2:color=skin
  elif slot==3:
   strand=.5+.5*np.sin(texuv[...,0]*1900+np.sin(texuv[...,1]*70)*2)
   color=np.array([.012,.011,.010])+(np.array([.027,.025,.022])-[.012,.011,.010])*strand[...,None]
  elif slot==4:color=np.ones((*xx.shape,3))*[.006,.004,.005]
  else:color=np.ones((*xx.shape,3))*[.30,.28,.23]
  color=np.clip(color*shade[...,None]*255,0,255).astype('uint8')
  rgb[lo[1]:hi[1]+1,lo[0]:hi[0]+1][mask]=color[mask]
 row,col=divmod(idx,3);canvas.paste(Image.fromarray(rgb),(col*w,110+row*790));label(title,col*w+w//2,83+row*790,font)
label('Malla del proyecto y poses calculadas · Inspección fuera de Unity, sin el shader final',1080,915,font)
buffer=io.BytesIO();canvas.save(buffer,format='PNG');(root/'Docs/Sebastian_Revision10.png').write_bytes(buffer.getvalue())
