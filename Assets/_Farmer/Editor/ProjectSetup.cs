using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Farmer.Editor
{
    // Explicit setup only: opening the project never regenerates authored content.
    public static class ProjectSetup
    {
        public const string ScenePath = "Assets/_Farmer/Scenes/Farm.unity";
        private const string MaterialsPath = "Assets/_Farmer/Materials";

        [MenuItem("Farmer/Create Initial Scene")]
        public static void CreateInitialScene()
        {
            if (File.Exists(ScenePath))
                throw new InvalidOperationException("Farm scene already exists; refusing to overwrite it.");

            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            EditorSettings.serializationMode = SerializationMode.ForceText;
            PlayerSettings.companyName = "Cenk Gurses";
            PlayerSettings.productName = "Farmer";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.defaultScreenWidth = 1600;
            PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            Directory.CreateDirectory(MaterialsPath);
            AssetDatabase.Refresh();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var groundMaterial = CreateMaterial("Ground", new Color(0.36f, 0.48f, 0.23f));
            var soilMaterial = CreateMaterial("Soil", new Color(0.30f, 0.17f, 0.09f));

            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.transform.position = new Vector3(0f, -0.25f, 0f);
            ground.transform.localScale = new Vector3(20f, 0.5f, 20f);
            ground.GetComponent<Renderer>().sharedMaterial = groundMaterial;

            var plot = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plot.name = "Farm Plot - Placeholder";
            plot.transform.position = new Vector3(0f, 0.025f, 0f);
            plot.transform.localScale = new Vector3(4f, 0.05f, 4f);
            plot.GetComponent<Renderer>().sharedMaterial = soilMaterial;

            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.rotation = Quaternion.Euler(35.264f, 45f, 0f);
            cameraObject.transform.position = -cameraObject.transform.forward * 25f;
            var camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 9f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.63f, 0.75f, 0.80f);
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = false;

            var sunObject = new GameObject("Sun", typeof(Light));
            sunObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            var sun = sunObject.GetComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.91f, 0.77f);
            sun.intensity = 1.5f;
            sun.shadows = LightShadows.Soft;
            RenderSettings.sun = sun;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.55f, 0.61f, 0.67f);
            RenderSettings.skybox = null;

            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new IOException("Could not save the initial Farm scene.");

            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            ValidateProject();
            Debug.Log("FARMER_SETUP_OK: initial scene created. Gameplay is not implemented yet.");
        }

        [MenuItem("Farmer/Validate Project Setup")]
        public static void ValidateProject()
        {
            if (Application.unityVersion != "6000.3.25f1")
                throw new InvalidOperationException("Use the pinned Unity Editor 6000.3.25f1.");
            if (EditorSettings.serializationMode != SerializationMode.ForceText)
                throw new InvalidOperationException("Asset serialization must use Force Text.");
            if (GraphicsSettings.defaultRenderPipeline is not UniversalRenderPipelineAsset)
                throw new InvalidOperationException("A URP pipeline asset must be assigned.");
            if (!EditorBuildSettings.scenes.Any(s => s.enabled && s.path == ScenePath))
                throw new InvalidOperationException("Farm scene must be included in build settings.");
            if (!File.Exists(ScenePath) || !File.Exists(ScenePath + ".meta"))
                throw new InvalidOperationException("Farm scene or its metadata is missing.");

            // Additive inspection avoids replacing a developer's current working scene.
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool openedForValidation = !scene.isLoaded;
            if (openedForValidation)
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                var cameras = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<Camera>(true)).ToArray();
                if (cameras.Length != 1 || !cameras[0].orthographic)
                    throw new InvalidOperationException("Farm requires exactly one orthographic camera.");
                Debug.Log("FARMER_VALIDATION_OK: pinned Editor, URP, text serialization and scene verified.");
            }
            finally
            {
                if (openedForValidation)
                    EditorSceneManager.CloseScene(scene, true);
            }
        }

        public static void BuildLinux()
        {
            ValidateProject();
            Directory.CreateDirectory("builds/Linux");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "builds/Linux/Farmer.x86_64",
                target = BuildTarget.StandaloneLinux64,
                options = BuildOptions.Development
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException($"Linux build failed: {report.summary.result}");
            Debug.Log("FARMER_BUILD_OK: builds/Linux/Farmer.x86_64");
        }

        private static Material CreateMaterial(string name, Color color)
        {
            string path = $"{MaterialsPath}/{name}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
                return existing;
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                throw new InvalidOperationException("URP Lit shader is unavailable.");
            var material = new Material(shader) { name = name };
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", 0.15f);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
    }
}
