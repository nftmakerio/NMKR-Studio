using System.Linq;
using System.Threading.Tasks;
using NMKR.Api.Controllers.SharedClasses;
using NMKR.Shared.Classes;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.Paymenttransactions.Processing.PaymentGatewayTransaction;

     public class ExtendReservationTimeClass : ControllerBase, IProcessPaymentTransactionInterface
    {
        private readonly IConnectionMultiplexer _redis;


        /// <summary>
        /// Reserves Nfts for Mint and Send
        /// </summary>
        /// <param name="redis"></param>
        public ExtendReservationTimeClass(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }
        /// <summary>
        /// Cancels a Mint and send Transaction
        /// </summary>
        /// <param name="db"></param>
        /// <param name="apikey"></param>
        /// <param name="remoteipaddress"></param>
        /// <param name="result"></param>
        /// <param name="preparedtransaction"></param>
        /// <param name="postparameter1"></param>
        /// <returns></returns>
        public async Task<IActionResult> ProcessTransaction(EasynftprojectsContext db,string apikey, string remoteipaddress, ApiErrorResultClass result,
            Preparedpaymenttransaction preparedtransaction, object postparameter1)
        {
            if (preparedtransaction.Transactiontype != nameof(PaymentTransactionTypes.paymentgateway_mintandsend_random) &&
               preparedtransaction.Transactiontype != nameof(PaymentTransactionTypes.paymentgateway_mintandsend_specific))
            {
                result.ErrorCode = 1102;
                result.ErrorMessage = "Command does not fit to this transaction";
                result.ResultState = ResultStates.Error;
                return StatusCode(404, result);
            }

            int extendTime = (int)postparameter1;
            
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


            if (preparedtransaction.Expires != null)
            {
                preparedtransaction.Expires.Value.AddMinutes(extendTime);
                await db.SaveChangesAsync();
            }

            var reservation = await (from a in db.Nftreservations
                where a.Reservationtoken == preparedtransaction.Reservationtoken
                select a).ToListAsync();
            foreach (var res in reservation)
            {
               res.Reservationtime+= extendTime;
            }
            await db.SaveChangesAsync();
            
            return Ok(StaticTransactionFunctions.GetTransactionState(db, _redis, preparedtransaction.Transactionuid, true));
        }
    }
