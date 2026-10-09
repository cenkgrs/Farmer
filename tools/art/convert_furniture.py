"""Normalize delivered furniture locally; preserve source GLBs and UVs."""
import bpy,json,math,numpy as np
from mathutils import Vector
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
report={}
for name,size in [('home_chest',(.9,.55,.55)),('home_table',(.8,1.6,.78)),('home_chair',(.5,.5,.9)),('home_lantern',None)]:
 bpy.ops.wm.read_factory_settings(use_empty=True)
 bpy.ops.import_scene.gltf(filepath=str(ROOT/'ArtSource/furniture_v01'/f'{name}.glb'))
 obj=next(o for o in bpy.context.scene.objects if o.type=='MESH');matrix=obj.matrix_world.copy();obj.parent=None;obj.matrix_world=matrix
 bpy.context.view_layer.objects.active=obj;obj.select_set(True);bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
 if name=='home_table':
  obj.rotation_euler.z=math.pi/2;bpy.ops.object.transform_apply(location=False,rotation=True,scale=False)
 pts=np.array([v.co[:] for v in obj.data.vertices]);lo=pts.min(0);hi=pts.max(0)
 scale=np.array(size)/(hi-lo) if size else 1.65/(hi[2]-lo[2])
 pts=(pts-np.array([(lo[0]+hi[0])/2,(lo[1]+hi[1])/2,lo[2]]))*scale
 for v,p in zip(obj.data.vertices,pts):v.co=p
 obj.data.update();obj.name='Body'
 # Keep the closed chest intact for reliable geometry; storage interaction does not require a rig.
 bpy.ops.export_scene.fbx(filepath=str(ROOT/'Assets/_Farmer/Art/Models'/f'{name}.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_anim=False,add_leaf_bones=False)
 obj.data.calc_loop_triangles();report[name]={'triangles':len(obj.data.loop_triangles),'size_blender_xyz':np.ptp(pts,axis=0).tolist()}
 scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=12;scene.render.resolution_x=600;scene.render.resolution_y=600;scene.render.resolution_percentage=100
 scene.world=bpy.data.worlds.new("Studio");scene.world.color=(.6,.6,.6);scene.render.film_transparent=True
 target=Vector((0,0,float(np.ptp(pts,axis=0)[2])*.5));radius=max(np.ptp(pts,axis=0))*2.5
 bpy.ops.object.camera_add(location=target+Vector((1,-1,.8))*radius);cam=bpy.context.object;cam.rotation_euler=(target-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=max(np.ptp(pts,axis=0))*1.55;scene.camera=cam
 for pos,power in [((3,-4,5),500),((-3,-1,3),250)]:
  bpy.ops.object.light_add(type='AREA',location=pos);bpy.context.object.data.energy=power;bpy.context.object.data.shape='DISK';bpy.context.object.data.size=4
 scene.render.filepath=str(ROOT/'builds/QA'/f'{name}-source.png');bpy.ops.render.render(write_still=True)
(ROOT/'ArtSource/furniture_v01/conversion.json').write_text(json.dumps(report,indent=2)+'\n')
