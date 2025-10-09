using System.Threading.Tasks;
using NMKR.Api.Controllers.SharedClasses;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.Paymenttransactions.Processing.Legacy
{
    /// <summary>
    /// Cancels legacy directsale transactions
    /// </summary>
    public class CancelLegacyDirectSaleTransactionClass : ControllerBase, IProcessPaymentTransactionInterface
    {
        private readonly IConnectionMultiplexer _redis;

        public CancelLegacyDirectSaleTransactionClass(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Cancels legacy directsale transactions
        /// </summary>
        /// <param name="db"></param>
        /// <param name="apikey"></param>
        /// <param name="result"></param>
        /// <param name="preparedtransaction"></param>
        /// <param name="postparameter1"></param>
        /// <returns></returns>
        public async Task<IActionResult> ProcessTransaction(EasynftprojectsContext db,  string apikey, string remoteipaddress, ApiErrorResultClass result,
            Preparedpaymenttransaction preparedtransaction, object postparameter1)
        {
            if (preparedtransaction.Transactiontype != nameof(PaymentTransactionTypes.legacy_directsale))
            {
                result.ErrorCode = 1102;
                result.ErrorMessage = "Command does not fit to this transaction";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            if (preparedtransaction.State != nameof(PaymentTransactionsStates.active))
            {
                result.ErrorCode = 1209;
                result.ErrorMessage = "Transaction is not in active state";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            if (preparedtransaction.Smartcontractstate != nameof(PaymentTransactionSubstates.waitingforsale))
            {
                result.ErrorCode = 1308;
                result.ErrorMessage = "Transaction is not in the state of sale - cancel not possible";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            if (preparedtransaction.Legacydirectsales.State != "active")
            {
                result.ErrorCode = 1209;
                result.ErrorMessage = "Legacy directsale is not in active state";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }


            string guid = GlobalFunctions.GetGuid();
            string matxrawfile = GeneralConfigurationClass.TempFilePath + "matx" + guid + ".raw";
            string protocolParamsFile = GeneralConfigurationClass.TempFilePath + "protocol" + guid + ".params";


            ConsoleCommand.GenerateProtocolParamsFile(protocolParamsFile,_redis, GlobalFunctions.IsMainnet(), out var errormessage);
            BuildTransactionClass bt = new();

            var ok = await ConsoleCommand.CancelSmartLegacyDirectSaleAsync(db, _redis, preparedtransaction.Legacydirectsales,
                GlobalFunctions.IsMainnet());

            // Last check if the transaction is still in the right state - mabye we have to check this with a mysql function
            if (!ok)
            {
                result.ErrorCode = 1145;
                result.ErrorMessage = "Transaction can not be canceled. Please contact support";
                result.ResultState = ResultStates.Error;
                return StatusCode(500, result);
            }

            preparedtransaction.State = nameof(PaymentTransactionsStates.canceled);
            preparedtransaction.Smartcontractstate = nameof(PaymentTransactionSubstates.canceled);
            preparedtransaction.Legacydirectsales.State = "finished";
            await db.SaveChangesAsync();

            GlobalFunctions.DeleteFile(matxrawfile);
            GlobalFunctions.DeleteFile(protocolParamsFile);

            return Ok(StaticTransactionFunctions.GetTransactionState(db, _redis, preparedtransaction.Transactionuid, true));
        }

       
    }
}
