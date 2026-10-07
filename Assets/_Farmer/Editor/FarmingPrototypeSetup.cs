using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Farmer.Editor
{
    public static class FarmingPrototypeSetup
    {
        [MenuItem("Farmer/Add Farming Prototype")]
        public static void UpgradeScene()
        {
            var scene = EditorSceneManager.OpenScene(ProjectSetup.ScenePath, OpenSceneMode.Additive);
            var previous = SceneManager.GetActiveScene();
            try
            {
                if (scene.isDirty || scene.GetRootGameObjects().Any(r => r.GetComponentInChildren<FarmGame>() != null))
                    throw new InvalidOperationException("Farm already contains farming or unsaved edits. Refusing to replace it.");
                SceneManager.SetActiveScene(scene);
                var selection = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<FarmSelection>()).Single();
                var player = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<PlayerMotor>()).Single();
                var camera = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Camera>()).Single();
                var oldHud = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<PrototypeHud>()).Single();
                UnityEngine.Object.DestroyImmediate(oldHud.gameObject);
                Directory.CreateDirectory("Assets/_Farmer/Data");
                Directory.CreateDirectory("Assets/_Farmer/Prefabs/Crops");
                AssetDatabase.Refresh();
                var leaf = Material("TurnipLeaf", new Color(0.28f, 0.56f, 0.13f));
                var pink = Material("TurnipPink", new Color(0.81f, 0.24f, 0.41f));
                var cream = Material("TurnipCream", new Color(0.97f, 0.88f, 0.65f));
                var wood = Material("Wood", new Color(0.45f, 0.28f, 0.14f));
                var red = Material("MarketAwning", new Color(0.74f, 0.31f, 0.22f));
                var cloth = Material("CampCloth", new Color(0.22f, 0.46f, 0.51f));
                const string cropPath = "Assets/_Farmer/Data/Turnip.asset";
                if (File.Exists(cropPath)) throw new InvalidOperationException("Turnip data exists; refusing to replace it.");
                var crop = ScriptableObject.CreateInstance<CropDefinition>();
                for (int stage = 0; stage < 4; stage++)
                {
                    var root = new GameObject("Turnip Stage " + stage);
                    if (stage >= 2)
                    {
                        float size = stage == 2 ? 0.27f : 0.49f;
                        Part(root.transform, "Bulb", PrimitiveType.Sphere, new Vector3(0, size * 0.38f, 0), Vector3.one * size, pink);
                        Part(root.transform, "Root", PrimitiveType.Sphere, new Vector3(0, size * 0.13f, 0), new Vector3(size * 0.72f, size * 0.6f, size * 0.72f), cream);
                    }
                    float length = 0.16f + stage * 0.085f;
                    float baseHeight = stage < 2 ? 0.06f : stage == 2 ? 0.2f : 0.36f;
                    for (int n = 0; n < (stage < 2 ? 2 : 4); n++)
                    {
                        var blade = Part(root.transform, "Leaf", PrimitiveType.Sphere, new Vector3(0, baseHeight + length * 0.35f, 0),
                            new Vector3(0.075f + stage * 0.025f, length, 0.05f + stage * 0.012f), leaf);
                        blade.transform.localRotation = Quaternion.Euler(0, n * 90f, 38f);
                    }
                    string path = $"Assets/_Farmer/Prefabs/Crops/TurnipStage{stage}.prefab";
                    root.transform.localScale = Vector3.one * 1.5f;
                    if (File.Exists(path)) throw new InvalidOperationException("Crop prefab exists: " + path);
                    crop.growthStages[stage] = PrefabUtility.SaveAsPrefabAsset(root, path);
                    UnityEngine.Object.DestroyImmediate(root);
                }
                AssetDatabase.CreateAsset(crop, cropPath);

                var system = new GameObject("Farming");
                var market = new GameObject("Market"); market.transform.SetParent(system.transform);
                market.transform.position = new Vector3(-5.6f, 0, 2.2f);
                Part(market.transform, "Counter", PrimitiveType.Cube, new Vector3(0, 0.55f, 0), new Vector3(1.8f, 1.1f, 0.8f), wood, true);
                for (int x = -1; x <= 1; x += 2)
                    Part(market.transform, "Post", PrimitiveType.Cube, new Vector3(x * 0.85f, 1.35f, 0.28f), new Vector3(0.12f, 2.7f, 0.12f), wood);
                for (int stripe = 0; stripe < 6; stripe++)
                    Part(market.transform, "Awning", PrimitiveType.Cube, new Vector3(-0.875f + stripe * 0.35f, 2.35f, 0),
                        new Vector3(0.35f, 0.12f, 1.25f), stripe % 2 == 0 ? red : cream);
                for (int n = 0; n < 3; n++)
                    Part(market.transform, "Seed Sack", PrimitiveType.Sphere, new Vector3(-0.55f + n * 0.5f, 1.27f, 0), new Vector3(0.32f, 0.38f, 0.32f), cream);
                Sign(market.transform, "PAZAR", new Vector3(0, 2.8f, 0), camera);

                var camp = new GameObject("Camp"); camp.transform.SetParent(system.transform);
                camp.transform.position = new Vector3(-5.6f, 0, -3.4f);
                Part(camp.transform, "Bedroll", PrimitiveType.Cube, new Vector3(0, 0.1f, 0), new Vector3(1.05f, 0.2f, 1.7f), cloth);
                Part(camp.transform, "Pillow", PrimitiveType.Cube, new Vector3(0, 0.24f, 0.56f), new Vector3(0.85f, 0.13f, 0.35f), cream);
                Part(camp.transform, "Camp Marker", PrimitiveType.Cube, new Vector3(-0.7f, 0.55f, 0.6f), new Vector3(0.09f, 1.1f, 0.09f), wood);
                Sign(camp.transform, "KAMP", new Vector3(-0.7f, 1.35f, 0.6f), camera);
                player.transform.position = new Vector3(-5.6f, 0.1f, 0.4f);

                var game = system.AddComponent<FarmGame>();
                game.Configure(new[] { crop }, selection, player.transform, market.transform, camp.transform);
                var soil = new Renderer[36];
                for (int z = 0; z < 6; z++) for (int x = 0; x < 6; x++)
                    soil[x + z * 6] = selection.transform.Find($"Soil {x},{z}").GetComponent<Renderer>();
                system.AddComponent<FarmPresentation>().Configure(game, soil);
                system.AddComponent<FarmHud>().Configure(game);
                selection.SetHudPanels(Array.Empty<RectTransform>());
                if (!EditorSceneManager.SaveScene(scene, ProjectSetup.ScenePath)) throw new IOException("Could not save farming scene.");
                AssetDatabase.SaveAssets();
                Debug.Log("FARMER_FARMING_SETUP_OK");
            }
            finally { SceneManager.SetActiveScene(previous); EditorSceneManager.CloseScene(scene, true); }
        }

        private static Material Material(string name, Color color)
        {
            string path = "Assets/_Farmer/Materials/" + name + ".mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path); if (existing != null) return existing;
            var result = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name };
            result.SetColor("_BaseColor", color); result.SetFloat("_Smoothness", 0.07f);
            AssetDatabase.CreateAsset(result, path); return result;
        }
        private static GameObject Part(Transform parent, string name, PrimitiveType type, Vector3 position, Vector3 scale, Material material, bool solid = false)
        {
            var obj = GameObject.CreatePrimitive(type); obj.name = name; obj.transform.SetParent(parent, false);
            obj.transform.localPosition = position; obj.transform.localScale = scale;
            obj.GetComponent<Renderer>().sharedMaterial = material;
            if (!solid) UnityEngine.Object.DestroyImmediate(obj.GetComponent<Collider>());
            return obj;
        }
        private static void Sign(Transform parent, string title, Vector3 position, Camera camera)
        {
            var obj = new GameObject(title); obj.transform.SetParent(parent, false); obj.transform.localPosition = position;
            obj.transform.rotation = camera.transform.rotation;
            var text = obj.AddComponent<TextMesh>(); text.text = title; text.fontSize = 64; text.characterSize = 0.06f;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            obj.GetComponent<MeshRenderer>().sharedMaterial = text.font.material;
            text.anchor = TextAnchor.MiddleCenter; text.color = new Color(1f, 0.94f, 0.74f);
        }

        public static void PolishPresentation()
        {
            var scene = EditorSceneManager.OpenScene(ProjectSetup.ScenePath, OpenSceneMode.Additive);
            try
            {
                if (scene.isDirty) throw new InvalidOperationException("Save scene edits first.");
                foreach (var text in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<TextMesh>()))
                {
                    text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    text.GetComponent<MeshRenderer>().sharedMaterial = text.font.material;
                }
                for (int stage = 0; stage < 4; stage++)
                {
                    string path = $"Assets/_Farmer/Prefabs/Crops/TurnipStage{stage}.prefab";
                    var root = PrefabUtility.LoadPrefabContents(path);
                    try { root.transform.localScale = Vector3.one * 1.5f; PrefabUtility.SaveAsPrefabAsset(root, path); }
                    finally { PrefabUtility.UnloadPrefabContents(root); }
                }
                EditorSceneManager.SaveScene(scene);
                Debug.Log("FARMER_PRESENTATION_OK");
            }
            finally { EditorSceneManager.CloseScene(scene, true); }
        }
    }
}
