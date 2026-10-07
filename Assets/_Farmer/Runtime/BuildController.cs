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
        private BlockRecord removeTarget;
        private int level, rotation;
        public int Level => level;
        public int Rotation => rotation;
        public bool ValidPreview { get; private set; }
        public string Status { get; private set; } = "Fareyi işaretli inşa alanına götür.";
        public int VisibleBlockCount => placed.Count;
        public Vector3Int? Target => target;
        public void Configure(FarmGame source, Material line) { game = source; outlineMaterial = line; }
        public static Vector3 Center(int x, int y, int z) => new Vector3(BuildingModel.OriginX + x + .5f, y + .5f, BuildingModel.OriginZ + z + .5f);
        private void Start()
        {
            preview = Instantiate(game.BuildPieces[0].prefab, transform); preview.name = "Build Preview";
            foreach (var collider in preview.GetComponentsInChildren<Collider>()) collider.enabled = false;
            previewMaterial = new Material(outlineMaterial);
            previewMaterial.SetColor("_BaseColor", new Color(.3f, .8f, .55f));
            previewRenderers = preview.GetComponentsInChildren<Renderer>();
            foreach (var renderer in previewRenderers) { renderer.sharedMaterial = previewMaterial; renderer.shadowCastingMode = ShadowCastingMode.Off; }
            preview.SetActive(false);
            CreateGrid(); game.Changed += Rebuild; Rebuild();
        }
        private void OnDestroy()
        {
            if (game != null) game.Changed -= Rebuild;
            if (previewMaterial != null) Destroy(previewMaterial);
        }
        private void OnDisable()
        {
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
            { preview.SetActive(false); grid.SetActive(false); return; }
            if (keyboard?.digit4Key.wasPressedThisFrame == true) ToggleMode();
            if (keyboard?.escapeKey.wasPressedThisFrame == true) game.SetBuildMode(false);
            grid.SetActive(game.BuildMode);
            if (!game.BuildMode) { preview.SetActive(false); target = null; return; }
            if (keyboard?.rKey.wasPressedThisFrame == true) rotation = (rotation + 1) % 4;
            if (mouse != null && Mathf.Abs(mouse.scroll.ReadValue().y) > .01f)
                level = Mathf.Clamp(level + (mouse.scroll.ReadValue().y > 0 ? 1 : -1), 0, BuildingModel.Levels - 1);
            RefreshTarget();
            if (mouse?.leftButton.wasPressedThisFrame == true && target.HasValue)
            {
                if (ValidPreview)
                {
                    var c = target.Value;
                    game.PlaceBlock(game.BuildPieces[0].id, c.x, c.y, c.z, rotation);
                }
                else game.ShowBuildFeedback(Status);
            }
            if (mouse?.rightButton.wasPressedThisFrame == true && removeTarget != null)
            {
                var b = removeTarget;
                if (InReach(Center(b.x, b.level, b.z))) game.RemoveBlock(b.x, b.level, b.z);
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
            Status = "Fareyi işaretli inşa alanına götür.";
            if (game.Selection.PointerBlocked || Mouse.current == null) return;
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out var hit, 100f, ~0, QueryTriggerInteraction.Ignore))
            {
                var marker = hit.collider.GetComponent<PlacedBlockView>();
                if (marker != null) removeTarget = marker.Record;
            }
            var plane = new Plane(Vector3.up, Vector3.up * level);
            if (!plane.Raycast(ray, out float distance)) return;
            Vector3 point = ray.GetPoint(distance);
            int x = Mathf.FloorToInt(point.x - BuildingModel.OriginX), z = Mathf.FloorToInt(point.z - BuildingModel.OriginZ);
            if (!BuildingModel.InBounds(x, level, z)) return;
            target = new Vector3Int(x, level, z);
            Vector3 center = Center(x, level, z);
            bool valid = game.Model.Building.CanPlace(game.BuildPieces[0].id, x, level, z, rotation, out string reason);
            if (!InReach(center)) { valid = false; reason = "Yerleştirmek için yaklaş."; }
            else if (valid && Physics.CheckBox(center, Vector3.one * .48f, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore))
            { valid = false; reason = "Oyuncu veya başka bir nesneyle çakışıyor."; }
            ValidPreview = valid; Status = reason;
            preview.transform.SetPositionAndRotation(center, Quaternion.Euler(0, rotation * 90, 0));
            previewMaterial.SetColor("_BaseColor", valid ? new Color(.28f, .8f, .5f) : new Color(.94f, .3f, .25f));
            preview.SetActive(true);
        }
        private void Rebuild()
        {
            // Disable old colliders immediately; Destroy itself is deferred until end of frame.
            foreach (var obj in placed) { obj.SetActive(false); Destroy(obj); }
            placed.Clear();
            foreach (var b in game.Model.Building.Blocks)
            {
                var definition = System.Array.Find(game.BuildPieces, d => d.id == b.pieceId);
                var obj = Instantiate(definition.prefab, Center(b.x, b.level, b.z), Quaternion.Euler(0, b.rotation * 90, 0), transform);
                obj.name = $"Built {b.pieceId} {b.x},{b.level},{b.z}";
                obj.AddComponent<PlacedBlockView>().Record = b;
                placed.Add(obj);
            }
            Physics.SyncTransforms();
        }
        private void CreateGrid()
        {
            grid = new GameObject("Construction Grid"); grid.transform.SetParent(transform, false);
            for (int x = 0; x <= BuildingModel.Width; x++)
                Line(new Vector3(BuildingModel.OriginX + x, .018f, BuildingModel.OriginZ), new Vector3(BuildingModel.OriginX + x, .018f, BuildingModel.OriginZ + BuildingModel.Depth));
            for (int z = 0; z <= BuildingModel.Depth; z++)
                Line(new Vector3(BuildingModel.OriginX, .018f, BuildingModel.OriginZ + z), new Vector3(BuildingModel.OriginX + BuildingModel.Width, .018f, BuildingModel.OriginZ + z));
            grid.SetActive(false);
        }
        private void Line(Vector3 a, Vector3 b)
        {
            var obj = new GameObject("Build Grid Line"); obj.transform.SetParent(grid.transform, false);
            var line = obj.AddComponent<LineRenderer>(); line.sharedMaterial = outlineMaterial;
            line.positionCount = 2; line.SetPosition(0, a); line.SetPosition(1, b);
            line.startWidth = line.endWidth = .014f;
            line.startColor = line.endColor = new Color(.82f, .72f, .46f);
            line.shadowCastingMode = ShadowCastingMode.Off;
        }
    }
    public sealed class PlacedBlockView : MonoBehaviour { public BlockRecord Record { get; set; } }
}
