#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Farmer
{
    public static class DemoSmokeChecks
    {
        public static IEnumerator Run(string screenshot)
        {
            var game=Object.FindFirstObjectByType<FarmGame>();var motor=game.Player.GetComponent<PlayerMotor>();var combat=game.GetComponent<CombatController>();
            var physical=InputSystem.devices.Where(d=>d.enabled&&(d is Mouse||d is Keyboard)).ToArray();
            var keyboard=InputSystem.AddDevice<Keyboard>();var mouse=InputSystem.AddDevice<Mouse>();
            try
            {
                foreach(var d in physical)InputSystem.DisableDevice(d);
                Check(Application.isFocused,"Native player owns keyboard focus.");
                Check(game.Ready&&game.Model.Combat.Health==100&&!game.Model.OwnsPickaxe,"Clean start has health and requires buying the pickaxe.");
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.J));yield return null;yield return null;
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
                Check(game.JournalOpen&&game.WorldInputBlocked,"J opens the journal and pauses world actions.");
                yield return Capture(screenshot,"journal");
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Escape));yield return null;yield return null;
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
                Check(!game.JournalOpen&&!game.MenuOpen,"Escape closes only the journal.");
                game.GetComponent<DemoHud>().Track(2);
                var slime=combat.View(0);Check(slime!=null&&combat.View(1)!=null,"Both optional encounters exist.");
                if(slime==null)yield break;
                motor.Teleport(slime.Home+Vector3.left*1.7f);
                var camera=Camera.main;camera.transform.position=slime.Home+new Vector3(8,7,9);camera.transform.LookAt(slime.Home+Vector3.up*.4f);camera.orthographicSize=4.5f;
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Digit6));yield return null;yield return null;
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
                var point=(Vector2)camera.WorldToScreenPoint(slime.transform.position+Vector3.up*.5f);
                InputSystem.QueueStateEvent(mouse,new MouseState{position=point});yield return null;yield return null;
                InputSystem.QueueStateEvent(mouse,new MouseState{position=point,buttons=1});yield return null;yield return null;
                InputSystem.QueueStateEvent(mouse,new MouseState{position=point});yield return null;
                Check(game.Model.Combat.Enemy(0).health==20,"6 and mouse click execute the real axe input path.");
                Vector3 before=game.Player.position;
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Space,Key.W));yield return new WaitForSecondsRealtime(.14f);
                Check(motor.Invulnerable&&CombatController.PlanarDistance(before,game.Player.position)>.5f,"Space dashes and gives a short invulnerability window.");
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return new WaitForSecondsRealtime(.3f);
                yield return Capture(screenshot,"combat");
                Check(game.SaveGame()&&game.LoadGame()&&game.Model.Combat.Enemy(0).health==20,"Native player saves and reloads combat progress.");
                motor.Teleport(combat.Sanctuary);game.GetComponent<DemoHud>().Track(0);
                camera.transform.position=combat.Sanctuary+new Vector3(8,7,9);camera.transform.LookAt(combat.Sanctuary);camera.orthographicSize=7;
                int frames=0;float total=0,max=0;float end=Time.realtimeSinceStartup+3;
                while(Time.realtimeSinceStartup<end){yield return null;frames++;total+=Time.unscaledDeltaTime;max=Mathf.Max(max,Time.unscaledDeltaTime);}
                Debug.Log($"FARMER_DEMO_PERFORMANCE: {SystemInfo.graphicsDeviceName}; {frames/total:F1} FPS; worst frame {max*1000:F1} ms; {Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Length} renderers; short farm sample only.");
            }
            finally{InputSystem.RemoveDevice(mouse);InputSystem.RemoveDevice(keyboard);foreach(var d in physical)InputSystem.EnableDevice(d);}
        }
        private static IEnumerator Capture(string path,string suffix)
        {yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Path.GetDirectoryName(path),Path.GetFileNameWithoutExtension(path)+"-"+suffix+".png"));yield return new WaitForSecondsRealtime(.2f);}
        private static void Check(bool ok,string message){if(ok)Debug.Log("FARMER_DEMO_CHECK_OK: "+message);else Debug.LogError("FARMER_DEMO_CHECK_FAILED: "+message);}
    }
}
#endif
