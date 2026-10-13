using System;
using UnityEngine;
namespace Farmer
{
    [Serializable] public sealed class QuestItem
    {
        [Tooltip("Çanta kimliği: crop:turnip, seed:carrot, crafted:vegetable_crate, wood, hunt:cooked_meat ...")]
        public string itemId;
        public string displayName;
        [Min(1)] public int count=1;
        public QuestRequirement Rule=>new QuestRequirement(itemId,displayName,count);
    }
    [Serializable] public sealed class DialogueLine
    {
        [Tooltip("İşaretliyse oyuncu portresi ve Sen adı kullanılır; değilse konuşulan NPC.")]
        public bool player;
        [TextArea(2,5)] public string text;
    }
    [CreateAssetMenu(menuName="Farmer/Quest")]
    public sealed class QuestDefinition : ScriptableObject
    {
        [Tooltip("Kalıcı ASCII kimlik. Yayınlandıktan sonra değiştirme.")]
        public string id;
        public string title;
        [TextArea(3,6)] public string description;
        public QuestItem[] requiredItems=Array.Empty<QuestItem>();
        [Min(0)] public int rewardMoney;
        public QuestItem[] rewardItems=Array.Empty<QuestItem>();
        [Header("Göreve özel konuşmalar — satırlar sırayla oynar")]
        public DialogueLine[] offer=Array.Empty<DialogueLine>();
        public DialogueLine[] accepted=Array.Empty<DialogueLine>();
        public DialogueLine[] inProgress=Array.Empty<DialogueLine>();
        public DialogueLine[] readyToDeliver=Array.Empty<DialogueLine>();
        public DialogueLine[] completed=Array.Empty<DialogueLine>();
    }
}
