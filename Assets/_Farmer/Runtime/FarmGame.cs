using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Farmer
{
    [DefaultExecutionOrder(-100)]
    public sealed class FarmGame : MonoBehaviour
    {
        [SerializeField] private CropDefinition[] crops;
        [SerializeField] private FarmSelection selection;
        [SerializeField] private Transform player;
        [SerializeField] private Transform market;
        [SerializeField] private Transform camp;
        [SerializeField] private int startingMoney = 60;
        public FarmModel Model { get; private set; }
        public FarmSelection Selection => selection;
        public CropDefinition ActiveCrop => crops[0];
        public CropDefinition Definition(string id) => crops.First(c => c.id == id);
        public Transform Market => market;
        public Transform Camp => camp;
        public string Feedback { get; private set; } = "Pazardan tohum alarak başlayabilirsin.";
        public string SaveStatus { get; private set; } = "Yeni çiftlik";
        public bool Ready { get; private set; }
        public bool NearMarket => Near(market);
        public bool NearCamp => Near(camp);
        public bool WateringActive { get; private set; }
        public event Action<bool> WateringChanged;
        private bool wateringGesture;
        public event Action Changed;
        public event Action<int, string> Harvested;
        private FarmSaveStore store;
        private bool smokeSession;
        public string SavePath { get; private set; }

        public void Configure(CropDefinition[] definitions, FarmSelection grid, Transform actor, Transform shop, Transform rest)
        { crops = definitions; selection = grid; player = actor; market = shop; camp = rest; }

        private void Awake()
        {
            var rules = crops.Select(c => c.Rules).ToArray();
            Model = new FarmModel(rules, selection.Layout.Width, selection.Layout.Depth, startingMoney);
            string directory = Application.persistentDataPath;
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            smokeSession = Array.IndexOf(Environment.GetCommandLineArgs(), "--farmer-smoke-capture") >= 0;
            if (smokeSession) directory = Path.Combine(Application.temporaryCachePath, "FarmerQA", Guid.NewGuid().ToString("N"));
#endif
            SavePath = Path.Combine(directory, "farm-v1.json");
            store = new FarmSaveStore(SavePath, snapshot => FarmModel.Restore(snapshot, rules, selection.Layout.Width, selection.Layout.Depth));
            LoadGame();
        }
        private void Update()
        {
            var k = Keyboard.current;
            if (!Application.isFocused) { StopWatering(); return; }
            if (k?.f9Key.wasPressedThisFrame == true) LoadGame();
            if (!Ready) { StopWatering(); return; }
            if (k?.digit1Key.wasPressedThisFrame == true) Equip(FarmItem.Seeds);
            if (k?.digit2Key.wasPressedThisFrame == true) Equip(FarmItem.WateringCan);
            if (k?.digit3Key.wasPressedThisFrame == true) Equip(FarmItem.Sickle);
            UpdateToolInput();
            if (k?.bKey.wasPressedThisFrame == true) Buy(1);
            if (k?.vKey.wasPressedThisFrame == true) SellHarvest();
            if (k?.nKey.wasPressedThisFrame == true) Rest();
            if (k?.f5Key.wasPressedThisFrame == true) SaveGame();
        }

        private void UpdateToolInput()
        {
            var mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.isPressed) { StopWatering(); return; }
            selection.RefreshPointer();
            if (Model.EquippedItem != FarmItem.WateringCan)
            {
                StopWatering();
                if (mouse.leftButton.wasPressedThisFrame) UseHovered();
                return;
            }
            // A press beginning on UI cannot spill into the world when dragged off a button.
            if (mouse.leftButton.wasPressedThisFrame)
                wateringGesture = !selection.PointerBlocked;
            bool pouring = wateringGesture && selection.HoveredInReach;
            SetWatering(pouring);
            if (!pouring) return;
            var plot = Model.Plot(HoveredIndex);
            // Persist only the first successful watering, not every frame of the held gesture.
            if (!string.IsNullOrEmpty(plot.cropId) && !plot.watered && !Model.IsReady(HoveredIndex))
                UseHovered();
        }
        private void SetWatering(bool active)
        {
            if (WateringActive == active) return;
            WateringActive = active; WateringChanged?.Invoke(active);
        }
        private void StopWatering() { wateringGesture = false; SetWatering(false); }
        private void OnApplicationFocus(bool focused) { if (!focused) StopWatering(); }
        private void OnApplicationPause(bool paused) { if (paused) StopWatering(); }
        private void OnDisable() => StopWatering();

        private bool Near(Transform target)
        {
            if (target == null || player == null) return false;
            var delta = target.position - player.position; delta.y = 0;
            return delta.sqrMagnitude <= 2.6f * 2.6f;
        }

        public Transform Player => player;
        public int HoveredIndex => selection.HoveredCell is Vector2Int c ? c.x + c.y * Model.Width : -1;
        public string EquippedName => Model.EquippedItem == FarmItem.Seeds ? ActiveCrop.displayName + " tohumu"
            : Model.EquippedItem == FarmItem.WateringCan ? "Sulama kabı" : "Orak";
        public string ActionLabel => HoveredIndex < 0 ? "Fareyi tarlaya götür" : !selection.HoveredInReach ? "Kareye yaklaş"
            : Model.EquippedItem == FarmItem.Seeds ? "Sol tık · Tohum ek" : Model.EquippedItem == FarmItem.WateringCan ? "Sol tuşu basılı tut · Sula" : "Sol tık · Hasat et";

        public void Equip(FarmItem item)
        {
            if (!Ready || !Model.Equip(item)) return;
            StopWatering();
            Feedback = item == FarmItem.WateringCan ? "Sulama kabı: sol tuşu basılı tutarak fareyi tarlada gezdir."
                : EquippedName + " seçildi. Fareyi yakındaki kareye götür ve sol tıkla.";
            SaveGame(); Changed?.Invoke();
        }

        public bool UseHovered()
        {
            if (!Ready || !Application.isFocused) return false;
            selection.RefreshPointer();
            int index = HoveredIndex;
            if (index < 0) return false; // UI and outside-world clicks are not farm actions.
            if (!selection.HoveredInReach)
            { Feedback = "Bu kare uzakta. Biraz yaklaş."; return false; }
            var plot = Model.Plot(index);
            bool harvesting = Model.EquippedItem == FarmItem.Sickle;
            bool ok = Model.UseEquipped(index, ActiveCrop.id, out string message);
            Complete(ok, message);
            if (ok && harvesting)
            {
                var definition = Definition(plot.cropId);
                Harvested?.Invoke(index, $"+{definition.harvestYield} {definition.displayName}");
            }
            return ok;
        }

        public bool Buy(int count)
        {
            if (!Ready) return false;
            if (!NearMarket) { Feedback = "Tohum almak için pazar tezgâhına yaklaş."; return false; }
            bool ok = Model.BuySeeds(ActiveCrop.id, count, out var message); return Complete(ok, message);
        }
        public bool SellHarvest()
        {
            if (!Ready) return false;
            if (!NearMarket) { Feedback = "Satış yapmak için pazar tezgâhına yaklaş."; return false; }
            bool ok = Model.Sell(ActiveCrop.id, Model.Produce(ActiveCrop.id), out var message); return Complete(ok, message);
        }
        public bool Rest()
        {
            if (!Ready) return false;
            if (!NearCamp) { Feedback = "Günü bitirmek için kamp minderine yaklaş."; return false; }
            bool ok = Model.EndDay(out var message); return Complete(ok, message);
        }
        private bool Complete(bool success, string message)
        {
            Feedback = message;
            if (success) { SaveGame(); Changed?.Invoke(); }
            return success;
        }

        public bool SaveGame()
        {
            if (!Ready) return false;
            try { store.Save(Model.Snapshot()); SaveStatus = "Kaydedildi"; return true; }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException)
            { SaveStatus = "Kayıt başarısız · F5 ile tekrar dene"; Debug.LogWarning("Farm save failed: " + e.Message); return false; }
        }
        public bool LoadGame()
        {
            StopWatering();
            try
            {
                var loaded = store.Load(out bool recovered);
                if (loaded != null) Model = loaded;
                Ready = true;
                SaveStatus = recovered ? "Yedek kayıt yüklendi" : loaded == null ? "Yeni çiftlik" : "Kayıt yüklendi";
                Feedback = recovered ? "Son sağlam yedek açıldı; önceki dosya korunacak." : "1/2/3 ile eşya seç; fareyi yakındaki kareye götür ve sol tıkla.";
                Changed?.Invoke(); return true;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException)
            {
                Ready = false; SaveStatus = "Kayıt okunamadı · Dosya korundu";
                Feedback = "Kayıt açılamadı; üzerine yazılmadı. Dosyayı kontrol edip F9 ile yeniden dene.";
                Debug.LogWarning("Farm load failed: " + e.Message); return false;
            }
        }

        private void OnApplicationQuit()
        {
            if (Ready) SaveGame();
            if (smokeSession && SavePath != null)
            {
                try { Directory.Delete(Path.GetDirectoryName(SavePath), true); }
                catch (IOException) { }
            }
        }
    }
}
