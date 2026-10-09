using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Farmer.Editor
{
    public static class MapEditingTools
    {
        [MenuItem("Farmer/Harita/Model paletini seç")]
        public static void Palette(){Selection.activeObject=AssetDatabase.LoadAssetAtPath<Object>("Assets/_Farmer/Resources/ValleyArt");EditorGUIUtility.PingObject(Selection.activeObject);}
        [MenuItem("Farmer/Harita/Köy meydanına odaklan")]
        public static void FocusVillage(){if(SceneView.lastActiveSceneView!=null)SceneView.lastActiveSceneView.Frame(new Bounds(new Vector3(-55,1,96),new Vector3(32,10,32)),false);}
        // One-time migration: changes only the road renderers, never regenerates scenery.
        public static void ApplyPavingAndBuild()
        {
            AssetDatabase.Refresh();
            var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/_Farmer/Art/Textures/village_paving.png");
            importer.wrapMode=TextureWrapMode.Repeat;importer.maxTextureSize=1024;importer.mipmapEnabled=true;importer.SaveAndReimport();
            var material=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Farmer/Art/Materials/village_paving.mat");
            if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,"Assets/_Farmer/Art/Materials/village_paving.mat");}
            material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Farmer/Art/Textures/village_paving.png"));material.SetColor("_BaseColor",Color.white);material.SetFloat("_Smoothness",.08f);EditorUtility.SetDirty(material);
            var scene=EditorSceneManager.OpenScene(ProjectSetup.ScenePath);
            var root=GameObject.Find("Valley First Slice");
            foreach(var renderer in root.GetComponentsInChildren<Renderer>())
            {
                // Pave the village approach and square; keep the countryside trail earthy.
                if(renderer.name!="Valley path"||renderer.transform.position.z<80)continue;
                renderer.transform.position=new Vector3(renderer.transform.position.x,renderer.transform.localScale.x>10?.015f:.009f,renderer.transform.position.z);
                renderer.sharedMaterial=material;if(renderer.GetComponent<PavingTiling>()==null)renderer.gameObject.AddComponent<PavingTiling>();
                EditorUtility.SetDirty(renderer);
            }
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            ProjectSetup.BuildLinux();
        }
    }
}
