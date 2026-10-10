using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Farmer.Tests
{
    public sealed class MiningTests
    {
        private static readonly CropRules[] Crops={new CropRules("turnip",10,18,3,1)};
        private static FarmModel Fresh(int money=200)=>new FarmModel(Crops,startingMoney:money,worldSeed:42);
        private static FarmModel Restore(FarmSnapshot s)=>FarmModel.Restore(s,Crops,6,6);
        private static string Json(FarmModel f)=>JsonUtility.ToJson(f.Snapshot());
        [Test] public void PickaxeMustBePurchasedAndCannotBeBoughtTwice()
        {
            var f=Fresh();Assert.That(f.OwnsPickaxe,Is.False);Assert.That(f.ItemCount(FarmItem.Pickaxe,"turnip"),Is.Zero);
            Assert.That(f.Equip(FarmItem.Pickaxe),Is.False);Assert.That(f.BuyPickaxe(out _),Is.True);
            Assert.That(f.Money,Is.EqualTo(200-FarmModel.PickaxePrice));Assert.That(f.Equip(FarmItem.Pickaxe),Is.True);
            var saved=Json(f);Assert.That(f.BuyPickaxe(out _),Is.False);Assert.That(Json(f),Is.EqualTo(saved));
        }
        [Test] public void InsufficientMoneyLeavesInventoryAndWalletUntouched()
        {
            var f=Fresh(FarmModel.PickaxePrice-1);var before=Json(f);
            Assert.That(f.BuyPickaxe(out _),Is.False);Assert.That(Json(f),Is.EqualTo(before));
        }
        [Test] public void OnlyOwnedEquippedPickaxeBreaksRockAndPaysOnce()
        {
            var f=Fresh();var rock=f.Exploration.Nodes.First(n=>n.kind==ResourceKind.Stone);
            f.Equip(FarmItem.Axe);var before=Json(f);Assert.That(f.Gather(rock.id,"turnip",out _),Is.False);Assert.That(Json(f),Is.EqualTo(before));
            f.BuyPickaxe(out _);f.Equip(FarmItem.Pickaxe);
            Assert.That(f.Gather(rock.id,"turnip",out _),Is.True);Assert.That(f.Gather(rock.id,"turnip",out _),Is.True);
            Assert.That(f.Stone,Is.Zero);Assert.That(f.Gather(rock.id,"turnip",out _),Is.True);Assert.That(f.Stone,Is.EqualTo(6));
            Assert.That(f.Exploration.Node(rock.id).collected,Is.True);Assert.That(f.Gather(rock.id,"turnip",out _),Is.False);Assert.That(f.Stone,Is.EqualTo(6));
            Assert.That(f.Gather(f.Exploration.Nodes.First(n=>n.kind==ResourceKind.Tree).id,"turnip",out _),Is.False);
        }
        [Test] public void FullStoneStackDoesNotConsumeFinalHit()
        {
            var f=Fresh();f.BuyPickaxe(out _);f.Equip(FarmItem.Pickaxe);var s=f.Snapshot();s.stone=FarmModel.StackLimit-5;f=Restore(s);
            var rock=f.Exploration.Nodes.First(n=>n.kind==ResourceKind.Stone);f.Gather(rock.id,"turnip",out _);f.Gather(rock.id,"turnip",out _);
            var before=Json(f);Assert.That(f.Gather(rock.id,"turnip",out _),Is.False);Assert.That(Json(f),Is.EqualTo(before));
        }
        [Test] public void OwnershipStoneAndPartialRockHitsSurviveJsonRoundTrip()
        {
            var f=Fresh();f.BuyPickaxe(out _);f.Equip(FarmItem.Pickaxe);var rocks=f.Exploration.Nodes.Where(n=>n.kind==ResourceKind.Stone).Take(2).ToArray();
            for(int i=0;i<3;i++)f.Gather(rocks[0].id,"turnip",out _);f.Gather(rocks[1].id,"turnip",out _);
            var loaded=Restore(JsonUtility.FromJson<FarmSnapshot>(Json(f)));Assert.That(Json(loaded),Is.EqualTo(Json(f)));
            Assert.That(loaded.OwnsPickaxe,Is.True);Assert.That(loaded.Stone,Is.EqualTo(6));Assert.That(loaded.Exploration.Node(rocks[1].id).hits,Is.EqualTo(1));
        }
        [Test] public void StoneTransfersToChestAndBackWithoutDuplication()
        {
            var f=Fresh(500);var s=f.Snapshot();s.stone=23;f=Restore(s);
            f.BuyFurniture("home_chest",out _);f.Building.Place("home_chest",0,0,0,0,out _);var chest=f.Building.Blocks.Single().instanceId;
            Assert.That(f.TransferStorage(chest,"stone",23,false,out _),Is.True);Assert.That(f.Stone,Is.Zero);
            f=Restore(JsonUtility.FromJson<FarmSnapshot>(Json(f)));Assert.That(f.Building.StoredItems(chest).Single().count,Is.EqualTo(23));
            Assert.That(f.TransferStorage(chest,"stone",23,true,out _),Is.True);Assert.That(f.Stone,Is.EqualTo(23));
            Assert.That(f.TransferStorage(chest,"stone",1,true,out _),Is.False);
            f.BuyPickaxe(out _);Assert.That(f.TransferStorage(chest,"tool:pickaxe",1,false,out _),Is.False);
        }
        private static FarmSnapshot Legacy()
        {
            var f=Fresh();f.Equip(FarmItem.Axe);var tree=f.Exploration.Nodes.First(n=>n.kind==ResourceKind.Tree);f.Gather(tree.id,"turnip",out _);
            f.Gather(f.Exploration.Nodes.First(n=>n.kind==ResourceKind.Chest).id,"turnip",out _);
            var s=f.Snapshot();s.version=7;s.exploration.generation=0;s.exploration.nodes=s.exploration.nodes.Where(n=>n.id<=41).ToArray();
            return s;
        }
        [Test] public void LegacyExpansionPreservesEveryOriginalNodeAndRunsOnce()
        {
            var s=Legacy();var f=Restore(s);
            foreach(var node in s.exploration.nodes)Assert.That(JsonUtility.ToJson(f.Exploration.Node(node.id)),Is.EqualTo(JsonUtility.ToJson(node)));
            Assert.That(f.Exploration.Nodes.Count(n=>n.kind==ResourceKind.Tree),Is.EqualTo(168));
            Assert.That(f.Exploration.Nodes.Count(n=>n.kind==ResourceKind.Stone),Is.EqualTo(48));
            Assert.That(f.Money,Is.EqualTo(s.money));Assert.That(f.OwnsPickaxe,Is.False);
            Assert.That(Json(Restore(f.Snapshot())),Is.EqualTo(Json(f)));Assert.That(Json(Restore(s)),Is.EqualTo(Json(f)));
        }
        [Test] public void ExpansionAvoidsFarmBuildingsRoadVillageAndAuthoredScenery()
        {
            var s=Legacy();var target=Restore(s).Exploration.Nodes.First(n=>n.id>41);
            var f=Fresh();f.Till(target.x,target.z,out _);s.plots=f.Snapshot().plots;
            Assert.That(Restore(s).Exploration.Nodes.Any(n=>n.x==target.x&&n.z==target.z),Is.False);
            s.plots=Array.Empty<PlotRecord>();f.Building.Place("wood_block",target.x,0,target.z,0,out _);s.building=f.Building.Snapshot();
            Assert.That(Restore(s).Exploration.Nodes.Any(n=>n.x==target.x&&n.z==target.z),Is.False);
            var blocked=FarmModel.Restore(Legacy(),Crops,6,6,resourceBlocked:(x,z)=>x>=0);
            Assert.That(blocked.Exploration.Nodes.Where(n=>n.id>41).All(n=>n.x<0),Is.True);
            Assert.That(blocked.Exploration.Nodes.Any(n=>n.x>=-78&&n.x<=-30&&n.z>=80&&n.z<=122),Is.False);
        }
        [Test] public void NewWorldResourcesStayWithinBoundsAndLeaveWalkingGaps()
        {
            var nodes=Fresh().Exploration.Nodes.ToArray();Assert.That(nodes.Length,Is.EqualTo(233));
            foreach(var n in nodes)
            {
                Assert.That(Math.Max(Math.Abs(n.x),Math.Abs(n.z)),Is.InRange(12,108));
                Assert.That(nodes.Count(m=>m.id!=n.id&&Math.Abs(m.x-n.x)<4&&Math.Abs(m.z-n.z)<4),Is.Zero);
            }
        }
        [TestCase(-1)] [TestCase(1000)] public void InvalidStoneCountsAreRejected(int stone)
        {var s=Fresh().Snapshot();s.stone=stone;Assert.Throws<ArgumentException>(()=>Restore(s));}
        [Test] public void UnownedEquippedPickaxeAndFutureGenerationAreRejected()
        {
            var s=Fresh().Snapshot();s.equippedItem=FarmItem.Pickaxe;Assert.Throws<ArgumentException>(()=>Restore(s));
            s=Fresh().Snapshot();s.exploration.generation=2;Assert.Throws<ArgumentException>(()=>Restore(s));
        }
        [Test] public void UpgradeKeepsOriginalV7FileBeforeWritingCurrentVersion()
        {
            string directory=Path.Combine(Path.GetTempPath(),"FarmerQA-Mining-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(directory);
            string path=Path.Combine(directory,"farm.json"),old=JsonUtility.ToJson(Legacy());File.WriteAllText(path,old);
            try
            {
                var store=new FarmSaveStore(path,Restore);var f=store.Load(out _);store.Save(f.Snapshot());store.Save(f.Snapshot());
                Assert.That(File.ReadAllText(path+".pre-v"+FarmModel.SaveVersion),Is.EqualTo(old));Assert.That(Json(store.Load(out _)),Is.EqualTo(Json(f)));
            }
            finally {Directory.Delete(directory,true);}
        }
    }
}
