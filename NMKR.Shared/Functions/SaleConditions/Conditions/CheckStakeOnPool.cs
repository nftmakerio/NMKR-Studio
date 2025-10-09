using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions.Koios;
using NMKR.Shared.Functions.Solana;
using NMKR.Shared.Model;
using StackExchange.Redis;

namespace NMKR.Shared.Functions.SaleConditions.Conditions
{
    internal class CheckStakeOnPool : ISaleConditions
    {
        public bool CheckCondition(EasynftprojectsContext db, IConnectionMultiplexer redis, Nftprojectsalecondition nftprojectsalecondition, string receiveraddress, string stakeaddress,
            int addrNftprojectId, long countnft, AssetsAssociatedWithAccount[] assets, ref CheckConditionsResultClass res)
        {
            {
                bool met7 = true;
                if (nftprojectsalecondition.Condition != nameof(SaleConditionsTypes.stakeonpool)) return met7;
                met7 = false;
                var stakepool = receiveraddress.ToLower().StartsWith("addr") ? KoiosFunctions.GetStakePoolInformation(redis, receiveraddress) : SolanaFunctions.GetStakePoolInformation(redis, receiveraddress);
                if (stakepool == null) return met7;
                if (string.IsNullOrEmpty(stakepool.DelegatedPool)) return met7;
                if (stakepool.DelegatedPool == nftprojectsalecondition.Policyid ||
                    stakepool.DelegatedPool == nftprojectsalecondition.Policyid2 ||
                    stakepool.DelegatedPool == nftprojectsalecondition.Policyid3 ||
                    stakepool.DelegatedPool == nftprojectsalecondition.Policyid4 ||
                    stakepool.DelegatedPool == nftprojectsalecondition.Policyid5 ||
                    stakepool.DelegatedPool == nftprojectsalecondition.Policyid6 ||
                    stakepool.DelegatedPool == nftprojectsalecondition.Policyid7 ||
                    stakepool.DelegatedPool == nftprojectsalecondition.Policyid8 ||
                    stakepool.DelegatedPool == nftprojectsalecondition.Policyid9 ||
                    stakepool.DelegatedPool == nftprojectsalecondition.Policyid10 ||
                    stakepool.DelegatedPool == nftprojectsalecondition.Policyid11 ||
                    stakepool.DelegatedPool == nftprojectsalecondition.Policyid12 ||
                    stakepool.DelegatedPool == nftprojectsalecondition.Policyid13 ||
                    stakepool.DelegatedPool == nftprojectsalecondition.Policyid14 ||
                    stakepool.DelegatedPool == nftprojectsalecondition.Policyid15)
                {
                    met7 = true;
                }

                return met7;
            }

        }
    }
}
