using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Farmer.Editor
{
    public static class ArtSceneSetup
    {
        private const string Root = ArtImportSetup.Root;
        public static void Apply()
        {
            if (File.Exists(Root + "/Animation/farmer.controller"))
                throw new InvalidOperationException("Art already exists; refusing to replace it.");
            Directory.CreateDirectory(Root + "/Prefabs"); Directory.CreateDirectory(Root + "/Animation"); AssetDatabase.Refresh();
            var can = Tool("watering_can", .386f, .418f, Quaternion.Euler(0, -90, 0));
            var sickle = Tool("sickle", .05f, .16f, Quaternion.identity);
            var controller = Controller();
            var scene = EditorSceneManager.OpenScene(ProjectSetup.ScenePath, OpenSceneMode.Additive);
            try
            {
                if (scene.isDirty) throw new InvalidOperationException("Save unsaved Farm changes first.");
                var roots = scene.GetRootGameObjects();
                var motor = roots.SelectMany(r => r.GetComponentsInChildren<PlayerMotor>()).Single();
                var game = roots.SelectMany(r => r.GetComponentsInChildren<FarmGame>()).Single();
                var presentation = game.GetComponent<FarmPresentation>();
                var camera = roots.SelectMany(r => r.GetComponentsInChildren<Camera>()).Single();
                if (motor.Visual.name != "Placeholder Visual") throw new InvalidOperationException("Art already applied; refusing to replace the character.");
                var visual = new GameObject("Farmer Visual").transform;
                visual.SetParent(motor.transform, false); visual.localRotation = motor.Visual.localRotation;
                var actor = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Models/farmer_idle.fbx"), scene);
                actor.name = "Farmer Animated"; actor.transform.SetParent(visual, false);
                foreach (var renderer in actor.GetComponentsInChildren<Renderer>()) renderer.sharedMaterial = Material("farmer");
                var animator = actor.GetComponent<Animator>(); animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false; animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animator.Rebind(); animator.Update(0);
                var skin = actor.GetComponentInChildren<SkinnedMeshRenderer>();
                var baked = new Mesh(); skin.BakeMesh(baked);
                var points = baked.vertices.Select(v => actor.transform.InverseTransformPoint(skin.transform.TransformPoint(v))).ToArray();
                float minY = points.Min(p => p.y), maxY = points.Max(p => p.y);
                float scale = 1.85f / (maxY - minY);
                actor.transform.localScale = Vector3.one * scale;
                actor.transform.localPosition = new Vector3(0, -minY * scale, 0);
                UnityEngine.Object.DestroyImmediate(baked);
                var hand = animator.GetBoneTransform(HumanBodyBones.RightHand);
                Quaternion grip = Quaternion.Inverse(visual.rotation) * hand.rotation;
                var socket = new GameObject("Tool Socket").transform; socket.SetParent(hand, false);
                socket.rotation = visual.rotation;
                socket.localScale = Vector3.one / hand.lossyScale.x;
                var driver = actor.AddComponent<FarmerAnimator>(); driver.Configure(motor, game, socket);
                UnityEngine.Object.DestroyImmediate(motor.Visual.gameObject);
                motor.Configure(camera, visual);
                var serialized = new SerializedObject(presentation);
                serialized.FindProperty("heldItemSocket").objectReferenceValue = socket;
                serialized.FindProperty("wateringCanPrefab").objectReferenceValue = can;
                serialized.FindProperty("sicklePrefab").objectReferenceValue = sickle;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                var market = game.Market;
                foreach (Transform child in market.Cast<Transform>().ToArray())
                    if (child.name != "PAZAR") UnityEngine.Object.DestroyImmediate(child.gameObject);
                var stall = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Models/market_stall.fbx"), scene);
                stall.name = "Market Stall Art"; stall.transform.SetParent(market, false);
                foreach (var renderer in stall.GetComponentsInChildren<Renderer>()) renderer.sharedMaterial = Material("market_stall");
                var collider = market.gameObject.AddComponent<BoxCollider>();
                collider.center = new Vector3(0, .6f, 0); collider.size = new Vector3(2.5f, 1.2f, 1.3f);
                Debug.Log($"FARMER_CHARACTER_FIT: min={minY} max={maxY} scale={scale} hand={hand.position} grip={grip.eulerAngles}");
                if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Art scene save failed.");
                AssetDatabase.SaveAssets();
                Debug.Log("FARMER_ART_SCENE_OK");
            }
            finally { EditorSceneManager.CloseScene(scene, true); }
        }
        public static void RefineGripAndBuild() { RefineGrip(); ProjectSetup.BuildLinux(); }
        public static void RefineGrip()
        {
            Tool("watering_can", .386f, .418f, Quaternion.Euler(0, -90, 0));
            Tool("sickle", .05f, .16f, Quaternion.identity);
            var scene = EditorSceneManager.OpenScene(ProjectSetup.ScenePath, OpenSceneMode.Additive);
            try
            {
                if (scene.isDirty) throw new InvalidOperationException("Save Farm first.");
                var game = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<FarmGame>()).Single();
                var motor = game.Player.GetComponent<PlayerMotor>();
                var driver = game.Player.GetComponentInChildren<FarmerAnimator>();
                var socket = game.Player.GetComponentsInChildren<Transform>().Single(t => t.name == "Tool Socket");
                driver.Configure(motor, game, socket);
                EditorUtility.SetDirty(driver);
                EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
                Debug.Log("FARMER_GRIP_SETUP_OK");
            }
            finally { EditorSceneManager.CloseScene(scene, true); }
        }
        private static Material Material(string name) => AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/" + name + ".mat");
        private static GameObject Tool(string name, float gripMin, float gripMax, Quaternion rotation)
        {
            var root = new GameObject(name);
            try
            {
                var model = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Models/" + name + ".fbx"));
                model.transform.SetParent(root.transform, false);
                var filter = model.GetComponentInChildren<MeshFilter>();
                var gripPoints = filter.sharedMesh.vertices.Select(v => filter.transform.TransformPoint(v))
                    .Where(v => v.y > gripMin && v.y < gripMax).ToArray();
                if (gripPoints.Length == 0) throw new InvalidOperationException("No vertices in tool grip region.");
                Vector3 grip = gripPoints.Aggregate(Vector3.zero, (sum, v) => sum + v) / gripPoints.Length;
                model.transform.localPosition = -(rotation * grip);
                Debug.Log($"FARMER_TOOL_GRIP: {name} native={grip} offset={model.transform.localPosition}");
                // Preserve the FBX axis conversion; apply art yaw in the wrapper space.
                model.transform.localRotation = rotation * model.transform.localRotation;
                foreach (var renderer in model.GetComponentsInChildren<Renderer>()) renderer.sharedMaterial = Material(name);
                return PrefabUtility.SaveAsPrefabAsset(root, Root + "/Prefabs/" + name + ".prefab");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }
        private static AnimatorController Controller()
        {
            string path = Root + "/Animation/farmer.controller";
            if (File.Exists(path)) throw new InvalidOperationException("Animator controller already exists.");
            var result = AnimatorController.CreateAnimatorControllerAtPath(path);
            result.AddParameter("MoveSpeed", AnimatorControllerParameterType.Float);
            var layers = result.layers; layers[0].iKPass = true; result.layers = layers;
            var state = result.layers[0].stateMachine.AddState("Locomotion");
            var blend = new BlendTree { name = "Idle Walk", blendParameter = "MoveSpeed", blendType = BlendTreeType.Simple1D, useAutomaticThresholds = false };
            AssetDatabase.AddObjectToAsset(blend, result);
            blend.AddChild(Clip("farmer_idle"), 0); blend.AddChild(Clip("farmer_walk"), 1);
            var children = blend.children; children[1].timeScale = 1.6f; blend.children = children;
            state.motion = blend; result.layers[0].stateMachine.defaultState = state;
            return result;
        }
        private static AnimationClip Clip(string name) => AssetDatabase.LoadAllAssetsAtPath(Root + "/Models/" + name + ".fbx")
            .OfType<AnimationClip>().Single(c => !c.name.StartsWith("__preview"));
    }
}
