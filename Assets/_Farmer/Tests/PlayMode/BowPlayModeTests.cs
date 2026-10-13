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
    public sealed class BowPlayModeTests
    {
        private FarmGame game;
        [UnityTest] public IEnumerator FreeAimHitsCreaturesAndRespectsWallsRangeAndPause()
        {
            if(!Environment.GetCommandLineArgs().Contains("--farmer-smoke-capture"))Assert.Ignore("Requires isolated QA save.");
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/_Farmer/Scenes/Farm.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return null;yield return null;
            game=Object.FindFirstObjectByType<FarmGame>();game.GetComponent<SessionMenu>().SetOpen(false);game.GetComponent<DayNightCycle>().ClockPaused=true;
            var combat=game.GetComponent<CombatController>();var wildlife=game.GetComponent<WildlifeController>();combat.enabled=wildlife.enabled=false;
            var motor=game.Player.GetComponent<PlayerMotor>();motor.enabled=false;
            var state=game.Model.Snapshot();state.money=1000;File.WriteAllText(game.SavePath,JsonUtility.ToJson(state));game.LoadGame();combat.Simulate(0,false);wildlife.Simulate(0,false);yield return null;
            Assert.That(game.Model.BuyHuntingTool("bow",out _),Is.True);game.Equip(FarmItem.Bow);
            Vector3 origin=new Vector3(1000,0,1000);motor.Teleport(origin);var enemy=combat.View(0);Move(enemy.transform,origin+Vector3.forward*6);
            var bow=game.GetComponent<BowCombat>();Physics.SyncTransforms();int health=combat.Health(0);
            // The cursor ray points past every target; no target selection is supplied.
            Assert.That(bow.FireRay(new Ray(origin+new Vector3(0,20,20),Vector3.down)),Is.True);
            Assert.That(bow.FireDirection(Vector3.forward),Is.False,"Rate limit is independent of targets.");
            var arrow=Object.FindFirstObjectByType<HuntingArrow>();arrow.enabled=false;game.Equip(FarmItem.Axe);arrow.Simulate(1);
            Assert.That(combat.Health(0),Is.EqualTo(health-15));Assert.That(arrow.Finished,Is.True);yield return null;
            Move(enemy.transform,origin+Vector3.right*20);
            var animal=wildlife.View(0);Move(animal.transform,origin+Vector3.forward*5);Physics.SyncTransforms();
            arrow=Shot(origin);arrow.Simulate(1);Assert.That(game.Model.Animal(0).health,Is.Zero);Assert.That(game.Model.Animal(0).roughKill,Is.False);Assert.That(game.Model.BagCount("hunt:raw_meat"),Is.EqualTo(2));yield return null;
            Move(animal.transform,origin+Vector3.right*20);
            var crab=combat.View(1);Move(crab.transform,origin+Vector3.forward*7);health=combat.Health(1);Physics.SyncTransforms();
            arrow=Shot(origin);arrow.Simulate(1);Assert.That(combat.Health(1),Is.EqualTo(health-15));yield return null;health=combat.Health(1);
            var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.transform.position=origin+new Vector3(0,.75f,3);wall.transform.localScale=new Vector3(2,1.5f,.05f);Physics.SyncTransforms();
            arrow=Shot(origin);arrow.Simulate(1);Assert.That(arrow.Finished,Is.True);Assert.That(combat.Health(1),Is.EqualTo(health),"Thin walls block even a long frame.");Object.Destroy(wall);yield return null;
            Move(crab.transform,origin+Vector3.forward*15);Physics.SyncTransforms();arrow=Shot(origin);arrow.Simulate(1);
            Assert.That(arrow.Travelled,Is.EqualTo(HuntingArrow.Range).Within(.001f));Assert.That(combat.Health(1),Is.EqualTo(health));yield return null;
            Move(crab.transform,origin+Vector3.forward*6);Physics.SyncTransforms();arrow=Shot(origin);Move(crab.transform,origin+new Vector3(3,0,6));Physics.SyncTransforms();arrow.Simulate(1);
            Assert.That(combat.Health(1),Is.EqualTo(health),"Moving away evades a straight arrow.");yield return null;
            arrow=Shot(origin);game.GetComponent<SessionMenu>().SetOpen(true);arrow.Simulate(.2f);Assert.That(arrow.Travelled,Is.Zero);game.GetComponent<SessionMenu>().SetOpen(false);arrow.Simulate(.1f);Assert.That(arrow.Travelled,Is.GreaterThan(0));
            game.SaveGame();game.LoadGame();arrow.Simulate(.1f);Assert.That(arrow.Finished,Is.True,"An old-save arrow must not damage new-save actors.");
            Debug.Log("FARMER_BOW_OK: free aim, enemy and animal hits, in-flight weapon switch, walls, range, misses, pause, load invalidation.");
        }
        private HuntingArrow Shot(Vector3 origin){var a=HuntingArrow.Launch(game,origin+Vector3.up*.5f,Vector3.forward);a.enabled=false;return a;}
        private static void Move(Transform t,Vector3 p){var c=t.GetComponent<CharacterController>();c.enabled=false;t.position=p;c.enabled=true;}
        [UnityTearDown] public IEnumerator Cleanup(){if(game!=null&&game.SavePath.Contains("FarmerQA")){string dir=Path.GetDirectoryName(game.SavePath);Object.Destroy(game.gameObject);yield return null;if(Directory.Exists(dir))Directory.Delete(dir,true);}}
    }
}
#endif
