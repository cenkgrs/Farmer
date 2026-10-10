using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
namespace Farmer
{
    [DefaultExecutionOrder(-220)]
    public sealed class StorageInteraction:MonoBehaviour
    {
        private FarmGame game;private GameObject overlay;private RectTransform bagPanel,chestPanel;
        private StorageSlot[] bagSlots,chestSlots;private Text notice;
        private FarmModel openedModel;public string ChestId {get;private set;}
        public string Hint {get;private set;}
        public bool IsOpen=>game!=null&&game.InventoryOpen;
        private void Awake()=>game=GetComponent<FarmGame>();
        private void Start()
        {
            var canvas=new GameObject("Storage Canvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvas.transform.SetParent(transform,false);
            canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;canvas.GetComponent<Canvas>().overrideSorting=true;canvas.GetComponent<Canvas>().sortingOrder=50;
            var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280,720);scaler.matchWidthOrHeight=.5f;
            var dim=FarmHud.Panel("Inventory Overlay",canvas.transform,new Vector2(.5f,.5f),Vector2.zero,new Vector2(4000,4000),new Color(0,0,0,.35f));overlay=dim.gameObject;
            bagPanel=FarmHud.Panel("Bag Panel",dim,new Vector2(.5f,.5f),new Vector2(-215,0),new Vector2(408,464),new Color(.92f,.81f,.60f));
            chestPanel=FarmHud.Panel("Chest Panel",dim,new Vector2(.5f,.5f),new Vector2(215,0),new Vector2(408,464),new Color(.92f,.81f,.60f));
            FarmHud.Label(bagPanel,"ÇANTAM",22,22,15,250,32,new Color(.25f,.16f,.08f));
            FarmHud.Label(chestPanel,"DEPOLAMA SANDIĞI",22,22,15,350,32,new Color(.25f,.16f,.08f));
            FarmHud.Button(bagPanel,"Kapat",308,15,80,28,Close);
            bagSlots=MakeSlots(bagPanel,false);chestSlots=MakeSlots(chestPanel,true);
            notice=FarmHud.Label(bagPanel,"",12,18,394,372,56,new Color(.25f,.16f,.08f));
            FarmHud.Label(chestPanel,"Tık: 1 adet · Shift+tık: tüm yığın\nSürükle-bırak: tüm yığın",12,18,394,372,56,new Color(.25f,.16f,.08f));overlay.SetActive(false);
        }
        private StorageSlot[] MakeSlots(RectTransform panel,bool chest)
        {
            var result=new StorageSlot[16];
            for(int i=0;i<16;i++)
            {
                var rect=FarmHud.Panel((chest?"Chest":"Bag")+" Item "+i,panel,new Vector2(0,1),new Vector2(18+(i%4)*94,-60-(i/4)*81),new Vector2(88,75),new Color(.98f,.90f,.73f));
                var slot=rect.gameObject.AddComponent<StorageSlot>();slot.Owner=this;slot.FromChest=chest;
                var picture=new GameObject("Item",typeof(RectTransform),typeof(Image));var r=picture.GetComponent<RectTransform>();r.SetParent(rect,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,1);r.anchoredPosition=new Vector2(0,-3);r.sizeDelta=new Vector2(50,45);
                slot.Icon=picture.GetComponent<Image>();slot.Icon.preserveAspect=true;slot.Icon.raycastTarget=false;
                slot.Name=FarmHud.Label(rect,"",10,3,49,82,23,new Color(.25f,.16f,.08f));slot.Name.alignment=TextAnchor.MiddleCenter;
                slot.CountLabel=FarmHud.Label(rect,"",12,55,28,30,20,new Color(.25f,.16f,.08f));slot.CountLabel.alignment=TextAnchor.MiddleRight;result[i]=slot;
            }
            return result;
        }
        private void Update()
        {
            Hint=null;if(game==null||!game.Ready||game.MenuOpen||!Application.isFocused||overlay==null)return;
            var k=Keyboard.current;
            if(k?.tabKey.wasPressedThisFrame==true){if(IsOpen)Close();else Open(null);return;}
            if(IsOpen)
            {
                if(k?.escapeKey.wasPressedThisFrame==true||openedModel!=game.Model||(ChestId!=null&&!CanUse(ChestId))){Close();return;}
                Refresh();return;
            }
            if(game.BuildMode||Mouse.current==null)return;
            game.Selection.RefreshPointer();if(game.Selection.PointerBlocked)return;
            var view=PickChest(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()));
            if(view==null||view.Record.pieceId!="home_chest")return;
            bool near=CanUse(view.Record.instanceId);Hint=near?"F · Sandığı aç":"Sandığı açmak için yaklaş.";
            if(near&&k?.fKey.wasPressedThisFrame==true)Open(view.Record.instanceId);
        }
        // Visual occluders must not intercept pointing into an already visible room.
        // CanUse still tests the physical player-to-chest line independently.
        public PlacedBlockView PickChest(Ray ray)
        {
            foreach(var hit in Physics.RaycastAll(ray,150,~0,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance))
            {
                if(hit.collider.transform.IsChildOf(game.Player))continue;
                var faded=hit.collider.GetComponentInParent<OccludingWall>();
                if(faded!=null&&faded.Opacity<.99f)continue;
                var piece=hit.collider.GetComponentInParent<PlacedBlockView>();
                return piece!=null&&piece.Record.pieceId=="home_chest"?piece:null;
            }
            return null;
        }
        public bool CanUse(string id)
        {
            var chest=game.Model.Building.FindStorage(id);if(chest==null)return false;
            var point=new Vector3(chest.x+.5f,.4f,chest.z+.5f);var from=game.Player.position+Vector3.up*.7f;var flat=point-from;flat.y=0;if(flat.sqrMagnitude>6.25f)return false;
            foreach(var hit in Physics.RaycastAll(from,(point-from).normalized,Vector3.Distance(point,from),~0,QueryTriggerInteraction.Ignore))
            {
                if(hit.collider.transform.IsChildOf(game.Player))continue;
                var b=hit.collider.GetComponentInParent<PlacedBlockView>();
                if(b!=null&&b.Record.instanceId==id)continue;
                return false;
            }
            return true;
        }
        public void Open(string id)
        {
            if(!game.Ready||overlay==null||(id!=null&&!CanUse(id)))return;
            game.SetBuildMode(false);game.ShowBuildFeedback("");ChestId=id;openedModel=game.Model;game.InventoryOpen=true;game.Selection.ModalBlocked=true;
            bagPanel.anchoredPosition=new Vector2(id==null?0:-215,0);chestPanel.gameObject.SetActive(id!=null);overlay.SetActive(true);Refresh();
        }
        public void Close(){game.InventoryOpen=false;game.Selection.ModalBlocked=false;ChestId=null;if(overlay!=null)overlay.SetActive(false);}
        private void OnDisable(){if(game!=null)Close();}
        private void Refresh()
        {
            var ids=game.Model.BagItems();var items=ChestId==null?Array.Empty<ItemStack>():game.Model.Building.StoredItems(ChestId);
            for(int i=0;i<16;i++)
            {
                SetSlot(bagSlots[i],i<ids.Length?ids[i]:null,i<ids.Length?game.Model.BagCount(ids[i]):0);
                SetSlot(chestSlots[i],i<items.Length?items[i].id:null,i<items.Length?items[i].count:0);
            }
            notice.text=ChestId==null?"Tab / Esc · Kapat\nMobilyaları İnşa > Mobilya bölümünden yerleştir.":game.Feedback;
        }
        private void SetSlot(StorageSlot slot,string id,int count)
        {
            slot.ItemId=id;slot.Count=count;slot.Icon.enabled=id!=null;slot.Icon.sprite=id==null?null:Icon(id);slot.Icon.color=count>0?Color.white:new Color(1,1,1,.25f);
            slot.Name.text=id==null?"":Name(id);slot.CountLabel.text=id==null?"":id.StartsWith("tool:")?"":count.ToString();
        }
        public string Name(string id)
        {
            if(id.StartsWith("furniture:"))return game.BuildPieces.First(p=>p.id==id.Substring(10)).displayName;
            if(id.StartsWith("seed:"))return game.Definition(id.Substring(5)).displayName+" tohumu";
            if(id.StartsWith("crop:"))return game.Definition(id.Substring(5)).displayName;
            return id=="stone"?"Taş":id=="tool:pickaxe"?"Kazma":id=="wood"?"Odun":id=="tool:watering"?"Sulama kabı":id=="tool:sickle"?"Orak":id=="tool:hoe"?"Çapa":"Balta";
        }
        public static Sprite Icon(string id)
        {
            if(id.StartsWith("furniture:"))return ConstructionArtwork.Get(id.Substring(10));
            return InventoryIcon.Artwork(id=="stone"?10:id=="tool:pickaxe"?9:id=="wood"?6:id.StartsWith("seed:")?0:id.StartsWith("crop:")?5:id=="tool:watering"?1:id=="tool:sickle"?2:id=="tool:hoe"?3:8);
        }
        public void Transfer(string id,bool fromChest,int amount)
        {
            if(!IsOpen||ChestId==null||id==null)return;
            if(id.StartsWith("tool:")){game.ShowBuildFeedback("Çalışma aletleri her zaman yanında kalır.");return;}
            game.TransferStorage(ChestId,id,amount,fromChest);Refresh();
        }
        public bool DropOnOtherSide(bool fromChest,Vector2 point)=>ChestId!=null&&RectTransformUtility.RectangleContainsScreenPoint(fromChest?bagPanel:chestPanel,point);
    }
    public sealed class StorageSlot:MonoBehaviour,IPointerClickHandler,IBeginDragHandler,IDragHandler,IEndDragHandler
    {
        public StorageInteraction Owner;public bool FromChest;public string ItemId;public int Count;public Image Icon;public Text Name,CountLabel;
        private GameObject ghost;private string dragged;private int amount;
        public void OnPointerClick(PointerEventData e)
        {
            if(e.button!=PointerEventData.InputButton.Left||Count<1||ItemId==null)return;
            bool all=Keyboard.current?.leftShiftKey.isPressed==true||Keyboard.current?.rightShiftKey.isPressed==true;
            Owner.Transfer(ItemId,FromChest,all?Count:1);
        }
        public void OnBeginDrag(PointerEventData e)
        {
            if(e.button!=PointerEventData.InputButton.Left||Count<1||ItemId==null||ItemId.StartsWith("tool:"))return;
            dragged=ItemId;amount=Count;ghost=new GameObject("Dragged item",typeof(RectTransform),typeof(Image));ghost.transform.SetParent(GetComponentInParent<Canvas>().transform,false);
            var image=ghost.GetComponent<Image>();image.sprite=Icon.sprite;image.preserveAspect=true;image.raycastTarget=false;((RectTransform)ghost.transform).sizeDelta=new Vector2(56,56);ghost.transform.position=e.position;
        }
        public void OnDrag(PointerEventData e){if(ghost!=null)ghost.transform.position=e.position;}
        public void OnEndDrag(PointerEventData e){if(ghost==null)return;Destroy(ghost);ghost=null;if(Owner.DropOnOtherSide(FromChest,e.position))Owner.Transfer(dragged,FromChest,amount);dragged=null;}
        private void OnDisable(){if(ghost!=null)Destroy(ghost);ghost=null;dragged=null;}
    }
}
