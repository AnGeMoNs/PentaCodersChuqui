"""Pack original generated albedos and derive high-detail multi-scale normals/masks + blood/rust tiles.
Python + numpy + Pillow. Sources are 1254px, packed at 1024px; no fake 4K upscaling.
"""
from pathlib import Path
import numpy as np
from PIL import Image,ImageFilter
import uuid,json
ROOT=Path(__file__).resolve().parents[1];out=ROOT/'Assets/Ecos/Resources/PBR';out.mkdir(parents=True,exist_ok=True)
sources=ROOT/'SourceArt/TextureSources';sources.mkdir(exist_ok=True)
names=['Enamel','Steel','Linen','Wood','Plaster','Tiles']
files=['exec-95461178-85e6-450b-8404-40d9f27db926.png','exec-b69e93fc-ba82-49cf-bfa7-6e17aceb76f8.png','exec-969d4a1e-35c1-45b3-8b54-164e249b4ad2.png','exec-ce97db5b-21e1-4ded-a17b-d5251810c42e.png','exec-e67045f5-fb81-4955-b3dc-b3dae5c14793.png','exec-f54858e5-0949-4de0-aa9e-8d075d496b80.png']
atlas=Image.new('RGB',(4096,2048),'white');normal=Image.new('RGB',(4096,2048),(128,128,255));mask=Image.new('RGB',(4096,2048),(0,80,255))
rng=np.random.default_rng(19852026)

yy,xx=np.mgrid[0:1008,0:1008]/1008.0
splat=(np.sin(xx*2*np.pi*3)*np.cos(yy*2*np.pi*3)
      +0.5*np.sin((xx+yy)*2*np.pi*7)*np.cos((xx-yy)*2*np.pi*5)
      +0.25*np.sin(xx*2*np.pi*13)*np.sin(yy*2*np.pi*11))
splat=(splat-splat.min())/(splat.max()-splat.min()+1e-9)

for i,(name,file) in enumerate(zip(names,files)):
 src=sources/(name+'.jpg')
 if not src.exists():Image.open(ROOT.parent/'generated_images'/file).convert('RGB').save(src,quality=96)
 im=Image.open(src).resize((1008,1008),Image.Resampling.LANCZOS)
 a=np.array(im,dtype=np.float32)/255.0
 lum=a@np.array([.2126,.7152,.0722])
 blur_med=np.array(Image.fromarray((lum*255).astype('uint8')).filter(ImageFilter.GaussianBlur(2.2)))/255.0
 detail_hi=lum-blur_med
 a=np.clip(a+detail_hi[...,None]*0.32,0,1)
 if name in ('Plaster','Tiles','Enamel','Linen'):
  stain=np.clip((splat-0.62)*2.6,0,1)*(1.0-lum)*0.45
  blood_tint=np.array([0.26,0.025,0.02],dtype=np.float32)
  a=a*(1.0-stain[...,None])+blood_tint*stain[...,None]
 h_fine=np.array(Image.fromarray((lum*255).astype('uint8')).filter(ImageFilter.GaussianBlur(.55)))/255.0
 h_macro=np.array(Image.fromarray((lum*255).astype('uint8')).filter(ImageFilter.GaussianBlur(2.5)))/255.0
 h=h_fine*0.65+h_macro*0.35
 gy,gx=np.gradient(h);n=np.stack([-gx*4.8,gy*4.8,np.ones_like(gx)],axis=-1);n/=np.linalg.norm(n,axis=-1)[...,None]
 metal=np.ones_like(h)*([.24,.82,0,0,0,0][i]);smooth=np.clip([.46,.56,.14,.32,.18,.38][i]+(lum-.5)*.24,.04,.82);ao=np.clip(.78+lum*.22-np.abs(detail_hi)*.35,0,1)
 maps=[(np.clip(a,0,1)*255).astype('uint8'),((n*.5+.5)*255).astype('uint8'),(np.stack([metal,smooth,ao],axis=-1)*255).astype('uint8')]
 for target,arr in zip([atlas,normal,mask],maps):target.paste(Image.fromarray(np.pad(arr,((8,8),(8,8),(0,0)),mode='edge')),(i%4*1024,i//4*1024))

plaster_im=np.array(Image.open(sources/'Plaster.jpg').resize((1008,1008),Image.Resampling.LANCZOS),dtype=np.float32)/255.0
steel_im=np.array(Image.open(sources/'Steel.jpg').resize((1008,1008),Image.Resampling.LANCZOS),dtype=np.float32)/255.0
for extra_idx,base_arr,tint_col,met_val,sm_val in [
 (2, plaster_im, np.array([0.42,0.04,0.03]), 0.08, 0.68),
 (3, steel_im,   np.array([0.38,0.16,0.08]), 0.55, 0.34)
]:
 lum=base_arr@np.array([.2126,.7152,.0722])
 mix_mask=np.clip((splat*0.7+(1-lum)*0.3-0.35)*2.2,0,1)
 col=np.clip(base_arr*0.75*(1-mix_mask[...,None])+tint_col*mix_mask[...,None],0,1)
 h=lum*0.6+mix_mask*0.4
 gy,gx=np.gradient(h);n=np.stack([-gx*5.2,gy*5.2,np.ones_like(gx)],axis=-1);n/=np.linalg.norm(n,axis=-1)[...,None]
 metal=np.clip(np.ones_like(h)*met_val+mix_mask*0.15,0,1)
 smooth=np.clip(sm_val*mix_mask+0.2*(1-mix_mask),0.05,0.88)
 ao=np.clip(0.72+lum*0.28,0,1)
 maps=[(col*255).astype('uint8'),((n*.5+.5)*255).astype('uint8'),(np.stack([metal,smooth,ao],axis=-1)*255).astype('uint8')]
 for target,arr in zip([atlas,normal,mask],maps):target.paste(Image.fromarray(np.pad(arr,((8,8),(8,8),(0,0)),mode='edge')),(extra_idx*1024,0))

atlas.save(out/'Albedo.jpg',quality=95);normal.save(out/'DetailNormal.png');mask.save(out/'MaterialMask.png')
for name in ['Albedo.jpg','DetailNormal.png','MaterialMask.png']:
 path=out/name
 meta=Path(str(path)+'.meta');guid=uuid.uuid5(uuid.NAMESPACE_URL,'Ecos/PBR/'+path.name).hex
 meta.write_text(f'''fileFormatVersion: 2
guid: {guid}
TextureImporter:
  serializedVersion: 13
  mipmaps:
    enableMipMap: 1
    sRGBTexture: {1 if path.stem=='Albedo' else 0}
  isReadable: 0
  textureType: 0
  textureShape: 1
  maxTextureSize: 4096
  textureSettings:
    serializedVersion: 2
    filterMode: 2
    aniso: 8
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  textureCompression: 1
  compressionQuality: 75
  alphaIsTransparency: 0
  userData: Original generated material atlas; masks use linear sampling
''')
print('6 sources 1254px + 2 blood/rust cells; three 4096x2048 atlases, 1008px useful tiles + 8px gutters')
