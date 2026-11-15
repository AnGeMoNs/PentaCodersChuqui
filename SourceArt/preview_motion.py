"""Render the actual skinned project mesh with the shared attack keyframes.
Offline OpenGL inspection, not footage of the Unity game. Requires moderngl/EGL,
numpy, scipy, Pillow and ffmpeg. Generates a 16-second 24fps MP4 with review music.
"""
from pathlib import Path
import json,subprocess,io
import numpy as np
import moderngl
from PIL import Image,ImageDraw,ImageFont
from sebastian_pose import pose
ROOT=Path(__file__).resolve().parents[1];W,H,FPS=960,640,24
ctx=moderngl.create_standalone_context(backend='egl');fbo=ctx.simple_framebuffer((W,H));fbo.use();ctx.enable(moderngl.DEPTH_TEST)
vs='''#version 330
in vec3 p;in vec3 n;in vec2 uv;in vec2 cat;
uniform mat4 mvp;out vec3 norm;out vec2 texuv;out vec2 kind;
void main(){gl_Position=mvp*vec4(p,1);norm=n;texuv=uv;kind=cat;}
'''
fs='''#version 330
in vec3 norm;in vec2 texuv;in vec2 kind;uniform sampler2D faceTex;uniform sampler2D clothTex;out vec4 color;
void main(){vec2 su=fract(texuv*3)*vec2(.18,.12)+vec2(.03,.82);vec3 skin=texture(faceTex,su).rgb;vec3 albedo;
if(kind.x<.5)albedo=mix(skin,texture(faceTex,clamp(texuv,0.,1.)).rgb,kind.y);
else if(kind.x<1.5)albedo=texture(clothTex,texuv*2.8).rgb;
else if(kind.x<2.5)albedo=skin;
else if(kind.x<3.5)albedo=mix(vec3(.012,.011,.010),vec3(.027,.025,.022),.5+.5*sin(texuv.x*1900+sin(texuv.y*70)*2));
else if(kind.x<4.5)albedo=vec3(.006,.004,.005);
else albedo=vec3(.30,.28,.23);
vec3 N=normalize(norm);float lambert=max(0.,dot(N,normalize(vec3(-.4,.7,-.8))));
float fill=max(0.,dot(N,normalize(vec3(.7,.2,.4))));color=vec4(albedo*(.30+.75*lambert+.15*fill),1);}
'''
prog=ctx.program(vertex_shader=vs,fragment_shader=fs)
d=json.loads((ROOT/'Assets/Ecos/Resources/Models/Sebastian.json').read_text());nv=len(d['vertices'])//3
vbo=ctx.buffer(reserve=nv*6*4);uvbo=ctx.buffer(np.column_stack([np.array(d['uv']).reshape(-1,2),np.array(d['groups']).reshape(-1,2)]).astype('f4').tobytes());ibo=ctx.buffer(np.array(d['triangles'],dtype='u4').tobytes())
vao=ctx.vertex_array(prog,[(vbo,'3f 3f','p','n'),(uvbo,'2f 2f','uv','cat')],ibo)
for i,name in enumerate(['FaceAlbedo.jpg','ClothAlbedo.jpg']):
 im=Image.open(ROOT/'Assets/Ecos/Resources/Sebastian'/name).convert('RGB').transpose(Image.Transpose.FLIP_TOP_BOTTOM)
 tex=ctx.texture(im.size,3,im.tobytes());tex.build_mipmaps();tex.filter=(moderngl.LINEAR_MIPMAP_LINEAR,moderngl.LINEAR);tex.use(i)
 prog['faceTex' if i==0 else 'clothTex']=i

def lookat(eye,target):
 f=target-eye;f/=np.linalg.norm(f);r=np.cross(f,[0,1,0]);r/=np.linalg.norm(r);u=np.cross(r,f)
 m=np.eye(4);m[:3,:3]=[r,u,-f];m[:3,3]=-m[:3,:3]@eye;return m
