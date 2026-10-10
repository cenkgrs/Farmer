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
    public sealed class GardenPlayModeTests
    {
        [UnityTest] public IEnumerator WalletGardenPlacementGateAndPersistence()
        {
            if(!Environment.GetCommandLineArgs().Contains("--farmer-smoke-capture"))Assert.Ignore("Requires isolated FarmerQA save.");
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/_Farmer/Scenes/Farm.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return null;yield return null;
            var game=Object.FindFirstObjectByType<FarmGame>();Assert.That(game.SavePath,Does.Contain("FarmerQA"));game.GetComponent<SessionMenu>().SetOpen(false);game.GetComponent<DayNightCycle>().ClockPaused=true;
            Camera.main.GetComponent<ExplorationCamera>().enabled=false;
            var snapshot=game.Model.Snapshot();snapshot.money=987654;snapshot.stone=30;snapshot.building.wood=200;snapshot.minuteOfDay=720;snapshot.building.blocks=Array.Empty<BlockRecord>();snapshot.plots=Array.Empty<PlotRecord>();
            File.WriteAllText(game.SavePath,JsonUtility.ToJson(snapshot));Assert.That(game.LoadGame(),Is.True);yield return null;
            GenerateIcons();
            var header=GameObject.Find("Clock and Wallet");Assert.That(header,Is.Not.Null);
            var coin=header.transform.Find("Wallet coin").GetComponent<RawImage>();Assert.That(coin.texture,Is.SameAs(Resources.Load<Texture2D>("MarketArt/approved")));
            Assert.That(header.GetComponentsInChildren<Text>().Any(t=>t.text=="987654"),Is.True);
            var controller=game.Player.GetComponent<CharacterController>();controller.enabled=false;game.Player.position=new Vector3(2.5f,.15f,3.5f);controller.enabled=true;Physics.SyncTransforms();
            CropExpansionPlayModeTests.Frame(new Vector3(1.5f,.2f,1.5f),4);yield return null;CropExpansionPlayModeTests.Capture("garden-hud",true);
            game.SetBuildMode(true);yield return null;
            Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b=>b.name=="Bahçe").onClick.Invoke();yield return null;
            Assert.That(game.GetComponent<BuildController>().ActiveDefinition.isOutdoor,Is.True);
            Assert.That(GameObject.Find("Construction Inventory").GetComponentsInChildren<Image>().Count(i=>i.name=="Construction Icon"&&i.enabled),Is.EqualTo(3));
            // Three-by-three garden perimeter with a gate and paths through its entrance.
            for(int x=0;x<3;x++)
            {
                Assert.That(game.PlaceBlock(x==1?"garden_gate":"garden_fence",x,0,2,0),Is.True);
                Assert.That(game.PlaceBlock("garden_fence",x,0,0,2),Is.True);
            }
            for(int z=0;z<3;z++)
            {
                Assert.That(game.PlaceBlock("garden_fence",0,0,z,3),Is.True);Assert.That(game.PlaceBlock("garden_fence",2,0,z,1),Is.True);
                Assert.That(game.PlaceBlock("stone_path",1,0,z,0),Is.True);
            }
            Assert.That(game.Model.Stone,Is.EqualTo(24));yield return null;yield return null;
            var gate=Object.FindObjectsByType<DoorView>(FindObjectsSortMode.None).Single(d=>d.Record.pieceId=="garden_gate");
            var ray=new Ray(new Vector3(1.5f,.5f,2.5f),Vector3.forward);var leaf=gate.transform.Find("Hinge").GetComponent<BoxCollider>();Physics.SyncTransforms();Assert.That(leaf.Raycast(ray,out _,2),Is.True);
            game.SetBuildMode(false);yield return null;Assert.That(gate.TryToggle(),Is.True,game.Feedback);
            yield return new WaitForSeconds(.5f);Physics.SyncTransforms();Assert.That(leaf.Raycast(ray,out _,2),Is.False,"Open leaf leaves a walkable gate opening.");
            controller.enabled=false;game.Player.position=new Vector3(1.5f,.15f,1.5f);controller.enabled=true;yield return null;
            Assert.That(game.GetComponent<HouseVisibility>().Indoors,Is.False);Assert.That(game.GetComponent<HouseVisibility>().FadedCount,Is.Zero);
            Assert.That(Object.FindObjectsByType<PlacedBlockView>(FindObjectsSortMode.None).Where(b=>b.Record.pieceId.StartsWith("garden_")).All(b=>b.GetComponent<OccludingWall>()==null),Is.True);
            game.SaveGame();Assert.That(game.LoadGame(),Is.True);yield return null;yield return null;
            gate=Object.FindObjectsByType<DoorView>(FindObjectsSortMode.None).Single(d=>d.Record.pieceId=="garden_gate");Assert.That(gate.IsOpen,Is.True);Assert.That(game.Model.Stone,Is.EqualTo(24));
            game.SetBuildMode(true);game.GetComponent<BuildController>().SelectPiece(Array.FindIndex(game.BuildPieces,p=>p.id=="stone_path"));yield return null;
            CropExpansionPlayModeTests.Frame(new Vector3(1.5f,.35f,1.4f),3.7f);CropExpansionPlayModeTests.Capture("garden-building",true);
            CropExpansionPlayModeTests.Capture("garden-hud-4x3",true,1024,768);
            var path=game.Model.Building.Blocks.First(b=>b.pieceId=="stone_path");Assert.That(game.RemoveBlock(path),Is.True);Assert.That(game.Model.Stone,Is.EqualTo(26));
            Assert.That(game.MoveBlock(gate.Record,4,0,2,0),Is.True);yield return null;yield return null;
            Assert.That(game.Model.Building.Blocks.Single(b=>b.pieceId=="garden_gate").doorOpen,Is.True);
            // Regression: unloading while building must not notify an already-destroyed tool socket.
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/_Farmer/Scenes/Farm.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return null;yield return null;
            game=Object.FindFirstObjectByType<FarmGame>();game.GetComponent<SessionMenu>().SetOpen(false);yield return null;
            Object.Destroy(game.gameObject);yield return null;
            Debug.Log("FARMER_GARDEN_PLAYMODE_OK: wallet art, category, placement costs, colliders, gate swing, outdoor visibility, persistence, removal and move.");
        }
        private static void GenerateIcons()
        {
            var cameraObject=new GameObject("Garden icon camera",typeof(Camera));var camera=cameraObject.GetComponent<Camera>();camera.enabled=false;camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;camera.cullingMask=1<<30;camera.nearClipPlane=.01f;camera.farClipPlane=10;
            foreach(string id in new[]{"garden_fence","garden_gate","stone_path"})
            {
                var item=Object.Instantiate(Resources.Load<GameObject>("GardenArt/"+id),Vector3.one*1000,Quaternion.identity);foreach(var t in item.GetComponentsInChildren<Transform>())t.gameObject.layer=30;
                var renderers=item.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
                camera.transform.position=bounds.center+new Vector3(1,1.2f,1);camera.transform.LookAt(bounds.center);camera.orthographicSize=bounds.size.magnitude*.53f;
                var texture=CropExpansionPlayModeTests.Render(camera,256,256);string path="Assets/_Farmer/Resources/ConstructionArt/"+id+".png";File.WriteAllBytes(path,texture.EncodeToPNG());Object.Destroy(texture);Object.DestroyImmediate(item);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
            }
            Object.Destroy(cameraObject);
        }
    }
}
#endif
