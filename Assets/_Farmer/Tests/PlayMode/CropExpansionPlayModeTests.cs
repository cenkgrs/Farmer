#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object=UnityEngine.Object;
namespace Farmer.Tests
{
    public sealed class CropExpansionPlayModeTests
    {
        private FarmGame game;
        [UnityTest] public IEnumerator SelectGrowRegrowCraftStoreAndReload()
        {
            if(!Environment.GetCommandLineArgs().Contains("--farmer-smoke-capture"))Assert.Ignore("Requires isolated FarmerQA save.");
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/_Farmer/Scenes/Farm.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;yield return null;
            game=Object.FindFirstObjectByType<FarmGame>();Assert.That(game.SavePath,Does.Contain("FarmerQA"));
            game.GetComponent<SessionMenu>().SetOpen(false);game.GetComponent<DayNightCycle>().ClockPaused=true;
            Camera.main.GetComponent<ExplorationCamera>().enabled=false;
            Assert.That(game.Crops.Select(c=>c.id),Is.EquivalentTo(new[]{"turnip","carrot","tomato","lettuce","wheat","pumpkin"}));
            Assert.That(game.Recipes.First().id,Is.EqualTo("vegetable_crate"));
            Assert.That(game.Craft("vegetable_crate"),Is.False,"Remote crafting is blocked.");
            var snapshot=game.Model.Snapshot();snapshot.money=500;snapshot.minuteOfDay=720;
            File.WriteAllText(game.SavePath,JsonUtility.ToJson(snapshot));Assert.That(game.LoadGame(),Is.True);yield return null;
            GenerateIcons();
            Teleport(game.Market.position+new Vector3(0,.1f,-1.8f));yield return null;
            Assert.That(game.GetComponent<MarketInteraction>().TryOpen(),Is.True);yield return null;
            foreach(string id in new[]{"turnip","carrot","tomato"})
            {
                FindButton("Market offer "+id).onClick.Invoke();Assert.That(game.ActiveCrop.id,Is.EqualTo(id));
                Assert.That(game.Buy(4),Is.True);
            }
            Assert.That(game.Model.Seeds("carrot"),Is.EqualTo(4));Assert.That(game.Model.Seeds("tomato"),Is.EqualTo(4));
            Frame(game.Market.position,5);yield return null;Capture("crop-market",true);
            // Expose the three species and four visible stages without using the player's save.
            int x=0;
            foreach(var crop in game.Crops.Take(3))
                for(int stage=0;stage<4;stage++)
                {
                    game.Model.Till(x,0,out _);game.Model.Plant(game.Model.IndexAt(x,0),crop.id,out _);x++;
                }
            snapshot=game.Model.Snapshot();x=0;
            foreach(var crop in game.Crops.Take(3))
                for(int stage=0;stage<4;stage++)
                {
                    int column=x++;var plot=snapshot.plots.Single(p=>p.x==column&&p.z==0);plot.growth=(int)Math.Ceiling(stage*crop.wateredDays/3.0);plot.watered=true;
                }
            File.WriteAllText(game.SavePath,JsonUtility.ToJson(snapshot));Assert.That(game.LoadGame(),Is.True);yield return null;
            for(int i=0;i<12;i++)Assert.That(game.transform.Find($"{game.Model.Plot(i).cropId} {i} stage {i%4}"),Is.Not.Null);
            Teleport(new Vector3(6,.1f,-1.8f));Frame(new Vector3(6,.4f,.5f),6);yield return null;Capture("crop-growth-stages",false);
            int tomato=game.Model.IndexAt(11,0);
            Assert.That(game.Model.Harvest(game.Model.IndexAt(3,0),out _),Is.True);
            Assert.That(game.Model.Harvest(game.Model.IndexAt(7,0),out _),Is.True);
            Assert.That(game.Model.Harvest(tomato,out _),Is.True);Assert.That(game.Model.Stage(tomato),Is.EqualTo(2));
            game.SaveGame();Assert.That(game.LoadGame(),Is.True);yield return null;
            Assert.That(game.transform.Find($"tomato {tomato} stage 2"),Is.Not.Null);
            game.Model.AdvanceMinutes(1440);Assert.That(game.Model.IsReady(tomato),Is.False);
            game.Model.AdvanceMinutes(1440);Assert.That(game.Model.IsReady(tomato),Is.True);
            Teleport(game.Market.position+new Vector3(0,.1f,-1.8f));yield return null;
            Assert.That(game.GetComponent<MarketInteraction>().TryOpen(),Is.True);yield return null;
            game.GetComponent<MarketHud>().ChooseCategory(MarketCategory.Production);yield return null;
            var craft=FindButton("Market purchase");Assert.That(craft.interactable,Is.True);craft.onClick.Invoke();yield return null;
            Assert.That(game.Model.BagCount("crafted:vegetable_crate"),Is.EqualTo(1));Assert.That(craft.interactable,Is.False);
            game.GetComponent<MarketInteraction>().Close();yield return null;
            var storage=game.GetComponent<StorageInteraction>();storage.Open(null);yield return null;
            Frame(game.Market.position,5);Capture("crop-bag-page1",true);
            Assert.That(FindButton("›").interactable,Is.True);FindButton("›").onClick.Invoke();yield return null;
            Assert.That(Object.FindObjectsByType<StorageSlot>(FindObjectsSortMode.None).Any(s=>s.ItemId=="furniture:home_lantern"),Is.True);
            Assert.That(Object.FindObjectsByType<StorageSlot>(FindObjectsSortMode.None).Any(s=>s.ItemId=="crafted:vegetable_crate"&&s.Count==1),Is.True);
            Frame(game.Market.position,5);Capture("crop-bag-page2",true);storage.Close();
            game.SaveGame();Assert.That(game.LoadGame(),Is.True);yield return null;
            Assert.That(game.ActiveCrop.id,Is.EqualTo("tomato"));Assert.That(game.Model.BagCount("crafted:vegetable_crate"),Is.EqualTo(1));
            Assert.That(game.GetComponent<MarketInteraction>().TryOpen(),Is.True);
            int money=game.Model.Money;Assert.That(game.SellCrafted("vegetable_crate"),Is.True);Assert.That(game.Model.Money,Is.EqualTo(money+90));
            game.Model.Equip(FarmItem.Hoe);Assert.That(game.Model.Uproot(tomato,out _),Is.True);Assert.That(game.Model.Stage(tomato),Is.EqualTo(-1));
            Debug.Log("FARMER_CROP_PLAYMODE_OK: selection, market, stages, regrowth, processing, pagination, persistence, sale, uproot.");
        }
        private static Button FindButton(string name)=>Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b=>b.name==name);
        private void Teleport(Vector3 point)
        {var c=game.Player.GetComponent<CharacterController>();c.enabled=false;game.Player.position=point;c.enabled=true;Physics.SyncTransforms();}
        internal static void Frame(Vector3 point,float size)
        {Camera.main.transform.position=point+new Vector3(8,7,9);Camera.main.transform.LookAt(point);Camera.main.orthographicSize=size;}
        internal static Texture2D Render(Camera camera,int sizeX,int sizeY)
        {
            var target=new RenderTexture(sizeX,sizeY,24,RenderTextureFormat.ARGB32);target.Create();var previous=RenderTexture.active;
            var image=new Texture2D(sizeX,sizeY,TextureFormat.RGBA32,false);
            try
            {
                RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest{destination=target});
                RenderTexture.active=target;image.ReadPixels(new Rect(0,0,sizeX,sizeY),0,0);image.Apply();return image;
            }
            finally{RenderTexture.active=previous;target.Release();Object.Destroy(target);}
        }
        private static void GenerateIcons()
        {
            string directory="Assets/_Farmer/Resources/CropIcons";Directory.CreateDirectory(directory);
            var cameraObject=new GameObject("Icon Camera",typeof(Camera));var camera=cameraObject.GetComponent<Camera>();camera.enabled=false;
            camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;camera.cullingMask=1<<30;camera.nearClipPlane=.01f;camera.farClipPlane=10;
            foreach(string id in new[]{"carrot","tomato","vegetable_crate"})
            {
                var prefab=Resources.Load<GameObject>("CropArt/"+id+(id=="vegetable_crate"?"":"_stage_3"));
                var item=Object.Instantiate(prefab,new Vector3(1000,1000,1000),Quaternion.identity);
                foreach(var t in item.GetComponentsInChildren<Transform>())t.gameObject.layer=30;
                var renderers=item.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
                camera.transform.position=bounds.center+new Vector3(1,1.1f,1);camera.transform.LookAt(bounds.center);camera.orthographicSize=bounds.size.magnitude*.63f;
                var texture=Render(camera,256,256);string path=$"{directory}/{id}.png";File.WriteAllBytes(path,texture.EncodeToPNG());Object.Destroy(texture);Object.DestroyImmediate(item);
                AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            }
            Object.Destroy(cameraObject);
        }
        internal static void Capture(string name,bool includeUI,int width=1280,int height=720)
        {
            var canvases=includeUI?Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray():Array.Empty<Canvas>();
            var camera=Camera.main;var previous=camera.targetTexture;float near=camera.nearClipPlane;
            var target=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);target.Create();
            var scalers=canvases.Select(c=>c.GetComponent<CanvasScaler>()).ToArray();
            var scales=canvases.Select(c=>c.scaleFactor).ToArray();
            var planes=canvases.Select(c=>c.planeDistance).ToArray();
            var extra=camera.GetUniversalAdditionalCameraData();bool post=extra.renderPostProcessing;
            try
            {
                camera.targetTexture=target;camera.nearClipPlane=.1f;extra.renderPostProcessing=false;
                for(int i=0;i<canvases.Length;i++)
                {
                    var c=canvases[i];var scaler=scalers[i];if(scaler!=null)scaler.enabled=false;
                    c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=1;
                    if(scaler!=null)
                    {
                        var r=scaler.referenceResolution;
                        c.scaleFactor=scaler.screenMatchMode==CanvasScaler.ScreenMatchMode.Expand?Mathf.Min(width/r.x,height/r.y):Mathf.Pow(width/r.x,1-scaler.matchWidthOrHeight)*Mathf.Pow(height/r.y,scaler.matchWidthOrHeight);
                    }
                }
                foreach(var c in canvases)foreach(var t in c.GetComponentsInChildren<Text>())t.SetAllDirty();
                Canvas.ForceUpdateCanvases();
                foreach(var c in canvases.Where(c=>c.name=="Market Canvas"))
                {
                    var frame=c.transform.Find("Market") as RectTransform;var corners=new Vector3[4];frame.GetWorldCorners(corners);
                    var bounds=corners.Select(v=>RectTransformUtility.WorldToScreenPoint(camera,v)).ToArray();
                    Assert.That(bounds.Min(v=>v.x),Is.GreaterThanOrEqualTo(-1));Assert.That(bounds.Max(v=>v.x),Is.LessThanOrEqualTo(width+1));
                    Assert.That(bounds.Min(v=>v.y),Is.GreaterThanOrEqualTo(-1));Assert.That(bounds.Max(v=>v.y),Is.LessThanOrEqualTo(height+1));
                    Assert.That((bounds.Max(v=>v.x)-bounds.Min(v=>v.x))/(bounds.Max(v=>v.y)-bounds.Min(v=>v.y)),Is.EqualTo(1672f/941f).Within(.001f));
                }
                var image=Render(camera,width,height);string[] args=Environment.GetCommandLineArgs();string directory=Path.GetDirectoryName(args[Array.IndexOf(args,"--farmer-smoke-capture")+1]);
                File.WriteAllBytes(Path.Combine(directory,name+".png"),image.EncodeToPNG());Object.Destroy(image);
            }
            finally
            {
                camera.targetTexture=previous;camera.nearClipPlane=near;extra.renderPostProcessing=post;
                for(int i=0;i<canvases.Length;i++)
                {
                    var c=canvases[i];c.renderMode=RenderMode.ScreenSpaceOverlay;c.worldCamera=null;c.planeDistance=planes[i];c.scaleFactor=scales[i];if(scalers[i]!=null)scalers[i].enabled=true;
                }
                target.Release();Object.Destroy(target);Canvas.ForceUpdateCanvases();
            }
        }

        [UnityTearDown] public IEnumerator Cleanup()
        {
            if(game!=null&&game.SavePath.Contains("FarmerQA"))
            {
                string dir=Path.GetDirectoryName(game.SavePath);Object.Destroy(game.gameObject);yield return null;
                if(Directory.Exists(dir))Directory.Delete(dir,true);
            }
        }
    }
}
#endif
