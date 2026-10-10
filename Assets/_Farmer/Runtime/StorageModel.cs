using System;
using System.Collections.Generic;
using System.Linq;
namespace Farmer
{
    [Serializable] public sealed class ItemStack
    {
        public string id;
        public int count;
        public ItemStack Copy()=>new ItemStack{id=id,count=count};
    }
    public sealed partial class BuildingModel
    {
        private readonly Dictionary<string,int> furniture=new Dictionary<string,int>();
        public IEnumerable<BuildRules> FurnitureRules=>catalog.Values.Where(r=>r.IsFurniture);
        public int FurnitureCount(string id)=>catalog.TryGetValue(id,out var r)&&r.IsFurniture?(r.IsBed?Beds:furniture.TryGetValue(id,out var n)?n:0):0;
        internal bool ChangeFurniture(string id,int delta)
        {
            if(!catalog.TryGetValue(id,out var r)||!r.IsFurniture)return false;
            long total=(long)FurnitureCount(id)+delta;if(total<0||total>WoodLimit)return false;
            if(r.IsBed)Beds=(int)total;else furniture[id]=(int)total;return true;
        }
        internal bool ChangeWood(int delta){long total=(long)Wood+delta;if(total<0||total>WoodLimit)return false;Wood=(int)total;return true;}
        private ItemStack[] FurnitureSnapshot()=>furniture.OrderBy(p=>p.Key,StringComparer.Ordinal).Select(p=>new ItemStack{id=p.Key,count=p.Value}).ToArray();
        private void RestoreFurniture(ItemStack[] records)
        {
            if(records==null)return;
            var seen=new HashSet<string>();
            foreach(var s in records)
                if(s==null||s.id==null||!catalog.TryGetValue(s.id,out var r)||!r.IsFurniture||r.IsBed||!seen.Add(s.id)||s.count<0||s.count>WoodLimit||!ChangeFurniture(s.id,s.count))throw new ArgumentException("Invalid furniture inventory.");
        }
        internal BlockRecord Storage(string id)=>string.IsNullOrEmpty(id)?null:blocks.Values.FirstOrDefault(b=>b.pieceId=="home_chest"&&b.instanceId==id);
        public ItemStack[] StoredItems(string id)=>Storage(id)?.contents?.Select(i=>i.Copy()).ToArray()??Array.Empty<ItemStack>();
        public BlockRecord FindStorage(string id)=>Storage(id)?.Copy();
    }
    public sealed partial class FarmModel
    {
        public const int ChestSlots=16;
        public string[] BagItems()=>new[]{"tool:watering","tool:sickle","tool:hoe","tool:axe","tool:pickaxe","wood","stone"}
            .Concat(crops.Keys.Select(c=>"seed:"+c)).Concat(crops.Keys.Select(c=>"crop:"+c))
            .Concat(recipes.Keys.Select(id=>"crafted:"+id)).Concat(Building.FurnitureRules.Select(r=>"furniture:"+r.Id)).ToArray();
        public int BagCount(string id)
        {
            if(id==null)return 0;
            if(id.StartsWith("crafted:",StringComparison.Ordinal))return crafted.TryGetValue(id.Substring(8),out var amount)?amount:0;
            if(id=="wood")return Building.Wood;
            if(id=="stone")return Stone;
            if(id=="tool:pickaxe")return OwnsPickaxe?1:0;
            if(id=="tool:watering"||id=="tool:sickle"||id=="tool:hoe"||id=="tool:axe")return 1;
            if(id.StartsWith("seed:",StringComparison.Ordinal))return Seeds(id.Substring(5));
            if(id.StartsWith("crop:",StringComparison.Ordinal))return Produce(id.Substring(5));
            if(id.StartsWith("furniture:",StringComparison.Ordinal))return Building.FurnitureCount(id.Substring(10));
            return 0;
        }
        private bool CanStore(string id)=>id!=null&&!id.StartsWith("tool:",StringComparison.Ordinal)&&BagItems().Contains(id);
        private bool ChangeBag(string id,int delta)
        {
            long total=(long)BagCount(id)+delta;if(total<0||total>StackLimit)return false;
            if(id.StartsWith("crafted:",StringComparison.Ordinal)){crafted[id.Substring(8)]=(int)total;return true;}
            if(id=="wood")return Building.ChangeWood(delta);
            if(id=="stone"){Stone=(int)total;return true;}
            if(id.StartsWith("furniture:",StringComparison.Ordinal))return Building.ChangeFurniture(id.Substring(10),delta);
            var dict=id.StartsWith("seed:",StringComparison.Ordinal)?seeds:produce;
            dict[id.Substring(5)]=(int)total;return true;
        }
        public bool BuyFurniture(string id,out string message)
        {
            var rule=Building.FurnitureRules.FirstOrDefault(r=>r.Id==id);
            if(rule==null)return Fail("Bu mobilya satılmıyor.",out message);
            if(Money<rule.Price)return Fail($"Bu mobilya için {rule.Price} para gerekiyor.",out message);
            if(!Building.ChangeFurniture(id,1))return Fail("Mobilya çantan dolu.",out message);
            Money-=rule.Price;message="Mobilya çantaya eklendi. İnşa > Mobilya bölümünden yerleştir.";return true;
        }
        public bool TransferStorage(string chestId,string item,int amount,bool withdraw,out string message)
        {
            var chest=Building.Storage(chestId);
            if(chest==null||!CanStore(item)||amount<1||amount>StackLimit)return Fail("Geçersiz sandık veya eşya aktarımı.",out message);
            var contents=(chest.contents??Array.Empty<ItemStack>()).ToDictionary(s=>s.id,s=>s.count);
            int stored=contents.TryGetValue(item,out var n)?n:0,bag=BagCount(item);
            if(withdraw?(stored<amount||bag>StackLimit-amount):(bag<amount||stored>StackLimit-amount))return Fail("Yeterli eşya veya yığın kapasitesi yok.",out message);
            if(!withdraw&&stored==0&&contents.Count>=ChestSlots)return Fail("Sandık dolu.",out message);
            if(!ChangeBag(item,withdraw?amount:-amount))return Fail("Çantaya aktarılamadı.",out message);
            int remaining=stored+(withdraw?-amount:amount);
            if(remaining==0)contents.Remove(item);else contents[item]=remaining;
            chest.contents=contents.OrderBy(p=>p.Key,StringComparer.Ordinal).Select(p=>new ItemStack{id=p.Key,count=p.Value}).ToArray();
            message=withdraw?"Eşya çantaya alındı.":"Eşya sandığa kondu.";return true;
        }
        private void ValidateStorage(int version)
        {
            var identities=new HashSet<string>();
            foreach(var b in Building.Blocks)
            {
                if(b.pieceId!="home_chest")
                {if(b.contents!=null&&b.contents.Length>0)throw new ArgumentException("Contents on non-storage furniture.");continue;}
                if(version<7||string.IsNullOrWhiteSpace(b.instanceId)||!identities.Add(b.instanceId)||b.contents==null||b.contents.Length>ChestSlots)throw new ArgumentException("Invalid storage identity or capacity.");
                var seen=new HashSet<string>();
                foreach(var i in b.contents)
                    if(i==null||!CanStore(i.id)||!seen.Add(i.id)||i.count<1||i.count>StackLimit)throw new ArgumentException("Invalid stored item.");
            }
        }
    }
}