view=lookat(np.array([1.4,1.30,-3.8]),np.array([0,.78,0]))
proj=np.zeros((4,4));f=1/np.tan(np.radians(29)/2);proj[0,0]=f/(W/H);proj[1,1]=f;proj[2,2]=-1.002;proj[2,3]=-.2002;proj[3,2]=-1
prog['mvp'].write((proj@view).astype('f4').T.tobytes())
font=ImageFont.truetype('DejaVuSans.ttf',19);small=ImageFont.truetype('DejaVuSans.ttf',13);big=ImageFont.truetype('DejaVuSans.ttf',26)
output=ROOT/'Docs/Sebastian_Animaciones_11.mp4'
cmd=['ffmpeg','-y','-v','error','-f','rawvideo','-pix_fmt','rgb24','-s',f'{W}x{H}','-r',str(FPS),'-i','-','-i',str(ROOT/'Docs/Musica_Persecucion_11.wav'),'-c:v','libx264','-crf','20','-preset','fast','-pix_fmt','yuv420p','-c:a','aac','-b:a','160k','-shortest','-movflags','+faststart',str(output)]
proc=subprocess.Popen(cmd,stdin=subprocess.PIPE)
snapshots=[]
for frame in range(16*FPS):
 t=frame/FPS;args={'clock':t,'voice':False}
 if t<2:label='ACECHO';args.update(motion=.45,phase=t*4,chase=0,reach=0)
 elif t<2.75:label='DETECCIÓN';args.update(chase=1,alert=t-2,motion=0)
 elif t<6.5:label='CARRERA ENCORVADA';args.update(chase=1,motion=1,phase=(t-2.75)*10,reach=0)
 elif t<8:label='TE ESTÁ ALCANZANDO';args.update(chase=1,motion=1,phase=t*10,reach=1)
 elif t<11.6:
  attack=min(2,int((t-8)/1.2));tt=t-8-attack*1.2;label=['ZARPAZO IZQUIERDO','ZARPAZO DERECHO','AGARRE FRONTAL'][attack]
  movement=np.sin(np.pi*np.clip((tt-.36)/.20,0,1)) if .36<=tt<=.56 else 0
  args.update(chase=1,motion=movement,phase=np.clip((tt-.36)/.2,0,1)*np.pi,attack=attack,attack_time=tt)
 else:label='BÚSQUEDA TRAS PERDERTE';args.update(chase=1,motion=.3,phase=t*3,search=True,reach=0)
 v,n=pose(d,**args);vbo.write(np.column_stack([v,n]).astype('f4').tobytes())
 fbo.clear(.025,.035,.038,1);vao.render();im=Image.frombytes('RGB',(W,H),fbo.read(components=3)).transpose(Image.Transpose.FLIP_TOP_BOTTOM)
 dr=ImageDraw.Draw(im);dr.rectangle((0,0,W,80),fill=(8,12,14));dr.text((28,16),'SEBASTIÁN / REVISIÓN 11',font=big,fill=(224,220,207));dr.text((29,52),'Inspección de la malla y las poses fuera de Unity',font=small,fill=(140,157,156))
 dr.rectangle((0,H-77,W,H),fill=(8,12,14));dr.text((28,H-62),label,font=font,fill=(205,218,195));dr.text((28,H-31),'Animación en sitio · La colisión, iluminación y mezcla del juego requieren prueba en Unity.',font=small,fill=(139,153,150))
 dr.rectangle((28,H-9,28+(W-56)*t/16,H-6),fill=(154,53,45))
 if frame in [24,60,108,204,233,262]:snapshots.append(im.copy())
 proc.stdin.write(im.tobytes())
proc.stdin.close();assert proc.wait()==0
sheet=Image.new('RGB',(W*3,H*2))
for i,im in enumerate(snapshots):sheet.paste(im,((i%3)*W,(i//3)*H))
b=io.BytesIO();sheet.save(b,format='JPEG',quality=90);(ROOT/'Docs/Animaciones_11.jpg').write_bytes(b.getvalue())
print('Rendered',output,'16 seconds, 24 fps; offline preview, not a Unity recording.')
