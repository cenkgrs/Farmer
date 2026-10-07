using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Farmer.Tests
{
    public sealed class WorldTests
    {
        private static CropRules[] Crops => new[] { new CropRules("turnip",10,18,3,1) };
        private static FarmModel Fresh() => new FarmModel(Crops);
        private static FarmModel Reload(FarmModel m) => FarmModel.Restore(JsonUtility.FromJson<FarmSnapshot>(JsonUtility.ToJson(m.Snapshot())),Crops,6,6);
        [Test] public void NewWorldStartsWithUntilledGroundAndHoeDoesNotSpendSeeds()
        {
            var m=Fresh(); m.BuySeeds("turnip",2,out _);
            Assert.That(m.PlotCount,Is.Zero); Assert.That(m.Plant(-1,"turnip",out _),Is.False);
            Assert.That(m.Till(-22,45,out _),Is.True); Assert.That(m.Till(-22,45,out _),Is.False);
            Assert.That(m.PlotCount,Is.EqualTo(1)); Assert.That(m.Seeds("turnip"),Is.EqualTo(2));
            m.Equip(FarmItem.Hoe); Assert.That(m.ItemCount(FarmItem.Hoe,"turnip"),Is.EqualTo(1));
            var loaded=Reload(m); Assert.That(loaded.IndexAt(-22,45),Is.EqualTo(0));
            Assert.That(loaded.EquippedItem,Is.EqualTo(FarmItem.Hoe));
            Assert.That(loaded.Plant(0,"turnip",out _),Is.True); loaded.Water(0,out _);
            loaded.AdvanceMinutes(3*1440); Assert.That(loaded.Harvest(0,out _),Is.True);
            Assert.That(loaded.IndexAt(-22,45),Is.EqualTo(0)); Assert.That(loaded.Plant(0,"turnip",out _),Is.True);
        }
        [Test] public void WorldBuildingHasNoFarmBoundaryAndBedHasRotatedFootprint()
        {
            var b=Fresh().Building;
            Assert.That(b.Place("wood_block",-25,0,45,0,out _),Is.True);
            Assert.That(b.Place("wood_block",-25,1,45,0,out _),Is.True);
            Assert.That(b.Place("bed",52,0,-20,1,out _),Is.True);
            Assert.That(b.Occupied(53,0,-20),Is.True);
            Assert.That(b.Place("wood_block",53,0,-20,0,out _),Is.False);
            Assert.That(b.Place("wood_block",52,1,-20,0,out _),Is.False);
            Assert.That(b.Place("bed",-25,2,45,0,out _),Is.False);
            int wood=b.Wood; Assert.That(b.Remove(52,0,-20,out _),Is.True);
            Assert.That(b.Wood,Is.EqualTo(wood+8)); Assert.That(b.Occupied(53,0,-20),Is.False);
        }
        [Test] public void MidnightGrowsOnceAndAfterMidnightSleepDoesNotGrowAgain()
        {
            var m=Fresh(); m.Till(12,-17,out _); m.BuySeeds("turnip",1,out _); m.Plant(0,"turnip",out _); m.Water(0,out _);
            m.AdvanceMinutes(1079); Assert.That(m.Day,Is.EqualTo(1)); Assert.That(m.Plot(0).growth,Is.Zero);
            m.AdvanceMinutes(1); Assert.That(m.ClockText,Is.EqualTo("00:00")); Assert.That(m.Day,Is.EqualTo(2)); Assert.That(m.Plot(0).growth,Is.EqualTo(1));
            m.AdvanceMinutes(60); m.EndDay(out _);
            Assert.That(m.ClockText,Is.EqualTo("06:00")); Assert.That(m.Day,Is.EqualTo(2)); Assert.That(m.Plot(0).growth,Is.EqualTo(1));
            m.EndDay(out _); Assert.That(m.Day,Is.EqualTo(3)); Assert.That(m.Plot(0).growth,Is.EqualTo(2));
            m.AdvanceMinutes(1440); Assert.That(m.IsReady(0),Is.True);
        }
        [Test] public void ClockAndWorldStateRoundTripWithoutOfflineAdvance()
        {
            var m=Fresh();m.Till(-80,22,out _);m.Building.Place("bed",-77,0,23,3,out _);m.AdvanceMinutes(853.25);
            var restored=Reload(m);Assert.That(restored.MinuteOfDay,Is.EqualTo(1213.25));Assert.That(restored.Day,Is.EqualTo(1));
            Assert.That(JsonUtility.ToJson(restored.Snapshot()),Is.EqualTo(JsonUtility.ToJson(m.Snapshot())));
        }
        [Test] public void V2MigrationPreservesWorldPositionsFarmAndResources()
        {
            var old=FarmingTests.Prepared(Fresh());old.BuySeeds("turnip",2,out _);old.Plant(0,"turnip",out _);old.Water(0,out _);
            var s=old.Snapshot();s.version=2;s.building=new BuildingSnapshot{wood=18,blocks=new[]{new BlockRecord{pieceId="wood_block",x=2,z=3,level=0,rotation=1}}};
            var m=FarmModel.Restore(s,Crops,6,6);
            Assert.That(m.Building.Occupied(5,0,-4),Is.True);Assert.That(m.Building.Wood,Is.EqualTo(18));Assert.That(m.IndexAt(-3,-3),Is.EqualTo(0));
            Assert.That(m.Plot(0).watered,Is.True);Assert.That(m.Money,Is.EqualTo(40));Assert.That(m.MinuteOfDay,Is.EqualTo(360));
            Assert.That(Reload(m).Building.Occupied(5,0,-4),Is.True);
        }
        [TestCase("clock")] [TestCase("nan")] [TestCase("duplicate")] [TestCase("bounds")] [TestCase("overlap")]
        public void CorruptWorldSaveIsRejected(string fault)
        {
            var m=Fresh();m.Till(0,0,out _);var s=m.Snapshot();
            switch(fault)
            {
                case "clock":s.minuteOfDay=1440;break;
                case "nan":s.minuteOfDay=double.NaN;break;
                case "duplicate":s.plots=new[]{s.plots[0],s.plots[0]};break;
                case "bounds":s.plots[0].x=100001;break;
                case "overlap":s.plots[0].cropId="turnip";s.building.blocks=new[]{new BlockRecord{pieceId="wood_block"}};break;
            }
            Assert.Throws<ArgumentException>(()=>FarmModel.Restore(s,Crops,6,6));
        }
    }
}
