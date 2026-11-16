"""Read actual Build() callsites; audit generated-model bounds offline.
AABB overlap warnings are conservative and do not replace Unity collision tests.
"""
from pathlib import Path
import json,itertools,re,math
import numpy as np
root=Path(__file__).resolve().parents[1]
models={p.stem:np.array(json.loads(p.read_text())['vertices']).reshape(-1,3) for p in (root/'Assets/Ecos/Resources/Models').glob('*.json')}
placements=[]

def split_top_args(s):
 args=[];depth=0;cur=[]
 for ch in s:
  if ch in '([{':depth+=1
  elif ch in ')]}':depth-=1
  if ch==',' and depth==0:args.append(''.join(cur).strip());cur=[]
  else:cur.append(ch)
 if cur:args.append(''.join(cur).strip())
 return args

def eval_expr(expr,env):
 expr=expr.strip()
 while expr.startswith('(') and expr.endswith(')'):
  d=0;ok=True
  for i,ch in enumerate(expr[:-1]):
   if ch=='(':d+=1
   elif ch==')':d-=1
   if d==0:ok=False;break
  if ok:expr=expr[1:-1].strip()
  else:break
 m=re.match(r'^new\s+Vector3\s*\((.*)\)$',expr)
 if m:return [eval_expr(a,env) for a in split_top_args(m.group(1))]
 m=re.match(r'^new\s*\[\s*\]\s*\{(.*)\}\s*\[\s*(.*)\s*\]$',expr)
 if m:
  items=[eval_expr(a,env) for a in split_top_args(m.group(1))]
  idx=int(eval_expr(m.group(2),env))
  return items[idx]
 d=0;q_idx=-1;c_idx=-1
 for i,ch in enumerate(expr):
  if ch in '([{':d+=1
  elif ch in ')]}':d-=1
  elif d==0 and ch=='?' and q_idx<0:q_idx=i
  elif d==0 and ch==':' and q_idx>=0 and c_idx<0:c_idx=i;break
 if q_idx>=0 and c_idx>=0:
  cond=eval_expr(expr[:q_idx],env)
  return eval_expr(expr[q_idx+1:c_idx] if cond else expr[c_idx+1:],env)
 if expr in ('true','false'):return expr=='true'
 if expr.startswith('"') and expr.endswith('"'):return json.loads(expr)
 py_expr=re.sub(r'(\d+|\.\d+)f\b',r'\1',expr)
 py_expr=py_expr.replace('&&',' and ').replace('||',' or ')
 py_expr=re.sub(r'!(?!=)',r' not ',py_expr)
 return eval(py_expr,{"__builtins__":{}},env)

def parse_headers(text):
 loops=[];conds=[]
 for m in re.finditer(r'\b(for|foreach|if)\s*\(',text):
  kw=m.group(1);start_p=m.end()-1;d=0;end_p=start_p
  for k in range(start_p,len(text)):
   if text[k]=='(':d+=1
   elif text[k]==')':
    d-=1
    if d==0:end_p=k;break
  header=text[start_p+1:end_p]
  if kw=='for':
   fm=re.search(r'int\s+(\w+)\s*=\s*(\d+)\s*;\s*\1\s*<(=?)\s*(\d+)',header)
   if fm:
    limit=int(fm.group(4))+(1 if fm.group(3)=='=' else 0)
    loops.append((fm.group(1),range(int(fm.group(2)),limit)))
  elif kw=='foreach' and 'side' in header:
   loops.append(('side',[-1,1]))
  elif kw=='if' and not any(x in header for x in ['Application.isPlaying','name==','Mathf.','!map.Cells']):
   conds.append(header)
 return loops,conds

