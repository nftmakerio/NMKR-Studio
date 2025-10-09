using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NMKR.Api.Controllers.SharedClasses;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.SaleConditions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.Paymenttransactions.Processing.Decentral
{
    /// <summary>
    /// Submits a multi-sig (decentral) Transaction
    /// </summary>
    public class SubmitDecentralPaymentTransactionClass : ControllerBase, IProcessPaymentTransactionInterface
    {
        private readonly IConnectionMultiplexer _redis;


        /// <summary>
        /// Reserves Nfts for Mint and Send
        /// </summary>
        /// <param name="redis"></param>
        public SubmitDecentralPaymentTransactionClass(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Submits a multi-sig (decentral) Transaction
        /// </summary>
        /// <param name="db"></param>
        /// <param name="apikey"></param>
        /// <param name="remoteipaddress"></param>
        /// <param name="result"></param>
        /// <param name="preparedtransaction"></param>
        /// <param name="postparameter1"></param>
        /// <returns></returns>
        public async Task<IActionResult> ProcessTransaction(EasynftprojectsContext db, string apikey,
            string remoteipaddress, ApiErrorResultClass result,
            Preparedpaymenttransaction preparedtransaction, object postparameter1)
        {
            var postparameter = postparameter1 as SubmitTransactionClass;
            await GlobalFunctions.LogMessageAsync(db, "API: SubmitDecentralTransaction", JsonConvert.SerializeObject(postparameter));
            if (preparedtransaction.Smartcontractstate != nameof(PaymentTransactionSubstates.readytosignbybuyer))
            {
                result.ErrorCode = 1317;
                result.ErrorMessage = "Transaction is not in the state of signing";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            if (postparameter == null || string.IsNullOrEmpty(postparameter.SignedCbor))
            {
                result.ErrorCode = 1105;
                result.ErrorMessage = "Cbor is empty";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            string guid = GlobalFunctions.GetGuid();

            // Take the "unsigned" file 
            var origtx = preparedtransaction.PreparedpaymenttransactionsSmartcontractsjsons.LastOrDefault();
            if (origtx == null)
            {
                result.ErrorCode = 1401;
                result.ErrorMessage = "Transaction can not signed (1)";
                result.ResultState = ResultStates.Error;
                return StatusCode(500, result);
            }



            // Before submitting - check the payment conditions
            if (preparedtransaction.Transactiontype == nameof(PaymentTransactionTypes.decentral_mintandsale_random) ||
                preparedtransaction.Transactiontype == nameof(PaymentTransactionTypes.decentral_mintandsale_specific))
            {

                long cx = preparedtransaction.Countnft ?? 1;
                if (preparedtransaction.Nftproject.Maxsupply > 1)
                    cx = preparedtransaction.PreparedpaymenttransactionsNfts.Sum(x => x.Count) *
                         Math.Max(1, preparedtransaction.Nftproject.Multiplier);


                // Check Sale Conditions
                var cond = await CheckSalesConditionClass.CheckForSaleConditionsMet(db,_redis, preparedtransaction.NftprojectId,
                    preparedtransaction.Changeaddress, cx, 0, preparedtransaction.Nftproject.Usefrankenprotection, Blockchain.Cardano);

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

                    return StatusCode(412, result1); // Status412PreconditionFailed
                }

                // Check if the NFT is still in reserved state - and not already sold - to prevent double mints

                var nftreservation = await (from a in db.Nftreservations
                    where a.Reservationtoken == preparedtransaction.Reservationtoken
                    select a).AsNoTracking().ToListAsync();

                if (!nftreservation.Any())
                {
                    var result1 = new RejectedErrorResultClass
                    {
                        ErrorCode = 1301,
                        ErrorMessage = "Nfts are not longer reservered",
                        ResultState = ResultStates.Error,
                        RejectParameter = cond.RejectParameter,
                        RejectReason = cond.RejectReason
                    };
                    return StatusCode(406, result1); 
                }

                if (preparedtransaction.Nftproject.Maxsupply == 1) 
                {
                    foreach (var nftreservation1 in nftreservation)
                    {
                        var nft = await (from a in db.Nfts
                                .Include(x=>x.Nftproject)
                            where a.Id == nftreservation1.NftId
                            select a).AsNoTracking().FirstOrDefaultAsync();
                        if (nft == null)
                        {
                            var result1 = new RejectedErrorResultClass
                            {
                                ErrorCode = 1302,
                                ErrorMessage = "Nfts are not longer available",
                                ResultState = ResultStates.Error,
                                RejectParameter = cond.RejectParameter,
                                RejectReason = cond.RejectReason
                            };
                            return StatusCode(406, result1);
                        }

                        if (nft.State != "reserved")
                        {
                            var result1 = new RejectedErrorResultClass
                            {
                                ErrorCode = 1303,
                                ErrorMessage = "Nfts are not longer reservered",
                                ResultState = ResultStates.Error,
                                RejectParameter = cond.RejectParameter,
                                RejectReason = cond.RejectReason
                            };
                            return StatusCode(406, result1);
                        }


                        var as1 = await ConsoleCommand.GetAssetFromBlockchainAsync(nft,nft.Nftproject);
                        // Check if the NFT is not already minted

                        if (as1!=null)
                        {
                            var result1 = new RejectedErrorResultClass
                            {
                                ErrorCode = 1304,
                                ErrorMessage = "Nfts are already minted",
                                ResultState = ResultStates.Error,
                                RejectParameter = cond.RejectParameter,
                                RejectReason = cond.RejectReason
                            };
                            return StatusCode(406, result1);
                        }
                    }
                }
            }




            string txbodyfilenamewithmetdata =
                GeneralConfigurationClass.TempFilePath + "txbody_withmetadata_" + guid + ".raw";
            string txbodyfilenamewithoutmetdata =
                GeneralConfigurationClass.TempFilePath + "txbody_withoutmetadata_" + guid + ".raw";
            string policyskey = GeneralConfigurationClass.TempFilePath + "policy_" + guid + ".skey";
            string witnessfileproject = GeneralConfigurationClass.TempFilePath + "witness_project_" + guid + ".signed";
            string witnessfilepromotion =
                GeneralConfigurationClass.TempFilePath + "witness_promotion_" + guid + ".signed";
            string finalassemblesfile = GeneralConfigurationClass.TempFilePath + "assembled_" + guid + ".signed";
            string promotionpolicyfile = GeneralConfigurationClass.TempFilePath + "promotionpolicy_" + guid + ".skey";
            BuildTransactionClass bt = new();

            bt.LogFile+=JsonConvert.SerializeObject(postparameter,Formatting.Indented, new JsonSerializerSettings(){ReferenceLoopHandling = ReferenceLoopHandling.Ignore })+Environment.NewLine;
            bt.LogFile += "Transactiontype: " + preparedtransaction.Transactiontype + Environment.NewLine;
            // Write original Raw TW (with metadata)
            await System.IO.File.WriteAllTextAsync(txbodyfilenamewithmetdata, origtx.Rawtx);

            // Write the cbor without the metadata (only hash)
            await System.IO.File.WriteAllTextAsync(txbodyfilenamewithoutmetdata, origtx.Signedcbr);

            // Write Policykey
            string polskey = Encryption.DecryptString(preparedtransaction.Nftproject.Policyskey,
                preparedtransaction.Nftproject.Password);
            await System.IO.File.WriteAllTextAsync(policyskey, polskey);


            // Promotion
            PromotionClass promotion = null;
            if (preparedtransaction.PromotionId != null)
            {
                promotion = await GlobalFunctions.GetPromotionAsync(db,_redis, (int) preparedtransaction.PromotionId,
                    preparedtransaction.Promotionmultiplier ?? 1);
                if (promotion != null)
                    await System.IO.File.WriteAllTextAsync(promotionpolicyfile, promotion.SKey);
            }


            // Sign with policy file
            bool ok = ConsoleCommand.SignWitness(txbodyfilenamewithmetdata, witnessfileproject, policyskey, ref bt);
            if (!ok)
            {
                result.ErrorCode = 1402;
                result.ErrorMessage = "Transaction can not signed (2)";
                result.ResultState = ResultStates.Error;
                return StatusCode(500, result);
            }

            if (promotion != null)
            {
                bool ok2 = ConsoleCommand.SignWitness(txbodyfilenamewithmetdata, witnessfilepromotion,
                    promotionpolicyfile, ref bt);
                if (!ok2)
                {
                    result.ErrorCode = 1404;
                    result.ErrorMessage = "Transaction can not signed (3)";
                    result.ResultState = ResultStates.Error;
                    return StatusCode(500, result);
                }
            }


            string[] witnesses = postparameter.SignedCbor.Split(',');
            List<string> witnessfiles = new();
            witnessfiles.Add(witnessfileproject);
            int i = 0;
            foreach (var s in witnesses)
            {
                i++;
                string witnessfilewallet = GeneralConfigurationClass.TempFilePath + $"witness_wallet_{i}_{guid}.signed";
                // Save File from user
                MatxRawClass mrc = new()
                    {CborHex = s, Description = "Key Witness ShelleyEra", Type = "TxWitness ConwayEra" };
                await System.IO.File.WriteAllTextAsync(witnessfilewallet, JsonConvert.SerializeObject(mrc));
                witnessfiles.Add(witnessfilewallet);
            }

            if (promotion != null)
            {
                witnessfiles.Add(witnessfilepromotion);
            }


            // Assemble the file
            ok = ConsoleCommand.AssembleFiles(txbodyfilenamewithmetdata, finalassemblesfile, witnessfiles.ToArray(),
                ref bt);
            if (!ok)
            {
                result.ErrorCode = 1403;
                result.ErrorMessage = "Transaction can not be assembled";
                result.ResultState = ResultStates.Error;
                preparedtransaction.Logfile = result.ErrorMessage + Environment.NewLine + result.InnerErrorMessage;
                preparedtransaction.State = nameof(PaymentTransactionsStates.error);
                preparedtransaction.Signedcbor = "";
                preparedtransaction.Cbor = "";
                await db.SaveChangesAsync();
                await NftReservationClass.ReleaseAllNftsAsync(db, _redis, preparedtransaction.Reservationtoken);
                await GlobalFunctions.LogMessageAsync(db,
                    "Multisig Transaction can not be assembled " + bt.ErrorMessage, bt.LogFile);
                await NftReservationClass.SetLogfileToNfts(db, preparedtransaction.Reservationtoken, bt.LogFile);
                await NftReservationClass.ReleaseAllNftsAsync(db, _redis, preparedtransaction.Reservationtoken);
                return StatusCode(500, result);
            }

            bt.LogFile += Environment.NewLine + preparedtransaction.Logfile;

            var submissionresult= await ConsoleCommand.SubmitTransactionWithFallbackAsync(finalassemblesfile, bt);
            bt = submissionresult.Buildtransaction;
            if (submissionresult.Success)
            {
                bt.TxHash = submissionresult.TxHash;
            }


            if (string.IsNullOrEmpty(bt.TxHash))
            {

                result.ErrorCode = 1119;
                result.ErrorMessage = "Transaction could not be submitted. See innerErrorMessage for more details";
                result.InnerErrorMessage = preparedtransaction.Logfile + Environment.NewLine + bt.LogFile + Environment.NewLine + submissionresult.ErrorMessage;
                result.ResultState = ResultStates.Error;
                preparedtransaction.Logfile = result.ErrorMessage + Environment.NewLine + result.InnerErrorMessage;
                preparedtransaction.State = nameof(PaymentTransactionsStates.error);
                preparedtransaction.Signedcbor = "";
                preparedtransaction.Cbor = "";
              
                await db.SaveChangesAsync();
                await GlobalFunctions.LogMessageAsync(db, "Submit failed "+bt.ErrorMessage, bt.LogFile);
                await NftReservationClass.SetLogfileToNfts(db, preparedtransaction.Reservationtoken, bt.LogFile);
                await NftReservationClass.ReleaseAllNftsAsync(db, _redis, preparedtransaction.Reservationtoken);
                return StatusCode(500, result);
            }

            preparedtransaction.Signedcbor= ConsoleCommand.GetCbor(await System.IO.File.ReadAllTextAsync(finalassemblesfile));
            preparedtransaction.Cbor = "";

            if (promotion != null)
            {
                await GlobalFunctions.SetPromotionSoldcountAsync(db, (int) preparedtransaction.PromotionId,
                    promotion.Tokencount);
            }

         
            preparedtransaction.Paymentgatewaystate = nameof(PaymentGatewayStates.signedbybuyer);
            preparedtransaction.Logfile += bt.LogFile;
            /*  ok = ConsoleCommand.GetTxId(finalassemblesfile, ref bt);*/

            await NftReservationClass.MarkAllNftsAsSold(db,  preparedtransaction.Reservationtoken, preparedtransaction.Nftproject.Cip68, preparedtransaction.Buyeraddress);
            preparedtransaction.State = nameof(PaymentTransactionsStates.finished);
            preparedtransaction.Txhash = bt.TxHash;
            // Set substate to submitted - then we can create the transaction later, when the tx is confirmed
            preparedtransaction.Smartcontractstate = nameof(PaymentTransactionSubstates.submitted);
            preparedtransaction.Submitteddate = DateTime.Now;

            if (!string.IsNullOrEmpty(preparedtransaction.Createroyaltytokenaddress))
            {
                preparedtransaction.Nftproject.Hasroyality = true;
            }


            await db.SaveChangesAsync();

            string signedcbor = await System.IO.File.ReadAllTextAsync(finalassemblesfile);

            GlobalFunctions.DeleteFile(txbodyfilenamewithmetdata);
            GlobalFunctions.DeleteFile(witnessfileproject);
            GlobalFunctions.DeleteFile(policyskey);
            GlobalFunctions.DeleteFile(finalassemblesfile);


            if (string.IsNullOrEmpty(RedisFunctions.GetStringData(_redis, $"TXtoDBSaved_{bt.TxHash}", false)))
            {
                // Save the Transaction to the Transactions Database
                await SaveTransactionClass.SaveTransactionToDatabase(db,_redis, preparedtransaction, promotion, signedcbor);
                await GlobalFunctions.LogMessageAsync(db, "Saved transaction to database",
                    Environment.NewLine + bt.LogFile);

                // Save the Whitelist (if there was one)
                long c = preparedtransaction.Countnft ?? 1;
                if (preparedtransaction.Nftproject.Maxsupply > 1)
                    c = preparedtransaction.PreparedpaymenttransactionsNfts.Sum(x => x.Count) *
                        Math.Max(1, preparedtransaction.Nftproject.Multiplier);


                await WhitelistFunctions.SaveUsedAddressesToWhitelistSaleCondition(db, preparedtransaction.NftprojectId,
                    preparedtransaction.Changeaddress, preparedtransaction.Changeaddress,
                    Bech32Engine.GetStakeFromAddress(preparedtransaction.Changeaddress),
                    bt.TxHash, c);

                RedisFunctions.SetStringData(_redis, $"TXtoDBSaved_{bt.TxHash}", "Submitted",5000);
            }

            return Ok(StaticTransactionFunctions.GetTransactionState(db, _redis, preparedtransaction.Transactionuid, true));
        }

    }
}
