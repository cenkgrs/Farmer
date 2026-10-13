using System;
using System.Linq;
using NUnit.Framework;
namespace Farmer.Tests
{
    public sealed class WildlifeTests
    {
        private static readonly CropRules[] Crops={new CropRules("turnip",10,18,3,1)};
        private static readonly BuildRules[] Pieces=BuildRules.Defaults.Concat(new[]{new BuildRules("home_stove",1,price:100)}).ToArray();
        private static FarmModel Fresh()=>new FarmModel(Crops,startingMoney:500,buildCatalog:Pieces,worldSeed:42);
        private static FarmModel Restore(FarmSnapshot s)=>FarmModel.Restore(s,Crops,6,6,Pieces);
        [TestCase(FarmItem.Axe,0,1,0)] [TestCase(FarmItem.Bow,0,2,1)] [TestCase(FarmItem.Dagger,4,4,2)] [TestCase(FarmItem.Axe,4,2,1)]
        public void WeaponDeterminesYieldAndRewardCannotRepeat(FarmItem weapon,int id,int meat,int hides)
        {
            var f=Fresh();f.BuyHuntingTool("bow",out _);f.BuyHuntingTool("dagger",out _);f.Equip(weapon);
            while(f.Animal(id).health>0)Assert.That(f.StrikeAnimal(id),Is.True);
            Assert.That(f.ClaimAnimal(id,out _),Is.True);Assert.That(f.BagCount("hunt:raw_meat"),Is.EqualTo(meat));Assert.That(f.BagCount("hunt:hide"),Is.EqualTo(hides));
            f=Restore(f.Snapshot());Assert.That(f.EquippedItem,Is.EqualTo(weapon));Assert.That(f.ClaimAnimal(id,out _),Is.False);Assert.That(f.RefreshWildlife(),Is.False);
            f.AdvanceMinutes(1440);Assert.That(f.RefreshWildlife(),Is.True);Assert.That(f.Animal(id).health,Is.EqualTo(WildlifeRules.Health(id)));
        }
        [Test] public void SwitchingAwayFromAxeCannotImproveAnAlreadyDamagedHide()
        {var f=Fresh();f.BuyHuntingTool("dagger",out _);f.Equip(FarmItem.Axe);f.StrikeAnimal(4);f=Restore(f.Snapshot());f.Equip(FarmItem.Dagger);f.StrikeAnimal(4);f.ClaimAnimal(4,out _);Assert.That(f.BagCount("hunt:raw_meat"),Is.EqualTo(2));Assert.That(f.BagCount("hunt:hide"),Is.EqualTo(1));}
        [Test] public void FullBagLeavesWholeRewardAvailable()
        {
            var f=Fresh();f.Equip(FarmItem.Axe);f.StrikeAnimal(4);f.StrikeAnimal(4);var s=f.Snapshot();s.wildlife.items.Single(i=>i.id=="raw_meat").count=998;f=Restore(s);
            Assert.That(f.ClaimAnimal(4,out _),Is.False);Assert.That(f.BagCount("hunt:hide"),Is.Zero);Assert.That(f.Animal(4).claimed,Is.False);
        }
        [Test] public void CookingRequiresPlacedStoveAndConsumesFuelAtomically()
        {
            var f=Fresh();f.Equip(FarmItem.Axe);f.StrikeAnimal(0);f.ClaimAnimal(0,out _);Assert.That(f.CookMeat(null,out _),Is.False);
            f.BuyFurniture("home_stove",out _);Assert.That(f.Building.Place("home_stove",10,0,10,0,out _),Is.True);var stove=f.Building.Blocks.Single();int wood=f.Building.Wood;
            Assert.That(f.CookMeat(stove,out _),Is.True);Assert.That(f.Building.Wood,Is.EqualTo(wood-1));Assert.That(f.BagCount("hunt:raw_meat"),Is.Zero);
            Assert.That(f.EatCookedMeat(out _),Is.False);Assert.That(f.BagCount("hunt:cooked_meat"),Is.EqualTo(1));f.Combat.Damage(55);Assert.That(f.EatHarvest(out _),Is.True);Assert.That(f.Combat.Health,Is.EqualTo(85));Assert.That(f.BagCount("hunt:cooked_meat"),Is.Zero);
            Assert.That(f.CookMeat(stove,out _),Is.False);Assert.That(f.Building.Wood,Is.EqualTo(wood-1));
        }
        [Test] public void MigrationPreservesLegacyFarmAndRejectsCorruptInventory()
        {var f=Fresh();var s=f.Snapshot();s.version=12;s.wildlife=null;var r=Restore(s);Assert.That(r.Money,Is.EqualTo(f.Money));Assert.That(r.OwnsBow,Is.False);s=r.Snapshot();s.wildlife.items[0].count=-1;Assert.Throws<ArgumentException>(()=>Restore(s));}
        [Test] public void ToolsAreUniqueAndCannotBeEquippedBeforePurchase()
        {var f=Fresh();Assert.That(f.Equip(FarmItem.Bow),Is.False);Assert.That(f.Equip(FarmItem.Dagger),Is.False);Assert.That(f.BuyHuntingTool("bow",out _),Is.True);int money=f.Money;Assert.That(f.BuyHuntingTool("bow",out _),Is.False);Assert.That(f.Money,Is.EqualTo(money));}
        [Test] public void WatchAndFleeHaveHysteresis()
        {Assert.That(WildlifeRules.Mood(15,false),Is.EqualTo(WildlifeMood.Wander));Assert.That(WildlifeRules.Mood(8,false),Is.EqualTo(WildlifeMood.Watch));Assert.That(WildlifeRules.Mood(3,false),Is.EqualTo(WildlifeMood.Flee));Assert.That(WildlifeRules.Mood(7,true),Is.EqualTo(WildlifeMood.Flee));Assert.That(WildlifeRules.Mood(10,true),Is.EqualTo(WildlifeMood.Watch));}
    }
}
