# Valley delivery v01

Eight user GLBs preserved unchanged. Names were verified visually rather than inferred from filenames: `medieval cottage` is the shop facade, `wooden bench` is the bridge, `tree` is the pine. Source filename mapping: village_shop→medieval cottage, ruin_arch→stone arch, village_well→stone well, cliff_module→stone cliff, granite_boulder→rock boulder, pine_tree→tree, wood_bridge→wooden bench, meadow_bush→bush. Original filenames end in ` 3d model.glb`.

SHA-256, original texture resolution and triangle counts: `source_manifest.json`. Runtime geometry budgets/dimensions: `conversion.json`. Original embedded 4K textures preserved; Unity working textures 1K. Local `prepare_valley.py` extracts textures, Blender `convert_valley.py` normalizes dimensions and decimates static hard-surface meshes. UVs are retained; normal maps are not rebaked. Pine and bush keep source geometry after the first aggressive decimation damaged foliage silhouettes. The supplied bush is the leafy variant; the alternative simplified v02 reference is also saved, the reference actually used for the user generation was not verified.

Runtime triangles: shop 12,000; arch 4,000; well 4,000; cliff 4,000; boulder 2,500; pine 16,328; bridge 6,000; bush 7,637. No LOD chain yet; foliage exceeds the original low-poly target. All eight local renders were inspected. Closed shop exterior only; no accessible interior or NPC rig. Bridge prefab is imported, but no river crossing is placed in this first slice.

Unity `Farmer.Editor.ValleyArtSetup.ApplyAndBuild` builds ValleyArt prefabs/materials and authors the first village/road scene. Later builds use `ProjectSetup.BuildLinux`. The authoring method replaces only its `Valley First Slice` scenery root; do not rerun after hand-editing that root without preserving those edits.

No paid Tripo generation/API call. User files are the source of the models.
