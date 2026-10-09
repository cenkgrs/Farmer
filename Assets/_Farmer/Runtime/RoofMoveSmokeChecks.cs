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
    public static class RoofMoveSmokeChecks
    {
        public static IEnumerator Run(string screenshot)
        {
            var game=Object.FindFirstObjectByType<FarmGame>();var builder=game.GetComponent<BuildController>();var cc=game.Player.GetComponent<CharacterController>();
            if(!game.SavePath.Contains("FarmerQA"))throw new System.InvalidOperationException("Expected isolated save.");
            var original=game.Model.Snapshot();bool campActive=game.Camp.gameObject.activeSelf;var camera=Camera.main;var oldPosition=camera.transform.position;float oldSize=camera.orthographicSize;
            var physical=InputSystem.devices.Where(d=>d.enabled&&(d is Mouse||d is Keyboard)).ToArray();var keyboard=InputSystem.AddDevice<Keyboard>();var mouse=InputSystem.AddDevice<Mouse>();
            try
            {
                foreach(var d in physical)InputSystem.DisableDevice(d);game.Camp.gameObject.SetActive(false);
                var fresh=new FarmModel(new[]{game.ActiveCrop.Rules},buildCatalog:game.BuildPieces.Select(d=>d.Rules));Load(game,fresh.Snapshot());yield return null;
                Check(game.Model.Money==50&&Object.FindObjectsByType<Bed>(FindObjectsSortMode.None).Length==0,"A new world has 50 coins and no starter bed.");
                Teleport(cc,game.Camp.position+Vector3.right);yield return Press(keyboard,Key.N);Check(game.Model.Day==1&&!game.NearCamp,"The removed starter bed cannot skip the night.");
                Teleport(cc,game.Market.position+Vector3.back*1.7f);yield return null;
                Check(!game.BuyBed()&&game.Model.Money==50,"A bed is unaffordable before earning 100 coins.");
                var paid=game.Model.Snapshot();paid.money=100;Load(game,paid);yield return null;
                var button=Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b=>b.GetComponentInChildren<Text>()?.text=="Yatak al (100 para)");
                var buyPoint=RectTransformUtility.WorldToScreenPoint(null,button.transform.position+(Vector3)((RectTransform)button.transform).rect.center);
                yield return Pointer(mouse,buyPoint,true);yield return Pointer(mouse,buyPoint,false);
                Check(game.Model.Money==0&&game.Model.Building.Beds==1,"Market UI buys one bed for 100 coins.");
                Teleport(cc,new Vector3(1.5f,.1f,3.5f));yield return Press(keyboard,Key.Digit4);yield return null;
                var buildSlots=Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Where(b=>b.name.StartsWith("Construction Slot ")).OrderBy(b=>b.name).ToArray();
                Check(buildSlots.Length==8&&!Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Any(b=>b.name=="Inventory Slot 0"),"Build mode replaces the farming bar with eight construction slots.");
                Check(buildSlots.Take(6).All(b=>b.GetComponentsInChildren<Image>().Any(i=>i.gameObject.name=="Construction Icon"&&i.sprite!=null)),"Every available construction piece has its generated icon.");
                var slotPoint=RectTransformUtility.WorldToScreenPoint(null,buildSlots[1].transform.position+(Vector3)((RectTransform)buildSlots[1].transform).rect.center);
                yield return Pointer(mouse,slotPoint,true);yield return Pointer(mouse,slotPoint,false);
                Check(builder.ActiveDefinition.isBed&&game.Model.Building.Count==0,"Clicking the bed icon selects it without placing through the UI.");
                var emptyPoint=RectTransformUtility.WorldToScreenPoint(null,buildSlots[7].transform.position+(Vector3)((RectTransform)buildSlots[7].transform).rect.center);
                yield return Pointer(mouse,emptyPoint,true);yield return Pointer(mouse,emptyPoint,false);
                Check(builder.ActiveDefinition.isBed&&game.Model.Building.Count==0,"Empty construction slots do not change selection or place a piece.");
                while(builder.Rotation!=0)yield return Press(keyboard,Key.R);
                yield return Pointer(mouse,Cell(1,5),false);yield return Pointer(mouse,Cell(1,5),true);yield return Pointer(mouse,Cell(1,5),false);
                Check(game.Model.Building.Beds==0&&game.Model.Building.Wood==24&&game.Model.Building.Count==1,"Purchased bed places without spending wood.");
                yield return Press(keyboard,Key.M);var bed=Object.FindObjectsByType<PlacedBlockView>(FindObjectsSortMode.None).Single();
                yield return Pointer(mouse,camera.WorldToScreenPoint(bed.transform.position+new Vector3(0,.1f,.5f)),true);
                Check(builder.IsDragging,"Holding a placed bed in move mode starts a drag.");
                yield return Pointer(mouse,Cell(3,5),true);Check(builder.ValidPreview,"Dragged bed previews at another reachable location.");yield return Pointer(mouse,Cell(3,5),false);
                Check(game.Model.Building.Occupied(3,0,5)&&!game.Model.Building.Occupied(1,0,5)&&game.Model.Building.Wood==24,"Release moves the bed without a refund, duplication or wood cost.");
                string before=JsonUtility.ToJson(game.Model.Snapshot());bed=Object.FindObjectsByType<PlacedBlockView>(FindObjectsSortMode.None).Single();
                yield return Pointer(mouse,camera.WorldToScreenPoint(bed.transform.position+new Vector3(0,.1f,.5f)),true);yield return Pointer(mouse,new Vector2(40,Screen.height-40),true);yield return Pointer(mouse,new Vector2(40,Screen.height-40),false);
                Check(JsonUtility.ToJson(game.Model.Snapshot())==before,"Dropping a dragged bed on UI preserves its original placement.");
                yield return Pointer(mouse,camera.WorldToScreenPoint(bed.transform.position+new Vector3(0,.1f,.5f)),true);yield return Press(keyboard,Key.Escape);yield return Pointer(mouse,Cell(2,5),false);
                Check(Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Any(b=>b.name=="Inventory Slot 0")&&!Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Any(b=>b.name.StartsWith("Construction Slot ")),"Leaving build mode restores the normal farming inventory.");
                Check(!builder.IsDragging&&JsonUtility.ToJson(game.Model.Snapshot())==before,"Escape cancels carrying without losing the bed.");
                var thinHouse=new FarmModel(new[]{game.ActiveCrop.Rules},buildCatalog:game.BuildPieces.Select(d=>d.Rules)).Snapshot();
                thinHouse.building.wood=100;Load(game,thinHouse);
                game.Model.Building.Place("wood_wall",0,0,5,0,out _);
                game.Model.Building.Place("wood_door",3,0,5,0,out _);
                for(int y=0;y<3;y++)game.Model.Building.Place("wood_block",-2,y,5,0,out _);
                game.NotifyTimeAdvanced();Teleport(cc,new Vector3(1.5f,.1f,4f));
                yield return Press(keyboard,Key.Digit4);builder.SelectPiece(5);yield return null;
                yield return Pointer(mouse,Cell(0,5,2.4f),false);
                Check(builder.Level==2&&builder.ValidPreview,"Roof automatically snaps to a thin wall at 2.4m without scrolling.");
                yield return Pointer(mouse,Cell(0,5,2.4f),true);yield return Pointer(mouse,Cell(0,5,2.4f),false);
                Check(game.Model.Building.Blocks.Any(b=>b.pieceId=="wood_roof"&&b.x==0&&b.z==5&&b.level==2),"A real pointer click places the roof above the thin wall.");
                yield return Pointer(mouse,Cell(1,5,2.4f),false);
                Check(builder.Level==2&&builder.ValidPreview,"The next roof panel retains the thin wall height over empty air.");
                yield return Pointer(mouse,Cell(1,5,2.4f),true);yield return Pointer(mouse,Cell(1,5,2.4f),false);
                yield return Pointer(mouse,Cell(3,5,2.4f),false);
                Check(builder.Level==2&&builder.ValidPreview,"A door frame also accepts the automatic roof preview.");
                yield return Pointer(mouse,Cell(3,5,2.4f),true);yield return Pointer(mouse,Cell(3,5,2.4f),false);
                Check(game.Model.Building.Blocks.Count(b=>b.pieceId=="wood_roof"&&b.level==2)==3,"Wall, extension and door roofs all place through real input.");
                yield return Pointer(mouse,Cell(-1,5,3),false);
                Check(builder.Level==3&&builder.ValidPreview,$"Moving from thin walls to a block wall automatically selects 3m. Level={builder.Level}, target={builder.Target}, status={builder.Status}, supports={game.Model.Building.CanSupportRoof(-1,3,5)}.");
                yield return Pointer(mouse,Cell(-1,5,3),true);yield return Pointer(mouse,Cell(-1,5,3),false);
                yield return Pointer(mouse,Cell(1,2,3),false);
                Check(!builder.ValidPreview,"Automatic height does not allow roofs without structural support.");
                yield return Capture(screenshot,"thin-wall-roof");yield return Press(keyboard,Key.Escape);
                var house=new FarmModel(new[]{game.ActiveCrop.Rules},buildCatalog:game.BuildPieces.Select(d=>d.Rules)).Snapshot();house.building.wood=300;house.building.beds=1;house.minuteOfDay=720;Load(game,house);
                var model=game.Model.Building;
                for(int x=0;x<=4;x++)for(int z=3;z<=7;z++)if((x==0||x==4||z==3||z==7)&&!(x==2&&z==3))for(int y=0;y<3;y++)model.Place("wood_block",x,y,z,0,out _);
                model.Place("bed",1,0,5,0,out _);game.NotifyTimeAdvanced();yield return null;
                Teleport(cc,new Vector3(2.5f,.1f,5.5f));camera.orthographicSize=5.3f;camera.transform.position=new Vector3(2.5f,1.3f,5.5f)-camera.transform.forward*14;yield return new WaitForSecondsRealtime(.5f);
                var fade=Object.FindObjectsByType<OccludingWall>(FindObjectsSortMode.None).Where(v=>v.Record.pieceId=="wood_block").ToArray();
                Check(fade.Count(v=>v.Opacity<.5f)>=12,"Tall block-house front columns fade even without floor tiles.");Check(fade.Any(v=>v.Opacity>.99f),"The far side of the block house remains opaque.");
                yield return Capture(screenshot,"block-interior");yield return Press(keyboard,Key.Digit4);builder.SelectPiece(5);yield return null;
                yield return Capture(screenshot,"construction-inventory");
                yield return Pointer(mouse,Cell(1,5,3),false);Check(builder.ValidPreview,"A roof begins beside a three-high wall without filling the room below.");yield return Pointer(mouse,Cell(1,5,3),true);yield return Pointer(mouse,Cell(1,5,3),false);
                yield return Pointer(mouse,Cell(2,5,3),true);yield return Pointer(mouse,Cell(2,5,3),false);
                Check(model.Blocks.Count(b=>b.pieceId=="wood_roof")==2&&!model.Occupied(2,1,5),"Roof extends across empty interior air.");
                for(int x=1;x<=3;x++)for(int z=4;z<=6;z++)if(!model.Blocks.Any(b=>b.pieceId=="wood_roof"&&b.x==x&&b.z==z))model.Place("wood_roof",x,3,z,0,out _);
                game.NotifyTimeAdvanced();yield return Press(keyboard,Key.Escape);yield return new WaitForSecondsRealtime(.5f);
                Check(model.Blocks.Count(b=>b.pieceId=="wood_roof")==9,"Nine roof panels cover the three-by-three room.");
                Check(Object.FindObjectsByType<OccludingWall>(FindObjectsSortMode.None).Where(v=>v.Record.pieceId=="wood_roof").All(v=>v.Opacity<.5f),"The roof also reveals the occupied block room.");
                yield return Capture(screenshot,"roof-interior");string saved=JsonUtility.ToJson(game.Model.Snapshot());game.SaveGame();yield return Press(keyboard,Key.F9);
                Check(saved==JsonUtility.ToJson(game.Model.Snapshot()),"Roof, moved furniture and purchased-bed inventory survive loading.");
                Teleport(cc,new Vector3(7,.1f,1));yield return new WaitForSecondsRealtime(.6f);
                Check(Object.FindObjectsByType<OccludingWall>(FindObjectsSortMode.None).All(v=>v.Opacity>.99f),"Exterior roof and walls become opaque after leaving.");yield return Capture(screenshot,"roof-exterior");
                Debug.Log("FARMER_ROOF_CHECKS_FINISHED");
            }
            finally
            {
                camera.transform.position=oldPosition;camera.orthographicSize=oldSize;InputSystem.RemoveDevice(keyboard);InputSystem.RemoveDevice(mouse);foreach(var d in physical)InputSystem.EnableDevice(d);
                game.Camp.gameObject.SetActive(campActive);Load(game,original);
            }
        }
        private static void Load(FarmGame game,FarmSnapshot snapshot){File.WriteAllText(game.SavePath,JsonUtility.ToJson(snapshot));game.LoadGame();}
        private static void Teleport(CharacterController cc,Vector3 p){cc.enabled=false;cc.transform.position=p;cc.enabled=true;Physics.SyncTransforms();}
        private static Vector2 Cell(int x,int z,float height=.01f)=>Camera.main.WorldToScreenPoint(new Vector3(x+.5f,height,z+.5f));
        private static IEnumerator Press(Keyboard k,Key key){InputSystem.QueueStateEvent(k,new KeyboardState(key));yield return null;yield return null;InputSystem.QueueStateEvent(k,new KeyboardState());yield return null;yield return null;}
        private static IEnumerator Pointer(Mouse m,Vector2 p,bool left){InputSystem.QueueStateEvent(m,new MouseState{position=p,buttons=(ushort)(left?1:0)});yield return null;yield return null;yield return null;}
        private static IEnumerator Capture(string path,string label){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Path.GetDirectoryName(path),Path.GetFileNameWithoutExtension(path)+"-"+label+".png"));yield return new WaitForSecondsRealtime(.3f);}
        private static void Check(bool ok,string message){if(ok)Debug.Log("FARMER_ROOF_CHECK_OK: "+message);else Debug.LogError("FARMER_ROOF_CHECK_FAILED: "+message);}
    }
}
#endif
