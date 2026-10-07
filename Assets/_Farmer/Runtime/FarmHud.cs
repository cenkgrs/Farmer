using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Farmer
{
    public sealed class FarmHud : MonoBehaviour
    {
        [SerializeField] private FarmGame game;
        private Text summary, objective, saveStatus, selectionStatus, feedback, campWarning, actionText;
        private RectTransform shop, camp;
        private Button buyOne, buyFive, sell, sleep, action;
        private static readonly Color Gold = new Color(0.98f, 0.84f, 0.49f);
        public void Configure(FarmGame source) => game = source;

        private void Start()
        {
            var root = new GameObject("Farm HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720); scaler.matchWidthOrHeight = 0.5f;
            if (EventSystem.current == null)
            {
                var events = new GameObject("Farm UI Input", typeof(EventSystem), typeof(InputSystemUIInputModule));
                events.transform.SetParent(transform, false);
                events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
            var header = Panel("Header", root.transform, new Vector2(0.5f, 1), new Vector2(0, -20), new Vector2(1232, 92));
            Label(header, "F A R M E R", 25, 18, 10, 230, 34, Gold);
            summary = Label(header, "", 21, 260, 12, 950, 32, Color.white);
            objective = Label(header, "", 16, 18, 55, 925, 28, Color.white);
            saveStatus = Label(header, "", 13, 955, 51, 260, 40, Gold);

            var detail = Panel("Cell Details", root.transform, new Vector2(1, 1), new Vector2(-24, -130), new Vector2(260, 142));
            selectionStatus = Label(detail, "", 18, 16, 13, 232, 117, Color.white);
            shop = Panel("Market", root.transform, new Vector2(0, 1), new Vector2(24, -130), new Vector2(265, 242));
            Label(shop, "PAZAR  ·  " + game.ActiveCrop.displayName, 21, 16, 12, 233, 33, Gold);
            Label(shop, $"Tohum {game.ActiveCrop.seedPrice}  /  Ürün {game.ActiveCrop.salePrice} para", 15, 16, 47, 233, 25, Color.white);
            buyOne = Button(shop, $"B · 1 tohum al ({game.ActiveCrop.seedPrice})", 16, 79, 233, 40, () => game.Buy(1));
            buyFive = Button(shop, $"5 tohum al ({game.ActiveCrop.seedPrice * 5})", 16, 129, 233, 40, () => game.Buy(5));
            sell = Button(shop, "V · Ürünlerin hepsini sat", 16, 179, 233, 40, () => game.SellHarvest());

            camp = Panel("Camp", root.transform, new Vector2(0, 1), new Vector2(24, -130), new Vector2(265, 214));
            Label(camp, "KAMP · DİNLEN", 21, 16, 12, 233, 32, Gold);
            campWarning = Label(camp, "", 16, 16, 53, 233, 90, Color.white);
            sleep = Button(camp, "N · Günü bitir", 16, 158, 233, 40, () => game.Rest());

            var footer = Panel("Controls", root.transform, new Vector2(0.5f, 0), new Vector2(0, 18), new Vector2(1232, 106));
            Label(footer, "WASD / OKLAR  Hareket     SOL TIK  Kare seç     E  Ek / sula / hasat\nPAZARDA: B  Tohum al · V  Sat      KAMPTA: N  Yeni gün      ESC / SAĞ TIK  Temizle",
                15, 18, 9, 930, 46, Gold);
            feedback = Label(footer, "", 16, 18, 68, 945, 29, Color.white);
            action = Button(footer, "Bir kare seç", 970, 29, 244, 48, () => game.ActOnSelected());
            actionText = action.GetComponentInChildren<Text>();
            game.Selection.SetHudPanels(new[] { header, detail, shop, camp, footer });
            Refresh();
        }

        private void LateUpdate() { if (summary != null) Refresh(); }
        private void Refresh()
        {
            var model = game.Model; var crop = game.ActiveCrop;
            summary.text = $"GÜN {model.Day}     •     {model.Money} para     •     Tohum {model.Seeds(crop.id)}     •     {crop.displayName} {model.Produce(crop.id)}";
            saveStatus.text = game.SaveStatus + "\nF5 Kaydet  /  F9 Yükle";
            feedback.text = game.Feedback;
            if (!game.Ready) objective.text = "Kayıt sorunu çözülene kadar çiftlik işlemleri duraklatıldı.";
            else if (model.ReadyCount > 0) objective.text = $"{model.ReadyCount} ürün hasada hazır. Topla ve pazarda sat.";
            else if (model.ThirstyCount > 0) objective.text = $"{model.ThirstyCount} kare sulama bekliyor. Kareyi seçip E'ye bas.";
            else if (model.Produce(crop.id) > 0) objective.text = "Hasadını pazarda sat; kazancınla yeni tohumlar al.";
            else if (model.PlantedCount > 0) objective.text = "Bitkilerin sulandı. Kampta dinlenerek yeni güne geç.";
            else if (model.Seeds(crop.id) > 0) objective.text = "Boş bir kareye yaklaş, sol tıkla seç ve E ile tohum ek.";
            else objective.text = "Çizgili pazar tezgâhına yaklaş; B ile ilk tohumunu al.";

            int index = game.SelectedIndex;
            if (index < 0) selectionStatus.text = $"TARLANI KEŞFET\n\nBoş bir kare seç.\n{crop.displayName}: {crop.wateredDays} sulanmış gün";
            else
            {
                var plot = model.Plot(index); var cell = game.Selection.SelectedCell.Value;
                string state = string.IsNullOrEmpty(plot.cropId) ? "Boş toprak" : model.IsReady(index) ? "Hasada hazır!"
                    : $"{game.Definition(plot.cropId).displayName} · Aşama {model.Stage(index) + 1}/4\n" + (plot.watered ? "Bugün sulandı" : "Su bekliyor");
                selectionStatus.text = $"KARE {cell.x + 1} / {cell.y + 1}\n{state}\n" + (game.Selection.SelectedInReach ? "Erişim mesafesinde" : "Kareye yaklaş");
            }
            actionText.text = game.ActionLabel;
            action.interactable = game.Ready && index >= 0 && game.Selection.SelectedInReach
                && (!model.Plot(index).watered || model.IsReady(index));
            shop.gameObject.SetActive(game.NearMarket);
            camp.gameObject.SetActive(game.NearCamp);
            buyOne.interactable = game.Ready && model.Money >= crop.seedPrice && model.Seeds(crop.id) < FarmModel.StackLimit;
            buyFive.interactable = game.Ready && model.Money >= crop.seedPrice * 5 && model.Seeds(crop.id) <= FarmModel.StackLimit - 5;
            sell.interactable = game.Ready && model.Produce(crop.id) > 0;
            sleep.interactable = game.Ready;
            campWarning.text = model.ThirstyCount > 0 ? $"{model.ThirstyCount} bitki bugün sulanmadı.\nBu gece büyümeyecekler."
                : "Sulanan bitkilerin gece\nboyunca büyüyecek.\nHazır olduğunda dinlen.";
        }

        private static RectTransform Panel(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Image)); var r = obj.GetComponent<RectTransform>();
            r.SetParent(parent, false); r.anchorMin = r.anchorMax = r.pivot = anchor; r.anchoredPosition = position; r.sizeDelta = size;
            obj.GetComponent<Image>().color = new Color(0.10f, 0.17f, 0.15f, 0.96f);
            return r;
        }
        private static Text Label(Transform parent, string value, int size, float x, float y, float width, float height, Color color)
        {
            var obj = new GameObject("Label", typeof(RectTransform), typeof(Text)); var r = obj.GetComponent<RectTransform>();
            r.SetParent(parent, false); r.anchorMin = r.anchorMax = r.pivot = new Vector2(0, 1);
            r.anchoredPosition = new Vector2(x, -y); r.sizeDelta = new Vector2(width, height);
            var t = obj.GetComponent<Text>(); t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = size; t.text = value; t.color = color; t.raycastTarget = false; return t;
        }
        private static Button Button(Transform parent, string label, float x, float y, float width, float height, UnityEngine.Events.UnityAction onClick)
        {
            var r = Panel(label, parent, new Vector2(0, 1), new Vector2(x, -y), new Vector2(width, height));
            r.GetComponent<Image>().color = new Color(0.29f, 0.43f, 0.30f);
            var b = r.gameObject.AddComponent<Button>(); b.targetGraphic = r.GetComponent<Image>();
            b.navigation = new Navigation { mode = Navigation.Mode.None };
            var colors = b.colors; colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.55f); b.colors = colors;
            var text = Label(r, label, 16, 6, 2, width - 12, height - 4, Color.white); text.alignment = TextAnchor.MiddleCenter;
            b.onClick.AddListener(onClick); return b;
        }
    }
}
