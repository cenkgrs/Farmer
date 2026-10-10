#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object=UnityEngine.Object;
namespace Farmer.Tests
{
    public sealed class MarketDesignPlayModeTests
    {
        private FarmGame game;
        [UnityTest] public IEnumerator CatalogQuantityPurchasesSalesAndReferenceLayout()
        {
            if(!Environment.GetCommandLineArgs().Contains("--farmer-smoke-capture"))Assert.Ignore("Requires isolated FarmerQA save.");
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/_Farmer/Scenes/Farm.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;yield return null;
            game=Object.FindFirstObjectByType<FarmGame>();Assert.That(game.SavePath,Does.Contain("FarmerQA"));
            game.GetComponent<SessionMenu>().SetOpen(false);game.GetComponent<DayNightCycle>().ClockPaused=true;
            var s=game.Model.Snapshot();s.money=308;s.selectedCropId="tomato";
            foreach(var c in s.seeds)c.count=4;
            s.produce.Single(c=>c.cropId=="turnip").count=12;s.produce.Single(c=>c.cropId=="carrot").count=28;s.produce.Single(c=>c.cropId=="tomato").count=18;
            s.crafted.Single(c=>c.id=="vegetable_crate").count=7;
            File.WriteAllText(game.SavePath,JsonUtility.ToJson(s));game.LoadGame();
            var cc=game.Player.GetComponent<CharacterController>();cc.enabled=false;game.Player.position=game.Market.position+new Vector3(0,.1f,-1.8f);cc.enabled=true;Physics.SyncTransforms();
            game.GetComponent<MarketInteraction>().TryOpen();yield return null;yield return null;
            var ui=game.GetComponent<MarketHud>();Assert.That(ui.Selected.Id,Is.EqualTo("tomato"));
            Assert.That(Object.FindObjectsByType<Text>(FindObjectsSortMode.None).Single(t=>t.name=="Wallet").text,Is.EqualTo("308"));
            CropExpansionPlayModeTests.Capture("market-design-seeds",true,1672,941);
            CropExpansionPlayModeTests.Capture("market-design-720p",true,1280,720);
            CropExpansionPlayModeTests.Capture("market-design-4x3",true,1024,768);
            CropExpansionPlayModeTests.Capture("market-design-wide",true,1920,1080);
            // UI event routing, including raycasts, proves the art doesn't swallow clicks.
            Click("Market quantity plus");Click("Market quantity plus");Assert.That(ui.Quantity,Is.EqualTo(3));
            Click("Market purchase");Assert.That(game.Model.Seeds("tomato"),Is.EqualTo(7));Assert.That(game.Model.Money,Is.EqualTo(236));
            ui.ChangeQuantity(9999);Assert.That(ui.Quantity,Is.EqualTo(FarmModel.StackLimit));Assert.That(ui.PurchaseSelected(),Is.False);
            Assert.That(game.Model.Money,Is.EqualTo(236));ui.ChangeQuantity(-9999);Assert.That(ui.Quantity,Is.EqualTo(1));
            Click("Market category Aletler");yield return null;Assert.That(ui.Selected.Id,Is.EqualTo("pickaxe"));
            ui.ChangeQuantity(10);Assert.That(ui.Quantity,Is.EqualTo(1));Click("Market purchase");Assert.That(game.Model.OwnsPickaxe,Is.True);Assert.That(ui.PurchaseSelected(),Is.False);
            CropExpansionPlayModeTests.Capture("market-design-tools",true);
            Click("Market category Malzemeler");yield return null;int wood=game.Model.Building.Wood;Click("Market purchase");Assert.That(game.Model.Building.Wood,Is.EqualTo(wood+10));
            CropExpansionPlayModeTests.Capture("market-design-materials",true);
            Click("Market category Mobilyalar");yield return null;Assert.That(ui.Selected.Id,Is.EqualTo("bed"));
            Click("Market purchase");Assert.That(game.Model.Building.Beds,Is.EqualTo(1));
            Click("Next market page");yield return null;
            Assert.That(ui.Selected.Id,Is.EqualTo("home_chair"));Click("Market purchase");Assert.That(game.Model.Building.FurnitureCount("home_chair"),Is.EqualTo(1));
            CropExpansionPlayModeTests.Capture("market-design-furniture",true);
            Click("Market category Üretim");yield return null;Click("Market quantity plus");Click("Market purchase");
            Assert.That(game.Model.BagCount("crafted:vegetable_crate"),Is.EqualTo(9));Assert.That(game.Model.Produce("tomato"),Is.EqualTo(14));
            CropExpansionPlayModeTests.Capture("market-design-production",true);
            Click("Market sell item crafted:vegetable_crate");int before=game.Model.Money;Click("Market sell");Assert.That(game.Model.Money,Is.EqualTo(before+810));Assert.That(game.Model.BagCount("crafted:vegetable_crate"),Is.Zero);
            Assert.That(ui.SellSelected(),Is.False);Assert.That(game.Model.SelectedCropId,Is.EqualTo("tomato"));
            Click("Market sell item crop:carrot");Click("Market sell");Assert.That(game.Model.Produce("carrot"),Is.Zero);Assert.That(game.Model.SelectedCropId,Is.EqualTo("tomato"));
            game.SaveGame();Assert.That(game.LoadGame(),Is.True);yield return null;Assert.That(game.MarketOpen,Is.False);Assert.That(ui.PurchaseSelected(),Is.False);
            Assert.That(game.Model.OwnsPickaxe,Is.True);Assert.That(game.Model.Building.FurnitureCount("home_chair"),Is.EqualTo(1));
            game.GetComponent<MarketInteraction>().TryOpen();yield return null;yield return null;
            Assert.That(ui.Category,Is.EqualTo(MarketCategory.Seeds));Assert.That(ui.Quantity,Is.EqualTo(1));
            Click("Close Market");Assert.That(game.MarketOpen,Is.False);
            Debug.Log("FARMER_MARKET_DESIGN_OK: raycast UI, five categories, quantity/capacity, unique tool, furniture pages, recipe, sale selection and reload.");
        }
        private static void Click(string name)
        {
            Canvas.ForceUpdateCanvases();var b=Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Single(x=>x.name==name);
            Assert.That(b.interactable,Is.True,name);
            Vector2 point=RectTransformUtility.WorldToScreenPoint(null,b.transform.TransformPoint(((RectTransform)b.transform).rect.center));
            var data=new PointerEventData(EventSystem.current){position=point,button=PointerEventData.InputButton.Left};
            var hits=new System.Collections.Generic.List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);
            Assert.That(hits.Count,Is.GreaterThan(0),name);
            Assert.That(ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject),Is.EqualTo(b.gameObject),"Top raycast must reach "+name);
            ExecuteEvents.Execute(b.gameObject,data,ExecuteEvents.pointerClickHandler);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if(game!=null&&game.SavePath.Contains("FarmerQA"))
            {string dir=Path.GetDirectoryName(game.SavePath);Object.Destroy(game.gameObject);yield return null;if(Directory.Exists(dir))Directory.Delete(dir,true);}
        }
    }
}
#endif
