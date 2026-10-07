"""blender -b --python tools/art/convert_props.py; only static GLBs are converted."""
import bpy, json
from pathlib import Path
from mathutils import Vector
ROOT = Path(__file__).resolve().parents[2]
report = {}
for name, height in [('watering_can', .42), ('sickle', .5), ('market_stall', 2.4)]:
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=str(ROOT / 'ArtSource/tripo_v01' / (name + '.glb')))
    mesh = next(o for o in bpy.context.scene.objects if o.type == 'MESH')
    matrix = mesh.matrix_world.copy(); mesh.parent = None; mesh.matrix_world = matrix
    bpy.context.view_layer.objects.active = mesh; mesh.select_set(True)
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    points = [v.co.copy() for v in mesh.data.vertices]
    low = Vector([min(p[i] for p in points) for i in range(3)])
    high = Vector([max(p[i] for p in points) for i in range(3)])
    center = Vector(((low.x + high.x)/2, (low.y + high.y)/2, low.z))
    factor = height / (high.z-low.z)
    for v in mesh.data.vertices: v.co = (v.co - center) * factor
    mesh.name = name; mesh.data.name = name + '_mesh'
    # Materials are authored explicitly for URP in Unity, using the extracted textures.
    mesh.data.materials.clear()
    bpy.ops.object.select_all(action='DESELECT'); mesh.select_set(True)
    dest = ROOT / 'Assets/_Farmer/Art/Models' / (name + '.fbx'); dest.parent.mkdir(parents=True, exist_ok=True)
    bpy.ops.export_scene.fbx(filepath=str(dest), use_selection=True, object_types={'MESH'}, axis_forward='-Z', axis_up='Y', apply_unit_scale=True, bake_anim=False, add_leaf_bones=False, use_mesh_modifiers=True)
    mesh.data.calc_loop_triangles()
    report[name] = {'triangles':len(mesh.data.loop_triangles), 'vertices':len(mesh.data.vertices), 'height_m':height, 'source_scale_factor':factor}
(ROOT/'ArtSource/tripo_v01/prop_conversion.json').write_text(json.dumps(report,indent=2)+'\n')
