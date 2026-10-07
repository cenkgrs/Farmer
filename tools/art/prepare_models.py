"""Run with Python + Pillow from the repo root. Extract Tripo textures; keep source GLBs intact."""
import io, json, struct
from pathlib import Path
from PIL import Image, ImageOps

ROOT = Path(__file__).resolve().parents[2]
for name, size in [('farmer', 2048), ('watering_can', 1024), ('sickle', 1024), ('market_stall', 2048)]:
    source = ROOT / 'ArtSource' / 'tripo_v01' / (name + '.glb')
    data = source.read_bytes()
    magic, version, total = struct.unpack_from('<III', data)
    assert magic == 0x46546C67 and version == 2 and total == len(data)
    offset, chunks = 12, {}
    while offset < total:
        length, kind = struct.unpack_from('<II', data, offset)
        chunks[kind] = data[offset + 8:offset + 8 + length]
        offset += length + 8
    model = json.loads(chunks[0x4E4F534A]); binary = chunks[0x004E4942]
    material = model['materials'][0]; pbr = material['pbrMetallicRoughness']
    dest = ROOT / 'Assets/_Farmer/Art/Textures' / name
    dest.mkdir(parents=True, exist_ok=True)
    def read_texture(info):
        image = model['images'][model['textures'][info['index']]['source']]
        view = model['bufferViews'][image['bufferView']]
        start = view.get('byteOffset', 0)
        return Image.open(io.BytesIO(binary[start:start + view['byteLength']])).convert('RGB').resize((size, size), Image.Resampling.LANCZOS)
    read_texture(pbr['baseColorTexture']).save(dest / (name + '_basecolor.png'))
    read_texture(material['normalTexture']).save(dest / (name + '_normal.png'))
    rm = read_texture(pbr['metallicRoughnessTexture'])
    metallic, smoothness = rm.getchannel('B'), ImageOps.invert(rm.getchannel('G'))
    empty = Image.new('L', rm.size, 0)
    Image.merge('RGBA', (metallic, empty, empty, smoothness)).save(dest / (name + '_metallic_smoothness.png'))
    print(name, 'textures extracted', size)
