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
    public sealed class MarketInteractionPlayModeTests
    {
        private FarmGame game;
        [UnityTest] public IEnumerator MarketRequiresInteractAndClosesWithoutLeakingInput()
        {
            if(!Environment.GetCommandLineArgs().Contains("--farmer-smoke-capture"))Assert.Ignore("Requires isolated FarmerQA save.");
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/_Farmer/Scenes/Farm.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;yield return null;
            game=Object.FindFirstObjectByType<FarmGame>();Assert.That(game.SavePath,Does.Contain("FarmerQA"));
            game.GetComponent<SessionMenu>().SetOpen(false);game.GetComponent<DayNightCycle>().ClockPaused=true;
            var market=game.GetComponent<MarketInteraction>();var bag=game.GetComponent<StorageInteraction>();
            Assert.That(market.TryOpen(),Is.False,"Cannot open remotely.");
            var s=game.Model.Snapshot();s.money=500;File.WriteAllText(game.SavePath,JsonUtility.ToJson(s));game.LoadGame();
            Teleport(game.Market.position+new Vector3(0,.1f,-1.8f));yield return null;
            CropExpansionPlayModeTests.Frame(game.Market.position,5);yield return null;
            CropExpansionPlayModeTests.Capture("market-closed",true);
            Assert.That(game.NearMarket,Is.True);Assert.That(game.MarketOpen,Is.False);
            var panel=game.transform.Find("Farm HUD/Market");Assert.That(panel.gameObject.activeSelf,Is.False);
            Assert.That(game.Buy(1),Is.False);Assert.That(game.BuyPickaxe(),Is.False);Assert.That(game.BuyWood(),Is.False);Assert.That(game.BuyBed(),Is.False);Assert.That(game.BuyFurniture("home_chair"),Is.False);
            Assert.That(game.Model.Money,Is.EqualTo(500));
            Assert.That(market.HandleInput(true,false),Is.True);Assert.That(game.MarketOpen,Is.True);Assert.That(game.WorldInputBlocked,Is.True);
            Assert.That(game.Selection.ModalBlocked,Is.True);Assert.That(market.ConsumedThisFrame,Is.True);
            yield return null;
            Assert.That(panel.gameObject.activeSelf,Is.True);Assert.That(game.Buy(1),Is.True);Assert.That(game.Model.Money,Is.EqualTo(490));
            bag.Open(null);Assert.That(game.InventoryOpen,Is.False);
            game.SetBuildMode(true);Assert.That(game.BuildMode,Is.False);
            Assert.That(game.UseHovered(),Is.False);Assert.That(game.UprootHovered(),Is.False);Assert.That(game.Rest(),Is.False);
            var equipped=game.Model.EquippedItem;game.Equip(FarmItem.Axe);Assert.That(game.Model.EquippedItem,Is.EqualTo(equipped));
            Assert.That(market.HandleInput(false,true),Is.True);Assert.That(game.MarketOpen,Is.False);Assert.That(game.MenuOpen,Is.False);Assert.That(market.ConsumedThisFrame,Is.True);
            bag.Open(null);Assert.That(game.InventoryOpen,Is.False,"The closing interaction cannot also open storage.");
            yield return null;Assert.That(game.WorldInputBlocked,Is.False);Assert.That(game.Selection.ModalBlocked,Is.False);
            Assert.That(market.HandleInput(true,false),Is.True);yield return null;
            Assert.That(market.HandleInput(true,false),Is.True);Assert.That(game.MarketOpen,Is.False);yield return null;
            Assert.That(market.TryOpen(),Is.True);yield return null;
            Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b=>b.name=="Close Market").onClick.Invoke();
            Assert.That(game.MarketOpen,Is.False);Assert.That(game.Selection.ModalBlocked,Is.False);
            Assert.That(market.TryOpen(),Is.True);Teleport(Vector3.zero);market.ValidateAccess();Assert.That(game.MarketOpen,Is.False);
            Teleport(game.Market.position+new Vector3(0,.1f,-1.8f));Assert.That(market.TryOpen(),Is.True);
            Assert.That(game.LoadGame(),Is.True);Assert.That(game.MarketOpen,Is.False);
            Assert.That(market.TryOpen(),Is.True);game.GetComponent<SessionMenu>().SetOpen(true);Assert.That(game.MarketOpen,Is.False);
            Assert.That(game.Selection.ModalBlocked,Is.True);Assert.That(market.TryOpen(),Is.False);
            game.GetComponent<SessionMenu>().SetOpen(false);
            bag.Open(null);Assert.That(market.TryOpen(),Is.False);bag.Close();
            game.SetBuildMode(true);Assert.That(market.TryOpen(),Is.False);game.SetBuildMode(false);
            Debug.Log("FARMER_MARKET_INTERACTION_OK: explicit F, close F/Esc/button, input isolation, proximity, transactions, load and menu transitions.");
        }
        private void Teleport(Vector3 p){var c=game.Player.GetComponent<CharacterController>();c.enabled=false;game.Player.position=p;c.enabled=true;Physics.SyncTransforms();}
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if(game!=null&&game.SavePath.Contains("FarmerQA"))
            {string dir=Path.GetDirectoryName(game.SavePath);Object.Destroy(game.gameObject);yield return null;if(Directory.Exists(dir))Directory.Delete(dir,true);}
        }
    }
}
#endif
