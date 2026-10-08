using System;
using System.Collections.Generic;
using System.Linq;

namespace Farmer
{
    public enum FarmItem { Seeds = 0, WateringCan = 1, Sickle = 2, Hoe = 3 }

    public sealed class CropRules
    {
        public string Id { get; }
        public int SeedPrice { get; }
        public int SalePrice { get; }
        public int WateredDays { get; }
        public int Yield { get; }
        public CropRules(string id, int seedPrice, int salePrice, int wateredDays, int yield)
        {
            if (string.IsNullOrWhiteSpace(id) || seedPrice < 1 || salePrice < 1 || seedPrice > 100000 || salePrice > 100000
                || wateredDays < 3 || wateredDays > 100 || yield < 1 || yield > FarmModel.StackLimit)
                throw new ArgumentException("Invalid crop rules.");
            Id = id; SeedPrice = seedPrice; SalePrice = salePrice; WateredDays = wateredDays; Yield = yield;
        }
    }

    [Serializable] public sealed class InventoryRecord { public string cropId; public int count; }
    [Serializable] public sealed class PlotRecord
    {
        public int x, z;
        public string cropId = "";
        public int growth;
        public bool watered;
        public PlotRecord Copy() => new PlotRecord { x = x, z = z, cropId = cropId, growth = growth, watered = watered };
    }
    [Serializable] public sealed class FarmSnapshot
    {
        public int version;
        public int width;
        public int depth;
        public int day;
        public int money;
        public double minuteOfDay;
        public FarmItem equippedItem;
        public InventoryRecord[] seeds;
        public InventoryRecord[] produce;
        public PlotRecord[] plots;
        public BuildingSnapshot building;
    }

    // No scene, input, filesystem or clock dependencies: all transactions validate before mutating.
    public sealed class FarmModel
    {
        public const int StackLimit = 999;
        public const int MoneyLimit = 1000000000;
        public const int DayLimit = 1000000;
        private readonly Dictionary<string, CropRules> crops;
        private readonly Dictionary<string, int> seeds = new Dictionary<string, int>();
        private readonly Dictionary<string, int> produce = new Dictionary<string, int>();
        private readonly List<PlotRecord> plots = new List<PlotRecord>();
        private readonly Dictionary<(int, int), int> plotIndices = new Dictionary<(int, int), int>();
        public const int PlotLimit = 10000;
        public double MinuteOfDay { get; private set; } = 360;
        public string ClockText => $"{(int)MinuteOfDay / 60:00}:{(int)MinuteOfDay % 60:00}";
        public int Width { get; }
        public int Depth { get; }
        public int Day { get; private set; } = 1;
        public int Money { get; private set; }
        public FarmItem EquippedItem { get; private set; }
        public BuildingModel Building { get; private set; }
        // The two reusable starter tools are permanent inventory items in the 0.1 prototype.
        public int ItemCount(FarmItem item, string cropId) => item == FarmItem.Seeds ? Seeds(cropId)
            : item == FarmItem.WateringCan || item == FarmItem.Sickle || item == FarmItem.Hoe ? 1 : 0;
        public bool Equip(FarmItem item)
        {
            if (item < FarmItem.Seeds || item > FarmItem.Hoe) return false;
            EquippedItem = item; return true;
        }
        public bool UseEquipped(int index, string seedId, out string message)
        {
            switch (EquippedItem)
            {
                case FarmItem.Seeds: return Plant(index, seedId, out message);
                case FarmItem.WateringCan: return Water(index, out message);
                case FarmItem.Sickle: return Harvest(index, out message);
                default: return Fail("Önce bir eşya kuşan.", out message);
            }
        }
        public int PlotCount => plots.Count;
        public int PlantedCount => plots.Count(p => !string.IsNullOrEmpty(p.cropId));
        public int ReadyCount => Enumerable.Range(0, plots.Count).Count(IsReady);

        public FarmModel(IEnumerable<CropRules> catalog, int width = 6, int depth = 6, int startingMoney = 60, IEnumerable<BuildRules> buildCatalog = null)
        {
            if (width < 1 || width > 100 || depth < 1 || depth > 100 || startingMoney < 0 || startingMoney > MoneyLimit)
                throw new ArgumentOutOfRangeException(nameof(width));
            crops = catalog.ToDictionary(c => c.Id);
            if (crops.Count == 0) throw new ArgumentException("A crop catalog is required.");
            Width = width; Depth = depth; Money = startingMoney;
            Building = new BuildingModel(buildCatalog ?? BuildRules.Defaults);
            foreach (string id in crops.Keys) { seeds[id] = 0; produce[id] = 0; }
        }

        public int Seeds(string id) => seeds.TryGetValue(id, out int count) ? count : 0;
        public int Produce(string id) => produce.TryGetValue(id, out int count) ? count : 0;
        public PlotRecord Plot(int index) => plots[index].Copy();
        public bool IsReady(int index) => Valid(index) && crops.TryGetValue(plots[index].cropId, out var crop) && plots[index].growth >= crop.WateredDays;
        public int Stage(int index) => string.IsNullOrEmpty(plots[index].cropId) ? -1
            : Math.Min(3, plots[index].growth * 3 / crops[plots[index].cropId].WateredDays);
        public int ThirstyCount => plots.Count(p => crops.TryGetValue(p.cropId, out var c) && p.growth < c.WateredDays && !p.watered);
        private bool Valid(int index) => index >= 0 && index < plots.Count;

