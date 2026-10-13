using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Farmer.Editor
{
    public static class QuestContentValidation
    {
        [MenuItem("Farmer/Validate Quest Content")]
        public static void Validate()
        {
            var npcs=Resources.LoadAll<NpcDefinition>("Npcs");
            foreach(var n in npcs)
            {
                if(string.IsNullOrWhiteSpace(n.displayName)||Resources.Load<Texture2D>("VillageArt/"+n.portraitId)==null)
                    throw new ArgumentException("NPC name/portrait missing: "+n.name);
                foreach(var q in n.quests)
                {
                    if(q==null)throw new ArgumentException("Empty quest slot: "+n.name);
                    foreach(var lines in new[]{q.offer,q.accepted,q.inProgress,q.readyToDeliver,q.completed})
                        if(lines==null||lines.Length==0||lines.Any(l=>l==null||string.IsNullOrWhiteSpace(l.text)))
                            Debug.LogWarning("Some dialogue uses a fallback: "+q.name,q);
                }
            }
            var crops=AssetDatabase.FindAssets("t:CropDefinition").Select(g=>AssetDatabase.LoadAssetAtPath<CropDefinition>(AssetDatabase.GUIDToAssetPath(g))).Select(c=>c.Rules);
            var recipes=Resources.LoadAll<RecipeDefinition>("Recipes").Select(r=>r.Rules);
            var builds=AssetDatabase.FindAssets("t:BuildDefinition").Select(g=>AssetDatabase.LoadAssetAtPath<BuildDefinition>(AssetDatabase.GUIDToAssetPath(g))).Select(b=>b.Rules);
            var quests=QuestCatalog.Build(npcs);
            _=new FarmModel(crops,buildCatalog:builds,recipeCatalog:recipes,questCatalog:quests,worldSeed:1);
            Debug.Log($"FARMER_QUEST_CONTENT_OK: {npcs.Length} NPC, {quests.Length} quest.");
        }
    }
}
