using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Farmer.Editor
{
    public static class MovementPrototypeSetup
    {
        [MenuItem("Farmer/Add Movement Prototype")]
        public static void UpgradeScene()
        {
            var scene = SceneManager.GetSceneByPath(ProjectSetup.ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ProjectSetup.ScenePath, OpenSceneMode.Additive);
            if (scene.isDirty) throw new InvalidOperationException("Save Farm scene changes before adding the prototype.");
            if (scene.GetRootGameObjects().Any(root => root.GetComponentInChildren<PlayerMotor>() != null))
                throw new InvalidOperationException("Movement prototype already exists; refusing to duplicate it.");

            var previous = SceneManager.GetActiveScene();
            SceneManager.SetActiveScene(scene);
            try
            {
                var camera = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Camera>()).Single();
                var placeholder = scene.GetRootGameObjects().FirstOrDefault(root => root.name == "Farm Plot - Placeholder");
                if (placeholder != null) UnityEngine.Object.DestroyImmediate(placeholder);

                var prototype = new GameObject("Movement Prototype");
                Material soil = Material("TilledSoil", new Color(0.34f, 0.20f, 0.12f));
                Material alternateSoil = Material("TilledSoilAlternate", new Color(0.38f, 0.235f, 0.14f));
                Material clothes = Material("FarmerClothes", new Color(0.18f, 0.39f, 0.49f));
                Material skin = Material("FarmerSkin", new Color(0.86f, 0.58f, 0.35f));
                Material straw = Material("StrawHat", new Color(0.90f, 0.67f, 0.28f));
                Material wood = Material("Wood", new Color(0.45f, 0.28f, 0.14f));
                Material trim = Material("WoodTrim", new Color(0.62f, 0.40f, 0.20f));
                Material path = Material("Path", new Color(0.65f, 0.57f, 0.36f));
                var gridRoot = new GameObject("Farm Grid");
                gridRoot.transform.SetParent(prototype.transform);
                var layout = new FarmGridLayout(new Vector2(-3, -3), 6, 6, 1f);
                for (int z = 0; z < layout.Depth; z++)
                    for (int x = 0; x < layout.Width; x++)
                        Primitive("Soil " + x + "," + z, PrimitiveType.Cube, gridRoot.transform,
                            layout.Center(new Vector2Int(x, z), 0.025f), new Vector3(0.95f, 0.05f, 0.95f),
                            (x + z) % 2 == 0 ? soil : alternateSoil, true);

                var actor = new GameObject("Farmer");
                actor.transform.SetParent(prototype.transform);
                actor.transform.position = new Vector3(-4, 0.1f, -4);
                var controller = actor.AddComponent<CharacterController>();
                controller.center = new Vector3(0, 0.9f, 0);
                controller.height = 1.8f; controller.radius = 0.3f;
                controller.stepOffset = 0.25f; controller.skinWidth = 0.03f;
                controller.minMoveDistance = 0;
                var model = new GameObject("Placeholder Visual");
                model.transform.SetParent(actor.transform, false);
                Primitive("Body", PrimitiveType.Capsule, model.transform, new Vector3(0, 0.85f, 0), new Vector3(0.65f, 0.65f, 0.65f), clothes);
                Primitive("Head", PrimitiveType.Sphere, model.transform, new Vector3(0, 1.62f, 0), Vector3.one * 0.55f, skin);
                Primitive("Hat Brim", PrimitiveType.Cylinder, model.transform, new Vector3(0, 1.9f, 0), new Vector3(0.85f, 0.035f, 0.85f), straw);
                Primitive("Hat Crown", PrimitiveType.Cylinder, model.transform, new Vector3(0, 2.04f, 0), new Vector3(0.48f, 0.11f, 0.48f), straw);
                Primitive("Nose", PrimitiveType.Sphere, model.transform, new Vector3(0, 1.59f, 0.27f), Vector3.one * 0.15f, skin);
                actor.AddComponent<PlayerMotor>().Configure(camera, model.transform);

                // A solid, visible obstacle also exercises the movement collision path.
                var crate = Primitive("Solid Crate", PrimitiveType.Cube, prototype.transform,
                    new Vector3(4.5f, 0.5f, 0), Vector3.one, wood, true);
                for (int i = -1; i <= 1; i += 2)
                    Primitive("Crate Band", PrimitiveType.Cube, crate.transform,
                        new Vector3(i * 0.32f, 0, 0), new Vector3(0.09f, 1.02f, 1.02f), trim);
                for (int i = 0; i < 6; i++)
                    Primitive("Path Stone", PrimitiveType.Cube, prototype.transform,
                        new Vector3(-4.2f, 0.015f, -2.5f + i), new Vector3(0.8f, 0.03f, 0.84f), path);

                Material lineMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Farmer/Materials/SelectionLine.mat");
                if (lineMaterial == null)
                {
                    lineMaterial = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
                    lineMaterial.SetColor("_BaseColor", Color.white);
                    AssetDatabase.CreateAsset(lineMaterial, "Assets/_Farmer/Materials/SelectionLine.mat");
                }
                var hover = Outline("Hover Outline", prototype.transform, lineMaterial, 0.025f);
                var selected = Outline("Selected Outline", prototype.transform, lineMaterial, 0.055f);
                var selection = gridRoot.AddComponent<FarmSelection>();
                var panels = CreateHud(prototype.transform, selection);
                selection.Configure(camera, actor.transform, hover, selected, panels);

                if (!EditorSceneManager.SaveScene(scene, ProjectSetup.ScenePath)) throw new IOException("Farm scene save failed.");
                AssetDatabase.SaveAssets();
                Debug.Log("FARMER_MOVEMENT_SETUP_OK");
            }
            finally
            {
                SceneManager.SetActiveScene(previous);
                if (opened) EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static GameObject Primitive(string name, PrimitiveType type, Transform parent, Vector3 position,
            Vector3 scale, Material material, bool solid = false)
        {
            var obj = GameObject.CreatePrimitive(type);
            obj.name = name; obj.transform.SetParent(parent, false);
            obj.transform.localPosition = position; obj.transform.localScale = scale;
            obj.GetComponent<Renderer>().sharedMaterial = material;
            if (!solid) UnityEngine.Object.DestroyImmediate(obj.GetComponent<Collider>());
            return obj;
        }

        private static Material Material(string name, Color color)
        {
            string path = "Assets/_Farmer/Materials/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name };
            material.SetColor("_BaseColor", color); material.SetFloat("_Smoothness", 0.05f);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static LineRenderer Outline(string name, Transform parent, Material material, float width)
        {
            var obj = new GameObject(name); obj.transform.SetParent(parent);
            var line = obj.AddComponent<LineRenderer>();
            line.sharedMaterial = material; line.positionCount = 4; line.loop = true;
            line.useWorldSpace = true; line.widthMultiplier = width;
            line.shadowCastingMode = ShadowCastingMode.Off; line.receiveShadows = false;
            line.numCornerVertices = 3; line.enabled = false;
            return line;
        }

        private static RectTransform[] CreateHud(Transform parent, FarmSelection selection)
        {
            var canvasObject = new GameObject("Prototype HUD", typeof(Canvas), typeof(CanvasScaler));
            canvasObject.transform.SetParent(parent);
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720); scaler.matchWidthOrHeight = 0.5f;
            var title = Panel("Title", canvasObject.transform, new Vector2(0, 1), new Vector2(24, -24), new Vector2(300, 100));
            Label("Title Text", title, "F A R M E R", 24, new Vector2(18, -12), new Vector2(268, 38), new Color(0.98f, 0.84f, 0.49f));
            Label("Subtitle", title, "İlk adımlar · Hareket ve seçim", 15, new Vector2(18, -56), new Vector2(268, 26), Color.white);
            var statusPanel = Panel("Selection Status", canvasObject.transform, new Vector2(1, 1), new Vector2(-24, -24), new Vector2(245, 100));
            var status = Label("Status", statusPanel, "TARLANI KEŞFET\n6 × 6 ekim alanı", 19, new Vector2(18, -16), new Vector2(214, 72), Color.white);
            var controls = Panel("Controls", canvasObject.transform, new Vector2(0.5f, 0), new Vector2(0, 22), new Vector2(760, 88));
            Label("Keys", controls, "WASD / OK TUŞLARI   Hareket       SOL TIK   Seç       ESC / SAĞ TIK   Temizle", 16,
                new Vector2(20, -12), new Vector2(722, 28), new Color(0.98f, 0.84f, 0.49f));
            var feedback = Label("Feedback", controls, "Bir tarla karesine yaklaş ve tıkla.", 17,
                new Vector2(20, -47), new Vector2(722, 28), Color.white);
            canvasObject.AddComponent<PrototypeHud>().Configure(selection, status, feedback);
            return new[] { title, statusPanel, controls };
        }

        private static RectTransform Panel(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = obj.GetComponent<RectTransform>(); rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = anchor; rect.anchoredPosition = position; rect.sizeDelta = size;
            var image = obj.GetComponent<Image>(); image.color = new Color(0.10f, 0.17f, 0.15f, 0.94f); image.raycastTarget = false;
            return rect;
        }

        private static Text Label(string name, Transform parent, string content, int fontSize, Vector2 position, Vector2 size, Color color)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Text));
            var rect = obj.GetComponent<RectTransform>(); rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1); rect.anchoredPosition = position; rect.sizeDelta = size;
            var text = obj.GetComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = content; text.fontSize = fontSize; text.color = color; text.raycastTarget = false;
            return text;
        }
    }
}
