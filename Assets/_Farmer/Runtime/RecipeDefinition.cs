using UnityEngine;
namespace Farmer
{
    [CreateAssetMenu(menuName="Farmer/Recipe")]
    public sealed class RecipeDefinition : ScriptableObject
    {
        public string id, displayName;
        public int catalogOrder=100;
        [Min(1)] public int outputCount=1, salePrice=90;
        public ItemStack[] ingredients;
        public RecipeRules Rules => new RecipeRules(id, outputCount, salePrice, ingredients);
    }
}
