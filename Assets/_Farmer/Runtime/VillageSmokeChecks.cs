using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Farmer
{
    public static class VillageSmokeChecks
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        public static IEnumerator Run(string output)
        {
            var game=Object.FindFirstObjectByType<FarmGame>();
            string qaRoot=Path.GetFullPath(Path.Combine(Application.temporaryCachePath,"FarmerQA"))+Path.DirectorySeparatorChar;
            if(!System.Environment.GetCommandLineArgs().Contains("--farmer-smoke-capture")||!Path.GetFullPath(game.SavePath).StartsWith(qaRoot,System.StringComparison.Ordinal))
            {Debug.LogError("Village QA refuses to modify a non-isolated save.");yield break;}
            Debug.Log("FARMER_VILLAGE_ISOLATED_SAVE: "+game.SavePath);
            var combat=game.GetComponent<CombatController>();combat.enabled=false;
            var wildlife=game.GetComponent<WildlifeController>();wildlife.enabled=false;var village=game.GetComponent<VillageInteraction>();
            var motor=game.Player.GetComponent<PlayerMotor>();motor.enabled=false;
            var state=game.Model.Snapshot();state.money=1000;state.minuteOfDay=660;File.WriteAllText(game.SavePath,JsonUtility.ToJson(state));game.LoadGame();yield return null;
            wildlife.Simulate(.01f);motor.Teleport(village.Guide.position+Vector3.forward*2);Physics.SyncTransforms();yield return null;
            Frame(village.Guide.position,6);if(!village.TryTalk())Debug.LogError("Native dialogue did not open.");yield return null;village.Advance();combat.RefreshHud();
            yield return Capture(output,"-dialogue-defne");village.Advance();village.Advance();yield return Capture(output,"-dialogue-player");village.Close();yield return null;yield return Capture(output,"-interaction");
            var rabbit=wildlife.View(0);motor.Teleport(rabbit.transform.position+Vector3.forward*5);wildlife.Simulate(.01f);Frame(rabbit.transform.position,2);yield return Capture(output,"-rabbit");
            foreach(int id in new[]{0,1}){var enemy=combat.View(id);Frame(enemy.transform.position,2);yield return Capture(output,id==0?"-slime":"-beetle");}
            var deer=wildlife.View(4);if(deer==null){Debug.LogError("No deer spawned.");yield break;}
            motor.Teleport(deer.transform.position+Vector3.forward*7);Physics.SyncTransforms();wildlife.Simulate(.01f);Frame(deer.transform.position+Vector3.forward*2,5);combat.RefreshHud();yield return Capture(output,"-wildlife");
            game.Model.Equip(FarmItem.Axe);game.Model.StrikeAnimal(4);wildlife.Simulate(.01f);yield return Capture(output,"-wildlife-hit");
            game.Model.BuyHuntingTool("bow",out _);game.Model.BuyHuntingTool("dagger",out _);game.Equip(FarmItem.Bow);game.CombatChanged("Yay ve hançer hazır.");
            motor.Teleport(game.Market.position+Vector3.forward*3);yield return null;game.GetComponent<MarketInteraction>().TryOpen();yield return null;
            var market=game.GetComponent<MarketHud>();market.ChooseCategory(MarketCategory.Tools);yield return null;yield return Capture(output,"-tools");game.GetComponent<MarketInteraction>().Close();
            yield return null;
            game.ShowBuildFeedback("");Frame(game.Market.position,5);motor.Teleport(game.Market.position-Vector3.forward*2);game.Model.Combat.Damage(15);combat.RefreshHud();
            yield return Capture(output,"-huseyin-hud");
            yield return null;village.TryTalkMerchant();village.Advance();yield return Capture(output,"-huseyin-dialogue");
            for(int i=0;i<4;i++)village.Advance();yield return Capture(output,"-huseyin-choices");village.OpenMerchantProducts();yield return Capture(output,"-huseyin-products");game.GetComponent<MarketInteraction>().Close();yield return null;
            game.GetComponent<StorageInteraction>().Open(null);yield return null;yield return Capture(output,"-bag");game.GetComponent<StorageInteraction>().Close();
            var preview=VillageArtwork.Animal(true);preview.transform.position=new Vector3(1000,0,1000);var gait=preview.AddComponent<DeerLocomotion>();
            var camera=Camera.main;camera.transform.position=preview.transform.position+new Vector3(3,1,1);camera.transform.LookAt(preview.transform.position+Vector3.up*.9f);camera.orthographicSize=1.4f;
            foreach(var canvas in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))canvas.gameObject.SetActive(false);
            gait.Step(.85f,.16f);yield return Capture(output,"-deer-step-a");gait.Step(.85f,.25f);yield return Capture(output,"-deer-step-b");
            Object.Destroy(preview);
            Debug.Log("FARMER_VILLAGE_NATIVE_OK: dialogue portraits, wildlife and weapon market rendered.");
        }
        private static void Frame(Vector3 p,float zoom){var c=Camera.main;c.orthographicSize=zoom;c.transform.position=p+new Vector3(-8,10,-8);c.transform.LookAt(p);}
        private static IEnumerator Capture(string output,string suffix){yield return new WaitForEndOfFrame();
            var overlay=GameObject.Find("Dialogue overlay");
            if(overlay!=null)
            {
                foreach(var name in new[]{"Speaker portrait","Dialogue parchment","Dialogue close","Dialogue words","Merchant choices"})
                {
                    var element=overlay.GetComponentsInChildren<RectTransform>().FirstOrDefault(t=>t.name==name);
                    if(element==null||!element.gameObject.activeInHierarchy)continue;
                    var corners=new Vector3[4];element.GetWorldCorners(corners);
                    if(corners.Any(p=>p.x<-.5f||p.y<-.5f||p.x>Screen.width+.5f||p.y>Screen.height+.5f))
                        Debug.LogError($"Dialogue layout clipped: {name} at {Screen.width}x{Screen.height}");
                }
            }
            ScreenCapture.CaptureScreenshot(Path.Combine(Path.GetDirectoryName(output),Path.GetFileNameWithoutExtension(output)+suffix+".png"));yield return new WaitForSecondsRealtime(.2f);}
#endif
    }
}
