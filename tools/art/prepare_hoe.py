"""Extract delivered exploration textures without modifying source GLBs."""
import io, json, struct, hashlib
from pathlib import Path
from PIL import Image, ImageOps
ROOT=Path(__file__).resolve().parents[2]
report={}
for name in ['hoe']:
    source=ROOT/'ArtSource/hoe_v01'/f'{name}.glb'
    data=source.read_bytes(); magic,version,total=struct.unpack_from('<III',data)
    assert magic==0x46546C67 and version==2 and total==len(data)
    offset=12; chunks={}
    while offset<total:
        length,kind=struct.unpack_from('<II',data,offset);chunks[kind]=data[offset+8:offset+8+length];offset+=8+length
    model=json.loads(chunks[0x4E4F534A]);binary=chunks[0x004E4942]
    assert len(model['meshes'])==1 and len(model['materials'])==1
    mat=model['materials'][0];pbr=mat['pbrMetallicRoughness'];sizes=[]
    def texture(info):
        img=model['images'][model['textures'][info['index']]['source']];view=model['bufferViews'][img['bufferView']];start=view.get('byteOffset',0)
        image=Image.open(io.BytesIO(binary[start:start+view['byteLength']]));sizes.append(image.size)
        return image.convert('RGB').resize((1024,1024),Image.Resampling.LANCZOS)
    dest=ROOT/'Assets/_Farmer/Art/Textures'/name;dest.mkdir(parents=True,exist_ok=True)
    texture(pbr['baseColorTexture']).save(dest/f'{name}_basecolor.png')
    texture(mat['normalTexture']).save(dest/f'{name}_normal.png')
    rm=texture(pbr['metallicRoughnessTexture']);zero=Image.new('L',rm.size,0)
    Image.merge('RGBA',(rm.getchannel('B'),zero,zero,ImageOps.invert(rm.getchannel('G')))).save(dest/f'{name}_metallic_smoothness.png')
    report[name]={'sha256':hashlib.sha256(data).hexdigest(),'source_texture_sizes':sizes,'working_texture_size':1024,'base_color_factor':pbr.get('baseColorFactor'),'triangles':sum(model['accessors'][p['indices']]['count']//3 for p in model['meshes'][0]['primitives'])}
(ROOT/'ArtSource/hoe_v01/source_manifest.json').write_text(json.dumps(report,indent=2)+'\n')
