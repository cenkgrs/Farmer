#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object=UnityEngine.Object;
namespace Farmer.Tests
{
    public sealed class DemoPlayModeTests
    {
        private FarmGame game;
        [UnityTest] public IEnumerator JournalSettingsNavigationAndResume()
        {
            if(!Environment.GetCommandLineArgs().Contains("--farmer-smoke-capture"))Assert.Ignore("Requires isolated QA save.");
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/_Farmer/Scenes/Farm.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return null;yield return null;
            game=Object.FindFirstObjectByType<FarmGame>();game.GetComponent<SessionMenu>().SetOpen(false);game.GetComponent<DayNightCycle>().ClockPaused=true;
            var guide=game.GetComponent<DemoHud>();guide.SetOpen(true);yield return null;
            Assert.That(GameObject.Find("Farm HUD").GetComponent<CanvasGroup>().alpha,Is.Zero);
            Assert.That(game.WorldInputBlocked,Is.True);Assert.That(game.Selection.ModalBlocked,Is.True);Assert.That(game.GetComponent<CombatController>().TryAttack(0),Is.False);
            Assert.That(game.GetComponent<MarketInteraction>().TryOpen(),Is.False);game.GetComponent<StorageInteraction>().Open(null);Assert.That(game.InventoryOpen,Is.False);
            double time=game.Model.MinuteOfDay;yield return new WaitForSeconds(.1f);Assert.That(game.Model.MinuteOfDay,Is.EqualTo(time));
            guide.ChangeVolume(-.3f);Assert.That(AudioListener.volume,Is.EqualTo(.7f).Within(.001f));guide.CycleFps();Assert.That(Application.targetFrameRate,Is.EqualTo(120));guide.ChangeVolume(10);Assert.That(AudioListener.volume,Is.EqualTo(1));
            CropExpansionPlayModeTests.Capture("demo-journal-1280",true);CropExpansionPlayModeTests.Capture("demo-journal-1920",true,1920,1080);
            var market=Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b=>b.GetComponentInChildren<Text>()?.text=="Pazar");market.onClick.Invoke();yield return null;
            Assert.That(game.JournalOpen,Is.False);Assert.That(game.WorldInputBlocked,Is.False);Assert.That(guide.Destination,Is.Zero);
            Assert.That(game.Model.BuySeeds(game.ActiveCrop.id,1,out _),Is.True);Assert.That(game.Model.Completed(JourneyStep.Seeds),Is.True);Assert.That(game.SaveGame(),Is.True);Assert.That(game.LoadGame(),Is.True);Assert.That(game.Model.Completed(JourneyStep.Seeds),Is.True);
            guide.Refresh();CropExpansionPlayModeTests.Capture("demo-navigation",true);
            game.GetComponent<SessionMenu>().SetOpen(true);guide.SetOpen(true);yield return null;guide.SetOpen(false);Assert.That(game.MenuOpen,Is.True);Assert.That(game.WorldInputBlocked,Is.True);
            Debug.Log("FARMER_DEMO_PLAYMODE_OK: journal, modal guards, navigation, settings, progress persistence and resolutions.");
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {AudioListener.volume=1;Application.targetFrameRate=60;if(game!=null&&game.SavePath.Contains("FarmerQA")){var dir=Path.GetDirectoryName(game.SavePath);Object.Destroy(game.gameObject);yield return null;if(Directory.Exists(dir))Directory.Delete(dir,true);}}
    }
}
#endif
