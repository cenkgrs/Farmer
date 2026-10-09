using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Farmer
{
    public sealed class FarmHud : MonoBehaviour
    {
        [SerializeField] private FarmGame game;
        private Text summary,money,saveStatus,selectionStatus,feedback,campWarning,tooltipText;
        private RectTransform shop,camp,detail,tooltip,toast,buildMenu,normalBar;
        private Button buyOne,buyFive,buyWood,sell,sleep,buildButton,buyBed,moveButton;
        private Button[] buildChoices;
        private bool furnitureCategory;
        private int[] categoryIndices;
        private Image[] buildIcons;
        private Button[] furnitureBuy;
        private BuildDefinition[] furnitureSale;
        private RectTransform shopFurniture,buildTabs;
        private Text[] buildCounts;
        private bool displayedBuildMode;
        private BuildController builder;
        private readonly Button[] slots=new Button[5];
        private Text seedCount,produceCount,woodCount;
        private string hovered;
        private CanvasGroup visibility;
        private static Sprite rounded;
        private static readonly Color Cream=new Color(.94f,.85f,.66f),Ink=new Color(.25f,.19f,.12f),Wood=Color.white,Gold=new Color(1f,.63f,.08f,.32f);
        public void Configure(FarmGame source)=>game=source;
        private void Start()
        {
            builder=game.GetComponent<BuildController>();
            var root=new GameObject("Farm HUD",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));root.transform.SetParent(transform,false);
            root.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            visibility=root.AddComponent<CanvasGroup>();
            var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280,720);scaler.matchWidthOrHeight=.5f;
            if(EventSystem.current==null)
            {
                var events=new GameObject("Farm UI Input",typeof(EventSystem),typeof(InputSystemUIInputModule));events.transform.SetParent(transform,false);
                events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
            var header=Panel("Clock and Wallet",root.transform,new Vector2(0,1),new Vector2(24,-20),new Vector2(224,76),Cream);
            summary=Label(header,"",20,14,9,196,26,Ink);money=Label(header,"",15,14,41,126,24,Ink);
            saveStatus=Label(header,"",10,130,46,88,18,new Color(.40f,.39f,.23f));
            detail=Panel("Cell Details",root.transform,new Vector2(1,1),new Vector2(-24,-20),new Vector2(244,110),Cream);
            selectionStatus=Label(detail,"",15,14,12,216,90,Ink);
            shop=Panel("Market",root.transform,new Vector2(0,1),new Vector2(24,-112),new Vector2(250,310),Cream);
            Label(shop,"Tohum & malzeme",20,14,10,224,28,Ink);
            Label(shop,$"{game.ActiveCrop.displayName} · satış {game.ActiveCrop.salePrice} para",13,14,41,224,22,Ink);
            buyOne=Button(shop,$"B · 1 tohum al ({game.ActiveCrop.seedPrice})",14,72,222,36,()=>game.Buy(1));
            buyFive=Button(shop,$"5 tohum al ({game.ActiveCrop.seedPrice*5})",14,116,222,36,()=>game.Buy(5));
            sell=Button(shop,"V · Ürünlerin hepsini sat",14,160,222,36,()=>game.SellHarvest());
            buyWood=Button(shop,"10 odun al (20 para)",14,204,222,36,()=>game.BuyWood());
            buyBed=Button(shop,"Yatak al (100 para)",14,248,222,36,()=>game.BuyBed());
            camp=Panel("Sleep",root.transform,new Vector2(0,1),new Vector2(24,-112),new Vector2(250,140),Cream);
            Label(camp,"Biraz dinlen",20,14,10,224,28,Ink);
            campWarning=Label(camp,"",13,14,43,224,44,Ink);
            sleep=Button(camp,"N · Sabaha kadar uyu",14,91,222,35,()=>game.Rest());

            buildMenu=Panel("Construction Inventory",root.transform,new Vector2(.5f,0),new Vector2(0,20),new Vector2(541,85),Wood);
            buildMenu.GetComponent<Image>().sprite=ConstructionArtwork.Get("construction_frame");buildMenu.GetComponent<Image>().type=Image.Type.Simple;
            Object.Destroy(buildMenu.GetComponent<Outline>());
            buildChoices=new Button[8];buildCounts=new Text[8];buildIcons=new Image[8];
            for(int i=0;i<8;i++)
            {
                int slot=i;var button=Button(buildMenu,"",22+i*63,14,54,54,()=>{if(slot<categoryIndices.Length)builder.SelectPiece(categoryIndices[slot]);});
                button.name="Construction Slot "+i;buildChoices[i]=button;
                Object.Destroy(button.GetComponentInChildren<Text>().gameObject);Object.Destroy(button.GetComponent<Outline>());button.GetComponent<Image>().color=Color.clear;
                var obj=new GameObject("Construction Icon",typeof(RectTransform),typeof(Image));var r=obj.GetComponent<RectTransform>();r.SetParent(button.transform,false);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=new Vector2(6,7);r.offsetMax=new Vector2(-6,-5);
                buildIcons[i]=obj.GetComponent<Image>();buildIcons[i].preserveAspect=true;buildIcons[i].raycastTarget=false;
                buildCounts[i]=Label(button.transform,"",12,28,37,23,17,Ink);buildCounts[i].alignment=TextAnchor.MiddleRight;
            }
            buildTabs=Panel("Build categories",root.transform,new Vector2(.5f,0),new Vector2(0,108),new Vector2(250,34),Cream);
            Button(buildTabs,"Yapı",6,3,110,28,()=>{SetCategory(false);builder.SelectPiece(categoryIndices[0]);});
            Button(buildTabs,"Mobilya",128,3,110,28,()=>{SetCategory(true);builder.SelectPiece(categoryIndices[0]);});
            SetCategory(false);
            shopFurniture=Panel("Furniture Market",root.transform,new Vector2(0,1),new Vector2(285,-112),new Vector2(250,248),Cream);
            Label(shopFurniture,"Mobilyalar",20,14,10,224,28,Ink);
            furnitureSale=game.BuildPieces.Where(d=>d.price>0&&!d.isBed).ToArray();furnitureBuy=new Button[furnitureSale.Length];
            for(int i=0;i<furnitureSale.Length;i++){var p=furnitureSale[i];furnitureBuy[i]=Button(shopFurniture,$"{p.displayName} ({p.price})",14,50+i*44,222,36,()=>game.BuyFurniture(p.id));}
            moveButton=Button(buildMenu,"M · Tutup taşı",551,28,150,28,()=>builder.ToggleMoveMode());
            var bar=Panel("Inventory Bar",root.transform,new Vector2(.5f,0),new Vector2(0,20),new Vector2(541,85),Wood);
            normalBar=bar;
            var frame=bar.GetComponent<Image>();frame.sprite=ConstructionArtwork.Get("construction_frame");frame.type=Image.Type.Simple;
            Object.Destroy(bar.GetComponent<Outline>());
            // Display order matches the existing shortcuts, but the slots themselves show items rather than instructions.
            int[] order={0,1,2,4,3,8,5,6};
            for(int i=0;i<order.Length;i++)
            {
                int kind=order[i];float x=22+i*63;
                var button=Button(bar,"",x,14,54,54,()=>{if(kind<4)game.Equip((FarmItem)kind);else if(kind==4)builder.ToggleMode();else if(kind==8)game.Equip(FarmItem.Axe);});
                Object.Destroy(button.GetComponentInChildren<Text>().gameObject);
                Object.Destroy(button.GetComponent<Outline>());
                var iconObj=new GameObject("Item Icon",typeof(RectTransform),typeof(InventoryIcon));var rect=iconObj.GetComponent<RectTransform>();rect.SetParent(button.transform,false);rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=new Vector2(6,7);rect.offsetMax=new Vector2(-6,-5);
                var icon=iconObj.GetComponent<InventoryIcon>();icon.Kind=kind;icon.raycastTarget=false;
                var count=Label(button.transform,"",12,28,36,23,17,Ink);count.alignment=TextAnchor.MiddleRight;
                if(kind<4){slots[kind]=button;button.name="Inventory Slot "+kind;}
                if(kind==4){buildButton=button;button.name="Build Mode Button";}
                if(kind==8){slots[4]=button;button.name="Inventory Slot 4";}
                if(kind==0)seedCount=count;if(kind==5)produceCount=count;if(kind==6)woodCount=count;
                string hint=kind==0?"Turp tohumu · 1\nHazır toprağa sol tıkla ek.":kind==1?"Sulama kabı · 2\nSol tuşu basılı tutarak sula.":kind==2?"Orak · 3\nOlgun ürünü sol tıkla hasat et.":kind==3?"Çapa · 5\nBoş toprağı sol tıkla hazırla.":kind==4?"İnşa · 4\nYapı kurmak için seç.":kind==5?"Turp\nHasadını pazarda satabilirsin.":kind==8?"Balta · 6\nAğaca sol tıkla odun topla.":"Odun\nAğaç keserek veya pazardan alınır.";
                AddHover(button.gameObject,hint.Split('\n')[0]);
                button.GetComponent<Image>().color=Color.clear;
            }
            tooltip=Panel("Item Tooltip",root.transform,new Vector2(.5f,0),new Vector2(0,106),new Vector2(440,56),Cream);
            tooltipText=Label(tooltip,"",14,12,8,416,44,Ink);tooltipText.alignment=TextAnchor.MiddleCenter;
            toast=Panel("Feedback Toast",root.transform,new Vector2(.5f,0),new Vector2(0,172),new Vector2(570,36),new Color(.20f,.24f,.17f,.92f));
            feedback=Label(toast,"",13,10,7,550,25,Cream);feedback.alignment=TextAnchor.MiddleCenter;
            game.Selection.SetHudPanels(new[]{header,detail,shop,camp,bar,tooltip,toast,buildMenu,buildTabs,shopFurniture,(RectTransform)moveButton.transform});Refresh();
        }
        private void SetCategory(bool furniture)
        {
            furnitureCategory=furniture;
            categoryIndices=Enumerable.Range(0,game.BuildPieces.Length).Where(i=>game.BuildPieces[i].IsFurniture==furniture).ToArray();
            for(int i=0;i<8;i++)
            {
                bool filled=i<categoryIndices.Length;buildChoices[i].interactable=filled;
                buildIcons[i].enabled=filled;buildIcons[i].sprite=filled?ConstructionArtwork.Get(game.BuildPieces[categoryIndices[i]].id):null;
                buildCounts[i].text="";
            }
        }
        private void AddHover(GameObject obj,string value)
        {
            var events=obj.AddComponent<EventTrigger>();
            var enter=new EventTrigger.Entry{eventID=EventTriggerType.PointerEnter};enter.callback.AddListener(_=>hovered=value);events.triggers.Add(enter);
            var exit=new EventTrigger.Entry{eventID=EventTriggerType.PointerExit};exit.callback.AddListener(_=>hovered=null);events.triggers.Add(exit);
        }
        private void LateUpdate(){if(summary!=null)Refresh();}
        private void Refresh()
        {
            visibility.alpha=game.MenuOpen?0:1;visibility.interactable=!game.MenuOpen;visibility.blocksRaycasts=!game.MenuOpen;
            var model=game.Model;var crop=game.ActiveCrop;
            if(displayedBuildMode!=game.BuildMode){hovered=null;displayedBuildMode=game.BuildMode;}
            normalBar.gameObject.SetActive(!game.BuildMode&&!game.InventoryOpen);
            tooltip.anchoredPosition=new Vector2(0,game.BuildMode?148:106);
            toast.anchoredPosition=new Vector2(0,game.BuildMode?214:172);
            summary.text=$"Gün {model.Day}   ·   {model.ClockText}";money.text=$"{model.Money} para";saveStatus.text=game.SaveStatus=="Kaydedildi"?"Kaydedildi":game.Ready?"":"Kayıt hatası";
            feedback.text=game.Feedback;toast.gameObject.SetActive(!game.Ready&&!game.InventoryOpen);
            seedCount.text=model.Seeds(crop.id).ToString();produceCount.text=model.Produce(crop.id).ToString();woodCount.text=model.Building.Wood.ToString();
            for(int i=0;i<slots.Length;i++)
            {
                slots[i].GetComponent<Image>().color=!game.BuildMode&&model.EquippedItem==(FarmItem)i?Gold:Color.clear;
                slots[i].interactable=game.Ready;
            }
            buildButton.GetComponent<Image>().color=game.BuildMode?Gold:Color.clear;buildButton.interactable=game.Ready;
            buildMenu.gameObject.SetActive(game.BuildMode);buildTabs.gameObject.SetActive(game.BuildMode);
            if(builder.ActiveDefinition.IsFurniture!=furnitureCategory)SetCategory(builder.ActiveDefinition.IsFurniture);
            for(int i=0;i<buildChoices.Length;i++)
            {
                if(i>=categoryIndices.Length){buildCounts[i].text="";buildChoices[i].interactable=false;continue;}
                var piece=game.BuildPieces[categoryIndices[i]];
                buildCounts[i].text=(piece.IsFurniture?model.Building.FurnitureCount(piece.id):model.Building.Wood/Mathf.Max(1,piece.woodCost)).ToString();
                buildChoices[i].GetComponent<Image>().color=!builder.MoveMode&&builder.ActiveDefinition==piece?Gold:Color.clear;
                buildChoices[i].interactable=game.Ready;
            }
            moveButton.GetComponentInChildren<Text>().text=builder.MoveMode?"M · İnşaya dön":"M · Tutup taşı";
            string hint=hovered;
            tooltip.gameObject.SetActive(!game.InventoryOpen&&hint!=null);tooltipText.text=hint??"";
            detail.gameObject.SetActive(!game.InventoryOpen&&game.BuildMode);
            selectionStatus.text=$"{builder.ActiveDefinition.displayName}\n{builder.HeightLabel} · {builder.Rotation*90}°";
            shop.gameObject.SetActive(game.NearMarket&&!game.InventoryOpen);shopFurniture.gameObject.SetActive(game.NearMarket&&!game.InventoryOpen);
            for(int i=0;i<furnitureBuy.Length;i++)furnitureBuy[i].interactable=game.Ready&&model.Money>=furnitureSale[i].price&&model.Building.FurnitureCount(furnitureSale[i].id)<BuildingModel.WoodLimit;
            camp.gameObject.SetActive(game.NearCamp&&!game.NearMarket&&!game.InventoryOpen);
            buyOne.interactable=game.Ready&&model.Money>=crop.seedPrice&&model.Seeds(crop.id)<FarmModel.StackLimit;
            buyFive.interactable=game.Ready&&model.Money>=crop.seedPrice*5&&model.Seeds(crop.id)<=FarmModel.StackLimit-5;
            sell.interactable=game.Ready&&model.Produce(crop.id)>0;
            buyWood.interactable=game.Ready&&model.Money>=BuildingModel.WoodPackPrice&&model.Building.Wood<=BuildingModel.WoodLimit-BuildingModel.WoodPackCount;
            buyBed.interactable=game.Ready&&model.Money>=BuildingModel.BedPrice&&model.Building.Beds<BuildingModel.WoodLimit;
            sleep.interactable=game.Ready;campWarning.text=model.ThirstyCount>0?$"{model.ThirstyCount} bitki su bekliyor.\nUyandığında saat 06:00 olacak.":"Yatağında sabaha kadar uyu.\nUyandığında saat 06:00 olacak.";
        }
        private static Sprite Rounded()
        {
            if(rounded!=null)return rounded;
            const int size=32;var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);texture.name="Rounded UI";texture.filterMode=FilterMode.Bilinear;
            var pixels=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float dx=Mathf.Max(8.5f-x,0,x-22.5f),dy=Mathf.Max(8.5f-y,0,y-22.5f);
                pixels[x+y*size]=new Color(1,1,1,Mathf.Clamp01(8.5f-Mathf.Sqrt(dx*dx+dy*dy)));
            }
            texture.SetPixels(pixels);texture.Apply();rounded=Sprite.Create(texture,new Rect(0,0,size,size),Vector2.one*.5f,100,0,SpriteMeshType.FullRect,new Vector4(10,10,10,10));return rounded;
        }
        internal static RectTransform Panel(string name,Transform parent,Vector2 anchor,Vector2 position,Vector2 size,Color tint)
        {
            var obj=new GameObject(name,typeof(RectTransform),typeof(Image));var r=obj.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=anchor;r.anchoredPosition=position;r.sizeDelta=size;
            var image=obj.GetComponent<Image>();image.sprite=Rounded();image.type=Image.Type.Sliced;image.color=tint;
            var outline=obj.AddComponent<Outline>();outline.effectColor=new Color(.23f,.15f,.08f,.8f);outline.effectDistance=new Vector2(1.5f,-1.5f);return r;
        }
        internal static Text Label(Transform parent,string value,int size,float x,float y,float width,float height,Color color)
        {
            var obj=new GameObject("Label",typeof(RectTransform),typeof(Text));var r=obj.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(width,height);
            var t=obj.GetComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontSize=size;t.text=value;t.color=color;t.raycastTarget=false;return t;
        }
        internal static Button Button(Transform parent,string label,float x,float y,float width,float height,UnityEngine.Events.UnityAction action)
        {
            var r=Panel(label,parent,new Vector2(0,1),new Vector2(x,-y),new Vector2(width,height),new Color(.78f,.66f,.43f));var b=r.gameObject.AddComponent<Button>();b.targetGraphic=r.GetComponent<Image>();b.navigation=new Navigation{mode=Navigation.Mode.None};b.onClick.AddListener(action);
            var text=Label(r,label,14,5,2,width-10,height-4,Ink);text.alignment=TextAnchor.MiddleCenter;return b;
        }
    }
}
