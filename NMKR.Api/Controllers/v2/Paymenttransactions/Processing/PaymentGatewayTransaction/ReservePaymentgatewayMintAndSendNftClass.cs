using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NMKR.Api.Controllers.SharedClasses;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.Paymenttransactions.Processing.PaymentGatewayTransaction
{
    /// <summary>
    /// Reserves Nfts for Mint and Send
    /// </summary>
    public class ReservePaymentgatewayMintAndSendNftClass : ControllerBase, IProcessPaymentTransactionInterface
    {
        private readonly IConnectionMultiplexer _redis;


        /// <summary>
        /// Reserves Nfts for Mint and Send
        /// </summary>
        /// <param name="redis"></param>
        public ReservePaymentgatewayMintAndSendNftClass(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        ///  Reserves Nfts for Mint and Send
        /// </summary>
        /// <param name="db"></param>
        /// <param name="apikey"></param>
        /// <param name="remoteipaddress"></param>
        /// <param name="result"></param>
        /// <param name="preparedtransaction"></param>
        /// <param name="postparameter1"></param>
        /// <returns></returns>
        public async Task<IActionResult> ProcessTransaction(EasynftprojectsContext db, string apikey,
            string remoteipaddress, ApiErrorResultClass result,
            Preparedpaymenttransaction preparedtransaction, object postparameter1)
        {
            if (preparedtransaction.Transactiontype !=
                nameof(PaymentTransactionTypes.paymentgateway_mintandsend_random) &&
                preparedtransaction.Transactiontype !=
                nameof(PaymentTransactionTypes.paymentgateway_mintandsend_specific))
            {
                result.ErrorCode = 1102;
                result.ErrorMessage = "Command does not fit to this transaction";
                result.ResultState = ResultStates.Error;
                return StatusCode(404, result);
            }

            switch (preparedtransaction.State)
            {
                //  'active','expired','finished','prepared'
                case nameof(PaymentTransactionsStates.active):
                    return Ok(StaticTransactionFunctions.GetTransactionState(db, _redis, preparedtransaction.Transactionuid,
                        true));
                case nameof(PaymentTransactionsStates.expired):
                    result.ErrorCode = 1304;
                    result.ErrorMessage = "Transaction already expired";
                    result.ResultState = ResultStates.Error;
                    return StatusCode(406, result);
                case nameof(PaymentTransactionsStates.finished):
                    result.ErrorCode = 1105;
                    result.ErrorMessage = "Transactioncommand already finished";
                    result.ResultState = ResultStates.Error;
                    return StatusCode(406, result);
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

            var guid = GlobalFunctions.GetGuid();

            int restime = preparedtransaction.Nftproject.Expiretime;
            if (postparameter1!=null)
                restime= (int) postparameter1;
            if (restime <= 0)
                restime = preparedtransaction.Nftproject.Expiretime;


            List<Nftreservation> reservation = null;
            if (preparedtransaction.Transactiontype ==
                nameof(PaymentTransactionTypes.paymentgateway_mintandsend_random))
            {

                reservation = await NftReservationClass.ReserveRandomNft(db,_redis, guid, preparedtransaction.NftprojectId,
                    preparedtransaction.Countnft ?? 0, restime,
                    false, false, Coin.ADA); // Mint and Send Flag will set on the MintNfts Command, because of the Expiration if the mindandsend was not send
                await GlobalFunctions.LogMessageAsync(db, "Made Reservation via Prepardpaymenttransaction Random",
                    JsonConvert.SerializeObject(reservation,
                        new JsonSerializerSettings() {ReferenceLoopHandling = ReferenceLoopHandling.Ignore}));
            }

            if (preparedtransaction.Transactiontype ==
                nameof(PaymentTransactionTypes.paymentgateway_mintandsend_specific))
            {
                var nfts = (from a in preparedtransaction.PreparedpaymenttransactionsNfts
                    select new ReserveNftsClass() {NftId = (int) a.NftId, Tokencount = a.Count, Multiplier = GlobalFunctions.GetMultiplier(db,a.NftId)}).ToArray();

                reservation = await NftReservationClass.ReserveSpecificNft(db,_redis, guid, preparedtransaction.NftprojectId,
                    nfts, restime,
                    false, false,Coin.ADA); // Mint and Send Flag will set on the MintNfts Command, because of the Expiration if the mindandsend was not send
                await GlobalFunctions.LogMessageAsync(db, "Made Reservation via Prepardpaymenttransaction Specific",
                    JsonConvert.SerializeObject(reservation,
                        new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
            }


            if (reservation != null && reservation.Any())
            {
                preparedtransaction.State = nameof(PaymentTransactionsStates.active);
                preparedtransaction.Reservationtoken = guid;
                preparedtransaction.Expires=DateTime.Now.AddMinutes(restime);
                await db.SaveChangesAsync();

                if (preparedtransaction.Transactiontype ==
                    nameof(PaymentTransactionTypes.paymentgateway_mintandsend_random))
                {
                    foreach (var resSelectedreservation in reservation)
                    {
                        var nft = await (from a in db.Nfts
                                .Include(a => a.Nftproject)
                            where a.Id == resSelectedreservation.NftId
                            select a).AsNoTracking().FirstOrDefaultAsync();
                        if (nft != null)
                        {
                            await db.PreparedpaymenttransactionsNfts.AddAsync(new()
                            {
                                Count = resSelectedreservation.Tc, 
                                Lovelace = 0, 
                                NftId = resSelectedreservation.NftId,
                                PreparedpaymenttransactionsId = preparedtransaction.Id,
                                Nftuid = nft.Uid,
                                Tokenname = (nft.Nftproject.Tokennameprefix ?? "") + nft.Assetname,
                                Tokennamehex =
                                    GlobalFunctions.ToHexString((nft.Nftproject.Tokennameprefix ?? "") + nft.Assetname),
                                Policyid = nft.Nftproject.Policyid,
                            });
                            await db.SaveChangesAsync();
                        }
                    }
                }
            }
            else
            {
                preparedtransaction.State = nameof(PaymentTransactionsStates.error);
                await db.SaveChangesAsync();
                result.ErrorCode = 2301;
                result.ErrorMessage = "No more NFT available or all NFT are already reserved";
                result.ResultState = ResultStates.Error;
                await GlobalFunctions.LogMessageAsync(db, "Api: " + result.ErrorMessage + $" - {result.ErrorCode}",
                    JsonConvert.SerializeObject(preparedtransaction));
                return StatusCode(404, result);
            }

            return Ok(StaticTransactionFunctions.GetTransactionState(db, _redis, preparedtransaction.Transactionuid, true));
        }
    }
}
