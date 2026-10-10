using System;
using System.Collections.Generic;
using System.Linq;

namespace Farmer
{
    public enum ResourceKind { Tree, WildPlant, Chest, Stone }
    public sealed class ResourceRules
    {
        public ResourceKind Kind { get; }
        public int Hits { get; }
        public int Reward { get; }
        public FarmItem? Tool { get; }
        public ResourceRules(ResourceKind kind,int hits,int reward,FarmItem? tool)
        {Kind=kind;Hits=hits;Reward=reward;Tool=tool;}
        public static ResourceRules For(ResourceKind kind)=>kind==ResourceKind.Tree?new ResourceRules(kind,3,8,FarmItem.Axe)
            :kind==ResourceKind.WildPlant?new ResourceRules(kind,1,2,FarmItem.Sickle)
            :kind==ResourceKind.Stone?new ResourceRules(kind,3,6,FarmItem.Pickaxe):new ResourceRules(kind,1,0,null);
    }
    [Serializable] public sealed class ResourceRecord
    {
        public int id,x,z,hits,coins;
        public ResourceKind kind;
        public bool collected;
        public ResourceRecord Copy()=>(ResourceRecord)MemberwiseClone();
    }
    [Serializable] public sealed class ExplorationSnapshot { public int seed; public int generation; public ResourceRecord[] nodes; }
    public sealed class ExplorationModel
    {
        public const int HalfExtent=28; // Original farm ring, retained for legacy layouts.
        public const int TreeCount=168, StoneCount=48;
        public const int ResourceExtent=108;
        private int generation;
        private readonly Dictionary<int,ResourceRecord> nodes;
        public int Seed { get; }
        public int Revision { get; private set; }
        public IEnumerable<ResourceRecord> Nodes=>nodes.Values.OrderBy(n=>n.id).Select(n=>n.Copy());
        private ExplorationModel(int seed,IEnumerable<ResourceRecord> records){Seed=seed;nodes=records.ToDictionary(n=>n.id,n=>n.Copy());}
        public ResourceRecord Node(int id)=>nodes.TryGetValue(id,out var n)?n.Copy():null;
        public static ExplorationModel Generate(int seed,Func<int,int,bool> blocked=null)
        {
            if(seed==0)throw new ArgumentException("World seed cannot be zero.");
            uint state=unchecked((uint)seed);
            int Next(int max){state^=state<<13;state^=state>>17;state^=state<<5;return (int)(state%(uint)max);}
            var cells=new List<(int x,int z)>();
            // Leave the established farm clear and wide corridors between every resource.
            for(int x=-24;x<=24;x+=4)for(int z=-24;z<=24;z+=4)
                if(Math.Max(Math.Abs(x),Math.Abs(z))>=12&&(blocked==null||!blocked(x,z)))cells.Add((x,z));
            for(int i=cells.Count-1;i>0;i--){int j=Next(i+1);var c=cells[i];cells[i]=cells[j];cells[j]=c;}
            var records=new List<ResourceRecord>();
            for(int i=0;i<Math.Min(41,cells.Count);i++)
            {
                var kind=i<5?ResourceKind.Chest:i<17?ResourceKind.WildPlant:ResourceKind.Tree;
                records.Add(new ResourceRecord{id=i+1,x=cells[i].x,z=cells[i].z,kind=kind,coins=kind==ResourceKind.Chest?30+Next(36):0});
            }
            return new ExplorationModel(seed,records).Expand(blocked);
        }
        public ExplorationSnapshot Snapshot()=>new ExplorationSnapshot{seed=Seed,generation=generation,nodes=Nodes.ToArray()};
        public static ExplorationModel Restore(ExplorationSnapshot saved)
        {
            if(saved==null||saved.seed==0||saved.nodes==null||saved.nodes.Length>512||saved.generation<0||saved.generation>1)throw new ArgumentException("Invalid exploration world.");
            var ids=new HashSet<int>();var cells=new HashSet<(int,int)>();
            foreach(var n in saved.nodes)
            {
                if(n==null||n.id<1||!ids.Add(n.id)||!cells.Add((n.x,n.z))||!Enum.IsDefined(typeof(ResourceKind),n.kind)
                    ||Math.Abs((long)n.x)>(saved.generation==0?26:ResourceExtent)||Math.Abs((long)n.z)>(saved.generation==0?26:ResourceExtent)||Math.Max(Math.Abs(n.x),Math.Abs(n.z))<11)
                    throw new ArgumentException("Invalid resource identity or position.");
                var rules=ResourceRules.For(n.kind);
                if(n.hits<0||n.hits>rules.Hits||n.collected!=(n.hits==rules.Hits)||(n.kind==ResourceKind.Chest?(n.coins<30||n.coins>65):n.coins!=0))
                    throw new ArgumentException("Invalid resource progress or loot.");
            }
            return new ExplorationModel(saved.seed,saved.nodes){generation=saved.generation};
        }
        // Append once: never reshuffle existing resources, chest loot or partial progress.
        public ExplorationModel Expand(Func<int,int,bool> blocked=null)
        {
            if(generation==1)return this;
            uint state=unchecked((uint)Seed)^0x9E3779B9u;
            if(state==0)state=1;
            int Next(int max){state^=state<<13;state^=state>>17;state^=state<<5;return (int)(state%(uint)max);}
            var cells=new List<(int x,int z)>();
            for(int x=-ResourceExtent;x<=ResourceExtent;x+=6)
                for(int z=-ResourceExtent;z<=ResourceExtent;z+=6)
                    if(Math.Max(Math.Abs(x),Math.Abs(z))>=32&&!Reserved(x,z)
                        &&(blocked==null||!blocked(x,z))
                        &&!nodes.Values.Any(n=>Math.Abs(n.x-x)<4&&Math.Abs(n.z-z)<4))cells.Add((x,z));
            for(int i=cells.Count-1;i>0;i--){int j=Next(i+1);var c=cells[i];cells[i]=cells[j];cells[j]=c;}
            int trees=Math.Max(0,TreeCount-nodes.Values.Count(n=>n.kind==ResourceKind.Tree));
            int stones=Math.Max(0,StoneCount-nodes.Values.Count(n=>n.kind==ResourceKind.Stone));
            int id=nodes.Count==0?1:nodes.Keys.Max()+1;
            foreach(var cell in cells)
            {
                if(trees+stones==0||nodes.Count>=512)break;
                bool rock=stones>0&&(trees==0||Next(trees+stones)<stones);
                nodes.Add(id,new ResourceRecord{id=id++,x=cell.x,z=cell.z,kind=rock?ResourceKind.Stone:ResourceKind.Tree});
                if(rock)stones--;else trees--;
            }
            generation=1;Revision++;return this;
        }
        private static bool Reserved(int x,int z)
        {
            // Keep the village and the curved main-road corridor open.
            if(x>=-78&&x<=-30&&z>=80&&z<=122)return true;
            var route=new[]{(-5,8),(-12,32),(-30,48),(-34,68),(-55,86),(-55,96)};
            for(int i=1;i<route.Length;i++)
            {
                var a=route[i-1];var b=route[i];double dx=b.Item1-a.Item1,dz=b.Item2-a.Item2;
                double t=Math.Max(0,Math.Min(1,((x-a.Item1)*dx+(z-a.Item2)*dz)/(dx*dx+dz*dz)));
                double px=x-a.Item1-t*dx,pz=z-a.Item2-t*dz;
                if(px*px+pz*pz<49)return true;
            }
            return false;
        }
        internal void Hit(int id){var n=nodes[id];n.hits++;n.collected=n.hits==ResourceRules.For(n.kind).Hits;Revision++;}
    }
}
