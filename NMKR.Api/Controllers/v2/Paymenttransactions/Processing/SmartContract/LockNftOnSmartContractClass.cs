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
    public class LockNftOnSmartContractClass : ControllerBase, IProcessPaymentTransactionInterface
    {
        private readonly IConnectionMultiplexer _redis;

        public LockNftOnSmartContractClass(IConnectionMultiplexer redis)
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
            SellerClass seller = null;
            try
            {
                seller = postparameter1 as SellerClass;
            }
            catch
            {
                result.ErrorCode = 1128;
                result.ErrorMessage = "Seller object is not correct";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            if (preparedtransaction.State == nameof(PaymentTransactionsStates.active) &&
                preparedtransaction.Smartcontractstate == nameof(PaymentTransactionSubstates.readytosignbyseller))
            {
              //  return Ok(StaticTransactionFunctions.GetTransactionState(db, preparedtransaction.Transactionuid, false));
            }
            else
            {
                if (preparedtransaction.State != nameof(PaymentTransactionsStates.prepared) &&
                    preparedtransaction.Smartcontractstate != nameof(PaymentTransactionSubstates.waitingforlocknft))
                {
                    result.ErrorCode = 1117;
                    result.ErrorMessage = "State of this Smartcontract does not fit to this command";
                    result.ResultState = ResultStates.Error;
                    return StatusCode(406, result);
                }
            }

            if (seller==null)
            {
                result.ErrorCode = 1129;
                result.ErrorMessage = "Seller object is null";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }
            if (seller.Seller == null)
            {
                result.ErrorCode = 1129;
                result.ErrorMessage = "Seller object is not correct";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }
            if (string.IsNullOrEmpty(seller.Seller.ChangeAddress))
            {
                result.ErrorCode = 1132;
                result.ErrorMessage = "Submit seller change address";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }
            if (seller.Seller.Addresses==null || !seller.Seller.Addresses.Any())
            {
                result.ErrorCode = 1133;
                result.ErrorMessage = "Submit seller address(es)";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            string tokenname = preparedtransaction.Policyid.ToLower() + "." + preparedtransaction.Tokenname.ToLower();

            BuildTransactionClass bt = new();
            string guid = GlobalFunctions.GetGuid();
            string matxrawfile = GeneralConfigurationClass.TempFilePath + "matx" + guid + ".raw";
            string protocolParamsFile = GeneralConfigurationClass.TempFilePath + "protocol" + guid + ".params";

            
            preparedtransaction.Selleraddresses = String.Join("/", seller.Seller.Addresses.Select(x => x.Address).ToArray()); // This are all addresses from the seller - if he have more than one - to find the right tx in for the token
            preparedtransaction.Selleraddress = seller.Seller.ChangeAddress;
            preparedtransaction.Changeaddress = seller.Seller.ChangeAddress;
            preparedtransaction.Sellerpkh = GlobalFunctions.GetPkhFromAddress(seller.Seller.ChangeAddress);

            await db.SaveChangesAsync();

            // Just for testing
            seller.Seller.CollateralTxIn = "";


            var utxofinal = StaticTransactionFunctions.GetAllNeededTxin(_redis,preparedtransaction.Selleraddresses.Split("/"),
                4500000, (long)(preparedtransaction.Tokencount ?? 1), tokenname, seller.Seller.CollateralTxIn, out string errormessage);
            if (utxofinal == null)
            {
                result.ErrorCode = 1105;
                result.ErrorMessage = errormessage;
                result.ResultState = ResultStates.Error;
                return StatusCode(404, result);
            }


            PreparedpaymenttransactionsSmartcontractsjson ptsj = new()
            {
                PreparedpaymenttransactionsId = preparedtransaction.Id,
                Templatetype = nameof(DatumTemplateTypes.locknft),
            };

            switch (preparedtransaction.Transactiontype)
            {
                    case nameof(PaymentTransactionTypes.smartcontract_auction):
                        ptsj.Json = StaticTransactionFunctions.FillJsonTemplateSellerAuction(db,
                            preparedtransaction);
                    break;
                    case nameof(PaymentTransactionTypes.smartcontract_directsale):
                        ptsj.Json = await StaticTransactionFunctions.FillJsonTemplateSellerDirectsaleAsync(db,
                            preparedtransaction);
                        break;
            }
            // not needed any longer - when we embed the json
            ptsj.Hash = StaticTransactionFunctions.GetHash(ptsj.Json);


            if (ptsj.Hash == "")
            {
                result.ErrorCode = 1021;
                result.ErrorMessage = "Hash for Directsale script could not generated. Please contact support.";
                result.ResultState = ResultStates.Error;
                return StatusCode(500, result);
            }

            db.PreparedpaymenttransactionsSmartcontractsjsons.Add(ptsj);
            await db.SaveChangesAsync();

            ConsoleCommand.GenerateProtocolParamsFile(protocolParamsFile, _redis,GlobalFunctions.IsMainnet(), out errormessage);

            string metadatafile = ConsoleCommand.GetMetadatafileForDatum(ptsj.Json, GeneralConfigurationClass.TempFilePath + "datumcbor" + guid + ".json");

           string scriptdatumfile = GeneralConfigurationClass.TempFilePath + "datum" + guid + ".json";
            await System.IO.File.WriteAllTextAsync(scriptdatumfile, ptsj.Json);



           bt.SenderAddress = preparedtransaction.Changeaddress;

            var ok = ConsoleCommand.CreateCliCommandLockTransactionSmartcontract(_redis, utxofinal, preparedtransaction.Changeaddress,
                preparedtransaction.Tokencount ?? 1,
                tokenname, preparedtransaction.Smartcontracts.Address,
                ptsj.Hash, scriptdatumfile,
                protocolParamsFile, matxrawfile,metadatafile, GlobalFunctions.IsMainnet(), ref bt);


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

                preparedtransaction.Smartcontractstate = nameof(PaymentTransactionSubstates.readytosignbyseller);
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
