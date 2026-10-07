using UnityEngine;

namespace Farmer
{
    [CreateAssetMenu(menuName = "Farmer/Build Piece")]
    public sealed class BuildDefinition : ScriptableObject
    {
        public string id = "wood_block";
        public string displayName = "Ahşap blok";
        [Min(1)] public int woodCost = 2;
        public GameObject prefab;
        public BuildRules Rules => new BuildRules(id, woodCost);
    }
}
