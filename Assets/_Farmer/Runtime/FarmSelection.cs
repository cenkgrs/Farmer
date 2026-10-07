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
        public FarmGridLayout Layout => layout ??= new FarmGridLayout(origin, width, depth, cellSize);
        public Vector2Int? HoveredCell { get; private set; }
        public Vector2Int? SelectedCell { get; private set; }
        public string Feedback { get; private set; } = "Bir tarla karesine yaklaş ve tıkla.";
        public bool SelectedInReach => SelectedCell.HasValue && InReach(SelectedCell.Value);

        public void Configure(Camera camera, Transform actor, LineRenderer hover, LineRenderer selected, RectTransform[] panels)
        {
            viewCamera = camera; player = actor; hoverOutline = hover; selectedOutline = selected; hudPanels = panels;
        }

        private void Update()
        {
            var mouse = Mouse.current;
            bool clear = Keyboard.current?.escapeKey.wasPressedThisFrame == true || mouse?.rightButton.wasPressedThisFrame == true;
            if (mouse == null || !Application.isFocused)
            {
                HoveredCell = null;
                DrawOutlines();
                return;
            }
            UpdatePointer(mouse.position.ReadValue(), mouse.leftButton.wasPressedThisFrame, clear);
        }

        public void UpdatePointer(Vector2 screenPosition, bool select, bool clear)
        {
            if (clear)
            {
                SelectedCell = null;
                Feedback = "Seçim temizlendi.";
            }
            HoveredCell = null;
            bool blocked = screenPosition.x < 0 || screenPosition.y < 0 || screenPosition.x >= Screen.width || screenPosition.y >= Screen.height;
            if (hudPanels != null)
                foreach (var panel in hudPanels)
                    blocked |= panel != null && RectTransformUtility.RectangleContainsScreenPoint(panel, screenPosition);

            var plane = new Plane(Vector3.up, new Vector3(0, 0.055f, 0));
            if (!blocked && viewCamera != null)
            {
                Ray ray = viewCamera.ScreenPointToRay(screenPosition);
                if (plane.Raycast(ray, out float distance) && Layout.TryGetCell(ray.GetPoint(distance), out var cell))
                    HoveredCell = cell;
            }
            if (select && !blocked)
            {
                if (!HoveredCell.HasValue) { SelectedCell = null; Feedback = "Tarlanın içindeki bir kareyi seç."; }
                else if (!InReach(HoveredCell.Value)) Feedback = "Bu kare uzakta. Biraz yaklaş.";
                else
                {
                    SelectedCell = HoveredCell;
                    Feedback = "Kare seçildi.";
                }
            }
            DrawOutlines();
        }

        private bool InReach(Vector2Int cell) => player != null && Layout.IsWithinReach(cell, player.position, reach);

        private void DrawOutlines()
        {
            Draw(hoverOutline, HoveredCell, HoveredCell.HasValue && InReach(HoveredCell.Value)
                ? new Color(1f, 0.96f, 0.78f) : new Color(1f, 0.45f, 0.22f), 0.48f, 0.09f);
            Draw(selectedOutline, SelectedCell, SelectedInReach
                ? new Color(0.98f, 0.81f, 0.24f) : new Color(1f, 0.45f, 0.22f), 0.43f, 0.10f);
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
