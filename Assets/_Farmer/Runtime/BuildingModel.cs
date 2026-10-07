using System;
using System.Collections.Generic;
using System.Linq;

namespace Farmer
{
    public sealed class BuildRules
    {
        public string Id { get; }
        public int WoodCost { get; }
        public bool IsBed { get; }
        public BuildRules(string id, int woodCost, bool isBed = false)
        {
            if (string.IsNullOrWhiteSpace(id) || woodCost < 1 || woodCost > BuildingModel.WoodLimit) throw new ArgumentException("Invalid build rules.");
            Id = id; WoodCost = woodCost; IsBed = isBed;
        }
        public static BuildRules[] Defaults => new[] { new BuildRules("wood_block", 2), new BuildRules("bed", 8, true) };
    }
    [Serializable] public sealed class BlockRecord
    {
        public string pieceId;
        public int x, level, z, rotation;
        public BlockRecord Copy() => new BlockRecord { pieceId = pieceId, x = x, level = level, z = z, rotation = rotation };
    }
    [Serializable] public sealed class BuildingSnapshot { public int wood; public BlockRecord[] blocks; }

    // Sparse world cells: no farm or construction-zone boundary.
    public sealed class BuildingModel
    {
        public const int Levels = 3, StarterWood = 24, WoodLimit = 999, BlockLimit = 10000;
        public const int WoodPackCount = 10, WoodPackPrice = 20;
        private readonly Dictionary<string, BuildRules> catalog;
        private readonly Dictionary<(int, int, int), BlockRecord> blocks = new Dictionary<(int, int, int), BlockRecord>();
        private readonly HashSet<(int,int,int)> occupied = new HashSet<(int,int,int)>();
        public int Revision { get; private set; }
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
        public static bool ValidCoordinate(int x, int z) => x >= -100000 && x <= 100000 && z >= -100000 && z <= 100000;
        public static bool InBounds(int x, int level, int z) => ValidCoordinate(x, z) && level >= 0 && level < Levels;
        public BuildRules Rules(string id) => catalog[id];
        public IEnumerable<(int x,int z)> Footprint(string id, int x, int z, int rotation)
        {
            yield return (x,z);
            if (catalog[id].IsBed)
                yield return (x + (rotation == 1 ? 1 : rotation == 3 ? -1 : 0), z + (rotation == 0 ? 1 : rotation == 2 ? -1 : 0));
        }
        public bool Occupied(int x, int level, int z) => occupied.Contains((x,level,z));
        private bool Supported(int x, int level, int z) => blocks.TryGetValue((x,level-1,z), out var b) && !catalog[b.pieceId].IsBed;
        public bool CanPlace(string id, int x, int level, int z, int rotation, out string message)
        {
            if (id == null || !catalog.TryGetValue(id, out var rule)) return Fail("Bu yapı parçası tanımlı değil.", out message);
            if (!InBounds(x, level, z)) return Fail("Geçersiz dünya koordinatı veya yükseklik.", out message);
            if (rotation < 0 || rotation > 3) return Fail("Geçersiz dönüş.", out message);
            if (blocks.Count >= BlockLimit) return Fail("Bu kayıt için yapı sınırına ulaşıldı.", out message);
            if (rule.IsBed && level != 0) return Fail("Yatağı zemine yerleştir.", out message);
            if (Footprint(id,x,z,rotation).Any(c => !ValidCoordinate(c.x,c.z))) return Fail("Geçersiz dünya karesi.", out message);
            if (Footprint(id,x,z,rotation).Any(c => Occupied(c.x,level,c.z))) return Fail("Bu hücre zaten dolu.", out message);
            if (level > 0 && !Supported(x, level, z)) return Fail("Önce altına bir blok yerleştir.", out message);
            if (Wood < rule.WoodCost) return Fail("Odunun yetmiyor. Pazardan alabilir veya bir bloğu sökebilirsin.", out message);
            message = "Sol tık · Yerleştir"; return true;
        }
        public bool Place(string id, int x, int level, int z, int rotation, out string message)
        {
            if (!CanPlace(id, x, level, z, rotation, out message)) return false;
            Wood -= catalog[id].WoodCost;
            blocks.Add((x, level, z), new BlockRecord { pieceId = id, x = x, level = level, z = z, rotation = rotation });
            foreach (var c in Footprint(id,x,z,rotation)) occupied.Add((c.x,level,c.z));
            Revision++;
            message = $"Yapı yerleştirildi. −{catalog[id].WoodCost} odun."; return true;
        }
        public bool Remove(int x, int level, int z, out string message)
        {
            if (!blocks.TryGetValue((x, level, z), out var block)) return Fail("Sökmek için bir bloğu hedefle.", out message);
            if (Occupied(x, level + 1, z)) return Fail("Önce üstündeki bloğu sök.", out message);
            int refund = catalog[block.pieceId].WoodCost;
            if (Wood + refund > WoodLimit) return Fail("Odun çantan dolu; önce biraz odun kullan.", out message);
            blocks.Remove((x, level, z)); Wood += refund; Revision++;
            foreach (var c in Footprint(block.pieceId,x,z,block.rotation)) occupied.Remove((c.x,level,c.z));
            message = $"Yapı söküldü. +{refund} odun geri alındı."; return true;
        }
        internal bool AddWood(int count)
        {
            if (count < 1 || count > WoodLimit - Wood) return false;
            Wood += count; return true;
        }
        public BuildingSnapshot Snapshot() => new BuildingSnapshot { wood = Wood, blocks = Blocks.OrderBy(b => b.level).ThenBy(b => b.z).ThenBy(b => b.x).ToArray() };
        public static BuildingModel Restore(BuildingSnapshot saved, IEnumerable<BuildRules> rules)
        {
            if (saved == null || saved.blocks == null || saved.blocks.Length > BlockLimit) throw new ArgumentException("Missing or oversized construction state.");
            var model = new BuildingModel(rules, saved.wood);
            if (saved.blocks.Any(b => b == null)) throw new ArgumentException("Null block.");
            foreach (var b in saved.blocks.OrderBy(b => b.level))
            {
                if (b.pieceId == null || !model.catalog.ContainsKey(b.pieceId) || !InBounds(b.x, b.level, b.z) || b.rotation < 0 || b.rotation > 3
                    || (model.catalog[b.pieceId].IsBed && b.level != 0)
                    || model.Footprint(b.pieceId,b.x,b.z,b.rotation).Any(c => !ValidCoordinate(c.x,c.z) || model.Occupied(c.x,b.level,c.z))
                    || (b.level > 0 && !model.Supported(b.x, b.level, b.z)))
                    throw new ArgumentException("Invalid, duplicate or unsupported block.");
                model.blocks.Add((b.x, b.level, b.z), b.Copy());
                foreach (var c in model.Footprint(b.pieceId,b.x,b.z,b.rotation)) model.occupied.Add((c.x,b.level,c.z));
            }
            return model;
        }
        private static bool Fail(string reason, out string message) { message = reason; return false; }
    }
}
