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
    public static class MenuSmokeChecks
    {
        public static IEnumerator Run(string screenshot)
        {
            var game=Object.FindFirstObjectByType<FarmGame>();var menu=game.GetComponent<SessionMenu>();
            var physical=InputSystem.devices.Where(d=>d.enabled&&(d is Mouse||d is Keyboard)).ToArray();
            var mouse=InputSystem.AddDevice<Mouse>();var keyboard=InputSystem.AddDevice<Keyboard>();
            try
            {
                foreach(var device in physical)InputSystem.DisableDevice(device);
                Check(menu.IsOpen,"Normal startup enters the menu before world input.");
                Check(Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Any(b=>b.GetComponentInChildren<Text>()?.text=="Başla"),"An isolated first save has a Start button, without a destructive New Game action.");
                menu.ToggleFullscreen();yield return new WaitForSecondsRealtime(1);
                Check(Screen.fullScreen,"Fullscreen request is applied by the Linux player.");
                menu.ToggleFullscreen();yield return new WaitForSecondsRealtime(1);
                Check(!Screen.fullScreen,"The fullscreen toggle returns to windowed mode.");
                var button=Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b=>b.GetComponentInChildren<Text>()?.text=="Başla");
                yield return Click(mouse,button);Check(!menu.IsOpen,"Start button enters the world.");
                int money=game.Model.Money;game.SaveGame();game.LoadGame();
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Escape));yield return null;yield return null;
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;yield return null;
                Check(menu.IsOpen,"Escape returns to the session menu.");
                yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Path.GetDirectoryName(screenshot),Path.GetFileNameWithoutExtension(screenshot)+"-title.png"));yield return new WaitForSecondsRealtime(.4f);
                button=Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b=>b.GetComponentInChildren<Text>()?.text=="Devam Et");
                yield return Click(mouse,button);Check(!menu.IsOpen&&game.Model.Money==money,"Continue preserves the loaded save and returns to the world.");
            }
            finally{menu.Continue();InputSystem.RemoveDevice(mouse);InputSystem.RemoveDevice(keyboard);foreach(var d in physical)InputSystem.EnableDevice(d);}
        }
        private static IEnumerator Click(Mouse mouse,Button button)
        {
            var r=(RectTransform)button.transform;var p=RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(r.rect.center));
            InputSystem.QueueStateEvent(mouse,new MouseState{position=p});yield return null;yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=p,buttons=1});yield return null;yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=p});yield return null;yield return null;
        }
        private static void Check(bool ok,string text){if(ok)Debug.Log("FARMER_MENU_CHECK_OK: "+text);else Debug.LogError("FARMER_MENU_CHECK_FAILED: "+text);}
    }
}
#endif
