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
        [SerializeField] private BuildDefinition[] buildPieces;
        public BuildDefinition[] BuildPieces => buildPieces;
        public bool BuildMode { get; private set; }
        public bool InventoryOpen { get; set; }
        [SerializeField] private FarmSelection selection;
        [SerializeField] private Transform player;
        [SerializeField] private Transform market;
        [SerializeField] private Transform camp;
        [SerializeField] private int startingMoney = 50;
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
        public bool NearCamp => Bed.IsNear(player.position); // Compatibility name; proximity is to a real bed.
        public bool WateringActive { get; private set; }
        public event Action<bool> WateringChanged;
        private bool wateringGesture;
        public event Action Changed;
        public event Action<int, string> Harvested;
        public event Action<Vector3> Hoed;
        public event Action<Vector3> ResourceUsed;
        public void NotifyTimeAdvanced() { SaveGame(); Changed?.Invoke(); }
        public Vector3 PlotCenter(int index, float height = .06f) { var p = Model.Plot(index); return new Vector3(p.x+.5f,height,p.z+.5f); }
        private FarmSaveStore store;
        private bool smokeSession;
        public string SavePath { get; private set; }

        public void Configure(CropDefinition[] definitions, FarmSelection grid, Transform actor, Transform shop, Transform rest)
        { crops = definitions; selection = grid; player = actor; market = shop; camp = rest; }

        private void Awake()
        {
            Application.targetFrameRate=60;
            if(GetComponent<StorageInteraction>()==null)gameObject.AddComponent<StorageInteraction>();
            var rules = crops.Select(c => c.Rules).ToArray();
            Model = new FarmModel(rules, selection.Layout.Width, selection.Layout.Depth, startingMoney, BuildingRules());
            string directory = Application.persistentDataPath;
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            smokeSession = Array.IndexOf(Environment.GetCommandLineArgs(), "--farmer-smoke-capture") >= 0;
            if (smokeSession) directory = Path.Combine(Application.temporaryCachePath, "FarmerQA", Guid.NewGuid().ToString("N"));
#endif
            SavePath = Path.Combine(directory, "farm-v1.json");
            store = new FarmSaveStore(SavePath, snapshot => FarmModel.Restore(snapshot, rules, selection.Layout.Width, selection.Layout.Depth, BuildingRules()));
            if(camp!=null)camp.gameObject.SetActive(false);
            LoadGame();
        }
        private void Update()
        {
            var k = Keyboard.current;
            if (!Application.isFocused) { StopWatering(); return; }
            if (k?.f9Key.wasPressedThisFrame == true) LoadGame();
            if (!Ready || InventoryOpen) { StopWatering(); return; }
            if (k?.digit1Key.wasPressedThisFrame == true) Equip(FarmItem.Seeds);
            if (k?.digit2Key.wasPressedThisFrame == true) Equip(FarmItem.WateringCan);
            if (k?.digit3Key.wasPressedThisFrame == true) Equip(FarmItem.Sickle);
            if (k?.digit5Key.wasPressedThisFrame == true) Equip(FarmItem.Hoe);
            if (k?.digit6Key.wasPressedThisFrame == true) Equip(FarmItem.Axe);
            if (BuildMode) StopWatering(); else UpdateToolInput();
            if (k?.bKey.wasPressedThisFrame == true) Buy(1);
            if (k?.vKey.wasPressedThisFrame == true) SellHarvest();
            if (k?.nKey.wasPressedThisFrame == true) Rest();
            if (k?.f5Key.wasPressedThisFrame == true) SaveGame();
        }

        private BuildRules[] BuildingRules() => buildPieces != null && buildPieces.Length > 0 ? buildPieces.Select(b => b.Rules).ToArray() : BuildRules.Defaults;
        public void SetBuildMode(bool active)
        {
            if (!Ready && active) return;
            BuildMode = active; selection.HideOutline = active; StopWatering(); Changed?.Invoke();
        }
        public bool BuyWood()
        {
            if (!Ready) return false;
            if (!NearMarket) { Feedback = "Odun almak için pazara yaklaş."; return false; }
            return Complete(Model.BuyWood(out var message), message);
        }
        public bool BuyBed()
        {
            if(!Ready||!NearMarket)return false;
            return Complete(Model.BuyBed(out var message),message);
        }
        public bool BuyFurniture(string id)
        {
            if(!Ready||!NearMarket)return false;
            return Complete(Model.BuyFurniture(id,out var message),message);
        }
        public bool TransferStorage(string chest,string item,int count,bool withdraw)
        {
            if(!Ready||!GetComponent<StorageInteraction>().CanUse(chest))return false;
            return Complete(Model.TransferStorage(chest,item,count,withdraw,out var message),message);
        }
        public bool MoveBlock(BlockRecord record,int x,int level,int z,int rotation)
        {
            if(!Ready||!BuildMode)return false;
            return Complete(Model.Building.Move(record,x,level,z,rotation,out var message),message);
        }
        public bool PlaceBlock(string id, int x, int level, int z, int rotation)
        {
            if (!Ready || !BuildMode) return false;
            return Complete(Model.Building.Place(id, x, level, z, rotation, out var message), message);
        }
        public bool RemoveBlock(int x, int level, int z)
        {
            if (!Ready || !BuildMode) return false;
            return Complete(Model.Building.Remove(x, level, z, out var message), message);
        }
        public bool RemoveBlock(BlockRecord record)
        {
            if(!Ready||!BuildMode)return false;
            return Complete(Model.Building.Remove(record,out var message),message);
        }
        public bool ToggleDoor(BlockRecord record)
        {
            if(!Ready||BuildMode)return false;
            return Complete(Model.Building.ToggleDoor(record,out var message),message);
        }
        public void ShowBuildFeedback(string message) => Feedback = message;

        private void UpdateToolInput()
        {
            var mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.isPressed) { StopWatering(); return; }
            selection.RefreshPointer();
            var exploration=GetComponent<ExplorationController>();
            if(exploration!=null&&exploration.RefreshPointer())
            {StopWatering();if(mouse.leftButton.wasPressedThisFrame)exploration.UseTarget();return;}
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
            if (HoveredIndex < 0) return;
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
        public int HoveredIndex => selection.WorldCell is Vector2Int c ? Model.IndexAt(c.x,c.y) : -1;
        public string EquippedName => Model.EquippedItem == FarmItem.Seeds ? ActiveCrop.displayName + " tohumu"
            : Model.EquippedItem == FarmItem.WateringCan ? "Sulama kabı" : Model.EquippedItem == FarmItem.Hoe ? "Çapa" : Model.EquippedItem==FarmItem.Axe?"Balta":"Orak";
        public string ActionLabel => !selection.WorldCell.HasValue ? "Fareyi toprağa götür" : !selection.HoveredInReach ? "Kareye yaklaş"
            : Model.EquippedItem == FarmItem.Axe?"Ağacı hedefle · Sol tıkla kes": Model.EquippedItem == FarmItem.Hoe ? "Sol tık · Toprağı çapala" : HoveredIndex < 0 ? "Önce çapa ile toprağı hazırla (5)" : Model.EquippedItem == FarmItem.Seeds ? "Sol tık · Tohum ek" : Model.EquippedItem == FarmItem.WateringCan ? "Sol tuşu basılı tut · Sula" : "Sol tık · Hasat et";

        public void Equip(FarmItem item)
        {
            if (!Ready || !Model.Equip(item)) return;
            SetBuildMode(false);
            Feedback = item == FarmItem.WateringCan ? "Sulama kabı: sol tuşu basılı tutarak fareyi tarlada gezdir."
                : EquippedName + " seçildi. Fareyi yakındaki kareye götür ve sol tıkla.";
            SaveGame(); Changed?.Invoke();
        }

        public bool UseHovered()
        {
            if (!Ready || BuildMode || !Application.isFocused) return false;
            selection.RefreshPointer();
            int index = HoveredIndex;
            if (!selection.WorldCell.HasValue) return false;
            if (!selection.HoveredInReach)
            { Feedback = "Bu kare uzakta. Biraz yaklaş."; return false; }
            var cell = selection.WorldCell.Value;
            if (Model.EquippedItem == FarmItem.Hoe)
            {
                if (!WorldGround.SupportsCell(cell.x,cell.y,true) || WorldGround.Obstructed(cell.x,cell.y,player))
                { Feedback = "Burada çapa kullanmak için boş toprak gerekiyor."; return false; }
                bool tilled = Complete(Model.Till(cell.x,cell.y,out var reason),reason);
                if (tilled) Hoed?.Invoke(new Vector3(cell.x+.5f,.06f,cell.y+.5f));
                return tilled;
            }
            if (index < 0) { Feedback = "Önce çapa (5) ile toprağı ekime hazırla."; return false; }
            if (Model.Building.Occupied(cell.x,0,cell.y) || WorldGround.Obstructed(cell.x,cell.y,player)) { Feedback = "Toprağın üzerinde bir yapı veya engel var."; return false; }
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

        public bool Gather(int id)
        {
            if(!Ready||BuildMode)return false;
            var node=Model.Exploration.Node(id);if(node==null)return false;
            var target=new Vector3(node.x+.5f,player.position.y,node.z+.5f);
            if((target-player.position).sqrMagnitude>6.25f||GetComponent<ExplorationController>()?.InReach(node)==false)return false;
            bool ok=Complete(Model.Gather(id,ActiveCrop.id,out var message),message);
            if(ok&&node.kind!=ResourceKind.Chest)ResourceUsed?.Invoke(target+Vector3.up*.6f);
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
            if (!NearCamp) { Feedback = "Uyumak için bir yatağa yaklaş."; return false; }
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
            BuildMode = false; selection.HideOutline = false;
            StopWatering();
            try
            {
                var loaded = store.Load(out bool recovered);
                if (loaded != null) Model = loaded;
                Ready = true;
                SaveStatus = recovered ? "Yedek kayıt yüklendi" : loaded == null ? "Yeni çiftlik" : "Kayıt yüklendi";
                Feedback = recovered ? "Son sağlam yedek açıldı; önceki dosya korunacak." : "5 ile çapa seç; boş toprağı hazırla. 1/2/3 ile ek, sula ve hasat et.";
                if(!recovered&&(loaded==null||JsonUtility.FromJson<FarmSnapshot>(File.ReadAllText(SavePath)).version<7))SaveGame();
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
