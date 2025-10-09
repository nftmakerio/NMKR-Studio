using NMKR.Api.Controllers.SharedClasses;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NMKR.Api.Controllers.v2.Paymenttransactions.Processing.SmartContract
{
    /// <summary>
    /// Locks a nft on a smartcontract (auction or directsale)
    /// </summary>
    public class LockAdaOnSmartContractClass : ControllerBase, IProcessPaymentTransactionInterface
    {
        private readonly IConnectionMultiplexer _redis;

        public LockAdaOnSmartContractClass(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }
        /// <summary>
        ///  Locks a nft on a smartcontract (auction or directsale)
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
            BuyerClass buyer = null;
            try
            {
                buyer = postparameter1 as BuyerClass;
            }
            catch
            {
                result.ErrorCode = 1128;
                result.ErrorMessage = "Buyer object is not correct";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            if (preparedtransaction.State == nameof(PaymentTransactionsStates.active) &&
                preparedtransaction.Smartcontractstate == nameof(PaymentTransactionSubstates.readytosignbybuyer))
            {
                //  return Ok(StaticTransactionFunctions.GetTransactionState(db, preparedtransaction.Transactionuid, false));
            }
            else
            {
                if (preparedtransaction.State != nameof(PaymentTransactionsStates.prepared) &&
                    preparedtransaction.Smartcontractstate != nameof(PaymentTransactionSubstates.waitingforlockada))
                {
                    result.ErrorCode = 1117;
                    result.ErrorMessage = "State of this Smartcontract does not fit to this command";
                    result.ResultState = ResultStates.Error;
                    return StatusCode(406, result);
                }
            }

            if (buyer == null)
            {
                result.ErrorCode = 1129;
                result.ErrorMessage = "Buyer object is null";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }
            if (buyer.Buyer == null)
            {
                result.ErrorCode = 1129;
                result.ErrorMessage = "Buyer object is not correct";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }
            if (string.IsNullOrEmpty(buyer.Buyer.ChangeAddress))
            {
                result.ErrorCode = 1132;
                result.ErrorMessage = "Submit buyer change address";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }
            if (buyer.Buyer.Addresses == null || !buyer.Buyer.Addresses.Any())
            {
                result.ErrorCode = 1133;
                result.ErrorMessage = "Submit buyer address(es)";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            string tokenname = preparedtransaction.Policyid.ToLower() + "." + preparedtransaction.Tokenname.ToLower();

            BuildTransactionClass bt = new();
            string guid = GlobalFunctions.GetGuid();
            string matxrawfile = GeneralConfigurationClass.TempFilePath + "matx" + guid + ".raw";
            string protocolParamsFile = GeneralConfigurationClass.TempFilePath + "protocol" + guid + ".params";


            preparedtransaction.Buyeraddresses = String.Join("/", buyer.Buyer.Addresses.Select(x => x.Address).ToArray()); // This are all addresses from the buyer - if he have more than one - to find the right tx in for the token
            preparedtransaction.Buyeraddress = buyer.Buyer.ChangeAddress;
            preparedtransaction.Changeaddress = buyer.Buyer.ChangeAddress;
            preparedtransaction.Buyerpkh = GlobalFunctions.GetPkhFromAddress(buyer.Buyer.ChangeAddress);

            await db.SaveChangesAsync();

            // Just for testing
            buyer.Buyer.CollateralTxIn = "";


            var utxofinal = StaticTransactionFunctions.GetAllNeededTxin(_redis, preparedtransaction.Buyeraddresses.Split("/"),
                preparedtransaction.Lovelace??0,0, null, buyer.Buyer.CollateralTxIn, out string errormessage);
            if (utxofinal == null)
            {
                result.ErrorCode = 1105;
                result.ErrorMessage = errormessage;
                result.ResultState = ResultStates.Error;
                return StatusCode(404, result);
            }

            
            // Fill Buyer Output
            await StaticTransactionFunctions.DeleteOutputs(db, preparedtransaction.Id, "buyer");
            // Add Buyer
               var buyerx = new PreparedpaymenttransactionsSmartcontractOutput()
               {
                   Address = preparedtransaction.Buyeraddress,
                   Pkh = preparedtransaction.Buyerpkh,
                   Lovelace = 2000000,
                   PreparedpaymenttransactionsId = preparedtransaction.Id,
                   Type = "buyer"
               };
               await db.PreparedpaymenttransactionsSmartcontractOutputs.AddAsync(buyerx);
               await db.SaveChangesAsync();
               // Add Buyer NFT
               await db.PreparedpaymenttransactionsSmartcontractOutputsAssets.AddAsync(
                   new PreparedpaymenttransactionsSmartcontractOutputsAsset()
                   {
                       Amount = preparedtransaction.Tokencount ?? 1,
                       Policyid = preparedtransaction.Policyid,
                       Tokennameinhex = preparedtransaction.Tokenname,
                       PreparedpaymenttransactionsSmartcontractOutputsId = buyerx.Id
                   });
               await db.SaveChangesAsync();
            // Fill Buyer Output End
            


            PreparedpaymenttransactionsSmartcontractsjson ptsj = new()
            {
                PreparedpaymenttransactionsId = preparedtransaction.Id,
                Templatetype = nameof(DatumTemplateTypes.lockada),
            };

            ptsj.Json = StaticTransactionFunctions.FillJsonTemplateBuyerDirectsaleOffer(preparedtransaction);

            // not needed any longer - when we embed the json
            ptsj.Hash = StaticTransactionFunctions.GetHash(ptsj.Json);


            if (ptsj.Hash == "")
            {
                result.ErrorCode = 1021;
                result.ErrorMessage = "Hash for Directsale offer script could not generated. Please contact support.";
                result.ResultState = ResultStates.Error;
                return StatusCode(500, result);
            }

            db.PreparedpaymenttransactionsSmartcontractsjsons.Add(ptsj);
            await db.SaveChangesAsync();

            ConsoleCommand.GenerateProtocolParamsFile(protocolParamsFile, _redis, GlobalFunctions.IsMainnet(), out errormessage);

            string metadatafile = ConsoleCommand.GetMetadatafileForDatum(ptsj.Json, GeneralConfigurationClass.TempFilePath + "datumcbor" + guid + ".json");

            string scriptdatumfile = GeneralConfigurationClass.TempFilePath + "datum" + guid + ".json";
            await System.IO.File.WriteAllTextAsync(scriptdatumfile, ptsj.Json);



            bt.SenderAddress = preparedtransaction.Changeaddress;

            var ok = ConsoleCommand.CreateCliCommandLockAdaTransactionSmartcontract(_redis, utxofinal, preparedtransaction.Changeaddress,
                preparedtransaction.Smartcontracts.Address,preparedtransaction.Lovelace??0,
                ptsj.Hash, scriptdatumfile,
                protocolParamsFile, matxrawfile, metadatafile, GlobalFunctions.IsMainnet(), ref bt);


            await GlobalFunctions.LogMessageAsync(db, "Smartcontract Build command",
                bt.Command + Environment.NewLine + bt.ErrorMessage);

            if (ok)
            {
                string raw = await System.IO.File.ReadAllTextAsync(matxrawfile);
                preparedtransaction.Estimatedfees = bt.Fees;

                ptsj.Rawtx = raw;
                ptsj.Created = DateTime.Now;
                ptsj.Fee = bt.Fees;
                ptsj.Logfile = bt.LogFile;
                ptsj.Signinguid = "L" + guid;

                ptsj.Signedcbr = raw;

                preparedtransaction.Smartcontractstate = nameof(PaymentTransactionSubstates.readytosignbybuyer);
                preparedtransaction.State = nameof(PaymentTransactionsStates.active);
                await db.SaveChangesAsync();
            }
            else
            {
                result.ErrorCode = 1118;
                result.ErrorMessage = "Can not create Cbor. Please contact support.";
                result.ResultState = ResultStates.Error;
                result.InnerErrorMessage = bt.LogFile;
                return StatusCode(500, result);
            }

            GlobalFunctions.DeleteFile(matxrawfile);
            GlobalFunctions.DeleteFile(protocolParamsFile);
            GlobalFunctions.DeleteFile(metadatafile);

            return Ok(StaticTransactionFunctions.GetTransactionState(db, _redis, preparedtransaction.Transactionuid, false));
        }


    }
}
