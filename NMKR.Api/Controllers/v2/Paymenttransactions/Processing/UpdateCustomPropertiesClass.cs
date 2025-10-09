using System.Collections.Generic;
using System.Threading.Tasks;
using NMKR.Api.Controllers.SharedClasses;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.Paymenttransactions.Processing
{
    /// <summary>
    /// Updates the custom properties from the payment transaction
    /// </summary>
    public class UpdateCustomPropertiesClass : ControllerBase, IProcessPaymentTransactionInterface
    {
        private readonly IConnectionMultiplexer _redis;

        public UpdateCustomPropertiesClass(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }
        /// <summary>
        /// Updates the custom properties from the payment transaction
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
            var properties = postparameter1 as Dictionary<string, string>;

            switch (preparedtransaction.State)
            {
                case nameof(PaymentTransactionsStates.expired):
                    result.ErrorCode = 1304;
                    result.ErrorMessage = "Transaction already expired";
                    result.ResultState = ResultStates.Error;
                    return StatusCode(404, result);
                case nameof(PaymentTransactionsStates.finished):
                    result.ErrorCode = 1105;
                    result.ErrorMessage = "Transactioncommand already finished";
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

            // Delete the old properties
            await GlobalFunctions.ExecuteSqlWithFallbackAsync(db,
                $"delete from preparedpaymenttransactions_customproperties where preparedpaymenttransactions_id={preparedtransaction.Id}");

            if (properties != null)
            {
                foreach (var ptcCustomProperty in properties)
                {
                    var custom = new PreparedpaymenttransactionsCustomproperty()
                    {
                        PreparedpaymenttransactionsId = preparedtransaction.Id,
                        Key = ptcCustomProperty.Key,
                        Value = ptcCustomProperty.Value
                    };
                    await db.AddAsync(custom);
                    await db.SaveChangesAsync();
                }
            }

            return Ok(StaticTransactionFunctions.GetTransactionState(db, _redis, preparedtransaction.Transactionuid, true));

        }
    }
}
