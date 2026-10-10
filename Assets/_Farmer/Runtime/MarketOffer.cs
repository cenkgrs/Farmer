using System;
using System.Collections.Generic;
using System.Linq;
namespace Farmer
{
    public enum MarketCategory { Seeds, Tools, Materials, Furniture, Production }
    public enum MarketOfferKind { Seed, Pickaxe, Wood, Bed, Furniture, Recipe }

    // Catalog data and transaction limits are separate from the UI's artwork and layout.
    public sealed class MarketOffer
    {
        public string Id { get; }
        public string Name { get; }
        public int Price { get; }
        public MarketCategory Category { get; }
        public MarketOfferKind Kind { get; }
        public CropDefinition Crop { get; }
        public RecipeDefinition Recipe { get; }
        public MarketOffer(string id,string name,int price,MarketCategory category,MarketOfferKind kind,CropDefinition crop=null,RecipeDefinition recipe=null)
        {Id=id;Name=name;Price=price;Category=category;Kind=kind;Crop=crop;Recipe=recipe;}
        public bool HasQuantity => Kind==MarketOfferKind.Seed||Kind==MarketOfferKind.Recipe;
        public int Count(FarmModel model)
        {
            switch(Kind)
            {
                case MarketOfferKind.Seed:return model.Seeds(Id);
                case MarketOfferKind.Pickaxe:return model.OwnsPickaxe?1:0;
                case MarketOfferKind.Wood:return model.Building.Wood;
                case MarketOfferKind.Bed:return model.Building.Beds;
                case MarketOfferKind.Furniture:return model.Building.FurnitureCount(Id);
                default:return model.BagCount("crafted:"+Id);
            }
        }
        public bool CanPurchase(FarmModel model,int quantity)
        {
            if(quantity<1||quantity>FarmModel.StackLimit||(!HasQuantity&&quantity!=1))return false;
            if(Kind==MarketOfferKind.Recipe)return model.CanCraft(Id,quantity);
            if((long)Price*quantity>model.Money)return false;
            switch(Kind)
            {
                case MarketOfferKind.Seed:return Count(model)<=FarmModel.StackLimit-quantity;
                case MarketOfferKind.Pickaxe:return !model.OwnsPickaxe;
                case MarketOfferKind.Wood:return Count(model)<=BuildingModel.WoodLimit-BuildingModel.WoodPackCount;
                default:return Count(model)<BuildingModel.WoodLimit;
            }
        }
        public bool Purchase(FarmGame game,int quantity)
        {
            if(!game.CanTrade||!CanPurchase(game.Model,quantity))return false;
            switch(Kind)
            {
                case MarketOfferKind.Seed:return game.SelectCrop(Id)&&game.Buy(quantity);
                case MarketOfferKind.Pickaxe:return game.BuyPickaxe();
                case MarketOfferKind.Wood:return game.BuyWood();
                case MarketOfferKind.Bed:return game.BuyBed();
                case MarketOfferKind.Furniture:return game.BuyFurniture(Id);
                default:return game.Craft(Id,quantity);
            }
        }
        public static MarketOffer[] Create(FarmGame game)
        {
            var offers=new List<MarketOffer>();
            offers.AddRange(game.Crops.Select(c=>new MarketOffer(c.id,c.displayName+" tohumu",c.seedPrice,MarketCategory.Seeds,MarketOfferKind.Seed,c)));
            offers.Add(new MarketOffer("pickaxe","Kazma",FarmModel.PickaxePrice,MarketCategory.Tools,MarketOfferKind.Pickaxe));
            offers.Add(new MarketOffer("wood","10 odun",BuildingModel.WoodPackPrice,MarketCategory.Materials,MarketOfferKind.Wood));
            offers.Add(new MarketOffer("bed","Yatak",BuildingModel.BedPrice,MarketCategory.Furniture,MarketOfferKind.Bed));
            offers.AddRange(game.BuildPieces.Where(b=>b.price>0&&!b.isBed).Select(b=>new MarketOffer(b.id,b.displayName,b.price,MarketCategory.Furniture,MarketOfferKind.Furniture)));
            offers.AddRange(game.Recipes.Select(r=>new MarketOffer(r.id,r.displayName,0,MarketCategory.Production,MarketOfferKind.Recipe,null,r)));
            return offers.ToArray();
        }
    }
}
