using NMKR.Shared.Classes;
using NMKR.Shared.Model;
using StackExchange.Redis;

namespace NMKR.Shared.Functions.SaleConditions
{
    public interface ISaleConditions
    {
        public bool CheckCondition(EasynftprojectsContext db,IConnectionMultiplexer redis, Nftprojectsalecondition nftprojectsalecondition,
            string receiveraddress, string stakeaddress, int addrNftprojectId, long countnft, AssetsAssociatedWithAccount[] assets,
            ref CheckConditionsResultClass res);
    }
}