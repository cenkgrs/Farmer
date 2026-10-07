using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Farmer.Editor
{
    public static class WorldPrototypeSetup
    {
        public static void ApplyAndBuild()
        {
            var scene = EditorSceneManager.OpenScene(ProjectSetup.ScenePath,OpenSceneMode.Additive);
            try
            {
                var roots = scene.GetRootGameObjects();
                var game = roots.SelectMany(r=>r.GetComponentsInChildren<FarmGame>()).Single();
                if (game.GetComponent<DayNightCycle>() == null)
                {
                    roots.Single(r=>r.name=="Ground").AddComponent<WorldGround>();
                    var sign = roots.FirstOrDefault(r=>r.name=="Construction Sign");
                    if (sign != null) Object.DestroyImmediate(sign);
                    game.Camp.gameObject.AddComponent<Bed>();
                    foreach (var text in game.Camp.GetComponentsInChildren<TextMesh>()) text.text="YATAK";
                    var root = new GameObject("bed");
                    var wood = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Farmer/Materials/Wood.mat");
                    var cloth = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Farmer/Materials/CampCloth.mat");
                    var cream = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Farmer/Materials/TurnipCream.mat");
                    try
                    {
                        Part(root.transform,new Vector3(0,-.27f,.5f),new Vector3(.92f,.22f,1.92f),wood);
                        Part(root.transform,new Vector3(0,-.09f,.5f),new Vector3(.86f,.2f,1.82f),cloth);
                        Part(root.transform,new Vector3(0,.045f,1.15f),new Vector3(.76f,.12f,.4f),cream);
                        var collider = root.AddComponent<BoxCollider>(); collider.center=new Vector3(0,-.16f,.5f); collider.size=new Vector3(.94f,.68f,1.94f);
                        root.AddComponent<Bed>();
                        var prefab = PrefabUtility.SaveAsPrefabAsset(root,"Assets/_Farmer/Prefabs/bed.prefab");
                        var definition = AssetDatabase.LoadAssetAtPath<BuildDefinition>("Assets/_Farmer/Data/Bed.asset") ?? ScriptableObject.CreateInstance<BuildDefinition>(); definition.id="bed"; definition.displayName="Yatak"; definition.woodCost=8; definition.isBed=true; definition.prefab=prefab;
                        if (!AssetDatabase.Contains(definition)) AssetDatabase.CreateAsset(definition,"Assets/_Farmer/Data/Bed.asset");
                        EditorUtility.SetDirty(definition);
                        var serialized = new SerializedObject(game); var pieces=serialized.FindProperty("buildPieces"); pieces.arraySize=2; pieces.GetArrayElementAtIndex(1).objectReferenceValue=definition; serialized.ApplyModifiedPropertiesWithoutUndo();
                    }
                    finally { Object.DestroyImmediate(root); }
                    game.gameObject.AddComponent<DayNightCycle>().Configure(game,roots.Where(r=>r!=null).SelectMany(r=>r.GetComponentsInChildren<Light>()).Single(l=>l.type==LightType.Directional));
                    EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
                }
            }
            finally { EditorSceneManager.CloseScene(scene,true); }
            ProjectSetup.BuildLinux();
        }
        public static void ApplyHudAndBuild()
        {
            var scene=EditorSceneManager.OpenScene(ProjectSetup.ScenePath,OpenSceneMode.Additive);
            try
            {
                foreach(var text in scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<TextMesh>(true)).ToArray())
                    if(text.text=="PAZAR" || text.text=="YATAK" || text.text=="KAMP") Object.DestroyImmediate(text.gameObject);
                EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            }
            finally { EditorSceneManager.CloseScene(scene,true); }
            ProjectSetup.BuildLinux();
        }
        private static void Part(Transform parent,Vector3 position,Vector3 scale,Material material)
        {
            var p=GameObject.CreatePrimitive(PrimitiveType.Cube); Object.DestroyImmediate(p.GetComponent<Collider>());
            p.transform.SetParent(parent,false); p.transform.localPosition=position; p.transform.localScale=scale; p.GetComponent<Renderer>().sharedMaterial=material;
        }
    }
}
