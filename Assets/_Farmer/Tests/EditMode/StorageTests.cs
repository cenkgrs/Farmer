using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
namespace Farmer.Tests
{
    public sealed class StorageTests
    {
        private static CropRules[] Crops=>new[]{new CropRules("turnip",10,18,3,1)};
        private static FarmModel Fresh()=>new FarmModel(Crops,startingMoney:1000);
        private static BlockRecord Chest(FarmModel f){Assert.That(f.BuyFurniture("home_chest",out _),Is.True);Assert.That(f.Building.Place("home_chest",0,0,0,0,out _),Is.True);return f.Building.Blocks.Single();}
        [TestCase("home_chest",80)] [TestCase("home_table",60)] [TestCase("home_chair",25)] [TestCase("home_lantern",45)]
        public void PurchasePlacePackReturnsFurnitureInsteadOfWood(string id,int price)
        {
            var f=Fresh();Assert.That(f.Building.Place(id,0,0,0,0,out _),Is.False);
            Assert.That(f.BuyFurniture(id,out _),Is.True);Assert.That(f.Money,Is.EqualTo(1000-price));
            Assert.That(f.Building.Place(id,0,0,0,0,out _),Is.True);Assert.That(f.Building.FurnitureCount(id),Is.Zero);Assert.That(f.Building.Wood,Is.EqualTo(24));
            Assert.That(f.Building.Remove(f.Building.Blocks.Single(),out _),Is.True);Assert.That(f.Building.FurnitureCount(id),Is.EqualTo(1));Assert.That(f.Building.Wood,Is.EqualTo(24));
        }
        [Test] public void TableOccupiesTwoCellsAndDoesNotSupportConstruction()
        {
            var f=Fresh();f.BuyFurniture("home_table",out _);Assert.That(f.Building.Place("home_table",0,0,0,0,out _),Is.True);
            Assert.That(f.Building.Place("wood_block",0,0,1,0,out _),Is.False);Assert.That(f.Building.Place("wood_block",0,1,0,0,out _),Is.False);
            Assert.That(f.Building.Place("wood_wall",0,0,0,0,out _),Is.False);
            Assert.That(f.Building.Move(f.Building.Blocks.Single(),3,0,3,1,out _),Is.True);Assert.That(f.Building.Occupied(4,0,3),Is.True);
        }
        [Test] public void FullChestMovesWithContentsAndCannotBeDiscarded()
        {
            var f=Fresh();var chest=Chest(f);Assert.That(f.TransferStorage(chest.instanceId,"wood",20,false,out _),Is.True);
            Assert.That(f.Building.Remove(chest,out _),Is.False);Assert.That(f.Building.Move(chest,3,0,4,1,out _),Is.True);
            Assert.That(f.Building.StoredItems(chest.instanceId).Single().count,Is.EqualTo(20));
            Assert.That(f.TransferStorage(chest.instanceId,"wood",20,true,out _),Is.True);Assert.That(f.Building.Wood,Is.EqualTo(24));
            Assert.That(f.Building.Remove(f.Building.FindStorage(chest.instanceId),out _),Is.True);
            Assert.That(f.TransferStorage(chest.instanceId,"wood",1,true,out _),Is.False);
        }
        [Test] public void FailedTransfersAreAtomicAndToolsStayWithPlayer()
        {
            var f=Fresh();var c=Chest(f);string before=JsonUtility.ToJson(f.Snapshot());
            Assert.That(f.TransferStorage(c.instanceId,"wood",25,false,out _),Is.False);Assert.That(f.TransferStorage(c.instanceId,"wood",1,true,out _),Is.False);
            Assert.That(f.TransferStorage(c.instanceId,"tool:axe",1,false,out _),Is.False);Assert.That(f.TransferStorage(c.instanceId,"wood",-1,false,out _),Is.False);
            Assert.That(f.TransferStorage("stale","wood",1,false,out _),Is.False);Assert.That(JsonUtility.ToJson(f.Snapshot()),Is.EqualTo(before));
            f.TransferStorage(c.instanceId,"wood",24,false,out _);
            var saved=f.Snapshot();saved.building.wood=999;var full=FarmModel.Restore(saved,Crops,6,6);before=JsonUtility.ToJson(full.Snapshot());
            Assert.That(full.TransferStorage(c.instanceId,"wood",1,true,out _),Is.False);Assert.That(JsonUtility.ToJson(full.Snapshot()),Is.EqualTo(before));
        }
        [Test] public void SeedsProduceAndFurnitureRoundTripWithoutDuplication()
        {
            var f=Fresh();var c=Chest(f);f.BuySeeds("turnip",3,out _);f.BuyFurniture("home_chair",out _);
            Assert.That(f.TransferStorage(c.instanceId,"seed:turnip",3,false,out _),Is.True);Assert.That(f.TransferStorage(c.instanceId,"furniture:home_chair",1,false,out _),Is.True);
            var restored=FarmModel.Restore(JsonUtility.FromJson<FarmSnapshot>(JsonUtility.ToJson(f.Snapshot())),Crops,6,6);
            Assert.That(restored.Seeds("turnip"),Is.Zero);Assert.That(restored.TransferStorage(c.instanceId,"seed:turnip",3,true,out _),Is.True);
            Assert.That(restored.TransferStorage(c.instanceId,"seed:turnip",3,true,out _),Is.False);Assert.That(restored.Seeds("turnip"),Is.EqualTo(3));
            Assert.That(restored.TransferStorage(c.instanceId,"furniture:home_chair",1,true,out _),Is.True);Assert.That(restored.Building.FurnitureCount("home_chair"),Is.EqualTo(1));
        }
        [Test] public void LegacyV6PreservesWorldAndCorruptStorageIsRejected()
        {
            var old=Fresh().Snapshot();old.version=6;old.building.furniture=null;var restored=FarmModel.Restore(old,Crops,6,6);
            Assert.That(restored.Money,Is.EqualTo(old.money));Assert.That(restored.Snapshot().version,Is.EqualTo(FarmModel.SaveVersion));Assert.That(restored.Exploration.Snapshot().seed,Is.EqualTo(old.exploration.seed));
            var f=Fresh();var c=Chest(f);var bad=f.Snapshot();bad.building.blocks[0].contents=new[]{new ItemStack{id="wood",count=1000}};
            Assert.Throws<ArgumentException>(()=>FarmModel.Restore(bad,Crops,6,6));bad.building.blocks[0].contents=new[]{new ItemStack{id="unknown",count=1}};Assert.Throws<ArgumentException>(()=>FarmModel.Restore(bad,Crops,6,6));
            bad=f.Snapshot();bad.building.blocks[0].instanceId=null;Assert.Throws<ArgumentException>(()=>FarmModel.Restore(bad,Crops,6,6));
        }
    }
}
