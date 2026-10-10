using UnityEngine;

namespace Farmer
{
    [CreateAssetMenu(menuName = "Farmer/Crop")]
    public sealed class CropDefinition : ScriptableObject
    {
        public string id = "turnip";
        public string displayName = "Turp";
        [Min(1)] public int seedPrice = 10;
        [Min(1)] public int salePrice = 18;
        [Min(3)] public int wateredDays = 3;
        [Min(1)] public int harvestYield = 1;
        [Range(.1f,2f)] public float visualScale = 1f;
        public GameObject[] growthStages = new GameObject[4];
        public CropRules Rules => new CropRules(id, seedPrice, salePrice, wateredDays, harvestYield);
    }
}
