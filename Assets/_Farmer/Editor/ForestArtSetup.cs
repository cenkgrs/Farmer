using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Farmer.Editor
{
    public static class ForestArtSetup
    {
        public static void ApplyAndBuild()
        {
            const string art="Assets/_Farmer/Art";
            const string output="Assets/_Farmer/Resources/ExplorationArt";
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var interiorPath=art+"/Materials/chest_interior.mat";
            var interior=AssetDatabase.LoadAssetAtPath<Material>(interiorPath);
            if(interior==null){interior=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(interior,interiorPath);}
            interior.SetColor("_BaseColor",new Color(.22f,.105f,.04f));interior.SetFloat("_Smoothness",.1f);
            EditorUtility.SetDirty(interior);
            foreach(var name in new[]{"tree_oak","treasure_chest","wild_plant"})
            {
                var path=$"{art}/Models/{name}.fbx";
                var importer=(ModelImporter)AssetImporter.GetAtPath(path);
                importer.importAnimation=false;importer.importCameras=false;importer.importLights=false;
                importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.isReadable=false;
                importer.SaveAndReimport();ArtImportSetup.CreateMaterial(name);
                var material=AssetDatabase.LoadAssetAtPath<Material>($"{art}/Materials/{name}.mat");
                if(name!="treasure_chest")
                {
                    // Organic surfaces in the delivered plant had metallic gold highlights.
                    material.SetTexture("_MetallicGlossMap",null);material.DisableKeyword("_METALLICSPECGLOSSMAP");
                    material.SetFloat("_Metallic",0);material.SetFloat("_Smoothness",.18f);EditorUtility.SetDirty(material);
                }
                var root=new GameObject(name);
                GameObject model=null;
                try
                {
                    model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));
                    foreach(var filter in model.GetComponentsInChildren<MeshFilter>())
                    {
                        var piece=filter.gameObject;
                        // Unpack before reparenting model children into stable named prefab parts.
                        if(PrefabUtility.IsPartOfPrefabInstance(model))PrefabUtility.UnpackPrefabInstance(model,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
                        piece.transform.SetParent(root.transform,true);
                        var materials=Enumerable.Repeat(material,filter.sharedMesh.subMeshCount).ToArray();
                        if(materials.Length>1)
                        {
                            int innerSlot=name=="tree_oak"
                                ? Enumerable.Range(0,materials.Length).OrderBy(i=>filter.sharedMesh.GetIndexCount(i)).First() : 1;
                            materials[innerSlot]=interior;
                            Debug.Log($"FARMER_SURFACE {name}/{piece.name} innerSlot={innerSlot} counts={string.Join(",",Enumerable.Range(0,materials.Length).Select(i=>filter.sharedMesh.GetIndexCount(i)))}");
                        }
                        piece.GetComponent<Renderer>().sharedMaterials=materials;
                    }
                    if(name=="treasure_chest")
                    {
                        var lid=root.transform.Find("Lid");if(lid==null)throw new InvalidOperationException("Missing chest lid.");
                        var hinge=new GameObject("Lid hinge").transform;hinge.SetParent(root.transform,false);hinge.localPosition=new Vector3(0,.386f,-.32f);
                        lid.SetParent(hinge,true);
                    }
                    if(name=="tree_oak"&&(root.transform.Find("Trunk")==null||root.transform.Find("Canopy")==null))throw new InvalidOperationException("Missing separated tree parts.");
                    PrefabUtility.SaveAsPrefabAsset(root,$"{output}/{name}.prefab");
                    Debug.Log($"FARMER_FOREST_ART {name} renderers={root.GetComponentsInChildren<Renderer>().Length}");
                }
                finally{if(model!=null)UnityEngine.Object.DestroyImmediate(model);UnityEngine.Object.DestroyImmediate(root);}
            }
            AssetDatabase.SaveAssets();ProjectSetup.BuildLinux();
        }
    }
}
