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
        private Button buyOne, buyFive, buyWood, sell, sleep, buildButton;
        private Text controls;
        private BuildController builder;
        private readonly Button[] slots = new Button[3];
        private readonly Text[] slotLabels = new Text[3];
        private static readonly Color Gold = new Color(0.98f, 0.84f, 0.49f);
        public void Configure(FarmGame source) => game = source;

        private void Start()
        {
            builder = game.GetComponent<BuildController>();
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
            summary = Label(header, "", 19, 260, 12, 950, 32, Color.white);
            objective = Label(header, "", 16, 18, 55, 925, 28, Color.white);
            saveStatus = Label(header, "", 13, 955, 51, 260, 40, Gold);

            var detail = Panel("Cell Details", root.transform, new Vector2(1, 1), new Vector2(-24, -130), new Vector2(260, 142));
            selectionStatus = Label(detail, "", 18, 16, 13, 232, 117, Color.white);
            shop = Panel("Market", root.transform, new Vector2(0, 1), new Vector2(24, -130), new Vector2(265, 294));
            Label(shop, "PAZAR  ·  " + game.ActiveCrop.displayName, 21, 16, 12, 233, 33, Gold);
            Label(shop, $"Tohum {game.ActiveCrop.seedPrice}  /  Ürün {game.ActiveCrop.salePrice} para", 15, 16, 47, 233, 25, Color.white);
            buyOne = Button(shop, $"B · 1 tohum al ({game.ActiveCrop.seedPrice})", 16, 79, 233, 40, () => game.Buy(1));
            buyFive = Button(shop, $"5 tohum al ({game.ActiveCrop.seedPrice * 5})", 16, 129, 233, 40, () => game.Buy(5));
            sell = Button(shop, "V · Ürünlerin hepsini sat", 16, 179, 233, 40, () => game.SellHarvest());

            buyWood = Button(shop, "10 odun al (20 para)", 16, 229, 233, 40, () => game.BuyWood());

            camp = Panel("Camp", root.transform, new Vector2(0, 1), new Vector2(24, -130), new Vector2(265, 214));
            Label(camp, "KAMP · DİNLEN", 21, 16, 12, 233, 32, Gold);
            campWarning = Label(camp, "", 16, 16, 53, 233, 90, Color.white);
            sleep = Button(camp, "N · Günü bitir", 16, 158, 233, 40, () => game.Rest());

            var footer = Panel("Controls", root.transform, new Vector2(0.5f, 0), new Vector2(0, 18), new Vector2(1232, 106));
            controls = Label(footer, "WASD / OKLAR  Hareket    FARE  Hedefle    SOL TIK  Eşyayı kullan    PAZAR: B Al · V Sat    KAMP: N Yeni gün",
                14, 18, 7, 1195, 24, Gold);
            for (int i = 0; i < slots.Length; i++)
            {
                var item = (FarmItem)i;
                slots[i] = Button(footer, "", 18 + i * 190, 36, 180, 38, () => game.Equip(item));
                slots[i].gameObject.name = "Inventory Slot " + i;
                slotLabels[i] = slots[i].GetComponentInChildren<Text>();
            }
            buildButton = Button(footer, "4 · İnşa", 590, 36, 145, 38, () => builder.ToggleMode());
            buildButton.gameObject.name = "Build Mode Button";
            actionText = Label(footer, "", 15, 752, 40, 455, 38, Gold);
            feedback = Label(footer, "", 14, 18, 79, 1195, 24, Color.white);
            game.Selection.SetHudPanels(new[] { header, detail, shop, camp, footer });
            Refresh();
        }

        private void LateUpdate() { if (summary != null) Refresh(); }
        private void Refresh()
        {
            var model = game.Model; var crop = game.ActiveCrop;
            summary.text = $"GÜN {model.Day}     •     {model.Money} para     •     Tohum {model.Seeds(crop.id)}     •     {crop.displayName} {model.Produce(crop.id)}     •     Odun {model.Building.Wood}";
            saveStatus.text = game.SaveStatus + "\nF5 Kaydet  /  F9 Yükle";
            feedback.text = game.Feedback;
            if (!game.Ready) objective.text = "Kayıt sorunu çözülene kadar çiftlik işlemleri duraklatıldı.";
            else if (game.BuildMode) objective.text = $"Ahşap blok · {game.BuildPieces[0].woodCost} odun. İşaretli alanda kendi yapını kur; sökünce odun geri gelir.";
            else if (model.ReadyCount > 0) objective.text = $"{model.ReadyCount} ürün hasada hazır. Orağı al (3), hedefle ve sol tıkla topla.";
            else if (model.ThirstyCount > 0) objective.text = $"{model.ThirstyCount} kare sulama bekliyor. Sulama kabını al (2), sol tuşu basılı tut ve gezdir.";
            else if (model.Produce(crop.id) > 0) objective.text = "Hasadını pazarda sat; kazancınla yeni tohumlar al.";
            else if (model.PlantedCount > 0) objective.text = "Bitkilerin sulandı. Kampta dinlenerek yeni güne geç.";
            else if (model.Seeds(crop.id) > 0) objective.text = "Tohumu al (1), boş kareyi fareyle hedefle ve sol tıkla ek.";
            else objective.text = "Çizgili pazar tezgâhına yaklaş; B ile ilk tohumunu al.";

            int index = game.HoveredIndex;
            if (index < 0) selectionStatus.text = $"TARLANI KEŞFET\n\nFareyi kareye götür.\n{crop.displayName}: {crop.wateredDays} gece · tek sulama";
            else
            {
                var plot = model.Plot(index); var cell = game.Selection.HoveredCell.Value;
                string state = string.IsNullOrEmpty(plot.cropId) ? "Boş toprak" : model.IsReady(index) ? "Hasada hazır!"
                    : $"{game.Definition(plot.cropId).displayName} · Aşama {model.Stage(index) + 1}/4\n" + (plot.watered ? "Sulandı · bakım tamam" : "Su bekliyor");
                selectionStatus.text = $"KARE {cell.x + 1} / {cell.y + 1}\n{state}\n" + (game.Selection.HoveredInReach ? "Erişim mesafesinde" : "Kareye yaklaş");
            }
            if (game.BuildMode)
                selectionStatus.text = $"İNŞA · Ahşap blok\nYükseklik {builder.Level + 1}/{BuildingModel.Levels} · Dönüş {builder.Rotation * 90}°\n{builder.Status}";
            actionText.text = game.BuildMode ? builder.Status : game.ActionLabel;
            controls.text = game.BuildMode
                ? "WASD  Hareket    SOL TIK  Yerleştir    SAĞ TIK  Sök    R  Döndür    TEKERLEK  Yükseklik    ESC / 1–3  Çık"
                : "WASD / OKLAR  Hareket    SOL TIK  Eşyayı kullan    4  İnşa    PAZAR: B Al · V Sat    KAMP: N Yeni gün";
            buildButton.GetComponent<Image>().color = game.BuildMode ? new Color(.64f, .48f, .21f) : new Color(.29f, .43f, .30f);
            buildButton.interactable = game.Ready;
            for (int i = 0; i < slots.Length; i++)
            {
                var item = (FarmItem)i;
                string name = item == FarmItem.Seeds ? crop.displayName + " tohumu" : item == FarmItem.WateringCan ? "Sulama kabı" : "Orak";
                slotLabels[i].text = $"{i + 1} · {name} ×{model.ItemCount(item, crop.id)}";
                slots[i].GetComponent<Image>().color = !game.BuildMode && model.EquippedItem == item ? new Color(0.64f, 0.48f, 0.21f) : new Color(0.29f, 0.43f, 0.30f);
                slots[i].interactable = game.Ready;
            }
            shop.gameObject.SetActive(game.NearMarket);
            camp.gameObject.SetActive(game.NearCamp);
            buyOne.interactable = game.Ready && model.Money >= crop.seedPrice && model.Seeds(crop.id) < FarmModel.StackLimit;
            buyFive.interactable = game.Ready && model.Money >= crop.seedPrice * 5 && model.Seeds(crop.id) <= FarmModel.StackLimit - 5;
            sell.interactable = game.Ready && model.Produce(crop.id) > 0;
            buyWood.interactable = game.Ready && model.Money >= BuildingModel.WoodPackPrice && model.Building.Wood <= BuildingModel.WoodLimit - BuildingModel.WoodPackCount;
            sleep.interactable = game.Ready;
            campWarning.text = model.ThirstyCount > 0 ? $"{model.ThirstyCount} bitki henüz sulanmadı.\nBu gece büyümeyecekler."
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
