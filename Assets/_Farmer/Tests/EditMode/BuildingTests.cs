using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Farmer.Tests
{
    public sealed class BuildingTests
    {
        private static CropRules[] Crops => new[] { new CropRules("turnip", 10, 18, 3, 1) };
        private static FarmModel Fresh() => FarmingTests.Prepared(new FarmModel(Crops,startingMoney:60));
        private static string State(FarmModel farm) => JsonUtility.ToJson(farm.Snapshot());
        [Test] public void PlacementStackingRotationAndRemovalConserveWood()
        {
            var b = Fresh().Building;
            Assert.That(b.Place("wood_block", 0, 0, 0, 0, out _), Is.True);
            Assert.That(b.Place("wood_block", 1, 0, 0, 1, out _), Is.True);
            Assert.That(b.Place("wood_block", 0, 1, 0, 2, out _), Is.True);
            Assert.That(b.Wood, Is.EqualTo(18)); Assert.That(b.Count, Is.EqualTo(3));
            Assert.That(b.Remove(0, 0, 0, out _), Is.False);
            Assert.That(b.Remove(0, 1, 0, out _), Is.True);
            Assert.That(b.Remove(0, 1, 0, out _), Is.False);
            Assert.That(b.Remove(0, 0, 0, out _), Is.True);
            Assert.That(b.Remove(1, 0, 0, out _), Is.True);
            Assert.That(b.Wood, Is.EqualTo(BuildingModel.StarterWood));
        }
        [TestCase(-100001,0,0,0)] [TestCase(100001,0,0,0)] [TestCase(0,0,100001,0)] [TestCase(0,3,0,0)]
        [TestCase(0,1,0,0)] [TestCase(0,0,0,4)] [TestCase(0,-1,0,0)]
        public void InvalidPlacementDoesNotSpendWood(int x, int level, int z, int rotation)
        {
            var farm = Fresh(); string before = State(farm);
            Assert.That(farm.Building.Place("wood_block", x, level, z, rotation, out _), Is.False);
            Assert.That(State(farm), Is.EqualTo(before));
        }
        [Test] public void OccupiedAndUnknownPiecesCannotSpendWood()
        {
            var farm = Fresh(); farm.Building.Place("wood_block", 0, 0, 0, 0, out _); string before = State(farm);
            Assert.That(farm.Building.Place("wood_block", 0, 0, 0, 0, out _), Is.False);
            Assert.That(farm.Building.Place("unknown", 1, 0, 0, 0, out _), Is.False);
            Assert.That(State(farm), Is.EqualTo(before));
        }
        [Test] public void MissingWoodAndFullRefundBagRejectAtomically()
        {
            var empty = new BuildingModel(BuildRules.Defaults, 0);
            Assert.That(empty.Place("wood_block", 0, 0, 0, 0, out _), Is.False);
            var b = new BuildingModel(BuildRules.Defaults); b.Place("wood_block", 0, 0, 0, 0, out _);
            var saved = b.Snapshot(); saved.wood = BuildingModel.WoodLimit;
            var full = BuildingModel.Restore(saved, BuildRules.Defaults);
            Assert.That(full.Remove(0, 0, 0, out _), Is.False);
            Assert.That(full.Count, Is.EqualTo(1)); Assert.That(full.Wood, Is.EqualTo(BuildingModel.WoodLimit));
        }
        [Test] public void WoodPurchaseSharesMoneyWithoutChangingCrops()
        {
            var farm = Fresh(); farm.BuySeeds("turnip", 2, out _); farm.Plant(0, "turnip", out _);
            Assert.That(farm.BuyWood(out _), Is.True); Assert.That(farm.Money, Is.EqualTo(20));
            Assert.That(farm.Building.Wood, Is.EqualTo(34)); Assert.That(farm.Seeds("turnip"), Is.EqualTo(1));
            Assert.That(farm.Plot(0).cropId, Is.EqualTo("turnip"));
            farm.BuyWood(out _); string before = State(farm);
            Assert.That(farm.BuyWood(out _), Is.False); Assert.That(State(farm), Is.EqualTo(before));
            var saved = Fresh().Snapshot(); saved.building.wood = 995;
            farm = FarmModel.Restore(saved, Crops, 6, 6); before = State(farm);
            Assert.That(farm.BuyWood(out _), Is.False); Assert.That(State(farm), Is.EqualTo(before));
        }
        [Test] public void LegacySaveMigratesOnceWithoutLosingTheFarm()
        {
            var farm = Fresh(); farm.BuySeeds("turnip", 2, out _); farm.Plant(0, "turnip", out _); farm.Water(0, out _); farm.EndDay(out _); farm.Equip(FarmItem.Sickle);
            var old = farm.Snapshot(); old.version = 1; old.building = null;
            var migrated = FarmModel.Restore(old, Crops, 6, 6);
            Assert.That(migrated.Money, Is.EqualTo(farm.Money)); Assert.That(migrated.Day, Is.EqualTo(farm.Day));
            Assert.That(migrated.Seeds("turnip"), Is.EqualTo(1)); Assert.That(migrated.Plot(0).growth, Is.EqualTo(1));
            Assert.That(migrated.EquippedItem, Is.EqualTo(FarmItem.Sickle)); Assert.That(migrated.Building.Wood, Is.EqualTo(24));
            migrated.Building.Place("wood_block", 0, 0, 0, 3, out _);
            var loaded = FarmModel.Restore(migrated.Snapshot(), Crops, 6, 6);
            Assert.That(loaded.Building.Wood, Is.EqualTo(22)); Assert.That(State(loaded), Is.EqualTo(State(migrated)));
            var detached = loaded.Snapshot(); detached.building.blocks.Single(b=>b.pieceId=="wood_block").rotation = 0;
            Assert.That(loaded.Building.Snapshot().blocks.Single(b=>b.pieceId=="wood_block").rotation, Is.EqualTo(3));
        }
        [TestCase("missing")] [TestCase("wood")] [TestCase("unknown")] [TestCase("duplicate")]
        [TestCase("floating")] [TestCase("rotation")] [TestCase("bounds")] [TestCase("null")]
        public void BrokenCurrentConstructionIsRejected(string fault)
        {
            var farm = Fresh(); farm.Building.Place("wood_block", 0, 0, 0, 0, out _); var s = farm.Snapshot();
            switch (fault)
            {
                case "missing": s.building = null; break;
                case "wood": s.building.wood = -1; break;
                case "unknown": s.building.blocks[0].pieceId = "missing"; break;
                case "duplicate": s.building.blocks = new[] { s.building.blocks[0], s.building.blocks[0] }; break;
                case "floating": s.building.blocks[0].level = 1; break;
                case "rotation": s.building.blocks[0].rotation = -1; break;
                case "bounds": s.building.blocks[0].x = 100001; break;
                case "null": s.building.blocks[0] = null; break;
            }
            Assert.Throws<ArgumentException>(() => FarmModel.Restore(s, Crops, 6, 6));
        }
        [Test] public void CustomBuildDefinitionUsesTheSamePlacementRules()
        {
            var rules = new[] { new BuildRules("custom", 5) };
            var farm = new FarmModel(Crops, buildCatalog: rules);
            Assert.That(farm.Building.Place("custom", 0, 0, 0, 1, out _), Is.True);
            Assert.That(farm.Building.Wood, Is.EqualTo(19));
            Assert.That(FarmModel.Restore(farm.Snapshot(), Crops, 6, 6, rules).Building.Count, Is.EqualTo(1));
        }
        [Test] public void FirstV6WriteKeepsOriginalV1AndRecoversV5Backup()
        {
            string directory = Path.Combine(Path.GetTempPath(), "farmer-building-test-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, "farm-v1.json");
            try
            {
                var old = Fresh().Snapshot(); old.version = 1; old.building = null;
                string original = JsonUtility.ToJson(old); File.WriteAllText(path, original);
                var store = new FarmSaveStore(path, s => FarmModel.Restore(s, Crops, 6, 6));
                var farm = store.Load(out _); farm.Building.Place("wood_block", 0, 0, 0, 0, out _); store.Save(farm.Snapshot());
                Assert.That(File.ReadAllText(path + ".pre-v7"), Is.EqualTo(original));
                farm.Building.Place("wood_block", 0, 1, 0, 1, out _); store.Save(farm.Snapshot());
                File.WriteAllText(path, "broken"); var restored = store.Load(out bool recovered);
                Assert.That(recovered, Is.True); Assert.That(restored.Building.Count, Is.EqualTo(2)); Assert.That(restored.Building.Wood, Is.EqualTo(22));
                Assert.That(File.ReadAllText(path + ".pre-v7"), Is.EqualTo(original));
            }
            finally { Directory.Delete(directory, true); }
        }
    }
}
