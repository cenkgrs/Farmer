#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Farmer
{
    public static class BuildingSmokeChecks
    {
        public static IEnumerator Run(string screenshot)
        {
            var game = Object.FindFirstObjectByType<FarmGame>(); var builder = game.GetComponent<BuildController>();
            if (!game.SavePath.Contains("FarmerQA")) throw new System.InvalidOperationException("Expected isolated save.");
            var original = game.Model.Snapshot(); var cc = game.Player.GetComponent<CharacterController>();
            var physical = InputSystem.devices.Where(d => d.enabled && (d is Mouse || d is Keyboard)).ToArray();
            var keyboard = InputSystem.AddDevice<Keyboard>(); var mouse = InputSystem.AddDevice<Mouse>();
            GameObject obstacle = null;
            try
            {
                foreach (var device in physical) InputSystem.DisableDevice(device);
                var fixture = game.Model.Snapshot(); fixture.building = new BuildingSnapshot { wood = 40, blocks = new BlockRecord[0] };
                fixture.seeds[0].count = 10; fixture.money = 60;
                for (int i = 0; i < fixture.plots.Length; i++) fixture.plots[i] = new PlotRecord { x=i%6-3, z=i/6-3 };
                Directory.CreateDirectory(Path.GetDirectoryName(game.SavePath)); File.WriteAllText(game.SavePath, JsonUtility.ToJson(fixture)); game.LoadGame();
                Teleport(cc, new Vector3(3.4f, .1f, -6.5f));
                InputSystem.QueueStateEvent(mouse, new MouseState { position = new Vector2(-10,-10) });
                yield return KeyPress(keyboard, Key.Digit4);
                Check(game.BuildMode && builder.VisibleBlockCount == 0, "4 enters construction with a clean synchronized view.");
                yield return Pointer(mouse, Cell(1,0,0), false);
                Check(builder.ValidPreview && builder.Target == new Vector3Int(4,0,-7), $"Reachable empty ground has a valid preview. [{builder.Status}, target={builder.Target}]");
                yield return Capture(screenshot, "build-preview");
                yield return KeyPress(keyboard, Key.R);
                Check(builder.Rotation == 1, "R rotates the preview by a quarter turn.");
                yield return Pointer(mouse, Cell(1,0,0), true);
                Check(game.Model.Building.Count == 1 && game.Model.Building.Wood == 38 && builder.VisibleBlockCount == 1, "Click builds one block, spends wood and creates a collider.");
                Check(game.Model.Building.Snapshot().blocks[0].rotation == 1, "Placed rotation is stored.");
                yield return Pointer(mouse, Cell(2,0,0), true);
                Check(game.Model.Building.Count == 1, "Holding the mouse does not accidentally place repeated blocks.");
                yield return Pointer(mouse, Cell(2,0,0), false);
                yield return Pointer(mouse, Cell(2,0,0), true);
                Check(game.Model.Building.Count == 2 && game.Model.Building.Wood == 36, "Adjacent placement works with a new click.");
                yield return Pointer(mouse, Cell(2,0,0), false);
                InputSystem.QueueStateEvent(mouse, new MouseState { position = Cell(1,1,0), scroll = new Vector2(0,120) });
                yield return null; yield return null;
                yield return Pointer(mouse, Cell(1,1,0), false);
                Check(builder.Level == 1 && builder.ValidPreview, $"Wheel selects the supported upper level. [{builder.Status}]");
                yield return Pointer(mouse, Cell(1,1,0), true);
                Check(game.Model.Building.Occupied(4,1,-7) && game.Model.Building.Wood == 34, "Upper block stacks and spends wood once.");
                yield return Pointer(mouse, Cell(3,1,0), false);
                Check(!builder.ValidPreview, "Floating upper blocks get an invalid preview.");
                yield return Pointer(mouse, Cell(3,1,0), true);
                Check(game.Model.Building.Count == 3 && game.Model.Building.Wood == 34, "Invalid placement has no inventory side effect.");
                yield return Pointer(mouse, Cell(3,1,0), false);
                yield return Capture(screenshot, "build-stacked");
                // Hit the upper block's top so the ray selects the actual visible block, not a layer-plane guess.
                Vector2 top = Camera.main.WorldToScreenPoint(BuildController.Center(4,1,-7) + Vector3.up * .48f);
                yield return Pointer(mouse, top, false, true);
                Check(!game.Model.Building.Occupied(4,1,-7) && game.Model.Building.Wood == 36, "Right click removes the hit upper block and refunds its wood.");
                yield return Pointer(mouse, top, false);
                string saved = JsonUtility.ToJson(game.Model.Snapshot()); game.SaveGame();
                yield return KeyPress(keyboard, Key.F9);
                Check(!game.BuildMode && JsonUtility.ToJson(game.Model.Snapshot()) == saved && builder.VisibleBlockCount == 2, "F9 restores blocks, rotation and inventory while exiting build mode.");
                yield return KeyPress(keyboard, Key.Digit4);
                // Reset the height selector with one downward wheel tick.
                InputSystem.QueueStateEvent(mouse, new MouseState { position = Cell(0,0,0), scroll = new Vector2(0,-120) });
                yield return null; yield return null;
                yield return Pointer(mouse, Cell(0,0,0), false);
                Check(builder.Level == 0 && !builder.ValidPreview && builder.Status.Contains("çakış"), "Preview refuses to enclose the player.");
                yield return Pointer(mouse, Cell(0,0,0), true);
                Check(game.Model.Building.Count == 2, "Player overlap cannot spend resources or place a block.");
                yield return Pointer(mouse, Cell(0,0,0), false);
                obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube); obstacle.transform.position = BuildController.Center(3,0,-6); Physics.SyncTransforms();
                yield return Pointer(mouse, Cell(0,0,1), false);
                Check(!builder.ValidPreview && builder.Status.Contains("çakış"), "Other solid objects reject placement.");
                Object.Destroy(obstacle); obstacle = null; yield return null;
                yield return Pointer(mouse, Cell(5,0,4), false);
                Check(!builder.ValidPreview && builder.Status.Contains("yaklaş"), "Distant placement is blocked.");
                Vector2 farmCell = Camera.main.WorldToScreenPoint(game.Selection.Layout.Center(new Vector2Int(0,0), .055f));
                yield return Pointer(mouse, farmCell, true);
                Check(game.Model.PlantedCount == 0 && game.Model.Seeds(game.ActiveCrop.id) == 10, "Build-mode clicks never plant or water the farm.");
                yield return Pointer(mouse, new Vector2(60,Screen.height-60), false);
                yield return Pointer(mouse, new Vector2(60,Screen.height-60), true);
                Check(builder.Target == null && game.Model.Building.Count == 2, "HUD clicks never place blocks.");
                yield return Pointer(mouse, new Vector2(-10,-10), false);
                yield return KeyPress(keyboard, Key.Escape);
                Check(!game.BuildMode && !game.Selection.HideOutline, "Escape restores normal farming mode.");
                Teleport(cc, new Vector3(3.6f,.1f,-6.5f));
                Vector3 before = game.Player.position;
                var motor = game.Player.GetComponent<PlayerMotor>();
                for (int i=0; i<15; i++) motor.Step(new Vector2(1,1), .04f);
                Check(game.Player.position.x > before.x + .02f && game.Player.position.x < 4.1f, "Placed blocks participate in player collision.");
                Teleport(cc, game.Market.position + new Vector3(0,.1f,-1.7f));
                int wood = game.Model.Building.Wood, money = game.Model.Money;
                Check(game.BuyWood() && game.Model.Building.Wood == wood + 10 && game.Model.Money == money - 20, "Market supplies wood through the shared economy.");
                Debug.Log("FARMER_BUILDING_CHECKS_FINISHED");
            }
            finally
            {
                if (obstacle != null) Object.Destroy(obstacle);
                InputSystem.RemoveDevice(keyboard); InputSystem.RemoveDevice(mouse);
                foreach (var device in physical) InputSystem.EnableDevice(device);
                File.WriteAllText(game.SavePath, JsonUtility.ToJson(original)); game.LoadGame();
            }
        }
        private static void Teleport(CharacterController cc, Vector3 position) { cc.enabled=false; cc.transform.position=position; cc.enabled=true; Physics.SyncTransforms(); }
        private static Vector2 Cell(int x,int y,int z) => Camera.main.WorldToScreenPoint(BuildController.Center(x+3,y,z-7) - Vector3.up*.5f);
        private static IEnumerator KeyPress(Keyboard keyboard, Key key)
        {
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(key)); yield return null; yield return null;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState()); yield return null; yield return null;
        }
        private static IEnumerator Pointer(Mouse mouse,Vector2 position,bool left,bool right=false)
        {
            InputSystem.QueueStateEvent(mouse,new MouseState { position=position, buttons=(ushort)((left?1:0)|(right?2:0)) });
            yield return null; yield return null; yield return null;
        }
        private static IEnumerator Capture(string path,string label)
        {
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(Path.GetDirectoryName(path),Path.GetFileNameWithoutExtension(path)+"-"+label+".png"));
            yield return new WaitForSecondsRealtime(.2f);
        }
        private static void Check(bool ok,string message)
        { if(ok) Debug.Log("FARMER_BUILDING_CHECK_OK: "+message); else Debug.LogError("FARMER_BUILDING_CHECK_FAILED: "+message); }
    }
}
#endif
