using System.Collections.Generic;
using UnityEngine;
namespace Farmer
{
    public sealed class NpcActor : MonoBehaviour
    {
        [Tooltip("Bu dünya karakterinin adı, portresi, konuşmaları ve görev sırası.")]
        public NpcDefinition definition;
        public static readonly List<NpcActor> Active=new List<NpcActor>();
        private void OnEnable(){if(!Active.Contains(this))Active.Add(this);}
        private void OnDisable()=>Active.Remove(this);
    }
}
