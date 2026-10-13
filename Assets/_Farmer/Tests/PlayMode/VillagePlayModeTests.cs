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
using Object=UnityEngine.Object;
namespace Farmer.Tests
{
    public sealed class VillagePlayModeTests
    {
        private FarmGame game;
        [UnityTest] public IEnumerator DialogueHuntCookAndReload()
        {
            if(!Environment.GetCommandLineArgs().Contains("--farmer-smoke-capture"))Assert.Ignore("Requires isolated QA save.");
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/_Farmer/Scenes/Farm.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return null;yield return null;
            game=Object.FindFirstObjectByType<FarmGame>();game.GetComponent<SessionMenu>().SetOpen(false);game.GetComponent<DayNightCycle>().ClockPaused=true;
            game.GetComponent<CombatController>().enabled=false;Camera.main.GetComponent<ExplorationCamera>().enabled=false;
            var wildlife=game.GetComponent<WildlifeController>();wildlife.enabled=false;var village=game.GetComponent<VillageInteraction>();
            var motor=game.Player.GetComponent<PlayerMotor>();motor.enabled=false;
            var guide=village.Guide;Assert.That(guide,Is.Not.Null);
            motor.Teleport(guide.position+Vector3.forward*2);Physics.SyncTransforms();yield return null;
            Assert.That(village.TryTalk(),Is.True);Assert.That(game.WorldInputBlocked,Is.True);Assert.That(game.GetComponent<MarketInteraction>().TryOpen(),Is.False);
            Assert.That(village.Speaker,Is.EqualTo("Defne"));yield return new WaitForSeconds(.2f);village.Advance();
            game.GetComponent<CombatController>().RefreshHud();yield return null;CropExpansionPlayModeTests.Frame(guide.position,6);CropExpansionPlayModeTests.Capture("village-dialogue-defne",true);
            village.Advance();Assert.That(village.Speaker,Is.EqualTo("Sen"));village.Advance();yield return null;
            CropExpansionPlayModeTests.Capture("village-dialogue-player",true);village.Close();yield return null;Assert.That(game.WorldInputBlocked,Is.False);
            var animal=wildlife.View(0);Assert.That(animal,Is.Not.Null);
            motor.Teleport(animal.transform.position+Vector3.forward*8);Physics.SyncTransforms();wildlife.Simulate(.01f);Assert.That(animal.Mood,Is.EqualTo(WildlifeMood.Watch));
            motor.Teleport(animal.transform.position+Vector3.forward*3);Physics.SyncTransforms();wildlife.Simulate(.01f);Assert.That(animal.Mood,Is.EqualTo(WildlifeMood.Flee));
            Assert.That(game.Model.BuyHuntingTool("bow",out _),Is.False,"Starts without enough money for bow");
            var s=game.Model.Snapshot();s.money=1000;File.WriteAllText(game.SavePath,JsonUtility.ToJson(s));Assert.That(game.LoadGame(),Is.True);yield return null;
            wildlife.Simulate(.01f);animal=wildlife.View(0);game.Model.BuyHuntingTool("bow",out _);game.Equip(FarmItem.Bow);
            // Choose an unobstructed firing direction around the animal.
            bool fired=false;
            for(int i=0;i<8&&!fired;i++){motor.Teleport(animal.transform.position+Quaternion.Euler(0,i*45,0)*Vector3.forward*6);Physics.SyncTransforms();fired=wildlife.TryAttack(0);}
            Assert.That(fired,Is.True);Assert.That(game.Model.Animal(0).health,Is.EqualTo(15));
            for(int i=0;i<10;i++)wildlife.Simulate(.1f);
            Assert.That(game.Model.Animal(0).health,Is.Zero);Assert.That(game.Model.BagCount("hunt:raw_meat"),Is.EqualTo(2));Assert.That(game.Model.BagCount("hunt:hide"),Is.EqualTo(1));
            CropExpansionPlayModeTests.Frame(animal.transform.position,5);yield return null;CropExpansionPlayModeTests.Capture("village-hunting",true);
            var point=wildlife.FindSpace(game.Player.position+Vector3.right*5);Assert.That(point.HasValue,Is.True);int x=Mathf.FloorToInt(point.Value.x),z=Mathf.FloorToInt(point.Value.z);
            Assert.That(game.Model.BuyFurniture("home_stove",out _),Is.True);Assert.That(game.Model.Building.Place("home_stove",x,0,z,0,out _),Is.True);game.CombatChanged("Ocak kuruldu.");yield return null;
            var stove=game.Model.Building.Blocks.First(b=>b.pieceId=="home_stove");motor.Teleport(point.Value+Vector3.forward*1.6f);Physics.SyncTransforms();yield return null;
            Assert.That(village.BeginCooking(stove),Is.True);yield return null;for(int tick=0;tick<30;tick++)village.TickCooking(.1f);
            Assert.That(game.Model.BagCount("hunt:cooked_meat"),Is.EqualTo(1));game.Model.Combat.Damage(50);Assert.That(game.Model.EatHarvest(out _),Is.True);Assert.That(game.Model.Combat.Health,Is.EqualTo(90));
            CropExpansionPlayModeTests.Frame(point.Value,4);yield return null;CropExpansionPlayModeTests.Capture("village-stove",true);
            game.SaveGame();Assert.That(game.LoadGame(),Is.True);yield return null;Assert.That(game.Model.OwnsBow,Is.True);Assert.That(game.Model.Animal(0).claimed,Is.True);Assert.That(game.Model.Building.Blocks.Any(b=>b.pieceId=="home_stove"),Is.True);
            Debug.Log("FARMER_VILLAGE_PLAY_OK: dialogue speakers/modal, watch/flee, bow impact/loot, stove cooking, food and persistence.");
        }
        [UnityTearDown] public IEnumerator Cleanup(){if(game!=null&&game.SavePath.Contains("FarmerQA")){string dir=Path.GetDirectoryName(game.SavePath);Object.Destroy(game.gameObject);yield return null;if(Directory.Exists(dir))Directory.Delete(dir,true);}}
    }
}
#endif
