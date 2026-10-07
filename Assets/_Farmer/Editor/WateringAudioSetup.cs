using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Farmer.Editor
{
    public static class WateringAudioSetup
    {
        public static void Apply()
        {
            const string path = "Assets/_Farmer/Audio/watering_can_pour.wav";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (AudioImporter)AssetImporter.GetAtPath(path);
            importer.forceToMono = true;
            var settings = importer.defaultSampleSettings;
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.PCM;
            settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            importer.defaultSampleSettings = settings;
            importer.SaveAndReimport();
            var scene = EditorSceneManager.OpenScene(ProjectSetup.ScenePath, OpenSceneMode.Additive);
            try
            {
                if (scene.isDirty) throw new InvalidOperationException("Refusing to edit an unsaved scene.");
                var target = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<FarmPresentation>()).Single();
                var serialized = new SerializedObject(target);
                serialized.FindProperty("wateringSound").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                AssetDatabase.SaveAssets();
                Debug.Log("FARMER_WATERING_AUDIO_ASSIGNED");
            }
            finally { EditorSceneManager.CloseScene(scene, true); }
        }
    }
}
