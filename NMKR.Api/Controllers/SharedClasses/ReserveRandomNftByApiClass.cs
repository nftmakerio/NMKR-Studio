using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CardanoSharp.Wallet.Enums;
using NMKR.Shared.Blockchains;
using NMKR.Shared.Blockchains.APTOS;
using NMKR.Shared.Blockchains.BITCOIN;
using NMKR.Shared.Blockchains.Cardano;
using NMKR.Shared.Blockchains.Solana;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.PricelistFunctions;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.SharedClasses
{
    internal static class ReserveRandomNftByApiClass
    {
        internal static async Task<ReserveAddressQueueResultClass> RequestRandomAddress(IConnectionMultiplexer redis,
        ReserveAddressQueueClass adrreq, int? prepardpaymenttransactionid=null)
        {
            ReserveAddressQueueResultClass result = new()
            { ApiError = new() };

            // Check if the Apikey is valid
            ApiErrorResultClass resultapikey = adrreq.NftprojectId != null
                ? CheckCachedAccess.CheckApikeyOrToken(redis, MethodBase.GetCurrentMethod()?.DeclaringType?.FullName,
                    adrreq.NftprojectId, adrreq.ApiKey,
                    adrreq.RemoteIpAddress ?? string.Empty)
                : CheckCachedAccess.CheckApikeyOrToken(redis, MethodBase.GetCurrentMethod()?.DeclaringType?.FullName,
                     adrreq.NftprojectUId, adrreq.ApiKey,
                    adrreq.RemoteIpAddress ?? string.Empty);

            if (resultapikey.ResultState != ResultStates.Ok)
            {
                result.StatusCode = 401;
                result.ApiError = resultapikey;
                return result;
            }

            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            var project = await (from a in db.Nftprojects
                    .Include(a => a.Settings)
                    .AsSplitQuery()
                    .Include(a => a.Nftprojectsadditionalpayouts)
                    .AsSplitQuery()
                    .Include(a => a.Pricelists)
                    .AsSplitQuery()
                                 where (adrreq.NftprojectId!=null && a.Id == adrreq.NftprojectId ) || (adrreq.NftprojectUId==a.Uid)
                                 select a).AsNoTracking().FirstOrDefaultAsync();
            if (project == null)
            {
                result.ApiError.ErrorCode = 570;
                result.ApiError.ErrorMessage = "Internal error (2). Please contact support";
                await GlobalFunctions.LogMessageAsync(db, "Api: "+result.ApiError.ErrorMessage,
                    JsonConvert.SerializeObject(adrreq));
                result.ApiError.ResultState = ResultStates.Error;
                result.StatusCode = 500;
                await db.Database.CloseConnectionAsync();
                return result;
            }
            if (project.Disablerandomsales)
            {
                result.ApiError.ErrorCode = 4501;
                result.ApiError.ErrorMessage = "Random sales are not enabled on this project";
                await GlobalFunctions.LogMessageAsync(db, "Api: " + result.ApiError.ErrorMessage,
                    JsonConvert.SerializeObject(adrreq));
                result.ApiError.ResultState = ResultStates.Error;
                result.StatusCode = 406;
                await db.Database.CloseConnectionAsync();
                return result;
            }

            if (project.Paymentgatewaysalestart != null && project.Paymentgatewaysalestart > DateTime.Now)
            {
                result.ApiError.ErrorCode = 199;
                result.ApiError.ErrorMessage = "Start time for paymentgateway (API Addresses) is " + project.Paymentgatewaysalestart;
                await GlobalFunctions.LogMessageAsync(db, "Api: " + result.ApiError.ErrorMessage,
                    JsonConvert.SerializeObject(adrreq));
                result.ApiError.ResultState = ResultStates.Error;
                result.StatusCode = 406;
                await db.Database.CloseConnectionAsync();
                return result;
            }
            if (adrreq.Addresstype != AddressType.Enterprise && adrreq.Addresstype != AddressType.Base && adrreq.Coin==Coin.ADA)
            {
                result.ApiError.ErrorCode = 206;
                result.ApiError.ErrorMessage = "Address type is not valid";
                result.ApiError.ResultState = ResultStates.Error;
                result.StatusCode = 406;
                return result;
            }
            if (!GlobalFunctions.CheckExpirationSlot(project) && adrreq.Coin==Coin.ADA)
            {
                result.ApiError.ErrorCode = 205;
                result.ApiError.ErrorMessage = "Policy is already locked. No further minting possible (1)";
              /*  await GlobalFunctions.LogMessageAsync(db, "Api: " + result.ApiError.ErrorMessage,
                    JsonConvert.SerializeObject(adrreq));*/
                result.ApiError.ResultState = ResultStates.Error;
                result.StatusCode = 404;
                await db.Database.CloseConnectionAsync();
                return result;
            }
            if (!string.IsNullOrEmpty(adrreq.OptionalReceiverAddress))
            {
                if (!ConsoleCommand.CheckIfAddressIsValid(db, adrreq.OptionalReceiverAddress, GlobalFunctions.IsMainnet(), out string outaddress, out Blockchain blockchain,true))
                {
                    result.ApiError.ErrorCode = 210;
                    result.ApiError.ErrorMessage = "Receiver address is not valid";
                    await GlobalFunctions.LogMessageAsync(db, "Api: " + result.ApiError.ErrorMessage,
                        JsonConvert.SerializeObject(adrreq));
                    result.ApiError.ResultState = ResultStates.Error;
                    result.StatusCode = 406;
                    await db.Database.CloseConnectionAsync();
                    return result;
                }
                adrreq.OptionalReceiverAddress = outaddress;
            }

            if (adrreq.Coin == Coin.SOL && !project.Enabledcoins.Contains(Coin.SOL.ToString()))
            {
                result.ApiError.ErrorCode = 83;
                result.ApiError.ErrorMessage = $"Solana is not enabled in this project";
                result.ApiError.ResultState = ResultStates.Error;
                result.StatusCode = 406;
                return result;
            }
            if (adrreq.Coin == Coin.APT && !project.Enabledcoins.Contains(Coin.APT.ToString()))
            {
                result.ApiError.ErrorCode = 83;
                result.ApiError.ErrorMessage = $"Aptos is not enabled in this project";
                result.ApiError.ResultState = ResultStates.Error;
                result.StatusCode = 406;
                return result;
            }
            if (adrreq.Coin == Coin.ADA && !project.Enabledcoins.Contains(Coin.ADA.ToString()))
            {
                result.ApiError.ErrorCode = 83;
                result.ApiError.ErrorMessage = $"Cardano is not enabled in this project";
                result.ApiError.ResultState = ResultStates.Error;
                result.StatusCode = 406;
                return result;
            }
            if (adrreq.Coin == Coin.BTC && !project.Enabledcoins.Contains(Coin.BTC.ToString()))
            {
                result.ApiError.ErrorCode = 83;
                result.ApiError.ErrorMessage = $"Bitcoin is not enabled in this project";
                result.ApiError.ResultState = ResultStates.Error;
                result.StatusCode = 406;
                return result;
            }
            if (adrreq.Coin != Coin.ADA && adrreq.Coin != Coin.APT && adrreq.Coin != Coin.SOL && adrreq.Coin != Coin.BTC)
            {
                result.ApiError.ErrorCode = 84;
                result.ApiError.ErrorMessage = $"Blockchain not supported";
                result.ApiError.ResultState = ResultStates.Error;
                result.StatusCode = 406;
                return result;
            }

            if (adrreq.CountNft == 0)
            {
                result.ApiError.ErrorCode = 10;
                result.ApiError.ErrorMessage = "Specify the amount of NFT. Null is not valid";
                await GlobalFunctions.LogMessageAsync(db, "Api: " + result.ApiError.ErrorMessage,
                    JsonConvert.SerializeObject(adrreq));
                result.ApiError.ResultState = ResultStates.Error;
                result.StatusCode = 406;
                await db.Database.CloseConnectionAsync();
                return result;
            }

            if (adrreq.CountNft > 30 && project.Maxsupply == 1)
            {
                result.ApiError.ErrorCode = 10;
                result.ApiError.ErrorMessage =
                    "Amount of NFT in one Random Distribution is too large. Max 30 NFT. If you want to sell a large amount of single tokens, use GetAddressForSpecificNftSale";
                await GlobalFunctions.LogMessageAsync(db, "Api: " + result.ApiError.ErrorMessage,
                    JsonConvert.SerializeObject(adrreq));
                result.ApiError.ResultState = ResultStates.Error;
                result.StatusCode = 406;
                await db.Database.CloseConnectionAsync();
                return result;
            }

            if (project.Maxsupply == 1)
            {
                if (project.Free1 < adrreq.CountNft)
                {
                    result.ApiError.ErrorCode = 101;
                    result.ApiError.ErrorMessage = "No more NFT available";
                    await GlobalFunctions.LogMessageAsync(db, "Api: " + result.ApiError.ErrorMessage + $" - {result.ApiError.ErrorCode} - Project: {project.Id} - {adrreq.CountNft}",
                        JsonConvert.SerializeObject(adrreq));
                    result.ApiError.ResultState = ResultStates.Error;
                    result.StatusCode = 404;
                    await db.Database.CloseConnectionAsync();
                    return result;
                }
            }
            else
            {
                if (project.Totaltokens1 - (project.Tokensreserved1 + project.Tokenssold1) < adrreq.CountNft)
                {
                    result.ApiError.ErrorCode = 102;
                    result.ApiError.ErrorMessage = "No more NFT available";
                    await GlobalFunctions.LogMessageAsync(db, "Api: " + result.ApiError.ErrorMessage + $" - {result.ApiError.ErrorCode} - Project: {project.Id} - {adrreq.CountNft}",
                        JsonConvert.SerializeObject(adrreq));
                    result.ApiError.ResultState = ResultStates.Error;
                    result.StatusCode = 404;
                    await db.Database.CloseConnectionAsync();
                    return result;
                }
            }

            // If Lovelace is null, catch the price from the database pricelice

            if (adrreq.Price == null)
            {
                var pricelist = await GetPricelistClass.GetPriceForCountOfNfts(db, project, redis, adrreq.CountNft, adrreq.Coin);

                if (pricelist == null)
                {
                    result.ApiError.ErrorCode = 59;
                    result.ApiError.ResultState = ResultStates.Error;
                    result.ApiError.ErrorMessage =
                        $"No price for {adrreq.CountNft} Nft/Tokens found in pricelist for this project";
                    await GlobalFunctions.LogMessageAsync(db, "Api: " + result.ApiError.ErrorMessage,
                        JsonConvert.SerializeObject(adrreq));
                    result.StatusCode = 406;
                    await db.Database.CloseConnectionAsync();
                    return result;
                }

                adrreq.CardanoLovelace = pricelist.PriceInLovelace;
                adrreq.SolanaLamport = pricelist.PriceInLamport;
                adrreq.AptosOcta = pricelist.PriceInOctas;
                adrreq.BitcoinSatoshi = pricelist.PriceInSatoshis;
                adrreq.Freemint = pricelist.FreeMint;
                if (pricelist.AdditionalPriceInTokens != null && pricelist.AdditionalPriceInTokens.Any())
                {
                    adrreq.PriceInToken=pricelist.AdditionalPriceInTokens.First().TotalCount/ GlobalFunctions.GetMultiplierFromDecimals(pricelist.AdditionalPriceInTokens.First().Decimals);
                    adrreq.TokenPolicyId = pricelist.AdditionalPriceInTokens.First().PolicyId;
                    adrreq.TokenAssetId = pricelist.AdditionalPriceInTokens.First().AssetName;
                    adrreq.TokenAssetIdHex = pricelist.AdditionalPriceInTokens.First().AssetNameInHex;
                    adrreq.TotalTokens = pricelist.AdditionalPriceInTokens.First().TotalCount;
                    adrreq.Multiplier =
                        GlobalFunctions.GetMultiplierFromDecimals(pricelist.AdditionalPriceInTokens.First().Decimals);
                    adrreq.Decimals = pricelist.AdditionalPriceInTokens.First().Decimals;
                }
            }
            else
            {
                switch (adrreq.Coin)
                {
                    case Coin.ADA:
                        adrreq.CardanoLovelace = adrreq.Price;
                        break;
                    case Coin.SOL:
                        adrreq.SolanaLamport = adrreq.Price;
                        break;
                    case Coin.APT:
                        adrreq.AptosOcta = adrreq.Price;
                        break;
                    case Coin.BTC:
                        adrreq.BitcoinSatoshi = adrreq.Price;
                        break;
                }
            }


            var selectedreservations =
                await NftReservationClass.ReserveRandomNft(db, redis, adrreq.Uid, project.Id, adrreq.CountNft,
                    adrreq.ReservationTimeInMinutes is null or 0 ? project.Expiretime : (int) adrreq.ReservationTimeInMinutes, 
                    false, false, adrreq.Coin);

            if (selectedreservations.Sum(x => x.Tc) < adrreq.CountNft)
            {
                result.ApiError.ErrorCode = 103;
                result.ApiError.ErrorMessage = "No more NFT available";
                await GlobalFunctions.LogMessageAsync(db, "Api: " + result.ApiError.ErrorMessage + $" - {result.ApiError.ErrorCode} - Project: {project.Id} - {adrreq.CountNft}",
                    JsonConvert.SerializeObject(adrreq));
                result.ApiError.ResultState = ResultStates.Error;
                result.StatusCode = 404;
                await NftReservationClass.ReleaseAllNftsAsync(db, redis, adrreq.Uid);
                return result;
            }


            // Receiver min 2 Lovelace
            // Sender to send back min 2 Lovelace
            // Mintingcosts min 2 Lovelace
            // Fees min 1 Lovelace

            long sendback = GlobalFunctions.CalculateSendbackToUser(db,redis, adrreq.CountNft, project.Id);
            // Add Sendback when decentral payment is enabled
            if (project.Enabledecentralpayments && adrreq.Price > 0)
            {
                adrreq.Price += sendback;
            }


            if (adrreq.Coin == Coin.ADA)
            {
                /*   long mincosts = GlobalFunctions.CalculateMinutxoNew(project, selectedreservations.Count,
                       (long) (adrreq.CardanoLovelace??0), sendback);
                   if (adrreq.CardanoLovelace < mincosts &&  adrreq.CardanoLovelace > 0 && adrreq.CardanoLovelace != 2000000)
                   {
                       result.ApiError.ErrorCode = 52;
                       result.ApiError.ErrorMessage =
                           $"Lovelace Amount is too small - min. {((double) mincosts / (double) 1000000)} ADA";
                       await GlobalFunctions.LogMessageAsync(db, "Api: " + result.ApiError.ErrorMessage,
                           JsonConvert.SerializeObject(adrreq));
                       result.ApiError.ResultState = ResultStates.Error;
                       result.StatusCode = 406;
                       await NftReservationClass.ReleaseAllNftsAsync(db, redis, adrreq.Uid);
                       await db.Database.CloseConnectionAsync();
                       return result;
                   }*/
                if (adrreq.CardanoLovelace < 3000000 && adrreq.CardanoLovelace > 0 && adrreq.CardanoLovelace != 2000000)
                {
                    result.ApiError.ErrorCode = 52;
                    result.ApiError.ErrorMessage =
                        $"Lovelace Amount is too small - min. 3 ADA";
                    await GlobalFunctions.LogMessageAsync(db, "Api: " + result.ApiError.ErrorMessage,
                        JsonConvert.SerializeObject(adrreq));
                    result.ApiError.ResultState = ResultStates.Error;
                    result.StatusCode = 406;
                    await NftReservationClass.ReleaseAllNftsAsync(db, redis, adrreq.Uid);
                    await db.Database.CloseConnectionAsync();
                    return result;
                }


            }


            Referer? referer = null;
            // Check for referer
            if (!string.IsNullOrEmpty(adrreq.Referer))
            {
                referer = await (from a in db.Referers
                    where a.Referertoken == adrreq.Referer && a.State=="active"
                    select a).FirstOrDefaultAsync();
            }

            IBlockchainFunctions reserve = null;
            switch (adrreq.Coin)
            {
                case Coin.APT:
                    reserve = new AptosBlockchainFunctions();
                    break;
                case Coin.ADA:
                    reserve = new CardanoBlockchainFunctions();
                    break;
                case Coin.SOL:
                    reserve = new SolanaBlockchainFunctions();
                    break;
                case Coin.BTC:
                    reserve = new BitcoinBlockchainFunctions();
                    break;
            }

            if (reserve == null)
            {
                result.ApiError.ErrorCode = 465;
                result.ApiError.ErrorMessage = "Can not create an Address - internal error";
                await GlobalFunctions.LogMessageAsync(db, "Api: " + result.ApiError.ErrorMessage,
                    JsonConvert.SerializeObject(adrreq));
                result.ApiError.ResultState = ResultStates.Error;
                result.StatusCode = 500;
                await NftReservationClass.ReleaseAllNftsAsync(db, redis, adrreq.Uid);
                await db.Database.CloseConnectionAsync();
                return result;
            }

            var address = await reserve.CreateAddress(redis, adrreq, prepardpaymenttransactionid,
                db, project,"random", selectedreservations, sendback, referer);
            if (address == null)
            {
                result.ApiError.ErrorCode = 466;
                result.ApiError.ErrorMessage = "Can not create an Address - internal error";
                await GlobalFunctions.LogMessageAsync(db, "Api: " + result.ApiError.ErrorMessage,
                    JsonConvert.SerializeObject(adrreq));
                result.ApiError.ResultState = ResultStates.Error;
                result.StatusCode = 500;
                await NftReservationClass.ReleaseAllNftsAsync(db, redis, adrreq.Uid);
                await db.Database.CloseConnectionAsync();
                return result;
            }

            foreach (var sn in selectedreservations)
            {
                Nfttonftaddress nton = new() { NftaddressesId = address.Id, NftId = sn.NftId, Tokencount = sn.Tc };
                await db.Nfttonftaddresses.AddAsync(nton);
            }
            await db.SaveChangesAsync();

            var pnrc = GlobalFunctions.GetPaymentAddressResult(db,redis, address, project);
            result.SuccessResult = pnrc;
            result.StatusCode = 0;

            await db.Database.CloseConnectionAsync();
            return result;
        }
    }

}
