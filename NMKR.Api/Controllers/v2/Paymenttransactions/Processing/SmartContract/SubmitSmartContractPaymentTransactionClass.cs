using System;
using System.Linq;
using System.Threading.Tasks;
using NMKR.Api.Controllers.SharedClasses;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.Paymenttransactions.Processing.SmartContract
{
    /// <summary>
    /// Submits a Smartcontract Transaction (Directsale or Auction)
    /// </summary>
    public class SubmitSmartContractPaymentTransactionClass : ControllerBase, IProcessPaymentTransactionInterface
    {

        private readonly IConnectionMultiplexer _redis;

        public SubmitSmartContractPaymentTransactionClass(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        ///  Submits a Smartcontract Transaction (Directsale or Auction)
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
            var postparameter = postparameter1 as SubmitTransactionClass;
            await GlobalFunctions.LogMessageAsync(db, "API: SubmitSmartcontractTransaction", JsonConvert.SerializeObject(postparameter));

            if (preparedtransaction.State != nameof(PaymentTransactionsStates.active))
            {
                result.ErrorCode = 1209;
                result.ErrorMessage = "Transaction is not in active state";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }


            if (preparedtransaction.Transactiontype == nameof(PaymentTransactionTypes.smartcontract_auction) ||
                preparedtransaction.Transactiontype == nameof(PaymentTransactionTypes.smartcontract_directsale) ||
                preparedtransaction.Transactiontype == nameof(PaymentTransactionTypes.smartcontract_directsale_offer))
            {
                if (preparedtransaction.Smartcontractstate != nameof(PaymentTransactionSubstates.readytosignbyseller) &&
                    preparedtransaction.Smartcontractstate != nameof(PaymentTransactionSubstates.readytosignbysellercancel) &&
                    preparedtransaction.Smartcontractstate != nameof(PaymentTransactionSubstates.readytosignbybuyer) &&
                    preparedtransaction.Smartcontractstate != nameof(PaymentTransactionSubstates.readytosignbybuyercancel))
                {
                    result.ErrorCode = 1301;
                    result.ErrorMessage = "Smartcontract is not in the state of signing";
                    result.ResultState = ResultStates.Error;
                    return StatusCode(406, result);
                }
            }
            if (preparedtransaction.Transactiontype == nameof(PaymentTransactionTypes.legacy_auction) ||
                preparedtransaction.Transactiontype == nameof(PaymentTransactionTypes.legacy_directsale))
            {
                if (preparedtransaction.Smartcontractstate != nameof(PaymentTransactionSubstates.readytosignbyseller) &&
                    preparedtransaction.Smartcontractstate != nameof(PaymentTransactionSubstates.readytosignbysellercancel) &&
                    preparedtransaction.Smartcontractstate != nameof(PaymentTransactionSubstates.readytosignbybuyer) &&
                    preparedtransaction.Smartcontractstate != nameof(PaymentTransactionSubstates.readytosignbybuyercancel) &&
                    preparedtransaction.Smartcontractstate != nameof(PaymentTransactionSubstates.waitingforbid))
                {
                    result.ErrorCode = 1302;
                    result.ErrorMessage = "Transaction is not in the state of signing";
                    result.ResultState = ResultStates.Error;
                    return StatusCode(406, result);
                }
            }

            if (string.IsNullOrEmpty(postparameter.SignedCbor))
            {
                result.ErrorCode = 1105;
                result.ErrorMessage = "Cbor is empty";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            var smjsons =
                preparedtransaction.PreparedpaymenttransactionsSmartcontractsjsons.FirstOrDefault(x =>
                    x.Signinguid == postparameter.SignGuid);

            if (smjsons == null)
            {
                result.ErrorCode = 1120;
                result.ErrorMessage = "Corresponding Json not found. Please contact support";
                result.ResultState = ResultStates.Error;
                return StatusCode(500, result);
            }

            if (smjsons.Signedandsubmitted)
            {
                result.ErrorCode = 1121;
                result.ErrorMessage = "This transaction is already signed and submitted.";
                result.ResultState = ResultStates.Error;
                return StatusCode(500, result);
            }


            MatxRawClass mrc = new()
            { CborHex = postparameter.SignedCbor, Description = "", Type = "Tx BabbageEra" };
            string guid = GlobalFunctions.GetGuid();
            string matxsignedfile = GeneralConfigurationClass.TempFilePath + "matx" + guid + ".signed";
            await System.IO.File.WriteAllTextAsync(matxsignedfile, JsonConvert.SerializeObject(mrc));
            smjsons.Signedcbr = postparameter.SignedCbor;
            smjsons.Signed = DateTime.Now;
            await db.SaveChangesAsync();

            BuildTransactionClass bt = new();

            var submissionresult = await ConsoleCommand.SubmitTransactionWithFallbackAsync(matxsignedfile, bt);

            bt = submissionresult.Buildtransaction;
            if (submissionresult.Success)
            {
                bt.TxHash = submissionresult.TxHash;
            }
            else
            {
                await GlobalFunctions.LogMessageAsync(db, $"Smartcontract submit failed - {postparameter.SignGuid}", submissionresult.ErrorMessage);
            }


            if (string.IsNullOrEmpty(bt.TxHash))
            {
                result.ErrorCode = 1139;
                result.ErrorMessage = "Transaction could not be submitted. See innerErrorMessage for more details";
                result.InnerErrorMessage = bt.LogFile;
                result.ResultState = ResultStates.Error;
                return StatusCode(500, result);
            }

            //   ok = ConsoleCommand.GetTxId(matxsignedfile, ref bt);
            //  if (ok)
            {
                smjsons.Txid = bt.TxHash;
                smjsons.Submitted = DateTime.Now;
                smjsons.Signedandsubmitted = true;
            }
            /*    else
                {
                    result.ErrorCode = 1123;
                    result.ErrorMessage = "Can not determinate TxHash.";
                    result.InnerErrorMessage = bt.LogFile;
                    result.ResultState = ResultStates.Error;
                    return StatusCode(500, result);
                }
            */

            if (preparedtransaction.Transactiontype == nameof(PaymentTransactionTypes.smartcontract_auction))
                preparedtransaction.Smartcontractstate = nameof(PaymentTransactionSubstates.waitingforbid);


            if (preparedtransaction.Transactiontype == nameof(PaymentTransactionTypes.legacy_auction) && preparedtransaction.Legacyauctions.State == "notactive" && smjsons.Templatetype == nameof(DatumTemplateTypes.locknft))
            {
                preparedtransaction.Legacyauctions.State = "active";
                preparedtransaction.Legacyauctions.Locknftstxinhashid = bt.TxHash;
            }
            if (preparedtransaction.Transactiontype == nameof(PaymentTransactionTypes.legacy_directsale) && preparedtransaction.Legacydirectsales.State == "notactive" && smjsons.Templatetype == nameof(DatumTemplateTypes.locknft))
            {
                preparedtransaction.Legacydirectsales.State = "active";
                preparedtransaction.Legacydirectsales.Locknftstxinhashid = bt.TxHash;
            }

            if (preparedtransaction.Transactiontype == nameof(PaymentTransactionTypes.smartcontract_directsale) || preparedtransaction.Transactiontype == nameof(PaymentTransactionTypes.legacy_directsale))
            {
                preparedtransaction.Buyeraddress =
                    preparedtransaction.PreparedpaymenttransactionsSmartcontractsjsons.Last().Address;

                switch (preparedtransaction.Smartcontractstate)
                {
                    case nameof(PaymentTransactionSubstates.readytosignbybuyer) when smjsons.Templatetype != nameof(DatumTemplateTypes.locknft):
                        preparedtransaction.State = nameof(PaymentTransactionsStates.finished);
                        preparedtransaction.Smartcontractstate = nameof(PaymentTransactionSubstates.sold);
                        await db.SaveChangesAsync();
                        GlobalFunctions.DeleteFile(matxsignedfile);
                        return Ok(StaticTransactionFunctions.GetTransactionState(db, _redis, preparedtransaction.Transactionuid, true));
                    case nameof(PaymentTransactionSubstates.readytosignbysellercancel) when (smjsons.Templatetype == nameof(DatumTemplateTypes.cancel)):
                        preparedtransaction.State = nameof(PaymentTransactionsStates.canceled);
                        preparedtransaction.Smartcontractstate = nameof(PaymentTransactionSubstates.canceled);
                        await db.SaveChangesAsync();
                        GlobalFunctions.DeleteFile(matxsignedfile);
                        return Ok(StaticTransactionFunctions.GetTransactionState(db, _redis, preparedtransaction.Transactionuid, true));
                    default:
                        preparedtransaction.Smartcontractstate = nameof(PaymentTransactionSubstates.waitingforsale);
                        break;
                }
            }
            if (preparedtransaction.Transactiontype == nameof(PaymentTransactionTypes.smartcontract_directsale_offer) )
            {
                switch (preparedtransaction.Smartcontractstate)
                {
                    case nameof(PaymentTransactionSubstates.readytosignbyseller) when smjsons.Templatetype != nameof(DatumTemplateTypes.lockada):
                        preparedtransaction.State = nameof(PaymentTransactionsStates.finished);
                        preparedtransaction.Smartcontractstate = nameof(PaymentTransactionSubstates.sold);
                        preparedtransaction.Selleraddress=preparedtransaction.PreparedpaymenttransactionsSmartcontractsjsons.Last().Address;
                        await db.SaveChangesAsync();
                        GlobalFunctions.DeleteFile(matxsignedfile);
                        return Ok(StaticTransactionFunctions.GetTransactionState(db, _redis, preparedtransaction.Transactionuid, true));
                    case nameof(PaymentTransactionSubstates.readytosignbybuyercancel) when (smjsons.Templatetype == nameof(DatumTemplateTypes.cancel)):
                        preparedtransaction.State = nameof(PaymentTransactionsStates.canceled);
                        preparedtransaction.Smartcontractstate = nameof(PaymentTransactionSubstates.canceled);
                        await db.SaveChangesAsync();
                        GlobalFunctions.DeleteFile(matxsignedfile);
                        return Ok(StaticTransactionFunctions.GetTransactionState(db, _redis, preparedtransaction.Transactionuid, true));
                    default:
                        preparedtransaction.Smartcontractstate = nameof(PaymentTransactionSubstates.waitingforsale);
                        break;
                }
            }

            await db.SaveChangesAsync();
            GlobalFunctions.DeleteFile(matxsignedfile);
            return Ok(StaticTransactionFunctions.GetTransactionState(db, _redis, preparedtransaction.Transactionuid, true));
        }

    }
}
