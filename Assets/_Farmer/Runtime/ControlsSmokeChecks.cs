#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Farmer
{
    // Explicit development-player automation: exercises the same device polling as manual play.
    public static class ControlsSmokeChecks
    {
        public static IEnumerator Run()
        {
            var motor = Object.FindFirstObjectByType<PlayerMotor>();
            var selection = Object.FindFirstObjectByType<FarmSelection>();
            var camera = Camera.main;
            if (motor == null || selection == null || camera == null)
            {
                Debug.LogError("FARMER_CONTROLS_FAILED: prototype scene components missing.");
                yield break;
            }
            var controller = motor.GetComponent<CharacterController>();
            var start = motor.transform.position;
            var cameraPosition = camera.transform.position;
            var cameraRotation = camera.transform.rotation;
            var keyboard = InputSystem.AddDevice<Keyboard>();
            var mouse = InputSystem.AddDevice<Mouse>();
            try
            {
                Check(Application.isFocused, "Player window must have focus for input validation.");
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W));
                yield return new WaitForSecondsRealtime(0.35f);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                yield return null;
                Vector3 travelled = motor.transform.position - start; travelled.y = 0;
                Vector3 forward = camera.transform.forward; forward.y = 0;
                Check(travelled.magnitude > 0.6f && travelled.magnitude < 2.2f, "W moves the player at the expected speed.");
                Check(Vector3.Dot(travelled.normalized, forward.normalized) > 0.98f, "W follows projected camera forward.");

                Teleport(controller, new Vector3(4.5f, 0.1f, -2f));
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W, Key.A));
                yield return new WaitForSecondsRealtime(0.9f);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                yield return null;
                Check(motor.transform.position.z > -1.4f && motor.transform.position.z < -0.7f,
                    "CharacterController stops at the solid crate.");

                Teleport(controller, new Vector3(27.25f, 0.1f, 6));
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W, Key.D));
                yield return new WaitForSecondsRealtime(0.3f);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                yield return null;
                Check(motor.transform.position.x <= 27.31f && motor.transform.position.y > -0.1f,
                    "Player remains on the ground at the world boundary.");

                Teleport(controller, new Vector3(-4, 0.1f, -4));
                yield return null;
                var near = new Vector2Int(0, 0);
                Vector2 nearScreen = camera.WorldToScreenPoint(selection.Layout.Center(near, 0.055f));
                yield return Hover(mouse, nearScreen);
                Check(selection.HoveredCell == near, "Mouse ray targets the reachable cell without clicking.");
                yield return Click(mouse, new Vector2(60, Screen.height - 60));
                Check(!selection.HoveredCell.HasValue, "HUD blocks the world target.");

                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Escape));
                yield return null;
                yield return null;
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                Check(!selection.HoveredCell.HasValue, "HUD target remains empty after Escape.");
                Vector2 farScreen = camera.WorldToScreenPoint(selection.Layout.Center(new Vector2Int(5, 5), 0.055f));
                yield return Click(mouse, farScreen);
                Check(selection.HoveredCell.HasValue && !selection.HoveredInReach, "Distant targets are out of reach.");
                yield return Hover(mouse, nearScreen);
                InputSystem.QueueStateEvent(mouse, new MouseState { position = new Vector2(-10, -10) });
                yield return null;
                yield return null;
                Check(!selection.HoveredCell.HasValue,
                    "Leaving the viewport clears the target.");
                Check(camera.transform.position == cameraPosition && Quaternion.Angle(cameraRotation, camera.transform.rotation) < 0.001f,
                    "Camera angle and farm fixture framing stay fixed while targeting.");
                Debug.Log("FARMER_CONTROLS_CHECKS_FINISHED");
            }
            finally
            {
                InputSystem.RemoveDevice(keyboard);
                InputSystem.RemoveDevice(mouse);
            }
        }

        private static IEnumerator Hover(Mouse mouse, Vector2 position)
        {
            InputSystem.QueueStateEvent(mouse, new MouseState { position = position });
            yield return null; yield return null;
        }

        private static IEnumerator Click(Mouse mouse, Vector2 position)
        {
            InputSystem.QueueStateEvent(mouse, new MouseState { position = position, buttons = 1 });
            yield return null;
            yield return null;
            InputSystem.QueueStateEvent(mouse, new MouseState { position = position });
            yield return null;
        }

        private static void Teleport(CharacterController controller, Vector3 position)
        {
            controller.enabled = false;
            controller.transform.position = position;
            controller.enabled = true;
            Physics.SyncTransforms();
        }

        private static void Check(bool condition, string message)
        {
            if (condition) Debug.Log("FARMER_CHECK_OK: " + message);
            else Debug.LogError("FARMER_CHECK_FAILED: " + message);
        }
    }
}
#endif
