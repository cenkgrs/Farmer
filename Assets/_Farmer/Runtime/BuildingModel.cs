using System;
using System.Collections.Generic;
using System.Linq;

namespace Farmer
{
    public sealed class BuildRules
    {
        public string Id { get; }
        public int WoodCost { get; }
        public BuildRules(string id, int woodCost)
        {
            if (string.IsNullOrWhiteSpace(id) || woodCost < 1 || woodCost > BuildingModel.WoodLimit) throw new ArgumentException("Invalid build rules.");
            Id = id; WoodCost = woodCost;
        }
        public static BuildRules[] Defaults => new[] { new BuildRules("wood_block", 2) };
    }
    [Serializable] public sealed class BlockRecord
    {
        public string pieceId;
        public int x, level, z, rotation;
        public BlockRecord Copy() => new BlockRecord { pieceId = pieceId, x = x, level = level, z = z, rotation = rotation };
    }
    [Serializable] public sealed class BuildingSnapshot { public int wood; public BlockRecord[] blocks; }

    // Small v0.2 construction lot: rules and persistence have no scene/input dependencies.
    public sealed class BuildingModel
    {
        public const int Width = 6, Depth = 5, Levels = 3, StarterWood = 24, WoodLimit = 999;
        public const int OriginX = 3, OriginZ = -7, WoodPackCount = 10, WoodPackPrice = 20;
        private readonly Dictionary<string, BuildRules> catalog;
        private readonly Dictionary<(int, int, int), BlockRecord> blocks = new Dictionary<(int, int, int), BlockRecord>();
        public int Wood { get; private set; }
        public int Count => blocks.Count;
        public IEnumerable<BlockRecord> Blocks => blocks.Values.Select(b => b.Copy());
        public BuildingModel(IEnumerable<BuildRules> rules, int wood = StarterWood)
        {
            if (wood < 0 || wood > WoodLimit) throw new ArgumentException("Invalid wood inventory.");
            catalog = rules.ToDictionary(r => r.Id);
            if (catalog.Count == 0) throw new ArgumentException("Build catalog is empty.");
            Wood = wood;
        }
        public static bool InBounds(int x, int level, int z) => x >= 0 && x < Width && z >= 0 && z < Depth && level >= 0 && level < Levels;
        public bool Occupied(int x, int level, int z) => blocks.ContainsKey((x, level, z));
        public bool CanPlace(string id, int x, int level, int z, int rotation, out string message)
        {
            if (id == null || !catalog.TryGetValue(id, out var rule)) return Fail("Bu yapı parçası tanımlı değil.", out message);
            if (!InBounds(x, level, z)) return Fail("İşaretli inşa alanının içine yerleştir.", out message);
            if (rotation < 0 || rotation > 3) return Fail("Geçersiz dönüş.", out message);
            if (Occupied(x, level, z)) return Fail("Bu hücre zaten dolu.", out message);
            if (level > 0 && !Occupied(x, level - 1, z)) return Fail("Önce altına bir blok yerleştir.", out message);
            if (Wood < rule.WoodCost) return Fail("Odunun yetmiyor. Pazardan alabilir veya bir bloğu sökebilirsin.", out message);
            message = "Sol tık · Yerleştir"; return true;
        }
        public bool Place(string id, int x, int level, int z, int rotation, out string message)
        {
            if (!CanPlace(id, x, level, z, rotation, out message)) return false;
            Wood -= catalog[id].WoodCost;
            blocks.Add((x, level, z), new BlockRecord { pieceId = id, x = x, level = level, z = z, rotation = rotation });
            message = $"Blok yerleştirildi. −{catalog[id].WoodCost} odun."; return true;
        }
        public bool Remove(int x, int level, int z, out string message)
        {
            if (!blocks.TryGetValue((x, level, z), out var block)) return Fail("Sökmek için bir bloğu hedefle.", out message);
            if (Occupied(x, level + 1, z)) return Fail("Önce üstündeki bloğu sök.", out message);
            int refund = catalog[block.pieceId].WoodCost;
            if (Wood + refund > WoodLimit) return Fail("Odun çantan dolu; önce biraz odun kullan.", out message);
            blocks.Remove((x, level, z)); Wood += refund;
            message = $"Blok söküldü. +{refund} odun geri alındı."; return true;
        }
        internal bool AddWood(int count)
        {
            if (count < 1 || count > WoodLimit - Wood) return false;
            Wood += count; return true;
        }
        public BuildingSnapshot Snapshot() => new BuildingSnapshot { wood = Wood, blocks = Blocks.OrderBy(b => b.level).ThenBy(b => b.z).ThenBy(b => b.x).ToArray() };
        public static BuildingModel Restore(BuildingSnapshot saved, IEnumerable<BuildRules> rules)
        {
            if (saved == null || saved.blocks == null || saved.blocks.Length > Width * Depth * Levels) throw new ArgumentException("Missing or oversized construction state.");
            var model = new BuildingModel(rules, saved.wood);
            if (saved.blocks.Any(b => b == null)) throw new ArgumentException("Null block.");
            foreach (var b in saved.blocks.OrderBy(b => b.level))
            {
                if (b.pieceId == null || !model.catalog.ContainsKey(b.pieceId) || !InBounds(b.x, b.level, b.z) || b.rotation < 0 || b.rotation > 3
                    || model.Occupied(b.x, b.level, b.z) || (b.level > 0 && !model.Occupied(b.x, b.level - 1, b.z)))
                    throw new ArgumentException("Invalid, duplicate or unsupported block.");
                model.blocks.Add((b.x, b.level, b.z), b.Copy());
            }
            return model;
        }
        private static bool Fail(string reason, out string message) { message = reason; return false; }
    }
}
