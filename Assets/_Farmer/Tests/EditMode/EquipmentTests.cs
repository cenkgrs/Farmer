using NUnit.Framework;
using UnityEngine;

namespace Farmer.Tests
{
    public sealed class EquipmentTests
    {
        private static CropRules[] Catalog => new[] { new CropRules("turnip", 10, 18, 3, 1) };
        [Test]
        public void ItemsHaveIndependentActionsAndReusableTools()
        {
            var farm = new FarmModel(Catalog);
            Assert.That(farm.ItemCount(FarmItem.WateringCan, "turnip"), Is.EqualTo(1));
            Assert.That(farm.ItemCount(FarmItem.Sickle, "turnip"), Is.EqualTo(1));
            Assert.That(farm.UseEquipped(0, "turnip", out _), Is.False);
            farm.BuySeeds("turnip", 2, out _);
            Assert.That(farm.UseEquipped(0, "turnip", out _), Is.True);
            Assert.That(farm.UseEquipped(0, "turnip", out _), Is.False);
            Assert.That(farm.Plot(0).watered, Is.False);
            farm.Equip(FarmItem.WateringCan);
            Assert.That(farm.UseEquipped(1, "turnip", out _), Is.False);
            Assert.That(farm.ItemCount(FarmItem.Seeds, "turnip"), Is.EqualTo(1));
            farm.UseEquipped(0, "turnip", out _);
            for (int day = 0; day < 3; day++) farm.EndDay(out _);
            Assert.That(farm.UseEquipped(0, "turnip", out _), Is.False);
            Assert.That(farm.Produce("turnip"), Is.Zero);
            farm.Equip(FarmItem.Sickle);
            Assert.That(farm.UseEquipped(0, "turnip", out _), Is.True);
            Assert.That(farm.UseEquipped(0, "turnip", out _), Is.False);
            Assert.That(farm.Seeds("turnip"), Is.EqualTo(1));
            Assert.That(farm.Produce("turnip"), Is.EqualTo(1));
            Assert.That(farm.ItemCount(FarmItem.WateringCan, "turnip"), Is.EqualTo(1));
        }
        [Test]
        public void EquippedItemRoundTripsAndLegacySaveDefaultsToSeeds()
        {
            var farm = new FarmModel(Catalog);
            farm.Equip(FarmItem.Sickle);
            Assert.That(FarmModel.Restore(farm.Snapshot(), Catalog, 6, 6).EquippedItem, Is.EqualTo(FarmItem.Sickle));
            string legacy = JsonUtility.ToJson(farm.Snapshot()).Replace("\"equippedItem\":2,", "");
            Assert.That(FarmModel.Restore(JsonUtility.FromJson<FarmSnapshot>(legacy), Catalog, 6, 6).EquippedItem, Is.EqualTo(FarmItem.Seeds));
        }
        [Test]
        public void InvalidEquipmentCannotChangeInventoryOrBeLoaded()
        {
            var farm = new FarmModel(Catalog);
            Assert.That(farm.Equip((FarmItem)100), Is.False);
            Assert.That(farm.EquippedItem, Is.EqualTo(FarmItem.Seeds));
            var save = farm.Snapshot(); save.equippedItem = (FarmItem)100;
            Assert.Throws<System.ArgumentException>(() => FarmModel.Restore(save, Catalog, 6, 6));
        }
    }
}
