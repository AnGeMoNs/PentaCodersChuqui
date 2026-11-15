"""Regression: rebuilding the main atlas must preserve other importers byte for byte."""
from pathlib import Path
import shutil, subprocess, sys, tempfile

root = Path(__file__).resolve().parents[1]
with tempfile.TemporaryDirectory(prefix='ecos-atlas-') as tmp:
    stage = Path(tmp)
    art = stage / 'SourceArt'
    art.mkdir()
    shutil.copytree(root / 'SourceArt/TextureSources', art / 'TextureSources')
    shutil.copy2(root / 'SourceArt/build_materials.py', art / 'build_materials.py')
    output = stage / 'Assets/Ecos/Resources/PBR'
    output.mkdir(parents=True)
    expected = {}
    for source in (root / 'Assets/Ecos/Resources/PBR').glob('Props*'):
        shutil.copy2(source, output / source.name)
        expected[source.name] = source.read_bytes()
    subprocess.run([sys.executable, str(art / 'build_materials.py')], check=True)
    for name, data in expected.items():
        assert (output / name).read_bytes() == data, f'Rebuild changed {name}'
print('PASS: rebuilding the main atlas preserves additional textures and their import settings.')
