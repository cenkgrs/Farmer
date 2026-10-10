using UnityEngine;
using UnityEngine.InputSystem;

namespace Farmer
{
    public sealed class DoorInteraction : MonoBehaviour
    {
        private FarmGame game;
        public string Hint { get; private set; }
        private void Awake()=>game=GetComponent<FarmGame>();
        private void Update()
        {
            Hint=null;
            if(!game.Ready||game.WorldInputBlocked||game.BuildMode||!game.isActiveAndEnabled||!Application.isFocused||Mouse.current==null)return;
            game.Selection.RefreshPointer();
            if(game.Selection.PointerBlocked)return;
            var ray=Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if(!Physics.Raycast(ray,out var hit,100,~0,QueryTriggerInteraction.Ignore))return;
            var door=hit.collider.GetComponentInParent<DoorView>();
            if(door==null)return;
            var delta=door.InteractionPoint-game.Player.position;delta.y=0;
            if(delta.sqrMagnitude>2.5f*2.5f){Hint="Kapıyı kullanmak için yaklaş.";return;}
            Hint=door.IsOpen?"F · Kapıyı kapat":"F · Kapıyı aç";
            if(Keyboard.current?.fKey.wasPressedThisFrame==true)door.TryToggle();
        }
    }
}
