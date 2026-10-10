using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
namespace Farmer.Tests
{
    public sealed class GardenBuildingTests
    {
        private static readonly CropRules[] Crops={new CropRules("turnip",10,18,3,1)};
        private static FarmModel Restore(FarmSnapshot s)=>FarmModel.Restore(s,Crops,6,6);
        private static FarmModel Fresh(int stone=10){var s=new FarmModel(Crops,worldSeed:12).Snapshot();s.stone=stone;return Restore(s);}
        private static string Json(FarmModel f)=>JsonUtility.ToJson(f.Snapshot());
        [Test] public void StoneDebitAndRefundAreAtomicAndNeverSpendWood()
        {
            var f=Fresh(2);int wood=f.Building.Wood;Assert.That(f.PlaceStructure("stone_path",20,0,20,0,out _),Is.True);Assert.That(f.Stone,Is.Zero);Assert.That(f.Building.Wood,Is.EqualTo(wood));
            string before=Json(f);Assert.That(f.PlaceStructure("stone_path",21,0,20,0,out _),Is.False);Assert.That(Json(f),Is.EqualTo(before));
            var path=f.Building.Blocks.Single();Assert.That(f.RemoveStructure(path,out _),Is.True);Assert.That(f.Stone,Is.EqualTo(2));Assert.That(f.RemoveStructure(path,out _),Is.False);Assert.That(f.Stone,Is.EqualTo(2));
        }
        [Test] public void FullStoneBagRejectsRemovalWithoutLosingThePath()
        {
            var f=Fresh();f.PlaceStructure("stone_path",20,0,20,0,out _);var s=f.Snapshot();s.stone=999;f=Restore(s);string before=Json(f);
            Assert.That(f.RemoveStructure(f.Building.Blocks.Single(),out _),Is.False);Assert.That(Json(f),Is.EqualTo(before));
        }
        [Test] public void MovingAnExistingPathNeedsNoResourcesAndPreservesIdentity()
        {
            var f=Fresh(2);f.PlaceStructure("stone_path",20,0,20,0,out _);var path=f.Building.Blocks.Single();
            Assert.That(f.Building.Move(path,21,0,20,1,out _),Is.True);Assert.That(f.Stone,Is.Zero);Assert.That(f.Building.Blocks.Single().instanceId,Is.EqualTo(path.instanceId));
            Assert.That(f.Building.Move(f.Building.Blocks.Single(),21,1,20,0,out _),Is.False);
        }
        [TestCase(0,20,21,2)] [TestCase(1,21,20,3)] [TestCase(2,20,19,0)] [TestCase(3,19,20,1)]
        public void GateAndFenceCannotDuplicateTheSameSharedEdge(int rotation,int x,int z,int opposite)
        {
            var f=Fresh();Assert.That(f.PlaceStructure("garden_fence",20,0,20,rotation,out _),Is.True);int wood=f.Building.Wood;
            Assert.That(f.PlaceStructure("garden_gate",x,0,z,opposite,out _),Is.False);Assert.That(f.Building.Wood,Is.EqualTo(wood));
        }
        [Test] public void GardenEdgesAllowCropsButPathsBlockAndRespectThem()
        {
            var f=Fresh();f.Till(20,20,out _);f.BuySeeds("turnip",1,out _);f.Plant(f.IndexAt(20,20),"turnip",out _);
            Assert.That(f.PlaceStructure("garden_fence",20,0,20,0,out _),Is.True);Assert.That(f.PlaceStructure("stone_path",20,0,20,0,out _),Is.False);
            f.Till(21,20,out _);Assert.That(f.PlaceStructure("stone_path",21,0,20,0,out _),Is.True);
            Assert.That(f.Till(21,20,out _),Is.False);Assert.That(f.Plant(f.IndexAt(21,20),"turnip",out _),Is.False);
            Assert.That(f.PlaceStructure("wood_floor",21,0,20,0,out _),Is.False);Assert.That(f.PlaceStructure("wood_block",21,0,20,0,out _),Is.False);
            Assert.That(f.PlaceStructure("wood_block",22,0,20,0,out _),Is.True);Assert.That(f.PlaceStructure("stone_path",22,0,20,0,out _),Is.False);
        }
        [Test] public void OutdoorEdgesDoNotSupportRoofsOrUpperFloors()
        {
            var f=Fresh();f.PlaceStructure("garden_fence",20,0,20,0,out _);
            Assert.That(f.PlaceStructure("wood_roof",20,2,20,0,out _),Is.False);Assert.That(f.PlaceStructure("garden_gate",21,1,20,0,out _),Is.False);Assert.That(f.PlaceStructure("stone_path",21,1,20,0,out _),Is.False);
        }
        [Test] public void GateStatePathsAndRemainingMaterialsRoundtrip()
        {
            var f=Fresh();f.PlaceStructure("garden_gate",20,0,20,0,out _);f.PlaceStructure("stone_path",20,0,20,0,out _);
            var gate=f.Building.Blocks.Single(b=>b.pieceId=="garden_gate");f.Building.ToggleDoor(gate,out _);string saved=Json(f);
            var restored=Restore(JsonUtility.FromJson<FarmSnapshot>(saved));Assert.That(Json(restored),Is.EqualTo(saved));Assert.That(restored.Building.DoorIsOpen(gate),Is.True);
        }
        [Test] public void V9SaveIsBackedUpBeforeFirstV10Write()
        {
            string dir=Path.Combine(Path.GetTempPath(),"FarmerQA-Garden-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);string path=Path.Combine(dir,"farm.json");
            try {var s=Fresh().Snapshot();s.version=9;string old=JsonUtility.ToJson(s);File.WriteAllText(path,old);var store=new FarmSaveStore(path,Restore);var f=store.Load(out _);Assert.That(f.Stone,Is.EqualTo(10));store.Save(f.Snapshot());Assert.That(File.ReadAllText(path+".pre-v10"),Is.EqualTo(old));Assert.That(store.Load(out _).Snapshot().version,Is.EqualTo(10));}
            finally {Directory.Delete(dir,true);}
        }
    }
}
