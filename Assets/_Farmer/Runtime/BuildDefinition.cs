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
        public bool isBed;
        public int price;
        public int footprintLength=1;
        public bool IsFurniture => isBed || price>0;
        public BuildPlacement placement;
        public bool isDoor;
        public BuildRules Rules => new BuildRules(id, woodCost, isBed, placement, isDoor, price, footprintLength);
    }
}
