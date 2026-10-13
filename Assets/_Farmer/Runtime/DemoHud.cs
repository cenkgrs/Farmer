using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Farmer
{
    [DefaultExecutionOrder(-350)]
    public sealed class DemoHud : MonoBehaviour
    {
        private FarmGame game;
        private GameObject overlay, hint;
        private Text progress, navigation, settings;
        private int consumed=-1, destination=-1, fps=60;
        private bool qa;
        private float nextRefresh;
        public bool ConsumedThisFrame=>consumed==Time.frameCount;
        public int Destination=>destination;
        private void Start()
        {
            game=GetComponent<FarmGame>();qa=Environment.GetCommandLineArgs().Contains("--farmer-smoke-capture");
            AudioListener.volume=qa?1:Mathf.Clamp01(PlayerPrefs.GetFloat("Farmer.Volume",.8f));
            fps=qa?60:PlayerPrefs.GetInt("Farmer.Fps",60);if(fps!=30&&fps!=60&&fps!=120)fps=60;Application.targetFrameRate=fps;
            var canvas=new GameObject("Journey and Settings",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvas.transform.SetParent(transform,false);
            canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;canvas.GetComponent<Canvas>().overrideSorting=true;canvas.GetComponent<Canvas>().sortingOrder=110;
            var scale=canvas.GetComponent<CanvasScaler>();scale.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scale.referenceResolution=new Vector2(1280,720);scale.matchWidthOrHeight=.5f;
            var help=FarmHud.Button(canvas.transform,"J · Çiftlik defteri",24,0,174,34,()=>SetOpen(true));var hr=help.GetComponent<RectTransform>();hr.anchorMin=hr.anchorMax=hr.pivot=new Vector2(0,0);hr.anchoredPosition=new Vector2(24,20);hint=help.gameObject;
            var nav=FarmHud.Panel("Destination",canvas.transform,new Vector2(1,1),new Vector2(-24,-20),new Vector2(250,64),new Color(.94f,.86f,.69f));HudArtwork.ApplyFrame(nav);nav.GetComponent<Image>().raycastTarget=false;
            navigation=FarmHud.Label(nav,"",14,22,22,206,24,new Color(.28f,.21f,.13f));navigation.transform.parent.gameObject.SetActive(false);
            var dim=FarmHud.Panel("Journal backdrop",canvas.transform,Vector2.one*.5f,Vector2.zero,new Vector2(4000,4000),new Color(.05f,.08f,.05f,.65f));overlay=dim.gameObject;
            var card=FarmHud.Panel("Farm journal",dim,Vector2.one*.5f,Vector2.zero,new Vector2(930,610),new Color(.94f,.87f,.72f));
            HudArtwork.ApplyFrame(card);
            FarmHud.Label(card,"ÇİFTLİK DEFTERİ",27,28,22,800,40,new Color(.28f,.21f,.13f));
            FarmHud.Button(card,"×",864,18,40,38,()=>SetOpen(false));
            FarmHud.Label(card,"Kendi hızında ilerle. Savaş isteğe bağlı; çiftliğin güvenli.",14,30,68,840,26,new Color(.38f,.31f,.21f));
            progress=FarmHud.Label(card,"",16,30,110,545,316,new Color(.28f,.23f,.15f));
            FarmHud.Label(card,"YOLUNU BUL",17,610,110,270,30,new Color(.28f,.23f,.15f));
            string[] locations={"Pazar", "Çiftliğe dönüş", "Orman açıklığı", "Maden girişi"};
            for(int i=0;i<locations.Length;i++){int id=i;FarmHud.Button(card,locations[i],610,150+i*43,275,35,()=>{Track(id);SetOpen(false);});}
            FarmHud.Button(card,"Yönlendirmeyi kapat",610,326,275,30,()=>Track(-1));
            settings=FarmHud.Label(card,"",14,610,382,275,52,new Color(.28f,.23f,.15f));
            FarmHud.Button(card,"Ses −",610,440,85,32,()=>ChangeVolume(-.1f));FarmHud.Button(card,"Ses +",703,440,85,32,()=>ChangeVolume(.1f));FarmHud.Button(card,"FPS",796,440,89,32,CycleFps);
            FarmHud.Button(card,"Tam ekran / pencere",610,482,275,34,()=>game.GetComponent<SessionMenu>().ToggleFullscreen());
            FarmHud.Label(card,"WASD / oklar: hareket   ·   Tekerlek: yakınlaştır\n1 tohum · 2 su · 3 orak · 4 inşa · 5 çapa · 6 balta · 7 kazma\nSol tık: eldeki alet / hedef   ·   F: pazar, kapı, sandık\nTab: çanta   ·   N: yatakta uyu   ·   Space: kaçın   ·   H: sebze ye\nF5: kaydet   ·   F9: yükle   ·   Esc: menü   ·   J: bu defter",13,30,434,555,133,new Color(.38f,.31f,.21f));
            FarmHud.Label(card,"Demo 0.5 · İlerleme otomatik kaydedilir.",12,610,552,290,25,new Color(.38f,.31f,.21f));
            overlay.SetActive(false);Refresh();
        }
        private void Update()
        {
            if(game==null)return;
            if(game.QuestOpen||game.QuestInputConsumed||game.ConversationOpen||game.VillageInputConsumed){Refresh();return;}
            if(Application.isFocused)
            {
                var key=Keyboard.current;
                if(key?.jKey.wasPressedThisFrame==true){SetOpen(!game.JournalOpen);consumed=Time.frameCount;}
                else if(game.JournalOpen&&key?.escapeKey.wasPressedThisFrame==true){SetOpen(false);consumed=Time.frameCount;}
            }
            if(Time.unscaledTime>=nextRefresh){nextRefresh=Time.unscaledTime+.15f;Refresh();}
        }
        public void SetOpen(bool open)
        {
            if(game==null||overlay==null)return;
            if(open&&(game.QuestOpen||game.QuestInputConsumed||!game.Ready||game.InventoryOpen||game.MarketOpen||game.BuildMode))return;
            game.JournalOpen=open;game.Selection.ModalBlocked=open||game.MenuOpen||game.InventoryOpen||game.MarketOpen;overlay.SetActive(open);Refresh();
        }
        public void Track(int id){if(id < -1 || id > 3)return;destination=id;Refresh();}
        public void ChangeVolume(float delta)
        {AudioListener.volume=Mathf.Clamp01(AudioListener.volume+delta);if(!qa){PlayerPrefs.SetFloat("Farmer.Volume",AudioListener.volume);PlayerPrefs.Save();}Refresh();}
        public void CycleFps()
        {fps=fps==30?60:fps==60?120:30;Application.targetFrameRate=fps;if(!qa){PlayerPrefs.SetInt("Farmer.Fps",fps);PlayerPrefs.Save();}Refresh();}
        public void Refresh()
        {
            if(game==null||progress==null||!game.Ready)return;
            string Check(JourneyStep step)=>game.Model.Completed(step)?"✓":"○";
            progress.text=$"{Check(JourneyStep.Seeds)}  Pazardan tohum al\n     Sağdaki Pazar yönünü izle; yaklaşınca F.\n{Check(JourneyStep.Plant)}  Boş toprağı hazırla ve ek\n     5 + sol tık çapala; 1 ile tohumunu ek.\n{Check(JourneyStep.Water)}  Bir kez sula ve büyümeyi bekle\n     2 + sol tık. Bir gün 10 dakika; turp 3 gece.\n{Check(JourneyStep.Harvest)}  İlk hasadını topla\n     3 + sol tık. Beklerken ağaç kes, evini kur.\n{Check(JourneyStep.Sale)}  Hasadını pazarda sat\n     F → satış; yeniden tohum veya yatak al.\n{Check(JourneyStep.Build)}  İlk yapı parçanı yerleştir\n     6 ile odun topla; 4 ile inşa et.\n{Check(JourneyStep.Explore)}  Dışarıdaki kaynak zulasını bul\n     Orman: odun / Maden: kazmayla taş.";
            settings.text=$"Ses: %{Mathf.RoundToInt(AudioListener.volume*100)}\nKare sınırı: {fps} FPS";
            hint.SetActive(!game.WorldInputBlocked&&!game.BuildMode);
            var combat=game.GetComponent<CombatController>();Vector3? target=null;string label="";
            if(destination==0){target=game.Market.position;label="Pazar";}
            else if(destination==1&&combat!=null){target=combat.Sanctuary;label="Çiftlik";}
            else if(destination>=2&&combat!=null&&combat.View(destination-2)!=null){target=combat.View(destination-2).Home;label=EnemyRules.Catalog[destination-2].Area;}
            navigation.transform.parent.gameObject.SetActive(target.HasValue&&!game.WorldInputBlocked&&!game.BuildMode);
            if(target.HasValue&&Camera.main!=null)
            {
                var delta=target.Value-game.Player.position;delta.y=0;var forward=Camera.main.transform.forward;forward.y=0;forward.Normalize();var right=Camera.main.transform.right;right.y=0;right.Normalize();
                float angle=Mathf.Atan2(Vector3.Dot(delta,right),Vector3.Dot(delta,forward))*Mathf.Rad2Deg;
                string[] arrows={"↑","↗","→","↘","↓","↙","←","↖"};int n=((Mathf.RoundToInt(angle/45)%8)+8)%8;
                navigation.text=$"{arrows[n]}  {label} · {Mathf.RoundToInt(delta.magnitude)} m";
            }
        }
    }
}
