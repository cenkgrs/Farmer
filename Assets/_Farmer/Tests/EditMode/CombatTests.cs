using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Farmer.Tests
{
    public sealed class CombatTests
    {
        private static readonly CropRules[] Crops={new CropRules("turnip",10,18,3,1)};
        private static FarmModel Fresh(int money=50)=>new FarmModel(Crops,startingMoney:money,worldSeed:42);
        private static FarmModel Restore(FarmSnapshot s)=>FarmModel.Restore(s,Crops,6,6);
        [Test] public void LegacySaveStartsHealthyWithoutNewTools()
        {var s=Fresh().Snapshot();s.version=11;s.combat=null;var f=Restore(s);Assert.That(f.Combat.Health,Is.EqualTo(100));Assert.That(f.OwnsPickaxe,Is.False);Assert.That(f.Money,Is.EqualTo(50));}
        [Test] public void DefeatPreservesBagAndCapsPenalty()
        {var f=Fresh(1000);f.BuySeeds("turnip",3,out _);f.Combat.Damage(100);int before=f.Seeds("turnip");Assert.That(f.RecoverFromDefeat(),Is.EqualTo(10));Assert.That(f.Combat.Health,Is.EqualTo(100));Assert.That(f.Money,Is.EqualTo(960));Assert.That(f.Seeds("turnip"),Is.EqualTo(before));}
        [Test] public void PoorPlayerCannotGoNegativeOnDefeat()
        {var f=Fresh(3);Assert.That(f.RecoverFromDefeat(),Is.Zero);Assert.That(f.Money,Is.EqualTo(3));}
        [Test] public void RewardPersistsAndCannotBeClaimedTwice()
        {
            var f=Fresh();f.Combat.Locate(0,28,-22);for(int i=0;i<3;i++)Assert.That(f.Combat.Strike(0,1),Is.True);
            Assert.That(f.Combat.Strike(0,1),Is.False);Assert.That(f.ClaimExpedition(0,out _),Is.True);
            f=Restore(JsonUtility.FromJson<FarmSnapshot>(JsonUtility.ToJson(f.Snapshot())));
            Assert.That(f.ClaimExpedition(0,out _),Is.False);Assert.That(f.Combat.RefreshDay(1),Is.False);
            f.AdvanceMinutes(1440);Assert.That(f.Combat.RefreshDay(f.Day),Is.True);Assert.That(f.Combat.Enemy(0).health,Is.EqualTo(35));Assert.That(f.ClaimExpedition(0,out _),Is.False);
        }
        [Test] public void FullHealthDoesNotConsumeFoodAndHealingClamps()
        {var s=Fresh().Snapshot();s.produce[0].count=2;var f=Restore(s);Assert.That(f.EatHarvest(out _),Is.False);Assert.That(f.Produce("turnip"),Is.EqualTo(2));f.Combat.Damage(12);Assert.That(f.EatHarvest(out _),Is.True);Assert.That(f.Combat.Health,Is.EqualTo(100));Assert.That(f.Produce("turnip"),Is.EqualTo(1));}
        [Test] public void MineRewardRequiresPurchasedPickaxeAndFullBagDoesNotConsumeClaim()
        {
            var f=Fresh(500);f.Combat.Locate(1,48,-42);for(int i=0;i<4;i++)f.Combat.Strike(1,1);
            Assert.That(f.ClaimExpedition(1,out _),Is.False);Assert.That(f.Combat.CanClaim(1,1),Is.True);
            f.BuyPickaxe(out _);var s=f.Snapshot();s.stone=999;f=Restore(s);Assert.That(f.ClaimExpedition(1,out _),Is.False);Assert.That(f.Combat.CanClaim(1,1),Is.True);
            s=f.Snapshot();s.stone=980;f=Restore(s);Assert.That(f.ClaimExpedition(1,out _),Is.True);Assert.That(f.Stone,Is.EqualTo(992));
        }
        [Test] public void CompactResourcesPreserveLootAndProgressAcrossReload()
        {
            var snapshot=Fresh().Snapshot();
            var n=Array.Find(snapshot.exploration.nodes,r=>r.kind==ResourceKind.Tree);n.x=100;n.z=-100;n.hits=2;
            var farm=Restore(snapshot);farm.Exploration.Compact();var moved=farm.Exploration.Node(n.id);
            Assert.That(Math.Max(Math.Abs(moved.x),Math.Abs(moved.z)),Is.LessThanOrEqualTo(54));
            Assert.That(moved.hits,Is.EqualTo(2));
            Assert.That(JsonUtility.ToJson(Restore(farm.Snapshot()).Exploration.Node(n.id)),Is.EqualTo(JsonUtility.ToJson(moved)));
        }
        [Test] public void CompactEnemyHomeSurvivesReload()
        {var f=Fresh();f.Combat.Locate(0,14,-14);Assert.That(Restore(f.Snapshot()).Combat.Enemy(0).x,Is.EqualTo(14));}
        [Test] public void LargeHealingAmountCannotOverflow()
        {var f=Fresh();f.Combat.Damage(90);f.Combat.Heal(int.MaxValue);Assert.That(f.Combat.Health,Is.EqualTo(100));}
        [Test] public void NoFoodDoesNotChangeHealth()
        {var f=Fresh();f.Combat.Damage(50);Assert.That(f.EatHarvest(out _),Is.False);Assert.That(f.Combat.Health,Is.EqualTo(50));}
        [TestCase(0)][TestCase(101)][TestCase(-1)] public void RejectInvalidHealth(int health)
        {var s=Fresh().Snapshot();s.combat.health=health;Assert.Throws<ArgumentException>(()=>Restore(s));}
        [Test] public void RejectInvalidEnemyAndUnsafeHome()
        {var s=Fresh().Snapshot();s.combat.enemies[0].health=900;Assert.Throws<ArgumentException>(()=>Restore(s));Assert.Throws<ArgumentException>(()=>Fresh().Combat.Locate(0,0,0));}
        [Test] public void RejectFutureReward()
        {var s=Fresh().Snapshot();s.combat.enemies[0].rewardDay=2;Assert.Throws<ArgumentException>(()=>Restore(s));}
        [Test] public void CombatMigrationKeepsOriginalAndBackup()
        {
            string dir=Path.Combine(Path.GetTempPath(),"FarmerCombat-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);string path=Path.Combine(dir,"save.json");
            var old=Fresh().Snapshot();old.version=11;old.combat=null;string raw=JsonUtility.ToJson(old);File.WriteAllText(path,raw);
            try{var store=new FarmSaveStore(path,Restore);var f=store.Load(out _);store.Save(f.Snapshot());Assert.That(File.ReadAllText(path+".pre-v"+FarmModel.SaveVersion),Is.EqualTo(raw));Assert.That(store.Load(out _).Combat.Health,Is.EqualTo(100));}finally{Directory.Delete(dir,true);}
        }
    }
}
