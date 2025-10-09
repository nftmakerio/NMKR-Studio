using System.Linq;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace NMKR.Shared.Functions.SaleConditions.Conditions
{
    internal class CheckOnlyOneSale : ISaleConditions
    {
        public bool CheckCondition(EasynftprojectsContext db, IConnectionMultiplexer redis,
            Nftprojectsalecondition nftprojectsalecondition, string receiveraddress, string stakeaddress,
            int addrNftprojectId, long countnft, AssetsAssociatedWithAccount[] assets,
            ref CheckConditionsResultClass res)
        {
            if (nftprojectsalecondition.Condition != nameof(SaleConditionsTypes.onlyonesale)) return true;

            var transactions = (from a in db.Transactions
                where a.NftprojectId == addrNftprojectId && a.Stakeaddress == stakeaddress
                select a).AsNoTracking().FirstOrDefault();

            if (transactions != null)
            {
                res.RejectParameter = nftprojectsalecondition?.Policyprojectname;
                res.RejectReason = nftprojectsalecondition?.Condition;
            }

            return transactions == null;

        }
    }
}
