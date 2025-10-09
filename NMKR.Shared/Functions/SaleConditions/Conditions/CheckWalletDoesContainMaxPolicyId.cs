using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Model;
using StackExchange.Redis;

namespace NMKR.Shared.Functions.SaleConditions.Conditions
{
    internal class CheckWalletDoesContainMaxPolicyId :ISaleConditions
    {
        public bool CheckCondition(EasynftprojectsContext db, IConnectionMultiplexer redis, Nftprojectsalecondition nftprojectsalecondition, string receiveraddress, string stakeaddress,
            int addrNftprojectId, long countnft, AssetsAssociatedWithAccount[] assets, ref CheckConditionsResultClass res)
        {
            {
                bool met2 = true;

                // Then Check count against blockfrost
                if (met2 && nftprojectsalecondition.Condition == nameof(SaleConditionsTypes.walletdoescontainmaxpolicyid))
                {
                    int count = 0;
                    if (assets != null)
                    {
                        foreach (var asset in assets)
                        {
                            if (asset == null)
                                continue;
                            count = CountPolicyId(nftprojectsalecondition.Policyid, asset, count);
                            count = CountPolicyId(nftprojectsalecondition.Policyid2, asset, count);
                            count = CountPolicyId(nftprojectsalecondition.Policyid3, asset, count);
                            count = CountPolicyId(nftprojectsalecondition.Policyid4, asset, count);
                            count = CountPolicyId(nftprojectsalecondition.Policyid5, asset, count);
                            count = CountPolicyId(nftprojectsalecondition.Policyid6, asset, count);
                            count = CountPolicyId(nftprojectsalecondition.Policyid7, asset, count);
                            count = CountPolicyId(nftprojectsalecondition.Policyid8, asset, count);
                            count = CountPolicyId(nftprojectsalecondition.Policyid9, asset, count);
                            count = CountPolicyId(nftprojectsalecondition.Policyid10, asset, count);
                            count = CountPolicyId(nftprojectsalecondition.Policyid11, asset, count);
                            count = CountPolicyId(nftprojectsalecondition.Policyid12, asset, count);
                            count = CountPolicyId(nftprojectsalecondition.Policyid13, asset, count);
                            count = CountPolicyId(nftprojectsalecondition.Policyid14, asset, count);
                            count = CountPolicyId(nftprojectsalecondition.Policyid15, asset, count);
                        }
                    }

                    met2 = (count+countnft <= nftprojectsalecondition.Maxvalue);
                    if (met2 == false)
                    {
                        res.RejectParameter = nftprojectsalecondition?.Policyprojectname;
                        res.RejectReason = nftprojectsalecondition?.Condition;
                    }
                }

                return met2;

            }

        }

        private static int CountPolicyId(string policyid, AssetsAssociatedWithAccount asset,
            int count)
        {
            if (asset.Unit != null && !string.IsNullOrEmpty(policyid) &&
                asset.Unit.Contains(policyid))
            {
                count++;
            }

            return count;
        }
    }
}
