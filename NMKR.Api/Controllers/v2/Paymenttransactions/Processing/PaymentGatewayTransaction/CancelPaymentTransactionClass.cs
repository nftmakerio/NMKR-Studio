using System.Threading.Tasks;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.Paymenttransactions.Processing.PaymentGatewayTransaction
{
    /// <summary>
    /// Cancels a payment transaction
    /// </summary>
    public class CancelPaymentTransactionClass : ControllerBase, IProcessPaymentTransactionInterface
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IBus _bus;

        /// <summary>
        /// Cancels a payment transaction
        /// </summary>
        /// <param name="redis"></param>
        public CancelPaymentTransactionClass(IConnectionMultiplexer redis, IBus bus)
        {
            _redis = redis;
            _bus = bus;
        }
        /// <summary>
        /// Cancels a payment transaction
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
                if (preparedtransaction.Transactiontype != nameof(PaymentTransactionTypes.paymentgateway_nft_specific) &&
                    preparedtransaction.Transactiontype != nameof(PaymentTransactionTypes.paymentgateway_nft_random))
                {
                    result.ErrorCode = 1102;
                    result.ErrorMessage = "Command does not fit to this transaction";
                    result.ResultState = ResultStates.Error;
                    return StatusCode(404, result);
                }

                if (preparedtransaction.State != nameof(PaymentTransactionsStates.active))
                {
                    result.ErrorCode = 1209;
                    result.ErrorMessage = "Transaction is not in active state";
                    result.ResultState = ResultStates.Error;
                    return StatusCode(406, result);
                }

                preparedtransaction.State = nameof(PaymentTransactionsStates.canceled);
                await db.SaveChangesAsync();
                await GlobalFunctions.LogMessageAsync(db, "Cancel Address Reseration Transaction", JsonConvert.SerializeObject(postparameter1));

                Addressreservation.CancelAddressReservationController cancelAddressReservationController = new(_redis, _bus);
                RedisFunctions.DeleteKey(_redis, "TransactionState_" + preparedtransaction.Transactionuid);

            return await cancelAddressReservationController.Cancel(result, new(apikey,
                        preparedtransaction.Transactionuid,
                        preparedtransaction.Transactiontype), preparedtransaction.Nftproject.Uid,
                    preparedtransaction.NftaddressesNavigation.Address, remoteipaddress);


        }
    }
}
