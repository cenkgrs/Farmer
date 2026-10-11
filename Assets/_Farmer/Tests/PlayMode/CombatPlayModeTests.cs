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
    public sealed class CombatPlayModeTests
    {
        private FarmGame game;
        [UnityTest] public IEnumerator CombatCollisionTelegraphDodgeDefeatAndPersistence()
        {
            if(!Environment.GetCommandLineArgs().Contains("--farmer-smoke-capture"))Assert.Ignore("Requires isolated QA save.");
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/_Farmer/Scenes/Farm.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return null;yield return null;
            game=Object.FindFirstObjectByType<FarmGame>();game.GetComponent<SessionMenu>().SetOpen(false);game.GetComponent<DayNightCycle>().ClockPaused=true;
            var combat=game.GetComponent<CombatController>();combat.enabled=false;var motor=game.Player.GetComponent<PlayerMotor>();motor.enabled=false;
            Camera.main.GetComponent<ExplorationCamera>().enabled=false;
            var slime=combat.View(0);var beetle=combat.View(1);Assert.That(slime,Is.Not.Null);Assert.That(beetle,Is.Not.Null);
            Assert.That(motor.WorldBounds.HasValue,Is.True);Assert.That(motor.WorldBounds.Value.size.x,Is.LessThan(160));
            Assert.That(CombatController.PlanarDistance(slime.Home,combat.Sanctuary),Is.InRange(17,35));Assert.That(combat.TryAttack(0),Is.False);
            motor.Teleport(slime.Home+Vector3.forward*1.5f);game.Equip(FarmItem.Axe);Physics.SyncTransforms();
            for(int i=0;i<40&&!slime.WindingUp;i++)combat.Simulate(.1f);Assert.That(slime.WindingUp,Is.True);Assert.That(game.Model.Combat.Health,Is.EqualTo(100));
            for(int i=0;i<10;i++)combat.Simulate(.1f);Assert.That(game.Model.Combat.Health,Is.EqualTo(88));
            game.InventoryOpen=true;for(int i=0;i<30;i++)combat.Simulate(.1f);Assert.That(game.Model.Combat.Health,Is.EqualTo(88));Assert.That(combat.TryAttack(0),Is.False);
            game.InventoryOpen=false;combat.Simulate(.1f);Assert.That(game.Model.Combat.Health,Is.EqualTo(88));
            Assert.That(motor.TryDodge(Vector2.up),Is.True);combat.HitPlayer(0);Assert.That(game.Model.Combat.Health,Is.EqualTo(88));Assert.That(motor.TryDodge(Vector2.up),Is.False);motor.CancelDodge();
            var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.name="Combat test wall";wall.transform.position=slime.Home+Vector3.forward*.75f+Vector3.up;wall.transform.localScale=new Vector3(3,2,.2f);Physics.SyncTransforms();
            Assert.That(combat.TryAttack(0),Is.False,"Cannot hit through walls.");Object.Destroy(wall);yield return null;
            Assert.That(combat.TryAttack(0),Is.True);Assert.That(combat.Health(0),Is.EqualTo(35),"Damage waits for contact.");Assert.That(combat.TryAttack(0),Is.False,"Cooldown prevents repeat hits.");
            for(int n=0;n<8;n++)combat.Simulate(.1f);Assert.That(combat.TryAttack(0),Is.True);for(int n=0;n<8;n++)combat.Simulate(.1f);Assert.That(combat.TryAttack(0),Is.True);for(int n=0;n<4;n++)combat.Simulate(.1f);Assert.That(game.Model.Combat.Enemy(0).health,Is.Zero);
            combat.Simulate(.1f);combat.RefreshHud();CropExpansionPlayModeTests.Frame(slime.Home+Vector3.up*.4f,4);yield return null;CropExpansionPlayModeTests.Capture("combat-forest-cache",true);
            Assert.That(combat.Claimed(0),Is.True,"Reward is automatic at defeat.");Assert.That(combat.TryAttack(0),Is.False);Assert.That(game.SaveGame(),Is.True);Assert.That(game.LoadGame(),Is.True);combat.Simulate(.1f);yield return null;Assert.That(game.Model.Combat.Enemy(0).rewardDay,Is.EqualTo(game.Model.Day));
            var occupied=GameObject.CreatePrimitive(PrimitiveType.Cube);occupied.name="Structure over enemy home";occupied.transform.position=combat.View(0).Home+Vector3.up*.6f;occupied.transform.localScale=new Vector3(1.3f,1.2f,1.3f);Physics.SyncTransforms();
            game.Model.AdvanceMinutes(1440);combat.Simulate(.1f);Assert.That(CombatController.PlanarDistance(combat.View(0).transform.position,combat.View(0).Home),Is.GreaterThan(.5f),"Respawn must respect a newly placed structure.");Object.Destroy(occupied);yield return null;
            beetle=combat.View(1);motor.Teleport(beetle.Home+Vector3.forward*1.6f);for(int i=0;i<40&&!beetle.WindingUp;i++)combat.Simulate(.1f);
            Assert.That(beetle.WindingUp,Is.True);combat.Simulate(.1f);combat.RefreshHud();CropExpansionPlayModeTests.Frame(beetle.Home+Vector3.up*.4f,4);yield return new WaitForSeconds(.3f);CropExpansionPlayModeTests.Capture("combat-beetle-warning",true);
            game.Model.Combat.Damage(game.Model.Combat.Health-1);int before=game.Model.Money;combat.HitPlayer(1);
            Assert.That(game.Model.Combat.Health,Is.EqualTo(100));Assert.That(game.Model.Money,Is.EqualTo(before-Math.Min(10,before/10)));Assert.That(CombatController.PlanarDistance(game.Player.position,combat.Sanctuary),Is.LessThan(10));
            for(int i=0;i<100;i++)combat.Simulate(.1f);Assert.That(game.Model.Combat.Health,Is.EqualTo(100));Assert.That(CombatController.PlanarDistance(beetle.transform.position,beetle.Home),Is.LessThan(.3f));
            // Dash uses the same CharacterController as walking and cannot cross a solid wall.
            var testGround=GameObject.CreatePrimitive(PrimitiveType.Cube);testGround.transform.position=new Vector3(80,-.25f,-80);testGround.transform.localScale=new Vector3(10,.5f,10);
            motor.WorldBounds=null;motor.Teleport(new Vector3(80,.1f,-80));motor.Configure(Camera.main,motor.Visual);Camera.main.transform.rotation=Quaternion.LookRotation(Vector3.forward);
            wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.transform.position=new Vector3(80,1,-79);wall.transform.localScale=new Vector3(4,2,.2f);Physics.SyncTransforms();motor.Step(Vector2.zero,2);Assert.That(motor.TryDodge(Vector2.up),Is.True);
            for(int i=0;i<6;i++)motor.Step(Vector2.up,.05f);Assert.That(game.Player.position.z,Is.LessThan(-79.3f));Object.Destroy(wall);Object.Destroy(testGround);
            Debug.Log("FARMER_COMBAT_PLAYMODE_OK: two enemies, telegraph, cooldown, modal pause, dodge, wall occlusion, rewards, persistence and safe defeat.");
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {if(game!=null&&game.SavePath.Contains("FarmerQA")){string dir=Path.GetDirectoryName(game.SavePath);Object.Destroy(game.gameObject);yield return null;if(Directory.Exists(dir))Directory.Delete(dir,true);}}
    }
}
#endif
