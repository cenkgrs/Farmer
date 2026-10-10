using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Farmer
{
    [DefaultExecutionOrder(-300)]
    public sealed class SessionMenu : MonoBehaviour
    {
        private FarmGame game;
        private GameObject overlay,illustration;
        private Text subtitle, screenLabel;
        private Button resume;
        private bool started;
        public bool IsOpen => game.MenuOpen;
        private void Awake()
        {
            game=GetComponent<FarmGame>();
            // Automated players deliberately enter their isolated fixture directly.
            started=Array.IndexOf(Environment.GetCommandLineArgs(),"--farmer-smoke-capture")>=0&&Array.IndexOf(Environment.GetCommandLineArgs(),"--farmer-check-menu")<0;
            game.MenuOpen=!started;
        }
        private void Start()
        {
            var canvas=new GameObject("Session Menu",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            canvas.transform.SetParent(transform,false);
            canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            canvas.GetComponent<Canvas>().sortingOrder=100;
            var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1280,720);scaler.matchWidthOrHeight=.5f;
            var artwork=new GameObject("Farm illustration",typeof(RectTransform),typeof(RawImage),typeof(AspectRatioFitter));
            artwork.transform.SetParent(canvas.transform,false);
            var texture=Resources.Load<Texture2D>("MenuArt/farm_background");
            artwork.GetComponent<RawImage>().texture=texture;artwork.GetComponent<RawImage>().raycastTarget=false;
            var fit=artwork.GetComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.EnvelopeParent;fit.aspectRatio=(float)texture.width/texture.height;
            var dim=FarmHud.Panel("Menu Overlay",canvas.transform,Vector2.one*.5f,Vector2.zero,new Vector2(4000,4000),new Color(.055f,.085f,.065f,.32f));
            overlay=dim.gameObject;
            var card=FarmHud.Panel("Menu Card",dim,Vector2.one*.5f,new Vector2(-330,0),new Vector2(440,490),new Color(.94f,.85f,.66f));
            var title=FarmHud.Label(card,"F A R M E R",38,25,52,390,55,new Color(.28f,.20f,.10f));title.alignment=TextAnchor.MiddleCenter;
            subtitle=FarmHud.Label(card,"",15,30,120,380,56,new Color(.40f,.34f,.22f));subtitle.alignment=TextAnchor.MiddleCenter;
            resume=FarmHud.Button(card,"Devam Et",55,204,330,52,Continue);
            var screen=FarmHud.Button(card,"",55,272,330,46,ToggleFullscreen);screen.name="Fullscreen Button";
            screenLabel=screen.GetComponentInChildren<Text>();
            FarmHud.Button(card,"Kaydet ve Çık",55,334,330,46,Quit);
            var footer=FarmHud.Label(card,"Çiftliğine dön. Kendi yolunu çiz.",13,25,428,390,28,new Color(.47f,.41f,.29f));footer.alignment=TextAnchor.MiddleCenter;
            artwork.SetActive(game.MenuOpen);
            illustration=artwork;SetOpen(game.MenuOpen);
        }
        private void Update()
        {
            if(Keyboard.current?.f11Key.wasPressedThisFrame==true)ToggleFullscreen();
            if(Keyboard.current?.escapeKey.wasPressedThisFrame==true&&!game.InventoryOpen&&!game.MarketOpen&&!game.BuildMode)
            {if(IsOpen&&started)Continue();else SetOpen(true);}
            if(screenLabel!=null)screenLabel.text="Tam Ekran: "+(Screen.fullScreen?"Açık":"Kapalı");
        }
        public void SetOpen(bool open)
        {
            if(open)game.GetComponent<MarketInteraction>()?.Close();
            game.MenuOpen=open;game.Selection.ModalBlocked=open||game.InventoryOpen||game.MarketOpen;
            if(overlay!=null)overlay.SetActive(open);if(illustration!=null)illustration.SetActive(open);
            if(subtitle!=null)subtitle.text=game.Ready?$"Gün {game.Model.Day} · {game.Model.ClockText}\n"+(started?"Biraz soluklan.":game.HadSaveAtStartup?"Çiftliğin seni bekliyor.":"Yeni bir başlangıç."):game.SaveStatus;
            if(resume!=null){resume.interactable=game.Ready;resume.GetComponentInChildren<Text>().text=started||game.HadSaveAtStartup?"Devam Et":"Başla";}
        }
        public void Continue(){if(!game.Ready)return;started=true;SetOpen(false);}
        public void ToggleFullscreen()
        {
            if(Screen.fullScreen)Screen.SetResolution(1280,720,FullScreenMode.Windowed);
            else Screen.SetResolution(Screen.currentResolution.width,Screen.currentResolution.height,FullScreenMode.FullScreenWindow);
        }
        private void Quit()
        {
            if(game.Ready&&!game.SaveGame()){subtitle.text=game.SaveStatus;return;}
            Application.Quit();
        }
    }
}
