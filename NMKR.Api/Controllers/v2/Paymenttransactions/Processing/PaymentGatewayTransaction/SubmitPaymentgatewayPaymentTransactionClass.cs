using System.Threading.Tasks;
using NMKR.Api.Controllers.SharedClasses;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.Paymenttransactions.Processing.PaymentGatewayTransaction
{
    /// <summary>
    /// Submits a payment gateway (PGW) Transaction (Directsale or Auction)
    /// </summary>
    public class SubmitPaymentgatewayPaymentTransactionClass : ControllerBase, IProcessPaymentTransactionInterface
    {

        private readonly IConnectionMultiplexer _redis;


        /// <summary>
        /// Reserves Nfts for Mint and Send
        /// </summary>
        /// <param name="redis"></param>
        public SubmitPaymentgatewayPaymentTransactionClass(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Submits a payment gateway (PGW) Transaction (Directsale or Auction)
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
            await GlobalFunctions.LogMessageAsync(db, "API: SubmitPaymentgatewayTransaction", JsonConvert.SerializeObject(postparameter));

            if (preparedtransaction.Paymentgatewaystate != nameof(PaymentGatewayStates.readytosignbybuyer))
            {
                result.ErrorCode = 1317;
                result.ErrorMessage = "Transaction is not in the state of signing";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            if (string.IsNullOrEmpty(postparameter.SignedCbor))
            {
                result.ErrorCode = 1105;
                result.ErrorMessage = "Cbor is empty";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }


            MatxRawClass mrc = new()
            { CborHex = postparameter.SignedCbor, Description = "", Type = "Tx ConwayEra" };
            string guid = GlobalFunctions.GetGuid();
            string matxsignedfile = GeneralConfigurationClass.TempFilePath + "matx" + guid + ".signed";
            await System.IO.File.WriteAllTextAsync(matxsignedfile, JsonConvert.SerializeObject(mrc));
            preparedtransaction.Signedcbor = postparameter.SignedCbor;
            await db.SaveChangesAsync();

            BuildTransactionClass bt = new();
            var ok = ConsoleCommand.SubmitTransaction(matxsignedfile, GlobalFunctions.IsMainnet(), ref bt);
            if (!ok.Success)
            {
                result.ErrorCode = 1129;
                result.ErrorMessage = "Transaction could not be submitted. See innerErrorMessage for more details";
                result.InnerErrorMessage = bt.LogFile;
                result.ResultState = ResultStates.Error;
                return StatusCode(500, result);
            }

            preparedtransaction.Paymentgatewaystate = nameof(PaymentGatewayStates.signedbybuyer);
            preparedtransaction.Logfile += bt.LogFile;
            ConsoleCommand.GetTxId(matxsignedfile, ref bt);

            await db.SaveChangesAsync();

            GlobalFunctions.DeleteFile(matxsignedfile);
            return Ok(StaticTransactionFunctions.GetTransactionState(db, _redis, preparedtransaction.Transactionuid, true));
        }

    }
}
