"""Rebuild only Sebastian, without rewriting any scenery or audio assets."""
from pathlib import Path
import ast,sys
sys.path.insert(0,str(Path(__file__).resolve().parent))
source=Path(__file__).with_name('create_assets.py')
tree=ast.parse(source.read_text())
nodes=[]
for node in tree.body:
 nodes.append(node)
 if isinstance(node,ast.Expr) and isinstance(node.value,ast.Call) and isinstance(node.value.func,ast.Name) and node.value.func.id=='install':break
scope={'__file__':str(source)}
exec(compile(ast.Module(body=nodes,type_ignores=[]),str(source),'exec'),scope)
from sebastian_model import build
mesh=scope['Mesh']('Sebastian');build(mesh);mesh.save()
