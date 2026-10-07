using UnityEngine;
using UnityEngine.UI;

namespace Farmer
{
    public sealed class PrototypeHud : MonoBehaviour
    {
        [SerializeField] private FarmSelection selection;
        [SerializeField] private Text status;
        [SerializeField] private Text feedback;
        public void Configure(FarmSelection source, Text statusLabel, Text feedbackLabel)
        {
            selection = source; status = statusLabel; feedback = feedbackLabel;
        }
        private void LateUpdate()
        {
            if (selection == null) return;
            if (selection.HoveredCell is Vector2Int cell)
                status.text = $"KARE {cell.x + 1} / {cell.y + 1}\n" + (selection.HoveredInReach ? "Erişim mesafesinde" : "Yaklaşman gerekiyor");
            else status.text = "TARLANI KEŞFET\n6 × 6 ekim alanı";
            feedback.text = "Fareyi yakındaki kareye götür.";
        }
    }
}
