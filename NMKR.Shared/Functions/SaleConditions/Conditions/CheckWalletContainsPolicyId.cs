using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Model;
using StackExchange.Redis;

namespace NMKR.Shared.Functions.SaleConditions.Conditions
{
    internal class CheckWalletContainsPolicyId : ISaleConditions
    {
        public bool CheckCondition(EasynftprojectsContext db, IConnectionMultiplexer redis, Nftprojectsalecondition nftprojectsalecondition, string receiveraddress, string stakeaddress,
            int addrNftprojectId, long countnft, AssetsAssociatedWithAccount[] assets, ref CheckConditionsResultClass res)
        {
                bool met1 = true;
                if (nftprojectsalecondition.Condition == nameof(SaleConditionsTypes.walletcontainspolicyid))
                {
                    met1 = false;
                    if (assets != null)
                    {
                        foreach (var asset in assets)
                        {
                            met1 = CheckForPolicyId(nftprojectsalecondition.Policyid, asset, met1);
                            met1 = CheckForPolicyId(nftprojectsalecondition.Policyid2, asset, met1);
                            met1 = CheckForPolicyId(nftprojectsalecondition.Policyid3, asset, met1);
                            met1 = CheckForPolicyId(nftprojectsalecondition.Policyid4, asset, met1);
                            met1 = CheckForPolicyId(nftprojectsalecondition.Policyid5, asset, met1);
                            met1 = CheckForPolicyId(nftprojectsalecondition.Policyid6, asset, met1);
                            met1 = CheckForPolicyId(nftprojectsalecondition.Policyid7, asset, met1);
                            met1 = CheckForPolicyId(nftprojectsalecondition.Policyid8, asset, met1);
                            met1 = CheckForPolicyId(nftprojectsalecondition.Policyid9, asset, met1);
                            met1 = CheckForPolicyId(nftprojectsalecondition.Policyid10, asset, met1);
                            met1 = CheckForPolicyId(nftprojectsalecondition.Policyid11, asset, met1);
                            met1 = CheckForPolicyId(nftprojectsalecondition.Policyid12, asset, met1);
                            met1 = CheckForPolicyId(nftprojectsalecondition.Policyid13, asset, met1);
                            met1 = CheckForPolicyId(nftprojectsalecondition.Policyid14, asset, met1);
                            met1 = CheckForPolicyId(nftprojectsalecondition.Policyid15, asset, met1);
                        }
                    }

                    if (met1 == false)
                    {
                        res.RejectParameter = nftprojectsalecondition.Policyprojectname;
                        res.RejectReason = nftprojectsalecondition.Condition;
                    }
                }

                return met1;
            }

        private static bool CheckForPolicyId(string policyid, AssetsAssociatedWithAccount asset,
            bool met1)
        {
            if (asset.Unit != null && !string.IsNullOrEmpty(policyid) &&
                asset.Unit.Contains(policyid))
            {
                met1 = true;
            }

            return met1;
        }
    }
}
