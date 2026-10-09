using System;
using System.Collections.Generic;
using System.Linq;

namespace Farmer
{
    public enum BuildPlacement { Solid, Floor, Edge, Roof }

    public sealed class BuildRules
    {
        public string Id { get; }
        public int WoodCost { get; }
        public bool IsBed { get; }
        public BuildPlacement Placement { get; }
        public bool IsDoor { get; }
        public int Price { get; }
        public int Length { get; }
        public bool IsFurniture => IsBed || Price>0;
        public BuildRules(string id, int woodCost, bool isBed = false, BuildPlacement placement = BuildPlacement.Solid, bool isDoor = false, int price = 0, int length = 1)
        {
            if (string.IsNullOrWhiteSpace(id) || woodCost < 1 || woodCost > BuildingModel.WoodLimit) throw new ArgumentException("Invalid build rules.");
            if (!Enum.IsDefined(typeof(BuildPlacement), placement) || (isBed && placement != BuildPlacement.Solid) || (isDoor && placement != BuildPlacement.Edge)) throw new ArgumentException("Invalid placement kind.");
            if(price<0 || price>100000 || length<1 || length>2 || (price>0 && placement!=BuildPlacement.Solid))throw new ArgumentException("Invalid furniture rules.");
            Price=isBed?BuildingModel.BedPrice:price;Length=isBed?2:length;
            Id = id; WoodCost = woodCost; IsBed = isBed; Placement = placement; IsDoor = isDoor;
        }
        public static BuildRules[] Defaults => new[] { new BuildRules("wood_block", 2), new BuildRules("bed", 8, true), new BuildRules("wood_floor", 1, placement: BuildPlacement.Floor), new BuildRules("wood_wall", 2, placement: BuildPlacement.Edge), new BuildRules("wood_door", 3, placement: BuildPlacement.Edge, isDoor: true), new BuildRules("wood_roof", 2, placement: BuildPlacement.Roof), new BuildRules("home_chest",1,price:80), new BuildRules("home_table",1,price:60,length:2), new BuildRules("home_chair",1,price:25), new BuildRules("home_lantern",1,price:45) };
    }
    [Serializable] public sealed class BlockRecord
    {
        public string pieceId;
        public int x, level, z, rotation;
        public bool doorOpen;
        public string instanceId;
        public ItemStack[] contents;
        public BlockRecord Copy() => new BlockRecord { pieceId = pieceId, x = x, level = level, z = z, rotation = rotation, doorOpen = doorOpen, instanceId=instanceId, contents=contents?.Select(s=>s?.Copy()).ToArray() };
    }
    [Serializable] public sealed class BuildingSnapshot { public int wood; public int beds; public BlockRecord[] blocks; public ItemStack[] furniture; }

