using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
namespace Farmer.Tests
{
    public sealed class RoofMoveTests
    {
        private static CropRules[] Crops=>new[]{new CropRules("turnip",10,18,3,1)};
        private static BuildingModel Fresh()=>new BuildingModel(BuildRules.Defaults,200,2);
        [Test] public void NewGameHasFiftyCoinsAndNoFreeBed()
        {
            var f=new FarmModel(Crops);Assert.That(f.Money,Is.EqualTo(50));Assert.That(f.Building.Beds,Is.Zero);Assert.That(f.Building.Count,Is.Zero);
            Assert.That(f.Building.Place("bed",0,0,0,0,out _),Is.False);Assert.That(f.BuyBed(out _),Is.False);Assert.That(f.Money,Is.EqualTo(50));
        }
        [Test] public void BedCostsMoneyOnceAndCanBePackedAndPlacedWithoutWood()
        {
            var f=new FarmModel(Crops,startingMoney:150);Assert.That(f.BuyBed(out _),Is.True);Assert.That(f.Money,Is.EqualTo(50));int wood=f.Building.Wood;
            Assert.That(f.Building.Place("bed",0,0,0,0,out _),Is.True);Assert.That(f.Building.Beds,Is.Zero);Assert.That(f.Building.Remove(0,0,0,out _),Is.True);
            Assert.That(f.Building.Beds,Is.EqualTo(1));Assert.That(f.Building.Wood,Is.EqualTo(wood));Assert.That(f.Building.Place("bed",4,0,4,1,out _),Is.True);
        }
        [Test] public void RoofSpansAnEmptyRoomButNotWithoutAnAnchor()
        {
            var b=Fresh();Assert.That(b.Place("wood_roof",0,3,0,0,out _),Is.False);
            for(int y=0;y<3;y++)b.Place("wood_block",-1,y,0,0,out _);
            Assert.That(b.HasDirectRoofSupport(0,3,0),Is.True);
            Assert.That(b.HasDirectRoofSupport(0,2,0),Is.False);
            Assert.That(b.Place("wood_roof",0,3,0,0,out _),Is.True);
            Assert.That(b.HasDirectRoofSupport(1,3,0),Is.False);
            Assert.That(b.CanSupportRoof(1,3,0),Is.True);
            for(int x=1;x<=4;x++)Assert.That(b.Place("wood_roof",x,3,0,0,out _),Is.True);
            Assert.That(b.Place("wood_roof",5,3,0,0,out _),Is.False);
            Assert.That(b.Remove(-1,2,0,out _),Is.False);
            var last=b.Blocks.Single(r=>r.pieceId=="wood_roof"&&r.x==4);Assert.That(b.Remove(last,out _),Is.True);
            Assert.That(b.Move(b.Blocks.Single(r=>r.pieceId=="wood_roof"&&r.x==0),0,3,1,0,out _),Is.False);
        }
        [Test] public void ThinWallRoofAndSupportSurviveUnorderedSave()
        {
            var b=Fresh();b.Place("wood_wall",0,0,0,0,out _);Assert.That(b.Place("wood_roof",0,2,0,0,out _),Is.True);
            b.Place("wood_roof",1,2,0,0,out _);var s=b.Snapshot();Array.Reverse(s.blocks);
            Assert.That(BuildingModel.Restore(s,BuildRules.Defaults).Count,Is.EqualTo(3));
            s.blocks=s.blocks.Where(r=>r.pieceId!="wood_wall").ToArray();Assert.Throws<ArgumentException>(()=>BuildingModel.Restore(s,BuildRules.Defaults));
        }
        [TestCase("wood_wall",0)] [TestCase("wood_wall",1)]
        [TestCase("wood_wall",2)] [TestCase("wood_wall",3)]
        [TestCase("wood_door",0)] [TestCase("wood_door",1)]
        [TestCase("wood_door",2)] [TestCase("wood_door",3)]
        public void RoofSupportsBothSidesOfEveryWallAndDoor(string id,int rotation)
        {
            var b=Fresh();Assert.That(b.Place(id,0,0,0,rotation,out _),Is.True);
            int x=rotation==1?1:rotation==3?-1:0, z=rotation==0?1:rotation==2?-1:0;
            Assert.That(b.HasDirectRoofSupport(0,2,0),Is.True);
            Assert.That(b.CanSupportRoof(0,2,0),Is.True);
            Assert.That(b.CanSupportRoof(x,2,z),Is.True);
            Assert.That(b.CanSupportRoof(0,3,0),Is.False);
            Assert.That(b.Place("wood_roof",x,2,z,0,out _),Is.True);
            Assert.That(b.CanSupportRoof(x,2,z),Is.True);
            Assert.That(b.Remove(b.Blocks.Single(r=>r.pieceId==id),out _),Is.False);
        }
        [Test] public void MoveIsAtomicAndPreservesWoodBedsAndDoorState()
        {
            var b=Fresh();b.Place("bed",0,0,0,0,out _);b.Place("wood_block",4,0,4,0,out _);var bed=b.Blocks.Single(r=>r.pieceId=="bed");string before=JsonUtility.ToJson(b.Snapshot());
            Assert.That(b.Move(bed,4,0,4,1,out _),Is.False);Assert.That(JsonUtility.ToJson(b.Snapshot()),Is.EqualTo(before));
            int wood=b.Wood,beds=b.Beds;Assert.That(b.Move(bed,2,0,2,1,out _),Is.True);Assert.That(b.Occupied(0,0,0),Is.False);Assert.That(b.Occupied(3,0,2),Is.True);Assert.That(b.Wood,Is.EqualTo(wood));Assert.That(b.Beds,Is.EqualTo(beds));
            b.Place("wood_door",-3,0,0,0,out _);var door=b.Blocks.Single(r=>r.pieceId=="wood_door");b.ToggleDoor(door,out _);Assert.That(b.Move(door,-4,0,0,2,out _),Is.True);Assert.That(b.Blocks.Single(r=>r.pieceId=="wood_door").doorOpen,Is.True);
        }
        [Test] public void ExistingStarterBedMigratesOnceWithoutResettingProgress()
        {
            var f=new FarmModel(Crops,startingMoney:1728);f.AdvanceMinutes(44*1440);f.Building.Place("wood_block",4,0,4,0,out _);var s=f.Snapshot();s.version=4;
            var loaded=FarmModel.Restore(s,Crops,6,6);Assert.That(loaded.Money,Is.EqualTo(1728));Assert.That(loaded.Day,Is.EqualTo(45));Assert.That(loaded.Building.Blocks.Count(r=>r.pieceId=="bed"),Is.EqualTo(1));
            var twice=FarmModel.Restore(loaded.Snapshot(),Crops,6,6);Assert.That(JsonUtility.ToJson(twice.Snapshot()),Is.EqualTo(JsonUtility.ToJson(loaded.Snapshot())));
        }
        [Test] public void BlockedLegacyBedGoesIntoInventoryWithoutOverwritingStructure()
        {
            var f=new FarmModel(Crops);f.Building.Place("wood_block",-6,0,-4,0,out _);var s=f.Snapshot();s.version=4;
            var restored=FarmModel.Restore(s,Crops,6,6);Assert.That(restored.Building.Count,Is.EqualTo(1));Assert.That(restored.Building.Beds,Is.EqualTo(1));
        }
    }
}
