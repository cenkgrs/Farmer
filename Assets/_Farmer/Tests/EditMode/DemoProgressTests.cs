using System;
using NUnit.Framework;
namespace Farmer.Tests
{
    public sealed class DemoProgressTests
    {
        private static readonly CropRules[] Crops={new CropRules("turnip",10,18,3,1)};
        [Test] public void FirstFarmLoopRecordsOnlySuccessfulActionsAndSurvivesReload()
        {
            var f=new FarmModel(Crops,worldSeed:42);Assert.That(f.Journey,Is.EqualTo((JourneyStep)0));Assert.That(f.BuySeeds("turnip",100,out _),Is.False);Assert.That(f.Journey,Is.EqualTo((JourneyStep)0));
            f.BuySeeds("turnip",1,out _);f.Till(0,0,out _);int p=f.IndexAt(0,0);f.Plant(p,"turnip",out _);f.Water(p,out _);f.AdvanceMinutes(3*1440);f.Harvest(p,out _);f.Sell("turnip",1,out _);
            Assert.That((int)f.Journey,Is.EqualTo(31));f=FarmModel.Restore(f.Snapshot(),Crops,6,6);Assert.That((int)f.Journey,Is.EqualTo(31));
        }
        [Test] public void UnknownProgressBitsRejectSave()
        {var f=new FarmModel(Crops,worldSeed:42);var s=f.Snapshot();s.journey=128;Assert.Throws<ArgumentException>(()=>FarmModel.Restore(s,Crops,6,6));}
    }
}
