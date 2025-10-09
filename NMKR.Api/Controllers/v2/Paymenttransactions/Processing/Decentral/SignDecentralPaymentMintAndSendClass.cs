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
    public class SignDecentralPaymentMintAndSendClass : ControllerBase, IProcessPaymentTransactionInterface
    {
        private readonly IConnectionMultiplexer _redis;


        /// <summary>
        /// Signs a multi-sig (decentral) mint and send command
        /// </summary>
        /// <param name="redis"></param>
        public SignDecentralPaymentMintAndSendClass(IConnectionMultiplexer redis)
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

            await GlobalFunctions.LogMessageAsync(db, "API-Call: SignDecentralPaymentMintAndSend", JsonConvert.SerializeObject(signDecentralClass));

            if (preparedtransaction.Transactiontype != nameof(PaymentTransactionTypes.decentral_mintandsend_random) &&
                preparedtransaction.Transactiontype != nameof(PaymentTransactionTypes.decentral_mintandsend_specific))
            {
                result.ErrorCode = 1102;
                result.ErrorMessage = "Command does not fit to this transaction";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }


            switch (preparedtransaction.State)
            {
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
                return StatusCode(406, result);
            }

            if (signDecentralClass.Buyer == null)
            {
                result.ErrorCode = 1141;
                result.ErrorMessage = "No buyer submitted";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            if (string.IsNullOrEmpty(signDecentralClass.Buyer.ChangeAddress))
            {
                result.ErrorCode = 1142;
                result.ErrorMessage = "You must submit a change address - we will send the nft(s) to this address";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            if (!ConsoleCommand.CheckIfAddressIsValid(db, signDecentralClass.Buyer.ChangeAddress,
                    GlobalFunctions.IsMainnet(), out string outaddress, out Blockchain blockchain, false))
            {
                result.ErrorCode = 1159;
                result.ErrorMessage = "Change address is not a valid cardano address";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            if (signDecentralClass.Buyer.Addresses == null || !signDecentralClass.Buyer.Addresses.Any())
            {
                result.ErrorCode = 1143;
                result.ErrorMessage = "You must submit a the address(es) of the buyer";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            

            // When the tx was already started before - but we receive the sign twice - release the old nft and create a new transaction
            if (preparedtransaction.State == nameof(PaymentTransactionsStates.active) && !string.IsNullOrEmpty(preparedtransaction.Reservationtoken))
            {
                await NftReservationClass.ReleaseAllNftsAsync(db, _redis, preparedtransaction.Reservationtoken);
                await GlobalFunctions.ExecuteSqlWithFallbackAsync(db,
                    $"delete from preparedpaymenttransactions_nfts where preparedpaymenttransactions_id={preparedtransaction.Id}");
                await GlobalFunctions.ExecuteSqlWithFallbackAsync(db,
                    $"delete from preparedpaymenttransactions_smartcontractsjsons where preparedpaymenttransactions_id={preparedtransaction.Id}");

                preparedtransaction.State = nameof(PaymentTransactionsStates.prepared);
                await db.SaveChangesAsync();
            }


            string guid = GlobalFunctions.GetGuid();
            List<Nftreservation> selectedreservations = new();
                var mints = new List<Token>();

                if (preparedtransaction.Transactiontype == nameof(PaymentTransactionTypes.decentral_mintandsend_random))
                {
                    selectedreservations =
                        await NftReservationClass.ReserveRandomNft(db, _redis, guid, preparedtransaction.NftprojectId,
                            preparedtransaction.Countnft??1, preparedtransaction.Nftproject.Expiretime, false,
                            true, Coin.ADA);

                    if (!selectedreservations.Any() ||
                        selectedreservations.Sum(x => x.Tc) != preparedtransaction.Countnft)
                    {
                        result.ErrorCode = 1801;
                        result.ErrorMessage = "NFT Reservation not possible. No more NFTs available?";
                        result.ResultState = ResultStates.Error;
                        preparedtransaction.State = nameof(PaymentTransactionsStates.error);
                        preparedtransaction.Logfile = result.ErrorMessage + Environment.NewLine;
                        await db.SaveChangesAsync();
                        await NftReservationClass.ReleaseAllNftsAsync(db, _redis, guid);
                        return StatusCode(404, result);
                    }

                    // TODO: Check for Sale conditions


                    // Write the selected NFTS to the database
                    foreach (var nftreservation in selectedreservations)
                    {
                        var nft = await (from a in db.Nfts
                            where a.Id == nftreservation.NftId
                            select a).FirstOrDefaultAsync();
                        if (nft == null)
                        {
                            result.ErrorCode = 1802;
                            result.ErrorMessage = "NFT Reservation not possible. No more NFTs available?";
                            result.ResultState = ResultStates.Error;
                            await NftReservationClass.ReleaseAllNftsAsync(db, _redis, guid);
                            return StatusCode(404, result);
                        }

                    await db.PreparedpaymenttransactionsNfts.AddAsync(new()
                    {
                        Count = nftreservation.Tc,
                        NftId = nftreservation.NftId,
                        PreparedpaymenttransactionsId = preparedtransaction.Id,
                        Lovelace = preparedtransaction.Lovelace,
                        Policyid = preparedtransaction.Nftproject.Policyid,
                        Tokenname = (preparedtransaction.Nftproject.Tokennameprefix ?? "") + nft.Name,
                        Tokennamehex = GlobalFunctions.ToHexString((preparedtransaction.Nftproject.Tokennameprefix ?? "") + nft.Name),
                        Nftuid = nft.Uid
                    });

                    string metadata = "";
                    if (preparedtransaction.Nftproject.Cip68)
                    {
                        GetMetadataClass gmcx = new GetMetadataClass(nft.Id, preparedtransaction.Buyeraddress, true, db);
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
                        TokenName = GlobalFunctions.ToHexString((preparedtransaction.Nftproject.Tokennameprefix ?? "") + nft.Name),
                        PolicyScriptJson = preparedtransaction.Nftproject.Policyscript,
                        Metadata = metadata,
                    });
                }
                }

            if (preparedtransaction.Transactiontype ==
                nameof(PaymentTransactionTypes.decentral_mintandsend_specific))
            {
                var nfts = (from a in preparedtransaction.PreparedpaymenttransactionsNfts
                            select new ReserveNftsClass() { Multiplier = 1, NftId = (int)a.NftId, Tokencount = a.Count })
                    .ToArray();

                selectedreservations =
                    await NftReservationClass.ReserveSpecificNft(db, _redis, guid, preparedtransaction.NftprojectId, nfts,
                        preparedtransaction.Nftproject.Expiretime, false, true, Coin.ADA);

                if (selectedreservations.Count != nfts.Length)
                {
                    result.ErrorCode = 1153;
                    result.ErrorMessage = "NFT Reservation not possible. NFT not available";
                    result.ResultState = ResultStates.Error;
                    preparedtransaction.State = nameof(PaymentTransactionsStates.error);
                    preparedtransaction.Logfile = result.ErrorMessage + Environment.NewLine;
                    await db.SaveChangesAsync();
                    await NftReservationClass.ReleaseAllNftsAsync(db, _redis, guid);

                    return StatusCode(404, result);
                }

                foreach (var nftreservation in selectedreservations)
                {
                    var nft = await (from a in db.Nfts
                                     where a.Id == nftreservation.NftId
                                     select a).FirstOrDefaultAsync();
                    if (nft == null)
                    {
                        result.ErrorCode = 1803;
                        result.ErrorMessage = "NFT Reservation not possible. No more NFTs available?";
                        result.ResultState = ResultStates.Error;
                        await NftReservationClass.ReleaseAllNftsAsync(db, _redis, guid);
                        return StatusCode(404, result);
                    }

                    string metadata = "";
                    if (preparedtransaction.Nftproject.Cip68)
                    {
                        GetMetadataClass gmcx = new GetMetadataClass(nft.Id, preparedtransaction.Buyeraddress, true, db);
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
                        TokenName = GlobalFunctions.ToHexString(preparedtransaction.Nftproject.Tokennameprefix +
                                                                    nft.Name),
                        PolicyScriptJson = preparedtransaction.Nftproject.Policyscript,
                        Metadata = metadata,
                    });
                }
            }


            // Create the Transaction

            preparedtransaction.Expires = DateTime.Now.AddMinutes(preparedtransaction.Nftproject.Expiretime);
            preparedtransaction.Reservationtoken = guid;
            preparedtransaction.State = nameof(PaymentTransactionsStates.active);
            preparedtransaction.Buyeraddress = signDecentralClass.Buyer.ChangeAddress;
            preparedtransaction.Buyeraddresses= string.Join(Environment.NewLine, signDecentralClass.Buyer.Addresses.Select(x => x.Address).ToArray());
            preparedtransaction.Changeaddress = signDecentralClass.Buyer.ChangeAddress;
            preparedtransaction.Buyerpkh = signDecentralClass.Buyer.Pkh;
            await db.SaveChangesAsync();

            var mintingcosts = GlobalFunctions
                .GetMintingcosts2(preparedtransaction.NftprojectId, preparedtransaction.PreparedpaymenttransactionsNfts.Count, preparedtransaction.Lovelace ?? 0)
                .Costs;

            long networkfees = preparedtransaction.Nftproject.Settings.Minfees;
            /*   var utxofinal = StaticTransactionFunctions.GetAllNeededTxin(signDecentralClass.Buyer.Addresses,
                   (long)(preparedtransaction.Lovelace ?? 0) + StaticTransactionFunctions.GetMinUtxo(db, signDecentralClass.Buyer.ChangeAddress, preparedtransaction.NftprojectId, preparedtransaction.Id) + mintingfees + networkfees,
                   0, null, signDecentralClass.Buyer.CollateralTxIn, out string errormessage);
            */
            var utxofinal = StaticTransactionFunctions.GetAllNeededTxin(_redis,signDecentralClass.Buyer.Addresses,
                (long)(preparedtransaction.Lovelace ?? 0), // + mintingcosts + networkfees,
                0, null, signDecentralClass.Buyer.CollateralTxIn, out string errormessage, out AllTxInAddressesClass alltxin);

            if (utxofinal == null)
            {
                await NftReservationClass.ReleaseAllNftsAsync(db, _redis, guid);
                result.ErrorCode = 1138;
                result.ErrorMessage = "Too less ADA in the Wallet";
                result.ResultState = ResultStates.Error;
                await db.SaveChangesAsync();

                await GlobalFunctions.LogMessageAsync(db, $"GetAllNeddedTxIn - Error: {errormessage} - SignDecentralMintAndSend "+ preparedtransaction.Transactionuid,
                    JsonConvert.SerializeObject(signDecentralClass.Buyer.Addresses) + Environment.NewLine +
                    Environment.NewLine + JsonConvert.SerializeObject(alltxin) + Environment.NewLine +
                    Environment.NewLine +
                    ((long)(preparedtransaction.Lovelace ?? 0) + networkfees ) + Environment.NewLine +
                    Environment.NewLine +
                    signDecentralClass.Buyer.CollateralTxIn + Environment.NewLine + Environment.NewLine +  "Transaction UID:+" + preparedtransaction.Transactionuid);

                return StatusCode(406, result);
            }

            StaticTransactionFunctions.SaveRecentlyUsedTxHashes(_redis, utxofinal);

            await GlobalFunctions.LogMessageAsync(db, $"GetAllNeddedTxIn - Successful",
                JsonConvert.SerializeObject(alltxin));


            string matxrawfile = GeneralConfigurationClass.TempFilePath + "matx" + guid + ".raw";
            string protocolparamsfile = GeneralConfigurationClass.TempFilePath + "protocol" + guid + ".params";
           
            GetMetadataClass gmc = new((from a in selectedreservations select new NftIdWithMintingAddressClass(a.NftId, preparedtransaction.Buyeraddress)).ToArray());//, preparedtransaction.Nftproject.Maxsupply != 1);
            BuildTransactionClass bt = new();

            var qt = ConsoleCommand.GetQueryTip();
            long ttl = (long)qt.Slot + 6000;

            string selleraddress = preparedtransaction.Selleraddress;
            if (preparedtransaction.Selleraddress == null)
            {
                // TODO: Set internal wallet
                // selleraddress = preparedtransaction.Nftproject.Customerwallet.Walletaddress; // Null
            }

            PromotionClass promotion = null;
            if (preparedtransaction.PromotionId != null)
            {
                promotion = await GlobalFunctions.GetPromotionAsync(db,_redis, (int)preparedtransaction.PromotionId, preparedtransaction.Promotionmultiplier??1);
            }

            CreateMintAndSendParametersClass cmaspc = new()
            {
                utxofinal = utxofinal,
                BuyerChangeAddress = signDecentralClass.Buyer.ChangeAddress,
                OptionalReceiverAddress = preparedtransaction.Optionalreceiveraddress,
                BuyerHasToPayInLovelace = 0,
                project = preparedtransaction.Nftproject,
                selectedreservations = selectedreservations,
                MetadataResult = (await gmc.MetadataResultAsync()).Metadata,
                MinUtxo = GlobalFunctions.CalculateMinutxoForBuyer(preparedtransaction.Nftproject, selectedreservations.Count, (long)(preparedtransaction.Lovelace ?? 0)),
                Mintingcosts = GlobalFunctions.GetMintingcosts2(preparedtransaction.NftprojectId, selectedreservations.Count,0).Costs,
                MintingcostsAddress = preparedtransaction.Nftproject.Settings.Mintingaddress,
                // AdditionalPayouts= nftproject.Nftprojectsadditionalpayouts.ToArray(),
              //  MintTokensString = ConsoleCommand.GetSendTokensString(db, selectedreservations, preparedtransaction.Nftproject),
                MintTokens = mints.ToArray(),
                ttl = ttl,
                SellerAddress = selleraddress,
                Promotion = promotion,
                Fees = 1 // Calculate Fee
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
                        await NftReservationClass.ReleaseAllNftsAsync(db, _redis, guid);
                        result.ErrorCode = 1145;
                        result.ErrorMessage = "Error while creating the Burning address";
                        result.ResultState = ResultStates.Error;
                        preparedtransaction.State = nameof(PaymentTransactionsStates.error);
                        preparedtransaction.Logfile = result.ErrorMessage + Environment.NewLine + result.InnerErrorMessage;
                        await db.SaveChangesAsync();
                        StaticTransactionFunctions.DeleteRecentlyUsedTxHashes(_redis, utxofinal);
                        return StatusCode(500, result);
                    }
                    cmaspc.Burningaddress = r.Address;
                }
            }




            var cborNoFee = await ConsoleCommand.CreateDecentralPaymentByCsl(cmaspc,db,_redis, GlobalFunctions.IsMainnet());
            if (cborNoFee==null || string.IsNullOrEmpty(cborNoFee.CslResult))
            {
                await NftReservationClass.ReleaseAllNftsAsync(db, _redis, guid);
                result.ErrorCode = 1131;
                result.ErrorMessage = "Error while creating the Transaction (1)";
                result.ResultState = ResultStates.Error;
                if (cborNoFee != null) result.InnerErrorMessage = cborNoFee.CreatedJson ?? "";
                preparedtransaction.State = nameof(PaymentTransactionsStates.error);
                preparedtransaction.Logfile=(cborNoFee?.CslError??"") +Environment.NewLine+ result.ErrorMessage + Environment.NewLine + result.InnerErrorMessage;
                await db.SaveChangesAsync();
                StaticTransactionFunctions.DeleteRecentlyUsedTxHashes(_redis, utxofinal);
                return StatusCode(500, result);
            }

            // Calculate the Min. Fees
            var matxrawtext = ConsoleCommand.GetCborJson(cborNoFee.CslResult);
            await System.IO.File.WriteAllTextAsync(matxrawfile, matxrawtext);
            ConsoleCommand.SaveProtocolParamsFile(_redis,protocolparamsfile, GlobalFunctions.IsMainnet(), ref bt);
            int txoutcount = Math.Max(3, cmaspc.AdditionalPayouts != null ? cmaspc.AdditionalPayouts.Length + 3 : 3);
            int witnesscount = Math.Max(2, cmaspc.utxofinal.Length + 2);

            ConsoleCommand.CalculateFees(matxrawfile, utxofinal.Length, txoutcount, witnesscount, GlobalFunctions.IsMainnet(),
                protocolparamsfile, ref bt, out var fee);

            if (fee == 0)
            {
                await NftReservationClass.ReleaseAllNftsAsync(db, _redis, guid);
                result.ErrorCode = 1139;
                result.ErrorMessage = "Error while calculating the fee";
                result.ResultState = ResultStates.Error;
                if (cborNoFee != null) result.InnerErrorMessage = bt.ErrorMessage + Environment.NewLine + bt.LogFile;
                preparedtransaction.State = nameof(PaymentTransactionsStates.error);
                preparedtransaction.Logfile = result.ErrorMessage + Environment.NewLine + result.InnerErrorMessage;
                await db.SaveChangesAsync();
                StaticTransactionFunctions.DeleteRecentlyUsedTxHashes(_redis, utxofinal);
                return StatusCode(500, result);
            }

            // Create CBOR incl. Fee and Metadata
            cmaspc.Fees = fee;
            cmaspc.IncludeMetadataHashOnly = false;
            var cborFeeAndMetadata = await ConsoleCommand.CreateDecentralPaymentByCsl(cmaspc,db,_redis, GlobalFunctions.IsMainnet());


            // Create CBOR incl. Fee without Metadata
            cmaspc.Fees = fee;
            cmaspc.IncludeMetadataHashOnly = true;
            var cborFeeNoMetadata = await ConsoleCommand.CreateDecentralPaymentByCsl(cmaspc,db, _redis,GlobalFunctions.IsMainnet());

           
            if (cborFeeNoMetadata == null || cborFeeAndMetadata == null)
            {
                await NftReservationClass.ReleaseAllNftsAsync(db, _redis, guid);
                result.ErrorCode = 1132;
                result.ErrorMessage = "Error while creating the Transaction (2)";
                result.ResultState = ResultStates.Error;
                if (cborFeeNoMetadata != null) result.InnerErrorMessage = cborFeeNoMetadata.CreatedJson ?? "";
                preparedtransaction.State = nameof(PaymentTransactionsStates.error);
                preparedtransaction.Logfile = (cborFeeNoMetadata?.CslError ?? "") + result.ErrorMessage + Environment.NewLine + result.InnerErrorMessage;
                await db.SaveChangesAsync();
                StaticTransactionFunctions.DeleteRecentlyUsedTxHashes(_redis, utxofinal);
                return StatusCode(500, result);
            }

            var matxrawtextFeeNoMetadata = ConsoleCommand.GetCborJson(cborFeeNoMetadata.CslResult);
            var matxrawtextFeeAndMetadata = ConsoleCommand.GetCborJson(cborFeeAndMetadata.CslResult);

            await GlobalFunctions.LogMessageAsync(db, "Api: CBOR " + preparedtransaction.Transactionuid, cborFeeAndMetadata.CslResult);


            PreparedpaymenttransactionsSmartcontractsjson ptsj = new()
            {
                PreparedpaymenttransactionsId = preparedtransaction.Id,
                Json = "",
                Redeemer = "",
                Templatetype = "mintandsend",
                Signedcbr = matxrawtextFeeNoMetadata,
                Rawtx = matxrawtextFeeAndMetadata,
                Logfile = bt.LogFile,
                Fee = bt.Fees,
                Hash = "",
                Signinguid = "M" + guid,
                Signedandsubmitted = false,
                Created = DateTime.Now,
            };

            db.PreparedpaymenttransactionsSmartcontractsjsons.Add(ptsj);
            preparedtransaction.Smartcontractstate = nameof(PaymentTransactionSubstates.readytosignbybuyer);
            preparedtransaction.Fee = bt.Fees;
            preparedtransaction.Logfile += Environment.NewLine + 
                                           cborNoFee.CreatedJson + Environment.NewLine +
                                           cborFeeAndMetadata.CreatedJson + Environment.NewLine +
                                           cborFeeNoMetadata.CreatedJson;
            await db.SaveChangesAsync();

         //   GlobalFunctions.DeleteFile(metadatajsonfile);
        //    GlobalFunctions.DeleteFile(policyscriptfile);
            GlobalFunctions.DeleteFile(matxrawfile);

            var res = StaticTransactionFunctions.GetTransactionState(db, _redis, preparedtransaction.Transactionuid,
                true);

            await GlobalFunctions.LogMessageAsync(db, "SignDecentralMintAndSend - OK", JsonConvert.SerializeObject(res));
            return Ok(res);
        }

    }
}
