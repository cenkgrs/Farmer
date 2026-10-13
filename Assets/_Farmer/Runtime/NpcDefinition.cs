using System;
using UnityEngine;
namespace Farmer
{
    [CreateAssetMenu(menuName="Farmer/NPC")]
    public sealed class NpcDefinition : ScriptableObject
    {
        public string id, displayName;
        [Tooltip("Resources/VillageArt içindeki portre adı (uzantısız).")]
        public string portraitId;
        public bool opensMarket;
        public DialogueLine[] greeting=Array.Empty<DialogueLine>();
        [Tooltip("Bu karakterin görev sırası. Önceki görev teslim edilmeden sıradaki açılamaz.")]
        public QuestDefinition[] quests=Array.Empty<QuestDefinition>();
    }
}
