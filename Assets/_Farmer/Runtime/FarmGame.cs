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
            selection.FeedbackChanged += OnSelectionFeedback;
            LoadGame();
        }
        private void OnSelectionFeedback(string message) => Feedback = message;
        private void OnDestroy() { if (selection != null) selection.FeedbackChanged -= OnSelectionFeedback; }

        private void Update()
        {
            var k = Keyboard.current;
            if (!Application.isFocused || k == null) return;
            if (k.f9Key.wasPressedThisFrame) LoadGame();
            if (!Ready) return;
            if (k.eKey.wasPressedThisFrame) ActOnSelected();
            if (k.bKey.wasPressedThisFrame) Buy(1);
            if (k.vKey.wasPressedThisFrame) SellHarvest();
            if (k.nKey.wasPressedThisFrame) Rest();
            if (k.f5Key.wasPressedThisFrame) SaveGame();
        }

        private bool Near(Transform target)
        {
            if (target == null || player == null) return false;
            var delta = target.position - player.position; delta.y = 0;
            return delta.sqrMagnitude <= 2.6f * 2.6f;
        }

        public int SelectedIndex => selection.SelectedCell is Vector2Int c ? c.x + c.y * Model.Width : -1;
        public string ActionLabel
        {
            get
            {
                int i = SelectedIndex;
                if (i < 0) return "Bir kare seç";
                if (!selection.SelectedInReach) return "Kareye yaklaş";
                var plot = Model.Plot(i);
                if (string.IsNullOrEmpty(plot.cropId)) return "E · " + ActiveCrop.displayName + " ek";
                if (Model.IsReady(i)) return "E · Hasat et";
                return plot.watered ? "Bugün sulandı" : "E · Sula";
            }
        }

        public bool ActOnSelected()
        {
            if (!Ready) return false;
            int index = SelectedIndex;
            if (index < 0 || !selection.SelectedInReach) { Feedback = "Önce yakındaki bir tarla karesini seç."; return false; }
            var plot = Model.Plot(index);
            bool wasReady = Model.IsReady(index);
            bool ok; string message;
            if (string.IsNullOrEmpty(plot.cropId)) ok = Model.Plant(index, ActiveCrop.id, out message);
            else if (Model.IsReady(index)) ok = Model.Harvest(index, out message);
            else ok = Model.Water(index, out message);
            Complete(ok, message);
            if (ok && wasReady)
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
            try
            {
                var loaded = store.Load(out bool recovered);
                if (loaded != null) Model = loaded;
                Ready = true;
                SaveStatus = recovered ? "Yedek kayıt yüklendi" : loaded == null ? "Yeni çiftlik" : "Kayıt yüklendi";
                Feedback = recovered ? "Son sağlam yedek açıldı; önceki dosya korunacak." : "Pazar: B ile tohum al. Tarla: bir kare seç ve E'ye bas.";
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
