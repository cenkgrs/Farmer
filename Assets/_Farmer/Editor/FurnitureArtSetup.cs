using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Farmer.Editor
{
    public static class FurnitureArtSetup
    {
        public static void ApplyAndBuild()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            string[] names={"home_chest","home_table","home_chair","home_lantern"};
            string[] labels={"Depolama sandığı","Ahşap masa","Ahşap sandalye","Ayaklı lamba"};
            int[] prices={80,60,25,45};
            var definitions=new BuildDefinition[4];
            for(int i=0;i<names.Length;i++)
            {
                string name=names[i],path=$"Assets/_Farmer/Art/Models/{name}.fbx";
                var importer=(ModelImporter)AssetImporter.GetAtPath(path);
                importer.importAnimation=false;importer.importCameras=false;importer.importLights=false;importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.SaveAndReimport();
                ArtImportSetup.CreateMaterial(name);
                var material=AssetDatabase.LoadAssetAtPath<Material>($"Assets/_Farmer/Art/Materials/{name}.mat");
                material.SetTexture("_MetallicGlossMap",null);material.DisableKeyword("_METALLICSPECGLOSSMAP");material.SetFloat("_Metallic",0);material.SetFloat("_Smoothness",.18f);EditorUtility.SetDirty(material);
                var root=new GameObject(name);
                var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));
                model.transform.SetParent(root.transform,false);model.transform.localPosition=new Vector3(0,-.5f,name=="home_table"?.5f:0);
                foreach(var r in model.GetComponentsInChildren<Renderer>())r.sharedMaterial=material;
                var bounds=new Bounds();bool first=true;
                foreach(var r in model.GetComponentsInChildren<Renderer>()){if(first){bounds=r.bounds;first=false;}else bounds.Encapsulate(r.bounds);}
                var box=root.AddComponent<BoxCollider>();box.center=bounds.center;box.size=bounds.size;
                if(name=="home_lantern")
                {
                    var bulb=new GameObject("Warm lamp light");bulb.transform.SetParent(root.transform,false);bulb.transform.localPosition=new Vector3(0,.75f,.15f);
                    var light=bulb.AddComponent<Light>();light.type=LightType.Point;light.color=new Color(1f,.72f,.35f);light.range=5;light.intensity=2.5f;light.shadows=LightShadows.None;
                    root.AddComponent<FurnitureLighting>();
                }
                var prefab=PrefabUtility.SaveAsPrefabAsset(root,$"Assets/_Farmer/Prefabs/{name}.prefab");UnityEngine.Object.DestroyImmediate(root);
                string dataPath=$"Assets/_Farmer/Data/{name}.asset";
                var def=AssetDatabase.LoadAssetAtPath<BuildDefinition>(dataPath);
                if(def==null){def=ScriptableObject.CreateInstance<BuildDefinition>();AssetDatabase.CreateAsset(def,dataPath);}
                def.id=name;def.displayName=labels[i];def.price=prices[i];def.woodCost=1;def.footprintLength=name=="home_table"?2:1;def.prefab=prefab;EditorUtility.SetDirty(def);definitions[i]=def;
                string iconPath=$"Assets/_Farmer/Resources/ConstructionArt/{name}.png";
                var icon=AssetImporter.GetAtPath(iconPath) as TextureImporter;
                if(icon!=null){icon.textureType=TextureImporterType.Default;icon.alphaIsTransparency=true;icon.maxTextureSize=512;icon.mipmapEnabled=false;icon.SaveAndReimport();}
            }
            var scene=EditorSceneManager.OpenScene("Assets/_Farmer/Scenes/Farm.unity");
            var game=UnityEngine.Object.FindFirstObjectByType<FarmGame>();var serialized=new SerializedObject(game);var pieces=serialized.FindProperty("buildPieces");
            var all=game.BuildPieces.Where(d=>!names.Contains(d.id)).Concat(definitions).ToArray();pieces.arraySize=all.Length;
            for(int i=0;i<all.Length;i++)pieces.GetArrayElementAtIndex(i).objectReferenceValue=all[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();Debug.Log("FARMER_FURNITURE_SETUP_OK");ProjectSetup.BuildLinux();
        }
    }
}
