using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NMKR.Api.Controllers.SharedClasses;
using NMKR.Shared.Classes;
using NMKR.Shared.Classes.CardanoSerialisationLibClasses;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.Koios;
using NMKR.Shared.Functions.Metadata;
using NMKR.Shared.Functions.SaleConditions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.Paymenttransactions.Processing.Decentral
{
    /// <summary>
    /// Signs a multi-sig (decentral) mint and send command
    /// </summary>
    public class SignDecentralPaymentMintAndSaleClass : ControllerBase, IProcessPaymentTransactionInterface
    {

        private readonly IConnectionMultiplexer _redis;


        /// <summary>
        /// Signs a multi-sig (decentral) mint and send command
        /// </summary>
        /// <param name="redis"></param>
        public SignDecentralPaymentMintAndSaleClass(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }


        /// <summary>
        /// Signs a multi-sig (decentral) mint and send command
        /// </summary>
        /// <param name="db"></param>
        /// <param name="apikey"></param>
        /// <param name="remoteipaddress"></param>
        /// <param name="result"></param>
        /// <param name="preparedtransaction"></param>
        /// <param name="postparameter1"></param>
        /// <returns></returns>

        public async Task<IActionResult> ProcessTransaction(EasynftprojectsContext db, string apikey, string remoteipaddress, ApiErrorResultClass result,
            Preparedpaymenttransaction preparedtransaction, object postparameter1)
        {
            var signDecentralClass = postparameter1 as SignDecentralClass;

            await GlobalFunctions.LogMessageAsync(db, "API-Call: SignDecentralPaymentMintAndSale", JsonConvert.SerializeObject(signDecentralClass));


            if (preparedtransaction.Transactiontype != nameof(PaymentTransactionTypes.decentral_mintandsale_specific) &&
                preparedtransaction.Transactiontype != nameof(PaymentTransactionTypes.decentral_mintandsale_random))
            {
                result.ErrorCode = 1102;
                result.ErrorMessage = "Command does not fit to this transaction";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            switch (preparedtransaction.State)
            {
                //  'active','expired','finished','prepared'
                case nameof(PaymentTransactionsStates.expired):
                    result.ErrorCode = 1305;
                    result.ErrorMessage = "Transaction already expired";
                    result.ResultState = ResultStates.Error;
                    return StatusCode(406, result);
                case nameof(PaymentTransactionsStates.finished):
                    result.ErrorCode = 1105;
                    result.ErrorMessage = "Transactioncommand already finished";
                    result.ResultState = ResultStates.Error;
                    return StatusCode(406, result);
                case nameof(PaymentTransactionsStates.error):
                    result.ErrorCode = 1106;
                    result.ErrorMessage = "Transactioncommand had errors";
                    result.ResultState = ResultStates.Error;
                    return StatusCode(406, result);
                case nameof(PaymentTransactionsStates.canceled):
                    result.ErrorCode = 1107;
                    result.ErrorMessage = "Transactioncommand already canceled";
                    result.ResultState = ResultStates.Error;
                    return StatusCode(406, result);
            }
            if (signDecentralClass == null)
            {
                result.ErrorCode = 1141;
                result.ErrorMessage = "No SignDecentralClass submitted";
                result.ResultState = ResultStates.Error;
                await GlobalFunctions.LogMessageAsync(db, $"API-Call: Decentral transaction error - {preparedtransaction.NftprojectId} - {result.ErrorMessage}", JsonConvert.SerializeObject(signDecentralClass));
                return StatusCode(406, result);
            }

            if (signDecentralClass.Buyer == null)
            {
                result.ErrorCode = 1141;
                result.ErrorMessage = "No buyer submitted";
                result.ResultState = ResultStates.Error;
                await GlobalFunctions.LogMessageAsync(db, $"API-Call: Decentral transaction error - {preparedtransaction.NftprojectId} - {result.ErrorMessage}", JsonConvert.SerializeObject(signDecentralClass));
                return StatusCode(406, result);
            }

            if (string.IsNullOrEmpty(signDecentralClass.Buyer.ChangeAddress))
            {
                result.ErrorCode = 1142;
                result.ErrorMessage = "You must submit a change address - we will send the nft(s) to this address";
                result.ResultState = ResultStates.Error;
                await GlobalFunctions.LogMessageAsync(db, $"API-Call: Decentral transaction error - {preparedtransaction.NftprojectId} - {result.ErrorMessage}", JsonConvert.SerializeObject(signDecentralClass));
                return StatusCode(406, result);
            }

            if (!ConsoleCommand.CheckIfAddressIsValid(db, signDecentralClass.Buyer.ChangeAddress,
                    GlobalFunctions.IsMainnet(), out string outaddress, out Blockchain blockchain, false))
            {
                result.ErrorCode = 1159;
                result.ErrorMessage = "Change address is not a valid cardano address";
                result.ResultState = ResultStates.Error;
                await GlobalFunctions.LogMessageAsync(db, $"API-Call: Decentral transaction error - {preparedtransaction.NftprojectId} - {result.ErrorMessage}", JsonConvert.SerializeObject(signDecentralClass));
                return StatusCode(406, result);
            }

            if (signDecentralClass.Buyer.Addresses == null || !signDecentralClass.Buyer.Addresses.Any())
            {
                result.ErrorCode = 1143;
                result.ErrorMessage = "You must submit a the address(es) of the buyer";
                result.ResultState = ResultStates.Error;
                await GlobalFunctions.LogMessageAsync(db, $"API-Call: Decentral transaction error - {preparedtransaction.NftprojectId} - {result.ErrorMessage}", JsonConvert.SerializeObject(signDecentralClass));
                return StatusCode(406, result);
            }
            if (preparedtransaction.Nftproject.CustomerwalletId==null)
            {
                result.ErrorCode = 1144;
                result.ErrorMessage = "Seller has no payout wallet defined";
                result.ResultState = ResultStates.Error;
                preparedtransaction.State = nameof(PaymentTransactionsStates.error);
                await GlobalFunctions.LogMessageAsync(db, $"API-Call: Decentral transaction error - {preparedtransaction.NftprojectId} - {result.ErrorMessage}", JsonConvert.SerializeObject(signDecentralClass));
                await db.SaveChangesAsync();
                return StatusCode(500, result);
            }

            if (preparedtransaction.Nftproject.Paymentgatewaysalestart != null &&
                preparedtransaction.Nftproject.Paymentgatewaysalestart > DateTime.Now)
            {
                result.ErrorCode = 1154;
                result.ErrorMessage = "Project has not reached start time";
                result.ResultState = ResultStates.Error;
                preparedtransaction.State = nameof(PaymentTransactionsStates.error);
                await GlobalFunctions.LogMessageAsync(db, $"API-Call: Decentral transaction error - {preparedtransaction.NftprojectId} - {result.ErrorMessage}", JsonConvert.SerializeObject(signDecentralClass));
                await db.SaveChangesAsync();
                return StatusCode(406, result);
            }




            // When the tx was already started before - but we receive the sign twice - release the old nft and create a new transaction
            if ((preparedtransaction.State == nameof(PaymentTransactionsStates.active) || preparedtransaction.State== nameof(PaymentTransactionsStates.rejected)) && !string.IsNullOrEmpty(preparedtransaction.Reservationtoken))
            {
                await NftReservationClass.ReleaseAllNftsAsync(db,_redis, preparedtransaction.Reservationtoken);
                await GlobalFunctions.ExecuteSqlWithFallbackAsync(db,
                    $"delete from preparedpaymenttransactions_nfts where preparedpaymenttransactions_id={preparedtransaction.Id}");
                await GlobalFunctions.ExecuteSqlWithFallbackAsync(db,
                    $"delete from preparedpaymenttransactions_smartcontractsjsons where preparedpaymenttransactions_id={preparedtransaction.Id}");

                preparedtransaction.State = nameof(PaymentTransactionsStates.prepared);
                await db.SaveChangesAsync();
            }


            long c = preparedtransaction.Countnft ?? 1;
            if (preparedtransaction.Nftproject.Maxsupply > 1)
                c = preparedtransaction.PreparedpaymenttransactionsNfts.Sum(x => x.Count) *
                    Math.Max(1, preparedtransaction.Nftproject.Multiplier);


            // Check Sale Conditions
            var cond = await CheckSalesConditionClass.CheckForSaleConditionsMet(db, _redis, preparedtransaction.NftprojectId,
                signDecentralClass.Buyer.ChangeAddress, c, 0, preparedtransaction.Nftproject.Usefrankenprotection, Blockchain.Cardano);

            if (!cond.ConditionsMet)
            {
                var result1 = new RejectedErrorResultClass
                {
                    ErrorCode = 1300,
                    ErrorMessage = "Paymentcondition not met",
                    ResultState = ResultStates.Error,
                    RejectParameter = cond.RejectParameter,
                    RejectReason = cond.RejectReason
                };

                preparedtransaction.State = nameof(PaymentTransactionsStates.rejected);
                preparedtransaction.Rejectparameter = cond.RejectParameter;
                preparedtransaction.Rejectreason = cond.RejectReason;
                await db.SaveChangesAsync();

                await GlobalFunctions.LogMessageAsync(db, "Api Salecondition not met " + preparedtransaction.NftprojectId + " " + preparedtransaction.Nftproject.Projectname + " " + signDecentralClass.Buyer.ChangeAddress,
                    signDecentralClass.Buyer.ChangeAddress + Environment.NewLine + cond.RejectParameter +
                    Environment.NewLine + cond.RejectReason);

                return StatusCode(412, result1); // Status412PreconditionFailed
            }


            string guid = GlobalFunctions.GetGuid();
            List<Nftreservation> selectedreservations = new();
            var mints = new List<Token>();
            long multiplier = Math.Max(1, preparedtransaction.Nftproject.Multiplier);
            if (preparedtransaction.Transactiontype == nameof(PaymentTransactionTypes.decentral_mintandsend_random) ||
                preparedtransaction.Transactiontype == nameof(PaymentTransactionTypes.decentral_mintandsale_random))
            {
                selectedreservations =
                    await NftReservationClass.ReserveRandomNft(db, _redis, guid, preparedtransaction.NftprojectId, preparedtransaction.Countnft??1, preparedtransaction.Nftproject.Expiretime, false, true, Coin.ADA);

               
                if (!selectedreservations.Any() || selectedreservations.Sum(x=>x.Tc) != preparedtransaction.Countnft)
                {
                    result.ErrorCode = 1701;
                    result.ErrorMessage = "NFT Reservation not possible. No more NFTs available?";
                    result.ResultState = ResultStates.Error;
                    await NftReservationClass.ReleaseAllNftsAsync(db,_redis, guid);
                    //preparedtransaction.IsActive = nameof(PaymentTransactionsStates.error);
                    preparedtransaction.Logfile = result.ErrorMessage + Environment.NewLine;
                    await db.SaveChangesAsync();

                    await GlobalFunctions.LogMessageAsync(db,
                        $"API: NFT Reservation not possible. {preparedtransaction.NftprojectId}",
                        selectedreservations.Count() + " - " + selectedreservations.Sum(x => x.Tc) + " - " +
                        preparedtransaction.Countnft + JsonConvert.SerializeObject(preparedtransaction.Nftproject));
                    await GlobalFunctions.LogMessageAsync(db, $"API-Call: Decentral transaction error - {preparedtransaction.NftprojectId} - {result.ErrorMessage}", JsonConvert.SerializeObject(signDecentralClass));
                    return StatusCode(404, result);
                }


                // Write the selected NFTS to the database
                foreach (var nftreservation in selectedreservations)
                {
                    var nft = await (from a in db.Nfts
                                     where a.Id == nftreservation.NftId
                                     select a).FirstOrDefaultAsync();
                    if (nft == null)
                    {
                        result.ErrorCode = 1702;
                        result.ErrorMessage = "NFT Reservation not possible. No more NFTs available?";
                        result.ResultState = ResultStates.Error;
                        preparedtransaction.Logfile = result.ErrorMessage + Environment.NewLine;
                        await NftReservationClass.ReleaseAllNftsAsync(db,_redis, guid);
                        await GlobalFunctions.LogMessageAsync(db,
                            $"API: NFT Reservation not possible. (2) {preparedtransaction.NftprojectId}",
                            selectedreservations.Count() + " - " + selectedreservations.Sum(x => x.Tc) + " - " +
                            preparedtransaction.Countnft + JsonConvert.SerializeObject(preparedtransaction.Nftproject));
                        await GlobalFunctions.LogMessageAsync(db, $"API-Call: Decentral transaction error - {preparedtransaction.NftprojectId} - {result.ErrorMessage}", JsonConvert.SerializeObject(signDecentralClass));
                        return StatusCode(404, result);
                    }

                    await db.PreparedpaymenttransactionsNfts.AddAsync(new()
                    {
                        Count = nftreservation.Tc,
                        NftId = nftreservation.NftId,
                        PreparedpaymenttransactionsId = preparedtransaction.Id,
                        Lovelace = 0,
                        Policyid = preparedtransaction.Nftproject.Policyid,
                        Tokenname = (preparedtransaction.Nftproject.Tokennameprefix??"") + nft.Name,
                        Tokennamehex = GlobalFunctions.ToHexString((preparedtransaction.Nftproject.Tokennameprefix??"") + nft.Name),
                        Nftuid = nft.Uid
                    });
                    await db.SaveChangesAsync();

                    string metadata = "";
                    if (preparedtransaction.Nftproject.Cip68)
                    {
                        GetMetadataClass gmcx = new GetMetadataClass(nft.Id,preparedtransaction.Buyeraddress, true, db);
                        var mra = (await gmcx.MetadataResultAsync());
                        metadata = mra.Metadata;
                        if (!string.IsNullOrEmpty(mra.Error))
                        {
                            await GlobalFunctions.LogMessageAsync(db, "Error in CIP68 Metadata", mra.Error);
                            result.ErrorCode = 1702;
                            result.ErrorMessage = $"Error in Metadata - {mra.Error}";
                            result.ResultState = ResultStates.Error;
                            return StatusCode(406, result);
                        }
                    }

                    mints.Add(new()
                    {
                        Count = nftreservation.Tc * Math.Max(1, nftreservation.Multiplier),
                        PolicyId = preparedtransaction.Nftproject.Policyid,
                        TokenName = GlobalFunctions.ToHexString((preparedtransaction.Nftproject.Tokennameprefix??"") + nft.Name),
                        PolicyScriptJson = preparedtransaction.Nftproject.Policyscript,
                        Metadata = metadata,
                    });
                }
            }

            if (preparedtransaction.Transactiontype == nameof(PaymentTransactionTypes.decentral_mintandsend_specific) ||
                preparedtransaction.Transactiontype == nameof(PaymentTransactionTypes.decentral_mintandsale_specific))
            {
                
                var nfts = (from a in preparedtransaction.PreparedpaymenttransactionsNfts
                            select new ReserveNftsClass() { Multiplier = multiplier, NftId = (int)a.NftId, Tokencount = a.Count })
                    .ToArray();

                selectedreservations =
                    await NftReservationClass.ReserveSpecificNft(db,_redis,guid, preparedtransaction.NftprojectId, nfts, preparedtransaction.Nftproject.Expiretime, false, true, Coin.ADA);

                if (selectedreservations.Count != nfts.Length)
                {
                    result.ErrorCode = 1153;
                    result.ErrorMessage = "NFT Reservation not possible. NFT not available";
                    result.ResultState = ResultStates.Error;
                    preparedtransaction.State = nameof(PaymentTransactionsStates.error);
                    preparedtransaction.Logfile = result.ErrorMessage + Environment.NewLine;
                    await db.SaveChangesAsync();
                    await NftReservationClass.ReleaseAllNftsAsync(db,_redis, guid);
                    await GlobalFunctions.LogMessageAsync(db,
                        $"API: NFT Reservation not possible. (3) {preparedtransaction.NftprojectId}",
                        selectedreservations.Count() + " - " + selectedreservations.Sum(x => x.Tc) + " - " +
                        preparedtransaction.Countnft + JsonConvert.SerializeObject(preparedtransaction.Nftproject));
                    await GlobalFunctions.LogMessageAsync(db, $"API-Call: Decentral transaction error - {preparedtransaction.NftprojectId} - {result.ErrorMessage}", JsonConvert.SerializeObject(signDecentralClass));
                    return StatusCode(404, result);
                }

                foreach (var nftreservation in selectedreservations)
                {
                    var nft = await (from a in db.Nfts
                                     where a.Id == nftreservation.NftId
                                     select a).FirstOrDefaultAsync();
                    if (nft == null)
                    {
                        result.ErrorCode = 1703;
                        result.ErrorMessage = "NFT Reservation not possible. No more NFTs available?";
                        result.ResultState = ResultStates.Error;
                        preparedtransaction.State = nameof(PaymentTransactionsStates.error);
                        preparedtransaction.Logfile = result.ErrorMessage + Environment.NewLine;
                        await db.SaveChangesAsync();
                        await NftReservationClass.ReleaseAllNftsAsync(db,_redis, guid);
                        await GlobalFunctions.LogMessageAsync(db,
                            $"API: NFT Reservation not possible. (4) {preparedtransaction.NftprojectId}",
                            selectedreservations.Count() + " - " + selectedreservations.Sum(x => x.Tc) + " - " +
                            preparedtransaction.Countnft + JsonConvert.SerializeObject(preparedtransaction.Nftproject));
                        await GlobalFunctions.LogMessageAsync(db, $"API-Call: Decentral transaction error - {preparedtransaction.NftprojectId} - {result.ErrorMessage}", JsonConvert.SerializeObject(signDecentralClass));
                        return StatusCode(404, result);
                    }

                    string metadata = "";
                    if (preparedtransaction.Nftproject.Cip68)
                    {
                        GetMetadataClass gmcx = new GetMetadataClass(nft.Id,preparedtransaction.Buyeraddress, true, db);
                        var mra = await gmcx.MetadataResultAsync();
                        metadata = mra.Metadata;
                        if (!string.IsNullOrEmpty(mra.Error))
                        {
                            await GlobalFunctions.LogMessageAsync(db, "Error in CIP68 Metadata", mra.Error);
                            result.ErrorCode = 1702;
                            result.ErrorMessage = $"Error in Metadata - {mra.Error}";
                            result.ResultState = ResultStates.Error;
                            return StatusCode(406, result);
                        }
                    }


                    mints.Add(new()
                    {
                        Count = nftreservation.Tc * Math.Max(1, nftreservation.Multiplier),
                        PolicyId = preparedtransaction.Nftproject.Policyid,
                        TokenName = GlobalFunctions.ToHexString(preparedtransaction.Nftproject.Tokennameprefix + nft.Name),
                        PolicyScriptJson = preparedtransaction.Nftproject.Policyscript,
                        Metadata= metadata,
                    });
                }
            }



            // Create the Transaction

            preparedtransaction.Expires = DateTime.Now.AddMinutes(preparedtransaction.Nftproject.Expiretime);
            preparedtransaction.Reservationtoken = guid;
            preparedtransaction.State = nameof(PaymentTransactionsStates.active);
            preparedtransaction.Buyeraddress = signDecentralClass.Buyer.ChangeAddress;
            preparedtransaction.Buyeraddresses = string.Join(Environment.NewLine, signDecentralClass.Buyer.Addresses.Select(x => x.Address).ToArray());
            preparedtransaction.Changeaddress= signDecentralClass.Buyer.ChangeAddress;
            preparedtransaction.Buyerpkh = signDecentralClass.Buyer.Pkh;
            await db.SaveChangesAsync();


            long networkfees = preparedtransaction.Nftproject.Settings.Minfees;

            
            var minutxoforbuyer = GlobalFunctions.CalculateMinutxoForBuyer(preparedtransaction.Nftproject,
                    selectedreservations.Count, (long)(preparedtransaction.Lovelace ?? 0));

            // Pay with tokens
            string searchForToken = null;
            long searchForTokenCount = 0;
            if (preparedtransaction.PreparedpaymenttransactionsTokenprices != null && preparedtransaction.PreparedpaymenttransactionsTokenprices.Any())
            {
                var tok = preparedtransaction.PreparedpaymenttransactionsTokenprices.First();
                searchForTokenCount = tok.Tokencount;
                searchForToken = tok.Policyid;
                if (!string.IsNullOrEmpty(tok.Assetname))
                    searchForToken += "." + tok.Assetname;
            }

            // Find the needed txins
            var utxofinal = StaticTransactionFunctions.GetAllNeededTxin(_redis, signDecentralClass.Buyer.Addresses,
                (long)(preparedtransaction.Lovelace ?? 0), //+ networkfees + 1000000,
                searchForTokenCount, searchForToken, signDecentralClass.Buyer.CollateralTxIn, out string errormessage, out AllTxInAddressesClass alltxin);

            if (utxofinal == null || utxofinal.Sum(x=>x.LovelaceSummary) < preparedtransaction.Lovelace)
            {
                await NftReservationClass.ReleaseAllNftsAsync(db,_redis, guid);
                result.ErrorCode = 1138;
                result.ErrorMessage = "Too less ADA in the Wallet";
                result.ResultState = ResultStates.Error;
                await db.SaveChangesAsync();

                await GlobalFunctions.LogMessageAsync(db, $"GetAllNeddedTxIn - Error: {errormessage} - SignDecentralMintAndSale " + preparedtransaction.Transactionuid,
                    JsonConvert.SerializeObject(signDecentralClass.Buyer.Addresses) + Environment.NewLine +
                    Environment.NewLine + JsonConvert.SerializeObject(alltxin) + Environment.NewLine +
                    Environment.NewLine +
                    "Lovelace:"+ ((long)(preparedtransaction.Lovelace ?? 0) ) + Environment.NewLine +
                    "Network fees:"+((long)networkfees) + Environment.NewLine +
                    "MinutxoBuyer:"+((long)minutxoforbuyer) + Environment.NewLine +
                    Environment.NewLine +
                    signDecentralClass.Buyer.CollateralTxIn + Environment.NewLine + Environment.NewLine +
                    searchForTokenCount +
                    Environment.NewLine + Environment.NewLine + searchForToken ?? "" + Environment.NewLine+"Transaction UID:+"+preparedtransaction.Transactionuid);

                return StatusCode(406, result);
            }

            StaticTransactionFunctions.SaveRecentlyUsedTxHashes(_redis,utxofinal);

            await GlobalFunctions.LogMessageAsync(db, $"GetAllNeddedTxIn - Successful: {signDecentralClass.Buyer.ChangeAddress}",
                JsonConvert.SerializeObject(alltxin) + Environment.NewLine+Environment.NewLine + JsonConvert.SerializeObject(utxofinal));


            string matxrawfile = GeneralConfigurationClass.TempFilePath + "matx" + guid + ".raw";
            string protocolparamsfile = GeneralConfigurationClass.TempFilePath + "protocol" + guid + ".params";

           

            GetMetadataClass gmc = new((from a in selectedreservations select new NftIdWithMintingAddressClass(a.NftId,preparedtransaction.Buyeraddress)).ToArray());//, preparedtransaction.Nftproject.Maxsupply!=1);
            BuildTransactionClass bt = new();

            var qt = ConsoleCommand.GetQueryTip();
            long ttl = (long)qt.Slot + 7200;

            string selleraddress = preparedtransaction.Selleraddress;
            if (preparedtransaction.Selleraddress == null)
            {
                await NftReservationClass.ReleaseAllNftsAsync(db, _redis, guid);
                result.ErrorCode = 1438;
                result.ErrorMessage = "No Sellerwallet specified";
                result.ResultState = ResultStates.Error;
                await db.SaveChangesAsync();
                await GlobalFunctions.LogMessageAsync(db, "SignDecentralMintAndSale - Error ",
                    "No Sellerwallet specified");
                return StatusCode(406, result);
            }

            // Set Stakepool (NMKR) Discounts
            var rewards = await RewardsClass.GetTokenAndStakeRewards(db, _redis, signDecentralClass.Buyer.ChangeAddress);

            if (rewards.StakeReward > 0)
            {
                preparedtransaction.Stakerewards = rewards.StakeReward;
                await db.SaveChangesAsync();
            }

            if (rewards.TokenReward > 0)
            {
                preparedtransaction.Tokenrewards = rewards.TokenReward;
                await db.SaveChangesAsync();
            }

            preparedtransaction.Logfile ??= "";

            // Check if lovelace is null (should not be happen, but sometimes, this occurs - and we dont know why
            if (preparedtransaction.Lovelace == null && preparedtransaction.Transactiontype ==
                nameof(PaymentTransactionTypes.decentral_mintandsale_random))
            {
                var pricelist = (from a in db.Pricelists
                    where a.NftprojectId == preparedtransaction.NftprojectId && a.State == "active" &&
                          (a.Validfrom == null || a.Validfrom <= DateTime.Now) &&
                          (a.Validto == null || a.Validto >= DateTime.Now) && a.Countnftortoken ==
                          preparedtransaction.Countnft
                                 select a).AsNoTracking().FirstOrDefault();
                if (pricelist != null)
                {
                    preparedtransaction.Lovelace = GlobalFunctions.GetPriceInEntities(_redis, pricelist);
                    await db.SaveChangesAsync();
                }
                else
                {
                    await NftReservationClass.ReleaseAllNftsAsync(db, _redis, guid);
                    result.ErrorCode = 1245;
                    result.ErrorMessage = "No price found";
                    result.ResultState = ResultStates.Error;
                    preparedtransaction.State = nameof(PaymentTransactionsStates.error);
                    preparedtransaction.Logfile += result.ErrorMessage + Environment.NewLine + result.InnerErrorMessage;
                    await db.SaveChangesAsync();
                    StaticTransactionFunctions.DeleteRecentlyUsedTxHashes(_redis, utxofinal);
                    await GlobalFunctions.LogMessageAsync(db, "SignDecentralMintAndSale - Error ",
                        "No Price found");
                    return StatusCode(500, result);
                }
            }

            // Set Discounts
            var discount = await PriceListDiscountClass.GetPricelistDiscount(db, _redis,
                preparedtransaction.NftprojectId, signDecentralClass.Buyer.ChangeAddress, preparedtransaction.Referer,
                preparedtransaction.PreparedpaymenttransactionsCustomproperties.FirstOrDefault()?.Value, 0, Blockchain.Cardano);
            preparedtransaction.Logfile += "Discount: " + signDecentralClass.Buyer.ChangeAddress + " - " + (discount!=null? discount?.Sendbackdiscount:"0") +
                                           Environment.NewLine;
            if (discount is {Sendbackdiscount: > 0})
            {
                var d = (long)((preparedtransaction.Lovelace ?? 0) / 100f * discount.Sendbackdiscount);
                preparedtransaction.Logfile += "Discount ADA: " + d + Environment.NewLine;
                preparedtransaction.Discount = d;
            }
            await db.SaveChangesAsync();


            var mintingcosts = GlobalFunctions
                .GetMintingcosts2( preparedtransaction.NftprojectId, selectedreservations.Count, preparedtransaction.Lovelace??0)
                .Costs - rewards.TotalRewards;

            PromotionClass promotion = null;
            if (preparedtransaction.PromotionId != null)
            {
                promotion = await GlobalFunctions.GetPromotionAsync(db,_redis, (int)preparedtransaction.PromotionId, preparedtransaction.Promotionmultiplier ?? 1);
            }

            var cp = preparedtransaction.PreparedpaymenttransactionsCustomproperties
                .FirstOrDefault(x => x.Key == "cp");
            string customproperty = cp?.Value ?? string.Empty;

            var additionalpayoutswallets = await (from a in db.Nftprojectsadditionalpayouts
                    .Include(a=>a.Wallet)
                where a.NftprojectId == preparedtransaction.NftprojectId && 
                      a.Coin==Coin.ADA.ToString() &&
                      (a.Custompropertycondition == null || a.Custompropertycondition == "")
                                                  select a).AsNoTracking().ToListAsync();

            CreateMintAndSendParametersClass cmaspc = new()
            {
                utxofinal = utxofinal,
                BuyerChangeAddress = signDecentralClass.Buyer.ChangeAddress,
                OptionalReceiverAddress = preparedtransaction.Optionalreceiveraddress,
                BuyerHasToPayInLovelace = preparedtransaction.Lovelace>0? Math.Max(1000000, (long) (preparedtransaction.Lovelace ?? 0) - 
                                                                                                    (preparedtransaction.Discount ?? 0) - 
                                                                                                    (preparedtransaction.Stakerewards ?? 0) - 
                                                                                                    (preparedtransaction.Tokenrewards ?? 0)) : 0,
                AdditionalPriceInTokens= preparedtransaction.PreparedpaymenttransactionsTokenprices?.ToList(),
                project = preparedtransaction.Nftproject,
                selectedreservations = selectedreservations,
                MetadataResult = (await gmc.MetadataResultAsync()).Metadata,
                MinUtxo = minutxoforbuyer,
                Discount=preparedtransaction.Discount??0,
                Stakerewards=preparedtransaction.Stakerewards??0,
                TokenRewards=preparedtransaction.Tokenrewards??0,
                Mintingcosts = mintingcosts>0? Math.Max(1000000, mintingcosts):0,
                MintingcostsAddress = preparedtransaction.Nftproject.Settings.Mintingaddress,
                AdditionalPayouts= additionalpayoutswallets.ToArray(),
              //  MintTokensString = ConsoleCommand.GetSendTokensString(db, selectedreservations, preparedtransaction.Nftproject),
                MintTokens = mints.ToArray(),
                ttl = ttl,
                SellerAddress = selleraddress,
                IncludeMetadataHashOnly = false,
                ReferenceAddress = preparedtransaction.Nftproject.Cip68referenceaddress,
                Promotion=promotion,
                Fees = 350000  // when null, max uses the changeaddress - but we will not need the change address, so set to 1 and calcluate the fee itself
            };

            // Set Royalties if necessary
            if (!string.IsNullOrEmpty(preparedtransaction.Createroyaltytokenaddress))
            {
                var getroyalty = await KoiosFunctions.GetRoyaltiesFromPolicyIdAsync(preparedtransaction.Nftproject.Policyid);
                if (getroyalty == null)
                {
                    cmaspc.Createroyaltytokenaddress = preparedtransaction.Createroyaltytokenaddress;
                    cmaspc.Createroyaltytokenpercentage = preparedtransaction.Createroyaltytokenpercentage;


                    var r = await GlobalFunctions.CreateBurningAddressAsync(db, cmaspc.project.Id, DateTime.Now.AddHours(4), Blockchain.Cardano, false);
                    if (r == null)
                    {
                        await NftReservationClass.ReleaseAllNftsAsync(db,_redis ,guid);
                        result.ErrorCode = 1145;
                        result.ErrorMessage = "Error while creating the Burning address";
                        result.ResultState = ResultStates.Error;
                        preparedtransaction.State = nameof(PaymentTransactionsStates.error);
                        preparedtransaction.Logfile += result.ErrorMessage + Environment.NewLine + result.InnerErrorMessage;
                        await db.SaveChangesAsync();
                        StaticTransactionFunctions.DeleteRecentlyUsedTxHashes(_redis, utxofinal);
                        await GlobalFunctions.LogMessageAsync(db, "SignDecentralMintAndSale - Error ",
                            "Error while creating the Burning address");

                        return StatusCode(500, result);
                    }
                    cmaspc.Burningaddress = r.Address;
                }
            }



            var cborNoFee = preparedtransaction.Nftproject.Cip68
                ? await ConsoleCommand.CreateDecentralPaymentCip68(cmaspc, db, _redis, GlobalFunctions.IsMainnet()) :
                await ConsoleCommand.CreateDecentralPaymentByCsl(cmaspc, db, _redis, GlobalFunctions.IsMainnet()); 
              //  await CardanoSharpFunctions.CreateDecentralPaymentByCardanoSharp(cmaspc,db, _redis,GlobalFunctions.IsMainnet());
            if (cborNoFee==null || string.IsNullOrEmpty(cborNoFee.CslResult))
            {
                await NftReservationClass.ReleaseAllNftsAsync(db,_redis, guid);
                result.ErrorCode = 1131;
                result.ErrorMessage = "Error while creating the Transaction";
                result.ResultState = ResultStates.Error;
                if (cborNoFee != null) result.InnerErrorMessage = cborNoFee.CreatedJson ?? "";
                preparedtransaction.State = nameof(PaymentTransactionsStates.error);
                preparedtransaction.Logfile +=  result.ErrorMessage + Environment.NewLine + result.InnerErrorMessage;
                await db.SaveChangesAsync();
                StaticTransactionFunctions.DeleteRecentlyUsedTxHashes(_redis, utxofinal);
                await GlobalFunctions.LogMessageAsync(db, "SignDecentralMintAndSale - Error ",
                    "Error while creating the Transaction" + Environment.NewLine + cborNoFee.CslError + Environment.NewLine+Environment.NewLine + cborNoFee.CreatedJson);

                return StatusCode(500, result);
            }

            // Calculate the Min. Fees
           var matxrawtext = preparedtransaction.Nftproject.Cip68
                ? cborNoFee.CslResult
                : ConsoleCommand.GetCborJson(cborNoFee.CslResult);


            await System.IO.File.WriteAllTextAsync(matxrawfile, matxrawtext);
            ConsoleCommand.SaveProtocolParamsFile(_redis,protocolparamsfile, GlobalFunctions.IsMainnet(), ref bt);
            
            int txoutcount=Math.Max(3,cmaspc.AdditionalPayouts!=null?cmaspc.AdditionalPayouts.Length+3:3);
            int witnesscount = Math.Max(2, cmaspc.utxofinal.Length + 2);

            ConsoleCommand.CalculateFees(matxrawfile, utxofinal.Length, txoutcount, witnesscount, GlobalFunctions.IsMainnet(),
                protocolparamsfile, ref bt, out var fee);


            if (fee == 0)
            {
                await NftReservationClass.ReleaseAllNftsAsync(db,_redis, guid);
                result.ErrorCode = 1139;
                result.ErrorMessage = "Error while calculating the fee";
                result.ResultState = ResultStates.Error;
                if (cborNoFee != null) result.InnerErrorMessage = bt.ErrorMessage+ Environment.NewLine + bt.LogFile;
                preparedtransaction.State = nameof(PaymentTransactionsStates.error);
                preparedtransaction.Logfile += result.ErrorMessage + Environment.NewLine + result.InnerErrorMessage;
                await db.SaveChangesAsync();
                StaticTransactionFunctions.DeleteRecentlyUsedTxHashes(_redis, utxofinal);
                await GlobalFunctions.LogMessageAsync(db, "SignDecentralMintAndSale - Error ",
                    "Error while calculating the fee");
                return StatusCode(500, result);
            }


            // Create CBOR incl. Fee and Metadata
            cmaspc.Fees = fee;
            cmaspc.IncludeMetadataHashOnly = false;
            var cborFeeAndMetadata = preparedtransaction.Nftproject.Cip68 ? 
                await ConsoleCommand.CreateDecentralPaymentCip68( cmaspc,db, _redis, GlobalFunctions.IsMainnet()) :
                 await ConsoleCommand.CreateDecentralPaymentByCsl(cmaspc, db, _redis, GlobalFunctions.IsMainnet());
            //await CardanoSharpFunctions.CreateDecentralPaymentByCardanoSharp(cmaspc,db,_redis, GlobalFunctions.IsMainnet());


            // Create CBOR incl. Fee without Metadata
            cmaspc.IncludeMetadataHashOnly = true;
            var cborFeeNoMetadata = preparedtransaction.Nftproject.Cip68 ? 
                await ConsoleCommand.CreateDecentralPaymentCip68( cmaspc,db, _redis, GlobalFunctions.IsMainnet()) : 
                await ConsoleCommand.CreateDecentralPaymentByCsl(cmaspc, db, _redis, GlobalFunctions.IsMainnet());
            //await CardanoSharpFunctions.CreateDecentralPaymentByCardanoSharp(cmaspc,db,_redis, GlobalFunctions.IsMainnet());


            if (cborFeeNoMetadata == null || cborFeeAndMetadata == null || string.IsNullOrEmpty(cborFeeNoMetadata.CslResult) || string.IsNullOrEmpty(cborFeeAndMetadata.CslResult))
            {
                await NftReservationClass.ReleaseAllNftsAsync(db,_redis, guid);
                result.ErrorCode = 1132;
                result.ErrorMessage = "Error while creating the Transaction";
                result.ResultState = ResultStates.Error;
                if (cborFeeNoMetadata != null) result.InnerErrorMessage = cborFeeNoMetadata.CreatedJson ?? "";
                if (cborFeeNoMetadata != null) result.InnerErrorMessage = cborFeeNoMetadata.CreatedJson ?? "";
                preparedtransaction.State = nameof(PaymentTransactionsStates.error);
                preparedtransaction.Logfile += result.ErrorMessage + Environment.NewLine + result.InnerErrorMessage;
                await db.SaveChangesAsync();
                await GlobalFunctions.LogMessageAsync(db, "SignDecentralMintAndSale - Error while calling CSL", result.ErrorMessage + Environment.NewLine + result.InnerErrorMessage);
                StaticTransactionFunctions.DeleteRecentlyUsedTxHashes(_redis, utxofinal);
                return StatusCode(500, result);
            }

            var matxrawtextFeeNoMetadata = preparedtransaction.Nftproject.Cip68 ? cborFeeNoMetadata.CslResult: ConsoleCommand.GetCborJson(cborFeeNoMetadata.CslResult);
            var matxrawtextFeeAndMetadata = preparedtransaction.Nftproject.Cip68 ? cborFeeAndMetadata.CslResult : ConsoleCommand.GetCborJson(cborFeeAndMetadata.CslResult);

            await GlobalFunctions.LogMessageAsync(db, "Api: CBOR " + preparedtransaction.Transactionuid, cborFeeAndMetadata.CslResult);

            PreparedpaymenttransactionsSmartcontractsjson ptsj = new()
            {
                PreparedpaymenttransactionsId = preparedtransaction.Id,
                Json = "",
                Redeemer = "",
                Templatetype = "mintandsale",
                Signedcbr =  matxrawtextFeeNoMetadata ,
                Rawtx = matxrawtextFeeAndMetadata,
                Logfile = bt.LogFile,
                Fee = fee,
                Hash = "",
                Signinguid = "M" + guid,
                Signedandsubmitted = false,
                Created = DateTime.Now,
            };

            db.PreparedpaymenttransactionsSmartcontractsjsons.Add(ptsj);
            preparedtransaction.Fee = fee;
            preparedtransaction.Smartcontractstate = nameof(PaymentTransactionSubstates.readytosignbybuyer);
            preparedtransaction.Logfile += Environment.NewLine +
                                           cborNoFee.CreatedJson + Environment.NewLine +
                                           cborFeeAndMetadata.CreatedJson + Environment.NewLine +
                                           cborFeeNoMetadata.CreatedJson;
            await db.SaveChangesAsync();

            GlobalFunctions.DeleteFile(matxrawfile);

            var res = StaticTransactionFunctions.GetTransactionState(db, _redis, preparedtransaction.Transactionuid, true);

            await GlobalFunctions.LogMessageAsync(db, "SignDecentralMintAndSale - OK", JsonConvert.SerializeObject(res));

            return Ok(res);
        }

    
    }
}
