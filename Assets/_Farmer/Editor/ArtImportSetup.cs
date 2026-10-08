using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Farmer.Editor
{
    public static class ArtImportSetup
    {
        public const string Root = "Assets/_Farmer/Art";
        public static void ImportAndInspect()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ConfigureCharacter("farmer_idle", null);
            var avatar = AssetDatabase.LoadAllAssetsAtPath(Root + "/Models/farmer_idle.fbx").OfType<Avatar>().Single();
            if (!avatar.isHuman || !avatar.isValid) throw new InvalidOperationException("Farmer Humanoid avatar is invalid.");
            ConfigureCharacter("farmer_walk", avatar);
            foreach (string name in new[] { "farmer", "watering_can", "sickle", "market_stall" }) CreateMaterial(name);
            foreach (string name in new[] { "farmer_idle", "farmer_walk", "watering_can", "sickle", "market_stall" })
            {
                string path = Root + "/Models/" + name + ".fbx";
                var instance = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path));
                var renderers = instance.GetComponentsInChildren<Renderer>();
                var bounds = renderers[0].bounds;
                foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
                Debug.Log($"FARMER_ART_IMPORT: {name} bounds={bounds} root={instance.transform.localScale} meshes={renderers.Length}");
                foreach (var clip in AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview")))
                    Debug.Log($"FARMER_ART_CLIP: {name} / {clip.name} length={clip.length} human={clip.humanMotion} loop={clip.isLooping}");
                var animator = instance.GetComponent<Animator>();
                if (animator != null && animator.isHuman)
                    foreach (var bone in new[] { HumanBodyBones.Hips, HumanBodyBones.RightHand, HumanBodyBones.LeftFoot, HumanBodyBones.Head })
                        Debug.Log($"FARMER_ART_BONE: {bone} {animator.GetBoneTransform(bone).position} rot={animator.GetBoneTransform(bone).eulerAngles}");
                UnityEngine.Object.DestroyImmediate(instance);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("FARMER_ART_IMPORT_OK");
        }
        private static void ConfigureCharacter(string name, Avatar source)
        {
            string path = Root + "/Models/" + name + ".fbx";
            var importer = (ModelImporter)AssetImporter.GetAtPath(path);
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = source == null ? ModelImporterAvatarSetup.CreateFromThisModel : ModelImporterAvatarSetup.CopyFromOther;
            importer.sourceAvatar = source;
            importer.importAnimation = true;
            importer.importCameras = false; importer.importLights = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.optimizeGameObjects = false;
            var clips = importer.defaultClipAnimations;
            foreach (var clip in clips)
            {
                clip.name = name == "farmer_idle" ? "Idle" : "Walk";
                clip.loopTime = true; clip.loopPose = true;
                clip.lockRootRotation = true; clip.lockRootHeightY = true; clip.lockRootPositionXZ = true;
                clip.keepOriginalOrientation = true; clip.keepOriginalPositionY = true; clip.keepOriginalPositionXZ = true;
            }
            importer.clipAnimations = clips;
            importer.SaveAndReimport();
        }
        public static void CreateMaterial(string name)
        {
            foreach (string suffix in new[] { "basecolor", "normal", "metallic_smoothness" })
            {
                string path = $"{Root}/Textures/{name}/{name}_{suffix}.png";
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = suffix == "normal" ? TextureImporterType.NormalMap : TextureImporterType.Default;
                importer.sRGBTexture = suffix == "basecolor";
                importer.alphaSource = TextureImporterAlphaSource.FromInput;
                importer.maxTextureSize = name == "farmer" || name == "market_stall" ? 2048 : 1024;
                importer.mipmapEnabled = true;
                importer.SaveAndReimport();
            }
            string materialPath = Root + "/Materials/" + name + ".mat";
            System.IO.Directory.CreateDirectory(Root + "/Materials"); AssetDatabase.Refresh();
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null) { material = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(material, materialPath); }
            material.SetColor("_BaseColor", Color.white);
            material.SetTexture("_BaseMap", Texture(name, "basecolor"));
            material.SetTexture("_BumpMap", Texture(name, "normal")); material.EnableKeyword("_NORMALMAP");
            material.SetTexture("_MetallicGlossMap", Texture(name, "metallic_smoothness")); material.EnableKeyword("_METALLICSPECGLOSSMAP");
            material.SetFloat("_Smoothness", .65f); material.SetFloat("_BumpScale", .65f);
            EditorUtility.SetDirty(material);
        }
        private static Texture2D Texture(string name, string suffix) => AssetDatabase.LoadAssetAtPath<Texture2D>($"{Root}/Textures/{name}/{name}_{suffix}.png");
    }
}
