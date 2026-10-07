using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace Farmer
{
    // Opt-in development-player check; normal launches never create this object.
    public sealed class DevelopmentSmokeCapture : MonoBehaviour
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        private string outputPath;
        private bool hadError;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (Application.isEditor)
                return;
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "--farmer-smoke-capture");
            if (index < 0)
                return;
            if (index + 1 >= args.Length || args[index + 1].StartsWith("-"))
            {
                Debug.LogError("Smoke capture requires an output PNG path.");
                Application.Quit(1);
                return;
            }
            var capture = new GameObject("Development Smoke Capture")
                .AddComponent<DevelopmentSmokeCapture>();
            capture.outputPath = Path.GetFullPath(args[index + 1]);
        }

        private void OnEnable() => Application.logMessageReceived += OnLog;
        private void OnDisable() => Application.logMessageReceived -= OnLog;

        private void OnLog(string message, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                hadError = true;
        }

        private IEnumerator Start()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            if (File.Exists(outputPath))
            {
                Debug.LogError("Smoke capture output already exists; use a fresh path.");
                Application.Quit(1);
                yield break;
            }
            yield return new WaitForSecondsRealtime(3f);
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "--farmer-check-controls") >= 0)
                yield return ControlsSmokeChecks.Run();
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "--farmer-check-farming") >= 0)
                yield return FarmingSmokeChecks.Run(outputPath);
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "--farmer-check-watering") >= 0)
                yield return WateringSmokeChecks.Run();
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "--farmer-check-art") >= 0)
                yield return ArtSmokeChecks.Run(outputPath);
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "--farmer-check-tool-animation") >= 0)
                yield return ToolAnimationSmokeChecks.Run(outputPath);
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(outputPath);
            float deadline = Time.realtimeSinceStartup + 15f;
            while (!File.Exists(outputPath) && Time.realtimeSinceStartup < deadline)
                yield return null;
            yield return new WaitForSecondsRealtime(1f);
            bool succeeded = File.Exists(outputPath) && new FileInfo(outputPath).Length > 0 && !hadError;
            Debug.Log(succeeded ? "FARMER_PLAYER_SMOKE_OK" : "FARMER_PLAYER_SMOKE_FAILED");
            Application.Quit(succeeded ? 0 : 1);
        }
#endif
    }
}
