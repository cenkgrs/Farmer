using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Farmer.Tests
{
    public sealed class HouseTests
    {
        private static BuildingModel Fresh()=>new BuildingModel(BuildRules.Defaults,100);
        [Test] public void FloorBedAndPerimeterWallsShareCellsAndRefundOnlyTheHitPiece()
        {
            var b=Fresh();Assert.That(b.Place("wood_floor",0,0,0,0,out _),Is.True);
            Assert.That(b.Place("wood_floor",0,0,1,0,out _),Is.True);
            Assert.That(b.Place("bed",0,0,0,0,out _),Is.True);
            Assert.That(b.Place("wood_wall",0,0,0,3,out _),Is.True);
            Assert.That(b.Place("wood_wall",0,0,0,2,out _),Is.True);
            int wood=b.Wood;var wall=b.Blocks.Single(p=>p.pieceId=="wood_wall"&&p.rotation==3);
            Assert.That(b.Remove(wall,out _),Is.True);Assert.That(b.Wood,Is.EqualTo(wood+2));
            Assert.That(b.HasFloor(0,0),Is.True);Assert.That(b.Blocks.Count(p=>p.pieceId=="bed"),Is.EqualTo(1));
        }
        [TestCase(0,0,1,2)] [TestCase(1,1,0,3)] [TestCase(2,0,-1,0)] [TestCase(3,-1,0,1)]
        public void SharedEdgeCannotBeBuiltAgainFromItsOtherSide(int r,int x,int z,int other)
        {
            var b=Fresh();b.Place("wood_wall",0,0,0,r,out _);int wood=b.Wood;
            Assert.That(b.Place("wood_door",x,0,z,other,out _),Is.False);Assert.That(b.Wood,Is.EqualTo(wood));
            Assert.That(b.Place("wood_wall",0,0,0,(r+1)%4,out _),Is.True);
        }
        [Test] public void WallCannotDivideBedRegardlessOfPlacementOrder()
        {
            var b=Fresh();b.Place("bed",0,0,0,0,out _);Assert.That(b.Place("wood_wall",0,0,0,0,out _),Is.False);
            b=Fresh();b.Place("wood_wall",0,0,0,0,out _);Assert.That(b.Place("bed",0,0,0,0,out _),Is.False);
        }
        [Test] public void FloorDoesNotSupportUpperBlocksAndBlocksFarming()
        {
            var f=new FarmModel(new[]{new CropRules("turnip",10,18,3,1)});f.Building.Place("wood_floor",0,0,0,0,out _);
            Assert.That(f.Building.Place("wood_block",0,1,0,0,out _),Is.False);Assert.That(f.Till(0,0,out _),Is.False);
            Assert.That(f.Building.Place("wood_wall",0,1,0,0,out _),Is.False);
            f.Till(1,0,out _);f.BuySeeds("turnip",1,out _);f.Building.Place("wood_floor",1,0,0,0,out _);
            Assert.That(f.Plant(f.IndexAt(1,0),"turnip",out _),Is.False);Assert.That(f.Seeds("turnip"),Is.EqualTo(1));
        }
        [Test] public void DoorStateAndLayeredStructuresSurviveRoundtrip()
        {
            var b=Fresh();b.Place("wood_floor",0,0,0,0,out _);b.Place("wood_door",0,0,0,0,out _);var door=b.Blocks.Single(x=>x.pieceId=="wood_door");
            Assert.That(b.ToggleDoor(door,out _),Is.True);var saved=JsonUtility.ToJson(b.Snapshot());var loaded=BuildingModel.Restore(JsonUtility.FromJson<BuildingSnapshot>(saved),BuildRules.Defaults);
            Assert.That(loaded.DoorIsOpen(door),Is.True);Assert.That(JsonUtility.ToJson(loaded.Snapshot()),Is.EqualTo(saved));
            Assert.That(loaded.ToggleDoor(door,out _),Is.True);Assert.That(loaded.DoorIsOpen(door),Is.False);
        }
        [Test] public void CorruptSharedEdgeAndDoorFlagsAreRejected()
        {
            var b=Fresh();b.Place("wood_wall",0,0,0,0,out _);var s=b.Snapshot();s.blocks[0].doorOpen=true;
            Assert.Throws<ArgumentException>(()=>BuildingModel.Restore(s,BuildRules.Defaults));s.blocks[0].doorOpen=false;
            s.blocks=new[]{s.blocks[0],new BlockRecord{pieceId="wood_door",x=0,z=1,rotation=2}};
            Assert.Throws<ArgumentException>(()=>BuildingModel.Restore(s,BuildRules.Defaults));
        }
        [Test] public void V3WorldSaveKeepsClockCropsAndExistingBuildings()
        {
            var crops=new[]{new CropRules("turnip",10,18,3,1)};var f=new FarmModel(crops);f.Till(-5,4,out _);f.Building.Place("bed",2,0,3,2,out _);f.AdvanceMinutes(71.25);
            var old=f.Snapshot();old.version=3;var restored=FarmModel.Restore(old,crops,6,6);
            Assert.That(restored.MinuteOfDay,Is.EqualTo(f.MinuteOfDay));Assert.That(restored.IndexAt(-5,4),Is.EqualTo(0));Assert.That(restored.Building.Count,Is.EqualTo(1));Assert.That(restored.Snapshot().version,Is.EqualTo(4));
        }
    }
}
