# Delivered furniture — 9 October 2026

The user's four GLBs are preserved unchanged. `source_manifest.json` records SHA-256, texture sizes and source triangle counts. Conversion is local Blender/Pillow; no Tripo API tasks or credits were used.

| Asset | Runtime dimensions | Triangles |
| --- | --- | --- |
| home_chest | 0.90 × 0.55 × 0.55 m | 9,763 |
| home_table | 0.80 wide × 1.60 deep × 0.78 m high | 4,742 |
| home_chair | 0.50 × 0.50 × 0.90 m | 4,584 |
| home_lantern | 0.427 × 0.620 × 1.65 m | 6,578 |

Scripts: `tools/art/prepare_furniture.py` extracts 1K basecolor/normal/metallic-smoothness from the original embedded textures. `tools/art/convert_furniture.py` normalizes each mesh and exports FBX with UVs preserved. Table rotates into a two-cell footprint. Original GLBs remain the source of full-resolution textures.

`FurnitureArtSetup.ApplyAndBuild` creates stable prefabs/data/materials and appends the four definitions to the existing Farm scene without regenerating it. Ordinary later builds use `ProjectSetup.BuildLinux`.

The delivered chest is a single closed mesh. Storage opens its UI; this revision does not animate its lid or invent interior geometry. Furniture has simple box colliders. The table/chair are placeable furnishings; character sitting animation is not included. The lamp has a warm point light that fades in at 18:00–19:00 and out at 06:00–07:00. Point-light shadows are disabled in this prototype, so wall light leakage is a known limitation. Glass panels are part of the shared texture, not separate emissive geometry.

Triangle counts exceed the sketch guide targets. These four props were retained at delivered detail for this small prototype; no LOD or large settlement performance claim is made. Inspect actual Unity screenshots and checks in YAZILIMCI.md before treating the integration as verified.
