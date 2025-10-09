using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Model;
using StackExchange.Redis;

namespace NMKR.Shared.Functions.SaleConditions.Conditions
{
    internal class CheckWalletContainsMinPolicyId : ISaleConditions
    {
        public bool CheckCondition(EasynftprojectsContext db, IConnectionMultiplexer redis, Nftprojectsalecondition nftprojectsalecondition, string receiveraddress, string stakeaddress,
            int addrNftprojectId, long countnft, AssetsAssociatedWithAccount[] assets, ref CheckConditionsResultClass res)
        {
            {
                bool met4 = true;
                if (nftprojectsalecondition.Condition == nameof(SaleConditionsTypes.walletcontainsminpolicyid))
                {
                    long countpolicy1 = 0;
                    if (assets != null)
                    {
                        foreach (var asset in assets)
                        {
                            if (asset == null)
                                continue;
                           
                            countpolicy1 = CountpolicyId(nftprojectsalecondition.Policyid, asset, countpolicy1);
                            countpolicy1 = CountpolicyId(nftprojectsalecondition.Policyid2, asset, countpolicy1);
                            countpolicy1 = CountpolicyId(nftprojectsalecondition.Policyid3, asset, countpolicy1);
                            countpolicy1 = CountpolicyId(nftprojectsalecondition.Policyid4, asset, countpolicy1);
                            countpolicy1 = CountpolicyId(nftprojectsalecondition.Policyid5, asset, countpolicy1);
                            countpolicy1 = CountpolicyId(nftprojectsalecondition.Policyid6, asset, countpolicy1);
                            countpolicy1 = CountpolicyId(nftprojectsalecondition.Policyid7, asset, countpolicy1);
                            countpolicy1 = CountpolicyId(nftprojectsalecondition.Policyid8, asset, countpolicy1);
                            countpolicy1 = CountpolicyId(nftprojectsalecondition.Policyid9, asset, countpolicy1);
                            countpolicy1 = CountpolicyId(nftprojectsalecondition.Policyid10, asset, countpolicy1);
                            countpolicy1 = CountpolicyId(nftprojectsalecondition.Policyid11, asset, countpolicy1);
                            countpolicy1 = CountpolicyId(nftprojectsalecondition.Policyid12, asset, countpolicy1);
                            countpolicy1 = CountpolicyId(nftprojectsalecondition.Policyid13, asset, countpolicy1);
                            countpolicy1 = CountpolicyId(nftprojectsalecondition.Policyid14, asset, countpolicy1);
                            countpolicy1 = CountpolicyId(nftprojectsalecondition.Policyid15, asset, countpolicy1);
                        }
                    }

                    met4 =  countpolicy1>= countnft; // + countnft;

                    if (met4 == false)
                    {
                        res.RejectParameter = nftprojectsalecondition?.Policyprojectname;
                        res.RejectReason = nftprojectsalecondition?.Condition;
                    }
                }

                return met4;
            }

        }

        private static long CountpolicyId(string policyid, AssetsAssociatedWithAccount asset,
            long countpolicy1)
        {
            if (asset.Unit != null && !string.IsNullOrEmpty(policyid) && asset.Unit.Contains(policyid))
            {
                    countpolicy1 += asset.Quantity;
            }

            return countpolicy1;
        }
    }
}
