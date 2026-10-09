using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Farmer.Editor
{
    public static class ValleyArtSetup
    {
        public static void ApplyAndBuild()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            string[] names={"village_shop","ruin_arch","village_well","cliff_module","granite_boulder","pine_tree","wood_bridge","meadow_bush"};
            Directory.CreateDirectory("Assets/_Farmer/Resources/ValleyArt");
            foreach(var name in names)
            {
                string path=$"Assets/_Farmer/Art/Models/{name}.fbx";
                var importer=(ModelImporter)AssetImporter.GetAtPath(path);
                importer.importAnimation=false;importer.importCameras=false;importer.importLights=false;
                importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.SaveAndReimport();
                ArtImportSetup.CreateMaterial(name);
                var material=AssetDatabase.LoadAssetAtPath<Material>($"Assets/_Farmer/Art/Materials/{name}.mat");
                material.SetTexture("_MetallicGlossMap",null);material.DisableKeyword("_METALLICSPECGLOSSMAP");material.SetFloat("_Metallic",0);material.SetFloat("_Smoothness",.12f);
                material.SetFloat("_Cull",0);EditorUtility.SetDirty(material);
                var root=new GameObject(name);
                var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));model.transform.SetParent(root.transform,false);
                foreach(var renderer in model.GetComponentsInChildren<Renderer>())renderer.sharedMaterial=material;
                if(name=="pine_tree") {var box=root.AddComponent<BoxCollider>();box.center=new Vector3(0,1,0);box.size=new Vector3(.65f,2,.65f);}
                else if(name!="meadow_bush")
                    foreach(var filter in model.GetComponentsInChildren<MeshFilter>()){var collider=filter.gameObject.AddComponent<MeshCollider>();collider.sharedMesh=filter.sharedMesh;}
                PrefabUtility.SaveAsPrefabAsset(root,$"Assets/_Farmer/Resources/ValleyArt/{name}.prefab");UnityEngine.Object.DestroyImmediate(root);
            }
            var scene=EditorSceneManager.OpenScene(ProjectSetup.ScenePath);
            var game=UnityEngine.Object.FindFirstObjectByType<FarmGame>();
            var old=GameObject.Find("Valley First Slice");if(old!=null)UnityEngine.Object.DestroyImmediate(old);
            var ground=UnityEngine.Object.FindFirstObjectByType<WorldGround>();ground.transform.localScale=new Vector3(256,.5f,256);
            var motor=new SerializedObject(game.Player.GetComponent<PlayerMotor>());motor.FindProperty("walkableHalfExtent").floatValue=127.3f;motor.ApplyModifiedPropertiesWithoutUndo();
            var rootWorld=new GameObject("Valley First Slice");
            var grass=ground.GetComponent<Renderer>().sharedMaterial;
            var pathMaterial=new Material(grass);pathMaterial.SetColor("_BaseColor",new Color(.56f,.43f,.25f));
            string matPath="Assets/_Farmer/Art/Materials/valley_path.mat";
            var savedMat=AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if(savedMat==null){AssetDatabase.CreateAsset(pathMaterial,matPath);savedMat=pathMaterial;}else{EditorUtility.CopySerialized(pathMaterial,savedMat);UnityEngine.Object.DestroyImmediate(pathMaterial);}
            GameObject Prop(string name,Vector3 position,float yaw=0,float scale=1)
            {
                var obj=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/_Farmer/Resources/ValleyArt/{name}.prefab"));
                obj.transform.SetParent(rootWorld.transform,false);obj.transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));obj.transform.localScale=Vector3.one*scale;
                obj.AddComponent<ValleyScenery>();return obj;
            }
            void PathSegment(Vector3 a,Vector3 b,float width)
            {
                var obj=GameObject.CreatePrimitive(PrimitiveType.Cube);obj.name="Valley path";obj.transform.SetParent(rootWorld.transform,false);
                UnityEngine.Object.DestroyImmediate(obj.GetComponent<Collider>());obj.transform.position=(a+b)*.5f+Vector3.up*.003f;
                obj.transform.rotation=Quaternion.LookRotation(b-a);obj.transform.localScale=new Vector3(width,.004f,Vector3.Distance(a,b)+.3f);obj.GetComponent<Renderer>().sharedMaterial=savedMat;
            }
            var points=new[]{new Vector3(-5,0,8),new Vector3(-12,0,32),new Vector3(-30,0,48),new Vector3(-34,0,68),new Vector3(-55,0,86),new Vector3(-55,0,96)};
            for(int i=1;i<points.Length;i++)PathSegment(points[i-1],points[i],3);
            PathSegment(new Vector3(-63,0,96),new Vector3(-47,0,96),16);
            Prop("village_shop",new Vector3(-64,0,101),90);
            Prop("village_shop",new Vector3(-46,0,101),180);
            Prop("village_shop",new Vector3(-55,0,108),180);
            Prop("village_well",new Vector3(-55,0,96));
            Prop("ruin_arch",new Vector3(30,0,78),20);
            game.Market.position=new Vector3(-60,game.Market.position.y,88);
            // Leave the complete legacy 56m world clear; preserve existing farms and resource nodes.
            var random=new System.Random(10926);
            for(int i=0;i<72;i++)
            {
                float x=-85+(float)random.NextDouble()*130,z=34+(float)random.NextDouble()*80;
                var p=new Vector3(x,0,z);bool road=false;
                for(int j=1;j<points.Length;j++)
                {
                    var d=points[j]-points[j-1];float t=Mathf.Clamp01(Vector3.Dot(p-points[j-1],d)/d.sqrMagnitude);
                    if(Vector3.Distance(p,points[j-1]+d*t)<5)road=true;
                }
                if(road||Vector3.Distance(p,new Vector3(-55,0,96))<17||Vector3.Distance(p,new Vector3(30,0,78))<5)continue;
                string name=i%4==0?"granite_boulder":i%4==1?"meadow_bush":"pine_tree";
                Prop(name,p,(float)random.NextDouble()*360,.75f+(float)random.NextDouble()*.45f);
            }
            for(int i=0;i<7;i++)Prop("cliff_module",new Vector3(39+i*4,0,109),180,1.3f);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Debug.Log("FARMER_VALLEY_SETUP_OK");ProjectSetup.BuildLinux();
        }
    }
}
