using System;
using System.Collections.Generic;
using System.Linq;
namespace Farmer
{
    public static class QuestCatalog
    {
        public static QuestRules[] Build(IEnumerable<NpcDefinition> characters)
        {
            var result=new List<QuestRules>();var npcs=new HashSet<string>();
            foreach(var npc in characters)
            {
                if(npc==null||string.IsNullOrWhiteSpace(npc.id)||!npcs.Add(npc.id))throw new ArgumentException("Missing or duplicate NPC id.");
                string previous=null;
                foreach(var q in npc.quests??Array.Empty<QuestDefinition>())
                {
                    if(q==null)throw new ArgumentException("Empty quest slot on "+npc.id);
                    result.Add(new QuestRules(q.id,q.title,q.description,previous,npc.id,npc.displayName,q.rewardMoney,
                        (q.requiredItems??Array.Empty<QuestItem>()).Select(r=>r.Rule).ToArray(),(q.rewardItems??Array.Empty<QuestItem>()).Select(r=>r.Rule).ToArray()));
                    previous=q.id;
                }
            }
            return result.ToArray();
        }
    }
}