        public bool BuySeeds(string id, int count, out string message)
        {
            if (!crops.TryGetValue(id, out var c) || count <= 0 || count > StackLimit) return Fail("Geçersiz alış miktarı.", out message);
            if (seeds[id] + count > StackLimit) return Fail("Tohum çantası dolu.", out message);
            long cost = (long)c.SeedPrice * count;
            if (Money < cost) return Fail("Yeterli paran yok.", out message);
            Money -= (int)cost; seeds[id] += count;
            message = $"{count} tohum alındı. −{cost} para."; return true;
        }

        public bool BuyWood(out string message)
        {
            if (Money < BuildingModel.WoodPackPrice) return Fail("Odun almak için yeterli paran yok.", out message);
            if (!Building.AddWood(BuildingModel.WoodPackCount)) return Fail("Odun çantası dolu.", out message);
            Money -= BuildingModel.WoodPackPrice;
            message = $"{BuildingModel.WoodPackCount} odun alındı. −{BuildingModel.WoodPackPrice} para."; return true;
        }

        public bool Plant(int index, string id, out string message)
        {
            if (!Valid(index) || !crops.ContainsKey(id)) return Fail("Geçersiz tarla karesi.", out message);
            if (Building.Occupied(plots[index].x,0,plots[index].z)) return Fail("Yapının altında ekim yapamazsın.", out message);
            if (!string.IsNullOrEmpty(plots[index].cropId)) return Fail("Bu kare zaten ekili.", out message);
            if (seeds[id] < 1) return Fail("Tohumun yok. Pazardan tohum al.", out message);
            seeds[id]--; plots[index].cropId = id; plots[index].growth = 0; plots[index].watered = false;
            message = "Tohum ekildi. Büyümeyi başlatmak için bir kez sula."; return true;
        }

        public bool Water(int index, out string message)
        {
            if (!Valid(index) || string.IsNullOrEmpty(plots[index].cropId)) return Fail("Önce tohum ek.", out message);
            if (IsReady(index)) return Fail("Bu ürün hasada hazır.", out message);
            if (plots[index].watered) return Fail("Bu bitki zaten sulandı; tekrar sulamana gerek yok.", out message);
            plots[index].watered = true;
            message = "Sulandı. Hasada kadar yeniden sulama gerekmiyor."; return true;
        }

        public bool Harvest(int index, out string message)
        {
            if (!IsReady(index)) return Fail("Ürün henüz hasada hazır değil.", out message);
            var c = crops[plots[index].cropId];
            if (produce[c.Id] + c.Yield > StackLimit) return Fail("Ürün çantası dolu. Önce pazarda satış yap.", out message);
            produce[c.Id] += c.Yield; plots[index].cropId = ""; plots[index].growth = 0; plots[index].watered = false;
            message = $"Hasat tamamlandı! +{c.Yield} ürün. Pazarda satabilirsin."; return true;
        }

        public bool Sell(string id, int count, out string message)
        {
            if (!crops.TryGetValue(id, out var c) || count <= 0 || count > Produce(id)) return Fail("Satılacak ürünün yok.", out message);
            long income = (long)c.SalePrice * count;
            if (Money + income > MoneyLimit) return Fail("Para sınırına ulaştın.", out message);
            produce[id] -= count; Money += (int)income;
            message = $"{count} ürün satıldı. +{income} para!"; return true;
        }

        public int IndexAt(int x, int z) => plotIndices.TryGetValue((x, z), out int index) ? index : -1;
        public bool Till(int x, int z, out string message)
        {
            if (!BuildingModel.ValidCoordinate(x, z)) return Fail("Geçersiz dünya karesi.", out message);
            if (IndexAt(x, z) >= 0) return Fail("Bu toprak zaten ekime hazır.", out message);
            if (Building.Occupied(x, 0, z)) return Fail("Yapının altında tarla açamazsın.", out message);
            if (plots.Count >= PlotLimit) return Fail("Bu kayıt için tarla sınırına ulaşıldı.", out message);
            plotIndices.Add((x,z), plots.Count); plots.Add(new PlotRecord { x = x, z = z });
            message = "Toprak çapalandı. Şimdi tohum ekebilirsin."; return true;
        }
        // Returns the number of crossed midnights. Sleeping uses this exact same clock path.
        public int AdvanceMinutes(double minutes)
        {
            if (double.IsNaN(minutes) || double.IsInfinity(minutes) || minutes < 0) throw new ArgumentOutOfRangeException(nameof(minutes));
            double available = (DayLimit - Day) * 1440.0 + 1439.999 - MinuteOfDay;
            double total = MinuteOfDay + Math.Min(minutes, Math.Max(0, available));
            int nights = (int)Math.Floor(total / 1440);
            MinuteOfDay = total % 1440; Day += nights;
            if (nights > 0)
                foreach (var plot in plots)
                    if (plot.watered && crops.TryGetValue(plot.cropId, out var crop))
                        plot.growth = Math.Min(crop.WateredDays, plot.growth + nights);
            return nights;
        }
        public bool EndDay(out string message)
        {
            double minutes = MinuteOfDay < 360 ? 360 - MinuteOfDay : 1800 - MinuteOfDay;
            if (Day == DayLimit && MinuteOfDay >= 360) return Fail("Gün sınırına ulaşıldı.", out message);
            AdvanceMinutes(minutes);
            message = $"Gün {Day} · 06:00. Dinlendin; yeni sabah başladı."; return true;
        }

