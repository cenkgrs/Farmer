using UnityEngine;
using UnityEngine.InputSystem;
namespace Farmer
{
    [DefaultExecutionOrder(300)]
    public sealed class ExplorationCamera : MonoBehaviour
    {
        [SerializeField] private Transform player;

        private float outdoorSize,sizeVelocity;
        private readonly float[] zoom = { 1f, 1f, 1f };
        private Camera view;
        private FarmGame game;
        private HouseVisibility house;
        public void Configure(Transform target)=>player=target;
        private void Awake(){view=GetComponent<Camera>();outdoorSize=view.orthographicSize;}
        private void LateUpdate()
        {
            if(player==null)return;
            if(game==null){game=FindFirstObjectByType<FarmGame>();if(game!=null)house=game.GetComponent<HouseVisibility>();}
            int context=game!=null&&game.BuildMode?2:house!=null&&house.Indoors?1:0;
            var mouse=Mouse.current;var keyboard=Keyboard.current;
            bool control=keyboard!=null&&(keyboard.leftCtrlKey.isPressed||keyboard.rightCtrlKey.isPressed);
            if(game!=null&&game.Ready&&Application.isFocused&&!game.InventoryOpen&&!game.MenuOpen&&mouse!=null)
            {
                game.Selection.RefreshPointer();
                if(!game.Selection.PointerBlocked&&(context!=2||control))
                {
                    float scroll=mouse.scroll.ReadValue().y;
                    if(Mathf.Abs(scroll)>.01f)
                        zoom[context]=Mathf.Clamp(zoom[context]*Mathf.Exp(-Mathf.Clamp(Mathf.Abs(scroll)>=120f?scroll/120f:scroll,-3,3)*.22f),.5f,1.8f);
                }
            }
            var focus=player.position+Vector3.up*.8f;
            var desired=focus-transform.forward*30;float size=outdoorSize;
            if(game!=null&&!game.BuildMode&&house!=null&&house.Indoors)
            {
                var bounds=house.RoomBounds;

                float halfWidth=0,halfHeight=0;
                for(int i=0;i<8;i++)
                {
                    var corner=Vector3.Scale(bounds.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));
                    halfWidth=Mathf.Max(halfWidth,Mathf.Abs(Vector3.Dot(corner,transform.right)));
                    halfHeight=Mathf.Max(halfHeight,Mathf.Abs(Vector3.Dot(corner,transform.up)));
                }
                // Scale framing to room dimensions, reserving space for the inventory and HUD.
                size=Mathf.Max(2.8f,halfHeight/.72f,(halfWidth+.5f)/(view.aspect*.82f));
            }
            size=Mathf.Clamp(size*zoom[context],1.8f,24f);
            transform.position=desired;
            view.orthographicSize=Mathf.SmoothDamp(view.orthographicSize,size,ref sizeVelocity,.12f);
        }
    }
}
