using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Farmer
{
    [DefaultExecutionOrder(-450)]
    public sealed class QuestHud : MonoBehaviour
    {
        private FarmGame game;
        private GameObject overlay, hint;
        private Text title, description, objectives, reward, status, footer;
        private Text[] rows;
        private Button[] buttons;
        private int selected,page,consumed=-1;
        private Button previousPage,nextPage;private Text pageLabel;
        private RectTransform detailContent;private ScrollRect detailScroll;private Text rewardHeading;
        private float nextRefresh;
        public bool IsOpen=>overlay!=null&&overlay.activeSelf;
        public bool ConsumedThisFrame=>consumed==Time.frameCount;
        private static readonly Color Ink=new Color(.27f,.17f,.08f);
        private void Start()
        {
            game=GetComponent<FarmGame>();
            var canvas=new GameObject("Quest UI",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvas.transform.SetParent(transform,false);
            canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;canvas.GetComponent<Canvas>().sortingOrder=105;
            var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1280,720);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
            hint=FarmHud.Button(canvas.transform,"Q · Görevler",210,0,145,34,()=>SetOpen(true)).gameObject;
            var hr=(RectTransform)hint.transform;hr.anchorMin=hr.anchorMax=hr.pivot=Vector2.zero;hr.anchoredPosition=new Vector2(210,20);
            var dim=FarmHud.Panel("Quest backdrop",canvas.transform,Vector2.one*.5f,Vector2.zero,new Vector2(4000,4000),new Color(.04f,.06f,.03f,.65f));overlay=dim.gameObject;
            var card=FarmHud.Panel("Quest journal",dim,Vector2.one*.5f,Vector2.zero,new Vector2(960,640),Color.white);
            card.GetComponent<Image>().sprite=VillageArtwork.Icon("quest_panel");
            Text Label(string text,int size,float x,float y,float w,float h)=>FarmHud.Label(card,text,size,x,y,w,h,Ink);
            Label("KÖY GÖREVLERİ",30,80,80,740,40).alignment=TextAnchor.MiddleCenter;
            StyledButton(card,"×",840,81,42,38,()=>SetOpen(false));
            rows=new Text[3];buttons=new Button[rows.Length];
            for(int i=0;i<rows.Length;i++)
            {
                int index=i;buttons[i]=StyledButton(card,"",66,165+i*108,225,90,()=>{selected=page*3+index;Refresh();detailScroll.verticalNormalizedPosition=1;});
                rows[i]=buttons[i].GetComponentInChildren<Text>();rows[i].fontSize=18;
            }
            title=Label("",26,388,159,490,40);title.fontStyle=FontStyle.Bold;
            status=Label("",15,390,202,490,28);
            var viewport=FarmHud.Panel("Quest details viewport",card,new Vector2(0,1),new Vector2(390,-241),new Vector2(485,292),Color.clear);
            Destroy(viewport.GetComponent<Outline>());viewport.gameObject.AddComponent<RectMask2D>();
            detailContent=new GameObject("Quest details",typeof(RectTransform)).GetComponent<RectTransform>();detailContent.SetParent(viewport,false);
            detailContent.anchorMin=detailContent.anchorMax=detailContent.pivot=new Vector2(0,1);detailContent.sizeDelta=new Vector2(475,292);
            detailScroll=viewport.gameObject.AddComponent<ScrollRect>();detailScroll.viewport=viewport;detailScroll.content=detailContent;detailScroll.horizontal=false;detailScroll.movementType=ScrollRect.MovementType.Clamped;detailScroll.scrollSensitivity=24;
            description=FarmHud.Label(detailContent,"",19,0,0,475,100,Ink);description.horizontalOverflow=HorizontalWrapMode.Wrap;
            objectives=FarmHud.Label(detailContent,"",21,0,110,475,80,Ink);
            rewardHeading=FarmHud.Label(detailContent,"ÖDÜL",15,0,220,475,25,Ink);rewardHeading.fontStyle=FontStyle.Bold;
            reward=FarmHud.Label(detailContent,"",22,0,248,475,44,Ink);
            previousPage=StyledButton(card,"‹",68,492,46,35,()=>ChangePage(-1));
            nextPage=StyledButton(card,"›",248,492,46,35,()=>ChangePage(1));
            pageLabel=Label("",16,120,497,123,26);pageLabel.alignment=TextAnchor.MiddleCenter;
            footer=Label("",15,95,552,770,28);footer.alignment=TextAnchor.MiddleCenter;
            overlay.SetActive(false);
        }
        public static Button StyledButton(Transform parent,string text,float x,float y,float w,float h,UnityEngine.Events.UnityAction action)
        {
            var b=FarmHud.Button(parent,text,x,y,w,h,action);HudArtwork.ApplyFrame((RectTransform)b.transform,6);
            b.GetComponentInChildren<Text>().transform.SetAsLastSibling();return b;
        }
        private void Update()
        {
            if(game==null||!game.Ready)return;
            if(Application.isFocused)
            {
                HandleInput(Keyboard.current?.qKey.wasPressedThisFrame==true,Keyboard.current?.escapeKey.wasPressedThisFrame==true);
            }
            hint.SetActive(!game.WorldInputBlocked&&!game.BuildMode);
            if(IsOpen&&Time.unscaledTime>=nextRefresh){nextRefresh=Time.unscaledTime+.15f;Refresh();}
        }
        public void HandleInput(bool toggle,bool escape)
        {if(toggle)SetOpen(!IsOpen);else if(IsOpen&&escape)SetOpen(false);}
        public void SelectQuest(string id)
        {for(int i=0;i<game.Model.QuestCatalog.Count;i++)if(game.Model.QuestCatalog[i].Id==id){selected=i;page=i/3;Refresh();detailScroll.verticalNormalizedPosition=1;return;}}
        public bool SetOpen(bool open)
        {
            if(game==null||overlay==null)return false;
            if(open&&(!game.Ready||game.WorldInputBlocked))return false;
            if(open&&game.BuildMode)game.SetBuildMode(false);
            consumed=Time.frameCount;overlay.SetActive(open);game.Selection.ModalBlocked=open||game.MenuOpen||game.InventoryOpen||game.MarketOpen||game.JournalOpen||game.ConversationOpen;
            if(open)Refresh();return true;
        }
        public void Refresh()
        {
            if(game==null||title==null)return;
            var catalog=game.Model.QuestCatalog;
            if(catalog.Count==0){foreach(var b in buttons)b.gameObject.SetActive(false);previousPage.gameObject.SetActive(false);nextPage.gameObject.SetActive(false);pageLabel.gameObject.SetActive(false);title.text="Henüz görev yok";status.text="";description.text="Köydeki karakterlerle konuş.";objectives.text=reward.text="";footer.text="Q / Esc: Kapat";return;}
            selected=Mathf.Clamp(selected,0,catalog.Count-1);page=Mathf.Clamp(page,0,(catalog.Count-1)/3);
            previousPage.gameObject.SetActive(catalog.Count>3);nextPage.gameObject.SetActive(catalog.Count>3);pageLabel.gameObject.SetActive(catalog.Count>3);
            previousPage.interactable=page>0;nextPage.interactable=(page+1)*3<catalog.Count;pageLabel.text=(page+1)+" / "+((catalog.Count+2)/3);
            for(int i=0;i<rows.Length;i++)
            {
                int index=page*3+i;buttons[i].gameObject.SetActive(index<catalog.Count);if(index>=catalog.Count)continue;
                var q=catalog[index];rows[i].text=q.Title+"\n"+q.NpcName+" · "+StateText(q.Id);rows[i].resizeTextForBestFit=true;rows[i].resizeTextMinSize=14;rows[i].resizeTextMaxSize=18;
                rows[i].fontStyle=index==selected?FontStyle.Bold:FontStyle.Normal;rows[i].color=index==selected?new Color(.25f,.34f,.14f):Ink;
                buttons[i].GetComponent<Image>().color=index==selected?new Color(.88f,.90f,.69f):new Color(1,.96f,.85f);
            }
            var chosen=catalog[selected];var state=game.Model.QuestState(chosen.Id);
            title.text=chosen.Title;status.text=chosen.NpcName+"  ·  "+StateText(chosen.Id);
            description.text=state==QuestStatus.Locked?"Önce “"+game.Model.FindQuest(chosen.Prerequisite).Title+"” görevini tamamla.":chosen.Description;
            objectives.text=string.Join("\n",chosen.Requirements.Select(r=>r.Label+"    "+(state==QuestStatus.Completed?r.Count:Mathf.Min(game.Model.BagCount(r.Item),r.Count))+" / "+r.Count));
            reward.text=chosen.RewardText;
            float y=0;
            foreach(var text in new[]{description,objectives,rewardHeading,reward})
            {var rect=(RectTransform)text.transform;float height=Mathf.Max(text.fontSize+8,text.preferredHeight+8);rect.anchoredPosition=new Vector2(0,-y);rect.sizeDelta=new Vector2(475,height);y+=height+18;}
            detailContent.sizeDelta=new Vector2(475,Mathf.Max(292,y));
            footer.text=state==QuestStatus.Completed?"Ödül alındı.   ·   Q / Esc: Kapat":state==QuestStatus.Locked?"Görev zincirinde ilerle.   ·   Q / Esc: Kapat":"Almak ve teslim etmek için "+chosen.NpcName+" ile konuş.   ·   Q / Esc: Kapat";
        }
        private void ChangePage(int delta){page+=delta;selected=page*3;Refresh();detailScroll.verticalNormalizedPosition=1;}
        private string StateText(string id)
        {
            switch(game.Model.QuestState(id))
            {
                case QuestStatus.Completed:return "Tamamlandı";
                case QuestStatus.Active:return game.Model.QuestReady(id)?"Teslime hazır":"Devam ediyor";
                case QuestStatus.Available:return "Alınabilir";
                default:return "Kilitli";
            }
        }
    }
}
