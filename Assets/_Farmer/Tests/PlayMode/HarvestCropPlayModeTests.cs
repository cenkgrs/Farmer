#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object=UnityEngine.Object;
namespace Farmer.Tests
{
    public sealed class HarvestCropPlayModeTests
    {
        private FarmGame game;
        [UnityTest] public IEnumerator SixCropMarketQuickSelectionGrowBasketAndReload()
        {
            if(!Environment.GetCommandLineArgs().Contains("--farmer-smoke-capture"))Assert.Ignore("Requires isolated FarmerQA save.");
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/_Farmer/Scenes/Farm.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return null;yield return null;
            game=Object.FindFirstObjectByType<FarmGame>();Assert.That(game.SavePath,Does.Contain("FarmerQA"));game.GetComponent<SessionMenu>().SetOpen(false);game.GetComponent<DayNightCycle>().ClockPaused=true;Camera.main.GetComponent<ExplorationCamera>().enabled=false;
            GenerateIcons();var ui=game.GetComponent<MarketHud>();ui.BuildSalePage(0);
            Assert.That(game.Crops.Select(c=>c.id),Is.EqualTo(new[]{"turnip","carrot","tomato","lettuce","wheat","pumpkin"}));
            var s=game.Model.Snapshot();s.money=500;s.minuteOfDay=720;File.WriteAllText(game.SavePath,JsonUtility.ToJson(s));game.LoadGame();yield return null;
            Teleport(game.Market.position+new Vector3(0,.1f,-1.8f));yield return null;Assert.That(game.GetComponent<MarketInteraction>().TryOpen(),Is.True);yield return null;
            Find("Next market page").onClick.Invoke();yield return null;
            foreach(string id in new[]{"lettuce","wheat","pumpkin"})
            {
                Find("Market offer "+id).onClick.Invoke();Find("Market purchase").onClick.Invoke();yield return null;Assert.That(game.Model.Seeds(id),Is.EqualTo(1));
                Assert.That(GameObject.Find("Selected product").GetComponent<Text>().text,Does.Contain(game.Definition(id).displayName));
            }
            CropExpansionPlayModeTests.Capture("harvest-market",true);
            // Reopening on a second-page seed must keep its actual card visible and selected.
            game.GetComponent<MarketInteraction>().Close();yield return null;game.GetComponent<MarketInteraction>().TryOpen();yield return null;yield return null;
            Assert.That(ui.Selected.Id,Is.EqualTo("pumpkin"));Assert.That(Find("Market offer pumpkin"),Is.Not.Null);
            CropExpansionPlayModeTests.Capture("harvest-market-4x3",true,1024,768);
            game.GetComponent<MarketInteraction>().Close();yield return null;
            Teleport(new Vector3(1.5f,.1f,2.5f));CropExpansionPlayModeTests.Frame(new Vector3(1.5f,.2f,.5f),3.5f);game.Equip(FarmItem.Seeds);yield return null;
            var picker=GameObject.Find("Seed selection").GetComponent<RectTransform>();Assert.That(picker.GetComponentsInChildren<Button>().Length,Is.EqualTo(6));
            foreach(var b in picker.GetComponentsInChildren<Button>()) {var rect=(RectTransform)b.transform;Assert.That(rect.anchoredPosition.y-rect.rect.height,Is.GreaterThanOrEqualTo(-picker.sizeDelta.y));}
            int x=0;foreach(string id in new[]{"lettuce","wheat","pumpkin"})
            {
                Find("Select crop "+id).onClick.Invoke();Assert.That(game.ActiveCrop.id,Is.EqualTo(id));game.Model.Till(x,0,out _);int plot=game.Model.IndexAt(x,0);Assert.That(game.Model.Plant(plot,id,out _),Is.True);Assert.That(game.Model.Water(plot,out _),Is.True);x++;
            }
            game.SaveGame();game.LoadGame();yield return null;CropExpansionPlayModeTests.Capture("harvest-seedlings",true);
            game.Model.AdvanceMinutes(1440);game.SaveGame();game.LoadGame();yield return null;
            Assert.That(game.Model.Stage(game.Model.IndexAt(0,0)),Is.EqualTo(1));Assert.That(game.Model.IsReady(game.Model.IndexAt(0,0)),Is.False);
            game.Model.AdvanceMinutes(7*1440);game.SaveGame();game.LoadGame();yield return null;
            for(int i=0;i<3;i++)Assert.That(game.Model.Stage(game.Model.IndexAt(i,0)),Is.EqualTo(3));
            CropExpansionPlayModeTests.Capture("harvest-ripe",true);
            CropExpansionPlayModeTests.Capture("harvest-ripe-4x3",true,1024,768);
            foreach(string id in new[]{"lettuce","wheat","pumpkin"})
            {
                Assert.That(CropArtwork.Get(id),Is.Not.Null);Assert.That(SeedArtwork.Get(id),Is.Not.Null);Assert.That(SeedArtwork.Get(id).texture,Is.SameAs(Resources.Load<Texture2D>("SeedArt/"+id)));var definition=game.Definition(id);Assert.That(definition.growthStages.Length,Is.EqualTo(4));
                foreach(var prefab in definition.growthStages)Assert.That(prefab.GetComponentsInChildren<Renderer>().All(r=>r.sharedMaterial!=null&&r.sharedMaterial.shader.isSupported),Is.True);
                int triangles=definition.growthStages[3].GetComponentsInChildren<MeshFilter>().Sum(m=>m.sharedMesh.triangles.Length/3);Assert.That(triangles,Is.LessThan(5000));Debug.Log("FARMER_CROP_TRIANGLES "+id+" "+triangles);
            }
            for(int i=0;i<3;i++)Assert.That(game.Model.Harvest(game.Model.IndexAt(i,0),out _),Is.True);
            game.SaveGame();game.LoadGame();yield return null;Teleport(game.Market.position+new Vector3(0,.1f,-1.8f));yield return null;game.GetComponent<MarketInteraction>().TryOpen();yield return null;
            ui.ChooseCategory(MarketCategory.Production);yield return null;Find("Market offer harvest_basket").onClick.Invoke();yield return null;Assert.That(Find("Market purchase").interactable,Is.True);Find("Market purchase").onClick.Invoke();yield return null;
            Assert.That(game.Model.BagCount("crafted:harvest_basket"),Is.EqualTo(1));Assert.That(Find("Market purchase").interactable,Is.False);
            Find("Next sale page").onClick.Invoke();yield return null;Find("Market sell item crafted:harvest_basket").onClick.Invoke();yield return null;
            Assert.That(GameObject.Find("Harvest icon crafted:harvest_basket").GetComponent<Image>().sprite,Is.Not.Null);
            CropExpansionPlayModeTests.Capture("harvest-basket",true);
            game.GetComponent<MarketInteraction>().Close();yield return null;var storage=game.GetComponent<StorageInteraction>();storage.Open(null);yield return null;
            Find("›").onClick.Invoke();yield return null;Assert.That(Object.FindObjectsByType<StorageSlot>(FindObjectsSortMode.None).Any(slot=>slot.ItemId=="crafted:harvest_basket"&&slot.Count==1),Is.True);CropExpansionPlayModeTests.Capture("harvest-bag",true);storage.Close();yield return null;
            game.SaveGame();Assert.That(game.LoadGame(),Is.True);yield return null;Assert.That(game.ActiveCrop.id,Is.EqualTo("pumpkin"));Assert.That(game.Model.BagCount("crafted:harvest_basket"),Is.EqualTo(1));
            game.GetComponent<MarketInteraction>().TryOpen();yield return null;ui.BuildSalePage(1);yield return null;Find("Market sell item crafted:harvest_basket").onClick.Invoke();int money=game.Model.Money;Assert.That(ui.SellSelected(),Is.True);Assert.That(game.Model.Money,Is.EqualTo(money+145));Assert.That(game.Model.SelectedCropId,Is.EqualTo("pumpkin"));
            ui.BuildSalePage(0);yield return null;Assert.That(Find("Market sell").interactable,Is.False,"Page changes cannot leave an off-page sale selected.");
            Debug.Log("FARMER_HARVEST_PLAYMODE_OK: six crops, seed/sale pages, icons, quick selection, growth, basket production, storage and persistence.");
        }
        private static Button Find(string name)=>Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b=>b.name==name);
        private void Teleport(Vector3 point){var c=game.Player.GetComponent<CharacterController>();c.enabled=false;game.Player.position=point;c.enabled=true;Physics.SyncTransforms();}
        private static void GenerateIcons()
        {
            var cameraObject=new GameObject("Harvest icon camera",typeof(Camera));var camera=cameraObject.GetComponent<Camera>();camera.enabled=false;camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;camera.cullingMask=1<<30;camera.nearClipPlane=.01f;camera.farClipPlane=10;
            foreach(string id in new[]{"lettuce","wheat","pumpkin","harvest_basket"})
            {
                var item=Object.Instantiate(Resources.Load<GameObject>("HarvestArt/"+id+(id=="harvest_basket"?"":"_stage_3")),Vector3.one*1000,Quaternion.identity);foreach(var t in item.GetComponentsInChildren<Transform>())t.gameObject.layer=30;
                var renderers=item.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
                camera.transform.position=bounds.center+new Vector3(1,1.2f,1);camera.transform.LookAt(bounds.center);camera.orthographicSize=bounds.size.magnitude*.52f;
                var texture=CropExpansionPlayModeTests.Render(camera,256,256);string path="Assets/_Farmer/Resources/CropIcons/"+id+".png";File.WriteAllBytes(path,texture.EncodeToPNG());Object.Destroy(texture);Object.DestroyImmediate(item);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
            }
            Object.Destroy(cameraObject);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {if(game!=null&&game.SavePath.Contains("FarmerQA")){string dir=Path.GetDirectoryName(game.SavePath);Object.Destroy(game.gameObject);yield return null;if(Directory.Exists(dir))Directory.Delete(dir,true);}}
    }
}
#endif
