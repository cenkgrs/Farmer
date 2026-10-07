using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Farmer.Editor
{
    public static class BuildingPrototypeSetup
    {
        public static void ApplyAndBuild()
        {
            var scene = EditorSceneManager.OpenScene(ProjectSetup.ScenePath, OpenSceneMode.Additive);
            try
            {
                if (scene.isDirty) throw new InvalidOperationException("Save Farm scene first.");
                var game = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<FarmGame>()).Single();
                if (game.GetComponent<BuildController>() == null)
                {
                    const string dataPath = "Assets/_Farmer/Data/WoodBlock.asset";
                    if (AssetDatabase.LoadAssetAtPath<BuildDefinition>(dataPath) != null) throw new InvalidOperationException("Build data exists without scene wiring; inspect before replacing.");
                    var wood = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Farmer/Materials/Wood.mat");
                    var trim = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Farmer/Materials/WoodTrim.mat");
                    var root = new GameObject("wood_block");
                    try
                    {
                        root.AddComponent<BoxCollider>().size = Vector3.one * .98f;
                        for (int i = 0; i < 4; i++)
                            Part(root.transform, new Vector3(0, -.36f + i * .24f, 0), new Vector3(.96f, .225f, .96f), i % 2 == 0 ? wood : trim);
                        for (int side = -1; side <= 1; side += 2)
                        {
                            Part(root.transform, new Vector3(side * .32f, 0, -.485f), new Vector3(.09f, .96f, .025f), wood);
                            Part(root.transform, new Vector3(side * .32f, 0, .485f), new Vector3(.09f, .96f, .025f), wood);
                        }
                        var prefab = PrefabUtility.SaveAsPrefabAsset(root, "Assets/_Farmer/Prefabs/wood_block.prefab");
                        var definition = ScriptableObject.CreateInstance<BuildDefinition>(); definition.prefab = prefab;
                        AssetDatabase.CreateAsset(definition, dataPath);
                        var serialized = new SerializedObject(game);
                        var pieces = serialized.FindProperty("buildPieces"); pieces.arraySize = 1; pieces.GetArrayElementAtIndex(0).objectReferenceValue = definition;
                        serialized.ApplyModifiedPropertiesWithoutUndo();
                        game.gameObject.AddComponent<BuildController>().Configure(game, AssetDatabase.LoadAssetAtPath<Material>("Assets/_Farmer/Materials/SelectionLine.mat"));
                        // A small label makes the first construction area discoverable without opening menus.
                        var sign = new GameObject("Construction Sign"); UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(sign, scene);
                        sign.transform.position = new Vector3(6, .15f, -7.35f);
                        sign.transform.rotation = CameraRotation(scene);
                        var text = sign.AddComponent<TextMesh>(); text.text = "İNŞA ALANI · 4"; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                        text.fontSize = 48; text.characterSize = .045f; text.anchor = TextAnchor.MiddleCenter; text.color = new Color(.97f, .89f, .66f);
                        sign.GetComponent<MeshRenderer>().sharedMaterial = text.font.material;
                        EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
                    }
                    finally { UnityEngine.Object.DestroyImmediate(root); }
                }
            }
            finally { EditorSceneManager.CloseScene(scene, true); }
            Debug.Log("FARMER_BUILDING_SETUP_OK");
            ProjectSetup.BuildLinux();
        }
        private static Quaternion CameraRotation(UnityEngine.SceneManagement.Scene scene) => scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Camera>()).Single().transform.rotation;
        private static void Part(Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube); UnityEngine.Object.DestroyImmediate(obj.GetComponent<Collider>());
            obj.transform.SetParent(parent, false); obj.transform.localPosition = position; obj.transform.localScale = scale;
            obj.GetComponent<Renderer>().sharedMaterial = material;
        }
    }
}
