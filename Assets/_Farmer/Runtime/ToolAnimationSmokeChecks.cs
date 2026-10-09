#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Farmer
{
    public static class ToolAnimationSmokeChecks
    {
        public static IEnumerator Run(string screenshot)
        {
            var game = Object.FindFirstObjectByType<FarmGame>();
            if (!game.SavePath.Contains("FarmerQA")) throw new System.InvalidOperationException("Expected isolated save.");
            var original = game.Model.Snapshot();
            float originalTimeScale=Time.timeScale;
            var motor = game.Player.GetComponent<PlayerMotor>();
            var driver = game.Player.GetComponentInChildren<FarmerAnimator>();
            var animator = driver.GetComponent<Animator>();
            var cc = game.Player.GetComponent<CharacterController>();
            var camera = Camera.main; var position = camera.transform.position; var rotation = camera.transform.rotation; float size = camera.orthographicSize;
            var physical = InputSystem.devices.Where(d => d.enabled && (d is Mouse || d is Keyboard)).ToArray();
            var keyboard = InputSystem.AddDevice<Keyboard>(); var mouse = InputSystem.AddDevice<Mouse>();
            try
            {
                foreach (var device in physical) InputSystem.DisableDevice(device);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                InputSystem.QueueStateEvent(mouse, new MouseState { position = new Vector2(-10, -10) });
                var fixture = game.Model.Snapshot(); fixture.seeds[0].count = 5;
                for (int i = 0; i < fixture.plots.Length; i++) fixture.plots[i] = new PlotRecord { x=i%6-3, z=i/6-3 };
                foreach (int i in new[] { 0, 1 }) fixture.plots[i].cropId = game.ActiveCrop.id;
                Directory.CreateDirectory(Path.GetDirectoryName(game.SavePath));
                File.WriteAllText(game.SavePath, JsonUtility.ToJson(fixture)); game.LoadGame();
                cc.enabled = false; game.Player.position = new Vector3(-1.5f, .1f, -3.7f); cc.enabled = true;
                motor.Visual.rotation = Quaternion.identity; Physics.SyncTransforms();
                game.Equip(FarmItem.WateringCan);
                yield return new WaitForSecondsRealtime(.8f);
                var fingerBase = animator.GetBoneTransform(HumanBodyBones.RightIndexProximal);
                var fingerMid = animator.GetBoneTransform(HumanBodyBones.RightIndexIntermediate);
                var fingerEnd = animator.GetBoneTransform(HumanBodyBones.RightIndexDistal);
                float curl = Vector3.Angle(fingerMid.position - fingerBase.position, fingerEnd.position - fingerMid.position);
                Check(curl > 25, $"Index finger actually curls around the grip (angle={curl:F1}).");
                Vector3 body = game.Player.position;
                yield return Pointer(mouse, Cell(game, 1), true);
                yield return new WaitForSecondsRealtime(.3f);
                Check(game.WateringActive && driver.PourWeight > .95f && driver.WaterVisible, "Held watering raises the arm, tips the can and starts water.");
                Check(Vector3.Angle(driver.ToolSocket.up, Vector3.up) > 35, "Can visibly tilts from its upright carrying pose.");
                Check(Vector2.Distance(new Vector2(body.x, body.z), new Vector2(game.Player.position.x, game.Player.position.z)) < .005f, "Action pose does not move the collision root.");
                Time.timeScale=.1f;
                var oldAim=driver.VisualActionTarget;var nextAim=game.PlotCenter(2,.08f);
                yield return Pointer(mouse,Cell(game,2),true);
                Check(Vector3.Distance(driver.VisualActionTarget,oldAim)>.001f && Vector3.Distance(driver.VisualActionTarget,nextAim)>.02f,"Watering aim travels between cells instead of snapping to the next center.");
                Time.timeScale=originalTimeScale;
                yield return new WaitForSecondsRealtime(.7f);
                Check(Vector3.Distance(driver.VisualActionTarget,nextAim)<.025f,"Smoothed watering aim settles on the selected cell.");
                yield return Pointer(mouse,Cell(game,1),true);yield return new WaitForSecondsRealtime(.5f);
                yield return Capture(screenshot, "watering-wide");
                Vector3 target = game.Player.position + Vector3.up * .8f;
                camera.transform.position = target + new Vector3(4, 2.8f, 5); camera.transform.LookAt(target); camera.orthographicSize = 1.8f;
                yield return Pointer(mouse, Cell(game, 1), true);
                yield return Capture(screenshot, "watering-close");
                Vector3 palmFocus = driver.ToolSocket.position;
                camera.transform.position = palmFocus + new Vector3(3, 1.6f, 4); camera.transform.LookAt(palmFocus); camera.orthographicSize = .65f;
                yield return Pointer(mouse, new Vector2(-10, -10), false);
                yield return Capture(screenshot, "grip-detail");
                camera.transform.SetPositionAndRotation(position, rotation); camera.orthographicSize = size;
                yield return Pointer(mouse, Cell(game, 0), true);
                yield return new WaitForSecondsRealtime(.25f);
                Check(game.Model.Plot(0).watered && game.Model.Plot(1).watered, "Animated held drag waters multiple cells.");
                Vector3 beforeWalk = game.Player.position;
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D));
                yield return new WaitForSecondsRealtime(.16f);
                Check(Vector3.Distance(game.Player.position, beforeWalk) > .15f && animator.GetFloat("MoveSpeed") > .2f && driver.PourWeight > .8f, "Locomotion continues during watering.");
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                yield return Pointer(mouse, Cell(game, 0), false);
                Check(!game.WateringActive && driver.PourWeight > 0 && driver.WaterVisible, "Release leaves a brief lowering and water tail.");
                yield return new WaitForSecondsRealtime(.8f);
                Check(driver.PourWeight == 0 && !driver.WaterVisible && Vector3.Angle(driver.ToolSocket.up, Vector3.up) < 1, "Water and can return fully to rest.");
                yield return Pointer(mouse, Cell(game, 0), true);
                yield return new WaitForSecondsRealtime(.25f);
                game.Equip(FarmItem.Sickle); yield return null; yield return new WaitForEndOfFrame();
                Check(driver.PourWeight == 0 && !driver.WaterVisible && !driver.HarvestActive, "Tool switch cancels the old pose and water.");
                yield return Pointer(mouse, Cell(game, 0), false);
                var ripe = game.Model.Snapshot();
                ripe.plots[0].growth = ripe.plots[1].growth = game.ActiveCrop.wateredDays;
                File.WriteAllText(game.SavePath, JsonUtility.ToJson(ripe)); game.LoadGame();
                int produce = game.Model.Produce(game.ActiveCrop.id);
                target = game.Player.position + Vector3.up * .8f;
                camera.transform.position = target + new Vector3(4, 2.8f, 5); camera.transform.LookAt(target); camera.orthographicSize = 1.8f;
                Time.timeScale=.1f;
                yield return Pointer(mouse, Cell(game, 1), true);
                Check(driver.HarvestActive && game.Model.Produce(game.ActiveCrop.id) == produce + 1, "Successful harvest starts one swing and grants one crop immediately.");
                yield return new WaitForSeconds(.11f);
                Check(Vector3.Angle(driver.ToolSocket.up, Vector3.up) > 15, "Sickle rotates through its cutting arc.");
                yield return Capture(screenshot, "harvest-cut");
                camera.transform.SetPositionAndRotation(position, rotation); camera.orthographicSize = size;
                yield return new WaitForSeconds(.6f);
                Check(!driver.HarvestActive && game.Model.Produce(game.ActiveCrop.id) == produce + 1, "Held mouse cannot repeat harvest or the swing.");
                yield return Pointer(mouse, Cell(game, 1), false);
                yield return Pointer(mouse, Cell(game, 1), true);
                Check(!driver.HarvestActive, "Invalid harvest does not play a success animation.");
                yield return Pointer(mouse, Cell(game, 0), false);
                yield return Pointer(mouse, Cell(game, 0), true);
                Check(driver.HarvestActive, "Next ripe cell can be harvested with a fresh click.");
                Time.timeScale=originalTimeScale;
                game.LoadGame(); yield return null;
                Check(!driver.HarvestActive && !driver.WaterVisible, "Loading a save clears transient action poses.");
                yield return Pointer(mouse, Cell(game, 0), false);
                game.Equip(FarmItem.WateringCan);
                yield return Pointer(mouse, Cell(game, 0), true);
                yield return new WaitForSecondsRealtime(.25f);
                game.SendMessage("OnApplicationFocus", false); driver.SendMessage("OnApplicationFocus", false);
                yield return null; yield return new WaitForEndOfFrame();
                Check(driver.PourWeight == 0 && !driver.WaterVisible, "Focus-loss callbacks cancel water and action immediately.");
                yield return Pointer(mouse, Cell(game, 0), false);
                yield return Pointer(mouse, new Vector2(60, Screen.height - 60), true);
                yield return Pointer(mouse, Cell(game, 0), true);
                Check(!game.WateringActive && driver.PourWeight == 0, "UI drag cannot start a watering animation.");
                yield return Pointer(mouse, Cell(game, 0), false);
                yield return Pointer(mouse, Cell(game, 0), true);
                yield return new WaitForSecondsRealtime(.25f);
                game.enabled = false; yield return null; yield return new WaitForEndOfFrame();
                Check(!driver.WaterVisible && driver.PourWeight == 0, "Disabling gameplay clears the visual action.");
                game.enabled = true;
                foreach (var item in new[] { FarmItem.Sickle, FarmItem.Hoe })
                {
                    game.Equip(item);
                    yield return new WaitForSecondsRealtime(.8f);
                    var hand = animator.GetBoneTransform(HumanBodyBones.RightHand);
                    float wrist = Vector3.Angle(hand.position - animator.GetBoneTransform(HumanBodyBones.RightLowerArm).position,
                        animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal).position - hand.position);
                    Check(wrist < 55, $"{item} relaxed wrist follows the forearm ({wrist:F1} degrees).");
                    Check(Vector3.Distance(driver.ToolSocket.position, hand.position) < .12f, $"{item} grip stays in the palm.");
                    target = game.Player.position + Vector3.up * .85f;
                    camera.transform.position = target + new Vector3(4, 2.8f, 5); camera.transform.LookAt(target); camera.orthographicSize = 1.35f;
                    yield return Capture(screenshot, item.ToString().ToLowerInvariant() + "-carry");
                    camera.transform.position = driver.ToolSocket.position + new Vector3(3, 1.6f, 4); camera.transform.LookAt(driver.ToolSocket.position); camera.orthographicSize = .48f;
                    yield return Capture(screenshot, item.ToString().ToLowerInvariant() + "-grip");
                    driver.SendMessage("OnHoe", game.Player.position + motor.Visual.forward);
                    Time.timeScale = .1f;
                    yield return new WaitForSeconds(.16f);
                    camera.transform.position = target + new Vector3(4, 2.8f, 5); camera.transform.LookAt(target); camera.orthographicSize = 1.35f;
                    yield return Capture(screenshot, item.ToString().ToLowerInvariant() + "-swing");
                    Time.timeScale = originalTimeScale;
                    yield return new WaitForSecondsRealtime(.7f);
                    Check(!driver.HarvestActive, $"{item} returns to carry after the action.");
                }
                Debug.Log("FARMER_TOOL_ANIMATION_CHECKS_FINISHED");
            }
            finally
            {
                Time.timeScale=originalTimeScale;
                game.enabled = true;
                InputSystem.RemoveDevice(keyboard); InputSystem.RemoveDevice(mouse);
                foreach (var device in physical) InputSystem.EnableDevice(device);
                camera.transform.SetPositionAndRotation(position, rotation); camera.orthographicSize = size;
                File.WriteAllText(game.SavePath, JsonUtility.ToJson(original)); game.LoadGame();
            }
        }
        private static Vector2 Cell(FarmGame game, int i) => Camera.main.WorldToScreenPoint(game.Selection.Layout.Center(new Vector2Int(i % game.Model.Width, i / game.Model.Width), .055f));
        private static IEnumerator Pointer(Mouse mouse, Vector2 position, bool held)
        {
            InputSystem.QueueStateEvent(mouse, new MouseState { position = position, buttons = (ushort)(held ? 1 : 0) });
            yield return null; yield return null;
        }
        private static IEnumerator Capture(string path, string label)
        {
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + "-" + label + ".png"));
            yield return new WaitForSecondsRealtime(.2f);
        }
        private static void Check(bool ok, string message)
        { if (ok) Debug.Log("FARMER_TOOL_ANIMATION_CHECK_OK: " + message); else Debug.LogError("FARMER_TOOL_ANIMATION_CHECK_FAILED: " + message); }
    }
}
#endif
