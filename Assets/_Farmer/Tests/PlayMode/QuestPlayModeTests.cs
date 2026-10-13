#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Farmer.Tests
{
    public sealed class QuestPlayModeTests
    {
        [UnityTest] public IEnumerator AuthoredNpcQuestsAndJournal()
        {
            var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"--farmer-smoke-capture");
            if(i<0)Assert.Ignore("Requires isolated QA save.");
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/_Farmer/Scenes/Farm.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return null;yield return null;
            yield return QuestSmokeChecks.Run(args[i+1]);
        }
    }
}
#endif
