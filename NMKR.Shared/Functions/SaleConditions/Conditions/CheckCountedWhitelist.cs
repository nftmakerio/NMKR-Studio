using System.Linq;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace NMKR.Shared.Functions.SaleConditions.Conditions
{
    internal class CheckCountedWhitelist : ISaleConditions
    {
        public bool CheckCondition(EasynftprojectsContext db, IConnectionMultiplexer redis, Nftprojectsalecondition nftprojectsalecondition, string receiveraddress,string stakeaddress,
            int addrNftprojectId, long countnft, AssetsAssociatedWithAccount[] assets, ref CheckConditionsResultClass res)
        {
            {
                bool met9 = true;
                if (nftprojectsalecondition.Condition == nameof(SaleConditionsTypes.countedwhitelistedaddresses))
                {
                    var countedWhitelists = (from a in db.Countedwhitelists
                              .Include(a => a.Countedwhitelistusedaddresses)
                              .AsSplitQuery()
                                             where a.SaleconditionsId == nftprojectsalecondition.Id
                                                 && (a.Address == receiveraddress || (a.Stakeaddress == stakeaddress && !string.IsNullOrEmpty(stakeaddress)))
                                             select a).AsNoTracking().FirstOrDefault();

                    if (countedWhitelists == null)
                    {
                        met9 = false;
                    }
                    else
                    {
                        long bought = 0;
                        if (countedWhitelists.Countedwhitelistusedaddresses != null &&
                            countedWhitelists.Countedwhitelistusedaddresses.Any())
                        {
                            bought = countedWhitelists.Countedwhitelistusedaddresses.Sum(x => x.Countnft);
                        }

                        if (bought + countnft > countedWhitelists.Maxcount)
                        {
                            met9 = false;
                        }
                    }
                }
                if (met9 == false)
                {
                    res.RejectParameter = nftprojectsalecondition?.Policyprojectname;
                    res.RejectReason = nftprojectsalecondition?.Condition;
                }
                return met9;
            }

        }
    }
}
