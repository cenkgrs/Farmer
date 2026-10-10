using System;
using System.Collections.Generic;
using System.Linq;
namespace Farmer
{
    public sealed class RecipeRules
    {
        public string Id { get; }
        public int OutputCount { get; }
        public int SalePrice { get; }
        private readonly ItemStack[] ingredients;
        public ItemStack[] Ingredients => ingredients.Select(i=>i.Copy()).ToArray();
        public RecipeRules(string id,int outputCount,int salePrice,IEnumerable<ItemStack> inputs)
        {
            ingredients=inputs?.Select(i=>i?.Copy()).ToArray();
            if(string.IsNullOrWhiteSpace(id)||outputCount<1||outputCount>FarmModel.StackLimit||salePrice<1||salePrice>100000
                ||ingredients==null||ingredients.Length==0||ingredients.Any(i=>i==null||string.IsNullOrWhiteSpace(i.id)||i.count<1||i.count>FarmModel.StackLimit)
                ||ingredients.Select(i=>i.id).Distinct().Count()!=ingredients.Length)
                throw new ArgumentException("Invalid recipe.");
            Id=id;OutputCount=outputCount;SalePrice=salePrice;
        }
    }
    public sealed partial class FarmModel
    {
        private Dictionary<string,RecipeRules> recipes;
        private readonly Dictionary<string,int> crafted=new Dictionary<string,int>();
        private void InitializeRecipes(IEnumerable<RecipeRules> definitions)
        {
            recipes=(definitions??Array.Empty<RecipeRules>()).ToDictionary(r=>r.Id);
            foreach(var r in recipes.Values)
            {
                // This first processing step consumes raw materials/crops, not other recipes or permanent tools.
                if(r.Ingredients.Any(i=>!CanStore(i.id)||i.id.StartsWith("crafted:")||i.id.StartsWith("furniture:")))
                    throw new ArgumentException("Unknown recipe ingredient.");
                crafted[r.Id]=0;
            }
        }
        public bool CanCraft(string id,int batches=1)
        {
            if(id==null||!recipes.TryGetValue(id,out var r)||batches<1||batches>StackLimit)return false;
            return (long)BagCount("crafted:"+id)+(long)r.OutputCount*batches<=StackLimit
                &&r.Ingredients.All(i=>(long)BagCount(i.id)>=(long)i.count*batches);
        }
        public bool Craft(string id,int batches,out string message)
        {
            if(!CanCraft(id,batches))return Fail("Malzeme yetersiz veya ürün yığını dolu.",out message);
            var r=recipes[id];
            foreach(var i in r.Ingredients)ChangeBag(i.id,-i.count*batches);
            ChangeBag("crafted:"+id,r.OutputCount*batches);
            message="Ürün hazırlandı ve çantaya eklendi.";return true;
        }
        public bool SellCrafted(string id,int count,out string message)
        {
            if(id==null||!recipes.TryGetValue(id,out var r)||count<1||count>BagCount("crafted:"+id))return Fail("Satılacak işlenmiş ürün yok.",out message);
            long income=(long)r.SalePrice*count;
            if(Money+income>MoneyLimit)return Fail("Para sınırına ulaştın.",out message);
            ChangeBag("crafted:"+id,-count);Money+=(int)income;message=$"+{income} para.";return true;
        }
        private ItemStack[] CraftedSnapshot()=>crafted.Select(p=>new ItemStack{id=p.Key,count=p.Value}).ToArray();
        private void RestoreCrafted(ItemStack[] items)
        {
            if(items==null)throw new ArgumentException("Missing crafted inventory.");
            var seen=new HashSet<string>();
            foreach(var item in items)
            {
                if(item==null||item.id==null||!recipes.ContainsKey(item.id)||!seen.Add(item.id)||item.count<0||item.count>StackLimit)
                    throw new ArgumentException("Invalid crafted inventory.");
                crafted[item.id]=item.count;
            }
        }
    }
}
