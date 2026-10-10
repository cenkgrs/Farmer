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
        public bool MenuOpen { get; set; }
        private MarketInteraction marketInteraction;
        public bool MarketOpen => marketInteraction != null && marketInteraction.IsOpen;
        public bool MarketInputConsumed => marketInteraction != null && marketInteraction.ConsumedThisFrame;
        public bool WorldInputBlocked => MenuOpen || InventoryOpen || MarketOpen || MarketInputConsumed;
        public bool CanTrade => Ready && MarketOpen && NearMarket && !MenuOpen && !InventoryOpen;
        public bool HadSaveAtStartup { get; private set; }
        [SerializeField] private FarmSelection selection;
        [SerializeField] private Transform player;
        [SerializeField] private Transform market;
        [SerializeField] private Transform camp;
        [SerializeField] private int startingMoney = 50;
        public FarmModel Model { get; private set; }
        public FarmSelection Selection => selection;
        public CropDefinition ActiveCrop => Model == null ? crops[0] : Definition(Model.SelectedCropId);
        public CropDefinition[] Crops => crops;
        public RecipeDefinition[] Recipes { get; private set; } = Array.Empty<RecipeDefinition>();
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
        private Bounds[] sceneryBounds = Array.Empty<Bounds>();
        public string SavePath { get; private set; }

        public void Configure(CropDefinition[] definitions, FarmSelection grid, Transform actor, Transform shop, Transform rest)
        { crops = definitions; selection = grid; player = actor; market = shop; camp = rest; }

        private void Awake()
        {
            Application.targetFrameRate=60;
            if(GetComponent<StorageInteraction>()==null)gameObject.AddComponent<StorageInteraction>();
            if(GetComponent<MarketHud>()==null)gameObject.AddComponent<MarketHud>();
            marketInteraction=GetComponent<MarketInteraction>();
            if(marketInteraction==null)marketInteraction=gameObject.AddComponent<MarketInteraction>();
            // Capture authored scenery once; never include runtime resource/building views.
            sceneryBounds=FindObjectsByType<ValleyScenery>(FindObjectsSortMode.None)
                .SelectMany(v=>v.GetComponentsInChildren<Renderer>()).Select(r=>r.bounds).ToArray();
            // Additional definitions extend the authored scene catalog without rewriting the user's scene.
            crops = crops.Concat(Resources.LoadAll<CropDefinition>("Crops").OrderBy(c=>c.id,StringComparer.Ordinal))
                .GroupBy(c=>c.id).Select(g=>g.First()).ToArray();
            Recipes = Resources.LoadAll<RecipeDefinition>("Recipes").OrderBy(r=>r.id,StringComparer.Ordinal).ToArray();
            var recipeRules = Recipes.Select(r=>r.Rules).ToArray();
            var rules = crops.Select(c => c.Rules).ToArray();
            Model = new FarmModel(rules, selection.Layout.Width, selection.Layout.Depth, startingMoney, BuildingRules(), resourceBlocked: ResourceBlockedByScenery, recipeCatalog: recipeRules);
            string directory = Application.persistentDataPath;
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            smokeSession = Array.IndexOf(Environment.GetCommandLineArgs(), "--farmer-smoke-capture") >= 0;
            if (smokeSession) directory = Path.Combine(Application.temporaryCachePath, "FarmerQA", Guid.NewGuid().ToString("N"));
#endif
            SavePath = Path.Combine(directory, "farm-v1.json");
            store = new FarmSaveStore(SavePath, snapshot => FarmModel.Restore(snapshot, rules, selection.Layout.Width, selection.Layout.Depth, BuildingRules(), ResourceBlockedByScenery, recipeRules));
            if(camp!=null)camp.gameObject.SetActive(false);
            HadSaveAtStartup=File.Exists(SavePath)||File.Exists(SavePath+".bak");
            LoadGame();
            gameObject.AddComponent<SessionMenu>();
        }
        private bool ResourceBlockedByScenery(int x,int z)
        {
            return sceneryBounds.Any(b=>x+.5f>b.min.x-1.6f&&x+.5f<b.max.x+1.6f
                &&z+.5f>b.min.z-1.6f&&z+.5f<b.max.z+1.6f);
        }
        private void Update()
        {
            var k = Keyboard.current;
            if (MenuOpen || !Application.isFocused) { StopWatering(); return; }
            if (k?.f9Key.wasPressedThisFrame == true) LoadGame();
            if (!Ready || InventoryOpen || MarketInputConsumed) { StopWatering(); return; }
            if(MarketOpen)
            {
                StopWatering();
                if(k?.bKey.wasPressedThisFrame==true)GetComponent<MarketHud>().PurchaseSelected();
                if(k?.vKey.wasPressedThisFrame==true)GetComponent<MarketHud>().SellSelected();
                if(k?.f5Key.wasPressedThisFrame==true)SaveGame();
                return;
            }
            if (k?.digit1Key.wasPressedThisFrame == true) Equip(FarmItem.Seeds);
            if (k?.digit2Key.wasPressedThisFrame == true) Equip(FarmItem.WateringCan);
            if (k?.digit3Key.wasPressedThisFrame == true) Equip(FarmItem.Sickle);
            if (k?.digit5Key.wasPressedThisFrame == true) Equip(FarmItem.Hoe);
            if (k?.digit6Key.wasPressedThisFrame == true) Equip(FarmItem.Axe);
            if (k?.digit7Key.wasPressedThisFrame == true) Equip(FarmItem.Pickaxe);
            if (BuildMode) StopWatering(); else
            {
                UpdateToolInput();
                if (Model.EquippedItem == FarmItem.Hoe && Mouse.current?.rightButton.wasPressedThisFrame == true) UprootHovered();
            }
            if (k?.nKey.wasPressedThisFrame == true) Rest();
            if (k?.f5Key.wasPressedThisFrame == true) SaveGame();
        }

        private BuildRules[] BuildingRules() => buildPieces != null && buildPieces.Length > 0 ? buildPieces.Select(b => b.Rules).ToArray() : BuildRules.Defaults;
        public void SetBuildMode(bool active)
        {
            if (active && (!Ready || WorldInputBlocked)) return;
            BuildMode = active; selection.HideOutline = active; StopWatering(); Changed?.Invoke();
        }
        public bool BuyWood()
        {
            if (!CanTrade) return false;
            return Complete(Model.BuyWood(out var message), message);
        }
        public bool BuyPickaxe()
        {
            if(!CanTrade)return false;
            return Complete(Model.BuyPickaxe(out var message),message);
        }
        public bool BuyBed()
        {
            if(!CanTrade)return false;
            return Complete(Model.BuyBed(out var message),message);
        }
        public bool BuyFurniture(string id)
        {
            if(!CanTrade)return false;
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
            : Model.EquippedItem == FarmItem.WateringCan ? "Sulama kabı" : Model.EquippedItem == FarmItem.Hoe ? "Çapa" : Model.EquippedItem==FarmItem.Axe?"Balta":Model.EquippedItem==FarmItem.Pickaxe?"Kazma":"Orak";
        public string ActionLabel => !selection.WorldCell.HasValue ? "Fareyi toprağa götür" : !selection.HoveredInReach ? "Kareye yaklaş"
            : Model.EquippedItem == FarmItem.Pickaxe?"Kayayı hedefle · Sol tıkla kır": Model.EquippedItem == FarmItem.Axe?"Ağacı hedefle · Sol tıkla kes": Model.EquippedItem == FarmItem.Hoe ? "Sol tık · Toprağı çapala" : HoveredIndex < 0 ? "Önce çapa ile toprağı hazırla (5)" : Model.EquippedItem == FarmItem.Seeds ? "Sol tık · Tohum ek" : Model.EquippedItem == FarmItem.WateringCan ? "Sol tuşu basılı tut · Sula" : "Sol tık · Hasat et";

        public void Equip(FarmItem item)
        {
            if (!Ready || WorldInputBlocked || !Model.Equip(item)) return;
            SetBuildMode(false);
            Feedback = item == FarmItem.WateringCan ? "Sulama kabı: sol tuşu basılı tutarak fareyi tarlada gezdir."
                : EquippedName + " seçildi. Fareyi yakındaki kareye götür ve sol tıkla.";
            SaveGame(); Changed?.Invoke();
        }

        public bool UprootHovered()
        {
            if(!Ready||WorldInputBlocked||BuildMode||!Application.isFocused)return false;
            selection.RefreshPointer();int index=HoveredIndex;
            if(index<0||!selection.HoveredInReach||selection.PointerBlocked)return false;
            var plot=Model.Plot(index);
            if(WorldGround.Obstructed(plot.x,plot.z,player))return false;
            bool ok=Complete(Model.Uproot(index,out var message),message);
            if(ok)Hoed?.Invoke(PlotCenter(index));return ok;
        }
        public bool UseHovered()
        {
            if (!Ready || WorldInputBlocked || BuildMode || !Application.isFocused) return false;
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
            if (index < 0) return false;
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
            if(!Ready||WorldInputBlocked||BuildMode)return false;
            var node=Model.Exploration.Node(id);if(node==null)return false;
            var target=new Vector3(node.x+.5f,player.position.y,node.z+.5f);
            if((target-player.position).sqrMagnitude>6.25f||GetComponent<ExplorationController>()?.InReach(node)==false)return false;
            bool ok=Complete(Model.Gather(id,crops[0].id,out var message),message);
            if(ok&&node.kind!=ResourceKind.Chest)ResourceUsed?.Invoke(target+Vector3.up*.6f);
            return ok;
        }
        public bool SelectCrop(string id)
        {
            if(!Ready||MenuOpen||!Model.SelectCrop(id))return false;
            StopWatering();SaveGame();Changed?.Invoke();return true;
        }
        public bool Craft(string id,int batches=1)
        {
            if(!CanTrade)return false;
            return Complete(Model.Craft(id,batches,out var message),message);
        }
        public bool SellCrafted(string id)
        {
            if(!CanTrade)return false;
            return Complete(Model.SellCrafted(id,Model.BagCount("crafted:"+id),out var message),message);
        }
        public bool Buy(int count)
        {
            if (!CanTrade) return false;
            bool ok = Model.BuySeeds(ActiveCrop.id, count, out var message); return Complete(ok, message);
        }
        public bool SellHarvest()=>SellHarvest(ActiveCrop.id);
        public bool SellHarvest(string cropId)
        {
            if (!CanTrade) return false;
            bool ok = Model.Sell(cropId, Model.Produce(cropId), out var message); return Complete(ok, message);
        }
        public bool Rest()
        {
            if (!Ready || WorldInputBlocked) return false;
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
            marketInteraction?.Close();
            BuildMode = false; selection.HideOutline = false;
            StopWatering();
            try
            {
                var loaded = store.Load(out bool recovered);
                if (loaded != null) Model = loaded;
                Ready = true;
                SaveStatus = recovered ? "Yedek kayıt yüklendi" : loaded == null ? "Yeni çiftlik" : "Kayıt yüklendi";
                Feedback = recovered ? "Son sağlam yedek açıldı; önceki dosya korunacak." : "5 ile çapa seç; boş toprağı hazırla. 1/2/3 ile ek, sula ve hasat et.";
                if(!recovered&&(loaded==null||JsonUtility.FromJson<FarmSnapshot>(File.ReadAllText(SavePath)).version<FarmModel.SaveVersion))SaveGame();
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
