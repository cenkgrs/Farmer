using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Farmer
{
    public static class QuestSmokeChecks
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private static void Check(bool ok,string message){if(!ok)Debug.LogError("FARMER_QUEST_FAILED: "+message);else Debug.Log("FARMER_QUEST_OK: "+message);}
        private static void Finish(VillageInteraction village){for(int i=0;i<100&&!village.AwaitingChoice;i++)village.Advance();}
        public static IEnumerator Run(string output)
        {
            var game=Object.FindFirstObjectByType<FarmGame>();
            var qa=Path.GetFullPath(Path.Combine(Application.temporaryCachePath,"FarmerQA"))+Path.DirectorySeparatorChar;
            if(!Path.GetFullPath(game.SavePath).StartsWith(qa,System.StringComparison.Ordinal)){Debug.LogError("Quest QA requires an isolated save.");yield break;}
            game.GetComponent<SessionMenu>().SetOpen(false);game.GetComponent<DayNightCycle>().ClockPaused=true;
            game.GetComponent<CombatController>().enabled=false;game.GetComponent<WildlifeController>().enabled=false;
            var motor=game.Player.GetComponent<PlayerMotor>();motor.enabled=false;
            var village=game.GetComponent<VillageInteraction>();var hud=game.GetComponent<QuestHud>();
            var state=game.Model.Snapshot();state.money=500;state.minuteOfDay=660;
            state.produce=new[]{new InventoryRecord{cropId="turnip",count=5}};
            File.WriteAllText(game.SavePath,JsonUtility.ToJson(state));Check(game.LoadGame(),"Load quest fixture");yield return null;
            Check(game.Model.QuestCatalog.Count==4,"Four authored quests loaded from two NPC profiles");
            motor.Teleport(game.Market.position-Vector3.forward*2);Physics.SyncTransforms();yield return null;
            Camera.main.GetComponent<ExplorationCamera>().enabled=false;Camera.main.transform.position=game.Market.position+new Vector3(-8,10,-8);Camera.main.transform.LookAt(game.Market.position);Camera.main.orthographicSize=5;
            Check(village.TryTalkMerchant(),"Merchant conversation opens");Finish(village);village.OpenQuestOffer();
            village.Advance();Check(village.Speaker=="Hüseyin","NPC offer speaker");village.Advance();Check(village.Speaker=="Sen","Quest-specific player reply");Finish(village);
            yield return Capture(output,"-offer");
            village.ActOnQuest();Finish(village);Check(game.Model.QuestState("first_stall")==QuestStatus.Active,"Accept from NPC dialogue");
            village.Close();yield return null;
            hud.HandleInput(true,false);hud.SelectQuest("first_stall");Check(hud.IsOpen&&game.WorldInputBlocked&&game.Selection.ModalBlocked,"Q opens modal journal");
            Check(!village.TryTalkMerchant(),"Dialogue blocked by journal");
            game.GetComponent<StorageInteraction>().Open(null);Check(!game.InventoryOpen,"Bag cannot overlap quests");
            yield return Capture(output,"-active");hud.HandleInput(false,true);Check(!hud.IsOpen&&!game.MenuOpen&&!game.Selection.ModalBlocked,"Escape closes only quest journal");yield return null;
            Check(game.SaveGame()&&game.LoadGame(),"Quest state round trip");yield return null;
            Check(game.Model.QuestReady("first_stall"),"Existing produce counts after reload");
            Check(village.TryTalkMerchant(),"Return for delivery");Finish(village);village.OpenQuestOffer();Finish(village);village.ActOnQuest();Finish(village);
            Check(game.Model.QuestState("first_stall")==QuestStatus.Completed&&game.Model.Money==560&&game.Model.Seeds("carrot")==3&&game.Model.Produce("turnip")==0,"Atomic delivery and rewards");
            Check(!game.Model.TurnInQuest("first_stall",out _),"Repeated delivery rejected");
            village.Close();yield return null;hud.SetOpen(true);hud.SelectQuest("first_stall");yield return Capture(output,"-completed");hud.SetOpen(false);yield return null;
            motor.Teleport(village.Guide.position+Vector3.forward*2);Physics.SyncTransforms();yield return null;
            Check(village.TryTalk(),"Defne conversation opens");Finish(village);village.OpenQuestOffer();village.Advance();village.Advance();village.Advance();
            Check(village.Speaker=="Sen","Defne quest has its own player reply");yield return Capture(output,"-defne-reply");Finish(village);village.ActOnQuest();Finish(village);village.Close();yield return null;
            Check(game.Model.QuestState("defne_supper")==QuestStatus.Active&&game.Model.QuestState("variety")==QuestStatus.Available,"Independent NPC chains");
            Check(game.SaveGame()&&game.LoadGame(),"Both NPC quest states persist");yield return null;
            hud.SetOpen(true);hud.SelectQuest("defne_supper");yield return Capture(output,"-defne-journal");hud.SetOpen(false);
            Debug.Log("FARMER_QUEST_CHECKS_FINISHED");
        }
        private static IEnumerator Capture(string path,string suffix)
        {
            yield return null;
            if(Application.isEditor)yield return null;else yield return new WaitForEndOfFrame();
            var card=GameObject.Find("Quest journal");
            if(card!=null){var c=new Vector3[4];card.GetComponent<RectTransform>().GetWorldCorners(c);Check(c.All(p=>p.x>=0&&p.y>=0&&p.x<=Screen.width&&p.y<=Screen.height),"Journal fits viewport");}
            ScreenCapture.CaptureScreenshot(Path.Combine(Path.GetDirectoryName(path),Path.GetFileNameWithoutExtension(path)+suffix+".png"));yield return new WaitForSecondsRealtime(.2f);
        }
#endif
    }
}
