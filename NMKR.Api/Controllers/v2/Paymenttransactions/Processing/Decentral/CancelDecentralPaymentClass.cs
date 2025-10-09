using System.Threading.Tasks;
using NMKR.Api.Controllers.SharedClasses;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.Paymenttransactions.Processing.Decentral
{
    /// <summary>
    /// Cancels a multi-sig (decentral) payment
    /// </summary>
    public class CancelDecentralPaymentClass : ControllerBase, IProcessPaymentTransactionInterface
    {

        private readonly IConnectionMultiplexer _redis;


        /// <summary>
        /// Cancels a multi-sig (decentral) payment
        /// </summary>
        /// <param name="redis"></param>
        public CancelDecentralPaymentClass(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task<IActionResult> ProcessTransaction(EasynftprojectsContext db, string apikey,
            string remoteipaddress, ApiErrorResultClass result,
            Preparedpaymenttransaction preparedtransaction, object postparameter1)
        {
            switch (preparedtransaction.State)
            {
                case nameof(PaymentTransactionsStates.expired):
                    result.ErrorCode = 1304;
                    result.ErrorMessage = "Transaction already expired";
                    result.ResultState = ResultStates.Error;
                    return StatusCode(404, result);
                case nameof(PaymentTransactionsStates.finished):
                    result.ErrorCode = 1108;
                    result.ErrorMessage = "Transactioncommand is already finished";
                    result.ResultState = ResultStates.Error;
                    return StatusCode(404, result);
                case nameof(PaymentTransactionsStates.error):
                    result.ErrorCode = 1106;
                    result.ErrorMessage = "Transactioncommand had errors";
                    result.ResultState = ResultStates.Error;
                    return StatusCode(404, result);
                case nameof(PaymentTransactionsStates.canceled):
                    result.ErrorCode = 1107;
                    result.ErrorMessage = "Transactioncommand already canceled";
                    result.ResultState = ResultStates.Error;
                    return StatusCode(404, result);
            }


            preparedtransaction.State = nameof(PaymentTransactionsStates.canceled);
            preparedtransaction.Cbor = null;
            preparedtransaction.Signedcbor = null;
            await db.SaveChangesAsync();

            await NftReservationClass.ReleaseAllNftsAsync(db,_redis, preparedtransaction.Reservationtoken, 0,true);

            return Ok(StaticTransactionFunctions.GetTransactionState(db, _redis, preparedtransaction.Transactionuid, true));
        }
    }
}
