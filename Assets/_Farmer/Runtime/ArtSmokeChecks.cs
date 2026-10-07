#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Farmer
{
    public static class ArtSmokeChecks
    {
        public static IEnumerator Run(string screenshot)
        {
            var game = Object.FindFirstObjectByType<FarmGame>();
            var motor = game.Player.GetComponent<PlayerMotor>();
            var animator = game.Player.GetComponentInChildren<Animator>();
            Check(animator != null && animator.avatar.isHuman && animator.avatar.isValid, "Character uses a valid Humanoid avatar.");
            Check(!animator.applyRootMotion, "Animation cannot move the gameplay root.");
            var skin = animator.GetComponentInChildren<SkinnedMeshRenderer>();
            Check(skin.sharedMesh.vertexCount > 10000 && skin.sharedMaterial.GetTexture("_BaseMap") != null, "Skinned Tripo mesh and extracted texture are bound.");
            Check(animator.runtimeAnimatorController.animationClips.Any(c => c.name == "Idle") && animator.runtimeAnimatorController.animationClips.Any(c => c.name == "Walk"), "Both supplied Mixamo clips are in the controller.");
            var camera = Camera.main; var position = camera.transform.position; var rotation = camera.transform.rotation; float size = camera.orthographicSize;
            var keyboard = InputSystem.AddDevice<Keyboard>(); var mouse = InputSystem.AddDevice<Mouse>();
            try
            {
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                InputSystem.QueueStateEvent(mouse, new MouseState { position = new Vector2(-10, -10) });
                var cc = game.Player.GetComponent<CharacterController>(); cc.enabled = false; game.Player.position = new Vector3(0, .1f, -4.5f); cc.enabled = true;
                motor.Visual.rotation = Quaternion.Euler(0, 225, 0); Physics.SyncTransforms();
                game.Equip(FarmItem.WateringCan);
                yield return new WaitForSecondsRealtime(.5f);
                Vector3 target = game.Player.position + Vector3.up * .9f;
                camera.transform.position = target + new Vector3(-4, 2.5f, -5);
                camera.transform.LookAt(target); camera.orthographicSize = 1.85f;
                Check(animator.GetFloat("MoveSpeed") < .01f, "Idle selected at rest.");
                Vector3 rest = game.Player.position;
                yield return new WaitForSecondsRealtime(.3f);
                Check(Vector2.Distance(new Vector2(rest.x, rest.z), new Vector2(game.Player.position.x, game.Player.position.z)) < .001f, "Idle does not cause horizontal drift.");
                yield return Capture(screenshot, "character-can");
                var hand = animator.GetBoneTransform(HumanBodyBones.RightHand);
                var can = GameObject.Find("Held WateringCan");
                Check(can != null && can.transform.IsChildOf(hand) && Vector3.Distance(can.transform.position, hand.position) < .12f, "Watering can follows the hand socket.");
                Check(can.GetComponentInChildren<MeshFilter>().sharedMesh.vertexCount > 1000, "Watering can uses delivered geometry.");
                float canHeight = can.GetComponentInChildren<Renderer>().bounds.size.y;
                Check(canHeight > .39f && canHeight < .45f, "Can stays upright at its intended 0.42 m height.");
                game.Equip(FarmItem.Sickle); yield return new WaitForSecondsRealtime(.2f);
                var sickle = GameObject.Find("Held Sickle");
                Check(sickle != null && sickle.transform.IsChildOf(hand) && !can.activeSelf, "Sickle replaces the can on the same hand.");
                float sickleHeight = sickle.GetComponentInChildren<Renderer>().bounds.size.y;
                Check(sickleHeight > .47f && sickleHeight < .53f, "Sickle preserves its upright 0.5 m silhouette.");
                yield return Capture(screenshot, "character-sickle");
                Vector3 foot = animator.GetBoneTransform(HumanBodyBones.LeftFoot).position;
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.S));
                yield return new WaitForSecondsRealtime(.25f);
                Check(motor.PlanarSpeed > 1 && animator.GetFloat("MoveSpeed") > .5f, "Walking animation follows actual movement.");
                Check(Vector3.Distance(foot, animator.GetBoneTransform(HumanBodyBones.LeftFoot).position) > .1f, "Walking moves the animated leg.");
                yield return Capture(screenshot, "character-walk");
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                yield return new WaitForSecondsRealtime(.7f);
                Check(animator.GetFloat("MoveSpeed") < .02f, "Stopping returns to idle.");
                var stall = game.Market.Find("Market Stall Art");
                Check(stall != null && stall.GetComponentInChildren<Renderer>().sharedMaterial.GetTexture("_BaseMap") != null && game.Market.GetComponent<Collider>() != null, "Market has delivered textured model and gameplay collider.");
                target = game.Market.position + Vector3.up * 1.1f;
                camera.transform.position = target + new Vector3(-4, 2.5f, -5);camera.transform.LookAt(target);camera.orthographicSize = 1.9f;
                yield return Capture(screenshot, "market-art");
                Debug.Log("FARMER_ART_CHECKS_FINISHED");
            }
            finally
            {
                InputSystem.RemoveDevice(keyboard); InputSystem.RemoveDevice(mouse);
                camera.transform.SetPositionAndRotation(position, rotation); camera.orthographicSize = size;
            }
        }
        private static IEnumerator Capture(string path, string label)
        {
            yield return new WaitForEndOfFrame();
            string file = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + "-" + label + ".png");
            ScreenCapture.CaptureScreenshot(file); yield return new WaitForSecondsRealtime(.25f);
        }
        private static void Check(bool ok, string message)
        { if (ok) Debug.Log("FARMER_ART_CHECK_OK: " + message); else Debug.LogError("FARMER_ART_CHECK_FAILED: " + message); }
    }
}
#endif
