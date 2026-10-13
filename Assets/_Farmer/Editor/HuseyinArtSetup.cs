using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Farmer.Editor
{
    public static class HuseyinArtSetup
    {
        public static void Apply()
        {
            UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            const string source="Assets/_Farmer/Art/Models/huseyin.fbx";
            var importer=(ModelImporter)AssetImporter.GetAtPath(source);importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.importCameras=false;importer.importLights=false;importer.SaveAndReimport();
            ArtImportSetup.CreateMaterial("huseyin");var material=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Farmer/Art/Materials/huseyin.mat");
            var root=new GameObject("Huseyin");var model=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(source),root.transform);model.name="Delivered Huseyin";
            var renderers=model.GetComponentsInChildren<Renderer>();if(renderers.Length==0)throw new System.Exception("Huseyin has no geometry.");
            foreach(var renderer in renderers)renderer.sharedMaterials=renderer.sharedMaterials.Select(m=>material).ToArray();
            Bounds BoundsOf(){var b=renderers[0].bounds;foreach(var r in renderers)b.Encapsulate(r.bounds);return b;}
            var bounds=BoundsOf();model.transform.localScale*=1.7f/bounds.size.y;bounds=BoundsOf();model.transform.position-=new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
            var meshCount=model.GetComponentsInChildren<MeshFilter>().Sum(m=>m.sharedMesh.triangles.Length/3)+model.GetComponentsInChildren<SkinnedMeshRenderer>().Sum(m=>m.sharedMesh.triangles.Length/3);
            var clips=AssetDatabase.LoadAllAssetsAtPath(source).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview")).ToArray();
            Debug.Log($"FARMER_HUSEYIN triangles={meshCount} size={BoundsOf().size} skinned={model.GetComponentsInChildren<SkinnedMeshRenderer>().Length} clips={clips.Length}");
            PrefabUtility.SaveAsPrefabAsset(root,"Assets/_Farmer/Resources/ExplorationArt/huseyin.prefab");Object.DestroyImmediate(root);AssetDatabase.SaveAssets();
        }
    }
}
