using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Farmer.Editor
{
    public static class ExplorationArtSetup
    {
        public static void ApplyAndBuild()
        {
            const string art="Assets/_Farmer/Art";
            const string output="Assets/_Farmer/Resources/ExplorationArt";
            Directory.CreateDirectory(output);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            foreach(var name in new[]{"axe","tree_stump"})
            {
                var path=$"{art}/Models/{name}.fbx";
                var importer=(ModelImporter)AssetImporter.GetAtPath(path);
                importer.importAnimation=false;importer.importCameras=false;importer.importLights=false;
                importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.isReadable=true;
                importer.SaveAndReimport();ArtImportSetup.CreateMaterial(name);
                var root=new GameObject(name);
                try
                {
                    var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));
                    model.transform.SetParent(root.transform,false);
                    foreach(var r in model.GetComponentsInChildren<Renderer>())r.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>($"{art}/Materials/{name}.mat");
                    if(name=="axe")
                    {
                        var filter=model.GetComponentInChildren<MeshFilter>();
                        var points=filter.sharedMesh.vertices.Select(v=>filter.transform.TransformPoint(v)).ToArray();
                        var gripPoints=points.Where(v=>v.y>.10f&&v.y<.18f).ToArray();
                        if(gripPoints.Length==0)throw new InvalidOperationException("Axe grip has no geometry.");
                        var grip=gripPoints.Aggregate(Vector3.zero,(sum,v)=>sum+v)/gripPoints.Length;
                        var head=points.Where(v=>v.y>.5f).Aggregate(Vector3.zero,(sum,v)=>sum+v)/points.Count(v=>v.y>.5f);
                        // Put the blade ahead of the hand in the vertical chopping plane.
                        var rotation=Quaternion.Euler(0,head.x>grip.x?-90:90,0);
                        model.transform.localPosition=-(rotation*grip);
                        model.transform.localRotation=rotation*model.transform.localRotation;
                        Debug.Log($"FARMER_AXE_GRIP {grip} head={head}");
                    }
                    PrefabUtility.SaveAsPrefabAsset(root,$"{output}/{name}.prefab");
                    var renderers=root.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;
                    foreach(var r in renderers)bounds.Encapsulate(r.bounds);
                    Debug.Log($"FARMER_EXPLORATION_ART {name} bounds={bounds}");
                }
                finally{UnityEngine.Object.DestroyImmediate(root);}
            }
            AssetDatabase.SaveAssets();ProjectSetup.BuildLinux();
        }
    }
}
