using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using NMKR.Shared.Blockchains;
using NMKR.Shared.Blockchains.APTOS;
using NMKR.Shared.Blockchains.Cardano;
using NMKR.Shared.Blockchains.Solana;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions.Blockfrost;
using NMKR.Shared.Functions.Extensions;
using NMKR.Shared.Functions.Koios;
using NMKR.Shared.Functions.Solana;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NMKR.Shared.Functions
{
    public enum PricelistDiscountTypes
    {
        [Description("Buyer must have one or more NFT with a specific Policy ID/Collection")]
        walletcontainsminofpolicyid,
        [Description("Whitelisted addresses")]
        whitlistedaddresses,
        [Description("Buyer must stake on a specific Pool")]
        stakeonpool,
        [Description("Buyer must enter a coupon code")]
        couponcode
    }
    public static class PriceListDiscountClass
    {
        public static async Task<Pricelistdiscount> GetPricelistDiscount(EasynftprojectsContext db, IConnectionMultiplexer redis, int addrNftprojectId,
          string receiveraddress,string referercode,string couponcode, int serverid, Blockchain blockchain)
        {
            var discounts = await (from a in db.Pricelistdiscounts
                                   where a.NftprojectId == addrNftprojectId && a.State == "active"
                                   orderby a.Sendbackdiscount descending
                                   select a).ToArrayAsync();

            foreach (var pricelistdiscount in discounts)
            {

                IBlockchainFunctions blockchainFunctions = null;
                switch (pricelistdiscount.Blockchain.ToEnum<Blockchain>())
                {
                    case Blockchain.Cardano:
                        blockchainFunctions = new CardanoBlockchainFunctions();
                        break;
                    case Blockchain.Solana:
                        blockchainFunctions = new SolanaBlockchainFunctions();
                        break;
                    case Blockchain.Aptos:
                        blockchainFunctions = new AptosBlockchainFunctions();
                        break;
                }

                var assets = await blockchainFunctions.GetAllAssetsInWalletAsync(redis, receiveraddress);


                switch (pricelistdiscount.Condition)
                {
                    case nameof(PricelistDiscountTypes.walletcontainsminofpolicyid):
                    {
                        long countp1 = 0;
                        long countp2 = 0;
                        long countp3 = 0;
                        long countp4 = 0;
                        long countp5 = 0;
                        if (assets != null)
                        {
                            foreach (var blockfrostAssetsAssociatedWithAccount in assets.Where(blockfrostAssetsAssociatedWithAccount => blockfrostAssetsAssociatedWithAccount != null))
                            {
                                countp1 = CountPolicyIds(blockfrostAssetsAssociatedWithAccount, pricelistdiscount.Policyid, countp1);
                                countp2 = CountPolicyIds(blockfrostAssetsAssociatedWithAccount, pricelistdiscount.Policyid2, countp2);
                                countp3 = CountPolicyIds(blockfrostAssetsAssociatedWithAccount, pricelistdiscount.Policyid3, countp3);
                                countp4 = CountPolicyIds(blockfrostAssetsAssociatedWithAccount, pricelistdiscount.Policyid4, countp4);
                                countp5 = CountPolicyIds(blockfrostAssetsAssociatedWithAccount, pricelistdiscount.Policyid5, countp5);
                            }
                        }

                        var minimum = pricelistdiscount.Minvalue ?? 0;
                        if (minimum == 0)
                            minimum = 1;

                        var minimum2 = pricelistdiscount.Minvalue2;
                        var minimum3 = pricelistdiscount.Minvalue3;
                        var minimum4 = pricelistdiscount.Minvalue4;
                        var minimum5 = pricelistdiscount.Minvalue5;

                        bool met1 = pricelistdiscount.Operator switch
                        {
                            "OR" => (countp1 >= minimum || countp2 >= minimum2 || countp3 >= minimum3 ||
                                     countp4 >= minimum4 || countp5 >= minimum5),
                            "AND" => (countp1 >= minimum && (minimum2 == null || countp2 >= minimum2) &&
                                      (minimum3 == null || countp3 >= minimum3) &&
                                      (minimum4 == null || countp4 >= minimum4) &&
                                      (minimum5 == null || countp5 >= minimum5)),
                            _ => false
                        };

                        if (met1)
                        {
                            await GlobalFunctions.LogMessageAsync(db,
                                $"Condition for Discount met 1 - {pricelistdiscount.Condition} - {pricelistdiscount.Policyid} {receiveraddress}",
                                JsonConvert.SerializeObject(assets), serverid);
                                return pricelistdiscount;
                        }
                        else
                        {
                            await GlobalFunctions.LogMessageAsync(db,
                                $"Condition for Discount NOT met 1 - {pricelistdiscount.Condition} - {pricelistdiscount.Policyid} {receiveraddress}",
                                JsonConvert.SerializeObject(assets), serverid);
                        }

                        break;
                    }
                    case nameof(PricelistDiscountTypes.whitlistedaddresses) when pricelistdiscount.Whitlistaddresses != null:
                    {
                        bool met2 = pricelistdiscount.Whitlistaddresses.Contains(receiveraddress);

                        if (!met2)
                        {
                            var stake = Bech32Engine.GetStakeFromAddress(receiveraddress);
                            if (!string.IsNullOrEmpty(stake))
                            {
                                foreach (var address in pricelistdiscount.Whitlistaddresses.Split(
                                             new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    var address1 = address.FilterToLetterOrDigit();
                                    var stake2 = Bech32Engine.GetStakeFromAddress(address1);
                                    if (string.IsNullOrEmpty(stake2) || stake2 != stake) continue;
                                    met2 = true;
                                    break;
                                }
                            }
                        }


                        if (!met2)
                        {
                            var adrlist = await BlockfrostFunctions.GetAllAddressesFromSingleAddressAsync(redis, receiveraddress);
                            if (adrlist != null)
                            {
                                if (adrlist.Any(getAddressesFromStakeClass => pricelistdiscount.Whitlistaddresses.Contains(getAddressesFromStakeClass
                                        .Address)))
                                {
                                    met2 = true;
                                }
                            }

                        }
                        if (met2)
                        {
                            await GlobalFunctions.LogMessageAsync(db,
                                $"Condition for Discount met 2 - {pricelistdiscount.Condition} - {pricelistdiscount.Policyid} {receiveraddress}",
                                JsonConvert.SerializeObject(assets), serverid);
                            return pricelistdiscount;
                        }
                        else
                        {
                            await GlobalFunctions.LogMessageAsync(db,
                                $"Condition for Discount NOT met 2 - {pricelistdiscount.Condition} - {pricelistdiscount.Policyid} {receiveraddress}",
                                JsonConvert.SerializeObject(assets), serverid);
                        }

                        break;
                    }
                    case nameof(PricelistDiscountTypes.stakeonpool):
                    {
                        bool met3 = false;
                        var stakepool =blockchain==Blockchain.Solana?
                            await SolanaFunctions.GetStakePoolInformationAsync(redis, receiveraddress):
                            await KoiosFunctions.GetStakePoolInformationAsync(redis, receiveraddress);

                        if (stakepool != null)
                        {
                            if (!string.IsNullOrEmpty(stakepool.DelegatedPool))
                            {
                                if (stakepool.DelegatedPool == pricelistdiscount.Policyid ||
                                    stakepool.DelegatedPool == pricelistdiscount.Policyid2 ||
                                    stakepool.DelegatedPool == pricelistdiscount.Policyid3 ||
                                    stakepool.DelegatedPool == pricelistdiscount.Policyid4 ||
                                    stakepool.DelegatedPool == pricelistdiscount.Policyid5)
                                {
                                    met3 = true;
                                }
                            }
                        }
                        if (met3)
                        {
                            await GlobalFunctions.LogMessageAsync(db,
                                $"Condition for Discount met 3 - {pricelistdiscount.Condition} - {pricelistdiscount.Policyid} {receiveraddress}",
                                JsonConvert.SerializeObject(assets), serverid);
                            return pricelistdiscount;
                        }
                        else
                        {
                            await GlobalFunctions.LogMessageAsync(db,
                                $"Condition for Discount NOT met 3 - {pricelistdiscount.Condition} - {pricelistdiscount.Policyid} {receiveraddress}",
                                JsonConvert.SerializeObject(assets), serverid);
                        }

                        break;
                    }
                    case nameof(PricelistDiscountTypes.couponcode):
                    {
                        bool met4 = (!string.IsNullOrEmpty(couponcode)) && (!string.IsNullOrEmpty(pricelistdiscount.Couponcode)) && couponcode.ToLower() == pricelistdiscount.Couponcode.ToLower();

                        if (met4)
                        {
                            await GlobalFunctions.LogMessageAsync(db,
                                $"Condition for Discount met 4 - {pricelistdiscount.Condition} - {pricelistdiscount.Policyid} {receiveraddress} - {couponcode}",
                                JsonConvert.SerializeObject(assets), serverid);
                            return pricelistdiscount;
                        }
                        else
                        {
                            await GlobalFunctions.LogMessageAsync(db,
                                $"Condition for Discount NOT met 4 - {pricelistdiscount.Condition} - {pricelistdiscount.Policyid} {receiveraddress}",
                                JsonConvert.SerializeObject(assets), serverid);
                        }

                        break;
                    }
                }
            }
            return null;
        }

        private static long CountPolicyIds(AssetsAssociatedWithAccount blockfrostAssetsAssociatedWithAccount,
            string policyid, long countp1)
        {
            if (blockfrostAssetsAssociatedWithAccount.Unit != null && string.IsNullOrEmpty(policyid) == false &&
                blockfrostAssetsAssociatedWithAccount.Unit.Contains(policyid))
            {
                if (blockfrostAssetsAssociatedWithAccount.Quantity != null)
                    countp1 += (long)blockfrostAssetsAssociatedWithAccount.Quantity;

            }

            return countp1;
        }
    }
}
