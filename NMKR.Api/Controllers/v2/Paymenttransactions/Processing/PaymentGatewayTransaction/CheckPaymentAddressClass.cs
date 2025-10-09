using System.Threading.Tasks;
using NMKR.Shared.Classes;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.Paymenttransactions.Processing.PaymentGatewayTransaction
{
    /// <summary>
    /// Checks a payment address and returns the state of it
    /// </summary>
    public class CheckPaymentAddressClass : ControllerBase, IProcessPaymentTransactionInterface
    {
        private readonly IConnectionMultiplexer _redis;
        /// <summary>
        /// Checks a payment address and returns the state of it
        /// </summary>
        /// <param name="redis"></param>
        public CheckPaymentAddressClass(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }
        /// <summary>
        /// Checks a payment address and returns the state of it
        /// </summary>
        /// <param name="db"></param>
        /// <param name="apikey"></param>
        /// <param name="remoteipaddress"></param>
        /// <param name="result"></param>
        /// <param name="preparedtransaction"></param>
        /// <param name="postparameter1"></param>
        /// <returns></returns>
        public async Task<IActionResult> ProcessTransaction(EasynftprojectsContext db,  string apikey, string remoteipaddress, ApiErrorResultClass result,
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

            if (preparedtransaction.Nftaddresses == null)
            {
                result.ErrorCode = 1111;
                result.ErrorMessage = "Internal error - NftAddress not found";
                result.ResultState = ResultStates.Error;
                return StatusCode(500, result);
            }

            switch (preparedtransaction.State)
            {
                //  'active','expired','finished','prepared'
                case nameof(PaymentTransactionsStates.prepared):
                    result.ErrorCode = 1106;
                    result.ErrorMessage = "Transactioncommand is not started";
                    result.ResultState = ResultStates.Error;
                    return StatusCode(404, result);
                case nameof(PaymentTransactionsStates.expired):
                    result.ErrorCode = 1303;
                    result.ErrorMessage = "Transaction already expired";
                    result.ResultState = ResultStates.Error;
                    return StatusCode(404, result);
            }

            Addressreservation.CheckAddressController checkAddress = new(_redis);
            return await checkAddress.Check(result, new(apikey, preparedtransaction.Transactionuid,
                    preparedtransaction.Transactiontype), preparedtransaction.Nftproject.Uid,
                preparedtransaction.NftaddressesNavigation.Address);


        }
    }
}
