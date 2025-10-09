using System;
using System.Linq;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions.Blockfrost;
using NMKR.Shared.Model;
using StackExchange.Redis;

namespace NMKR.Shared.Functions.SaleConditions.Conditions
{
    internal class CheckWhitelistAddresses : ISaleConditions
    {
        public bool CheckCondition(EasynftprojectsContext db,IConnectionMultiplexer redis, Nftprojectsalecondition nftprojectsalecondition, string receiveraddress, string stakeaddress,
            int addrNftprojectId, long countnft, AssetsAssociatedWithAccount[] assets, ref CheckConditionsResultClass res)
        {
            {
                bool met6 = true;
                if (nftprojectsalecondition.Condition == nameof(SaleConditionsTypes.whitlistedaddresses) && nftprojectsalecondition.Whitlistaddresses != null)
                {
                    met6 = nftprojectsalecondition.Whitlistaddresses.Contains(receiveraddress);

                    if (!met6)
                    {
                        if (!string.IsNullOrEmpty(stakeaddress))
                        {
                            foreach (var address in nftprojectsalecondition.Whitlistaddresses.Split(
                                new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                var address1 = address.FilterToLetterOrDigit();
                                var stake2 =
                                    Bech32Engine.GetStakeFromAddress(address1);
                                if (string.IsNullOrEmpty(stake2) || stake2 != stakeaddress) continue;

                                met6 = CheckIfWhitelistAddressIsAlreadyUsed(nftprojectsalecondition, address1, stake2);
                                if (!met6)
                                {
                                    res.ConditionsMet = false;
                                    return false;
                                }

                                break;
                            }
                        }
                    }
                    else
                    {
                        // Dont use the franken address protection if the address itself is on the whitelist
                        res.SendBackAddress.OriginatorAddress = receiveraddress;
                        res.SendBackAddress.Address = receiveraddress;

                        met6 = CheckIfWhitelistAddressIsAlreadyUsed(nftprojectsalecondition, receiveraddress, res.SendBackAddress.StakeAddress);
                        if (!met6)
                        {
                            res.ConditionsMet = false;
                            return false;
                        }
                    }

                    // If there was no stake address check against blockfrost
                    if (!met6)
                    {
                        var adrlist = BlockfrostFunctions.GetAllAddressesFromSingleAddress(redis, receiveraddress);
                            foreach (var getAddressesFromStakeClass in adrlist.OrEmptyIfNull())
                            {
                                if (!nftprojectsalecondition.Whitlistaddresses.Contains(getAddressesFromStakeClass
                                        .Address)) continue;
                                met6 = CheckIfWhitelistAddressIsAlreadyUsed(nftprojectsalecondition, getAddressesFromStakeClass.Address, res.SendBackAddress.StakeAddress);
                                if (!met6)
                                {
                                    res.ConditionsMet = false;
                                    return false;
                                }
                                break;
                            }

                    }
                    if (met6 == false)
                    {
                        res.RejectParameter = nftprojectsalecondition?.Policyprojectname;
                        res.RejectReason = nftprojectsalecondition?.Condition;
                    }
                }

                return met6;
            }

        }
        private static bool CheckIfWhitelistAddressIsAlreadyUsed(Nftprojectsalecondition nftprojectsalecondition, string receiveraddress, string stakeaddress)
        {
            if (nftprojectsalecondition.Onlyonesaleperwhitlistaddress == false)
                return true;

            // old
            if (!string.IsNullOrEmpty(nftprojectsalecondition.Usedwhitelistaddresses) && nftprojectsalecondition.Usedwhitelistaddresses.Contains(receiveraddress))
                return false;
            if (!string.IsNullOrEmpty(nftprojectsalecondition.Usedwhitelistaddresses) && !string.IsNullOrEmpty(stakeaddress) && nftprojectsalecondition.Usedwhitelistaddresses.Contains(stakeaddress))
                return false;

            // new - check the new table from the db
            var f = nftprojectsalecondition.Usedaddressesonsaleconditions.FirstOrDefault(x =>
                x.Address == receiveraddress);
            if (f != null)
                return false;

            var f1 = nftprojectsalecondition.Usedaddressesonsaleconditions.FirstOrDefault(x =>
                x.Address == stakeaddress);
            if (f1 != null)
                return false;

            return true;
        }
    }
}
