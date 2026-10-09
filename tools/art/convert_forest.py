"""Blender: split delivered forest props locally; preserve original GLBs and UVs."""
import bpy,bmesh,json,numpy as np
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
report={}
for name,height in [('tree_oak',3.0),('treasure_chest',.643),('wild_plant',.65)]:
 bpy.ops.wm.read_factory_settings(use_empty=True)
 bpy.ops.import_scene.gltf(filepath=str(ROOT/'ArtSource/forest_v01'/f'{name}.glb'))
 obj=next(o for o in bpy.context.scene.objects if o.type=='MESH');matrix=obj.matrix_world.copy();obj.parent=None;obj.matrix_world=matrix
 bpy.context.view_layer.objects.active=obj;obj.select_set(True);bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
 pts=np.array([v.co[:] for v in obj.data.vertices]);lo=pts.min(0);hi=pts.max(0);factor=height/(hi[2]-lo[2]);pts=(pts-np.array([(lo[0]+hi[0])/2,(lo[1]+hi[1])/2,lo[2]]))*factor
 for v,p in zip(obj.data.vertices,pts):v.co=p
 obj.data.update();objects=[]
 if name=='tree_oak':
  # A horizontal cut keeps bark intact: color segmentation also removed painted knots.
  for part,upper in [('Trunk',False),('Canopy',True)]:
   piece=obj.copy();piece.data=obj.data.copy();piece.name=part;bpy.context.collection.objects.link(piece)
   piece.data.materials.append(bpy.data.materials.new("CutSurface"))
   bm=bmesh.new();bm.from_mesh(piece.data);bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=.00001)
   bmesh.ops.bisect_plane(bm,geom=list(bm.verts)+list(bm.edges)+list(bm.faces),dist=.00001,plane_co=(0,0,.9),plane_no=(0,0,1),clear_inner=upper,clear_outer=not upper)
   caps=bmesh.ops.holes_fill(bm,edges=[e for e in bm.edges if e.is_boundary],sides=0)
   for f in caps['faces']:f.material_index=1
   bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(piece.data);bm.free();objects.append(piece)
  bpy.data.objects.remove(obj,do_unlink=True)
 elif name=='treasure_chest':
  # Measured horizontal lid seam; keep the lower shell open, cap the lid underside.
  seam=.386
  inner=bpy.data.materials.new('ChestInterior');inner.diffuse_color=(.19,.09,.035,1)
  for part,upper in [('Body',False),('Lid',True)]:
   piece=obj.copy();piece.data=obj.data.copy();piece.name=part;bpy.context.collection.objects.link(piece)
   piece.data.materials.append(bpy.data.materials.new("CutSurface"))
   bm=bmesh.new();bm.from_mesh(piece.data)
   bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=.00001)
   bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces))
   bmesh.ops.bisect_plane(bm,geom=list(bm.verts)+list(bm.edges)+list(bm.faces),dist=.00001,plane_co=(0,0,seam),plane_no=(0,0,1),clear_inner=upper,clear_outer=not upper)
   if upper:
    edges=[e for e in bm.edges if e.is_boundary and all(abs(v.co.z-seam)<.0001 for v in e.verts)]
    result=bmesh.ops.holes_fill(bm,edges=edges,sides=0)
    for f in result['faces']:f.material_index=1
   bm.to_mesh(piece.data);bm.free();indices=[f.material_index for f in piece.data.polygons];piece.data.materials.clear();piece.data.materials.append(bpy.data.materials.new('Exterior'));piece.data.materials.append(inner)
   for f,index in zip(piece.data.polygons,indices):f.material_index=index
   if not upper:
    solid=piece.modifiers.new('Interior wall thickness','SOLIDIFY');solid.thickness=.015;solid.offset=-1;solid.material_offset=1;solid.material_offset_rim=1
   objects.append(piece)
  bpy.data.objects.remove(obj,do_unlink=True)
  report['chest_seam_m']=seam
 else:
  obj.name='Plant';objects=[obj]
 if name!='treasure_chest':
  for o in objects:
   indices=[f.material_index for f in o.data.polygons];o.data.materials.clear()
   if name=='tree_oak':
    o.data.materials.append(bpy.data.materials.new('Bark'));o.data.materials.append(bpy.data.materials.new('BranchEnds'))
    for f,index in zip(o.data.polygons,indices):f.material_index=index
 bpy.ops.object.select_all(action='DESELECT')
 for o in objects:o.select_set(True)
 bpy.context.view_layer.objects.active=objects[0]
 bpy.ops.export_scene.fbx(filepath=str(ROOT/'Assets/_Farmer/Art/Models'/f'{name}.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_anim=False,add_leaf_bones=False,use_mesh_modifiers=True)
 counts={}
 for o in objects:
  evaluated=o.evaluated_get(bpy.context.evaluated_depsgraph_get());mesh=evaluated.to_mesh();mesh.calc_loop_triangles();counts[o.name]=len(mesh.loop_triangles);evaluated.to_mesh_clear()
 report[name]={'height_m':height,'parts_triangles':counts,'size_blender_xyz':np.ptp(pts,axis=0).tolist()}
(ROOT/'ArtSource/forest_v01/conversion.json').write_text(json.dumps(report,indent=2)+'\n')
