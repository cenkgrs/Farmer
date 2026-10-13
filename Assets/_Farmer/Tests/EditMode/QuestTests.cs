using System;
using NUnit.Framework;
using UnityEngine;
namespace Farmer.Tests
{
    public sealed class QuestTests
    {
        private static readonly CropRules[] Crops={new CropRules("turnip",10,18,3,1),new CropRules("carrot",14,27,4,1),new CropRules("tomato",24,12,5,2,2)};
        private static readonly RecipeRules[] Recipes={new RecipeRules("vegetable_crate",1,90,new[]{new ItemStack{id="crop:turnip",count=1}})};
        private static readonly QuestRules[] Quests={
            new QuestRules("first_stall","İlk Tezgâh","",null,"huseyin","Hüseyin",60,new[]{new QuestRequirement("crop:turnip","Turp",5)},new[]{new QuestRequirement("seed:carrot","Havuç tohumu",3)}),
            new QuestRules("variety","Çeşit","","first_stall","huseyin","Hüseyin",120,new[]{new QuestRequirement("crop:carrot","Havuç",3),new QuestRequirement("crop:tomato","Domates",3)},Array.Empty<QuestRequirement>()),
            new QuestRules("village_order","Sipariş","","variety","huseyin","Hüseyin",180,new[]{new QuestRequirement("crafted:vegetable_crate","Kasa",1)},Array.Empty<QuestRequirement>())};
        private static FarmModel Restore(FarmSnapshot s)=>FarmModel.Restore(s,Crops,6,6,recipeCatalog:Recipes,questCatalog:Quests);
        private static FarmModel Fresh()
        {
            var s=new FarmModel(Crops,startingMoney:500,worldSeed:12,recipeCatalog:Recipes,questCatalog:Quests).Snapshot();
            s.produce=new[]{new InventoryRecord{cropId="turnip",count=8},new InventoryRecord{cropId="carrot",count=3},new InventoryRecord{cropId="tomato",count=3}};
            return Restore(s);
        }
        [Test] public void ChainUsesExistingProduceAndPaysOnceAcrossReload()
        {
            var f=Fresh();Assert.That(f.AcceptQuest("variety",out _),Is.False);
            Assert.That(f.AcceptQuest("first_stall",out _),Is.True);Assert.That(f.AcceptQuest("first_stall",out _),Is.False);
            Assert.That(f.QuestReady("first_stall"),Is.True);
            f=Restore(JsonUtility.FromJson<FarmSnapshot>(JsonUtility.ToJson(f.Snapshot())));
            Assert.That(f.TurnInQuest("first_stall",out _),Is.True);Assert.That(f.Money,Is.EqualTo(560));
            Assert.That(f.Produce("turnip"),Is.EqualTo(3));Assert.That(f.Seeds("carrot"),Is.EqualTo(3));
            f=Restore(f.Snapshot());Assert.That(f.TurnInQuest("first_stall",out _),Is.False);Assert.That(f.Money,Is.EqualTo(560));
            Assert.That(f.AcceptQuest("variety",out _),Is.True);Assert.That(f.TurnInQuest("variety",out _),Is.True);
            Assert.That(f.Produce("carrot"),Is.Zero);Assert.That(f.Produce("tomato"),Is.Zero);
            f.AcceptQuest("village_order",out _);Assert.That(f.QuestReady("village_order"),Is.False);
            f.Craft("vegetable_crate",1,out _);Assert.That(f.TurnInQuest("village_order",out _),Is.True);
            Assert.That(f.BagCount("crafted:vegetable_crate"),Is.Zero);Assert.That(f.Money,Is.EqualTo(860));
            foreach(var q in Quests)Assert.That(Restore(f.Snapshot()).QuestState(q.Id),Is.EqualTo(QuestStatus.Completed));
        }
        [Test] public void SellingOrStoringRequirementsRemovesReadiness()
        {
            var f=Fresh();f.AcceptQuest("first_stall",out _);f.Sell("turnip",4,out _);
            Assert.That(f.QuestReady("first_stall"),Is.False);var before=JsonUtility.ToJson(f.Snapshot());
            Assert.That(f.TurnInQuest("first_stall",out _),Is.False);Assert.That(JsonUtility.ToJson(f.Snapshot()),Is.EqualTo(before));
            f=Fresh();f.AcceptQuest("first_stall",out _);f.BuyFurniture("home_chest",out _);f.Building.Place("home_chest",5,0,5,0,out _);
            var chest=System.Linq.Enumerable.First(f.Building.Blocks).instanceId;
            Assert.That(f.TransferStorage(chest,"crop:turnip",4,false,out _),Is.True);Assert.That(f.QuestReady("first_stall"),Is.False);
        }
        [Test] public void FullRewardStackAndMoneyDoNotConsumeGoods()
        {
            foreach(bool fullMoney in new[]{false,true})
            {
                var s=Fresh().Snapshot();if(fullMoney)s.money=FarmModel.MoneyLimit;else s.seeds=new[]{new InventoryRecord{cropId="carrot",count=998}};
                var f=Restore(s);f.AcceptQuest("first_stall",out _);var before=JsonUtility.ToJson(f.Snapshot());
                Assert.That(f.TurnInQuest("first_stall",out _),Is.False);Assert.That(JsonUtility.ToJson(f.Snapshot()),Is.EqualTo(before));
            }
        }
        [Test] public void NpcChainsAreIndependentAndRewardsAreGeneric()
        {
            var own=new QuestRules("defne_test","Defne işi","",null,"defne","Defne",5,new[]{new QuestRequirement("crop:turnip","Turp",1)},new[]{new QuestRequirement("wood","Odun",4)});
            var catalog=System.Linq.Enumerable.Concat(Quests,new[]{own});
            var f=FarmModel.Restore(Fresh().Snapshot(),Crops,6,6,recipeCatalog:Recipes,questCatalog:catalog);
            Assert.That(f.AcceptQuest("defne_test",out _),Is.True);Assert.That(f.AcceptQuest("first_stall",out _),Is.True);
            int wood=f.Building.Wood;Assert.That(f.TurnInQuest("defne_test",out _),Is.True);Assert.That(f.Building.Wood,Is.EqualTo(wood+4));
            Assert.That(f.QuestState("first_stall"),Is.EqualTo(QuestStatus.Active));Assert.That(f.QuestState("variety"),Is.EqualTo(QuestStatus.Locked));
        }
        [Test] public void MigratesOldSavesAndRejectsInvalidQuestProgress()
        {
            var s=Fresh().Snapshot();s.version=13;s.quests=null;Assert.That(Restore(s).QuestState("first_stall"),Is.EqualTo(QuestStatus.Available));
            s.version=14;Assert.Throws<ArgumentException>(()=>Restore(s));
            s.quests=new[]{new QuestRecord{id="retired",completed=true}};Assert.That(Restore(s).Snapshot().quests[0].completed,Is.True);
            s.quests=new[]{new QuestRecord{id=""}};Assert.Throws<ArgumentException>(()=>Restore(s));
            s.quests=new[]{new QuestRecord{id="first_stall"},new QuestRecord{id="first_stall"}};Assert.Throws<ArgumentException>(()=>Restore(s));
        }
    }
}
