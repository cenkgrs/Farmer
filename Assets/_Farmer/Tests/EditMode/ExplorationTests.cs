using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
namespace Farmer.Tests
{
    public sealed class ExplorationTests
    {
        private static CropRules[] Crops=>new[]{new CropRules("turnip",10,18,3,1)};
        private static FarmModel Fresh(int seed=42)=>new FarmModel(Crops,worldSeed:seed);
        [Test] public void WorldsHaveSeededDistinctReachableResources()
        {
            var a=Fresh();var b=Fresh(99);Assert.That(JsonUtility.ToJson(a.Exploration.Snapshot()),Is.Not.EqualTo(JsonUtility.ToJson(b.Exploration.Snapshot())));
            Assert.That(JsonUtility.ToJson(a.Exploration.Snapshot()),Is.EqualTo(JsonUtility.ToJson(Fresh().Exploration.Snapshot())));
            Assert.That(a.Exploration.Nodes.Count(n=>n.kind==ResourceKind.Chest),Is.EqualTo(5));Assert.That(a.Exploration.Nodes.Count(n=>n.kind==ResourceKind.Tree),Is.EqualTo(24));
            var nodes=a.Exploration.Nodes.ToArray();
            foreach(var n in nodes){Assert.That(Math.Max(Math.Abs(n.x),Math.Abs(n.z)),Is.InRange(12,24));Assert.That(nodes.Count(m=>m.id!=n.id&&Math.Abs(m.x-n.x)<4&&Math.Abs(m.z-n.z)<4),Is.Zero);}
        }
        [Test] public void DefaultNewWorldsAreRandomAndAllWorkToolsAreOwnedWithoutSword()
        {
            var a=new FarmModel(Crops);var b=new FarmModel(Crops);Assert.That(a.Exploration.Seed,Is.Not.EqualTo(b.Exploration.Seed));
            foreach(var tool in new[]{FarmItem.Hoe,FarmItem.Axe,FarmItem.Sickle,FarmItem.WateringCan})Assert.That(a.ItemCount(tool,"turnip"),Is.EqualTo(1));
            Assert.That(Enum.GetNames(typeof(FarmItem)),Does.Not.Contain("Sword"));
        }
        [Test] public void TreeNeedsAxeAndThreeHitsThenPaysOnlyOnce()
        {
            var f=Fresh();var tree=f.Exploration.Nodes.First(n=>n.kind==ResourceKind.Tree);int wood=f.Building.Wood;
            Assert.That(f.Gather(tree.id,"turnip",out _),Is.False);f.Equip(FarmItem.Axe);
            Assert.That(f.Gather(tree.id,"turnip",out _),Is.True);Assert.That(f.Gather(tree.id,"turnip",out _),Is.True);Assert.That(f.Building.Wood,Is.EqualTo(wood));
            Assert.That(f.Gather(tree.id,"turnip",out _),Is.True);Assert.That(f.Building.Wood,Is.EqualTo(wood+8));Assert.That(f.Gather(tree.id,"turnip",out _),Is.False);
        }
        [Test] public void PlantNeedsSickleChestDoesNotNeedAWeapon()
        {
            var f=Fresh();var plant=f.Exploration.Nodes.First(n=>n.kind==ResourceKind.WildPlant);Assert.That(f.Gather(plant.id,"turnip",out _),Is.False);
            f.Equip(FarmItem.Sickle);Assert.That(f.Gather(plant.id,"turnip",out _),Is.True);Assert.That(f.Seeds("turnip"),Is.EqualTo(2));
            var chest=f.Exploration.Nodes.First(n=>n.kind==ResourceKind.Chest);int money=f.Money;Assert.That(f.Gather(chest.id,"turnip",out _),Is.True);Assert.That(f.Money,Is.EqualTo(money+chest.coins));Assert.That(f.Gather(chest.id,"turnip",out _),Is.False);
        }
        [Test] public void ReloadPreservesLootPositionsPartialChoppingAndCollectedFlags()
        {
            var f=Fresh();var tree=f.Exploration.Nodes.First(n=>n.kind==ResourceKind.Tree);f.Equip(FarmItem.Axe);f.Gather(tree.id,"turnip",out _);
            var chest=f.Exploration.Nodes.First(n=>n.kind==ResourceKind.Chest);f.Gather(chest.id,"turnip",out _);
            string json=JsonUtility.ToJson(f.Snapshot());var loaded=FarmModel.Restore(JsonUtility.FromJson<FarmSnapshot>(json),Crops,6,6);
            Assert.That(JsonUtility.ToJson(loaded.Snapshot()),Is.EqualTo(json));loaded.AdvanceMinutes(1440*10);Assert.That(loaded.Gather(chest.id,"turnip",out _),Is.False);Assert.That(loaded.Exploration.Node(tree.id).hits,Is.EqualTo(1));
        }
        [Test] public void FullInventoryDoesNotConsumeFinalResourceOrChest()
        {
            var f=Fresh();var s=f.Snapshot();s.money=FarmModel.MoneyLimit;s.building.wood=BuildingModel.WoodLimit;s.seeds[0].count=FarmModel.StackLimit;f=FarmModel.Restore(s,Crops,6,6);
            foreach(var kind in new[]{ResourceKind.Tree,ResourceKind.WildPlant,ResourceKind.Chest})
            {
                var n=f.Exploration.Nodes.First(x=>x.kind==kind);var rule=ResourceRules.For(kind);if(rule.Tool.HasValue)f.Equip(rule.Tool.Value);
                for(int i=1;i<rule.Hits;i++)Assert.That(f.Gather(n.id,"turnip",out _),Is.True);
                var before=JsonUtility.ToJson(f.Snapshot());Assert.That(f.Gather(n.id,"turnip",out _),Is.False);Assert.That(JsonUtility.ToJson(f.Snapshot()),Is.EqualTo(before));
            }
        }
        [Test] public void InvalidExplorationSavesAreRejected()
        {
            var s=Fresh().Snapshot();s.exploration.nodes[1].id=s.exploration.nodes[0].id;Assert.Throws<ArgumentException>(()=>FarmModel.Restore(s,Crops,6,6));
            s=Fresh().Snapshot();s.exploration.nodes[0].coins=100000;Assert.Throws<ArgumentException>(()=>FarmModel.Restore(s,Crops,6,6));
            s=Fresh().Snapshot();s.exploration.nodes[0].collected=true;Assert.Throws<ArgumentException>(()=>FarmModel.Restore(s,Crops,6,6));
            s=Fresh().Snapshot();s.exploration=null;Assert.Throws<ArgumentException>(()=>FarmModel.Restore(s,Crops,6,6));
        }
        [Test] public void LegacyWorldMigrationKeepsProgressAndAvoidsExistingStructures()
        {
            var f=Fresh();f.Till(12,12,out _);f.Building.Place("wood_block",16,0,16,0,out _);var s=f.Snapshot();s.version=5;s.day=45;s.money=1728;s.exploration=null;
            var a=FarmModel.Restore(s,Crops,6,6);var b=FarmModel.Restore(s,Crops,6,6);Assert.That(a.Day,Is.EqualTo(45));Assert.That(a.Money,Is.EqualTo(1728));Assert.That(a.Building.Count,Is.EqualTo(1));
            Assert.That(a.Exploration.Nodes.Any(n=>n.x==12&&n.z==12||n.x==16&&n.z==16),Is.False);Assert.That(JsonUtility.ToJson(a.Exploration.Snapshot()),Is.EqualTo(JsonUtility.ToJson(b.Exploration.Snapshot())));
        }
    }
}
