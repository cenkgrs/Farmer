using UnityEngine;

namespace Farmer
{
    public sealed class DoorView : MonoBehaviour
    {
        private FarmGame game;
        private Transform hinge;
        private BlockRecord record;
        public BlockRecord Record => record;
        public bool IsOpen => game.Model.Building.DoorIsOpen(record);
        public Vector3 InteractionPoint => transform.TransformPoint(new Vector3(0,.5f,.5f));
        public void Configure(FarmGame source,BlockRecord sourceRecord)
        {
            game=source;record=sourceRecord;hinge=transform.Find("Hinge");
            hinge.localRotation=Quaternion.Euler(0,IsOpen?-90:0,0);
        }
        private void Update()
        {
            if(hinge==null)return;
            hinge.localRotation=Quaternion.RotateTowards(hinge.localRotation,Quaternion.Euler(0,IsOpen?-90:0,0),240*Time.deltaTime);
        }
        public bool TryToggle()
        {
            // Reserve the full swing for either direction so the leaf cannot sweep through the player or furniture.
            foreach(var hit in Physics.OverlapBox(transform.TransformPoint(new Vector3(0,.6f,.94f)),new Vector3(.41f,1.05f,.43f),transform.rotation,~0,QueryTriggerInteraction.Ignore))
            {
                if(hit.transform.IsChildOf(transform)||hit.GetComponent<WorldGround>()!=null)continue;
                var piece=hit.GetComponentInParent<PlacedBlockView>();
                if(piece!=null && game.Model.Building.Rules(piece.Record.pieceId).Placement==BuildPlacement.Floor)continue;
                game.ShowBuildFeedback("Kapının açılma alanı dolu. Biraz yana çekil.");return false;
            }
            return game.ToggleDoor(record);
        }
    }
}
