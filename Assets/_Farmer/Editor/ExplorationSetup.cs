using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Farmer.Editor
{
    public static class ExplorationSetup
    {
        public static void ApplyAndBuild()
        {
            var scene=EditorSceneManager.OpenScene(ProjectSetup.ScenePath,OpenSceneMode.Additive);
            try
            {
                var all=scene.GetRootGameObjects();var game=all.SelectMany(r=>r.GetComponentsInChildren<FarmGame>()).Single();
                var ground=all.SelectMany(r=>r.GetComponentsInChildren<WorldGround>()).Single();ground.transform.localScale=new Vector3(56,.5f,56);
                var controller=game.GetComponent<ExplorationController>()??game.gameObject.AddComponent<ExplorationController>();controller.Configure(ground.GetComponent<Renderer>().sharedMaterial);
                var motor=game.Player.GetComponent<PlayerMotor>();var serialized=new SerializedObject(motor);serialized.FindProperty("walkableHalfExtent").floatValue=27.3f;serialized.ApplyModifiedPropertiesWithoutUndo();
                var camera=all.SelectMany(r=>r.GetComponentsInChildren<Camera>()).Single();var follow=camera.GetComponent<ExplorationCamera>()??camera.gameObject.AddComponent<ExplorationCamera>();follow.Configure(game.Player);
                EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            }
            finally{EditorSceneManager.CloseScene(scene,true);}
            ProjectSetup.BuildLinux();
        }
    }
}
