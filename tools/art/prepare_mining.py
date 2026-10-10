"""Convert these static, identity-node GLBs to Unity mesh data; keep originals intact.
Requires numpy and Pillow. No external model generation or Unity package required.
"""
import io,json,struct,hashlib
from pathlib import Path
import numpy as np
from PIL import Image,ImageOps
ROOT=Path(__file__).resolve().parents[2]
report={}
for name,target in [('pickaxe',.80),('stone_outcrop',1.20)]:
    source=ROOT/'ArtSource/mining_v01'/f'{name}.glb'
    data=source.read_bytes();magic,version,total=struct.unpack_from('<III',data)
    assert magic==0x46546C67 and version==2 and total==len(data)
    offset=12;chunks={}
    while offset<total:
        length,kind=struct.unpack_from('<II',data,offset);chunks[kind]=data[offset+8:offset+8+length];offset+=8+length
    model=json.loads(chunks[0x4E4F534A]);binary=chunks[0x004E4942]
    assert len(model['meshes'])==len(model['materials'])==1
    assert all(not any(k in n for k in ['matrix','translation','rotation','scale','skin']) for n in model['nodes'])
    assert len(model['meshes'][0]['primitives'])==1
    primitive=model['meshes'][0]['primitives'][0];assert primitive.get('mode',4)==4
    def accessor(index):
        a=model['accessors'][index];v=model['bufferViews'][a['bufferView']]
        assert 'byteStride' not in v and 'sparse' not in a
        dtype={5126:'<f4',5125:'<u4',5123:'<u2'}[a['componentType']];size={'VEC3':3,'VEC2':2,'SCALAR':1}[a['type']]
        return np.frombuffer(binary,dtype=dtype,count=a['count']*size,offset=v.get('byteOffset',0)+a.get('byteOffset',0)).reshape(-1,size).copy()
    points=accessor(primitive['attributes']['POSITION']);normals=accessor(primitive['attributes']['NORMAL']);uv=accessor(primitive['attributes']['TEXCOORD_0']);triangles=accessor(primitive['indices']).reshape(-1,3)
    assert np.isfinite(points).all() and triangles.max()<len(points)
    low=points.min(0);high=points.max(0);scale=target/((high-low)[1] if name=='pickaxe' else max((high-low)[0],(high-low)[2]))
    points=(points-np.array([(low[0]+high[0])/2,low[1],(low[2]+high[2])/2]))*scale
    # glTF right-handed +Y up -> Unity left-handed. glTF images use upper-left UV origin.
    points[:,2]*=-1;normals[:,2]*=-1;uv[:,1]=1-uv[:,1];triangles=triangles[:,[0,2,1]]
    payload={'positions':points.flatten().tolist(),'normals':normals.flatten().tolist(),'uv':uv.flatten().tolist(),'triangles':triangles.flatten().tolist()}
    (source.parent/f'{name}_mesh.json').write_text(json.dumps(payload,separators=(',',':'))+'\n')
    mat=model['materials'][0];pbr=mat['pbrMetallicRoughness'];sizes=[]
    def texture(info):
        img=model['images'][model['textures'][info['index']]['source']];view=model['bufferViews'][img['bufferView']];start=view.get('byteOffset',0)
        im=Image.open(io.BytesIO(binary[start:start+view['byteLength']]));sizes.append(im.size)
        return im.convert('RGB').resize((1024,1024),Image.Resampling.LANCZOS)
    dest=ROOT/'Assets/_Farmer/Art/Textures'/name;dest.mkdir(parents=True,exist_ok=True)
    texture(pbr['baseColorTexture']).save(dest/f'{name}_basecolor.png')
    texture(mat['normalTexture']).save(dest/f'{name}_normal.png')
    rm=texture(pbr['metallicRoughnessTexture']);zero=Image.new('L',rm.size,0)
    Image.merge('RGBA',(rm.getchannel('B'),zero,zero,ImageOps.invert(rm.getchannel('G')))).save(dest/f'{name}_metallic_smoothness.png')
    report[name]={'sha256':hashlib.sha256(data).hexdigest(),'triangles':len(triangles),'vertices':len(points),'size_unity_xyz':np.ptp(points,axis=0).tolist(),'source_texture_sizes':sizes,'working_texture_size':1024,'base_color_factor':pbr.get('baseColorFactor')}
(ROOT/'ArtSource/mining_v01/source_manifest.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps(report,indent=2))
