using System;
using System.Linq;
using System.Threading.Tasks;
using NMKR.Api.Controllers.SharedClasses;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.Paymenttransactions.Processing.PaymentGatewayTransaction
{
    /// <summary>
    /// Mints the previously reserved nfts
    /// </summary>
    public class MintAndSendPaymentgatewayNftClass : ControllerBase, IProcessPaymentTransactionInterface
    {
        private readonly IConnectionMultiplexer _redis;
        /// <summary>
        /// Mints the previously reserved nfts
        /// </summary>
        /// <param name="redis"></param>
        public MintAndSendPaymentgatewayNftClass(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }
        /// <summary>
        /// Mints the previously reserved nfts
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

            var mintAndSendRecvier = postparameter1 as MintAndSendReceiverClass;

            if (preparedtransaction.Transactiontype != nameof(PaymentTransactionTypes.paymentgateway_mintandsend_random) &&
                preparedtransaction.Transactiontype != nameof(PaymentTransactionTypes.paymentgateway_mintandsend_specific))
            {
                result.ErrorCode = 1102;
                result.ErrorMessage = "Command does not fit to this transaction";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }
            switch (preparedtransaction.State)
            {
                //  'active','expired','finished','prepared'
                case nameof(PaymentTransactionsStates.prepared):
                  result.ErrorCode = 1103;
                  result.ErrorMessage = "Reserve NFT first";
                  result.ResultState = ResultStates.Error;
                  return StatusCode(406, result);
                case nameof(PaymentTransactionsStates.expired):
                    result.ErrorCode = 1304;
                    result.ErrorMessage = "Transaction already expired";
                    result.ResultState = ResultStates.Error;
                    return StatusCode(406, result);
                case nameof(PaymentTransactionsStates.finished):
                    return Ok(StaticTransactionFunctions.GetTransactionState(db, _redis, preparedtransaction.Transactionuid, true));
                case nameof(PaymentTransactionsStates.error):
                    result.ErrorCode = 1106;
                    result.ErrorMessage = "Transactioncommand had errors";
                    result.ResultState = ResultStates.Error;
                    return StatusCode(406, result);
                case nameof(PaymentTransactionsStates.canceled):
                    result.ErrorCode = 1107;
                    result.ErrorMessage = "Transactioncommand already canceled";
                    result.ResultState = ResultStates.Error;
                    return StatusCode(406, result);
            }

            if (mintAndSendRecvier == null)
            {
                result.ErrorCode = 2101;
                result.ErrorMessage = "You must submit the receiver";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            if (string.IsNullOrEmpty(mintAndSendRecvier.ReceiverAddress))
            {
                result.ErrorCode = 2102;
                result.ErrorMessage = "You must submit the receiver";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            if (!ConsoleCommand.CheckIfAddressIsValid(db,mintAndSendRecvier.ReceiverAddress, GlobalFunctions.IsMainnet(), out string outaddress, out Blockchain blockchain))
            {
                result.ErrorCode = 2103;
                result.ErrorMessage = "Receiver address is not a valid cardano address";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            var reservation = await (from a in db.Nftreservations
                where a.Reservationtoken == preparedtransaction.Reservationtoken
                select a).AsNoTracking().FirstOrDefaultAsync();

            if (reservation == null)
            {
                result.ErrorCode = 2104;
                result.ErrorMessage = "Reservation time for the nft is already expired";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }



            var ms = new Mintandsend()
            {
                Created = DateTime.Now,
                CustomerId = preparedtransaction.Nftproject.CustomerId,
                NftprojectId = preparedtransaction.NftprojectId,
                Receiveraddress = mintAndSendRecvier.ReceiverAddress,
                Reservationtoken = preparedtransaction.Reservationtoken,
                State = "execute",
                Onlinenotification = false,
                Reservelovelace = 0,
                Usecustomerwallet = true,
                Coin = blockchain == Blockchain.Cardano ? Coin.ADA.ToString() : Coin.SOL.ToString(),
            };
            await db.Mintandsends.AddAsync(ms);
            await db.SaveChangesAsync();

            // Set the Mint and send in the Nftreservation so that it will not be released automatically
            string sql =
                $"update nftreservations set mintandsendcommand=1 where reservationtoken='{preparedtransaction.Reservationtoken}'";
            await GlobalFunctions.ExecuteSqlWithFallbackAsync(db, sql);


            preparedtransaction.MintandsendId = ms.Id;
            preparedtransaction.State = nameof(PaymentTransactionsStates.finished);
            await db.SaveChangesAsync();
            return Ok(StaticTransactionFunctions.GetTransactionState(db, _redis, preparedtransaction.Transactionuid, true));
        }
    }
}