    // Sparse world cells: no farm or construction-zone boundary.
    public sealed partial class BuildingModel
    {
        public const int Levels = 3, StarterWood = 24, WoodLimit = 999, BlockLimit = 10000;
        public const int BedPrice=100;
        public const int WoodPackCount = 10, WoodPackPrice = 20;
        private readonly Dictionary<string, BuildRules> catalog;
        private readonly Dictionary<(int x, int level, int z, int layer, int axis), BlockRecord> blocks = new Dictionary<(int, int, int, int, int), BlockRecord>();
        private readonly HashSet<(int,int,int)> occupied = new HashSet<(int,int,int)>();
        public int Revision { get; private set; }
        public int Wood { get; private set; }
        public int Beds { get; private set; }
        private bool restoring;
        public int Count => blocks.Count;
        public IEnumerable<BlockRecord> Blocks => blocks.Values.Select(b => b.Copy());
        public BuildingModel(IEnumerable<BuildRules> rules, int wood = StarterWood, int beds = 0)
        {
            if (wood < 0 || wood > WoodLimit || beds < 0 || beds > WoodLimit) throw new ArgumentException("Invalid wood inventory.");
            catalog = rules.ToDictionary(r => r.Id);
            if (catalog.Count == 0) throw new ArgumentException("Build catalog is empty.");
            Wood = wood; Beds=beds;
        }
        public static bool ValidCoordinate(int x, int z) => x >= -100000 && x <= 100000 && z >= -100000 && z <= 100000;
        public static bool InBounds(int x, int level, int z) => ValidCoordinate(x, z) && level >= 0 && level < Levels;
        public BuildRules Rules(string id) => catalog[id];
        public IEnumerable<(int x,int z)> Footprint(string id, int x, int z, int rotation)
        {
            yield return (x,z);
            if (catalog[id].Length==2)
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
        private bool Supported(int x,int level,int z) => blocks.TryGetValue((x,level-1,z,0,0),out var b) && !catalog[b.pieceId].IsFurniture;
        private bool SolidBlock(int x,int z) => blocks.TryGetValue((x,0,z,0,0),out var b) && !catalog[b.pieceId].IsFurniture;
        private bool EdgeConflict(BuildRules rule,int x,int z,int rotation)
        {
            if(rule.Placement==BuildPlacement.Edge)
            {
                var e=Edge(x,z,rotation);
                if(e.axis==0 ? SolidBlock(e.x,e.z-1)||SolidBlock(e.x,e.z) : SolidBlock(e.x-1,e.z)||SolidBlock(e.x,e.z)) return true;
                // A bed may touch an outside wall, but a wall cannot divide its two-cell footprint.
                foreach(var bed in blocks.Values.Where(b=>catalog[b.pieceId].Length==2))
                {
                    var cells=Footprint(bed.pieceId,bed.x,bed.z,bed.rotation).ToArray();
                    var middle=cells[0].x==cells[1].x ? (cells[0].x,Math.Max(cells[0].z,cells[1].z),0) : (Math.Max(cells[0].x,cells[1].x),cells[0].z,1);
                    if(e==middle)return true;
                }
            }
            else if(rule.Placement==BuildPlacement.Solid)
            {
                if(rule.IsFurniture) { if(rule.Length==1)return false; var e=Edge(x,z,rotation);return blocks.ContainsKey((e.x,0,e.z,2,e.axis)); }
                for(int r=0;r<4;r++){var e=Edge(x,z,r);if(blocks.ContainsKey((e.x,0,e.z,2,e.axis)))return true;}
            }
            return false;
        }
        public bool CanPlace(string id,int x,int level,int z,int rotation,out string message,bool ignoreInventory=false)
        {
            if(id==null || !catalog.TryGetValue(id,out var rule))return Fail("Bu yapı parçası tanımlı değil.",out message);
            if(!(rule.Placement==BuildPlacement.Roof ? ValidCoordinate(x,z)&&(level==2||level==3) : InBounds(x,level,z))||rotation<0||rotation>3)return Fail("Geçersiz dünya koordinatı veya dönüş.",out message);
            if(blocks.Count>=BlockLimit)return Fail("Bu kayıt için yapı sınırına ulaşıldı.",out message);
            if((rule.IsFurniture||rule.Placement==BuildPlacement.Floor||rule.Placement==BuildPlacement.Edge)&&level!=0)return Fail("Bu parçayı zemine yerleştir.",out message);
            if(Footprint(id,x,z,rotation).Any(c=>!ValidCoordinate(c.x,c.z)))return Fail("Geçersiz dünya karesi.",out message);
            if(blocks.ContainsKey(Key(id,x,level,z,rotation)) || (rule.Placement==BuildPlacement.Solid && Footprint(id,x,z,rotation).Any(c=>occupied.Contains((c.x,level,c.z)))))return Fail("Bu yer zaten dolu.",out message);
            if(level==0 && EdgeConflict(rule,x,z,rotation))return Fail("Duvar veya eşya ile çakışıyor.",out message);
            if(rule.Placement==BuildPlacement.Solid && level>0 && !Supported(x,level,z))return Fail("Önce altına bir blok yerleştir.",out message);
            if(!restoring && !ignoreInventory && rule.IsFurniture && FurnitureCount(id)<1)return Fail("Önce bu mobilyayı dükkândan al.",out message);
            if(!restoring && rule.Placement==BuildPlacement.Roof && !RoofsSupported(new BlockRecord{pieceId=id,x=x,level=level,z=z,rotation=rotation}))return Fail("Çatıyı aynı yükseklikte duvar/blok kenarından başlat; en fazla 4 parça uzat.",out message);
            if(!rule.IsFurniture && !ignoreInventory && Wood<rule.WoodCost)return Fail("Odunun yetmiyor. Pazardan alabilir veya bir parçayı sökebilirsin.",out message);
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
            if(catalog[id].IsFurniture)ChangeFurniture(id,-1);else Wood-=catalog[id].WoodCost;Insert(new BlockRecord{pieceId=id,x=x,level=level,z=z,rotation=rotation,instanceId=Guid.NewGuid().ToString("N"),contents=id=="home_chest"?Array.Empty<ItemStack>():null});Revision++;
            message=catalog[id].IsFurniture?"Mobilya yerleştirildi.":$"Yapı yerleştirildi. −{catalog[id].WoodCost} odun.";return true;
        }
        // Legacy callers address volume blocks; runtime removal passes the exact ray-hit record.
        public bool Remove(int x,int level,int z,out string message) => Remove(blocks.TryGetValue((x,level,z,0,0),out var b)?b:null,out message);
        public bool Remove(BlockRecord requested,out string message)
        {
            if(requested==null||!catalog.ContainsKey(requested.pieceId)||!blocks.TryGetValue(Key(requested.pieceId,requested.x,requested.level,requested.z,requested.rotation),out var b))return Fail("Sökmek için bir parçayı hedefle.",out message);
            if(catalog[b.pieceId].Placement==BuildPlacement.Solid && occupied.Contains((b.x,b.level+1,b.z)))return Fail("Önce üstündeki bloğu sök.",out message);
            if(!CanDetach(b,out message))return false;
            if(b.contents!=null && b.contents.Any(i=>i.count>0))return Fail("Önce sandığı boşalt. İçindekilerle taşımak için M kullan.",out message);
            int refund=catalog[b.pieceId].IsFurniture?0:catalog[b.pieceId].WoodCost;
            if(catalog[b.pieceId].IsFurniture && FurnitureCount(b.pieceId)>=WoodLimit)return Fail("Yatak envanteri dolu.",out message);
            if(Wood+refund>WoodLimit)return Fail("Odun çantan dolu; önce biraz odun kullan.",out message);
            blocks.Remove(Key(b.pieceId,b.x,b.level,b.z,b.rotation));
            if(catalog[b.pieceId].Placement==BuildPlacement.Solid)
                foreach(var c in Footprint(b.pieceId,b.x,b.z,b.rotation))occupied.Remove((c.x,b.level,c.z));
            if(catalog[b.pieceId].IsFurniture)ChangeFurniture(b.pieceId,1);
            Wood+=refund;Revision++;message=catalog[b.pieceId].IsFurniture?"Mobilya çantaya alındı.":$"Yapı söküldü. +{refund} odun geri alındı.";return true;
        }
        public bool DoorIsOpen(BlockRecord record) => record!=null && catalog.ContainsKey(record.pieceId) && blocks.TryGetValue(Key(record.pieceId,record.x,record.level,record.z,record.rotation),out var b) && b.doorOpen;
        public bool ToggleDoor(BlockRecord record,out string message)
        {
            if(record==null||!catalog.TryGetValue(record.pieceId,out var rule)||!rule.IsDoor||!blocks.TryGetValue(Key(record.pieceId,record.x,record.level,record.z,record.rotation),out var b))return Fail("Kapı bulunamadı.",out message);
            b.doorOpen=!b.doorOpen;message=b.doorOpen?"Kapı açıldı.":"Kapı kapandı.";return true;
        }
        public bool AddBed() {if(Beds>=WoodLimit)return false;Beds++;return true;}
        private void Erase(BlockRecord b)
        {
            blocks.Remove(Key(b.pieceId,b.x,b.level,b.z,b.rotation));
            if(catalog[b.pieceId].Placement==BuildPlacement.Solid)
                foreach(var c in Footprint(b.pieceId,b.x,b.z,b.rotation))occupied.Remove((c.x,b.level,c.z));
        }
        public bool CanDetach(BlockRecord b,out string message)
        {
            if(catalog[b.pieceId].Placement==BuildPlacement.Solid && occupied.Contains((b.x,b.level+1,b.z)))return Fail("Önce üstündeki bloğu taşı veya sök.",out message);
            Erase(b);bool supported;
            try{supported=RoofsSupported();}finally{Insert(b);}
            if(!supported)return Fail("Bu parça çatıyı taşıyor. Önce çatıyı taşı veya sök.",out message);
            message="";return true;
        }
        public bool CanMove(BlockRecord record,int x,int level,int z,int rotation,out string message)
        {
            if(record==null||!catalog.ContainsKey(record.pieceId)||!blocks.TryGetValue(Key(record.pieceId,record.x,record.level,record.z,record.rotation),out var b))return Fail("Taşınacak eşya bulunamadı.",out message);
            if(!CanDetach(b,out message))return false;
            Erase(b);
            try{return CanPlace(b.pieceId,x,level,z,rotation,out message,true);}finally{Insert(b);}
        }
        public bool Move(BlockRecord record,int x,int level,int z,int rotation,out string message)
        {
            if(!CanMove(record,x,level,z,rotation,out message))return false;
            var b=blocks[Key(record.pieceId,record.x,record.level,record.z,record.rotation)];Erase(b);
            var moved=b.Copy();moved.x=x;moved.level=level;moved.z=z;moved.rotation=rotation;Insert(moved);Revision++;
            message="Eşya taşındı.";return true;
        }
        public static float RoofHeight(int level)=>level==2?2.4f:3f;
        public bool CanSupportRoof(int x, int level, int z)
        {
            if (!ValidCoordinate(x,z) || (level!=2 && level!=3)) return false;
            // An existing panel also supplies its height when extending the roof.
            if (blocks.Values.Any(b=>b.x==x && b.z==z && b.level==level && catalog[b.pieceId].Placement==BuildPlacement.Roof)) return true;
            return RoofsSupported(new BlockRecord { pieceId="wood_roof", x=x, level=level, z=z });
        }
        public bool HasDirectRoofSupport(int x, int level, int z) =>
            ValidCoordinate(x,z) && (level==2 || level==3) && DirectRoofSupport(new BlockRecord { x=x, level=level, z=z });
        private bool DirectRoofSupport(BlockRecord roof)
        {
            for(int r=0;r<4;r++)
            {
                var e=Edge(roof.x,roof.z,r);
                if(roof.level==2 && blocks.ContainsKey((e.x,0,e.z,2,e.axis)))return true;
            }
            if(roof.level==3)
                foreach(var c in new[]{(roof.x,roof.z),(roof.x-1,roof.z),(roof.x+1,roof.z),(roof.x,roof.z-1),(roof.x,roof.z+1)})
                    if(blocks.TryGetValue((c.Item1,2,c.Item2,0,0),out var support)&&!catalog[support.pieceId].IsFurniture)return true;
            return false;
        }
        private bool RoofsSupported(BlockRecord extra=null)
        {
            var roofs=blocks.Values.Where(b=>catalog[b.pieceId].Placement==BuildPlacement.Roof).ToList();if(extra!=null)roofs.Add(extra);
            var byCell=roofs.ToDictionary(b=>(b.x,b.level,b.z));var reached=new HashSet<(int,int,int)>();var queue=new Queue<(BlockRecord roof,int distance)>();
            foreach(var b in roofs)if(DirectRoofSupport(b)){reached.Add((b.x,b.level,b.z));queue.Enqueue((b,0));}
            while(queue.Count>0)
            {
                var entry=queue.Dequeue();if(entry.distance>=4)continue;var b=entry.roof;
                foreach(var key in new[]{(b.x-1,b.level,b.z),(b.x+1,b.level,b.z),(b.x,b.level,b.z-1),(b.x,b.level,b.z+1)})
                    if(byCell.TryGetValue(key,out var next)&&reached.Add(key))queue.Enqueue((next,entry.distance+1));
            }
            return reached.Count==roofs.Count;
        }
        internal bool AddWood(int count)
        {
            if (count < 1 || count > WoodLimit - Wood) return false;
            Wood += count; return true;
        }
        public BuildingSnapshot Snapshot() => new BuildingSnapshot { wood = Wood, beds = Beds, furniture=FurnitureSnapshot(), blocks = Blocks.OrderBy(b => b.level).ThenBy(b => b.z).ThenBy(b => b.x).ThenBy(b => b.pieceId,StringComparer.Ordinal).ThenBy(b => b.rotation).ToArray() };
        public static BuildingModel Restore(BuildingSnapshot saved, IEnumerable<BuildRules> rules)
        {
            if (saved == null || saved.blocks == null || saved.blocks.Length > BlockLimit) throw new ArgumentException("Missing or oversized construction state.");
            var model = new BuildingModel(rules, saved.wood, saved.beds);
            model.RestoreFurniture(saved.furniture);
            if (saved.blocks.Any(b => b == null)) throw new ArgumentException("Null block.");
            int wood=model.Wood;model.Wood=WoodLimit;model.restoring=true;
            foreach(var b in saved.blocks.OrderBy(b=>b.level))
            {
                if(!model.CanPlace(b.pieceId,b.x,b.level,b.z,b.rotation,out _) || (b.doorOpen&&!model.catalog[b.pieceId].IsDoor))throw new ArgumentException("Invalid, duplicate or unsupported structure.");
                model.Insert(b.Copy());
            }
            model.Wood=wood;model.restoring=false;
            if(!model.RoofsSupported())throw new ArgumentException("Unsupported roof.");
            return model;
        }
        private static bool Fail(string reason, out string message) { message = reason; return false; }
    }
}
