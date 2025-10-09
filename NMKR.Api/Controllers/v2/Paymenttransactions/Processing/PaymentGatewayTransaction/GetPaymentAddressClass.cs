using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using NMKR.Api.Controllers.SharedClasses;
using NMKR.Api.Controllers.v2.Addressreservation;
using NMKR.Shared.Classes;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.Paymenttransactions.Processing.PaymentGatewayTransaction;

/// <summary>
/// Retrieves a payment address and sends it to the transaction
/// </summary>
public class GetPaymentAddressClass : ControllerBase, IProcessPaymentTransactionInterface
{
    private readonly IConnectionMultiplexer _redis;

    /// <summary>
    /// Retrieves a payment address and sends it to the transaction
    /// </summary>
    /// <param name="redis"></param>
    public GetPaymentAddressClass(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    /// <summary>
    /// Retrieves a payment address and sends it to the transaction
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
        if (preparedtransaction.Transactiontype != nameof(PaymentTransactionTypes.paymentgateway_nft_specific) &&
            preparedtransaction.Transactiontype != nameof(PaymentTransactionTypes.paymentgateway_nft_random))
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
                if (!string.IsNullOrEmpty(preparedtransaction.Cachedresultgetpaymentaddress))
                {
                    return Ok(JsonConvert.DeserializeObject<GetPaymentAddressResultClass>(preparedtransaction
                        .Cachedresultgetpaymentaddress));
                }

                result.ErrorCode = 1103;
                result.ErrorMessage = "Transaction already started";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            case nameof(PaymentTransactionsStates.expired):
                result.ErrorCode = 1304;
                result.ErrorMessage = "Transaction already expired";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            case nameof(PaymentTransactionsStates.finished):
                result.ErrorCode = 1105;
                result.ErrorMessage = "Transaction already finished";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            case nameof(PaymentTransactionsStates.error):
                result.ErrorCode = 1106;
                result.ErrorMessage = "Transaction had errors";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            case nameof(PaymentTransactionsStates.canceled):
                result.ErrorCode = 1107;
                result.ErrorMessage = "Transaction already canceled";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
        }

        switch (preparedtransaction.Transactiontype)
        {
            case nameof(PaymentTransactionTypes.paymentgateway_nft_random):
            {
                ReserveAddressQueueClass raqc = new()
                {
                    ApiKey = apikey,
                    NftprojectId = preparedtransaction.NftprojectId,
                    CountNft = preparedtransaction.Countnft ?? 0,
                    CardanoLovelace = preparedtransaction.Lovelace,
                    Price = preparedtransaction.Lovelace,
                    RemoteIpAddress = IPAddress.None?.ToString(),
                    CustomerIpAddress = preparedtransaction.Customeripaddress,
                    Uid = preparedtransaction.Transactionuid,
                    OptionalReceiverAddress = preparedtransaction.Optionalreceiveraddress,
                };


                var resx = await ReserveRandomNftByApiClass.RequestRandomAddress(_redis, raqc,
                    preparedtransaction.Id);


                if (resx.StatusCode != 0)
                {
                    preparedtransaction.State = nameof(PaymentTransactionsStates.error);
                    await db.SaveChangesAsync();
                    return StatusCode(resx.StatusCode, resx.ApiError);
                }



                preparedtransaction.NftaddressesId = resx.SuccessResult.PaymentAddressId;
                preparedtransaction.State = nameof(PaymentTransactionsStates.active);
                preparedtransaction.Cachedresultgetpaymentaddress = JsonConvert.SerializeObject(resx.SuccessResult);
                await db.SaveChangesAsync();

                return Ok(resx.SuccessResult);
            }
            case nameof(PaymentTransactionTypes.paymentgateway_nft_specific):
            {
                GetPaymentAddressForSpecificNftSaleController getPayment = new(_redis);

                List<ReserveNftsClassV2> reserveNfts = new();

                if (preparedtransaction.PreparedpaymenttransactionsNfts != null)
                {
                    foreach (var preparedtransactionPreparedpaymenttransactionsNft in preparedtransaction
                                 .PreparedpaymenttransactionsNfts)
                    {
                        reserveNfts.Add(new()
                        {
                            NftUid = preparedtransactionPreparedpaymenttransactionsNft.Nft.Uid,
                            Tokencount = preparedtransactionPreparedpaymenttransactionsNft.Count
                        });
                    }
                }

                ReserveMultipleNftsClassV2 rmn = new()
                    {ReserveNfts = reserveNfts.ToArray()};

                ReserveAddressQueueClass raqc = new()
                {
                    ApiKey = apikey,
                    NftprojectId = preparedtransaction.NftprojectId,
                    CardanoLovelace = preparedtransaction.Lovelace,
                    Price= preparedtransaction.Lovelace,
                    RemoteIpAddress = IPAddress.None?.ToString(),
                    CustomerIpAddress = preparedtransaction.Customeripaddress,
                    Uid = preparedtransaction.Transactionuid,
                    Reservenfts = rmn,
                    OptionalReceiverAddress = preparedtransaction.Optionalreceiveraddress
                };

                var resx = await ReserveSpecificNftByApiClass.RequestSpecificAddress(_redis, raqc,
                    preparedtransaction.Id);


                if (resx.StatusCode != 0)
                {
                    preparedtransaction.State = nameof(PaymentTransactionsStates.error);
                    await db.SaveChangesAsync();
                    return StatusCode(resx.StatusCode, resx.ApiError);
                }

                preparedtransaction.NftaddressesId = resx.SuccessResult.PaymentAddressId;
                preparedtransaction.State = nameof(PaymentTransactionsStates.active);
                preparedtransaction.Cachedresultgetpaymentaddress = JsonConvert.SerializeObject(resx.SuccessResult);
                await db.SaveChangesAsync();

                return Ok(resx.SuccessResult);
            }
            default:
                return StatusCode(500);
        }
    }
}