using System;
using System.Linq;

namespace Farmer
{
    [Serializable] public sealed class EnemyRecord
    {
        public int health, defeatedDay, rewardDay;
        public bool located;
        public int x, z;
        public EnemyRecord Copy() => (EnemyRecord)MemberwiseClone();
    }
    [Serializable] public sealed class CombatSnapshot
    {
        public int health = 100;
        public EnemyRecord[] enemies;
    }
    public sealed class EnemyRules
    {
        public readonly string Name, Area;
        public readonly int Health, Damage;
        public readonly float Speed, Windup, Reach;
        public EnemyRules(string name, string area, int health, int damage, float speed, float windup, float reach)
        {Name=name;Area=area;Health=health;Damage=damage;Speed=speed;Windup=windup;Reach=reach;}
        public static readonly EnemyRules[] Catalog = {
            new EnemyRules("Orman slime'ı", "Orman açıklığı", 35, 12, 1.4f, .8f, 1.65f),
            new EnemyRules("Taş böceği", "Maden girişi", 55, 18, 1.9f, 1.1f, 2.1f)
        };
    }
    // Persistent combat state; movement, hit timing and presentation live outside the economy.
    public sealed class CombatModel
    {
        public const int MaxHealth=100;
        public int Health {get; private set;}=MaxHealth;
        private EnemyRecord[] enemies=EnemyRules.Catalog.Select(r=>new EnemyRecord{health=r.Health}).ToArray();
        public EnemyRecord Enemy(int id)=>enemies[id].Copy();
        public void Locate(int id,int x,int z)
        {
            if(Math.Abs((long)x)>110||Math.Abs((long)z)>110||x*x+z*z<16*16)throw new ArgumentException("Unsafe enemy home.");
            enemies[id].located=true;enemies[id].x=x;enemies[id].z=z;
        }
        public bool Strike(int id,int day)
        {
            var e=enemies[id];if(e.health==0)return false;
            e.health=Math.Max(0,e.health-15);if(e.health==0)e.defeatedDay=day;return true;
        }
        public bool RefreshDay(int day)
        {
            bool changed=false;
            for(int i=0;i<enemies.Length;i++)if(enemies[i].health==0&&enemies[i].defeatedDay<day)
            {enemies[i].health=EnemyRules.Catalog[i].Health;changed=true;}
            return changed;
        }
        public bool Damage(int amount)
        {if(amount<1)throw new ArgumentOutOfRangeException(nameof(amount));Health=Math.Max(0,Health-amount);return Health==0;}
        public bool Heal(int amount)
        {if(amount<1)throw new ArgumentOutOfRangeException(nameof(amount));int old=Health;Health=(int)Math.Min(MaxHealth,(long)Health+amount);return old!=Health;}
        public bool CanClaim(int id,int day)=>enemies[id].health==0&&enemies[id].defeatedDay==day&&enemies[id].rewardDay<day;
        internal void Claim(int id,int day)=>enemies[id].rewardDay=day;
        public CombatSnapshot Snapshot()=>new CombatSnapshot{health=Health,enemies=enemies.Select(e=>e.Copy()).ToArray()};
        public static CombatModel Restore(CombatSnapshot s,int day)
        {
            if(s==null||s.health<1||s.health>MaxHealth||s.enemies==null||s.enemies.Length!=EnemyRules.Catalog.Length)throw new ArgumentException("Invalid combat state.");
            for(int i=0;i<s.enemies.Length;i++)
            {
                var e=s.enemies[i];
                if(e==null||e.health<0||e.health>EnemyRules.Catalog[i].Health||e.defeatedDay<0||e.defeatedDay>day||e.rewardDay<0||e.rewardDay>e.defeatedDay
                    ||(e.health==0&&e.defeatedDay==0)||(!e.located&&(e.health!=EnemyRules.Catalog[i].Health||e.defeatedDay!=0||e.rewardDay!=0)))throw new ArgumentException("Invalid enemy state.");
                if(e.located&&(Math.Abs((long)e.x)>110||Math.Abs((long)e.z)>110||(long)e.x*e.x+(long)e.z*e.z<16*16))throw new ArgumentException("Invalid enemy home.");
            }
            return new CombatModel{Health=s.health,enemies=s.enemies.Select(e=>e.Copy()).ToArray()};
        }
    }
    public sealed partial class FarmModel
    {
        public CombatModel Combat {get;private set;}=new CombatModel();
        public int RecoverFromDefeat()
        {int loss=Math.Min(10,Money/10);Money-=loss;Combat.Heal(100);return loss;}
        public bool EatHarvest(out string message)
        {
            if(BagCount("hunt:cooked_meat")>0)return EatCookedMeat(out message);
            if(Combat.Health==100)return Fail("Canın zaten dolu.",out message);
            string id=new[]{"lettuce","tomato","carrot","turnip","pumpkin"}.FirstOrDefault(c=>produce.TryGetValue(c,out int n)&&n>0);
            if(id==null)return Fail("İyileşmek için bir sebze hasat et veya yatağında uyu.",out message);
            produce[id]--;Combat.Heal(25);message="Bir sebze yedin · +25 can.";return true;
        }
        public bool ClaimExpedition(int id,out string message)
        {
            if(id<0||id>=EnemyRules.Catalog.Length||!Combat.CanClaim(id,Day))return Fail("Bugünün kaynağı henüz hazır değil.",out message);
            if(id==1&&!OwnsPickaxe)return Fail("Maden zulası için pazardan kazma satın al.",out message);
            if(id==0)
            {if(!Building.AddWood(12))return Fail("Odun yığının dolu; yer açıp tekrar gel.",out message);}
            else {if(Stone>StackLimit-12)return Fail("Taş yığının dolu; yer açıp tekrar gel.",out message);Stone+=12;}
            Journey |= JourneyStep.Explore;Combat.Claim(id,Day);message=id==0?"Orman zulası · +12 odun.":"Maden zulası · +12 taş.";return true;
        }
    }
}
