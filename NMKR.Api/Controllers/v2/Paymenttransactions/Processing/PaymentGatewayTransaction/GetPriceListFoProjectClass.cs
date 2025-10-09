using System.Threading.Tasks;
using NMKR.Shared.Classes;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.Paymenttransactions.Processing.PaymentGatewayTransaction
{
    /// <summary>
    /// Retrives the pricelist for a specific project
    /// </summary>
    public class GetPriceListFoProjectClass : ControllerBase, IProcessPaymentTransactionInterface
    {
        private readonly IConnectionMultiplexer _redis;

        /// <summary>
        /// Retrives the pricelist for a specific project
        /// </summary>
        /// <param name="redis"></param>
        public GetPriceListFoProjectClass(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Retrives the pricelist for a specific project
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
            if (preparedtransaction.Transactiontype != nameof(PaymentTransactionTypes.paymentgateway_nft_specific) &&
                preparedtransaction.Transactiontype != nameof(PaymentTransactionTypes.paymentgateway_nft_random))
            {
                result.ErrorCode = 1102;
                result.ErrorMessage = "Command does not fit to this transaction";
                result.ResultState = ResultStates.Error;
                return StatusCode(404, result);
            }

            Projects.GetPricelistController getPricelistController = new(_redis);
            return await getPricelistController.GetPricelist(result, new(apikey,
                preparedtransaction.Transactionuid,
                preparedtransaction.Transactiontype), preparedtransaction.Nftproject.Uid, false);

        }
    }
}
