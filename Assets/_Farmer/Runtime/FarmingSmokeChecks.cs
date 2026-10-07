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
    public static class FarmingSmokeChecks
    {
        public static IEnumerator Run(string screenshot)
        {
            var game = Object.FindFirstObjectByType<FarmGame>();
            var player = Object.FindFirstObjectByType<PlayerMotor>();
            if (game == null || player == null) { Debug.LogError("FARMER_FARM_CHECK_FAILED: missing farm scene."); yield break; }
            var controller = player.GetComponent<CharacterController>();
            var keyboard = InputSystem.AddDevice<Keyboard>();
            var mouse = InputSystem.AddDevice<Mouse>();
            try
            {
                Check(game.Ready && game.Model.Day == 1 && game.Model.Money == 60, "Fresh isolated test save starts correctly.");
                Teleport(controller, game.Market.position + Vector3.back * 1.8f);
                yield return null;
                yield return Capture(screenshot, "market");
                yield return KeyPress(keyboard, Key.B);
                Check(game.Model.Seeds("turnip") == 1 && game.Model.Money == 50, "B buys one seed at the market.");
                var buyFive = Object.FindObjectsByType<Button>(FindObjectsSortMode.None)
                    .FirstOrDefault(b => b.GetComponentInChildren<Text>().text.StartsWith("5 tohum"));
                if (buyFive == null) { Debug.LogError("FARMER_FARM_CHECK_FAILED: market UI missing."); yield break; }
                yield return Click(mouse, RectTransformUtility.WorldToScreenPoint(null, buyFive.transform.position
                    + (Vector3)((RectTransform)buyFive.transform).rect.center));
                Check(game.Model.Seeds("turnip") == 6 && game.Model.Money == 0, "Market button buys five seeds through UI input.");
                yield return KeyPress(keyboard, Key.B);
                Check(game.Model.Money == 0 && game.Model.Seeds("turnip") == 6, "Unaffordable purchase is rejected.");

                yield return KeyPress(keyboard, Key.Digit1);
                yield return Hover(game, controller, mouse, 0);
                yield return KeyPress(keyboard, Key.E);
                Check(game.Model.Stage(0) == -1 && game.Model.Seeds("turnip") == 6, "E no longer performs farm actions.");
                Vector3 walkStart = player.transform.position;
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W));
                yield return new WaitForSecondsRealtime(0.1f);
                yield return Click(mouse, mouse.position.ReadValue());
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                yield return null; yield return null;
                Check(Vector3.Distance(player.transform.position, walkStart) > 0.1f, "W movement continues while left click uses an item.");
                Check(game.Model.Seeds("turnip") == 5 && game.Model.Stage(0) == 0, "One left click plants directly and consumes one seed.");
                yield return Click(mouse, mouse.position.ReadValue());
                Check(!game.Model.Plot(0).watered && game.Model.Seeds("turnip") == 5, "Seed item cannot water an occupied plot.");
                var waterSlot = GameObject.Find("Inventory Slot 1").GetComponent<Button>();
                yield return Click(mouse, RectTransformUtility.WorldToScreenPoint(null, waterSlot.transform.position
                    + (Vector3)((RectTransform)waterSlot.transform).rect.center));
                Check(game.Model.EquippedItem == FarmItem.WateringCan && GameObject.Find("Held WateringCan") != null, "Inventory click equips a visible watering can.");
                yield return Click(mouse, mouse.position.ReadValue());
                Check(!game.Model.Plot(0).watered, "Click over inventory does not use the previous soil target.");
                yield return Hover(game, controller, mouse, 0);
                yield return Click(mouse, mouse.position.ReadValue());
                Check(game.Model.Plot(0).watered, "Left click waters the planted crop.");
                yield return KeyPress(keyboard, Key.Digit1);
                yield return Hover(game, controller, mouse, 1);
                yield return Click(mouse, mouse.position.ReadValue());
                Check(!game.Model.Plot(1).watered, "Second planted crop starts dry.");
                yield return Capture(screenshot, "stage-0");
                yield return KeyPress(keyboard, Key.Digit2);
                for (int stage = 1; stage <= 3; stage++)
                {
                    Teleport(controller, game.Camp.position + Vector3.right);
                    yield return KeyPress(keyboard, Key.N);
                    Check(game.Model.Stage(0) == stage && game.Model.Stage(1) == 0, "Only the watered crop reaches stage " + stage + ".");
                    Check(game.Model.Plot(0).watered, "One watering persists across nights until harvest.");
                    yield return Hover(game, controller, mouse, 0);
                    yield return Capture(screenshot, "stage-" + stage);

                }
                Check(game.Model.IsReady(0) && game.Model.Day == 4, "One watering and three nights produce a ready turnip.");
                yield return Click(mouse, mouse.position.ReadValue());
                Check(game.Model.Produce("turnip") == 0 && game.Model.IsReady(0), "Watering can never harvest a ripe plant.");
                yield return KeyPress(keyboard, Key.Digit3);
                Check(GameObject.Find("Held Sickle") != null, "Number key equips a visible sickle.");
                yield return Capture(screenshot, "sickle");
                Teleport(controller, new Vector3(7, 0.1f, 7));
                yield return Click(mouse, mouse.position.ReadValue());
                Check(game.Model.IsReady(0) && game.Model.Produce("turnip") == 0, "Walking away prevents remote harvesting.");
                yield return Hover(game, controller, mouse, 0);
                yield return Click(mouse, mouse.position.ReadValue());
                Check(game.Model.Produce("turnip") == 1 && game.Model.Stage(0) == -1, "Harvest clears the plot and grants one item.");
                Check(GameObject.Find("Harvest Feedback") != null && game.GetComponent<AudioSource>().isPlaying,
                    "Harvest triggers a floating item label and an audio cue.");
                Check(!game.Model.Harvest(0, out _) && game.Model.Produce("turnip") == 1, "Harvest cannot be duplicated.");
                Teleport(controller, game.Market.position + Vector3.back * 1.8f);
                yield return KeyPress(keyboard, Key.V);
                Check(game.Model.Produce("turnip") == 0 && game.Model.Money == 18, "V sells the harvest for its configured price.");
                yield return KeyPress(keyboard, Key.B);
                Check(game.Model.Money == 8 && game.Model.Seeds("turnip") == 5, "Sale proceeds buy the next seed.");
                string before = JsonUtility.ToJson(game.Model.Snapshot());
                yield return KeyPress(keyboard, Key.F5);
                yield return KeyPress(keyboard, Key.F9);
                Check(File.Exists(game.SavePath) && JsonUtility.ToJson(game.Model.Snapshot()) == before, "Save/load preserves the complete farm state.");

                // Each crop is watered only once; stagger planting for a four-stage overview.
                yield return KeyPress(keyboard, Key.Digit1);
                yield return Hover(game, controller, mouse, 0); yield return Click(mouse, mouse.position.ReadValue());
                yield return KeyPress(keyboard, Key.Digit2); yield return Click(mouse, mouse.position.ReadValue());
                Teleport(controller, game.Camp.position + Vector3.right); yield return KeyPress(keyboard, Key.N);
                yield return Hover(game, controller, mouse, 1); yield return Click(mouse, mouse.position.ReadValue());
                Teleport(controller, game.Camp.position + Vector3.right); yield return KeyPress(keyboard, Key.N);
                yield return KeyPress(keyboard, Key.Digit1);
                yield return Hover(game, controller, mouse, 2); yield return Click(mouse, mouse.position.ReadValue());
                yield return KeyPress(keyboard, Key.Digit2); yield return Click(mouse, mouse.position.ReadValue());
                Teleport(controller, game.Camp.position + Vector3.right); yield return KeyPress(keyboard, Key.N);
                yield return KeyPress(keyboard, Key.Digit1);
                yield return Hover(game, controller, mouse, 3); yield return Click(mouse, mouse.position.ReadValue());
                yield return KeyPress(keyboard, Key.Digit2); yield return Click(mouse, mouse.position.ReadValue());
                Check(game.Model.Stage(0) == 3 && game.Model.Stage(1) == 2 && game.Model.Stage(2) == 1 && game.Model.Stage(3) == 0,
                    "Four distinct growth stages coexist in the final view.");
                yield return Hover(game, controller, mouse, 0);
                Teleport(controller, new Vector3(-4.5f, 0.1f, -2.8f));
                yield return null;
                // Pointer motion and left click arriving in the same frame must act on the new target.
                yield return KeyPress(keyboard, Key.Digit1);
                Vector3 fresh = game.Selection.Layout.Center(new Vector2Int(4, 0), 0.055f);
                Teleport(controller, fresh + Vector3.back * 1.5f + Vector3.up * 0.1f);
                yield return Click(mouse, Camera.main.WorldToScreenPoint(fresh));
                Check(game.Model.Stage(4) == 0, "Left click uses the new mouse position in the same frame.");
                string state = JsonUtility.ToJson(game.Model.Snapshot());
                yield return Click(mouse, new Vector2(-10, -10));
                Check(JsonUtility.ToJson(game.Model.Snapshot()) == state, "Click outside the window cannot affect the farm.");
                yield return KeyPress(keyboard, Key.Digit2);
                yield return Hover(game, controller, mouse, 2);
                Teleport(controller, new Vector3(-4.5f, 0.1f, -2.8f));
                Debug.Log("FARMER_FARMING_CHECKS_FINISHED");
            }
            finally { InputSystem.RemoveDevice(keyboard); InputSystem.RemoveDevice(mouse); }
        }

        private static IEnumerator Hover(FarmGame game, CharacterController player, Mouse mouse, int index)
        {
            var cell = new Vector2Int(index % game.Model.Width, index / game.Model.Width);
            Vector3 position = game.Selection.Layout.Center(cell, 0.055f);
            Teleport(player, position + Vector3.back * 1.5f + Vector3.up * 0.1f);
            yield return null;
            InputSystem.QueueStateEvent(mouse, new MouseState { position = Camera.main.WorldToScreenPoint(position) });
            yield return null; yield return null;
            Check(game.HoveredIndex == index && game.Selection.HoveredInReach, "Mouse hovers without clicking over plot " + index + ".");
        }
        private static IEnumerator KeyPress(Keyboard keyboard, Key key)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(key)); yield return null; yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); yield return null; yield return null;
        }
        private static IEnumerator Click(Mouse mouse, Vector2 position)
        {
            InputSystem.QueueStateEvent(mouse, new MouseState { position = position, buttons = 1 }); yield return null; yield return null;
            InputSystem.QueueStateEvent(mouse, new MouseState { position = position }); yield return null; yield return null;
        }
        private static IEnumerator Capture(string output, string suffix)
        {
            string path = Path.Combine(Path.GetDirectoryName(output), Path.GetFileNameWithoutExtension(output) + "-" + suffix + ".png");
            yield return new WaitForEndOfFrame(); ScreenCapture.CaptureScreenshot(path);
            float deadline = Time.realtimeSinceStartup + 5f;
            while (!File.Exists(path) && Time.realtimeSinceStartup < deadline) yield return null;
            Check(File.Exists(path), "Screenshot " + suffix + " captured.");
        }
        private static void Teleport(CharacterController controller, Vector3 position)
        { controller.enabled = false; controller.transform.position = position; controller.enabled = true; Physics.SyncTransforms(); }
        private static void Check(bool condition, string message)
        {
            if (condition) Debug.Log("FARMER_FARM_CHECK_OK: " + message);
            else Debug.LogError("FARMER_FARM_CHECK_FAILED: " + message);
        }
    }
}
#endif
