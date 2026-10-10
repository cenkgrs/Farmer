using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace Farmer.Tests
{
    public sealed class HarvestCropTests
    {
        private static CropRules[] Crops=>new[]{AssetDatabase.LoadAssetAtPath<CropDefinition>("Assets/_Farmer/Data/Turnip.asset").Rules}.Concat(Resources.LoadAll<CropDefinition>("Crops").Select(c=>c.Rules)).ToArray();
        private static RecipeRules[] Recipes=>Resources.LoadAll<RecipeDefinition>("Recipes").Select(r=>r.Rules).ToArray();
        private static FarmModel Fresh(int money=500)=>new FarmModel(Crops,startingMoney:money,worldSeed:12,recipeCatalog:Recipes);
        private static FarmModel Restore(FarmSnapshot s)=>FarmModel.Restore(s,Crops,6,6,recipeCatalog:Recipes);
        private static string Json(FarmModel f)=>JsonUtility.ToJson(f.Snapshot());
        private static int Plant(FarmModel f,string id,int x=20){Assert.That(f.BuySeeds(id,1,out _),Is.True);Assert.That(f.Till(x,20,out _),Is.True);int p=f.IndexAt(x,20);Assert.That(f.Plant(p,id,out _),Is.True);return p;}
        [TestCase("lettuce",2,1,6,10)] [TestCase("wheat",3,3,12,7)] [TestCase("pumpkin",8,1,40,85)]
        public void DeliveredDefinitionsGrowYieldAndSellAtTheirOwnPace(string id,int days,int yield,int seedPrice,int salePrice)
        {
            var f=Fresh();int p=Plant(f,id);Assert.That(f.Money,Is.EqualTo(500-seedPrice));f.AdvanceMinutes(days*1440);Assert.That(f.IsReady(p),Is.False,"Water is required.");
            f.Water(p,out _);f.AdvanceMinutes((days-1)*1440);Assert.That(f.IsReady(p),Is.False);f.AdvanceMinutes(1440);
            Assert.That(f.Stage(p),Is.EqualTo(3));Assert.That(f.Harvest(p,out _),Is.True);Assert.That(f.Produce(id),Is.EqualTo(yield));Assert.That(f.Plot(p).cropId,Is.Empty);Assert.That(f.Harvest(p,out _),Is.False);
            Assert.That(f.Sell(id,yield,out _),Is.True);Assert.That(f.Money,Is.EqualTo(500-seedPrice+yield*salePrice));
        }
        [Test] public void LettuceCreatesAnIncomeBeforeTwentyMinutesWithoutChangingTurnip()
        {
            var f=Fresh(50);int p=Plant(f,"lettuce");f.Water(p,out _);f.AdvanceMinutes(2519);Assert.That(f.IsReady(p),Is.False);f.AdvanceMinutes(1);Assert.That(f.Harvest(p,out _),Is.True);f.Sell("lettuce",1,out _);
            Assert.That(f.Money,Is.EqualTo(54));Assert.That(Crops.Single(c=>c.Id=="turnip").WateredDays,Is.EqualTo(3));
        }
        [Test] public void WheatRejectsPartialHarvestWhenTheStackWouldOverflow()
        {
            var f=Fresh();int p=Plant(f,"wheat");f.Water(p,out _);f.AdvanceMinutes(3*1440);var s=f.Snapshot();s.produce.Single(i=>i.cropId=="wheat").count=998;f=Restore(s);string before=Json(f);
            Assert.That(f.Harvest(p,out _),Is.False);Assert.That(Json(f),Is.EqualTo(before));
        }
        [Test] public void NewPlantsAndSelectedSeedsSurviveMidGrowthReload()
        {
            var f=Fresh();for(int i=0;i<3;i++){int p=Plant(f,new[]{"lettuce","wheat","pumpkin"}[i],20+i);f.Water(p,out _);}f.SelectCrop("pumpkin");f.AdvanceMinutes(1440);string json=Json(f);var loaded=Restore(JsonUtility.FromJson<FarmSnapshot>(json));
            Assert.That(Json(loaded),Is.EqualTo(json));loaded.AdvanceMinutes(1440);Assert.That(loaded.IsReady(loaded.IndexAt(20,20)),Is.True);Assert.That(loaded.IsReady(loaded.IndexAt(22,20)),Is.False);
        }
        private static FarmModel Stocked()
        {var s=Fresh().Snapshot();foreach(var id in new[]{"lettuce","wheat","pumpkin"})s.produce.Single(p=>p.cropId==id).count=id=="wheat"?6:2;return Restore(s);}
        [Test] public void HarvestBasketConsumesAllThreeCropsAndSellsWithAddedValue()
        {
            var f=Stocked();int wood=f.Building.Wood;Assert.That(f.Craft("harvest_basket",2,out _),Is.True);Assert.That(f.Building.Wood,Is.EqualTo(wood-4));Assert.That(f.BagCount("crafted:harvest_basket"),Is.EqualTo(2));
            foreach(string id in new[]{"lettuce","wheat","pumpkin"})Assert.That(f.Produce(id),Is.Zero);
            Assert.That(f.SellCrafted("harvest_basket",2,out _),Is.True);Assert.That(f.Money,Is.EqualTo(790));
        }
        [TestCase(false)] [TestCase(true)] public void BasketMissingIngredientOrFullOutputNeverConsumesInputs(bool fullOutput)
        {
            var s=Stocked().Snapshot();if(fullOutput)s.crafted.Single(c=>c.id=="harvest_basket").count=999;else s.produce.Single(c=>c.cropId=="wheat").count=2;
            var f=Restore(s);string before=Json(f);Assert.That(f.Craft("harvest_basket",1,out _),Is.False);Assert.That(Json(f),Is.EqualTo(before));
        }
        [Test] public void VersionTenUpgradePreservesOldCropsGardenAndStorageAndAddsEmptyInventories()
        {
            var oldCrops=Crops.Where(c=>new[]{"turnip","carrot","tomato"}.Contains(c.Id));var oldRecipes=Recipes.Where(r=>r.Id=="vegetable_crate");var old=new FarmModel(oldCrops,startingMoney:500,worldSeed:12,recipeCatalog:oldRecipes);
            int p=Plant(old,"tomato");old.Water(p,out _);old.AdvanceMinutes(1440);old.SelectCrop("tomato");old.PlaceStructure("garden_gate",25,0,25,0,out _);old.Building.ToggleDoor(old.Building.Blocks.Single(),out _);
            old.BuyFurniture("home_chest",out _);old.PlaceStructure("home_chest",28,0,28,0,out _);string chest=old.Building.Blocks.Single(b=>b.pieceId=="home_chest").instanceId;old.TransferStorage(chest,"wood",3,false,out _);
            var s=old.Snapshot();s.version=10;s.stone=37;string raw=JsonUtility.ToJson(s);string dir=Path.Combine(Path.GetTempPath(),"FarmerQA-Harvest-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);string path=Path.Combine(dir,"farm.json");File.WriteAllText(path,raw);
            try {var store=new FarmSaveStore(path,Restore);var f=store.Load(out _);Assert.That(f.Money,Is.EqualTo(old.Money));Assert.That(f.Stone,Is.EqualTo(37));Assert.That(f.SelectedCropId,Is.EqualTo("tomato"));Assert.That(f.Plot(p).growth,Is.EqualTo(1));Assert.That(f.Building.Blocks.Single(b=>b.pieceId=="garden_gate").doorOpen,Is.True);Assert.That(f.Building.StoredItems(chest).Single().count,Is.EqualTo(3));foreach(string id in new[]{"lettuce","wheat","pumpkin"}){Assert.That(f.Seeds(id),Is.Zero);Assert.That(f.Produce(id),Is.Zero);}Assert.That(f.BagCount("crafted:harvest_basket"),Is.Zero);store.Save(f.Snapshot());Assert.That(File.ReadAllText(path+".pre-v11"),Is.EqualTo(raw));Assert.That(Json(store.Load(out _)),Is.EqualTo(Json(f)));}
            finally {Directory.Delete(dir,true);}
        }
        [TestCase(0)] [TestCase(-1)] [TestCase(101)] public void InvalidGrowthDaysRemainRejected(int days)=>Assert.Throws<ArgumentException>(()=>new CropRules("invalid",1,1,days,1));
    }
}
