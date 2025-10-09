using System;
using System.Linq;
using System.Threading.Tasks;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.SharedClasses
{
    internal static class SaveTransactionClass
    {

        public static async Task SaveTransactionToDatabase(EasynftprojectsContext db,  IConnectionMultiplexer redis,
            Preparedpaymenttransaction preparedpaymenttransaction, PromotionClass promotion, string cbor)
        {

            var tx = await (from a in db.Transactions
                where a.Transactionid == preparedpaymenttransaction.Txhash
                select a).AsNoTracking().FirstOrDefaultAsync();

            // When already saved
            if (tx != null)
                return;
      
            Referer referer = null;
            // Check for referer
            if (!string.IsNullOrEmpty(preparedpaymenttransaction.Referer))
            {
                referer = await (from a in db.Referers
                    where a.Referertoken == preparedpaymenttransaction.Referer && a.State == "active"
                    select a).AsNoTracking().FirstOrDefaultAsync();
            }
            try
            {
                long countnft = preparedpaymenttransaction.PreparedpaymenttransactionsNfts.Count;
                if (countnft == 0 && preparedpaymenttransaction.Nftproject.Maxsupply==1)
                {
                    countnft = preparedpaymenttransaction.Countnft ?? 1;
                }

                if (preparedpaymenttransaction.Lovelace==null || preparedpaymenttransaction.Lovelace == 0)
                {
                    preparedpaymenttransaction.Stakerewards = 0;
                    preparedpaymenttransaction.Discount = 0;
                    preparedpaymenttransaction.Tokenrewards = 0;
                }
              
                var mintingcosts = GlobalFunctions
                    .GetMintingcosts2(preparedpaymenttransaction.NftprojectId, countnft, preparedpaymenttransaction.Lovelace ?? 0)
                    .Costs - (preparedpaymenttransaction.Stakerewards ?? 0) - (preparedpaymenttransaction.Tokenrewards ?? 0);

                var rates = await GlobalFunctions.GetNewRatesAsync(redis, Coin.ADA);

                Transaction t = new()
                    {
                    Senderaddress = preparedpaymenttransaction.Changeaddress,
                    Receiveraddress = preparedpaymenttransaction.Changeaddress,
                    Ada = 0,
                    Stakereward = preparedpaymenttransaction.Stakerewards??0,
                    Tokenreward = preparedpaymenttransaction.Tokenrewards??0,
                    Discount = preparedpaymenttransaction.Discount ??0,
                    Created = DateTime.Now,
                    CustomerId = preparedpaymenttransaction.Nftproject.CustomerId,
                    NftaddressId = null,
                    NftprojectId = preparedpaymenttransaction.NftprojectId,
                    Transactiontype = GetTransactionType(preparedpaymenttransaction),
                    Transactionid = preparedpaymenttransaction.Txhash,
                    Fee = preparedpaymenttransaction.Fee,
                    Projectaddress = preparedpaymenttransaction.Selleraddress,
                    Projectada = (GetTransactionType(preparedpaymenttransaction)== "decentralmintandsend") ? 0 : 
                        preparedpaymenttransaction.Lovelace -
                        mintingcosts -
                                 (preparedpaymenttransaction.Discount ?? 0),
                    Mintingcostsaddress = preparedpaymenttransaction.Nftproject.Settings.Mintingaddress,
                    Mintingcostsada = mintingcosts,
                    WalletId = preparedpaymenttransaction.Nftproject.CustomerwalletId,
                    State = "submitted",
                    Eurorate = (float)rates.EurRate,
                    Serverid = null,
                    RefererId = referer?.Id,
                    Originatoraddress = preparedpaymenttransaction.Changeaddress,
                    Cbor = cbor,
                    PreparedpaymenttransactionId = preparedpaymenttransaction.Id,
                    Nftcount = preparedpaymenttransaction.PreparedpaymenttransactionsNfts.Count,
                    Stakeaddress = Bech32Engine.GetStakeFromAddress(preparedpaymenttransaction.Changeaddress),
                    Coin= Coin.ADA.ToString(),
                    Paymentmethod= Coin.ADA.ToString(), // TODO: Add payment method FIAT etc
                    Metadatastandard= preparedpaymenttransaction.Nftproject.Cip68?"cip68":"cip25",
                };
                await db.AddAsync(t);
                await db.SaveChangesAsync();

                // Save additional wallets
                await SaveAdditionalWallets(db, t, preparedpaymenttransaction);

                foreach (var nft in preparedpaymenttransaction.PreparedpaymenttransactionsNfts)
                {
                    await db.TransactionNfts.AddAsync(new()
                    {
                        NftId = nft.NftId,
                        TransactionId = t.Id,
                        Mintedontransaction = true,
                        Tokencount = nft.Count,
                        Multiplier = await GlobalFunctions.GetNftMultiplierAsync(db, nft.NftId??0),
                        Ispromotion = false
                    });
                }

                await db.SaveChangesAsync();
               

                if (promotion != null)
                {
                    await db.TransactionNfts.AddAsync(new()
                    {
                        NftId = promotion.PromotionNft.Id,
                        TransactionId = t.Id,
                        Mintedontransaction = true,
                        Tokencount = promotion.Tokencount,
                        Multiplier = promotion.PromotionNft.Multiplier,
                        Ispromotion = true
                    });
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                await GlobalFunctions.LogExceptionAsync(db, $"API Exception: {e.Message}", (e.InnerException != null ? e.InnerException.Message:""), 0);
            }
        }

        private static async Task SaveAdditionalWallets(EasynftprojectsContext db, Transaction transaction, Preparedpaymenttransaction preparedpaymenttransaction)
        {
           if (preparedpaymenttransaction.Transactiontype!=nameof(PaymentTransactionTypes.decentral_mintandsale_random) &&
               preparedpaymenttransaction.Transactiontype!=nameof(PaymentTransactionTypes.decentral_mintandsale_specific))
               return;

           var cp = preparedpaymenttransaction.PreparedpaymenttransactionsCustomproperties
               .FirstOrDefault(x => x.Key == "cp");
           string customproperty = cp?.Value ?? string.Empty;

           var additionalpayoutswallets = await (from a in db.Nftprojectsadditionalpayouts
                   .Include(a => a.Wallet)
               where a.NftprojectId == preparedpaymenttransaction.NftprojectId && a.Coin == Coin.ADA.ToString() 
                                                                               && (a.Custompropertycondition == null || a.Custompropertycondition == "" || a.Custompropertycondition==customproperty)
                                                 select a).AsNoTracking().ToListAsync();


           foreach (var nftprojectsadditionalpayout in additionalpayoutswallets.OrEmptyIfNull())
           {
               long hastopay = preparedpaymenttransaction.Lovelace > 0
                   ? Math.Max(1000000, (long) (preparedpaymenttransaction.Lovelace ?? 0) -
                                       (preparedpaymenttransaction.Discount ?? 0) -
                                       (preparedpaymenttransaction.Stakerewards ?? 0) -
                                       (preparedpaymenttransaction.Tokenrewards ?? 0))
                   : 0;
               long nftcount= preparedpaymenttransaction.PreparedpaymenttransactionsNfts.Count;
               long addvalue = ConsoleCommand.GetAdditionalPayoutwalletsValue(nftprojectsadditionalpayout, hastopay, nftcount);
               if (addvalue <= 0) continue;
               await db.TransactionsAdditionalpayouts.AddAsync(new TransactionsAdditionalpayout()
               {
                   TransactionId = transaction.Id, Lovelace = addvalue,
                   Payoutaddress = nftprojectsadditionalpayout.Wallet.Walletaddress,
               });
               await db.SaveChangesAsync();
           }
        }

        private static string GetTransactionType(Preparedpaymenttransaction preparedpaymenttransaction)
        {
            switch (preparedpaymenttransaction.Transactiontype)
            {
                case nameof(PaymentTransactionTypes.decentral_mintandsale_random):
                    return nameof(TransactionTypes.decentralmintandsale);
                case nameof(PaymentTransactionTypes.decentral_mintandsale_specific):
                    return nameof(TransactionTypes.decentralmintandsale);
                case nameof(PaymentTransactionTypes.decentral_mintandsend_random):
                    return nameof(TransactionTypes.decentralmintandsend);
                case nameof(PaymentTransactionTypes.decentral_mintandsend_specific):
                    return nameof(TransactionTypes.decentralmintandsend);
                case nameof(PaymentTransactionTypes.legacy_auction):
                    return nameof(TransactionTypes.auction);
                case nameof(PaymentTransactionTypes.legacy_directsale):
                    return nameof(TransactionTypes.directsale);
                case nameof(PaymentTransactionTypes.paymentgateway_mintandsend_random):
                    return nameof(TransactionTypes.mintfromcustomeraddress);
                case nameof(PaymentTransactionTypes.paymentgateway_mintandsend_specific):
                    return nameof(TransactionTypes.mintfromcustomeraddress);
                case nameof(PaymentTransactionTypes.paymentgateway_nft_random):
                    return nameof(TransactionTypes.paidonftaddress);
                case nameof(PaymentTransactionTypes.paymentgateway_nft_specific):
                    return nameof(TransactionTypes.paidonftaddress);
                case nameof(PaymentTransactionTypes.smartcontract_auction):
                    return nameof(TransactionTypes.auction);
                case nameof(PaymentTransactionTypes.smartcontract_directsale):
                    return nameof(TransactionTypes.directsale);

            }
            return nameof(TransactionTypes.unknown);
        }
    }
}
