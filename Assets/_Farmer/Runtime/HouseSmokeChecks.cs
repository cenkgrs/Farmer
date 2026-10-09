#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Farmer
{
    public static class HouseSmokeChecks
    {
        public static IEnumerator Run(string screenshot)
        {
            var game=Object.FindFirstObjectByType<FarmGame>();var builder=game.GetComponent<BuildController>();var cc=game.Player.GetComponent<CharacterController>();
            if(!game.SavePath.Contains("FarmerQA"))throw new System.InvalidOperationException("Expected isolated save.");
            var original=game.Model.Snapshot();var camera=Camera.main;var oldCamera=camera.transform.position;float oldSize=camera.orthographicSize;
            var physical=InputSystem.devices.Where(d=>d.enabled&&(d is Mouse||d is Keyboard)).ToArray();var keyboard=InputSystem.AddDevice<Keyboard>();var mouse=InputSystem.AddDevice<Mouse>();
            try
            {
                foreach(var d in physical)InputSystem.DisableDevice(d);
                var fixture=new FarmModel(new[]{game.ActiveCrop.Rules},buildCatalog:game.BuildPieces.Select(d=>d.Rules)).Snapshot();fixture.building.wood=100;fixture.building.beds=1;fixture.minuteOfDay=720;
                File.WriteAllText(game.SavePath,JsonUtility.ToJson(fixture));game.LoadGame();Teleport(cc,new Vector3(4.5f,.1f,2.5f));
                yield return Press(keyboard,Key.Digit4);
                yield return Select(builder,keyboard,"wood_floor");while(builder.Rotation!=0)yield return Press(keyboard,Key.R);
                yield return Pointer(mouse,Cell(4,4),false);
                Check(builder.ValidPreview,"Floor has a valid world preview.");yield return Pointer(mouse,Cell(4,4),true);
                Check(game.Model.Building.HasFloor(4,4)&&game.Model.Building.Wood==99,"Click lays one floor and spends one wood.");
                yield return Pointer(mouse,Cell(4,4),false);yield return Select(builder,keyboard,"wood_wall");
                Check(builder.ValidPreview,"A thin wall shares the floor cell.");yield return Pointer(mouse,Cell(4,4),true);
                Check(game.Model.Building.Count==2&&game.Model.Building.Wood==97,"Wall occupies the cell edge without replacing the floor.");
                yield return Pointer(mouse,Cell(4,4),false);yield return Select(builder,keyboard,"wood_door");yield return Press(keyboard,Key.R);yield return Press(keyboard,Key.R);
                yield return Pointer(mouse,Cell(4,4),false);Check(builder.ValidPreview,"Door frame previews on the opposite edge.");yield return Pointer(mouse,Cell(4,4),true);
                Check(game.Model.Building.Count==3&&game.Model.Building.Wood==94,"Door placement spends its own cost.");yield return Pointer(mouse,Cell(4,4),false);yield return Press(keyboard,Key.Escape);
                var door=Object.FindFirstObjectByType<DoorView>();Check(door!=null&&!door.IsOpen,"A placed door starts closed.");
                Move(game,Vector3.forward,18);Check(game.Player.position.z<3.8f,"Closed door blocks the actual character controller.");
                yield return Pointer(mouse,camera.WorldToScreenPoint(door.InteractionPoint),false);yield return Press(keyboard,Key.F);
                Check(!door.IsOpen,"Door refuses to swing through the player standing in its path.");
                Teleport(cc,new Vector3(4.5f,.1f,2.5f));yield return Pointer(mouse,camera.WorldToScreenPoint(door.InteractionPoint),false);yield return Press(keyboard,Key.F);yield return new WaitForSecondsRealtime(.6f);
                Check(door.IsOpen,"F opens a nearby targeted door when the swing is clear.");
                Move(game,Vector3.forward,15);Check(game.Player.position.z>4.2f,"The character walks through the open doorway onto the floor.");
                Move(game,Vector3.forward,12);Check(game.Player.position.z<4.8f,"The rear wall blocks passage.");
                Teleport(cc,new Vector3(4.5f,.1f,4.4f));yield return Pointer(mouse,camera.WorldToScreenPoint(door.transform.TransformPoint(new Vector3(.465f,.7f,.5f))),false);yield return Press(keyboard,Key.F);yield return new WaitForSecondsRealtime(.6f);
                Check(!door.IsOpen,"F closes the door from the room side.");
                yield return Press(keyboard,Key.F);yield return new WaitForSecondsRealtime(.6f);
                Check(door.IsOpen,"The same frame remains targetable while open or closed.");
                string saved=JsonUtility.ToJson(game.Model.Snapshot());game.SaveGame();yield return Press(keyboard,Key.F9);door=Object.FindFirstObjectByType<DoorView>();
                Check(JsonUtility.ToJson(game.Model.Snapshot())==saved&&door.IsOpen,"Layered floor, wall and open door survive save/load.");
                Teleport(cc,new Vector3(4.5f,.1f,4.3f));yield return Press(keyboard,Key.Digit5);yield return Pointer(mouse,Cell(4,4),true);
                Check(game.Model.PlotCount==0,"Hoe cannot cultivate a wooden floor.");yield return Pointer(mouse,Cell(4,4),false);
                yield return Press(keyboard,Key.Digit4);
                var wall=Object.FindObjectsByType<PlacedBlockView>(FindObjectsSortMode.None).Single(v=>v.Record.pieceId=="wood_wall");
                yield return Pointer(mouse,camera.WorldToScreenPoint(wall.transform.TransformPoint(new Vector3(0,1,.5f))),false,true);
                Check(game.Model.Building.Count==2&&game.Model.Building.HasFloor(4,4)&&game.Model.Building.Wood==96,"Right-click removes only the hit wall and refunds it, leaving the floor and door.");
                yield return Pointer(mouse,new Vector2(-10,-10),false);yield return Press(keyboard,Key.Escape);
                // Build a complete room fixture using the same model rules; UI placement was exercised above.
                var room=new FarmModel(new[]{game.ActiveCrop.Rules},buildCatalog:game.BuildPieces.Select(d=>d.Rules)).Snapshot();room.building.wood=100;room.building.beds=1;room.minuteOfDay=720;
                File.WriteAllText(game.SavePath,JsonUtility.ToJson(room));game.LoadGame();var model=game.Model.Building;
                for(int x=0;x<3;x++)for(int z=4;z<7;z++)model.Place("wood_floor",x,0,z,0,out _);
                for(int x=0;x<3;x++){model.Place("wood_wall",x,0,6,0,out _);model.Place(x==1?"wood_door":"wood_wall",x,0,4,2,out _);}
                for(int z=4;z<7;z++){model.Place("wood_wall",0,0,z,3,out _);model.Place("wood_wall",2,0,z,1,out _);}
                game.NotifyTimeAdvanced();yield return null;
                Teleport(cc,new Vector3(2.5f,.1f,5.5f));yield return Press(keyboard,Key.Digit4);yield return Select(builder,keyboard,"bed");
                while(builder.Rotation!=0)yield return Press(keyboard,Key.R);
                yield return Pointer(mouse,Cell(0,5),false);
                Check(builder.ValidPreview,"A bed can be previewed inside perimeter walls on two floor tiles.");
                yield return Pointer(mouse,Cell(0,5),true);yield return Pointer(mouse,Cell(0,5),false);yield return Press(keyboard,Key.Escape);
                Check(model.Blocks.Any(b=>b.pieceId=="bed")&&model.Count==22,"A three-by-three room has nine floor tiles, twelve perimeter pieces and an indoor bed.");
                Teleport(cc,new Vector3(1.5f,.1f,5.5f));yield return null;Check(game.NearCamp,"The indoor bed retains its sleep interaction.");
                int day=game.Model.Day;yield return Press(keyboard,Key.N);Check(game.Model.Day==day+1,"N sleeps at the room's bed.");
                game.Model.AdvanceMinutes(360);camera.orthographicSize=4.2f;camera.transform.position=new Vector3(1.5f,1,5.5f)-camera.transform.forward*12;
                yield return new WaitForSecondsRealtime(.5f);
                var visibility=game.GetComponent<HouseVisibility>();
                Check(visibility.FadedCount>=6,"Camera-facing front and side walls fade together around the occupied room.");
                var visibleWalls=Object.FindObjectsByType<OccludingWall>(FindObjectsSortMode.None);
                Check(visibleWalls.Any(w=>w.Opacity>.99f),"Rear walls remain opaque while the interior is revealed.");
                Check(visibleWalls.Where(w=>w.Opacity<.5f).All(w=>w.GetComponentsInChildren<Collider>().All(c=>c.enabled)),"Faded walls retain their physical colliders.");
                yield return Capture(screenshot,"room");
                Teleport(cc,new Vector3(6.5f,.1f,2.5f));yield return new WaitForSecondsRealtime(.5f);
                Check(visibility.FadedCount==0,"Walls become opaque again after leaving the room and its camera sightline.");
                yield return Capture(screenshot,"exterior");Teleport(cc,new Vector3(1.5f,.1f,5.5f));yield return new WaitForSecondsRealtime(.5f);
                string roomSaved=JsonUtility.ToJson(game.Model.Snapshot());game.SaveGame();yield return Press(keyboard,Key.F9);
                Check(builder.VisibleBlockCount==22&&JsonUtility.ToJson(game.Model.Snapshot())==roomSaved,"The complete room and furniture restore together.");
                model=game.Model.Building;
                // Missing floor tiles must not fragment the detected room.
                model.Remove(model.Blocks.First(b=>b.pieceId=="wood_floor"&&b.x==1&&b.z==5),out _);
                for(int x=0;x<3;x++)for(int z=4;z<7;z++)model.Place("wood_roof",x,2,z,0,out _);
                game.NotifyTimeAdvanced();yield return new WaitForSecondsRealtime(.6f);
                Check(visibility.Indoors&&visibility.FadedCount>=15,"A room with a floor gap reveals all nine roof panels and front walls.");
                var follow=camera.GetComponent<ExplorationCamera>();follow.enabled=true;
                yield return new WaitForSecondsRealtime(1.5f);
                float indoorSize=camera.orthographicSize;
                var screen=camera.WorldToViewportPoint(game.Player.position+Vector3.up*.8f);
                Check(Vector2.Distance(new Vector2(screen.x,screen.y),Vector2.one*.5f)<.005f,"Indoor camera keeps the character centered.");
                yield return Wheel(mouse,120);
                yield return new WaitForSecondsRealtime(1.2f);
                Check(camera.orthographicSize<indoorSize-.2f,"Mouse wheel zooms into the occupied room.");
                yield return Wheel(mouse,-120);
                yield return new WaitForSecondsRealtime(1.2f);
                Check(Mathf.Abs(camera.orthographicSize-indoorSize)<.05f,"Reverse wheel restores the previous interior zoom.");
                float uiSize=camera.orthographicSize;
                InputSystem.QueueStateEvent(mouse,new MouseState {position=new Vector2(50,Screen.height-50),scroll=new Vector2(0,120)});
                yield return null;yield return null;
                InputSystem.QueueStateEvent(mouse,new MouseState {position=new Vector2(50,Screen.height-50)});
                yield return new WaitForSecondsRealtime(.6f);
                Check(Mathf.Abs(camera.orthographicSize-uiSize)<.05f,"Scrolling over the HUD does not zoom the world.");
                yield return Capture(screenshot,"interior-camera");
                game.SetBuildMode(true);yield return new WaitForSecondsRealtime(1.5f);
                Check(camera.orthographicSize>indoorSize+.5f,"Build mode restores the wide camera inside the house.");
                builder.SelectPiece(System.Array.FindIndex(game.BuildPieces,d=>d.id=="wood_block"));
                float wide=camera.orthographicSize;int oldLevel=builder.Level;
                yield return Wheel(mouse,120);yield return new WaitForSecondsRealtime(.6f);
                Check(builder.Level==oldLevel+1&&Mathf.Abs(camera.orthographicSize-wide)<.05f,"Build wheel changes height without zooming.");
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.LeftCtrl));yield return null;
                int height=builder.Level;yield return Wheel(mouse,120);yield return new WaitForSecondsRealtime(1.2f);
                Check(builder.Level==height&&camera.orthographicSize<wide-.2f,"Ctrl plus wheel zooms in build mode without changing height.");
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
                yield return Capture(screenshot,"interior-build-camera");
                game.SetBuildMode(false);Teleport(cc,new Vector3(8.5f,.1f,2.5f));yield return new WaitForSecondsRealtime(1.5f);
                screen=camera.WorldToViewportPoint(game.Player.position+Vector3.up*.8f);
                Check(!visibility.Indoors&&Vector2.Distance(new Vector2(screen.x,screen.y),Vector2.one*.5f)<.005f,"Outdoor camera follows immediately and stays centered.");
                follow.enabled=false;
                Debug.Log("FARMER_HOUSE_CHECKS_FINISHED");
            }
            finally
            {
                camera.GetComponent<ExplorationCamera>().enabled=false;
                camera.transform.position=oldCamera;camera.orthographicSize=oldSize;
                InputSystem.RemoveDevice(keyboard);InputSystem.RemoveDevice(mouse);foreach(var d in physical)InputSystem.EnableDevice(d);
                File.WriteAllText(game.SavePath,JsonUtility.ToJson(original));game.LoadGame();
            }
        }
        private static void Move(FarmGame game,Vector3 direction,int steps)
        {
            var f=Camera.main.transform.forward;f.y=0;f.Normalize();var r=Camera.main.transform.right;r.y=0;r.Normalize();var input=new Vector2(Vector3.Dot(direction,r),Vector3.Dot(direction,f));
            for(int i=0;i<steps;i++)game.Player.GetComponent<PlayerMotor>().Step(input,.04f);
        }
        private static IEnumerator Select(BuildController builder,Keyboard keyboard,string id)
        {
            for(int i=0;i<builder.GetComponent<FarmGame>().BuildPieces.Length&&builder.ActiveDefinition.id!=id;i++)yield return Press(keyboard,Key.Q);
        }
        private static void Teleport(CharacterController cc,Vector3 p){cc.enabled=false;cc.transform.position=p;cc.enabled=true;Physics.SyncTransforms();}
        private static Vector2 Cell(int x,int z)=>Camera.main.WorldToScreenPoint(new Vector3(x+.5f,.01f,z+.5f));
        private static IEnumerator Press(Keyboard k,Key key){InputSystem.QueueStateEvent(k,new KeyboardState(key));yield return null;yield return null;InputSystem.QueueStateEvent(k,new KeyboardState());yield return null;yield return null;}
        private static IEnumerator Wheel(Mouse mouse,float amount)
        {
            InputSystem.QueueStateEvent(mouse,new MouseState {position=new Vector2(Screen.width*.5f,Screen.height*.5f),scroll=new Vector2(0,amount)});
            yield return null;yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState {position=new Vector2(Screen.width*.5f,Screen.height*.5f)});
            yield return null;
        }
        private static IEnumerator Pointer(Mouse mouse,Vector2 p,bool left,bool right=false){InputSystem.QueueStateEvent(mouse,new MouseState{position=p,buttons=(ushort)((left?1:0)|(right?2:0))});yield return null;yield return null;yield return null;}
        private static IEnumerator Capture(string path,string label){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Path.GetDirectoryName(path),Path.GetFileNameWithoutExtension(path)+"-"+label+".png"));yield return new WaitForSecondsRealtime(.3f);}
        private static void Check(bool ok,string message){if(ok)Debug.Log("FARMER_HOUSE_CHECK_OK: "+message);else Debug.LogError("FARMER_HOUSE_CHECK_FAILED: "+message);}
    }
}
#endif
