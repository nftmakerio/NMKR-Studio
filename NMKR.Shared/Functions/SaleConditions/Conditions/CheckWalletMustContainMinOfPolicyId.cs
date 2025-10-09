using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Model;
using StackExchange.Redis;

namespace NMKR.Shared.Functions.SaleConditions.Conditions
{
    internal class CheckWalletMustContainMinOfPolicyId : ISaleConditions
    {
        public bool CheckCondition(EasynftprojectsContext db, IConnectionMultiplexer redis, Nftprojectsalecondition nftprojectsalecondition, string receiveraddress, string stakeaddress,
            int addrNftprojectId, long countnft, AssetsAssociatedWithAccount[] assets, ref CheckConditionsResultClass res)
        {
            {
                bool met5 = true;
                if (nftprojectsalecondition.Condition == nameof(SaleConditionsTypes.walletmustcontainminofpolicyid))
                {
                    met5 = false;
                    long countp1 = 0;
                    long countp2 = 0;
                    long countp3 = 0;
                    long countp4 = 0;
                    long countp5 = 0;
                    long countp6 = 0;
                    long countp7 = 0;
                    long countp8 = 0;
                    long countp9 = 0;
                    long countp10 = 0;
                    long countp11 = 0;
                    long countp12 = 0;
                    long countp13 = 0;
                    long countp14 = 0;
                    long countp15 = 0;

                    if (assets != null)
                    {
                        foreach (var asset in assets)
                        {
                            if (asset == null)
                                continue;
                           
                            countp1 = CheckPolicyId(nftprojectsalecondition.Policyid, asset, countp1);
                            countp2 = CheckPolicyId(nftprojectsalecondition.Policyid2, asset, countp2);
                            countp3 = CheckPolicyId(nftprojectsalecondition.Policyid3, asset, countp3);
                            countp4 = CheckPolicyId(nftprojectsalecondition.Policyid4, asset, countp4);
                            countp5 = CheckPolicyId(nftprojectsalecondition.Policyid5, asset, countp5);
                            countp6 = CheckPolicyId(nftprojectsalecondition.Policyid6, asset, countp6);
                            countp7 = CheckPolicyId(nftprojectsalecondition.Policyid7, asset, countp7);
                            countp8 = CheckPolicyId(nftprojectsalecondition.Policyid8, asset, countp8);
                            countp9 = CheckPolicyId(nftprojectsalecondition.Policyid9, asset, countp9);
                            countp10 = CheckPolicyId(nftprojectsalecondition.Policyid10, asset, countp10);
                            countp11 = CheckPolicyId(nftprojectsalecondition.Policyid11, asset, countp11);
                            countp12 = CheckPolicyId(nftprojectsalecondition.Policyid12, asset, countp12);
                            countp13 = CheckPolicyId(nftprojectsalecondition.Policyid13, asset, countp13);
                            countp14 = CheckPolicyId(nftprojectsalecondition.Policyid14, asset, countp14);
                            countp15 = CheckPolicyId(nftprojectsalecondition.Policyid15, asset, countp15);

                        }
                    }

                    long minimum = nftprojectsalecondition.Maxvalue != null
                        ? (long)nftprojectsalecondition.Maxvalue
                        : 1;
                    if (minimum == 0)
                        minimum = 1;

                    met5 = (countp1 >= minimum || countp2 >= minimum || countp3 >= minimum || countp4 >= minimum ||
                            countp5 >= minimum || countp6>=minimum || countp7 >= minimum || countp8 >= minimum || 
                            countp9 >= minimum || countp10 >= minimum || countp11 >= minimum || countp12 >= minimum || 
                            countp13 >= minimum || countp14 >= minimum || countp15 >= minimum) ;

                    if (met5 == false)
                    {
                        res.RejectParameter = nftprojectsalecondition?.Policyprojectname;
                        res.RejectReason = nftprojectsalecondition?.Condition;
                    }
                }

                return met5;
            }

        }

        private static long CheckPolicyId(string policyid, AssetsAssociatedWithAccount asset,
            long countp2)
        {
            if (asset.Unit != null && !string.IsNullOrEmpty(policyid) &&
                asset.Unit.Contains(policyid))
            {
                if (asset.Quantity != null)
                    countp2 += (long)asset.Quantity;
            }

            return countp2;
        }
    }
}
