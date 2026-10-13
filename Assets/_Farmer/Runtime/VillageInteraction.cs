using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
namespace Farmer
{
    [DefaultExecutionOrder(-400)]
    public sealed class VillageInteraction : MonoBehaviour
    {
        private FarmGame game;private FarmModel openedModel;
        private GameObject dialogue,context;private Transform guide;
        private Image portrait;private Text speaker,words,advance,hint;
        private int line,consumed=-1;private float reveal;
        private BlockRecord stove;private float cookRemaining;private FarmModel cookingModel;
        public bool IsOpen=>dialogue!=null&&dialogue.activeSelf;
        public bool ConsumedThisFrame=>consumed==Time.frameCount;
        public Transform Guide=>guide;
        public bool Cooking=>cookRemaining>0;
        public int Line=>line;
        public string Speaker=>line%2==0?"Defne":"Sen";
        public static readonly string[] Lines={
            "Hoş geldin! Ben Defne. Köyün çevresinde tavşanlar ve geyikler dolaşıyor. Seni fark ettiklerinde durup izlerler; fazla yaklaşırsan kaçarlar.",
            "Demek dikkatli yaklaşmam gerekiyor. Avladığım hayvanlardan ne elde edebilirim?",
            "Pazardan yay veya hançer alırsan avdan daha fazla et ve post çıkar. Balta ava zarar verir; verimi düşürür. Çantan doluysa yer açıp geri gelebilirsin.",
            "Etleri de öylece yiyemem sanırım. Nasıl pişireceğim?",
            "Pazarın Mobilyalar bölümünden bir ocak al. İnşa menüsünden yerleştir; yanında F'ye bas. Bir çiğ et ve bir odunla yemek hazırlarsın.",
            "Tamamdır. Pişmiş eti H ile yiyip canımı toparlarım; postları da sandığımda saklarım.",
            "Aynen öyle! Pişmiş et 40 can verir. İhtiyacın kadar avlan; toplanan avların yerine ertesi gün yenileri gelir.",
            "Sağ ol Defne. Önce ocağımı kurayım, sonra çevreyi keşfederim."
        };
        private void Start()
        {
            game=GetComponent<FarmGame>();
            var obj=VillageArtwork.Guide();obj.transform.SetParent(transform,false);
            var desired=game.Market.position+new Vector3(5,0,-3);
            obj.transform.position=FindGround(desired);guide=obj.transform;
            var canvas=new GameObject("Village dialogue UI",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvas.transform.SetParent(transform,false);
            canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;canvas.GetComponent<Canvas>().sortingOrder=30;
            var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280,720);scaler.matchWidthOrHeight=.5f;
            dialogue=Box(canvas.transform,"Dialogue overlay",new Rect(0,0,1280,720),new Color(.035f,.035f,.02f,.28f)).gameObject;
            portrait=Picture(dialogue.transform,"Speaker portrait",new Rect(10,170,370,550),"farmer");portrait.preserveAspect=true;
            Picture(dialogue.transform,"Dialogue parchment",new Rect(380,415,880,285),"dialogue_panel");
            speaker=Label(dialogue.transform,"Dialogue speaker",new Rect(445,449,235,38),25);speaker.fontStyle=FontStyle.Bold;
            words=Label(dialogue.transform,"Dialogue words",new Rect(445,514,745,113),23);words.alignment=TextAnchor.UpperLeft;words.horizontalOverflow=HorizontalWrapMode.Wrap;
            advance=Label(dialogue.transform,"Dialogue advance",new Rect(810,620,365,28),17);advance.alignment=TextAnchor.MiddleRight;
            var next=Box(dialogue.transform,"Dialogue next",new Rect(425,480,810,190),Color.clear).gameObject.AddComponent<Button>();next.onClick.AddListener(Advance);
            var close=Box(dialogue.transform,"Dialogue close",new Rect(1125,32,110,42),new Color(.18f,.12f,.06f,.75f)).gameObject.AddComponent<Button>();close.onClick.AddListener(Close);
            var closeText=Label(close.transform,"Close text",new Rect(0,0,110,42),18);closeText.text="Esc · Kapat";closeText.color=new Color(1,.93f,.76f);
            dialogue.SetActive(false);
            context=Box(canvas.transform,"Village interaction hint",new Rect(480,0,320,64),new Color(.92f,.82f,.61f,.98f)).gameObject;
            var hintRect=context.GetComponent<RectTransform>();hintRect.anchorMin=hintRect.anchorMax=new Vector2(.5f,0);hintRect.pivot=new Vector2(.5f,0);hintRect.anchoredPosition=new Vector2(0,208);
            HudArtwork.ApplyFrame(hintRect);hint=Label(context.transform,"Village hint",new Rect(20,14,280,36),17);context.SetActive(false);
        }
        private Vector3 FindGround(Vector3 desired)
        {
            for(int r=0;r<10;r++)for(int x=-r;x<=r;x++)for(int z=-r;z<=r;z++)
            {var p=new Vector3(Mathf.Floor(desired.x)+x+.5f,0,Mathf.Floor(desired.z)+z+.5f);if(WorldGround.SupportsCell(Mathf.FloorToInt(p.x),Mathf.FloorToInt(p.z),false)&&!Physics.OverlapSphere(p+Vector3.up*.8f,.65f).Any(c=>c.GetComponent<WorldGround>()==null))return p;}
            return desired;
        }
        private void Update()
        {
            if(game==null||!game.Ready)return;
            if(IsOpen)
            {
                context.SetActive(false);
                if(openedModel!=game.Model||game.MenuOpen||game.InventoryOpen||game.JournalOpen){Close();return;}
                reveal=Mathf.Min(Lines[line].Length,reveal+Time.unscaledDeltaTime*48);words.text=Lines[line].Substring(0,(int)reveal);
                if(!Application.isFocused)return;
                var k=Keyboard.current;
                if(k?.escapeKey.wasPressedThisFrame==true)Close();
                else if(k?.fKey.wasPressedThisFrame==true||k?.spaceKey.wasPressedThisFrame==true||k?.enterKey.wasPressedThisFrame==true)Advance();
                return;
            }
            if(game.WorldInputBlocked||game.BuildMode){context.SetActive(false);return;}
            if(Application.isFocused && TickCooking(Time.deltaTime))return;
            bool talk=CanTalk();stove=game.Model.Building.Blocks.Where(b=>b.pieceId=="home_stove"&&NearStove(b)).OrderBy(b=>Vector3.Distance(game.Player.position,new Vector3(b.x+.5f,b.level,b.z+.5f))).FirstOrDefault();
            context.SetActive(talk||stove!=null);
            if(talk)hint.text="F · Defne ile konuş";else if(stove!=null)hint.text="F · Et pişir   ·   1 çiğ et + 1 odun";
            if(Application.isFocused && Keyboard.current?.fKey.wasPressedThisFrame==true)
            {if(talk)TryTalk();else if(stove!=null){BeginCooking(stove);}}
            if(guide!=null&&CombatController.PlanarDistance(guide.position,game.Player.position)<7){var d=game.Player.position-guide.position;d.y=0;if(d.sqrMagnitude>.01f)guide.rotation=Quaternion.Slerp(guide.rotation,Quaternion.LookRotation(d),Time.deltaTime*3);}
        }
        public bool TickCooking(float delta)
        {
            if(!Cooking||game.WorldInputBlocked)return false;
            if(cookingModel!=game.Model||!NearStove(stove)){cookRemaining=0;game.ShowBuildFeedback("Pişirme iptal edildi; malzemeler korunuyor.");return false;}
            cookRemaining-=delta;hint.text=$"Pişiyor… {Mathf.CeilToInt(Mathf.Max(0,cookRemaining))} sn";context.SetActive(true);
            if(cookRemaining<=0){bool ok=game.Model.CookMeat(stove,out var message);game.CombatChanged(message);if(ok)CombatFeedback.Burst(new Vector3(stove.x+.5f,1.7f,stove.z+.5f),"+1 PİŞMİŞ ET",new Color(1,.8f,.3f));}
            return true;
        }
        public bool CanTalk()=>game!=null&&guide!=null&&CombatController.PlanarDistance(game.Player.position,guide.position)<=3.2f&&ClearLine(guide.position,guide);
        private bool ClearLine(Vector3 p,Transform target)
        {var from=game.Player.position+Vector3.up*.9f;var to=p+Vector3.up*.9f;return !Physics.RaycastAll(from,(to-from).normalized,Vector3.Distance(from,to),~0,QueryTriggerInteraction.Ignore).Any(h=>!h.transform.IsChildOf(game.Player)&&!h.transform.IsChildOf(target));}
        public bool TryTalk()
        {
            if(game==null||!game.Ready||game.WorldInputBlocked||game.BuildMode||!CanTalk())return false;
            consumed=Time.frameCount;openedModel=game.Model;line=0;dialogue.SetActive(true);game.Selection.ModalBlocked=true;RefreshLine();return true;
        }
        public void Advance()
        {
            if(!IsOpen)return;consumed=Time.frameCount;
            if(reveal<Lines[line].Length){reveal=Lines[line].Length;words.text=Lines[line];return;}
            if(++line>=Lines.Length){line=Lines.Length-1;Close();return;}RefreshLine();
        }
        private void RefreshLine(){reveal=0;words.text="";speaker.text=Speaker;portrait.sprite=VillageArtwork.Icon(line%2==0?"defne":"farmer");advance.text=line==Lines.Length-1?"F / Space · Bitir":"F / Space · Devam  ›";}
        public void Close(){if(dialogue==null)return;dialogue.SetActive(false);consumed=Time.frameCount;if(game!=null)game.Selection.ModalBlocked=game.MenuOpen||game.InventoryOpen||game.MarketOpen||game.JournalOpen;}
        public bool NearStove(BlockRecord b)
        {
            if(b==null||CombatController.PlanarDistance(game.Player.position,new Vector3(b.x+.5f,b.level,b.z+.5f))>2.7f||Mathf.Abs(game.Player.position.y-b.level)>1.6f)return false;
            var view=Object.FindObjectsByType<PlacedBlockView>(FindObjectsSortMode.None).FirstOrDefault(v=>v.Record.pieceId=="home_stove"&&v.Record.x==b.x&&v.Record.z==b.z&&v.Record.level==b.level);
            return view!=null&&ClearLine(view.transform.position,view.transform);
        }
        public bool BeginCooking(BlockRecord b)
        {
            if(Cooking||game.WorldInputBlocked||game.BuildMode||!NearStove(b))return false;
            consumed=Time.frameCount;
            if(game.Model.BagCount("hunt:raw_meat")<1||game.Model.Building.Wood<1){game.ShowBuildFeedback("1 çiğ et ve 1 odun gerekiyor.");return false;}
            stove=b.Copy();cookingModel=game.Model;cookRemaining=2.5f;return true;
        }
        private static RectTransform Box(Transform parent,string name,Rect r,Color color)
        {var t=new GameObject(name,typeof(RectTransform),typeof(Image)).GetComponent<RectTransform>();t.SetParent(parent,false);t.anchorMin=t.anchorMax=t.pivot=new Vector2(0,1);t.anchoredPosition=new Vector2(r.x,-r.y);t.sizeDelta=r.size;t.GetComponent<Image>().color=color;return t;}
        private static Image Picture(Transform parent,string name,Rect r,string asset)
        {var t=Box(parent,name,r,Color.white);var im=t.GetComponent<Image>();im.sprite=VillageArtwork.Icon(asset);im.raycastTarget=false;return im;}
        private static Text Label(Transform parent,string name,Rect r,int size)
        {var t=Box(parent,name,r,Color.clear);DestroyImmediate(t.GetComponent<Image>());var txt=t.gameObject.AddComponent<Text>();txt.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");txt.fontSize=size;txt.color=new Color(.23f,.14f,.07f);txt.raycastTarget=false;txt.alignment=TextAnchor.MiddleCenter;return txt;}
    }
}
