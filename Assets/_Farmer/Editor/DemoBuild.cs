using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace Farmer.Editor
{
    public static class DemoBuild
    {
        // -buildTarget must match --farmer-demo-target; do not switch the user's active editor.
        public static void Build()
        {
            string[] args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"--farmer-demo-target");
            string platform=index>=0&&index+1<args.Length?args[index+1]:"mac";
            var target=platform=="windows"?BuildTarget.StandaloneWindows64:platform=="linux"?BuildTarget.StandaloneLinux64:platform=="mac"?BuildTarget.StandaloneOSX:throw new ArgumentException("Use windows, linux or mac.");
            if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone,target))throw new InvalidOperationException("Install the matching Unity 6000.3.25f1 build support module.");
            ProjectSetup.ValidateProject();
            string folder="builds/Demo/"+platform;Directory.CreateDirectory(folder);
            string output=folder+(platform=="windows"?"/Farmer.exe":platform=="linux"?"/Farmer.x86_64":"/Farmer.app");
            bool development=args.Contains("--farmer-demo-development");
            int revisionIndex=Array.IndexOf(args,"--farmer-demo-revision");
            string revision=revisionIndex>=0&&revisionIndex+1<args.Length?args[revisionIndex+1]:"local-worktree";
            string old=PlayerSettings.bundleVersion;
            try
            {
                PlayerSettings.bundleVersion="0.5.0";
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/_Farmer/Scenes/Farm.unity"},locationPathName=output,target=target,options=BuildOptions.StrictMode|(development?BuildOptions.Development:BuildOptions.None)});
                if(report.summary.result!=BuildResult.Succeeded||report.summary.totalErrors!=0)throw new InvalidOperationException("Demo build failed: "+report.summary.result);
                File.WriteAllText(folder+"/build-info.json",JsonUtility.ToJson(new BuildInfo{platform=platform,unity=Application.unityVersion,version="0.5.0",development=development,sourceRevision=revision},true));
                Debug.Log("FARMER_DEMO_BUILD_OK: "+output);
            }
            finally{PlayerSettings.bundleVersion=old;}
        }
        [Serializable] private sealed class BuildInfo{public string platform,unity,version,sourceRevision;public bool development;}
    }
}
