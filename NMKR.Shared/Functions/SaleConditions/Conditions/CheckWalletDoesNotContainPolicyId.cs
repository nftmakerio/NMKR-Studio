using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Model;
using StackExchange.Redis;

namespace NMKR.Shared.Functions.SaleConditions.Conditions
{
    internal class CheckWalletDoesNotContainPolicyId : ISaleConditions
    {
        public bool CheckCondition(EasynftprojectsContext db, IConnectionMultiplexer redis, Nftprojectsalecondition nftprojectsalecondition, string receiveraddress, string stakeaddress,
            int addrNftprojectId, long countnft, AssetsAssociatedWithAccount[] assets, ref CheckConditionsResultClass res)
        {
            {
                bool met3 = true;
                if (nftprojectsalecondition.Condition == nameof(SaleConditionsTypes.walletdoesnotcontainpolicyid))
                {
                    if (assets != null)
                    {
                        foreach (var asset in assets)
                        {
                            if (asset == null)
                                continue;

                            met3 = CheckPolicyid(nftprojectsalecondition.Policyid, asset, met3);
                            met3 = CheckPolicyid(nftprojectsalecondition.Policyid2, asset, met3);
                            met3 = CheckPolicyid(nftprojectsalecondition.Policyid3, asset, met3);
                            met3 = CheckPolicyid(nftprojectsalecondition.Policyid4, asset, met3);
                            met3 = CheckPolicyid(nftprojectsalecondition.Policyid5, asset, met3);
                            met3 = CheckPolicyid(nftprojectsalecondition.Policyid6, asset, met3);
                            met3 = CheckPolicyid(nftprojectsalecondition.Policyid7, asset, met3);
                            met3 = CheckPolicyid(nftprojectsalecondition.Policyid8, asset, met3);
                            met3 = CheckPolicyid(nftprojectsalecondition.Policyid9, asset, met3);
                            met3 = CheckPolicyid(nftprojectsalecondition.Policyid10, asset, met3);
                            met3 = CheckPolicyid(nftprojectsalecondition.Policyid11, asset, met3);
                            met3 = CheckPolicyid(nftprojectsalecondition.Policyid12, asset, met3);
                            met3 = CheckPolicyid(nftprojectsalecondition.Policyid13, asset, met3);
                            met3 = CheckPolicyid(nftprojectsalecondition.Policyid14, asset, met3);
                            met3 = CheckPolicyid(nftprojectsalecondition.Policyid15, asset, met3);

                            if (met3 == false)
                            {
                                res.RejectParameter = nftprojectsalecondition?.Policyprojectname;
                                res.RejectReason = nftprojectsalecondition?.Condition;
                            }
                        }
                    }
                }

                return met3;
            }

        }

        private static bool CheckPolicyid(string policyid, AssetsAssociatedWithAccount asset,
            bool met3)
        {
            if (asset.Unit != null && !string.IsNullOrEmpty(policyid) &&
                asset.Unit.Contains(policyid))
            {
                met3 = false;
            }

            return met3;
        }
    }
}
