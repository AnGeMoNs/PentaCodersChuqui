"""Prepare four additional original surfaces and high-detail PBR data with weathered realism."""
from pathlib import Path
from PIL import Image,ImageFilter
import numpy as np,uuid
r=Path(__file__).resolve().parents[1];src=r/'SourceArt/TextureSources/AdditionalSurfaces.jpg'
if not src.exists():Image.open(r.parent/'generated_images/exec-2950054b-bbf6-4bf6-a52a-80fb73f6a2db.png').convert('RGB').save(src,quality=96)
im=Image.open(src);w,h=im.size
out=r/'Assets/Ecos/Resources/PBR'
al=Image.new('RGB',(1024,1024));normal=Image.new('RGB',(1024,1024));mask=Image.new('RGB',(1024,1024))
for i in range(4):
 x=i%2;y=i//2;tile=im.crop((x*w//2,y*h//2,(x+1)*w//2,(y+1)*h//2)).resize((496,496),Image.Resampling.LANCZOS)
 a=np.array(tile,dtype=float)/255;lum=a@np.array([.2126,.7152,.0722])
 blur_m=np.array(Image.fromarray((lum*255).astype('uint8')).filter(ImageFilter.GaussianBlur(1.8)))/255
 detail=lum-blur_m
 a=np.clip(a+detail[...,None]*0.28,0,1)
 height=np.array(Image.fromarray((lum*255).astype('uint8')).filter(ImageFilter.GaussianBlur(.6)))/255
 gy,gx=np.gradient(height);n=np.stack([-gx*3.2,gy*3.2,np.ones_like(gx)],-1);n/=np.linalg.norm(n,axis=-1)[...,None]
 rough=np.clip([.36,.12,.64,.22][i]+lum*.14,0,1)
 ao=np.clip(.82+lum*.18-np.abs(detail)*.3,0,1)
 maps=[(a*255).astype('uint8'),((n*.5+.5)*255).astype('uint8'),(np.stack([np.zeros_like(lum),rough,ao],-1)*255).astype('uint8')]
 for dest,arr in zip([al,normal,mask],maps):dest.paste(Image.fromarray(np.pad(arr,((8,8),(8,8),(0,0)),mode='edge')),(x*512,y*512))
al.save(out/'PropsAlbedo.jpg',quality=95);normal.save(out/'PropsNormal.png');mask.save(out/'PropsMask.png')
for name in ['PropsAlbedo.jpg','PropsNormal.png','PropsMask.png']:
 template=(out/('Albedo.jpg.meta' if name.endswith('.jpg') else 'DetailNormal.png.meta')).read_text()
 import re
 template=re.sub(r'guid: \w+','guid: '+uuid.uuid5(uuid.NAMESPACE_URL,'Ecos/PBR/'+name).hex,template).replace('maxTextureSize: 4096','maxTextureSize: 1024')
 (out/(name+'.meta')).write_text(template)
print('Additional material sources:',im.size,'; packed atlas 1024x1024; useful region 496px per surface.')
