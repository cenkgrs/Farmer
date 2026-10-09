# Delivered hoe

User file `wooden axe 3d model.glb` was visually verified as a hoe and preserved unchanged as `hoe.glb`. SHA-256 and texture report: `source_manifest.json`. 6,074 triangles, one mesh/material, original embedded 4K textures preserved; Unity textures are 1K. Above the 1–3K sketch target, no LOD/decimation in this delivery.

Local pipeline: `python3 tools/art/prepare_hoe.py`, Blender `tools/art/convert_hoe.py`, Unity `Farmer.Editor.HoeArtSetup.ApplyAndBuild`. Model long axis is normalized to 1.05 m; grip sampled at 0.24–0.32 m from the handle butt. The working head faces forward in socket space. FBX UVs and source geometry retained. Later builds use `ProjectSetup.BuildLinux`; committed prefab/material/FBX/meta files are sufficient on another computer.

The hoe replaces temporary cylinder/cube art only. Existing calibrated hand pose, farming input, starting inventory and save rules remain unchanged. No Tripo API call or paid generation used.
