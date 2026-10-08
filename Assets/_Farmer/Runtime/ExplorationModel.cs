using System;
using System.Collections.Generic;
using System.Linq;

namespace Farmer
{
    public enum ResourceKind { Tree, WildPlant, Chest }
    public sealed class ResourceRules
    {
        public ResourceKind Kind { get; }
        public int Hits { get; }
        public int Reward { get; }
        public FarmItem? Tool { get; }
        public ResourceRules(ResourceKind kind,int hits,int reward,FarmItem? tool)
        {Kind=kind;Hits=hits;Reward=reward;Tool=tool;}
        public static ResourceRules For(ResourceKind kind)=>kind==ResourceKind.Tree?new ResourceRules(kind,3,8,FarmItem.Axe)
            :kind==ResourceKind.WildPlant?new ResourceRules(kind,1,2,FarmItem.Sickle):new ResourceRules(kind,1,0,null);
    }
    [Serializable] public sealed class ResourceRecord
    {
        public int id,x,z,hits,coins;
        public ResourceKind kind;
        public bool collected;
        public ResourceRecord Copy()=>(ResourceRecord)MemberwiseClone();
    }
    [Serializable] public sealed class ExplorationSnapshot { public int seed; public ResourceRecord[] nodes; }
    public sealed class ExplorationModel
    {
        public const int HalfExtent=28;
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
            return new ExplorationModel(seed,records);
        }
        public ExplorationSnapshot Snapshot()=>new ExplorationSnapshot{seed=Seed,nodes=Nodes.ToArray()};
        public static ExplorationModel Restore(ExplorationSnapshot saved)
        {
            if(saved==null||saved.seed==0||saved.nodes==null||saved.nodes.Length>512)throw new ArgumentException("Invalid exploration world.");
            var ids=new HashSet<int>();var cells=new HashSet<(int,int)>();
            foreach(var n in saved.nodes)
            {
                if(n==null||n.id<1||!ids.Add(n.id)||!cells.Add((n.x,n.z))||!Enum.IsDefined(typeof(ResourceKind),n.kind)
                    ||Math.Abs((long)n.x)>26||Math.Abs((long)n.z)>26||Math.Max(Math.Abs(n.x),Math.Abs(n.z))<11)
                    throw new ArgumentException("Invalid resource identity or position.");
                var rules=ResourceRules.For(n.kind);
                if(n.hits<0||n.hits>rules.Hits||n.collected!=(n.hits==rules.Hits)||(n.kind==ResourceKind.Chest?(n.coins<30||n.coins>65):n.coins!=0))
                    throw new ArgumentException("Invalid resource progress or loot.");
            }
            return new ExplorationModel(saved.seed,saved.nodes);
        }
        internal void Hit(int id){var n=nodes[id];n.hits++;n.collected=n.hits==ResourceRules.For(n.kind).Hits;Revision++;}
    }
}
