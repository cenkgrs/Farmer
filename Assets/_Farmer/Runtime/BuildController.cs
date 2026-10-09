using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace Farmer
{
    [DefaultExecutionOrder(-150)]
    public sealed class BuildController : MonoBehaviour
    {
        [SerializeField] private FarmGame game;
        [SerializeField] private Material outlineMaterial;
        private readonly List<GameObject> placed = new List<GameObject>();
        private GameObject preview, grid;
        private Material previewMaterial;
        private Renderer[] previewRenderers;
        private Vector3Int? target;
        private BlockRecord removeTarget,dragged;
        private BuildingModel dragModel;
        public bool MoveMode { get; private set; }
        public bool IsDragging=>dragged!=null;
        public string HeightLabel=>ActiveDefinition.placement==BuildPlacement.Roof?$"Çatı {BuildingModel.RoofHeight(level):0.0} m · Otomatik":$"Yükseklik {level+1}/{BuildingModel.Levels}";
        private BuildingModel displayedModel;
        private int displayedRevision = -1;
        private int level, rotation, pieceIndex;
        public BuildDefinition ActiveDefinition => game.BuildPieces[pieceIndex];
        public int Level => level;
        public int Rotation => rotation;
        public bool ValidPreview { get; private set; }
        public string Status { get; private set; } = "Fareyi boş zemine götür.";
        public int VisibleBlockCount => placed.Count;
        public Vector3Int? Target => target;
        public void Configure(FarmGame source, Material line) { game = source; outlineMaterial = line; }
        public static Vector3 Center(int x, int y, int z) => new Vector3(x + .5f, y + .5f, z + .5f);
        private void Start()
        {
            previewMaterial = new Material(outlineMaterial);
            CreatePreview();
            CreateGrid(); game.Changed += Rebuild; Rebuild();
        }
        private void CreatePreview()
        {
            if (preview != null) { preview.SetActive(false); Destroy(preview); }
            preview = Instantiate(ActiveDefinition.prefab, transform); preview.name = "Build Preview";
            foreach (var collider in preview.GetComponentsInChildren<Collider>()) collider.enabled = false;
            foreach (var lamp in preview.GetComponentsInChildren<FurnitureLighting>()) lamp.enabled=false;
            foreach (var light in preview.GetComponentsInChildren<Light>()) light.enabled=false;
            foreach (var bed in preview.GetComponentsInChildren<Bed>()) { bed.enabled=false; Destroy(bed); }
            previewRenderers = preview.GetComponentsInChildren<Renderer>();
            foreach (var renderer in previewRenderers) { renderer.sharedMaterial = previewMaterial; renderer.shadowCastingMode = ShadowCastingMode.Off; }
            preview.SetActive(false);
        }
        private void OnDestroy()
        {
            if (game != null) game.Changed -= Rebuild;
            if (previewMaterial != null) Destroy(previewMaterial);
        }
        public void SelectPiece(int index)
        {
            if(index<0||index>=game.BuildPieces.Length)return;
            dragged=null;MoveMode=false;pieceIndex=index;level=ActiveDefinition.placement==BuildPlacement.Roof?3:0;CreatePreview();
        }
        public void ToggleMoveMode(){dragged=null;MoveMode=!MoveMode;}
        public static Vector3 Position(BuildDefinition definition,int x,int level,int z)=>new Vector3(x+.5f,(definition.placement==BuildPlacement.Roof?BuildingModel.RoofHeight(level):level)+.5f,z+.5f);
        private void OnDisable()
        {
            dragged=null;
            if (preview != null) preview.SetActive(false);
            if (grid != null) grid.SetActive(false);
            if (game != null && game.BuildMode) game.SetBuildMode(false);
        }
        public void ToggleMode() { if (game.Ready) game.SetBuildMode(!game.BuildMode); }
        private void Update()
        {
            if (preview == null) return;
            var keyboard = Keyboard.current; var mouse = Mouse.current;
            if (!Application.isFocused || !game.Ready || !game.isActiveAndEnabled)
            { dragged=null;preview.SetActive(false); grid.SetActive(false); return; }
            if (game.InventoryOpen||game.MenuOpen) { preview.SetActive(false);grid.SetActive(false);return; }
            if (keyboard?.digit4Key.wasPressedThisFrame == true) ToggleMode();
            if (keyboard?.escapeKey.wasPressedThisFrame == true) game.SetBuildMode(false);
            grid.SetActive(game.BuildMode);
            grid.transform.position = new Vector3(Mathf.Floor(game.Player.position.x),0,Mathf.Floor(game.Player.position.z));
            if (!game.BuildMode) { dragged=null;preview.SetActive(false); target = null; return; }
            if(dragged!=null && dragModel!=game.Model.Building)dragged=null;
            if(keyboard?.mKey.wasPressedThisFrame==true)ToggleMoveMode();
            if (keyboard?.qKey.wasPressedThisFrame == true && dragged==null)SelectPiece((pieceIndex+1)%game.BuildPieces.Length);
            if (keyboard?.rKey.wasPressedThisFrame == true) rotation = (rotation + 1) % 4;
            if (ActiveDefinition.placement==BuildPlacement.Solid && !ActiveDefinition.IsFurniture && keyboard?.leftCtrlKey.isPressed != true && keyboard?.rightCtrlKey.isPressed != true && mouse != null && Mathf.Abs(mouse.scroll.ReadValue().y) > .01f)
                level = Mathf.Clamp(level + (mouse.scroll.ReadValue().y > 0 ? 1 : -1), 0, BuildingModel.Levels - 1);
            RefreshTarget();
            if(MoveMode)
            {
                if(dragged==null)
                {
                    preview.SetActive(false);Status="Eşyayı sol tuşla tut, sürükle ve bırak. M: İnşaya dön.";
                    if(mouse?.leftButton.wasPressedThisFrame==true && removeTarget!=null && InReach(Center(removeTarget.x,removeTarget.level,removeTarget.z)))
                    {
                        dragged=removeTarget.Copy();dragModel=game.Model.Building;
                        pieceIndex=System.Array.FindIndex(game.BuildPieces,d=>d.id==dragged.pieceId);level=dragged.level;rotation=dragged.rotation;CreatePreview();
                    }
                }
                else if(mouse?.leftButton.wasReleasedThisFrame==true)
                {
                    if(ValidPreview&&target.HasValue){var c=target.Value;game.MoveBlock(dragged,c.x,c.y,c.z,rotation);}
                    else game.ShowBuildFeedback("Taşıma iptal edildi; eşya eski yerinde. "+Status);
                    dragged=null;preview.SetActive(false);
                }
                return;
            }
            if (mouse?.leftButton.wasPressedThisFrame == true && target.HasValue)
            {
                if (ValidPreview)
                {
                    var c = target.Value;
                    game.PlaceBlock(ActiveDefinition.id, c.x, c.y, c.z, rotation);
                }
                else game.ShowBuildFeedback(Status);
            }
            if (mouse?.rightButton.wasPressedThisFrame == true && removeTarget != null)
            {
                var b = removeTarget;
                if (InReach(Center(b.x, b.level, b.z))) game.RemoveBlock(b);
                else game.ShowBuildFeedback("Sökmek için bloğa yaklaş.");
            }
        }
        private bool InReach(Vector3 center)
        {
            Vector3 offset = center - game.Player.position; offset.y = 0;
            return offset.sqrMagnitude <= 3.5f * 3.5f;
        }
        private void RefreshTarget()
        {
            target = null; removeTarget = null; ValidPreview = false; preview.SetActive(false);
            game.Selection.RefreshPointer();
            Status = "Fareyi boş zemine götür.";
            if (game.Selection.PointerBlocked || Mouse.current == null) return;
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out var hit, 100f, ~0, QueryTriggerInteraction.Ignore))
            {
                var marker = hit.collider.GetComponentInParent<PlacedBlockView>();
                if (marker != null) removeTarget = marker.Record;
            }
            Vector3? roofPoint = null;
            if (ActiveDefinition.placement==BuildPlacement.Roof)
                roofPoint = ResolveRoofTarget(ray);
            float planeHeight=ActiveDefinition.placement==BuildPlacement.Roof?BuildingModel.RoofHeight(level):level;
            var plane = new Plane(Vector3.up, Vector3.up * planeHeight);
            if (!plane.Raycast(ray, out float distance)) return;
            Vector3 point = roofPoint ?? ray.GetPoint(distance);
            int x = Mathf.FloorToInt(point.x), z = Mathf.FloorToInt(point.z);
            if (!(ActiveDefinition.placement==BuildPlacement.Roof?BuildingModel.ValidCoordinate(x,z):BuildingModel.InBounds(x, level, z))) return;
            target = new Vector3Int(x, level, z);
            Vector3 center = Position(ActiveDefinition,x,level,z);
            preview.transform.SetPositionAndRotation(center, Quaternion.Euler(0, rotation * 90, 0));
            string reason;
            bool valid = dragged!=null?game.Model.Building.CanMove(dragged,x,level,z,rotation,out reason):game.Model.Building.CanPlace(ActiveDefinition.id, x, level, z, rotation, out reason);
            if (!InReach(center)) { valid = false; reason = "Yerleştirmek için yaklaş."; }
            else if (valid)
            {
                foreach (var cell in game.Model.Building.Footprint(ActiveDefinition.id,x,z,rotation))
                {
                    if (!WorldGround.SupportsCell(cell.x,cell.z,false)) { valid=false; reason="Düz ve sağlam zemin gerekiyor."; break; }
                    int soilIndex = game.Model.IndexAt(cell.x,cell.z);
                    if (ActiveDefinition.placement!=BuildPlacement.Roof && soilIndex >= 0 && !string.IsNullOrEmpty(game.Model.Plot(soilIndex).cropId)) { valid=false; reason="Önce buradaki ürünü hasat et."; break; }

                }
            }
            if(valid && OverlapsWorld()) { valid=false;reason="Oyuncu veya başka bir nesneyle çakışıyor."; }
            ValidPreview = valid; Status = reason;
            preview.transform.SetPositionAndRotation(center, Quaternion.Euler(0, rotation * 90, 0));
            previewMaterial.SetColor("_BaseColor", valid ? new Color(.28f, .8f, .5f) : new Color(.94f, .3f, .25f));
            preview.SetActive(true);
        }
        private Vector3? ResolveRoofTarget(Ray ray)
        {
            int preferred = level;
            if (removeTarget != null)
            {
                var rule = game.Model.Building.Rules(removeTarget.pieceId);
                if (rule.Placement==BuildPlacement.Edge) preferred=2;
                else if (rule.Placement==BuildPlacement.Roof) preferred=removeTarget.level;
                else if (rule.Placement==BuildPlacement.Solid && !rule.IsFurniture) preferred=3;
            }
            // Search direct supports front-to-back along the downward camera ray.
            // A lower wall behind a valid tall support must not steal the cursor.
            for (int pass=0; pass<2; pass++)
            foreach (int candidate in pass==0 ? new[] { 3, 2 } : new[] { preferred, preferred==2 ? 3 : 2 })
            {
                var plane = new Plane(Vector3.up, Vector3.up * BuildingModel.RoofHeight(candidate));
                if (!plane.Raycast(ray, out float distance)) continue;
                var point = ray.GetPoint(distance);
                int x=Mathf.FloorToInt(point.x), z=Mathf.FloorToInt(point.z);
                if (pass==0 && !game.Model.Building.HasDirectRoofSupport(x,candidate,z)) continue;
                if (!game.Model.Building.CanSupportRoof(x,candidate,z)) continue;
                level=candidate;
                return point;
            }
            // Pointing at the face of a wall should still snap onto its top edge.
            if (removeTarget!=null && game.Model.Building.CanSupportRoof(removeTarget.x,preferred,removeTarget.z))
            {
                level=preferred;
                return new Vector3(removeTarget.x+.5f,BuildingModel.RoofHeight(level),removeTarget.z+.5f);
            }
            return null;
        }
        private bool OverlapsWorld()
        {
            foreach(var box in preview.GetComponentsInChildren<BoxCollider>(true))
            {
                var half=Vector3.Scale(box.size,box.transform.lossyScale)*.5f-Vector3.one*.012f;
                half=Vector3.Max(half,Vector3.one*.005f);
                foreach(var hit in Physics.OverlapBox(box.transform.TransformPoint(box.center),half,box.transform.rotation,~0,QueryTriggerInteraction.Ignore))
                {
                    if(hit.GetComponent<WorldGround>()!=null)continue;
                    var existing=hit.GetComponentInParent<PlacedBlockView>();
                    if(existing!=null)
                    {
                        var record=existing.Record;
                        if(dragged!=null&&record.pieceId==dragged.pieceId&&record.x==dragged.x&&record.z==dragged.z&&record.level==dragged.level&&record.rotation==dragged.rotation)continue;
                        var kind=game.Model.Building.Rules(existing.Record.pieceId).Placement;
                        if(kind==BuildPlacement.Floor)continue;
                        if(ActiveDefinition.placement==BuildPlacement.Floor)continue;
                        if(kind==BuildPlacement.Edge && ActiveDefinition.placement==BuildPlacement.Edge)continue; // Shared corners are legal; model rejects duplicate edges.
                    }
                    // A ground-level floor can be laid under the player without enclosing them.
                    if(ActiveDefinition.placement==BuildPlacement.Floor && hit.transform.IsChildOf(game.Player))continue;
                    return true;
                }
            }
            return false;
        }
        private void Rebuild()
        {
            if (displayedModel == game.Model.Building && displayedRevision == game.Model.Building.Revision) return;
            displayedModel = game.Model.Building; displayedRevision = displayedModel.Revision;
            // Disable old colliders immediately; Destroy itself is deferred until end of frame.
            foreach (var obj in placed) { obj.SetActive(false); Destroy(obj); }
            placed.Clear();
            foreach (var b in game.Model.Building.Blocks)
            {
                var definition = System.Array.Find(game.BuildPieces, d => d.id == b.pieceId);
                var obj = Instantiate(definition.prefab, Position(definition,b.x,b.level,b.z), Quaternion.Euler(0, b.rotation * 90, 0), transform);
                obj.name = $"Built {b.pieceId} {b.x},{b.level},{b.z}";
                obj.AddComponent<PlacedBlockView>().Record = b;
                if(definition.isDoor)obj.AddComponent<DoorView>().Configure(game,b);
                if(!definition.IsFurniture&&definition.placement!=BuildPlacement.Floor)obj.AddComponent<OccludingWall>();
                placed.Add(obj);
            }
            Physics.SyncTransforms();
        }
        private void CreateGrid()
        {
            grid = new GameObject("Construction Grid"); grid.transform.SetParent(transform, false);
            for (int x = -3; x <= 4; x++) Line(new Vector3(x,.018f,-3),new Vector3(x,.018f,4));
            for (int z = -3; z <= 4; z++) Line(new Vector3(-3,.018f,z),new Vector3(4,.018f,z));
            grid.SetActive(false);
        }
        private void Line(Vector3 a, Vector3 b)
        {
            var obj = new GameObject("Build Grid Line"); obj.transform.SetParent(grid.transform, false);
            var line = obj.AddComponent<LineRenderer>(); line.sharedMaterial = outlineMaterial;
            line.useWorldSpace = false; line.positionCount = 2; line.SetPosition(0, a); line.SetPosition(1, b);
            line.startWidth = line.endWidth = .014f;
            line.startColor = line.endColor = new Color(.82f, .72f, .46f);
            line.shadowCastingMode = ShadowCastingMode.Off;
        }
    }
    public sealed class PlacedBlockView : MonoBehaviour { public BlockRecord Record { get; set; } }
}
