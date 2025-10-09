using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CardanoSharp.Wallet.Enums;
using NMKR.Shared.Blockchains.APTOS;
using NMKR.Shared.Blockchains.BITCOIN;
using NMKR.Shared.Blockchains.Cardano;
using NMKR.Shared.Blockchains.Solana;
using NMKR.Shared.Blockchains;
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
    public static class ReserveSpecificNftByApiClass
    {
        public static async Task<ReserveAddressQueueResultClass> RequestSpecificAddress(IConnectionMultiplexer redis,
            ReserveAddressQueueClass adrreq, int? prepardpaymenttransactionid = null)
        {
            ReserveAddressQueueResultClass result = new()
                {ApiError = new()};

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
            if (adrreq.NftprojectId == null && string.IsNullOrEmpty(adrreq.NftprojectUId) &&
                adrreq.Reservenfts != null && adrreq.Reservenfts.ReserveNfts != null &&
                adrreq.Reservenfts.ReserveNfts.Any())
            {
                var nfts = await (from a in db.Nfts
                    where a.Uid == adrreq.Reservenfts.ReserveNfts.First().NftUid
                    select a).FirstOrDefaultAsync();

                if (nfts != null)
                    adrreq.NftprojectId = nfts.NftprojectId;
            }



            var project = await (from a in db.Nftprojects
                    .Include(a => a.Customerwallet)
                    .AsSplitQuery()
                    .Include(a => a.Settings)
                    .AsSplitQuery()
                    .Include(a => a.Nftprojectsadditionalpayouts)
                    .AsSplitQuery()
                    .Include(a => a.Pricelists)
                    .AsSplitQuery()
                where (adrreq.NftprojectId != null && a.Id == adrreq.NftprojectId) || (adrreq.NftprojectUId == a.Uid)
                select a).AsNoTracking().FirstOrDefaultAsync();



            if (project == null)
            {
                result.ApiError.ErrorCode = 570;
                result.ApiError.ErrorMessage = "Internal error (2). Please contact support";
                result.ApiError.ResultState = ResultStates.Error;
                result.StatusCode = 500;
                return result;
            }

            if (project.Disablespecificsales)
            {
                result.ApiError.ErrorCode = 4502;
                result.ApiError.ErrorMessage = "Specific sales are not enabled on this project";
                result.ApiError.ResultState = ResultStates.Error;
                result.StatusCode = 406;
                return result;
            }

            if (project.Paymentgatewaysalestart != null && project.Paymentgatewaysalestart > DateTime.Now)
            {
                result.ApiError.ErrorCode = 199;
                result.ApiError.ErrorMessage = "Start time for paymentgateway (API Addresses) is " +
                                               project.Paymentgatewaysalestart;
                result.ApiError.ResultState = ResultStates.Error;
                result.StatusCode = 406;
                return result;
            }

            if (!GlobalFunctions.CheckExpirationSlot(project) && adrreq.Coin==Coin.ADA)
            {
                result.ApiError.ErrorCode = 205;
                result.ApiError.ErrorMessage = "Policy is already locked. No further minting possible (2)";
                result.ApiError.ResultState = ResultStates.Error;
                result.StatusCode = 404;
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

            if (!string.IsNullOrEmpty(adrreq.OptionalReceiverAddress))
            {
                if (!ConsoleCommand.CheckIfAddressIsValid(db, adrreq.OptionalReceiverAddress, GlobalFunctions.IsMainnet(),
                        out string outaddress, out Blockchain blockchain, true))
                {
                    result.ApiError.ErrorCode = 210;
                    result.ApiError.ErrorMessage = "Receiver address is not valid";
                    result.ApiError.ResultState = ResultStates.Error;
                    result.StatusCode = 406;
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

            long lovelace = 0;
            long lamports = 0;
            long octas = 0;
            foreach (var rn in adrreq.Reservenfts.ReserveNfts)
            {
                var nx = await (from a in db.Nfts
                        .Include(a => a.Nftproject)
                        .AsSplitQuery()
                    where ((rn.NftId != 0 && a.Id == rn.NftId) || (rn.NftUid != null && a.Uid == rn.NftUid)) &&
                          a.NftprojectId == project.Id
                    select a).AsNoTracking().FirstOrDefaultAsync();

                if (nx == null)
                {
                    result.ApiError.ErrorCode = 10;
                    result.ApiError.ErrorMessage = $"NFT not available (NFT ID wrong) {rn.NftId}";
                    result.ApiError.ResultState = ResultStates.Error;
                    result.StatusCode = 404;
                    return result;
                }

                if (nx.Isroyaltytoken)
                {
                    result.ApiError.ErrorCode = 10;
                    result.ApiError.ErrorMessage = $"NFT not available (NFT is royalty token) {rn.NftId}";
                    result.ApiError.ResultState = ResultStates.Error;
                    result.StatusCode = 406;
                    return result;
                }

                if (rn.Tokencount < 1)
                {
                    result.ApiError.ErrorCode = 10;
                    result.ApiError.ErrorMessage = $"Tokencount must be one or more {rn.NftId}";
                    result.ApiError.ResultState = ResultStates.Error;
                    result.StatusCode = 406;
                    return result;
                }

                if (nx.Errorcount + nx.Soldcount + nx.Reservedcount + rn.Tokencount * Math.Max(1, nx.Multiplier) >
                    nx.Nftproject.Maxsupply)
                {
                    result.ApiError.ErrorCode = 10;
                    result.ApiError.ErrorMessage = $"NFT not available (no more tokens available) {rn.NftId}";
                    result.ApiError.ResultState = ResultStates.Error;
                    result.StatusCode = 404;
                    return result;
                }

                var selectedreservations3 = await NftReservationClass.ReserveSpecificNft(db, redis, adrreq.Uid,
                    project.Id,
                    new ReserveNftsClass[]
                    {
                        new() {NftId = nx.Id, Tokencount = rn.Tokencount ?? 1, Multiplier = Math.Max(1, nx.Multiplier)}
                    },
                    adrreq.ReservationTimeInMinutes is null or 0 ? project.Expiretime : (int)adrreq.ReservationTimeInMinutes,
                    false, false, adrreq.Coin);


                if (selectedreservations3.Count == 0)
                {
                    result.ApiError.ErrorCode = 10;
                    result.ApiError.ErrorMessage = $"NFT not available (Not so many tokens available) {rn.NftId}";
                    result.ApiError.ResultState = ResultStates.Error;
                    await NftReservationClass.ReleaseAllNftsAsync(db, redis, adrreq.Uid);
                    result.StatusCode = 404;
                    return result;
                }

               

                // Calculate price
                if (adrreq.Price != null) continue;
                if (rn.Lovelace != null && rn.Lovelace > 0)
                {
                    lovelace += (long) rn.Lovelace;
                }
                else
                {
                    var found = false;

                    // Specific Price Cardano
                    if (nx.Price != null && nx.Price > 0)
                    {
                        lovelace += (long) nx.Price * rn.Tokencount ?? 1;
                        found = true;
                    }

                    // Specific Price Solana
                    if (nx.Pricesolana is > 0)
                    {
                        lamports += (long)nx.Pricesolana * rn.Tokencount ?? 1;
                        found = true;
                    }
                    // Specific Price Aptos
                    if (nx.Priceaptos is > 0)
                    {
                        octas += (long)nx.Priceaptos * rn.Tokencount ?? 1;
                        found = true;
                    }

                    // No specific Price - take it from the pricelist
                    if (!found)
                    {
                        var pricelist =
                            await GetPricelistClass.GetPriceForCountOfNfts(db, project, redis, adrreq.CountNft, adrreq.Coin);

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
                        adrreq.BitcoinSatoshi= pricelist.PriceInSatoshis;
                        if (pricelist.AdditionalPriceInTokens != null && pricelist.AdditionalPriceInTokens.Any())
                        {
                            adrreq.PriceInToken = pricelist.AdditionalPriceInTokens.First().TotalCount / GlobalFunctions.GetMultiplierFromDecimals(pricelist.AdditionalPriceInTokens.First().Decimals);
                            adrreq.TokenPolicyId = pricelist.AdditionalPriceInTokens.First().PolicyId;
                            adrreq.TokenAssetId = pricelist.AdditionalPriceInTokens.First().AssetName;
                            adrreq.TokenAssetIdHex = pricelist.AdditionalPriceInTokens.First().AssetNameInHex;
                            adrreq.TotalTokens = pricelist.AdditionalPriceInTokens.First().TotalCount;
                            adrreq.Multiplier =
                                GlobalFunctions.GetMultiplierFromDecimals(pricelist.AdditionalPriceInTokens.First().Decimals);
                            adrreq.Decimals = pricelist.AdditionalPriceInTokens.First().Decimals;
                        }

                    }
                }
            }

            adrreq.CardanoLovelace ??= lovelace;
            adrreq.SolanaLamport ??= lamports;
            adrreq.AptosOcta ??= octas;

            var selectedreservations = (from a in db.Nftreservations
                where a.Reservationtoken == adrreq.Uid
                select a).ToList();


            bool error = false;
            // Check in NftToNftaddresses Database if there is a reservation
            foreach (var n1 in selectedreservations)
            {
                var nftto = await (from a in db.Nfttonftaddresses
                    where a.NftId == n1.NftId
                    select a).CountAsync();

                switch (project.Maxsupply)
                {
                    case 1 when nftto > 0:
                        result.ApiError.ErrorCode = 580;
                        result.ApiError.ErrorMessage = "Internal error (3). Please contact support";
                        result.ApiError.ResultState = ResultStates.Error;

                        var nft1 = await (from a in db.Nfts
                            where a.Id == n1.NftId
                            select a).FirstOrDefaultAsync();
                        // Mark the nft as sold - because it is not free
                        if (nft1 != null)
                        {
                            nft1.State = "error";
                            nft1.Soldcount = 0;
                            nft1.Reservedcount = 0;
                            nft1.Errorcount = 1;
                            nft1.Checkpolicyid = true;
                            nft1.Markedaserror = DateTime.Now;
                            nft1.Buildtransaction =
                                $"API-CALL GetAddressForSpecificNftSale from {adrreq.RemoteIpAddress}: ERROR: Reserved NFT not free (specific) {project.Id} - {n1.NftId} - {project.Projectname}";
                            await db.SaveChangesAsync();
                        }

                        error = true;
                        break;

                    case > 1 when nftto + n1.Tc > project.Maxsupply:
                        result.ApiError.ErrorCode = 590;
                        result.ApiError.ErrorMessage = "Internal error. Please contact support";
                        result.ApiError.ResultState = ResultStates.Error;
                        error = true;
                        break;
                }
            }

            if (error)
            {
                await NftReservationClass.ReleaseAllNftsAsync(db, redis, adrreq.Uid);
                result.StatusCode = 500;
                return result;
            }



            // Receiver min 2 Lovelace
            // Sender to send back min 2 Lovelace
            // Mintingcosts min 2 Lovelace
            // Fees min 1 Lovelace

            long sendback = GlobalFunctions.CalculateSendbackToUser(db, redis, adrreq.CountNft, project.Id);
            // Add Sendback when decentral payment is enabled
            if (project.Enabledecentralpayments && adrreq.Price > 0)
            {
                adrreq.Price += sendback;
            }

            if (adrreq.Coin == Coin.ADA)
            {
                /*  long mincosts =
                      GlobalFunctions.CalculateMinutxoNew(project, selectedreservations.Count, (long) adrreq.Price);
                  if (adrreq.Price < mincosts && adrreq.Price > 0 && adrreq.Price != 2000000)
                  {
                      result.ApiError.ErrorCode = 52;
                      result.ApiError.ErrorMessage =
                          $"Lovelace Amount is too small - min. {((double) mincosts / (double) 1000000)} ADA";
                      result.ApiError.ResultState = ResultStates.Error;
                      result.StatusCode = 406;
                      await NftReservationClass.ReleaseAllNftsAsync(db, redis, adrreq.Uid);

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
                                 where a.Referertoken == adrreq.Referer && a.State == "active"
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
                db, project,"specific", selectedreservations, sendback, referer);
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
