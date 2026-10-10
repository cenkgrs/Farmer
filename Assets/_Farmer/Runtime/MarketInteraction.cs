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
        private void Awake()=>game=GetComponent<FarmGame>();
        private void Update()
        {
            ValidateAccess();
            if(!Application.isFocused)return;
            var keyboard=Keyboard.current;
            HandleInput(keyboard?.fKey.wasPressedThisFrame==true,keyboard?.escapeKey.wasPressedThisFrame==true);
        }
        public void ValidateAccess()
        {
            if(IsOpen&&(!game.Ready||!game.NearMarket||game.MenuOpen||game.InventoryOpen||!game.isActiveAndEnabled))Close();
        }
        public bool HandleInput(bool interact,bool escape)
        {
            if(IsOpen&&(interact||escape)){Close();consumedFrame=Time.frameCount;return true;}
            if(interact&&TryOpen()){consumedFrame=Time.frameCount;return true;}
            return false;
        }
        public bool TryOpen()
        {
            if(!game.Ready||game.MenuOpen||game.InventoryOpen||game.BuildMode||!game.NearMarket)return false;
            IsOpen=true;game.SetBuildMode(false);game.Selection.ModalBlocked=true;
            return true;
        }
        public void Close()
        {
            IsOpen=false;
            if(game!=null&&game.Selection!=null)game.Selection.ModalBlocked=game.MenuOpen||game.InventoryOpen;
        }
        private void OnDisable()=>Close();
    }
}
