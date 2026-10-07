#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Farmer
{
    public static class WateringSmokeChecks
    {
        public static IEnumerator Run()
        {
            var game = Object.FindFirstObjectByType<FarmGame>();
            var controller = game.Player.GetComponent<CharacterController>();
            // Physical mouse motion must not replace the synthetic held pointer during checks.
            var physicalInput = InputSystem.devices.Where(d => d.enabled && (d is Mouse || d is Keyboard)).ToArray();
            var keyboard = InputSystem.AddDevice<Keyboard>();
            var mouse = InputSystem.AddDevice<Mouse>();
            var original = game.Model.Snapshot();
            int changes = 0;
            System.Action countChange = () => changes++;
            try
            {
                foreach (var device in physicalInput) InputSystem.DisableDevice(device);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                InputSystem.QueueStateEvent(mouse, new MouseState());
                yield return null; yield return null;
                // Explicit isolated smoke fixture; never run against a normal player's save.
                if (!game.SavePath.Contains("FarmerQA")) throw new System.InvalidOperationException("Expected isolated smoke save.");
                Directory.CreateDirectory(Path.GetDirectoryName(game.SavePath));
                var fixture = game.Model.Snapshot(); fixture.seeds[0].count = 10;
                for (int i = 0; i < fixture.plots.Length; i++) fixture.plots[i] = new PlotRecord();
                foreach (int i in new[] { 0, 1, 3 }) fixture.plots[i].cropId = game.ActiveCrop.id;
                File.WriteAllText(game.SavePath, JsonUtility.ToJson(fixture)); game.LoadGame();
                controller.enabled = false; controller.transform.position = new Vector3(-0.5f, 0.1f, -3.8f); controller.enabled = true;
                Physics.SyncTransforms(); game.Equip(FarmItem.WateringCan);
                yield return null; yield return null;
                var source = game.GetComponents<AudioSource>().Single(s => s.clip != null && s.clip.name == "watering_can_pour");
                var samples = new float[source.clip.samples]; source.clip.GetData(samples, 0);
                Check(source.clip.channels == 1 && source.clip.frequency == 22050 && Mathf.Abs(source.clip.length - 6f) < 0.01f, "Real watering-can recording is loaded with the expected import settings.");
                Check(samples.Any(v => Mathf.Abs(v) > 0.02f) && samples.All(v => !float.IsNaN(v) && Mathf.Abs(v) < 1), "Water loop contains non-silent, finite, unclipped samples.");
                Check(Mathf.Abs(samples[0] - samples[samples.Length - 1]) < 0.15f, "Water loop seam has no large amplitude step.");
                game.Changed += countChange;
                yield return Pointer(mouse, ScreenCell(game, 0), true);
                Check(game.Model.Plot(0).watered && game.WateringActive && source.isPlaying && source.loop && source.volume > 0,
                    $"Holding water starts watering and the audible loop. [focused={Application.isFocused}, target={game.HoveredIndex}, reach={game.Selection.HoveredInReach}, item={game.Model.EquippedItem}, player={game.Player.position}]");
                string state = JsonUtility.ToJson(game.Model.Snapshot());
                yield return new WaitForSecondsRealtime(source.clip.length + 0.25f);
                Check(Mathf.Abs(source.volume - 0.0832f) < 0.0001f,
                    $"Water source gain is increased by 30 percent from 0.064. [volume={source.volume}, focused={Application.isFocused}, pouring={game.WateringActive}]");
                Check(changes == 1 && JsonUtility.ToJson(game.Model.Snapshot()) == state, "Holding on a wet cell does not repeat transactions or saves.");
                yield return Pointer(mouse, ScreenCell(game, 1), true);
                Check(game.Model.Plot(1).watered && changes == 2 && source.isPlaying, "Dragging while held waters a second cell without a new press.");
                yield return Pointer(mouse, new Vector2(60, Screen.height - 60), true);
                Check(!game.WateringActive, "HUD interrupts watering immediately.");
                yield return new WaitForSecondsRealtime(0.8f);
                Check(!source.isPlaying, "HUD audio tail fades to silence.");
                yield return Pointer(mouse, new Vector2(-10, -10), true);
                Check(!source.isPlaying && changes == 2, "Outside-window hold does not water or play audio.");
                yield return Pointer(mouse, ScreenCell(game, 35), true);
                Check(!game.WateringActive && !source.isPlaying, "Out-of-reach target stops pouring.");
                yield return Pointer(mouse, ScreenCell(game, 1), true);
                Check(game.WateringActive && source.isPlaying, "Returning to reachable soil resumes an existing watering gesture.");
                yield return Pointer(mouse, ScreenCell(game, 1), false);
                Check(!game.WateringActive && source.isPlaying && source.volume > 0, "Release ends watering but preserves a short audio tail.");
                float tailStart = source.volume;
                string releasedState = JsonUtility.ToJson(game.Model.Snapshot());
                yield return new WaitForSecondsRealtime(0.15f);
                Check(source.isPlaying && source.volume > 0 && source.volume < tailStart, "Released water fades gradually instead of cutting off.");
                yield return Pointer(mouse, ScreenCell(game, 1), true);
                Check(game.WateringActive && source.isPlaying && source.volume > 0, "Re-gripping during fade resumes the same audio source.");
                yield return Pointer(mouse, ScreenCell(game, 1), false);
                yield return new WaitForSecondsRealtime(0.8f);
                Check(!source.isPlaying && source.volume == 0 && JsonUtility.ToJson(game.Model.Snapshot()) == releasedState,
                    "Tail ends in silence without performing extra watering.");
                yield return Pointer(mouse, new Vector2(60, Screen.height - 60), true);
                yield return Pointer(mouse, ScreenCell(game, 3), true);
                Check(!game.Model.Plot(3).watered && !source.isPlaying, "A press starting on UI cannot spill into the farm.");
                yield return Pointer(mouse, ScreenCell(game, 3), false);
                yield return Pointer(mouse, ScreenCell(game, 3), true);
                Check(game.Model.Plot(3).watered && source.isPlaying, "A fresh press starts a new watering gesture.");
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Digit1)); yield return null; yield return null;
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                Check(!game.WateringActive, "Changing equipment stops watering immediately.");
                yield return new WaitForSecondsRealtime(0.8f);
                Check(!source.isPlaying, "Changing equipment lets the audio tail finish.");
                yield return Pointer(mouse, ScreenCell(game, 2), true);
                Check(game.Model.Stage(2) == -1, "Switching to seeds while held does not plant.");
                yield return Pointer(mouse, ScreenCell(game, 2), false);
                yield return Pointer(mouse, ScreenCell(game, 2), true);
                yield return Pointer(mouse, ScreenCell(game, 4), true);
                Check(game.Model.Stage(2) == 0 && game.Model.Stage(4) == -1, "Seeds remain single-click even when dragged.");
                yield return Pointer(mouse, ScreenCell(game, 4), false);
                var ripe = game.Model.Snapshot();
                ripe.plots[0].growth = ripe.plots[1].growth = game.ActiveCrop.wateredDays;
                File.WriteAllText(game.SavePath, JsonUtility.ToJson(ripe)); game.LoadGame();
                game.Equip(FarmItem.Sickle);
                yield return Pointer(mouse, ScreenCell(game, 0), true);
                yield return Pointer(mouse, ScreenCell(game, 1), true);
                Check(game.Model.Stage(0) == -1 && game.Model.IsReady(1), "Harvest stays single-click when dragged.");
                yield return Pointer(mouse, ScreenCell(game, 1), false);
                game.Equip(FarmItem.WateringCan);
                yield return Pointer(mouse, ScreenCell(game, 1), true);
                game.enabled = false; yield return null;
                Check(!game.WateringActive && !source.isPlaying, "Disabling gameplay stops the loop.");
                game.enabled = true;
                // Lifecycle callbacks are sent explicitly; this is not an OS focus-switch test.
                yield return Pointer(mouse, ScreenCell(game, 1), false);
                yield return Pointer(mouse, ScreenCell(game, 1), true);
                game.SendMessage("OnApplicationFocus", false);
                Check(!game.WateringActive && !source.isPlaying, "Focus-loss callback cancels watering.");
                yield return null; yield return null;
                Check(!source.isPlaying, "Held input cannot restart after focus cancellation without a new press.");
                yield return Pointer(mouse, ScreenCell(game, 1), false);
                yield return Pointer(mouse, ScreenCell(game, 35), true);
                yield return Pointer(mouse, ScreenCell(game, 1), true);
                Check(game.WateringActive && source.isPlaying, "A hold starting far away pours when its target comes within reach.");
                yield return Pointer(mouse, ScreenCell(game, 1), false);
                Debug.Log("FARMER_WATERING_CHECKS_FINISHED");
            }
            finally
            {
                game.Changed -= countChange;
                InputSystem.RemoveDevice(keyboard); InputSystem.RemoveDevice(mouse);
                foreach (var device in physicalInput) InputSystem.EnableDevice(device);
                File.WriteAllText(game.SavePath, JsonUtility.ToJson(original)); game.LoadGame();
            }
        }
        private static Vector2 ScreenCell(FarmGame game, int index) => Camera.main.WorldToScreenPoint(
            game.Selection.Layout.Center(new Vector2Int(index % game.Model.Width, index / game.Model.Width), 0.055f));
        private static IEnumerator Pointer(Mouse mouse, Vector2 position, bool held)
        {
            InputSystem.QueueStateEvent(mouse, new MouseState { position = position, buttons = (ushort)(held ? 1 : 0) });
            yield return null; yield return null; yield return null;
        }
        private static void Check(bool condition, string message)
        {
            if (condition) Debug.Log("FARMER_WATER_CHECK_OK: " + message);
            else Debug.LogError("FARMER_WATER_CHECK_FAILED: " + message);
        }
    }
}
#endif
