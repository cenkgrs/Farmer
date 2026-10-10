using System.Linq;
namespace Farmer
{
    public sealed partial class FarmModel
    {
        // Stone remains in the farm inventory. Structural checks and inventory mutation
        // finish together so a rejected placement/removal never spends or refunds it.
        public bool CanPlaceStructure(string id,int x,int level,int z,int rotation,out string message)
        {
            if(!Building.CanPlace(id,x,level,z,rotation,out message,availableStone:Stone))return false;
            var rule=Building.Rules(id);int soil=IndexAt(x,z);
            if(level==0&&!(rule.IsOutdoor&&rule.Placement==BuildPlacement.Edge)&&soil>=0&&!string.IsNullOrEmpty(Plot(soil).cropId))return Fail("Önce buradaki ürünü hasat et.",out message);
            return true;
        }
        public bool PlaceStructure(string id,int x,int level,int z,int rotation,out string message)
        {
            if(!CanPlaceStructure(id,x,level,z,rotation,out message))return false;
            if(!Building.Place(id,x,level,z,rotation,out message,Stone))return false;
            int cost=Building.Rules(id).StoneCost;Stone-=cost;
            if(cost>0)message=$"Yol döşendi. −{cost} taş.";
            return true;
        }
        public bool RemoveStructure(BlockRecord record,out string message)
        {
            if(record==null)return Fail("Sökmek için bir parçayı hedefle.",out message);
            var actual=Building.Blocks.FirstOrDefault(b=>b.pieceId==record.pieceId&&b.x==record.x&&b.z==record.z&&b.level==record.level&&b.rotation==record.rotation);
            if(actual==null)return Fail("Yapı bulunamadı.",out message);
            int refund=Building.Rules(actual.pieceId).StoneCost;
            if(!Building.Remove(actual,out message,Stone))return false;
            Stone+=refund;if(refund>0)message=$"Yol söküldü. +{refund} taş.";return true;
        }
    }
}
