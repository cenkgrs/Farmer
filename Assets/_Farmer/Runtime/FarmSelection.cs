using UnityEngine;
using UnityEngine.InputSystem;

namespace Farmer
{
    public sealed class FarmSelection : MonoBehaviour
    {
        [SerializeField] private Camera viewCamera;
        [SerializeField] private Transform player;
        [SerializeField] private LineRenderer hoverOutline;
        [SerializeField] private LineRenderer selectedOutline;
        [SerializeField] private RectTransform[] hudPanels;
        [SerializeField] private Vector2 origin = new Vector2(-3, -3);
        [SerializeField] private int width = 6;
        [SerializeField] private int depth = 6;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private float reach = 2.5f;
        private FarmGridLayout layout;
        public FarmGridLayout Layout => layout ??= new FarmGridLayout(origin, width, depth, cellSize, true);
        public Vector2Int? HoveredCell { get; private set; }
        public Vector2Int? WorldCell => HoveredCell.HasValue ? new Vector2Int(Mathf.FloorToInt(Layout.Center(HoveredCell.Value).x), Mathf.FloorToInt(Layout.Center(HoveredCell.Value).z)) : (Vector2Int?)null;
        public bool HideOutline { get; set; }
        public bool PointerBlocked { get; private set; } = true;
        public bool HoveredInReach => HoveredCell.HasValue && InReach(HoveredCell.Value);
        public void SetHudPanels(RectTransform[] panels) => hudPanels = panels;

        public void Configure(Camera camera, Transform actor, LineRenderer hover, LineRenderer selected, RectTransform[] panels)
        {
            viewCamera = camera; player = actor; hoverOutline = hover; selectedOutline = selected; hudPanels = panels;
        }

        private void Update() => RefreshPointer();

        // Re-sample on use as FarmGame can update before this component in the same frame.
        public void RefreshPointer()
        {
            var mouse = Mouse.current;
            if (mouse == null || !Application.isFocused)
            {
                HoveredCell = null; PointerBlocked = true;
                DrawOutlines();
                return;
            }
            UpdatePointer(mouse.position.ReadValue());
        }

        public void UpdatePointer(Vector2 screenPosition)
        {
            HoveredCell = null;
            bool blocked = screenPosition.x < 0 || screenPosition.y < 0 || screenPosition.x >= Screen.width || screenPosition.y >= Screen.height;
            if (hudPanels != null)
                foreach (var panel in hudPanels)
                    blocked |= panel != null && panel.gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(panel, screenPosition);

            PointerBlocked = blocked;
            var plane = new Plane(Vector3.up, new Vector3(0, 0.055f, 0));
            if (!blocked && viewCamera != null)
            {
                Ray ray = viewCamera.ScreenPointToRay(screenPosition);
                if (plane.Raycast(ray, out float distance) && Layout.TryGetCell(ray.GetPoint(distance), out var cell))
                {
                    Vector3 center = Layout.Center(cell);
                    if (WorldGround.SupportsCell(Mathf.FloorToInt(center.x), Mathf.FloorToInt(center.z), true)) HoveredCell = cell;
                }
            }
            DrawOutlines();
        }

        private bool InReach(Vector2Int cell) => player != null && Layout.IsWithinReach(cell, player.position, reach);

        private void DrawOutlines()
        {
            Draw(hoverOutline, HideOutline ? null : HoveredCell, HoveredCell.HasValue && InReach(HoveredCell.Value)
                ? new Color(1f, 0.96f, 0.78f) : new Color(1f, 0.45f, 0.22f), 0.48f, 0.09f);
            if (selectedOutline != null) selectedOutline.enabled = false;
        }

        private void Draw(LineRenderer line, Vector2Int? cell, Color color, float inset, float height)
        {
            if (line == null) return;
            line.enabled = cell.HasValue;
            if (!cell.HasValue) return;
            Vector3 c = Layout.Center(cell.Value, height);
            float r = cellSize * inset;
            line.startColor = line.endColor = color;
            line.SetPositions(new[] { c + new Vector3(-r, 0, -r), c + new Vector3(r, 0, -r),
                c + new Vector3(r, 0, r), c + new Vector3(-r, 0, r) });
        }
    }
}