        public FarmSnapshot Snapshot() => new FarmSnapshot
        {
            version = 4, width = Width, depth = Depth, day = Day, money = Money, minuteOfDay = MinuteOfDay, equippedItem = EquippedItem,
            seeds = seeds.Select(p => new InventoryRecord { cropId = p.Key, count = p.Value }).ToArray(),
            produce = produce.Select(p => new InventoryRecord { cropId = p.Key, count = p.Value }).ToArray(),
            plots = plots.Select(p => p.Copy()).ToArray(), building = Building.Snapshot()
        };

        public static FarmModel Restore(FarmSnapshot saved, IEnumerable<CropRules> catalog, int width, int depth, IEnumerable<BuildRules> buildCatalog = null)
        {
            if (saved == null || (saved.version < 1 || saved.version > 4) || saved.width != width || saved.depth != depth || saved.day < 1
                || saved.day > DayLimit || saved.money < 0 || saved.money > MoneyLimit || saved.plots == null || (saved.version < 3 ? saved.plots.Length != width * depth : saved.plots.Length > PlotLimit))
                throw new ArgumentException("Save header or grid is invalid or unsupported.");
            var rules = (buildCatalog ?? BuildRules.Defaults).ToArray();
            var model = new FarmModel(catalog, width, depth, saved.money, rules) { Day = saved.day };
            if (saved.version >= 2)
            {
                var building = saved.building;
                if (saved.version == 2 && building != null && building.blocks != null)
                    building = new BuildingSnapshot { wood = building.wood, blocks = building.blocks.Select(b =>
                    {
                        if (b == null || b.x < 0 || b.x >= 6 || b.z < 0 || b.z >= 5) throw new ArgumentException("Invalid legacy block.");
                        var copy = b.Copy(); copy.x += 3; copy.z -= 7; return copy;
                    }).ToArray() };
                model.Building = BuildingModel.Restore(building, rules);
            }
            if (saved.version >= 3)
            {
                if (double.IsNaN(saved.minuteOfDay) || double.IsInfinity(saved.minuteOfDay) || saved.minuteOfDay < 0 || saved.minuteOfDay >= 1440)
                    throw new ArgumentException("Invalid clock.");
                model.MinuteOfDay = saved.minuteOfDay;
            }
            // V1 has no construction state. JsonUtility may materialize an empty nested object;
            // migrate by schema version, not by the nullness of that object.
            if (!model.Equip(saved.equippedItem)) throw new ArgumentException("Unknown equipped item.");
            model.RestoreInventory(saved.seeds, model.seeds);
            model.RestoreInventory(saved.produce, model.produce);
            for (int i = 0; i < saved.plots.Length; i++)
            {
                var p = saved.plots[i];
                if (p == null || p.growth < 0) throw new ArgumentException("Invalid plot.");
                var copy = p.Copy();
                if (saved.version < 3) { copy.x = i % width - 3; copy.z = i / width - 3; }
                if (!BuildingModel.ValidCoordinate(copy.x, copy.z) || model.IndexAt(copy.x, copy.z) >= 0)
                    throw new ArgumentException("Invalid or duplicate soil coordinate.");
                if (string.IsNullOrEmpty(copy.cropId))
                {
                    if (copy.growth != 0 || copy.watered) throw new ArgumentException("Empty plot has crop state.");
                    copy.cropId = "";
                }
                else
                {
                    if (!model.crops.TryGetValue(copy.cropId, out var crop) || copy.growth > crop.WateredDays || model.Building.Occupied(copy.x, 0, copy.z))
                        throw new ArgumentException("Unknown crop, invalid growth or crop inside a building.");
                    copy.watered = copy.watered || copy.growth > 0;
                }
                model.plotIndices.Add((copy.x,copy.z), model.plots.Count); model.plots.Add(copy);
            }
            return model;
        }

        private void RestoreInventory(InventoryRecord[] records, Dictionary<string, int> inventory)
        {
            if (records == null) throw new ArgumentException("Missing inventory.");
            var seen = new HashSet<string>();
            foreach (var p in records)
            {
                if (p == null || p.cropId == null || !crops.ContainsKey(p.cropId) || !seen.Add(p.cropId) || p.count < 0 || p.count > StackLimit)
                    throw new ArgumentException("Invalid inventory entry.");
                inventory[p.cropId] = p.count;
            }
        }
        private static bool Fail(string reason, out string message) { message = reason; return false; }
    }
}
