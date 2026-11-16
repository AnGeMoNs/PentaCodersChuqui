"""Orthographic depth-buffer render of project meshes; not a Unity screenshot.
Requires numpy and Pillow; no modification of game models.
"""
from pathlib import Path
import json,numpy as np
from PIL import Image,ImageDraw,ImageFont
root=Path(__file__).resolve().parents[1]
models=[('Camcorder','VIDEOCÁMARA'),('Monitor','MONITOR CLÍNICO'),('IVStand','PORTASUEROS'),('Trolley','CARRO MÉDICO'),('Locker','CASILLEROS'),('MorgueDrawers','CÁMARAS DE CONSERVACIÓN'),('OxygenTank','CILINDRO DE OXÍGENO'),('PrivacyScreen','BIOMBO'),('SurgicalLamp','LÁMPARA QUIRÚRGICA')]
w,h=480,370
canvas=Image.new('RGB',(1440,1320),(16,26,27));draw=ImageDraw.Draw(canvas)
font=ImageFont.truetype('DejaVuSans.ttf',18);big=ImageFont.truetype('DejaVuSans.ttf',30)
def label(text,x,y,font):draw.text((x,y),text,font=font,fill=(220,212,181),anchor='mt')
label('ECOS · NUEVO EQUIPAMIENTO 3D',720,25,big)
cam=np.array([3,2,-5.]);cam/=np.linalg.norm(cam);right=np.array([5.,0,3]);right/=np.linalg.norm(right);up=np.cross(right,cam)
light=np.array([.4,.7,-.6]);light/=np.linalg.norm(light)
for idx,(name,title) in enumerate(models):
 d=json.loads((root/f'Assets/Ecos/Resources/Models/{name}.json').read_text());v=np.array(d['vertices']).reshape(-1,3);n=np.array(d['normals']).reshape(-1,3);c=np.array(d['colors']).reshape(-1,3);t=np.array(d['triangles']).reshape(-1,3)
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
  region[mask]=z[mask];shade=.46+.54*max(0,n[tri[0]]@light);color=np.clip(c[tri[0]]*shade*255,0,255).astype('uint8');rgb[lo[1]:hi[1]+1,lo[0]:hi[0]+1][mask]=color
 row,col=divmod(idx,3);canvas.paste(Image.fromarray(rgb),(col*w,110+row*390));label(title,col*w+w//2,83+row*390,font)
label('9 de los 18 modelos nuevos · Vista de mallas, no captura de Unity',720,1290,font)
canvas.save(root/'Docs/Modelos_Nuevos.png')
