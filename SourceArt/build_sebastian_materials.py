"""Native-resolution character textures and enhanced gore/necrotic micro-normal derivation.
Normals are inferred from high-frequency texture detail, not scanned geometry.
"""
from pathlib import Path
import hashlib,io,numpy as np
from PIL import Image,ImageFilter
ROOT=Path(__file__).resolve().parents[1]
SRC=ROOT/'SourceArt/SebastianTextures';OUT=ROOT/'Assets/Ecos/Resources/Sebastian';OUT.mkdir(parents=True,exist_ok=True)
yy,xx=np.mgrid[0:1254,0:1254]/1254.0
vein_pattern=np.abs(np.sin(xx*2*np.pi*9+np.cos(yy*2*np.pi*7)*1.8)*np.cos(yy*2*np.pi*11-np.sin(xx*2*np.pi*8)*1.5))
vein_mask=np.clip((0.18-vein_pattern)*5.5,0,1)
splat_pattern=(np.sin(xx*2*np.pi*5)*np.cos(yy*2*np.pi*6)+0.5*np.sin((xx+yy)*2*np.pi*13))
splat_mask=np.clip((splat_pattern-0.45)*1.8,0,1)

for name in ['Face','Cloth']:
 image=Image.open(SRC/(name+'.png')).convert('RGB')
 arr=np.array(image,dtype=float)/255.0
 lum=arr@np.array([.2126,.7152,.0722])
 low_lum=np.array(image.convert('L').filter(ImageFilter.GaussianBlur(3.2)),dtype=float)/255.0
 hi_detail=lum-low_lum
 arr=np.clip(arr+hi_detail[...,None]*0.38,0,1)
 if name=='Face':
  # Subtle dark necrotic veins and dried crimson tear/mouth stains
  blood_col=np.array([0.28,0.018,0.015])
  vein_blend=vein_mask*0.32*(1.0-lum)
  arr=np.clip(arr*(1.0-vein_blend[...,None])+blood_col*vein_blend[...,None],0,1)
 else:
  # Bloodstains and hospital grime on the pediatric gown
  blood_col=np.array([0.34,0.022,0.018])
  stain_blend=splat_mask*0.42*(0.4+0.6*yy)
  arr=np.clip(arr*(1.0-stain_blend[...,None])+blood_col*stain_blend[...,None],0,1)
 enhanced_img=Image.fromarray(np.uint8(np.clip(arr*255,0,255)))
 buffer=io.BytesIO();enhanced_img.save(buffer,format='JPEG',quality=96,subsampling=0);(OUT/(name+'Albedo.jpg')).write_bytes(buffer.getvalue())
 gray=np.array(enhanced_img.convert('L'),dtype=float)/255
 low=np.array(enhanced_img.convert('L').filter(ImageFilter.GaussianBlur(2.6)),dtype=float)/255
 detail=gray-low
 gy,gx=np.gradient(detail)
 strength=1.85 if name=='Face' else 3.6
 normal=np.stack([-gx*strength,gy*strength,np.ones_like(gx)],axis=-1)
 normal/=np.linalg.norm(normal,axis=-1,keepdims=True)
 buffer=io.BytesIO();Image.fromarray(np.uint8(np.clip(normal*.5+.5,0,1)*255)).save(buffer,format='PNG');(OUT/(name+'Normal.png')).write_bytes(buffer.getvalue())
for p in OUT.iterdir():
 if p.suffix not in ['.jpg','.png']:continue
 linear='Normal' in p.stem;clamp=1 if p.stem.startswith('Face') else 0
 guid=hashlib.md5(('ecos-r10/'+p.name).encode()).hexdigest()
 p.with_suffix(p.suffix+'.meta').write_text(f'''fileFormatVersion: 2
guid: {guid}
TextureImporter:
  serializedVersion: 13
  mipmaps:
    enableMipMap: 1
    sRGBTexture: {0 if linear else 1}
  isReadable: 0
  textureType: 0
  textureShape: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 2
    aniso: 8
    mipBias: 0
    wrapU: {clamp}
    wrapV: {clamp}
    wrapW: {clamp}
  nPOTScale: 0
  textureCompression: 0
  compressionQuality: 100
  alphaIsTransparency: 0
  userData: Native 1254px character texture; RGB linear micro-normal where applicable
''')
print('Character maps:',[(p.name,Image.open(p).size) for p in OUT.iterdir() if p.suffix in ['.jpg','.png']])
