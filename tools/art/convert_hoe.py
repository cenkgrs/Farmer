"""Local Blender conversion of delivered axe/stump, no remote generation."""
import bpy,json,numpy as np
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2];report={}
for name,height in [('hoe',1.05)]:
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=str(ROOT/'ArtSource/hoe_v01'/f'{name}.glb'))
    mesh=next(o for o in bpy.context.scene.objects if o.type=='MESH')
    matrix=mesh.matrix_world.copy();mesh.parent=None;mesh.matrix_world=matrix
    bpy.context.view_layer.objects.active=mesh;mesh.select_set(True)
    bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
    points=np.array([v.co[:] for v in mesh.data.vertices])
    if name=='hoe':
        center=points.mean(axis=0);_,axes=np.linalg.eigh(np.cov((points-center).T))
        long=axes[:,2];wide=axes[:,1]
        proj=(points-center)@long
        # The blade end is wider than the handle butt.
        span=proj.max()-proj.min()
        if np.ptp(((points-center)@wide)[proj<proj.min()+span*.25])>np.ptp(((points-center)@wide)[proj>proj.max()-span*.25]):long=-long
        thin=np.cross(long,wide);points=np.column_stack(((points-center)@wide,(points-center)@thin,(points-center)@long))
    low=points.min(axis=0);high=points.max(axis=0);factor=height/(high[2]-low[2])
    points=(points-np.array([(low[0]+high[0])/2,(low[1]+high[1])/2,low[2]]))*factor
    for v,p in zip(mesh.data.vertices,points):v.co=p
    mesh.name=name;mesh.data.name=name+'_mesh';mesh.data.materials.clear()
    bpy.ops.object.select_all(action='DESELECT');mesh.select_set(True)
    dest=ROOT/'Assets/_Farmer/Art/Models'/f'{name}.fbx'
    bpy.ops.export_scene.fbx(filepath=str(dest),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_anim=False,add_leaf_bones=False)
    mesh.data.calc_loop_triangles()
    report[name]={'triangles':len(mesh.data.loop_triangles),'vertices':len(mesh.data.vertices),'size_blender_xyz':np.ptp(points,axis=0).tolist(),'height_m':height}
(ROOT/'ArtSource/hoe_v01/conversion.json').write_text(json.dumps(report,indent=2)+'\n')
