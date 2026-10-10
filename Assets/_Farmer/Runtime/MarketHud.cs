using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Farmer
{
    // Coordinates are measured in the approved 1672 x 941 reference. Source pixels
    // supply the illustrated skin; only live values and interactive states are redrawn.
    public sealed class MarketHud : MonoBehaviour
    {
        private const float Width=1672,Height=941;
        private FarmGame game;
        private RectTransform root,catalog,details,navigation,sales;
        private Texture2D approved,clean;
        private Font regular,bold;
        private MarketOffer[] offers,visible;
        private readonly List<Button> cards=new List<Button>();
        private readonly List<GameObject> cardHighlights=new List<GameObject>();
        private readonly List<Text> saleCounts=new List<Text>();
        private readonly List<Button> saleButtons=new List<Button>();
        private readonly List<GameObject> saleHighlights=new List<GameObject>();
        private string[] saleIds;
        private Button purchase,sell,minus,plus;
        private Text wallet,quantityText,owned,total,description,status,saleSummary;
        private bool wasOpen;
        private int page,selectedSale;
        private float statusUntil;
        public MarketCategory Category { get; private set; }
        public MarketOffer Selected { get; private set; }
        public int Quantity { get; private set; }=1;
        public RectTransform Window => root;
        private static readonly Color Ink=new Color(.22f,.12f,.055f);
        private static readonly Rect[] SeedCards={new Rect(426,220,226,346),new Rect(663,220,226,346),new Rect(899,220,230,346)};
        private static readonly Rect[] SeedArt={new Rect(446,251,195,194),new Rect(676,251,202,194),new Rect(914,251,199,194)};
        private static readonly string[] SeedIds={"turnip","carrot","tomato"};
        private static readonly string[] CategoryNames={"Tohumlar","Aletler","Malzemeler","Mobilyalar","Üretim"};

        private void Start()
        {
            game=GetComponent<FarmGame>();offers=MarketOffer.Create(game);
            approved=Resources.Load<Texture2D>("MarketArt/approved");clean=Resources.Load<Texture2D>("MarketArt/clean");
            regular=Resources.Load<Font>("MarketArt/LiberationSerif-Regular");bold=Resources.Load<Font>("MarketArt/LiberationSerif-Bold");
            var canvasObject=new GameObject("Market Canvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvasObject.transform.SetParent(transform,false);
            var canvas=canvasObject.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=50;
            var scaler=canvasObject.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(Width,Height);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
            // Fit the whole artwork on any aspect ratio; never stretch or crop the frame.
            var shade=new GameObject("Market shade",typeof(RectTransform),typeof(Image));shade.transform.SetParent(canvasObject.transform,false);
            var sr=(RectTransform)shade.transform;sr.anchorMin=Vector2.zero;sr.anchorMax=Vector2.one;sr.offsetMin=sr.offsetMax=Vector2.zero;
            shade.GetComponent<Image>().color=new Color(.075f,.055f,.025f,1);
            root=Rect(canvasObject.transform,"Market",new Rect(0,0,Width,Height));root.anchorMin=root.anchorMax=root.pivot=Vector2.one*.5f;root.anchoredPosition=Vector2.zero;
            Raw(root,"Approved market artwork",approved,new Rect(0,0,Width,Height),new Rect(0,0,Width,Height));
            Hit(root,"Close Market",new Rect(1425,102,78,75),()=>game.GetComponent<MarketInteraction>().Close());
            Patch(root,new Rect(1260,114,90,38));wallet=Label(root,"Wallet",new Rect(1255,111,96,45),32,true);
            catalog=Rect(root,"Catalog",new Rect(0,0,Width,Height));
            navigation=Rect(root,"Categories",new Rect(0,0,Width,Height));
            details=Rect(root,"Product details",new Rect(0,0,Width,Height));
            sales=Rect(root,"Sales",new Rect(0,0,Width,Height));BuildSales();
            // Feedback occupies the unused wood under navigation, without altering the approved footer.
            status=Label(root,"Market feedback",new Rect(192,708,198,70),19,false);status.color=new Color(.99f,.91f,.70f);
            ChooseCategory(MarketCategory.Seeds);
            canvasObject.SetActive(false);canvasRoot=canvasObject;
        }
        private GameObject canvasRoot;
        private void LateUpdate()
        {
            if(canvasRoot==null)return;
            bool open=game.MarketOpen&&!game.MenuOpen;
            canvasRoot.SetActive(open);
            if(open&&!wasOpen)
            {
                ChooseCategory(MarketCategory.Seeds);
                SelectOffer(game.ActiveCrop.id);
                status.text="";
            }
            wasOpen=open;if(open)RefreshValues();
        }
        public void ChooseCategory(MarketCategory category)
        {
            Category=category;page=0;
            visible=offers.Where(o=>o.Category==category).ToArray();
            Selected=visible.FirstOrDefault();Quantity=1;
            if(category==MarketCategory.Seeds)Selected=visible.FirstOrDefault(o=>o.Id==game.ActiveCrop.id)??Selected;
            page=Selected==null?0:Array.IndexOf(visible,Selected)/3;
            BuildNavigation();BuildCatalog();BuildDetails();RefreshValues();
        }
        public bool SelectOffer(string id)
        {
            var offer=visible.FirstOrDefault(o=>o.Id==id);if(offer==null)return false;
            Selected=offer;Quantity=1;
            int targetPage=Array.IndexOf(visible,offer)/3;
            if(page!=targetPage){page=targetPage;BuildCatalog();}
            if(offer.Kind==MarketOfferKind.Seed)game.SelectCrop(id);
            BuildDetails();RefreshValues();return true;
        }
        private void BuildNavigation()
        {
            Clear(navigation);
            for(int i=0;i<5;i++)
            {
                int index=i;var r=new Rect(188,196+i*101,205,89);
                if(i==0&&Category!=MarketCategory.Seeds)
                {
                    Raw(navigation,"Neutral seed tab",clean,r,r);
                    Raw(navigation,"Leaf",approved,new Rect(204,216,42,48),new Rect(204,216,42,48));
                    Label(navigation,"Seed category",new Rect(256,214,130,48),28,true).text="Tohumlar";
                    Raw(navigation,"Hide seed arrow",clean,new Rect(392,205,16, 70),new Rect(392,205,16, 70));
                }
                if((int)Category==i&&i!=0)Border(navigation,r,true,5);
                Hit(navigation,"Market category "+CategoryNames[i],r,()=>ChooseCategory((MarketCategory)index));
            }
        }
        private void BuildCatalog()
        {
            Clear(catalog);cards.Clear();cardHighlights.Clear();
            Patch(catalog,new Rect(412,193,737,394));
            int start=page*3;
            for(int slot=0;slot<3;slot++)
            {
                if(start+slot>=visible.Length)continue;
                var offer=visible[start+slot];var r=SeedCards[slot];
                int seed=Array.IndexOf(SeedIds,offer.Id);
                if(offer.Kind==MarketOfferKind.Seed&&seed>=0)
                {
                    Raw(catalog,"Seed card art",approved,SeedCards[seed],r);
                    // Neutralize the baked selection, then add a live border below.
                    if(seed==2)Border(catalog,r,false);
                    int[] illustratedPrices={10,14,24};
                    if(offer.Price!=illustratedPrices[seed])
                    {
                        var priceRect=new Rect(r.x+105,503,82,42);Patch(catalog,priceRect);
                        Label(catalog,"Price "+offer.Id,priceRect,32,true).text=offer.Price.ToString();
                    }
                }
                else
                {
                    Icon(catalog,offer,offer.Kind==MarketOfferKind.Seed?new Rect(r.x+9,243,208,208):new Rect(r.x+28,255,170,180));
                    Label(catalog,"Name "+offer.Id,new Rect(r.x+10,455,r.width-20, 40),26,true).text=offer.Name;
                    if(offer.Price>0)Raw(catalog,"Coin",approved,new Rect(488,506,38,40),new Rect(r.x+58,507,38,40));
                    Label(catalog,"Price "+offer.Id,new Rect(r.x+99,501,116,48),32,true).text=offer.Price>0?offer.Price.ToString():"Tarif";
                }
                var highlight=Rect(catalog,"Selected "+offer.Id,r);Border(highlight,new Rect(0,0,r.width,r.height),true);cardHighlights.Add(highlight.gameObject);
                var button=Hit(catalog,"Market offer "+offer.Id,r,()=>SelectOffer(offer.Id));cards.Add(button);
            }
            if(visible.Length>3)
            {
                var p=Label(catalog,"Catalog page",new Rect(702,565,150,24),18,false);p.text=$"{page+1} / {(visible.Length+2)/3}";
                TextButton(catalog,"Previous market page","‹",new Rect(441,562,50,25),()=>ChangePage(-1));
                TextButton(catalog,"Next market page","›",new Rect(1061,562,50,25),()=>ChangePage(1));
            }
        }
        public void ChangePage(int delta)
        {
            int next=Mathf.Clamp(page+delta,0,Math.Max(0,(visible.Length-1)/3));
            if(next==page)return;page=next;Selected=visible[page*3];Quantity=1;BuildCatalog();BuildDetails();RefreshValues();
        }
        private void BuildDetails()
        {
            Clear(details);Patch(details,new Rect(1162,193,324,600));
            if(Selected==null)return;
            var o=Selected;
            if(o.Kind==MarketOfferKind.Seed)
            {
                int seed=Array.IndexOf(SeedIds,o.Id);
                if(seed==2)Raw(details,"Tomato detail art",approved,new Rect(1200,211,250,189),new Rect(1200,211,250,189));
                else if(seed>=0)Raw(details,"Seed detail art",approved,SeedArt[seed],new Rect(1208,213,239,184));
                else Icon(details,o,new Rect(1210,215,230,180));
            }
            else Icon(details,o,new Rect(1234,216,191,177));
            Label(details,"Selected product",new Rect(1181,394,287,42),32,true).text=o.Name;
            description=Label(details,"Product description",new Rect(1191,438,270,72),25,false);
            description.text=Description(o);description.resizeTextForBestFit=true;description.resizeTextMinSize=19;description.resizeTextMaxSize=25;
            owned=Label(details,"Owned quantity",new Rect(1201,523,250,34),25,false);
            minus=Hit(details,"Market quantity minus",new Rect(1198,567,62,59),()=>ChangeQuantity(-1));
            Label(details,"Minus",new Rect(1202,567,54,54),38,true).text="−";
            plus=Hit(details,"Market quantity plus",new Rect(1387,567,61,59),()=>ChangeQuantity(1));
            var plusText=Label(details,"Plus",new Rect(1392,566,51,56), 40,true);plusText.text="+";plusText.color=new Color(1,.95f,.79f);
            quantityText=Label(details,"Purchase quantity",new Rect(1274,572,98,49),32,true);
            if(o.Kind!=MarketOfferKind.Recipe)
            {
                Label(details,"Total label",new Rect(1225,638,92,43),27,false).text="Toplam:";
                Raw(details,"Total coin",approved,new Rect(1323,638,40,40),new Rect(1323,638,40,40));
            }
            total=Label(details,"Purchase total",o.Kind==MarketOfferKind.Recipe?new Rect(1201,638,247,43):new Rect(1366,638,82,43),30,true);
            purchase=Hit(details,"Market purchase",new Rect(1186,692,271,78),()=>PurchaseSelected());
            // Reuse the actual illustrated button, including the original lettering for purchases.
            Raw(details,"Purchase button artwork",approved,new Rect(1186,692,271,78),new Rect(1186,692,271,78));
            if(o.Kind==MarketOfferKind.Recipe)
            {
                Raw(details,"Craft button surface",clean,new Rect(1186,692,271,78),new Rect(1186,692,271,78));
                var t=Label(details,"Craft button text",new Rect(1196,705,250,49),36,true);t.text="Üret";t.color=new Color(1,.95f,.8f);
            }
            purchase.transform.SetAsLastSibling();
            disabledPurchase=DisabledOverlay(details,"Purchase disabled",new Rect(1186,692,271,78));
            disabledMinus=DisabledOverlay(details,"Minus disabled",new Rect(1198,567,62,59));
            disabledPlus=DisabledOverlay(details,"Plus disabled",new Rect(1387,567,61,59));
        }
        private GameObject disabledPurchase,disabledSell,disabledMinus,disabledPlus;
        private string Description(MarketOffer o)
        {
            if(o.Crop!=null)return $"İlk hasat: {o.Crop.wateredDays} gün\nHasat başına {o.Crop.harvestYield} adet\n"+(o.Crop.regrowDays>0?$"Tekrar hasat: {o.Crop.regrowDays} gün":"Tek hasat · Yeniden ekilir");
            if(o.Recipe!=null)return string.Join(" + ",o.Recipe.ingredients.Select(i=>$"{i.count*Quantity} {game.GetComponent<StorageInteraction>().Name(i.id)}"));
            switch(o.Kind)
            {
                case MarketOfferKind.Pickaxe:return "Kayaları kır, taş topla.\nKalıcı alet · 7 ile kuşan";
                case MarketOfferKind.Wood:return "İnşa ve üretim malzemesi\nBir pakette 10 odun";
                case MarketOfferKind.Bed:return "Yerleştir, N ile sabaha uyu.\nİnşa > Mobilya";
                default:return "Evin için yerleştirilebilir eşya\nİnşa > Mobilya";
            }
        }
        public void ChangeQuantity(int delta)
        {
            if(Selected==null||!Selected.HasQuantity)return;
            Quantity=Mathf.Clamp(Quantity+delta,1,FarmModel.StackLimit);RefreshValues();
        }
        public bool PurchaseSelected()
        {
            if(Selected==null||!game.CanTrade)return false;
            bool ok=Selected.Purchase(game,Quantity);
            Notify(ok?game.Feedback:Selected.Kind==MarketOfferKind.Pickaxe&&game.Model.OwnsPickaxe?"Kazman zaten var.":"Para, malzeme veya çanta kapasitesi yetersiz.");RefreshValues();return ok;
        }
        private void BuildSales()
        {
            saleIds=game.Crops.Select(c=>"crop:"+c.id).Concat(game.Recipes.Select(r=>"crafted:"+r.id)).ToArray();
            // Four sale slots per page preserve the approved layout as the catalog grows.
            BuildSalePage(0);
        }
        private int salePage;
        public void BuildSalePage(int pageIndex)
        {
            salePage=Mathf.Clamp(pageIndex,0,Math.Max(0,(saleIds.Length-1)/4));Clear(sales);saleCounts.Clear();saleButtons.Clear();saleHighlights.Clear();
            if(selectedSale/4!=salePage)selectedSale=salePage*4;
            Patch(sales,new Rect(440,663,488,113));
            for(int n=0;n<4&&salePage*4+n<saleIds.Length;n++)
            {
                int index=salePage*4+n;string id=saleIds[index];float x=442+n*123;
                int original=id=="crop:turnip"?0:id=="crop:carrot"?1:id=="crop:tomato"?2:id=="crafted:vegetable_crate"?3:-1;
                if(original>=0)Raw(sales,"Harvest art",approved,new Rect(449+original*123,676,91,85),new Rect(x+7,676,91,85));
                else
                {
                    var image=Rect(sales,"Harvest icon "+id,new Rect(x+13,681,84,74)).gameObject.AddComponent<Image>();
                    image.sprite=CropArtwork.Get(id.Substring(id.IndexOf(':')+1));image.preserveAspect=true;image.raycastTarget=false;
                }
                Patch(sales,new Rect(x+70,738,36,30));
                var count=Label(sales,"Sale count "+id,new Rect(x+68,735, 40,32),27,true);count.alignment=TextAnchor.MiddleRight;saleCounts.Add(count);
                var h=Rect(sales,"Sale selection "+id,new Rect(x,667,111,107));Border(h,new Rect(0,0,111,107),true,3);saleHighlights.Add(h.gameObject);
                saleButtons.Add(Hit(sales,"Market sell item "+id,new Rect(x,667,111,107),()=>{selectedSale=index;RefreshValues();}));
            }
            sell=Hit(sales,"Market sell",new Rect(943,696,176,65),()=>SellSelected());
            disabledSell=DisabledOverlay(sales,"Sale disabled",new Rect(943,696,176,65));
            saleSummary=Label(sales,"Sale summary",new Rect(650,612,467,40),21,false);saleSummary.alignment=TextAnchor.MiddleRight;
            if(saleIds.Length>4)
            {
                Label(sales,"Sale page",new Rect(968,670,107,25),18,false).text=$"{salePage+1} / {(saleIds.Length+3)/4}";
                TextButton(sales,"Previous sale page","‹",new Rect(928,670,32,25),()=>BuildSalePage(salePage-1));
                TextButton(sales,"Next sale page","›",new Rect(1085,670,32,25),()=>BuildSalePage(salePage+1));
            }
            RefreshValues();
        }
        public bool SellSelected()
        {
            if(!game.CanTrade||saleIds.Length==0)return false;
            string id=saleIds[selectedSale];bool ok=id.StartsWith("crop:")?game.SellHarvest(id.Substring(5)):game.SellCrafted(id.Substring(8));
            Notify(ok?game.Feedback:"Satılacak ürün yok veya para sınırına ulaşıldı.");RefreshValues();return ok;
        }
        private void Notify(string message){status.text=message;statusUntil=Time.unscaledTime+5;}
        private void RefreshValues()
        {
            if(wallet==null||Selected==null)return;
            wallet.text=game.Model.Money.ToString();wallet.resizeTextForBestFit=true;wallet.resizeTextMinSize=19;wallet.resizeTextMaxSize=32;
            quantityText.text=Quantity.ToString();owned.text=Selected.Kind==MarketOfferKind.Pickaxe&&game.Model.OwnsPickaxe?"Satın alındı":"Çantanda: "+Selected.Count(game.Model);
            total.text=Selected.Kind==MarketOfferKind.Recipe?$"Üretim: {Selected.Recipe.outputCount*Quantity}":$"{Selected.Price*Quantity}";
            description.text=Description(Selected);
            minus.interactable=Selected.HasQuantity&&Quantity>1;plus.interactable=Selected.HasQuantity&&Quantity<FarmModel.StackLimit;
            disabledMinus.SetActive(!minus.interactable);disabledPlus.SetActive(!plus.interactable);
            purchase.interactable=game.CanTrade&&Selected.CanPurchase(game.Model,Quantity);disabledPurchase.SetActive(!purchase.interactable);
            for(int n=0;n<cards.Count;n++)cardHighlights[n].SetActive(visible[page*3+n]==Selected);
            for(int n=0;n<saleCounts.Count;n++)
            {
                int index=salePage*4+n;saleCounts[n].text=game.Model.BagCount(saleIds[index]).ToString();saleHighlights[n].SetActive(index==selectedSale);
            }
            string saleId=saleIds[selectedSale];int count=game.Model.BagCount(saleId);
            int price=saleId.StartsWith("crop:")?game.Definition(saleId.Substring(5)).salePrice:game.Recipes.First(r=>r.id==saleId.Substring(8)).salePrice;
            sell.interactable=game.CanTrade&&count>0&&(long)game.Model.Money+(long)count*price<=FarmModel.MoneyLimit;
            disabledSell.SetActive(!sell.interactable);
            saleSummary.text=count>0?$"{game.GetComponent<StorageInteraction>().Name(saleId)} · {count} adet · {count*price} para":"";
            if(Time.unscaledTime>statusUntil)status.text="";
        }
        private void Icon(Transform parent,MarketOffer offer,Rect r)
        {
            Sprite sprite=offer.Kind==MarketOfferKind.Pickaxe?InventoryIcon.Artwork(9):offer.Kind==MarketOfferKind.Wood?InventoryIcon.Artwork(6):offer.Kind==MarketOfferKind.Recipe?CropArtwork.Get(offer.Id):offer.Kind==MarketOfferKind.Seed?SeedArtwork.Get(offer.Id):ConstructionArtwork.Get(offer.Id);
            var img=Rect(parent,"Product icon",r).gameObject.AddComponent<Image>();img.sprite=sprite;img.preserveAspect=true;img.raycastTarget=false;
        }
        private void Patch(Transform parent,Rect r)=>Raw(parent,"Clean surface",clean,r,r);
        private void PatchFrom(Transform parent,Rect source,Rect target)=>Raw(parent,"Clean surface",clean,source,target);
        private RawImage Raw(Transform parent,string name,Texture2D texture,Rect source,Rect target)
        {
            var image=Rect(parent,name,target).gameObject.AddComponent<RawImage>();image.texture=texture;
            image.uvRect=new Rect(source.x/Width,1-(source.y+source.height)/Height,source.width/Width,source.height/Height);image.raycastTarget=false;return image;
        }
        private void Border(Transform parent,Rect target,bool selected,float edge=12)
        {
            Texture2D texture=selected?approved:clean;Rect src=SeedCards[2];float b=12;
            Raw(parent,"Border top",texture,new Rect(src.x,src.y,src.width,b),new Rect(target.x,target.y,target.width,edge));
            Raw(parent,"Border bottom",texture,new Rect(src.x,src.yMax-b,src.width,b),new Rect(target.x,target.yMax-edge,target.width,edge));
            Raw(parent,"Border left",texture,new Rect(src.x,src.y+b,b,src.height-2*b),new Rect(target.x,target.y+edge,edge,target.height-2*edge));
            Raw(parent,"Border right",texture,new Rect(src.xMax-b,src.y+b,b,src.height-2*b),new Rect(target.xMax-edge,target.y+edge,edge,target.height-2*edge));
        }
        private Text Label(Transform parent,string name,Rect rect,int size,bool heavy)
        {
            var t=Rect(parent,name,rect).gameObject.AddComponent<Text>();t.font=heavy?bold:regular;t.fontSize=size;t.color=Ink;t.alignment=TextAnchor.MiddleCenter;t.raycastTarget=false;t.supportRichText=false;return t;
        }
        private Button Hit(Transform parent,string name,Rect rect,UnityEngine.Events.UnityAction action)
        {
            var r=Rect(parent,name,rect);var image=r.gameObject.AddComponent<Image>();image.color=Color.white;
            var b=r.gameObject.AddComponent<Button>();b.targetGraphic=image;b.navigation=new Navigation{mode=Navigation.Mode.None};
            var colors=b.colors;colors.fadeDuration=0;colors.normalColor=Color.clear;colors.highlightedColor=new Color(1,.85f,.38f,.09f);colors.pressedColor=new Color(.3f,.15f,0,.13f);colors.disabledColor=Color.clear;b.colors=colors;
            b.onClick.AddListener(action);return b;
        }
        private static GameObject DisabledOverlay(Transform parent,string name,Rect r)
        {
            var dim=Rect(parent,name,r);var image=dim.gameObject.AddComponent<Image>();
            image.color=new Color(.22f,.17f,.075f,.25f);image.raycastTarget=false;return dim.gameObject;
        }
        private void TextButton(Transform p,string name,string text,Rect r,UnityEngine.Events.UnityAction action)
        {var label=Label(p,name+" text",r,22,true);label.verticalOverflow=VerticalWrapMode.Overflow;label.text=text;Hit(p,name,r,action);}
        private static RectTransform Rect(Transform parent,string name,Rect rect)
        {
            var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(rect.x,-rect.y);r.sizeDelta=rect.size;return r;
        }
        private static void Clear(Transform parent)
        {for(int i=parent.childCount-1;i>=0;i--){var child=parent.GetChild(i).gameObject;child.SetActive(false);Destroy(child);}}
    }
}
