#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
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
    public sealed class MiningPlayModeTests
    {
        private FarmGame game;
        [UnityTest]
        public IEnumerator PurchaseMineReloadAndInspectPresentation()
        {
            if(!Environment.GetCommandLineArgs().Contains("--farmer-smoke-capture"))
                Assert.Ignore("Requires --farmer-smoke-capture to isolate the save from the player's farm.");
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/_Farmer/Scenes/Farm.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;yield return null;
            game=Object.FindFirstObjectByType<FarmGame>();Assert.That(game.SavePath,Does.Contain("FarmerQA"));
            Assert.That(game.Ready,Is.True);game.GetComponent<DayNightCycle>().ClockPaused=true;
            Camera.main.GetComponent<ExplorationCamera>().enabled=false;
            game.GetComponent<SessionMenu>().SetOpen(false);
            Assert.That(game.Model.OwnsPickaxe,Is.False);
            Assert.That(game.Model.Exploration.Nodes.Count(n=>n.kind==ResourceKind.Tree),Is.EqualTo(168));
            Assert.That(game.Model.Exploration.Nodes.Count(n=>n.kind==ResourceKind.Stone),Is.EqualTo(48));
            var slot=Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b=>b.name=="Inventory Slot 5");
            Assert.That(slot.interactable,Is.False);
            var fixture=game.Model.Snapshot();fixture.money=500;fixture.minuteOfDay=720;
            File.WriteAllText(game.SavePath,JsonUtility.ToJson(fixture));Assert.That(game.LoadGame(),Is.True);yield return null;
            Assert.That(game.BuyPickaxe(),Is.False,"Cannot buy remotely.");
            Teleport(game.Market.position+new Vector3(0,.1f,-1.8f));yield return null;
            var purchase=Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b=>b.name=="Buy Pickaxe");
            Assert.That(purchase.interactable,Is.True);purchase.onClick.Invoke();yield return null;
            Assert.That(game.Model.OwnsPickaxe,Is.True);Assert.That(game.Model.Money,Is.EqualTo(420));
            Assert.That(purchase.interactable,Is.False);Assert.That(slot.interactable,Is.True);
            slot.onClick.Invoke();yield return null;
            Assert.That(game.Model.EquippedItem,Is.EqualTo(FarmItem.Pickaxe));
            var driver=game.Player.GetComponentInChildren<FarmerAnimator>();
            Assert.That(driver.ToolSocket.Find("Held Pickaxe").gameObject.activeInHierarchy,Is.True);
            var rock=game.Model.Exploration.Nodes.Where(n=>n.kind==ResourceKind.Stone).OrderBy(n=>n.x*n.x+n.z*n.z).First();
            var point=new Vector3(rock.x+.5f,0,rock.z+.5f);
            Teleport(point+new Vector3(0,.1f,-1.6f));yield return null;
            game.Player.GetComponent<PlayerMotor>().Visual.rotation=Quaternion.identity;
            Frame(point+Vector3.up*.65f,3.1f);yield return new WaitForSeconds(.4f);
            var view=Object.FindObjectsByType<ResourceView>(FindObjectsSortMode.None).Single(v=>v.Id==rock.id);
            Assert.That(view.GetComponent<Collider>().enabled,Is.True);
            Capture("mining-before");
            Assert.That(game.Gather(rock.id),Is.True);yield return null;
            Assert.That(game.SaveGame(),Is.True);Assert.That(game.LoadGame(),Is.True);yield return null;
            Assert.That(game.Model.Exploration.Node(rock.id).hits,Is.EqualTo(1));
            Assert.That(game.Gather(rock.id),Is.True);yield return null;
            Assert.That(game.Gather(rock.id),Is.True);yield return null;
            Assert.That(game.Model.Stone,Is.EqualTo(6));Assert.That(game.Gather(rock.id),Is.False);
            view=Object.FindObjectsByType<ResourceView>(FindObjectsSortMode.None).Single(v=>v.Id==rock.id);
            Assert.That(view.GetComponent<Collider>().enabled,Is.False);
            Assert.That(view.GetComponentsInChildren<Renderer>().Length,Is.Zero);
            Capture("mining-after");
            Assert.That(game.Model.BagCount("stone"),Is.EqualTo(6));
            game.GetComponent<StorageInteraction>().Open(null);yield return null;
            Assert.That(Object.FindObjectsByType<StorageSlot>(FindObjectsSortMode.None).Any(s=>s.ItemId=="stone"&&s.Count==6),Is.True);
            game.GetComponent<StorageInteraction>().Close();
            Assert.That(game.SaveGame(),Is.True);Assert.That(game.LoadGame(),Is.True);yield return null;
            Assert.That(game.Model.OwnsPickaxe,Is.True);Assert.That(game.Model.Stone,Is.EqualTo(6));Assert.That(game.Model.Exploration.Node(rock.id).collected,Is.True);
            var crops=game.Model.Snapshot();crops.seeds[0].count=5;File.WriteAllText(game.SavePath,JsonUtility.ToJson(crops));game.LoadGame();
            for(int i=0;i<4;i++){game.Model.Till(i,0,out _);game.Model.Plant(game.Model.IndexAt(i,0),game.ActiveCrop.id,out _);}
            crops=game.Model.Snapshot();for(int i=0;i<4;i++){var p=crops.plots.Single(p=>p.x==i&&p.z==0);p.growth=i;p.watered=true;}
            File.WriteAllText(game.SavePath,JsonUtility.ToJson(crops));game.LoadGame();yield return null;
            for(int i=0;i<4;i++)
            {
                var plant=game.transform.Find($"turnip {game.Model.IndexAt(i,0)} stage {i}");Assert.That(plant,Is.Not.Null);
                Assert.That(plant.localScale.x,Is.EqualTo(game.ActiveCrop.growthStages[i].transform.localScale.x*.8f).Within(.001f));
            }
            Frame(new Vector3(2,.2f,.5f),3.4f);yield return null;Capture("turnip-scale");
            var tree=game.Model.Exploration.Nodes.First(n=>n.kind==ResourceKind.Tree&&n.id>41);
            Frame(new Vector3(tree.x,1,tree.z),18);yield return null;Capture("expanded-resources");
            Debug.Log("FARMER_MINING_PLAYMODE_OK: purchase, ownership, world resources, mining, reload, bag, crop scaling.");
        }
        private void Teleport(Vector3 position)
        {var controller=game.Player.GetComponent<CharacterController>();controller.enabled=false;game.Player.position=position;controller.enabled=true;Physics.SyncTransforms();}
        private static void Frame(Vector3 point,float size)
        {var camera=Camera.main;camera.transform.position=point+new Vector3(8,7,9);camera.transform.LookAt(point);camera.orthographicSize=size;}
        private static void Capture(string name)
        {
            if(SystemInfo.graphicsDeviceType==GraphicsDeviceType.Null)return;
            string[] args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"--farmer-smoke-capture");
            var directory=Path.GetDirectoryName(args[index+1]);Directory.CreateDirectory(directory);
            var camera=Camera.main;var target=new RenderTexture(1280,720,24);target.Create();
            var previous=RenderTexture.active;
            var image=new Texture2D(1280,720,TextureFormat.RGB24,false);
            try
            {
                RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest{destination=target});
                RenderTexture.active=target;image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();
                File.WriteAllBytes(Path.Combine(directory,name+".png"),image.EncodeToPNG());
            }
            finally {RenderTexture.active=previous;target.Release();Object.Destroy(target);Object.Destroy(image);}
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if(game!=null&&game.SavePath.Contains("FarmerQA"))
            {
                string directory=Path.GetDirectoryName(game.SavePath);
                Object.Destroy(game.gameObject);yield return null;
                if(Directory.Exists(directory))Directory.Delete(directory,true);
            }
        }
    }
}
#endif
