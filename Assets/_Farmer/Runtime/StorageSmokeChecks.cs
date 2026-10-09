#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
namespace Farmer
{
    public static class StorageSmokeChecks
    {
        public static IEnumerator Run(string screenshot)
        {
            var game=Object.FindFirstObjectByType<FarmGame>();var ui=game.GetComponent<StorageInteraction>();var builder=game.GetComponent<BuildController>();var cc=game.Player.GetComponent<CharacterController>();var camera=Camera.main;
            var original=game.Model.Snapshot();var position=game.Player.position;var camPosition=camera.transform.position;float camSize=camera.orthographicSize;
            var devices=InputSystem.devices.Where(d=>d.enabled&&(d is Mouse||d is Keyboard)).ToArray();var keyboard=InputSystem.AddDevice<Keyboard>();var mouse=InputSystem.AddDevice<Mouse>();
            if(!game.SavePath.Contains("FarmerQA"))throw new System.InvalidOperationException("Expected QA save.");
            try
            {
                foreach(var d in devices)InputSystem.DisableDevice(d);
                var fixture=new FarmModel(new[]{game.ActiveCrop.Rules},startingMoney:500,buildCatalog:game.BuildPieces.Select(p=>p.Rules),worldSeed:732);fixture.BuySeeds(game.ActiveCrop.id,5,out _);var snap=fixture.Snapshot();snap.minuteOfDay=720;Load(game,snap);yield return null;
                Check(game.BuildPieces.Length==10,"All four delivered furniture definitions join the existing build catalog.");
                Teleport(cc,game.Market.position+Vector3.back*1.7f);yield return null;
                foreach(var id in new[]{"home_chest","home_table","home_chair","home_lantern"})
                {
                    var piece=game.BuildPieces.Single(p=>p.id==id);var buy=Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b=>b.GetComponentInChildren<Text>()?.text==$"{piece.displayName} ({piece.price})");
                    yield return Click(mouse,ButtonPoint(buy));Check(game.Model.Building.FurnitureCount(id)==1,$"Market click buys {id} into the bag.");
                }
                Check(game.Model.Money==240,"Four purchases deduct exactly their prices after seed purchase.");
                yield return Press(keyboard,Key.Tab);Check(ui.IsOpen&&ui.ChestId==null,"Tab opens the complete bag.");yield return Capture(screenshot,"bag");
                var beforePosition=game.Player.position;yield return Press(keyboard,Key.W);Check(Vector3.Distance(beforePosition,game.Player.position)<.05f,"Movement is blocked while managing the bag.");
                yield return Press(keyboard,Key.Escape);Teleport(cc,new Vector3(1.5f,.1f,3.5f));yield return Press(keyboard,Key.Digit4);
                var category=Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b=>b.GetComponentInChildren<Text>()?.text=="Mobilya");yield return Click(mouse,ButtonPoint(category));
                var slots=Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Where(b=>b.name.StartsWith("Construction Slot ")).OrderBy(b=>b.name).ToArray();yield return Click(mouse,ButtonPoint(slots[1]));
                Check(builder.ActiveDefinition.id=="home_chest"&&game.Model.Building.Count==0,$"Furniture category selects the chest without a click leaking into the world. Active={builder.ActiveDefinition.id}, count={game.Model.Building.Count}.");
                yield return Click(mouse,Cell(0,5));var chest=game.Model.Building.Blocks.Single(b=>b.pieceId=="home_chest");string idChest=chest.instanceId;
                Check(game.Model.Building.FurnitureCount("home_chest")==0&&game.Model.Building.Wood==24,"Chest placement consumes the purchased item, not wood.");
                var view=Object.FindObjectsByType<PlacedBlockView>(FindObjectsSortMode.None).Single(v=>v.Record.instanceId==idChest);
                Check(view.GetComponentsInChildren<MeshFilter>().Any(f=>f.sharedMesh.vertexCount>1000),"Placed chest uses the delivered mesh.");
                yield return Press(keyboard,Key.Escape);yield return Pointer(mouse,camera.WorldToScreenPoint(new Vector3(.5f,.4f,5.5f)),false);yield return Press(keyboard,Key.F);
                Check(ui.IsOpen&&ui.ChestId==idChest,"F opens the nearby chest through its world collider.");
                var wood=Object.FindObjectsByType<StorageSlot>(FindObjectsSortMode.None).Single(s=>!s.FromChest&&s.ItemId=="wood");var target=Object.FindObjectsByType<StorageSlot>(FindObjectsSortMode.None).First(s=>s.FromChest);
                yield return Pointer(mouse,SlotPoint(wood),false);yield return Pointer(mouse,SlotPoint(wood),true);yield return Pointer(mouse,SlotPoint(target),true);yield return Pointer(mouse,SlotPoint(target),false);
                Check(game.Model.Building.Wood==0&&game.Model.Building.StoredItems(idChest).Single(i=>i.id=="wood").count==24,"Dragging the wood stack deposits it exactly once.");
                var storedWood=Object.FindObjectsByType<StorageSlot>(FindObjectsSortMode.None).Single(s=>s.FromChest&&s.ItemId=="wood");
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.LeftShift));yield return null;yield return Click(mouse,SlotPoint(storedWood));InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
                Check(game.Model.Building.Wood==24&&game.Model.Building.StoredItems(idChest).Length==0,"Shift-click withdraws the full stack without duplication.");
                var seed=Object.FindObjectsByType<StorageSlot>(FindObjectsSortMode.None).Single(s=>!s.FromChest&&s.ItemId=="seed:"+game.ActiveCrop.id);yield return Click(mouse,SlotPoint(seed));
                Check(game.Model.Seeds(game.ActiveCrop.id)==4&&game.Model.Building.StoredItems(idChest).Single().count==1,"Ordinary click transfers only one item.");
                string before=JsonUtility.ToJson(game.Model.Snapshot());yield return Pointer(mouse,SlotPoint(seed),true);yield return Pointer(mouse,new Vector2(10,10),true);yield return Pointer(mouse,new Vector2(10,10),false);
                Check(JsonUtility.ToJson(game.Model.Snapshot())==before,"Dropping outside the other inventory is a lossless cancellation.");yield return Capture(screenshot,"storage");
                yield return Press(keyboard,Key.Escape);game.SaveGame();yield return Press(keyboard,Key.F9);
                Check(game.Model.Building.StoredItems(idChest).Single().count==1&&game.Model.Seeds(game.ActiveCrop.id)==4,"Storage identity and exact counts survive save/load.");
                yield return Press(keyboard,Key.Digit4);builder.SelectPiece(6);yield return Press(keyboard,Key.M);
                yield return Pointer(mouse,camera.WorldToScreenPoint(new Vector3(.5f,.4f,5.5f)),true);Check(builder.IsDragging,"Move mode picks up the full chest.");
                yield return Pointer(mouse,Cell(2,5),true);yield return Pointer(mouse,Cell(2,5),false);
                Check(game.Model.Building.FindStorage(idChest).x==2&&game.Model.Building.StoredItems(idChest).Single().count==1,"Moving the chest preserves its identity and contents.");
                yield return Press(keyboard,Key.Escape);Teleport(cc,new Vector3(-8,.1f,7));ui.Open(idChest);Check(!ui.IsOpen,"A distant chest cannot be opened.");
                Teleport(cc,new Vector3(1.5f,.1f,4f));yield return Press(keyboard,Key.Digit4);
                foreach(var pair in new[]{("home_table",-2,5),("home_chair",-1,4),("home_lantern",3,4)})
                {
                    builder.SelectPiece(System.Array.FindIndex(game.BuildPieces,p=>p.id==pair.Item1));yield return Click(mouse,Cell(pair.Item2,pair.Item3));
                    Check(game.Model.Building.Blocks.Any(b=>b.pieceId==pair.Item1),$"Real preview and click place {pair.Item1}. Status={builder.Status}.");
                }
                yield return Press(keyboard,Key.Escape);yield return null;
                var lamp=Object.FindFirstObjectByType<FurnitureLighting>();Check(lamp!=null&&!lamp.GetComponentInChildren<Light>(true).enabled,"The lamp is off at midday.");
                var night=game.Model.Snapshot();night.minuteOfDay=1260;Load(game,night);yield return new WaitForSecondsRealtime(.6f);
                lamp=Object.FindFirstObjectByType<FurnitureLighting>();Check(lamp!=null&&lamp.GetComponentInChildren<Light>().enabled&&lamp.GetComponentInChildren<Light>().intensity>2,"The purchased lamp lights the scene at night.");
                camera.orthographicSize=4.8f;camera.transform.position=new Vector3(.5f,1,5)-camera.transform.forward*14;yield return Capture(screenshot,"night-furniture");
                var daylight=game.Model.Snapshot();daylight.minuteOfDay=720;Load(game,daylight);yield return new WaitForSecondsRealtime(.6f);yield return Capture(screenshot,"furniture");
                Debug.Log("FARMER_STORAGE_CHECKS_FINISHED");
            }
            finally{ui.Close();game.SetBuildMode(false);Load(game,original);Teleport(cc,position);camera.transform.position=camPosition;camera.orthographicSize=camSize;InputSystem.RemoveDevice(keyboard);InputSystem.RemoveDevice(mouse);foreach(var d in devices)InputSystem.EnableDevice(d);}
        }
        private static Vector2 SlotPoint(StorageSlot s)=>RectTransformUtility.WorldToScreenPoint(null,((RectTransform)s.transform).TransformPoint(((RectTransform)s.transform).rect.center));
        private static Vector2 ButtonPoint(Button b)=>RectTransformUtility.WorldToScreenPoint(null,((RectTransform)b.transform).TransformPoint(((RectTransform)b.transform).rect.center));
        private static Vector2 Cell(int x,int z)=>Camera.main.WorldToScreenPoint(new Vector3(x+.5f,.01f,z+.5f));
        private static void Load(FarmGame g,FarmSnapshot s){File.WriteAllText(g.SavePath,JsonUtility.ToJson(s));g.LoadGame();}
        private static void Teleport(CharacterController c,Vector3 p){c.enabled=false;c.transform.position=p;c.enabled=true;Physics.SyncTransforms();}
        private static IEnumerator Press(Keyboard k,Key key){InputSystem.QueueStateEvent(k,new KeyboardState(key));yield return null;yield return null;InputSystem.QueueStateEvent(k,new KeyboardState());yield return null;yield return null;}
        private static IEnumerator Click(Mouse m,Vector2 p){yield return Pointer(m,p,false);yield return Pointer(m,p,true);yield return Pointer(m,p,false);}
        private static IEnumerator Pointer(Mouse m,Vector2 p,bool down){InputSystem.QueueStateEvent(m,new MouseState{position=p,buttons=(ushort)(down?1:0)});yield return null;yield return null;yield return null;}
        private static IEnumerator Capture(string path,string label){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Path.GetDirectoryName(path),Path.GetFileNameWithoutExtension(path)+"-"+label+".png"));yield return new WaitForSecondsRealtime(.3f);}
        private static void Check(bool ok,string msg){if(ok)Debug.Log("FARMER_STORAGE_CHECK_OK: "+msg);else Debug.LogError("FARMER_STORAGE_CHECK_FAILED: "+msg);}
    }
}
#endif
