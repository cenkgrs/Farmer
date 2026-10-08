using System;
using System.Collections.Generic;
using System.Linq;

namespace Farmer
{
    public enum BuildPlacement { Solid, Floor, Edge }

    public sealed class BuildRules
    {
        public string Id { get; }
        public int WoodCost { get; }
        public bool IsBed { get; }
        public BuildPlacement Placement { get; }
        public bool IsDoor { get; }
        public BuildRules(string id, int woodCost, bool isBed = false, BuildPlacement placement = BuildPlacement.Solid, bool isDoor = false)
        {
            if (string.IsNullOrWhiteSpace(id) || woodCost < 1 || woodCost > BuildingModel.WoodLimit) throw new ArgumentException("Invalid build rules.");
            if (!Enum.IsDefined(typeof(BuildPlacement), placement) || (isBed && placement != BuildPlacement.Solid) || (isDoor && placement != BuildPlacement.Edge)) throw new ArgumentException("Invalid placement kind.");
            Id = id; WoodCost = woodCost; IsBed = isBed; Placement = placement; IsDoor = isDoor;
        }
        public static BuildRules[] Defaults => new[] { new BuildRules("wood_block", 2), new BuildRules("bed", 8, true), new BuildRules("wood_floor", 1, placement: BuildPlacement.Floor), new BuildRules("wood_wall", 2, placement: BuildPlacement.Edge), new BuildRules("wood_door", 3, placement: BuildPlacement.Edge, isDoor: true) };
    }
    [Serializable] public sealed class BlockRecord
    {
        public string pieceId;
        public int x, level, z, rotation;
        public bool doorOpen;
        public BlockRecord Copy() => new BlockRecord { pieceId = pieceId, x = x, level = level, z = z, rotation = rotation, doorOpen = doorOpen };
    }
    [Serializable] public sealed class BuildingSnapshot { public int wood; public BlockRecord[] blocks; }

