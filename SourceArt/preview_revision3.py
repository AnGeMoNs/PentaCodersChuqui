"""Orthographic depth-buffer render of project meshes; not a Unity screenshot.
Requires numpy and Pillow; no modification of game models.
"""
from pathlib import Path
import json,numpy as np
from PIL import Image,ImageDraw,ImageFont
root=Path(__file__).resolve().parents[1]
models=[('MedicineShelf','ESTANTERÍA / FRASCOS'),('Defibrillator','DESFIBRILADOR'),('JanitorCart','CARRO DE LIMPIEZA'),('VinylSofa','ASIENTOS DE VINILO'),('WashBasin','LAVAMANOS'),('ArchiveBoxes','CAJAS DE ARCHIVO')]

w,h=720,560
canvas=Image.new('RGB',(2160,1340),(16,26,27));draw=ImageDraw.Draw(canvas)
font=ImageFont.truetype('DejaVuSans.ttf',18);big=ImageFont.truetype('DejaVuSans.ttf',30)
def label(text,x,y,font):draw.text((x,y),text,font=font,fill=(220,212,181),anchor='mt')
label('ECOS · REVISIÓN DE GEOMETRÍA Y MATERIALES',1080,25,big)
cam=np.array([3,2,-5.]);cam/=np.linalg.norm(cam);right=np.array([5.,0,3]);right/=np.linalg.norm(right);up=np.cross(right,cam)
light=np.array([.4,.7,-.6]);light/=np.linalg.norm(light)
atlas=np.asarray(Image.open(root/'Assets/Ecos/Resources/PBR/Albedo.jpg'),dtype=float)/255
propsAtlas=np.asarray(Image.open(root/'Assets/Ecos/Resources/PBR/PropsAlbedo.jpg'),dtype=float)/255
for idx,(name,title) in enumerate(models):
 d=json.loads((root/f'Assets/Ecos/Resources/Models/{name}.json').read_text());v=np.array(d['vertices']).reshape(-1,3);n=np.array(d['normals']).reshape(-1,3);c=np.array(d['colors']).reshape(-1,3);t=np.array(d['triangles']).reshape(-1,3)
 uv=np.array(d["uv"]).reshape(-1,2);slots=np.array(d["groups"]).reshape(-1,2)[:,0]
 proj=np.column_stack([v@right,-v@up,v@cam]);mid=(proj[:,:2].min(0)+proj[:,:2].max(0))/2;extent=np.ptp(proj[:,:2],axis=0);scale=min((w-80)/max(extent[0],.001),(h-40)/max(extent[1],.001));proj[:,:2]=(proj[:,:2]-mid)*scale+[w/2,h/2]
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
  slot=int(slots[tri[0]]);tint=c[tri[0]]
  if slot<6:
   tx=(slot%4*1024+8+np.mod(texuv[...,0],1)*1008).astype(int)
   ty=(slot//4*1024+8+(1-np.mod(texuv[...,1],1))*1008).astype(int)
   sample=atlas[np.clip(ty,0,2047),np.clip(tx,0,4095)]
   if slot==0:tint=np.clip(tint*1.65+.08,0,1)
   elif slot==1:tint=np.array([.9,.93,.94])
   elif slot==2:tint=np.clip(tint*1.3,0,1)
   elif slot==3:tint=np.array([.9,.86,.8])
  elif slot>=8:
   sid=slot-8;tx=(sid%2*512+8+np.mod(texuv[...,0],1)*496).astype(int);ty=(sid//2*512+8+(1-np.mod(texuv[...,1],1))*496).astype(int)
   sample=propsAtlas[np.clip(ty,0,1023),np.clip(tx,0,1023)];tint=np.array([.9,.9,.9])
  else:sample=np.ones((*xx.shape,3))
  color=np.clip(sample*tint*shade[...,None]*255,0,255).astype('uint8')
  rgb[lo[1]:hi[1]+1,lo[0]:hi[0]+1][mask]=color[mask]
 row,col=divmod(idx,3);canvas.paste(Image.fromarray(rgb),(col*w,110+row*590));label(title,col*w+w//2,83+row*590,font)
label('Mallas y albedo del proyecto · Render de inspección, no captura de Unity',1080,1300,font)
canvas.save(root/'Docs/Objetos_Revision3.png')
