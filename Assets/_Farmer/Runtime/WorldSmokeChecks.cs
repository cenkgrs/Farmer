#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Farmer
{
    public static class WorldSmokeChecks
    {
        public static IEnumerator Run(string screenshot)
        {
            var game=Object.FindFirstObjectByType<FarmGame>(); var builder=game.GetComponent<BuildController>(); var clock=game.GetComponent<DayNightCycle>();
            if (!game.SavePath.Contains("FarmerQA")) throw new System.InvalidOperationException("Expected isolated save.");
            var original=game.Model.Snapshot(); var cc=game.Player.GetComponent<CharacterController>();
            var physical=InputSystem.devices.Where(d=>d.enabled&&(d is Keyboard||d is Mouse)).ToArray();
            var keyboard=InputSystem.AddDevice<Keyboard>();var mouse=InputSystem.AddDevice<Mouse>();
            try
            {
                foreach(var d in physical) InputSystem.DisableDevice(d);
                var fresh=new FarmModel(new[]{game.ActiveCrop.Rules},buildCatalog:game.BuildPieces.Select(d=>d.Rules));
                fresh.BuySeeds(game.ActiveCrop.id,2,out _);
                File.WriteAllText(game.SavePath,JsonUtility.ToJson(fresh.Snapshot())); game.LoadGame(); clock.ClockPaused=true;
                Check(Object.FindObjectsByType<InventoryIcon>(FindObjectsSortMode.None).Length==7 && GameObject.Find("Inventory Bar").GetComponent<RectTransform>().rect.width<500,"Compact inventory has seven illustrated slots.");
                Check(!Object.FindObjectsByType<TextMesh>(FindObjectsSortMode.None).Any(t=>t.text=="PAZAR"||t.text=="YATAK"),"Market and bed have no floating world labels.");
                Check(clock.RealMinutesPerDay==10,"A full 24-hour cycle lasts ten real minutes.");
                Teleport(cc,new Vector3(-1.2f,.1f,5.5f));yield return null;
                Check(game.Model.PlotCount==0 && !Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Any(r=>r.name.StartsWith("Tilled Soil")),"New game contains no automatic farm plots.");
                yield return Pointer(mouse,Cell(0,5),false);yield return Pointer(mouse,Cell(0,5),true);
                Check(game.Model.PlotCount==0 && game.Model.Seeds(game.ActiveCrop.id)==2,"Untilled ground refuses seeds without consuming inventory.");
                yield return Pointer(mouse,Cell(0,5),false);yield return Press(keyboard,Key.Digit5);
                Check(game.Model.EquippedItem==FarmItem.Hoe && GameObject.Find("Held Hoe")!=null,"5 equips a visible hoe.");
                yield return Pointer(mouse,Cell(0,5),true);
                Check(game.Model.IndexAt(0,5)==0 && game.Model.PlotCount==1 && GameObject.Find("Tilled Soil 0")!=null,"Hoe prepares visible soil outside the former farm.");
                yield return Capture(screenshot,"hoe");
                var camera=Camera.main; Vector3 oldPosition=camera.transform.position; float oldSize=camera.orthographicSize;
                camera.orthographicSize=2.1f;camera.transform.position=game.Player.position+Vector3.up*.8f-camera.transform.forward*10;
                yield return new WaitForSecondsRealtime(.55f);yield return Capture(screenshot,"hoe-close");
                camera.transform.position=oldPosition;camera.orthographicSize=oldSize;
                yield return Pointer(mouse,Cell(1,5),true);
                Check(game.Model.PlotCount==1,"A held hoe click does not accidentally till extra cells.");
                yield return Pointer(mouse,Cell(0,5),false);yield return Pointer(mouse,Cell(0,5),true);
                Check(game.Model.PlotCount==1,"Repeated tilling preserves the same world cell.");
                yield return Pointer(mouse,Cell(0,5),false);yield return Press(keyboard,Key.Digit1);yield return Pointer(mouse,Cell(0,5),true);
                Check(game.Model.PlantedCount==1 && game.Model.Seeds(game.ActiveCrop.id)==1,"Newly tilled world soil accepts the usual seed tool.");
                yield return Pointer(mouse,Cell(0,5),false);yield return Press(keyboard,Key.Digit2);yield return Pointer(mouse,Cell(0,5),true);
                Check(game.Model.Plot(0).watered && game.WateringActive,"Continuous watering works on the new world plot.");
                yield return Pointer(mouse,Cell(0,5),false);yield return Press(keyboard,Key.Digit4);yield return Pointer(mouse,Cell(0,5),false);
                while(builder.Rotation!=0) yield return Press(keyboard,Key.R);
                Check(!builder.ValidPreview && builder.Status.Contains("hasat"),"A planted crop cannot be buried under a structure.");
                yield return Pointer(mouse,Cell(0,4),false);
                Check(builder.ValidPreview,"Building preview works outside the old construction area.");
                yield return Pointer(mouse,Cell(0,4),true);
                Check(game.Model.Building.Occupied(0,0,4),"A wood block is placed freely at world coordinates.");
                yield return Pointer(mouse,Cell(0,4),false);yield return Press(keyboard,Key.Digit5);yield return Pointer(mouse,Cell(0,4),true);
                Check(game.Model.IndexAt(0,4)<0,"Hoe cannot create a farm under an existing structure.");
                yield return Pointer(mouse,Cell(0,4),false);yield return Press(keyboard,Key.Digit4);yield return Press(keyboard,Key.Q);
                yield return Pointer(mouse,Cell(1,5),false);
                Check(builder.ActiveDefinition.isBed && builder.ValidPreview,"Q selects a two-cell bed with valid ground support.");
                yield return Pointer(mouse,Cell(1,5),true);
                Check(game.Model.Building.Blocks.Any(b=>b.pieceId=="bed"&&b.x==1&&b.z==5)&&game.Model.Building.Occupied(1,0,6),"Placed bed reserves its entire footprint.");
                yield return Pointer(mouse,Cell(1,5),false);yield return Press(keyboard,Key.Escape);
                int day=game.Model.Day; double minute=game.Model.MinuteOfDay;
                yield return Press(keyboard,Key.N);
                Check(game.Model.Day==day && game.Model.MinuteOfDay==minute && !game.NearCamp,"N away from a bed cannot skip time.");
                Teleport(cc,new Vector3(.3f,.1f,6.5f));yield return null;
                Check(game.NearCamp,"A player-built bed offers sleep far from the starting bed.");
                game.Model.AdvanceMinutes(1081); game.NotifyTimeAdvanced();yield return null;
                int nightDay=game.Model.Day,growth=game.Model.Plot(0).growth;
                yield return Press(keyboard,Key.N);
                Check(game.Model.Day==nightDay&&game.Model.MinuteOfDay==360&&game.Model.Plot(0).growth==growth,"After-midnight sleep reaches 06:00 without a second growth tick.");
                game.Model.AdvanceMinutes(360);yield return null;
                Check(clock.Daylight>.99f,"Noon uses full daylight.");yield return Capture(screenshot,"day");
                game.Model.AdvanceMinutes(630);yield return null;
                Check(clock.Daylight<.01f,"Night uses visibly reduced lighting.");yield return Capture(screenshot,"night");
                double before=game.Model.MinuteOfDay;clock.ClockPaused=false;yield return new WaitForSecondsRealtime(1.3f);clock.ClockPaused=true;
                Check(game.Model.MinuteOfDay>before+1,"The normal runtime clock advances without sleeping or pressing N.");
                game.Model.AdvanceMinutes(1439.6-game.Model.MinuteOfDay);int previousDay=game.Model.Day;
                clock.ClockPaused=false;yield return new WaitForSecondsRealtime(1.2f);clock.ClockPaused=true;
                Check(game.Model.Day==previousDay+1&&game.Model.Plot(0).growth==growth+1,"An automatic midnight advances the date and grows the crop exactly once.");
                var saved=game.Model.Snapshot();game.SaveGame();yield return Press(keyboard,Key.F9);
                var loaded=game.Model.Snapshot();
                // JsonUtility may round a double by one ULP. Compare only time with a sub-microsecond tolerance.
                double clockDifference=System.Math.Abs(loaded.minuteOfDay-saved.minuteOfDay);
                loaded.minuteOfDay=saved.minuteOfDay;
                bool sameState=JsonUtility.ToJson(loaded)==JsonUtility.ToJson(saved);
                Check(clockDifference<1e-9 && sameState && builder.VisibleBlockCount==2 && game.NearCamp,$"Reload preserves the clock, world plot, structures and functional bed. [clockDelta={clockDifference:R}, same={sameState}, views={builder.VisibleBlockCount}, near={game.NearCamp}]");
                Debug.Log("FARMER_WORLD_CHECKS_FINISHED");
            }
            finally
            {
                clock.ClockPaused=true;InputSystem.RemoveDevice(keyboard);InputSystem.RemoveDevice(mouse);
                foreach(var d in physical) InputSystem.EnableDevice(d);
                File.WriteAllText(game.SavePath,JsonUtility.ToJson(original));game.LoadGame();
            }
        }
        private static void Teleport(CharacterController cc,Vector3 position){cc.enabled=false;cc.transform.position=position;cc.enabled=true;Physics.SyncTransforms();}
        private static Vector2 Cell(int x,int z)=>Camera.main.WorldToScreenPoint(new Vector3(x+.5f,.055f,z+.5f));
        private static IEnumerator Press(Keyboard keyboard,Key key){InputSystem.QueueStateEvent(keyboard,new KeyboardState(key));yield return null;yield return null;InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;yield return null;}
        private static IEnumerator Pointer(Mouse mouse,Vector2 position,bool left){InputSystem.QueueStateEvent(mouse,new MouseState{position=position,buttons=(ushort)(left?1:0)});yield return null;yield return null;yield return null;}
        private static IEnumerator Capture(string path,string label){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Path.GetDirectoryName(path),Path.GetFileNameWithoutExtension(path)+"-"+label+".png"));yield return new WaitForSecondsRealtime(.2f);}
        private static void Check(bool ok,string message){if(ok)Debug.Log("FARMER_WORLD_CHECK_OK: "+message);else Debug.LogError("FARMER_WORLD_CHECK_FAILED: "+message);}
    }
}
#endif
