using UnityEngine;
using UnityEngine.InputSystem;

namespace Farmer
{
    // Runs before chest/door input, so one F press can only act on one interaction.
    [DefaultExecutionOrder(-250)]
    public sealed class MarketInteraction : MonoBehaviour
    {
        private FarmGame game;
        private int consumedFrame=-1;
        public bool IsOpen { get; private set; }
        public bool ConsumedThisFrame => consumedFrame==Time.frameCount;
        private BoxCollider stallBody;
        public bool InReach
        {
            get
            {
                if(game==null||game.Player==null||stallBody==null)return false;
                var point=game.Player.position;point.y=stallBody.bounds.center.y;
                return Vector3.Distance(point,stallBody.ClosestPoint(point))<=3.2f;
            }
        }
        private void Awake()
        {
            game=GetComponent<FarmGame>();
            if(game.Market==null)return;
            var renderers=game.Market.GetComponentsInChildren<Renderer>();
            if(renderers.Length==0)return;
            var bounds=new Bounds(game.Market.InverseTransformPoint(renderers[0].bounds.center),Vector3.zero);
            foreach(var renderer in renderers)
            {
                var b=renderer.bounds;
                for(int i=0;i<8;i++)bounds.Encapsulate(game.Market.InverseTransformPoint(b.center+Vector3.Scale(b.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1))));
            }
            // Align the interaction pivot with the visible stall while preserving all child world positions.
            var offset=game.Market.TransformVector(new Vector3(bounds.center.x,0,bounds.center.z));
            game.Market.position+=offset;
            foreach(Transform child in game.Market)child.position-=offset;
            bounds.center=new Vector3(0,bounds.center.y,0);
            stallBody=game.Market.GetComponent<BoxCollider>();
            if(stallBody==null)stallBody=game.Market.gameObject.AddComponent<BoxCollider>();
            // Fit the actual authored model, including child offsets, rather than the old pivot.
            stallBody.center=new Vector3(bounds.center.x,bounds.min.y+.65f,bounds.center.z);
            stallBody.size=new Vector3(Mathf.Max(1,bounds.size.x*.92f),1.3f,Mathf.Max(.8f,bounds.size.z*.9f));
            stallBody.isTrigger=false;stallBody.enabled=true;
        }
        private void Update()
        {
            ValidateAccess();
            if(!Application.isFocused)return;
            var keyboard=Keyboard.current;
            HandleInput(keyboard?.fKey.wasPressedThisFrame==true,keyboard?.escapeKey.wasPressedThisFrame==true);
        }
        public void ValidateAccess()
        {
            if(IsOpen&&(!game.Ready||!game.NearMarket||game.MenuOpen||game.JournalOpen||game.InventoryOpen||!game.isActiveAndEnabled))Close();
        }
        public bool HandleInput(bool interact,bool escape)
        {
            if(IsOpen&&(interact||escape)){Close();consumedFrame=Time.frameCount;return true;}
            if(interact&&TryOpen()){consumedFrame=Time.frameCount;return true;}
            return false;
        }
        public bool TryOpen()
        {
            if(!game.Ready||game.ConversationOpen||game.VillageInputConsumed||game.MenuOpen||game.JournalOpen||game.InventoryOpen||game.BuildMode||!game.NearMarket)return false;
            IsOpen=true;game.SetBuildMode(false);game.Selection.ModalBlocked=true;
            return true;
        }
        public void Close()
        {
            IsOpen=false;
            if(game!=null&&game.Selection!=null)game.Selection.ModalBlocked=game.MenuOpen||game.JournalOpen||game.InventoryOpen;
        }
        private void OnDisable()=>Close();
    }
}
