using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Farmer.Editor
{
    public static class RoofPrototypeSetup
    {
        public static void ApplyAndBuild()
        {
            var scene=EditorSceneManager.OpenScene(ProjectSetup.ScenePath,OpenSceneMode.Additive);
            try
            {
                var game=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<FarmGame>()).Single();
                const string path="Assets/_Farmer/Data/wood_roof.asset";
                var definition=AssetDatabase.LoadAssetAtPath<BuildDefinition>(path);
                if(definition==null)
                {
                    var wood=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Farmer/Materials/WoodTrim.mat");var root=new GameObject("wood_roof");
                    for(int i=0;i<5;i++)
                    {
                        var plank=GameObject.CreatePrimitive(PrimitiveType.Cube);Object.DestroyImmediate(plank.GetComponent<Collider>());
                        plank.transform.SetParent(root.transform,false);plank.transform.localPosition=new Vector3(-.4f+i*.2f,-.44f,0);plank.transform.localScale=new Vector3(.196f,.12f,1);plank.GetComponent<Renderer>().sharedMaterial=wood;
                    }
                    var box=root.AddComponent<BoxCollider>();box.center=new Vector3(0,-.44f,0);box.size=new Vector3(1,.12f,1);
                    var prefab=PrefabUtility.SaveAsPrefabAsset(root,"Assets/_Farmer/Prefabs/wood_roof.prefab");Object.DestroyImmediate(root);
                    definition=ScriptableObject.CreateInstance<BuildDefinition>();definition.id="wood_roof";definition.displayName="Ahşap çatı";definition.woodCost=2;definition.placement=BuildPlacement.Roof;definition.prefab=prefab;AssetDatabase.CreateAsset(definition,path);
                }
                var so=new SerializedObject(game);so.FindProperty("startingMoney").intValue=50;var pieces=so.FindProperty("buildPieces");pieces.arraySize=6;pieces.GetArrayElementAtIndex(5).objectReferenceValue=definition;so.ApplyModifiedPropertiesWithoutUndo();
                game.Camp.gameObject.SetActive(false);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            }
            finally{EditorSceneManager.CloseScene(scene,true);}
            ProjectSetup.BuildLinux();
        }
    }
}
