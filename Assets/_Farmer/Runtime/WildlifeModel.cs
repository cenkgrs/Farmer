using System;
using System.Collections.Generic;
using System.Linq;
namespace Farmer
{
    public enum WildlifeMood { Wander, Watch, Flee }
    public static class WildlifeRules
    {
        public const int Count=6;
        public static bool Deer(int id)=>id>=4;
        public static int Health(int id)=>Deer(id)?30:15;
        public static int Meat(int id,bool rough=false)=>Deer(id)?(rough?2:4):(rough?1:2);
        public static int Hides(int id,bool rough=false)=>Deer(id)?(rough?1:2):(rough?0:1);
        public static WildlifeMood Mood(float distance,bool fleeing)
            =>distance<(fleeing?9:4)?WildlifeMood.Flee:distance<12?WildlifeMood.Watch:WildlifeMood.Wander;
    }
    [Serializable] public sealed class WildlifeRecord
    {
        public int health, huntedDay;
        public bool claimed, located, roughKill;
        public int x,z;
        public WildlifeRecord Copy()=>(WildlifeRecord)MemberwiseClone();
    }
    [Serializable] public sealed class WildlifeSnapshot
    {
        public WildlifeRecord[] animals;
        public ItemStack[] items;
        public bool ownsBow, ownsDagger;
    }
    public sealed partial class FarmModel
    {
        public bool OwnsBow {get;private set;}
        public bool OwnsDagger {get;private set;}
        public bool BuyHuntingTool(string id,out string message)
        {
            if(id!="bow"&&id!="dagger")return Fail("Bilinmeyen alet.",out message);
            bool bow=id=="bow";int cost=bow?120:65;
            if(bow?OwnsBow:OwnsDagger)return Fail("Bu alet zaten sende.",out message);
            if(Money<cost)return Fail($"{cost} para gerekiyor.",out message);
            Money-=cost;if(bow)OwnsBow=true;else OwnsDagger=true;
            message=bow?"Yay alındı · 8 ile kuşan. Temel oklar dahildir.":"Hançer alındı · 9 ile kuşan.";return true;
        }
        public static bool HuntingWeapon(FarmItem item)=>item==FarmItem.Axe||item==FarmItem.Bow||item==FarmItem.Dagger;
        public static readonly string[] HuntingItems={"raw_meat","hide","cooked_meat"};
        private Dictionary<string,int> hunting=HuntingItems.ToDictionary(id=>id,id=>0);
        private WildlifeRecord[] animals=Enumerable.Range(0,WildlifeRules.Count).Select(i=>new WildlifeRecord{health=WildlifeRules.Health(i)}).ToArray();
        public WildlifeRecord Animal(int id)=>animals[id].Copy();
        public void LocateAnimal(int id,int x,int z)
        {if(id<0||id>=animals.Length||Math.Abs((long)x)>110||Math.Abs((long)z)>110)throw new ArgumentException("Invalid animal home.");animals[id].located=true;animals[id].x=x;animals[id].z=z;}
        public bool StrikeAnimal(int id)=>StrikeAnimal(id,EquippedItem);
        public bool StrikeAnimal(int id,FarmItem weapon)
        {
            if(id<0||id>=animals.Length||!HuntingWeapon(weapon)||ItemCount(weapon,SelectedCropId)==0||animals[id].health<=0)return false;
            if(weapon==FarmItem.Axe)animals[id].roughKill=true;
            animals[id].health=Math.Max(0,animals[id].health-15);
            if(animals[id].health==0)animals[id].huntedDay=Day;
            return true;
        }
        public bool ClaimAnimal(int id,out string message)
        {
            if(id<0||id>=animals.Length||animals[id].health>0||animals[id].claimed)return Fail("Burada toplanacak av yok.",out message);
            int meat=WildlifeRules.Meat(id,animals[id].roughKill),hide=WildlifeRules.Hides(id,animals[id].roughKill);
            if(hunting["raw_meat"]>StackLimit-meat||hunting["hide"]>StackLimit-hide)return Fail("Et veya post yığını dolu. Çantanda yer aç.",out message);
            hunting["raw_meat"]+=meat;hunting["hide"]+=hide;animals[id].claimed=true;
            message=$"+{meat} çiğ et · +{hide} post çantaya eklendi.";return true;
        }
        public bool RefreshWildlife()
        {
            bool changed=false;
            for(int i=0;i<animals.Length;i++)if(animals[i].health==0&&animals[i].claimed&&animals[i].huntedDay<Day)
            {animals[i].health=WildlifeRules.Health(i);animals[i].claimed=false;animals[i].roughKill=false;changed=true;}
            return changed;
        }
        public bool CookMeat(BlockRecord stove,out string message)
        {
            if(stove==null||!Building.Blocks.Any(b=>b.pieceId=="home_stove"&&b.x==stove.x&&b.z==stove.z&&b.level==stove.level))return Fail("Önce pazardan bir ocak alıp yerleştir.",out message);
            if(hunting["raw_meat"]<1||Building.Wood<1)return Fail("Pişirmek için 1 çiğ et ve 1 odun gerekiyor.",out message);
            if(hunting["cooked_meat"]>=StackLimit)return Fail("Pişmiş et yığını dolu.",out message);
            hunting["raw_meat"]--;Building.ChangeWood(-1);hunting["cooked_meat"]++;
            message="Et pişti · +1 pişmiş et · H ile ye (+40 can).";return true;
        }
        public bool EatCookedMeat(out string message)
        {
            if(Combat.Health==CombatModel.MaxHealth)return Fail("Canın zaten dolu.",out message);
            if(hunting["cooked_meat"]==0)return Fail("Önce ocakta et pişir.",out message);
            hunting["cooked_meat"]--;Combat.Heal(40);message="Pişmiş et yedin · +40 can.";return true;
        }
        private WildlifeSnapshot WildlifeSave()=>new WildlifeSnapshot{ownsBow=OwnsBow,ownsDagger=OwnsDagger,animals=animals.Select(a=>a.Copy()).ToArray(),items=hunting.Select(p=>new ItemStack{id=p.Key,count=p.Value}).ToArray()};
        private void RestoreWildlife(WildlifeSnapshot s)
        {
            if(s==null||s.animals==null||s.animals.Length!=WildlifeRules.Count||s.items==null)throw new ArgumentException("Missing wildlife state.");
            for(int i=0;i<s.animals.Length;i++)
            {
                var a=s.animals[i];
                if(a==null||a.health<0||a.health>WildlifeRules.Health(i)||a.huntedDay<0||a.huntedDay>Day||Math.Abs((long)a.x)>110||Math.Abs((long)a.z)>110||a.health==0&&a.huntedDay==0||a.claimed&&(a.huntedDay==0||a.health>0))throw new ArgumentException("Invalid wildlife state.");
            }
            var seen=new HashSet<string>();
            foreach(var item in s.items)
            {if(item==null||item.id==null||!hunting.ContainsKey(item.id)||!seen.Add(item.id)||item.count<0||item.count>StackLimit)throw new ArgumentException("Invalid hunting item.");hunting[item.id]=item.count;}
            if(seen.Count!=HuntingItems.Length)throw new ArgumentException("Missing hunting items.");
            OwnsBow=s.ownsBow;OwnsDagger=s.ownsDagger;
            animals=s.animals.Select(a=>a.Copy()).ToArray();
        }
    }
}
