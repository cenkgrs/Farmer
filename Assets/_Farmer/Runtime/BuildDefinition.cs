using UnityEngine;

namespace Farmer
{
    [CreateAssetMenu(menuName = "Farmer/Build Piece")]
    public sealed class BuildDefinition : ScriptableObject
    {
        public string id = "wood_block";
        public string displayName = "Ahşap blok";
        [Min(0)] public int woodCost = 2;
        [Min(0)] public int stoneCost;
        public bool isOutdoor;
        public GameObject prefab;
        public bool isBed;
        public int price;
        public int footprintLength=1;
        public bool IsFurniture => isBed || price>0;
        public BuildPlacement placement;
        public bool isDoor;
        public BuildRules Rules => new BuildRules(id, woodCost, isBed, placement, isDoor, price, footprintLength, stoneCost, isOutdoor);
    }
}
