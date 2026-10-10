using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
namespace Farmer.Tests
{
    public sealed class CropExpansionTests
    {
        private static readonly CropRules[] Crops={new CropRules("turnip",10,18,3,1),new CropRules("carrot",14,27,4,1),new CropRules("tomato",24,12,5,2,2)};
        private static readonly RecipeRules[] Recipes={new RecipeRules("vegetable_crate",1,90,new[]{new ItemStack{id="crop:turnip",count=1},new ItemStack{id="crop:carrot",count=1},new ItemStack{id="crop:tomato",count=2},new ItemStack{id="wood",count=2}})};
        private static FarmModel Fresh()=>new FarmModel(Crops,startingMoney:500,worldSeed:12,recipeCatalog:Recipes);
        private static FarmModel Restore(FarmSnapshot s)=>FarmModel.Restore(s,Crops,6,6,recipeCatalog:Recipes);
        private static string Json(FarmModel f)=>JsonUtility.ToJson(f.Snapshot());
        private static int Plant(FarmModel f,string id,int x=0)
        {Assert.That(f.BuySeeds(id,1,out _),Is.True);f.Till(x,0,out _);int p=f.IndexAt(x,0);Assert.That(f.Plant(p,id,out _),Is.True);return p;}
        [Test] public void SelectedSeedsPersistAndDoNotChangeOtherInventory()
        {
            var f=Fresh();f.BuySeeds("carrot",2,out _);f.BuySeeds("tomato",1,out _);Assert.That(f.SelectCrop("tomato"),Is.True);
            f=Restore(JsonUtility.FromJson<FarmSnapshot>(Json(f)));Assert.That(f.SelectedCropId,Is.EqualTo("tomato"));
            f.Till(0,0,out _);f.Equip(FarmItem.Seeds);Assert.That(f.UseEquipped(0,f.SelectedCropId,out _),Is.True);
            Assert.That(f.Seeds("carrot"),Is.EqualTo(2));Assert.That(f.Seeds("tomato"),Is.Zero);
            var before=Json(f);Assert.That(f.SelectCrop("unknown"),Is.False);Assert.That(Json(f),Is.EqualTo(before));
        }
        [Test] public void TomatoRegrowsWithoutReseedingOrRewateringAndCannotDoubleHarvest()
        {
            var f=Fresh();int p=Plant(f,"tomato");f.AdvanceMinutes(5*1440);Assert.That(f.IsReady(p),Is.False);
            f.Water(p,out _);f.AdvanceMinutes(4*1440);Assert.That(f.IsReady(p),Is.False);
            f.AdvanceMinutes(1440);Assert.That(f.Harvest(p,out _),Is.True);Assert.That(f.Produce("tomato"),Is.EqualTo(2));
            Assert.That(f.Plot(p).cropId,Is.EqualTo("tomato"));Assert.That(f.Plot(p).watered,Is.True);Assert.That(f.Stage(p),Is.EqualTo(2));
            Assert.That(f.Harvest(p,out _),Is.False);Assert.That(f.Plant(p,"turnip",out _),Is.False);
            f=Restore(JsonUtility.FromJson<FarmSnapshot>(Json(f)));f.AdvanceMinutes(1440);Assert.That(f.IsReady(p),Is.False);
            f.AdvanceMinutes(1440);Assert.That(f.Harvest(p,out _),Is.True);Assert.That(f.Produce("tomato"),Is.EqualTo(4));
            Assert.That(f.Seeds("tomato"),Is.Zero);
        }
        [TestCase("turnip",3)] [TestCase("carrot",4)] public void RootVegetablesClearTheirPlotAfterHarvest(string id,int days)
        {
            var f=Fresh();int p=Plant(f,id);f.Water(p,out _);f.AdvanceMinutes(days*1440);Assert.That(f.Harvest(p,out _),Is.True);
            Assert.That(f.Plot(p).cropId,Is.Empty);Assert.That(f.Stage(p),Is.EqualTo(-1));Assert.That(f.Plot(p).regrowing,Is.False);
            Assert.That(f.Sell(id,1,out _),Is.True);
        }
        [Test] public void FullProduceStackPreservesRipeTomatoAndGrowthState()
        {
            var f=Fresh();int p=Plant(f,"tomato");f.Water(p,out _);f.AdvanceMinutes(5*1440);
            var s=f.Snapshot();s.produce.Single(i=>i.cropId=="tomato").count=998;f=Restore(s);var before=Json(f);
            Assert.That(f.Harvest(p,out _),Is.False);Assert.That(Json(f),Is.EqualTo(before));
        }
        private static FarmModel Stocked(int count=1)
        {
            var s=Fresh().Snapshot();
            foreach(var i in s.produce)i.count=(i.cropId=="tomato"?2:1)*count;
            s.building.wood=2*count;return Restore(s);
        }
        [Test] public void RecipeConsumesExactIngredientsAndCrateCanBeStoredReloadedAndSold()
        {
            var f=Stocked(2);Assert.That(f.Craft("vegetable_crate",1,out _),Is.True);
            Assert.That(f.Produce("turnip"),Is.EqualTo(1));Assert.That(f.Produce("carrot"),Is.EqualTo(1));Assert.That(f.Produce("tomato"),Is.EqualTo(2));Assert.That(f.Building.Wood,Is.EqualTo(2));
            Assert.That(f.BagCount("crafted:vegetable_crate"),Is.EqualTo(1));
            f.BuyFurniture("home_chest",out _);f.Building.Place("home_chest",5,0,5,0,out _);var id=f.Building.Blocks.Single().instanceId;
            Assert.That(f.TransferStorage(id,"crafted:vegetable_crate",1,false,out _),Is.True);f=Restore(JsonUtility.FromJson<FarmSnapshot>(Json(f)));
            Assert.That(f.BagCount("crafted:vegetable_crate"),Is.Zero);Assert.That(f.TransferStorage(id,"crafted:vegetable_crate",1,true,out _),Is.True);
            int money=f.Money;Assert.That(f.SellCrafted("vegetable_crate",1,out _),Is.True);Assert.That(f.Money,Is.EqualTo(money+90));Assert.That(f.SellCrafted("vegetable_crate",1,out _),Is.False);
        }
        [TestCase(0)] [TestCase(-1)] [TestCase(1000)] [TestCase(2)] public void InvalidOrUnaffordableBatchDoesNotConsumeIngredients(int batches)
        {var f=Stocked();var before=Json(f);Assert.That(f.Craft("vegetable_crate",batches,out _),Is.False);Assert.That(Json(f),Is.EqualTo(before));}
        [Test] public void FullOutputStackAndMoneyLimitRejectTransactionsAtomically()
        {
            var s=Stocked().Snapshot();s.crafted.Single().count=999;var f=Restore(s);var before=Json(f);
            Assert.That(f.Craft("vegetable_crate",1,out _),Is.False);Assert.That(Json(f),Is.EqualTo(before));
            s.money=FarmModel.MoneyLimit;f=Restore(s);before=Json(f);Assert.That(f.SellCrafted("vegetable_crate",1,out _),Is.False);Assert.That(Json(f),Is.EqualTo(before));
        }
        [Test] public void MissingSingleIngredientLeavesAllOtherMaterialsUntouched()
        {
            var s=Stocked().Snapshot();s.produce.Single(i=>i.cropId=="carrot").count=0;var f=Restore(s);var before=Json(f);
            Assert.That(f.Craft("vegetable_crate",1,out _),Is.False);Assert.That(Json(f),Is.EqualTo(before));
        }
        [Test] public void VersionEightMigratesCropInventoryAndKeepsOriginalBackup()
        {
            var old=new FarmModel(new[]{Crops[0]},worldSeed:12);old.BuySeeds("turnip",2,out _);old.Till(0,0,out _);old.Plant(0,"turnip",out _);old.Water(0,out _);
            var s=old.Snapshot();s.version=8;s.selectedCropId=null;s.crafted=null;var text=JsonUtility.ToJson(s);
            var path=Path.Combine(Path.GetTempPath(),"FarmerQA-Crops-"+Guid.NewGuid().ToString("N"),"farm.json");Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllText(path,text);
            try
            {
                var store=new FarmSaveStore(path,Restore);var f=store.Load(out _);Assert.That(f.SelectedCropId,Is.EqualTo("turnip"));Assert.That(f.Seeds("turnip"),Is.EqualTo(1));Assert.That(f.Seeds("carrot"),Is.Zero);
                Assert.That(f.Plot(0).watered,Is.True);store.Save(f.Snapshot());Assert.That(File.ReadAllText(path+".pre-v"+FarmModel.SaveVersion),Is.EqualTo(text));
                Assert.That(Json(store.Load(out _)),Is.EqualTo(Json(f)));
            }
            finally{Directory.Delete(Path.GetDirectoryName(path),true);}
        }
        [Test] public void UprootingRequiresHoeKeepsSoilAndDoesNotAwardHarvest()
        {
            var f=Fresh();int p=Plant(f,"tomato");f.Water(p,out _);f.AdvanceMinutes(5*1440);
            var before=Json(f);Assert.That(f.Uproot(p,out _),Is.False);Assert.That(Json(f),Is.EqualTo(before));
            f.Equip(FarmItem.Hoe);Assert.That(f.Uproot(p,out _),Is.True);Assert.That(f.PlotCount,Is.EqualTo(1));
            Assert.That(f.Produce("tomato"),Is.Zero);Assert.That(f.Seeds("tomato"),Is.Zero);Assert.That(f.Stage(p),Is.EqualTo(-1));
            Assert.That(f.Uproot(p,out _),Is.False);f.BuySeeds("carrot",1,out _);Assert.That(f.Plant(p,"carrot",out _),Is.True);
        }
        [Test] public void CorruptRegrowthSelectionAndCraftedItemsAreRejected()
        {
            var f=Fresh();int p=Plant(f,"turnip");var s=f.Snapshot();s.plots[p].regrowing=true;Assert.Throws<ArgumentException>(()=>Restore(s));
            s=f.Snapshot();s.selectedCropId="nope";Assert.Throws<ArgumentException>(()=>Restore(s));
            s=f.Snapshot();s.crafted.Single().count=-1;Assert.Throws<ArgumentException>(()=>Restore(s));
        }
    }
}
