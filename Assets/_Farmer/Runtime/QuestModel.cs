using System;
using System.Collections.Generic;
using System.Linq;

namespace Farmer
{
    public enum QuestStatus { Locked, Available, Active, Completed }
    [Serializable] public sealed class QuestRecord { public string id; public bool completed; }
    public sealed class QuestRequirement
    {
        public readonly string Item, Label; public readonly int Count;
        public QuestRequirement(string item,string label,int count){Item=item;Label=label;Count=count;}
    }
    public sealed class QuestRules
    {
        public readonly string Id, Title, Description, Prerequisite, NpcId, NpcName;
        public readonly IReadOnlyList<QuestRequirement> Rewards;
        public readonly int Money;
        public readonly IReadOnlyList<QuestRequirement> Requirements;
        public QuestRules(string id,string title,string description,string prerequisite,string npcId,string npcName,int money,QuestRequirement[] requirements,QuestRequirement[] rewards)
        {
            if(string.IsNullOrWhiteSpace(id)||string.IsNullOrWhiteSpace(npcId)||string.IsNullOrWhiteSpace(title)||money<0||money>FarmModel.MoneyLimit||requirements==null||requirements.Length==0||rewards==null)throw new ArgumentException("Invalid quest definition: "+id);
            foreach(var group in new[]{requirements,rewards})
                if(group.Any(r=>r==null||string.IsNullOrWhiteSpace(r.Item)||string.IsNullOrWhiteSpace(r.Label)||r.Count<1||r.Count>FarmModel.StackLimit)||group.Select(r=>r.Item).Distinct().Count()!=group.Length)throw new ArgumentException("Invalid quest items: "+id);
            Id=id;Title=title;Description=description;Prerequisite=prerequisite;NpcId=npcId;NpcName=npcName;Money=money;Requirements=Array.AsReadOnly(requirements);Rewards=Array.AsReadOnly(rewards);
        }
        public string RewardText => string.Join(" + ",(Money>0?new[]{Money+" para"}:Array.Empty<string>()).Concat(Rewards.Select(r=>r.Count+" "+r.Label)));

    }
    public sealed partial class FarmModel
    {
        public IReadOnlyList<QuestRules> QuestCatalog { get; private set; }=Array.Empty<QuestRules>();
        public QuestRules FindQuest(string id)=>QuestCatalog.FirstOrDefault(q=>q.Id==id);
        private void InitializeQuests(IEnumerable<QuestRules> catalog)
        {
            if(catalog==null)return;var rules=catalog.ToArray();
            if(rules.Select(q=>q.Id).Distinct().Count()!=rules.Length)throw new ArgumentException("Duplicate quest ID.");
            foreach(var q in rules)
            {
                if(q.Requirements.Concat(q.Rewards).Any(r=>!CanStore(r.Item)))throw new ArgumentException("Unknown quest item in "+q.Id);
                if(q.Prerequisite!=null&&!rules.Any(p=>p.Id==q.Prerequisite&&p.NpcId==q.NpcId))throw new ArgumentException("Missing quest predecessor.");
                var seen=new HashSet<string>();var current=q;
                while(current!=null){if(!seen.Add(current.Id))throw new ArgumentException("Quest cycle.");current=rules.FirstOrDefault(p=>p.Id==current.Prerequisite);}
            }
            QuestCatalog=Array.AsReadOnly(rules);
        }
        private readonly Dictionary<string,bool> quests=new Dictionary<string,bool>();
        public QuestStatus QuestState(string id)
        {
            var q=FindQuest(id);if(q==null)return QuestStatus.Locked;
            if(quests.TryGetValue(id,out var completed))return completed?QuestStatus.Completed:QuestStatus.Active;
            return q.Prerequisite==null||QuestState(q.Prerequisite)==QuestStatus.Completed?QuestStatus.Available:QuestStatus.Locked;
        }
        public bool QuestReady(string id)
        {
            var q=FindQuest(id);return q!=null&&QuestState(id)==QuestStatus.Active&&q.Requirements.All(r=>BagCount(r.Item)>=r.Count);
        }
        public bool AcceptQuest(string id,out string message)
        {
            if(QuestState(id)!=QuestStatus.Available)return Fail("Bu görev şu anda alınamaz.",out message);
            quests.Add(id,false);message="Görev alındı.";return true;
        }
        public bool TurnInQuest(string id,out string message)
        {
            if(!QuestReady(id))return Fail("Teslim edilecek ürünler çantanda eksik.",out message);
            var q=FindQuest(id);
            if(Money>MoneyLimit-q.Money||q.Rewards.Any(r=>!CanStore(r.Item)||(long)BagCount(r.Item)-q.Requirements.Where(i=>i.Item==r.Item).Sum(i=>i.Count)+r.Count>StackLimit))
                return Fail("Ödül için çantanda veya para miktarında yer aç.",out message);
            // Validate every input and reward before removing any inventory.
            foreach(var r in q.Requirements)ChangeBag(r.Item,-r.Count);
            Money+=q.Money;foreach(var r in q.Rewards)ChangeBag(r.Item,r.Count);
            quests[id]=true;message="Teşekkürler! "+q.RewardText+" çantana eklendi.";return true;
        }
        private QuestRecord[] QuestSave()=>quests.Select(p=>new QuestRecord{id=p.Key,completed=p.Value}).ToArray();
        private void RestoreQuests(QuestRecord[] records)
        {
            if(records==null||records.Length>4096)throw new ArgumentException("Invalid quest progress.");
            foreach(var r in records)
                if(r==null||string.IsNullOrWhiteSpace(r.id)||r.id.Length>128||quests.ContainsKey(r.id))throw new ArgumentException("Unknown or duplicate quest.");
                else quests.Add(r.id,r.completed);
            // Keep saved acceptance/completion if an author reorders or retires content.

        }
    }
}
