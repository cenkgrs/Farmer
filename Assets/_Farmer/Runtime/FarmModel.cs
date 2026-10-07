using System;
using System.Collections.Generic;
using System.Linq;

namespace Farmer
{
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
        public string cropId = "";
        public int growth;
        public bool watered;
        public PlotRecord Copy() => new PlotRecord { cropId = cropId, growth = growth, watered = watered };
    }
    [Serializable] public sealed class FarmSnapshot
    {
        public int version;
        public int width;
        public int depth;
        public int day;
        public int money;
        public InventoryRecord[] seeds;
        public InventoryRecord[] produce;
        public PlotRecord[] plots;
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
        private readonly PlotRecord[] plots;
        public int Width { get; }
        public int Depth { get; }
        public int Day { get; private set; } = 1;
        public int Money { get; private set; }
        public int PlotCount => plots.Length;
        public int PlantedCount => plots.Count(p => !string.IsNullOrEmpty(p.cropId));
        public int ReadyCount => Enumerable.Range(0, plots.Length).Count(IsReady);

        public FarmModel(IEnumerable<CropRules> catalog, int width = 6, int depth = 6, int startingMoney = 60)
        {
            if (width < 1 || width > 100 || depth < 1 || depth > 100 || startingMoney < 0 || startingMoney > MoneyLimit)
                throw new ArgumentOutOfRangeException(nameof(width));
            crops = catalog.ToDictionary(c => c.Id);
            if (crops.Count == 0) throw new ArgumentException("A crop catalog is required.");
            Width = width; Depth = depth; Money = startingMoney;
            plots = Enumerable.Range(0, width * depth).Select(_ => new PlotRecord()).ToArray();
            foreach (string id in crops.Keys) { seeds[id] = 0; produce[id] = 0; }
        }

        public int Seeds(string id) => seeds.TryGetValue(id, out int count) ? count : 0;
        public int Produce(string id) => produce.TryGetValue(id, out int count) ? count : 0;
        public PlotRecord Plot(int index) => plots[index].Copy();
        public bool IsReady(int index) => Valid(index) && crops.TryGetValue(plots[index].cropId, out var crop) && plots[index].growth >= crop.WateredDays;
        public int Stage(int index) => string.IsNullOrEmpty(plots[index].cropId) ? -1
            : Math.Min(3, plots[index].growth * 3 / crops[plots[index].cropId].WateredDays);
        public int ThirstyCount => plots.Count(p => crops.TryGetValue(p.cropId, out var c) && p.growth < c.WateredDays && !p.watered);
        private bool Valid(int index) => index >= 0 && index < plots.Length;

        public bool BuySeeds(string id, int count, out string message)
        {
            if (!crops.TryGetValue(id, out var c) || count <= 0 || count > StackLimit) return Fail("Geçersiz alış miktarı.", out message);
            if (seeds[id] + count > StackLimit) return Fail("Tohum çantası dolu.", out message);
            long cost = (long)c.SeedPrice * count;
            if (Money < cost) return Fail("Yeterli paran yok.", out message);
            Money -= (int)cost; seeds[id] += count;
            message = $"{count} tohum alındı. −{cost} para."; return true;
        }

        public bool Plant(int index, string id, out string message)
        {
            if (!Valid(index) || !crops.ContainsKey(id)) return Fail("Geçersiz tarla karesi.", out message);
            if (!string.IsNullOrEmpty(plots[index].cropId)) return Fail("Bu kare zaten ekili.", out message);
            if (seeds[id] < 1) return Fail("Tohumun yok. Pazardan tohum al.", out message);
            seeds[id]--; plots[index] = new PlotRecord { cropId = id };
            message = "Tohum ekildi. Büyümesi için bugün sula."; return true;
        }

        public bool Water(int index, out string message)
        {
            if (!Valid(index) || string.IsNullOrEmpty(plots[index].cropId)) return Fail("Önce tohum ek.", out message);
            if (IsReady(index)) return Fail("Bu ürün hasada hazır.", out message);
            if (plots[index].watered) return Fail("Bu kare bugün zaten sulandı.", out message);
            plots[index].watered = true;
            message = "Sulandı. Gece boyunca büyüyecek."; return true;
        }

        public bool Harvest(int index, out string message)
        {
            if (!IsReady(index)) return Fail("Ürün henüz hasada hazır değil.", out message);
            var c = crops[plots[index].cropId];
            if (produce[c.Id] + c.Yield > StackLimit) return Fail("Ürün çantası dolu. Önce pazarda satış yap.", out message);
            produce[c.Id] += c.Yield; plots[index] = new PlotRecord();
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

        public bool EndDay(out string message)
        {
            if (Day >= DayLimit) return Fail("Gün sınırına ulaşıldı.", out message);
            int grown = 0;
            foreach (var plot in plots)
            {
                if (plot.watered && crops.TryGetValue(plot.cropId, out var c) && plot.growth < c.WateredDays)
                { plot.growth++; grown++; }
                plot.watered = false;
            }
            Day++; message = $"Gün {Day}. {grown} bitki büyüdü. Yeni günün sulamasını unutma."; return true;
        }

        public FarmSnapshot Snapshot() => new FarmSnapshot
        {
            version = 1, width = Width, depth = Depth, day = Day, money = Money,
            seeds = seeds.Select(p => new InventoryRecord { cropId = p.Key, count = p.Value }).ToArray(),
            produce = produce.Select(p => new InventoryRecord { cropId = p.Key, count = p.Value }).ToArray(),
            plots = plots.Select(p => p.Copy()).ToArray()
        };

        public static FarmModel Restore(FarmSnapshot saved, IEnumerable<CropRules> catalog, int width, int depth)
        {
            if (saved == null || saved.version != 1 || saved.width != width || saved.depth != depth || saved.day < 1
                || saved.day > DayLimit || saved.money < 0 || saved.money > MoneyLimit || saved.plots == null || saved.plots.Length != width * depth)
                throw new ArgumentException("Save header or grid is invalid or unsupported.");
            var model = new FarmModel(catalog, width, depth, saved.money) { Day = saved.day };
            model.RestoreInventory(saved.seeds, model.seeds);
            model.RestoreInventory(saved.produce, model.produce);
            for (int i = 0; i < saved.plots.Length; i++)
            {
                var p = saved.plots[i];
                if (p == null || p.growth < 0) throw new ArgumentException("Invalid plot.");
                if (string.IsNullOrEmpty(p.cropId))
                {
                    if (p.growth != 0 || p.watered) throw new ArgumentException("Empty plot has crop state.");
                    model.plots[i] = new PlotRecord();
                }
                else
                {
                    if (!model.crops.TryGetValue(p.cropId, out var crop) || p.growth > crop.WateredDays)
                        throw new ArgumentException("Unknown crop or invalid growth.");
                    model.plots[i] = p.Copy();
                }
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