    // Sparse world cells: no farm or construction-zone boundary.
    public sealed class BuildingModel
    {
        public const int Levels = 3, StarterWood = 24, WoodLimit = 999, BlockLimit = 10000;
        public const int WoodPackCount = 10, WoodPackPrice = 20;
        private readonly Dictionary<string, BuildRules> catalog;
        private readonly Dictionary<(int x, int level, int z, int layer, int axis), BlockRecord> blocks = new Dictionary<(int, int, int, int, int), BlockRecord>();
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
        // Canonical shared edge: opposite sides of neighbouring cells address the same wall.
        public static (int x,int z,int axis) Edge(int x,int z,int rotation) => rotation == 0 ? (x,z+1,0) : rotation == 1 ? (x+1,z,1) : rotation == 2 ? (x,z,0) : (x,z,1);
        private (int,int,int,int,int) Key(string id,int x,int level,int z,int rotation)
        {
            var placement=catalog[id].Placement;
            if(placement==BuildPlacement.Edge) { var e=Edge(x,z,rotation);return(e.x,level,e.z,2,e.axis); }
            return(x,level,z,(int)placement,0);
        }
        public bool HasFloor(int x,int z) => blocks.ContainsKey((x,0,z,1,0));
        public bool Occupied(int x,int level,int z) => occupied.Contains((x,level,z)) || (level==0 && HasFloor(x,z));
        private bool Supported(int x,int level,int z) => blocks.TryGetValue((x,level-1,z,0,0),out var b) && !catalog[b.pieceId].IsBed;
        private bool SolidBlock(int x,int z) => blocks.TryGetValue((x,0,z,0,0),out var b) && !catalog[b.pieceId].IsBed;
        private bool EdgeConflict(BuildRules rule,int x,int z,int rotation)
        {
            if(rule.Placement==BuildPlacement.Edge)
            {
                var e=Edge(x,z,rotation);
                if(e.axis==0 ? SolidBlock(e.x,e.z-1)||SolidBlock(e.x,e.z) : SolidBlock(e.x-1,e.z)||SolidBlock(e.x,e.z)) return true;
                // A bed may touch an outside wall, but a wall cannot divide its two-cell footprint.
                foreach(var bed in blocks.Values.Where(b=>catalog[b.pieceId].IsBed))
                {
                    var cells=Footprint(bed.pieceId,bed.x,bed.z,bed.rotation).ToArray();
                    var middle=cells[0].x==cells[1].x ? (cells[0].x,Math.Max(cells[0].z,cells[1].z),0) : (Math.Max(cells[0].x,cells[1].x),cells[0].z,1);
                    if(e==middle)return true;
                }
            }
            else if(rule.Placement==BuildPlacement.Solid)
            {
                if(rule.IsBed) { var e=Edge(x,z,rotation);return blocks.ContainsKey((e.x,0,e.z,2,e.axis)); }
                for(int r=0;r<4;r++){var e=Edge(x,z,r);if(blocks.ContainsKey((e.x,0,e.z,2,e.axis)))return true;}
            }
            return false;
        }
        public bool CanPlace(string id,int x,int level,int z,int rotation,out string message)
        {
            if(id==null || !catalog.TryGetValue(id,out var rule))return Fail("Bu yapı parçası tanımlı değil.",out message);
            if(!InBounds(x,level,z)||rotation<0||rotation>3)return Fail("Geçersiz dünya koordinatı veya dönüş.",out message);
            if(blocks.Count>=BlockLimit)return Fail("Bu kayıt için yapı sınırına ulaşıldı.",out message);
            if((rule.IsBed||rule.Placement!=BuildPlacement.Solid)&&level!=0)return Fail("Bu parçayı zemine yerleştir.",out message);
            if(Footprint(id,x,z,rotation).Any(c=>!ValidCoordinate(c.x,c.z)))return Fail("Geçersiz dünya karesi.",out message);
            if(blocks.ContainsKey(Key(id,x,level,z,rotation)) || (rule.Placement==BuildPlacement.Solid && Footprint(id,x,z,rotation).Any(c=>occupied.Contains((c.x,level,c.z)))))return Fail("Bu yer zaten dolu.",out message);
            if(level==0 && EdgeConflict(rule,x,z,rotation))return Fail("Duvar veya eşya ile çakışıyor.",out message);
            if(level>0 && !Supported(x,level,z))return Fail("Önce altına bir blok yerleştir.",out message);
            if(Wood<rule.WoodCost)return Fail("Odunun yetmiyor. Pazardan alabilir veya bir parçayı sökebilirsin.",out message);
            message="Sol tık · Yerleştir";return true;
        }
        private void Insert(BlockRecord record)
        {
            blocks.Add(Key(record.pieceId,record.x,record.level,record.z,record.rotation),record);
            if(catalog[record.pieceId].Placement==BuildPlacement.Solid)
                foreach(var c in Footprint(record.pieceId,record.x,record.z,record.rotation))occupied.Add((c.x,record.level,c.z));
        }
        public bool Place(string id,int x,int level,int z,int rotation,out string message)
        {
            if(!CanPlace(id,x,level,z,rotation,out message))return false;
            Wood-=catalog[id].WoodCost;Insert(new BlockRecord{pieceId=id,x=x,level=level,z=z,rotation=rotation});Revision++;
            message=$"Yapı yerleştirildi. −{catalog[id].WoodCost} odun.";return true;
        }
        // Legacy callers address volume blocks; runtime removal passes the exact ray-hit record.
        public bool Remove(int x,int level,int z,out string message) => Remove(blocks.TryGetValue((x,level,z,0,0),out var b)?b:null,out message);
        public bool Remove(BlockRecord requested,out string message)
        {
            if(requested==null||!catalog.ContainsKey(requested.pieceId)||!blocks.TryGetValue(Key(requested.pieceId,requested.x,requested.level,requested.z,requested.rotation),out var b))return Fail("Sökmek için bir parçayı hedefle.",out message);
            if(catalog[b.pieceId].Placement==BuildPlacement.Solid && occupied.Contains((b.x,b.level+1,b.z)))return Fail("Önce üstündeki bloğu sök.",out message);
            int refund=catalog[b.pieceId].WoodCost;
            if(Wood+refund>WoodLimit)return Fail("Odun çantan dolu; önce biraz odun kullan.",out message);
            blocks.Remove(Key(b.pieceId,b.x,b.level,b.z,b.rotation));
            if(catalog[b.pieceId].Placement==BuildPlacement.Solid)
                foreach(var c in Footprint(b.pieceId,b.x,b.z,b.rotation))occupied.Remove((c.x,b.level,c.z));
            Wood+=refund;Revision++;message=$"Yapı söküldü. +{refund} odun geri alındı.";return true;
        }
        public bool DoorIsOpen(BlockRecord record) => record!=null && catalog.ContainsKey(record.pieceId) && blocks.TryGetValue(Key(record.pieceId,record.x,record.level,record.z,record.rotation),out var b) && b.doorOpen;
        public bool ToggleDoor(BlockRecord record,out string message)
        {
            if(record==null||!catalog.TryGetValue(record.pieceId,out var rule)||!rule.IsDoor||!blocks.TryGetValue(Key(record.pieceId,record.x,record.level,record.z,record.rotation),out var b))return Fail("Kapı bulunamadı.",out message);
            b.doorOpen=!b.doorOpen;message=b.doorOpen?"Kapı açıldı.":"Kapı kapandı.";return true;
        }
        internal bool AddWood(int count)
        {
            if (count < 1 || count > WoodLimit - Wood) return false;
            Wood += count; return true;
        }
        public BuildingSnapshot Snapshot() => new BuildingSnapshot { wood = Wood, blocks = Blocks.OrderBy(b => b.level).ThenBy(b => b.z).ThenBy(b => b.x).ThenBy(b => b.pieceId,StringComparer.Ordinal).ThenBy(b => b.rotation).ToArray() };
        public static BuildingModel Restore(BuildingSnapshot saved, IEnumerable<BuildRules> rules)
        {
            if (saved == null || saved.blocks == null || saved.blocks.Length > BlockLimit) throw new ArgumentException("Missing or oversized construction state.");
            var model = new BuildingModel(rules, saved.wood);
            if (saved.blocks.Any(b => b == null)) throw new ArgumentException("Null block.");
            int wood=model.Wood;model.Wood=WoodLimit;
            foreach(var b in saved.blocks.OrderBy(b=>b.level))
            {
                if(!model.CanPlace(b.pieceId,b.x,b.level,b.z,b.rotation,out _) || (b.doorOpen&&!model.catalog[b.pieceId].IsDoor))throw new ArgumentException("Invalid, duplicate or unsupported structure.");
                model.Insert(b.Copy());
            }
            model.Wood=wood;
            return model;
        }
        private static bool Fail(string reason, out string message) { message = reason; return false; }
    }
}