for filename in ['LevelBuilder.cs','HospitalDressing.cs']:
 lines=(root/'Assets/Ecos/Scripts'/filename).read_text().splitlines()
 in_build=False;ctx_stack=[];pending_loops=[];pending_conds=[]
 for line_no,raw in enumerate(lines,1):
  line=re.sub(r'//.*$','',raw).strip()
  if not line:continue
  if ' Build(' in raw:in_build=True;continue
  if not in_build:continue
  # Ignore array initializer braces like new[]{-1,1}
  clean_braces=re.sub(r'new\s*\[\s*\]\s*\{[^}]*\}','ARRAY_INIT',line)
  opens=clean_braces.count('{');closes=clean_braces.count('}')
  if opens>closes:
   brace_pos=clean_braces.find('{')
   l_before,c_before=parse_headers(line[:brace_pos])
   b_loops=pending_loops+l_before;b_conds=pending_conds+c_before
   pending_loops=[];pending_conds=[]
   for _ in range(opens-closes):
    ctx_stack.append((b_loops,b_conds));b_loops=[];b_conds=[]
   inline_text=line[brace_pos+1:] if brace_pos>=0 else line
  else:
   inline_text=line
  l_in,c_in=parse_headers(inline_text)
  if (l_in or c_in) and opens==0 and not re.search(r'\b(Model|Prop|Item|Clue|Box|Sign|Gate|Document)\s*\(',inline_text):
   pending_loops.extend(l_in);pending_conds.extend(c_in)
   continue
  loops=[];conds=[]
  for b_l,b_c in ctx_stack:loops.extend(b_l);conds.extend(b_c)
  loops.extend(l_in);conds.extend(c_in)
  for cm in re.finditer(r'\b(Model|Prop|Item|Clue)\s*\(',line):
   fn=cm.group(1);start_p=cm.end()-1;d=0;end_p=start_p
   for k in range(start_p,len(line)):
    if line[k]=='(':d+=1
    elif line[k]==')':
     d-=1
     if d==0:end_p=k;break
   arg_strs=split_top_args(line[start_p+1:end_p])
   for values in (itertools.product(*(v for k,v in loops)) if loops else [()]):
    env={k:v for (k,_),v in zip(loops,values)}
    env['z']=env.get('zone',0)*14
    if not all(bool(eval_expr(c,env)) for c in conds):continue
    model=eval_expr(arg_strs[0],env)
    if model not in models:continue
    pos_arg=arg_strs[5 if fn=='Item' else 4 if fn=='Clue' else 1]
    pos=np.array(eval_expr(pos_arg,env),float)
    yaw=float(eval_expr(arg_strs[2],env)) if fn in ('Model','Prop') and len(arg_strs)>2 else 0.0
    scale=float(eval_expr(arg_strs[3],env)) if fn in ('Model','Prop') and len(arg_strs)>3 else 1.0
    v=models[model]*scale;a=math.radians(yaw);rot=np.array([[math.cos(a),0,math.sin(a)],[0,1,0],[-math.sin(a),0,math.cos(a)]])
    v=v@rot.T
    if pos[1]==0:pos[1]-=v[:,1].min()
    if model=='Document':continue
    lo=v.min(0)+pos;hi=v.max(0)+pos
    placements.append({'model':model,'position':pos.tolist(),'min':lo.tolist(),'max':hi.tolist(),'line':line_no,'file':filename,'env':env})
  if closes>opens:
   for _ in range(closes-opens):
    if ctx_stack:ctx_stack.pop()
   if not ctx_stack and in_build:in_build=False

floor_names={'Bed','Desk','Chair','Crib','Wheelchair','Cabinet','XRay','Generator','MotherBody','ChildBody','IVStand','Trolley','OxygenTank','Locker','Bench','Sink','BioBin','PrivacyScreen','MorgueDrawers','BedsideTable','MedicineShelf','ArchiveBoxes','JanitorCart','Stretcher','VinylSofa','WashBasin','Incubator','AutopsyTable'}
for p in placements:
 if p['model'] in floor_names:assert abs(p['min'][1])<.0001,('Floating floor prop',p)
issues=[]
for i,a in enumerate(placements):
 for b in placements[i+1:]:
  if a['model'] not in floor_names or b['model'] not in floor_names:continue
  overlap=np.minimum(a['max'],b['max'])-np.maximum(a['min'],b['min'])
  if np.all(overlap>.025):issues.append({'a':a,'b':b,'overlap':overlap.tolist()})
(root/'Docs/LAYOUT_AUDIT.json').write_text(json.dumps({'placements':placements,'floor_aabb_overlaps':issues},indent=2))
print('PASS:',len(placements),'actual model placements parsed; floor-standing props aligned with floor.')
print('Conservative floor-furniture AABB overlap candidates:',len(issues))
for issue in issues:
 a,b=issue['a'],issue['b'];print(a['model'],a['position'],'vs',b['model'],b['position'])

assert not issues,"Overlapping floor furniture: inspect Docs/LAYOUT_AUDIT.json"
mother=next(p for p in placements if p['model']=='MotherBody')
for other in placements:
 if other is mother or other['model'] not in floor_names:continue
 lo=np.array(mother['min'])+[-14.6,0,0];hi=np.array(mother['max'])+[-14.6,0,0]
 assert not np.all(np.minimum(hi,other['max'])-np.maximum(lo,other['min'])>.025),('Final bed relocation overlaps',other)
for model,position,support in [('Monitor',(7.4,.83,49.5),.83),('Defibrillator',(6.3,.877,32),.885),('DeskLamp',(-8.9,1.044,20.2),1.06),('MedicalTray',(7.4,.320625,49.5),.33),('MedicalTray',(1.9,.82,101),.83),('Recorder',(-5.6,.885,102.6),.885)]:
 item=next(p for p in placements if p['model']==model and np.allclose(p['position'],position))
 assert abs(item['min'][1]-support)<.0001,(model,item['min'][1],support)
print('PASS: final moved-bed layout and six corrected tabletop/shelf contacts.')
