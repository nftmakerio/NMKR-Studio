using System;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions.Blockfrost;
using NMKR.Shared.Model;
using StackExchange.Redis;

namespace NMKR.Shared.Functions.SaleConditions.Conditions
{
    internal class CheckBlacklistAddresses : ISaleConditions
    {
        public bool CheckCondition(EasynftprojectsContext db, IConnectionMultiplexer redis, Nftprojectsalecondition nftprojectsalecondition, string receiveraddress, string stakeaddress,
            int addrNftprojectId, long countnft, AssetsAssociatedWithAccount[] assets, ref CheckConditionsResultClass res)
        {
            {
                bool met8 = true;
                if (nftprojectsalecondition.Condition == nameof(SaleConditionsTypes.blacklistedaddresses) &&
                                                                                   nftprojectsalecondition.Blacklistedaddresses != null)
                {
                    // First check if the address is blacklisted
                    if (nftprojectsalecondition.Blacklistedaddresses.Contains(receiveraddress))
                    {
                        met8 = false;
                    }


                    // Then check if the stakekey is blacklisted
                    if (met8 && !string.IsNullOrEmpty(stakeaddress))
                    {
                        if (nftprojectsalecondition.Blacklistedaddresses.Contains(stakeaddress))
                        {
                            met8 = false;
                        }
                    }


                    // Check all addresses associated with the stake key
                    if (met8)
                    {
                        if (!string.IsNullOrEmpty(stakeaddress))
                        {
                            foreach (var address in nftprojectsalecondition.Blacklistedaddresses.Split(
                                         new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                var address1 = address.FilterToLetterOrDigit();
                                var stake2 = Bech32Engine.GetStakeFromAddress(address1);
                                if (string.IsNullOrEmpty(stake2) || stake2 != stakeaddress) continue;

                                met8 = false;
                                break;
                            }
                        }
                    }


                    if (met8)
                    {
                        // If there was no stake address check all addresses received from blockfrost associated with the receiver address
                        var adrlist = BlockfrostFunctions.GetAllAddressesFromSingleAddress(redis, receiveraddress);
                        if (adrlist != null)
                        {
                            foreach (var getAddressesFromStakeClass in adrlist)
                            {
                                if (nftprojectsalecondition.Blacklistedaddresses.Contains(getAddressesFromStakeClass
                                        .Address))
                                {
                                    met8 = false;
                                    break;
                                }
                            }
                        }
                    }

                    if (met8 == false)
                    {
                        res.RejectParameter = nftprojectsalecondition?.Policyprojectname;
                        res.RejectReason = nftprojectsalecondition?.Condition;
                    }
                }

                return met8;
            }

        }
    }
}
