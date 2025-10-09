using System.Threading.Tasks;
using NMKR.Api.Controllers.SharedClasses;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.Paymenttransactions.Processing.Legacy
{
    /// <summary>
    /// Cancels legacy directsale transactions
    /// </summary>
    public class CancelNmkrPayTransactionClass : ControllerBase, IProcessPaymentTransactionInterface
    {
        private readonly IConnectionMultiplexer _redis;

        public CancelNmkrPayTransactionClass(IConnectionMultiplexer redis)
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
        public async Task<IActionResult> ProcessTransaction(EasynftprojectsContext db, string apikey, string remoteipaddress, ApiErrorResultClass result,
            Preparedpaymenttransaction preparedtransaction, object postparameter1)
        {
            if (preparedtransaction.Transactiontype != nameof(PaymentTransactionTypes.nmkr_pay_random) && preparedtransaction.Transactiontype!=nameof(PaymentTransactionTypes.nmkr_pay_specific))
            {
                result.ErrorCode = 1102;
                result.ErrorMessage = "Command does not fit to this transaction";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            if (preparedtransaction.State != nameof(PaymentTransactionsStates.active) && preparedtransaction.State!="prepared")
            {
                result.ErrorCode = 1209;
                result.ErrorMessage = "Transaction is not in active/prepared state";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            await GlobalFunctions.LogMessageAsync(db, "Cancel NMKR PAY Transaction", JsonConvert.SerializeObject(postparameter1));

            preparedtransaction.State = nameof(PaymentTransactionsStates.canceled);
            await db.SaveChangesAsync();
            RedisFunctions.DeleteKey(_redis, "TransactionState_" + preparedtransaction.Transactionuid);
            return Ok(StaticTransactionFunctions.GetTransactionState(db, _redis, preparedtransaction.Transactionuid, true));
        }


    }
}
