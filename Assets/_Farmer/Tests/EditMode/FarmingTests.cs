using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Farmer.Tests
{
    public class FarmingTests
    {
        private static CropRules[] Catalog => new[] { new CropRules("turnip", 10, 18, 3, 1) };
        private static FarmModel Fresh(int money = 60) => new FarmModel(Catalog, 6, 6, money);
        private static FarmModel Restore(FarmSnapshot snapshot) => FarmModel.Restore(snapshot, Catalog, 6, 6);
        private static string State(FarmModel m) => JsonUtility.ToJson(m.Snapshot());

        [Test]
        public void FullCycleSupportsReinvestmentAndFourStages()
        {
            var m = Fresh();
            Assert.That(m.BuySeeds("turnip", 3, out _), Is.True);
            Assert.That(m.Money, Is.EqualTo(30));
            Assert.That(m.Plant(0, "turnip", out _), Is.True);
            Assert.That(m.Seeds("turnip"), Is.EqualTo(2));
            Assert.That(m.Water(0, out _), Is.True);
            for (int stage = 0; stage < 3; stage++)
            {
                Assert.That(m.Stage(0), Is.EqualTo(stage));
                Assert.That(m.Harvest(0, out _), Is.False);
                Assert.That(m.Water(0, out _), Is.False);
                Assert.That(m.EndDay(out _), Is.True);
                Assert.That(m.Plot(0).watered, Is.True);
            }
            Assert.That(m.Stage(0), Is.EqualTo(3));
            Assert.That(m.Harvest(0, out _), Is.True);
            Assert.That(m.Harvest(0, out _), Is.False);
            Assert.That(m.Produce("turnip"), Is.EqualTo(1));
            Assert.That(m.Stage(0), Is.EqualTo(-1));
            Assert.That(m.Sell("turnip", 1, out _), Is.True);
            Assert.That(m.Money, Is.EqualTo(48));
            Assert.That(m.BuySeeds("turnip", 1, out _), Is.True);
            Assert.That(m.Money, Is.EqualTo(38));
            Assert.That(m.Seeds("turnip"), Is.EqualTo(3));
        }

        [Test]
        public void DryDaysDoNotAdvanceOrKillCropsAndWateringCannotStack()
        {
            var m = Fresh(); m.BuySeeds("turnip", 2, out _); m.Plant(0, "turnip", out _); m.Plant(1, "turnip", out _);
            m.Water(0, out _); Assert.That(m.Water(0, out _), Is.False); m.EndDay(out _);
            Assert.That(m.Plot(0).growth, Is.EqualTo(1)); Assert.That(m.Plot(1).growth, Is.Zero);
            for (int day = 0; day < 20; day++) m.EndDay(out _);
            Assert.That(m.Plot(0).growth, Is.EqualTo(3)); Assert.That(m.Plot(1).growth, Is.Zero); Assert.That(m.Plot(1).cropId, Is.EqualTo("turnip"));
        }

        [Test]
        public void SingleWateringSurvivesSaveAndHarvestResetsItForNextPlant()
        {
            var m = Fresh(); m.BuySeeds("turnip", 2, out _); m.Plant(0, "turnip", out _);
            m.Water(0, out _); m.EndDay(out _);
            var loaded = Restore(m.Snapshot());
            loaded.EndDay(out _); Assert.That(loaded.IsReady(0), Is.False);
            loaded.EndDay(out _); Assert.That(loaded.IsReady(0), Is.True);
            loaded.Harvest(0, out _); loaded.Plant(0, "turnip", out _);
            loaded.EndDay(out _);
            Assert.That(loaded.Plot(0).watered, Is.False);
            Assert.That(loaded.Plot(0).growth, Is.Zero);
        }

        [Test]
        public void LegacyGrowingCropDoesNotNeedToBeWateredAgain()
        {
            var saved = Fresh().Snapshot();
            saved.plots[0] = new PlotRecord { cropId = "turnip", growth = 1, watered = false };
            var m = Restore(saved);
            Assert.That(m.Plot(0).watered, Is.True);
            m.EndDay(out _); Assert.That(m.IsReady(0), Is.False);
            m.EndDay(out _); Assert.That(m.IsReady(0), Is.True);
        }

        [TestCase(-1)] [TestCase(0)] [TestCase(7)] [TestCase(1000)] [TestCase(int.MaxValue)]
        public void InvalidOrUnaffordablePurchasesHaveNoSideEffects(int count)
        {
            var m = Fresh(); string before = State(m);
            Assert.That(m.BuySeeds("turnip", count, out _), Is.False); Assert.That(State(m), Is.EqualTo(before));
        }

        [Test]
        public void OccupiedCellsMissingSeedsAndInvalidActionsDoNotConsumeItems()
        {
            var m = Fresh(); Assert.That(m.Plant(0, "turnip", out _), Is.False);
            m.BuySeeds("turnip", 2, out _); m.Plant(0, "turnip", out _); string before = State(m);
            Assert.That(m.Plant(0, "turnip", out _), Is.False);
            Assert.That(m.Plant(-1, "turnip", out _), Is.False);
            Assert.That(m.Plant(36, "turnip", out _), Is.False);
            Assert.That(m.Plant(1, "unknown", out _), Is.False);
            Assert.That(m.Water(1, out _), Is.False);
            Assert.That(m.Sell("turnip", 1, out _), Is.False);
            Assert.That(State(m), Is.EqualTo(before));
        }

        [Test]
        public void FullInventoryDoesNotDestroyReadyCropOrSpendMoney()
        {
            var saved = Fresh().Snapshot(); saved.seeds[0].count = FarmModel.StackLimit; saved.produce[0].count = FarmModel.StackLimit;
            saved.plots[0] = new PlotRecord { cropId = "turnip", growth = 3 };
            var m = Restore(saved); string before = State(m);
            Assert.That(m.Harvest(0, out _), Is.False); Assert.That(m.BuySeeds("turnip", 1, out _), Is.False);
            Assert.That(State(m), Is.EqualTo(before));
            m.Sell("turnip", 1, out _); Assert.That(m.Harvest(0, out _), Is.True);
        }

        [Test]
        public void DayAndMoneyLimitsRejectTransactionsWithoutOverflow()
        {
            var saved = Fresh().Snapshot(); saved.money = FarmModel.MoneyLimit; saved.day = FarmModel.DayLimit; saved.produce[0].count = 1;
            var m = Restore(saved); string before = State(m);
            Assert.That(m.Sell("turnip", 1, out _), Is.False); Assert.That(m.EndDay(out _), Is.False);
            Assert.That(State(m), Is.EqualTo(before));
        }

        [Test]
        public void SaveRoundTripPreservesMoneyInventoryGrowthDayAndWaterAndCopiesData()
        {
            var m = Fresh(); m.BuySeeds("turnip", 4, out _); m.Plant(0, "turnip", out _); m.Water(0, out _);
            m.EndDay(out _); m.Plant(5, "turnip", out _);
            string json = State(m);
            var saved = JsonUtility.FromJson<FarmSnapshot>(json); var restored = Restore(saved);
            Assert.That(State(restored), Is.EqualTo(json));
            saved.plots[0].growth = 3; saved.seeds[0].count = 900;
            Assert.That(State(restored), Is.EqualTo(json));
            var exported = restored.Snapshot(); exported.plots[0].growth = 3;
            var plot = restored.Plot(0); plot.growth = 3;
            Assert.That(restored.Plot(0).growth, Is.EqualTo(1));
        }

        [TestCase("version")] [TestCase("money")] [TestCase("day")] [TestCase("grid")]
        [TestCase("inventory")] [TestCase("duplicate")] [TestCase("crop")] [TestCase("growth")]
        [TestCase("empty")] [TestCase("missing")]
        public void InvalidSaveIsRejectedBeforeStateIsRestored(string fault)
        {
            var s = Fresh().Snapshot();
            switch (fault)
            {
                case "version": s.version = 99; break;
                case "money": s.money = -1; break;
                case "day": s.day = 0; break;
                case "grid": s.width = 7; break;
                case "inventory": s.seeds[0].count = -1; break;
                case "duplicate": s.seeds = new[] { s.seeds[0], s.seeds[0] }; break;
                case "crop": s.plots[0].cropId = "missing"; break;
                case "growth": s.plots[0] = new PlotRecord { cropId = "turnip", growth = 4 }; break;
                case "empty": s.plots[0].watered = true; break;
                case "missing": s.plots[4] = null; break;
            }
            Assert.Throws<ArgumentException>(() => Restore(s));
        }

        [Test]
        public void NewCropRulesUseSameInventoryAndGrowthTransactions()
        {
            var m = new FarmModel(new[] { Catalog[0], new CropRules("other", 5, 12, 6, 2) });
            m.BuySeeds("other", 1, out _); m.Plant(0, "other", out _);
            m.Water(0, out _);
            for (int i = 0; i < 6; i++) m.EndDay(out _);
            m.Harvest(0, out _); Assert.That(m.Produce("other"), Is.EqualTo(2)); Assert.That(m.Produce("turnip"), Is.Zero);
            m.Sell("other", 2, out _); Assert.That(m.Money, Is.EqualTo(79));
        }
    }

    public class FarmSaveStoreTests
    {
        private string directory, path;
        private static CropRules[] Rules => new[] { new CropRules("turnip", 10, 18, 3, 1) };
        private FarmSaveStore Store => new FarmSaveStore(path, s => FarmModel.Restore(s, Rules, 6, 6));
        [SetUp] public void Setup() { directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "farmer-test-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(directory); path = System.IO.Path.Combine(directory, "farm.json"); }
        [TearDown] public void Cleanup() { Directory.Delete(directory, true); }

        [Test]
        public void MissingSaveReturnsNoStateAndCreatesNoFiles()
        { Assert.That(Store.Load(out bool recovered), Is.Null); Assert.That(recovered, Is.False); Assert.That(Directory.GetFiles(directory), Is.Empty); }

        [Test]
        public void AtomicSaveLoadsLatestAndKeepsPreviousBackup()
        {
            var m = new FarmModel(Rules); Store.Save(m.Snapshot()); m.BuySeeds("turnip", 2, out _); Store.Save(m.Snapshot());
            Assert.That(Store.Load(out bool recovered).Money, Is.EqualTo(40)); Assert.That(recovered, Is.False);
            Assert.That(JsonUtility.FromJson<FarmSnapshot>(File.ReadAllText(path + ".bak")).money, Is.EqualTo(60));
            Assert.That(File.Exists(path + ".tmp"), Is.False);
        }

        [Test]
        public void CorruptPrimaryRecoversBackupWithoutOverwritingItWithBadData()
        {
            var m = new FarmModel(Rules); Store.Save(m.Snapshot()); m.BuySeeds("turnip", 1, out _); Store.Save(m.Snapshot());
            File.WriteAllText(path, "{broken");
            var recoveredModel = Store.Load(out bool recovered); Assert.That(recovered, Is.True); Assert.That(recoveredModel.Money, Is.EqualTo(60));
            Assert.That(File.ReadAllText(path), Is.EqualTo("{broken"));
            Store.Save(recoveredModel.Snapshot());
            Assert.That(Directory.GetFiles(directory, "*.corrupt-*"), Has.Length.EqualTo(1));
            Assert.That(Store.Load(out recovered).Money, Is.EqualTo(60)); Assert.That(recovered, Is.False);
            Assert.That(JsonUtility.FromJson<FarmSnapshot>(File.ReadAllText(path + ".bak")).money, Is.EqualTo(60));
        }

        [Test]
        public void InvalidPrimaryWithoutBackupFailsAndPreservesOriginal()
        {
            File.WriteAllText(path, "not a save"); Assert.Throws<InvalidDataException>(() => Store.Load(out _));
            Assert.That(File.ReadAllText(path), Is.EqualTo("not a save"));
        }

        [Test]
        public void InterruptedTemporaryWriteDoesNotReplaceExistingSave()
        {
            var m = new FarmModel(Rules); Store.Save(m.Snapshot()); string before = File.ReadAllText(path);
            Directory.CreateDirectory(path + ".tmp"); m.BuySeeds("turnip", 1, out _);
            Assert.Catch(() => Store.Save(m.Snapshot())); Assert.That(File.ReadAllText(path), Is.EqualTo(before));
        }
    }
}
