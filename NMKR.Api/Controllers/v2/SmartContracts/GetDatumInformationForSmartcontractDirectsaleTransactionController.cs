using System.Collections.Generic;
using System.Linq;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;
using NMKR.Shared.Functions.Koios;
using NMKR.Shared.SmartContracts;
using Microsoft.EntityFrameworkCore;
using CardanoSharp.Wallet.Enums;
using NMKR.Shared.Functions.Blockfrost;
using System;
using Asp.Versioning;

namespace NMKR.Api.Controllers.v2.SmartContracts
{
    /// <summary>
    /// Returns the datum information for a smartcontract directsale transaction
    /// </summary>
    ///
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class GetDatumInformationForSmartcontractDirectsaleTransactionController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        /// <summary>
        /// Returns the datum information for a smartcontract directsale transaction - Constructor
        /// </summary>
        /// <param name="redis"></param>
        public GetDatumInformationForSmartcontractDirectsaleTransactionController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Returns the datum information for a smartcontract directsale transaction
        /// </summary>
        /// <remarks>
        /// You will receive the datum information of a smartcontract directsale transaction (JPG Store V2 Contract and NMKR V2 Contract)
        /// </remarks>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="policyid">The txhash of the transaction</param>
        /// <response code="200">Returns an array of SmartcontractDirectsaleDatumInformationClass</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong policyid etc.)</response>     
        /// <response code="404">There are no royalty informations for this policyid</response>
        /// <response code="406">The policyid is not valid</response>  
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SmartcontractDirectsaleDatumInformationClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{txhash}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] {"Smartcontracts"}
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string txhash)
        {
          //  string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = txhash;

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode != 200)
                    return StatusCode(cachedResult.Statuscode,
                        JsonConvert.DeserializeObject<ApiErrorResultClass>(cachedResult.ResultString));
            }

            // Check if the Apikey is valid
            var result = CheckCachedAccess.CheckApikeyOrToken(_redis, apifunction,
                "", apikey, remoteIpAddress?.ToString());
            if (result.ResultState != ResultStates.Ok)
            {
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }

            var rt= await GetSmartcontractsOutputsAsync(result, txhash);
            if (rt.ApiError.ResultState == ResultStates.Error)
            {
                return StatusCode(rt.ActionResultStatuscode, rt.ApiError);
            }

            return Ok(rt.SuccessResultObject as SmartcontractDirectsaleDatumInformationClass);
        }


        internal ApiResultClass GetSmartcontractsOutputs(ApiErrorResultClass result, string txhash)
        {
            var res = Task.Run(async () => await GetSmartcontractsOutputsAsync(result, txhash));
            return res.Result;
        }


        internal async Task<ApiResultClass> GetSmartcontractsOutputsAsync(ApiErrorResultClass result, string txhash)
        {
            var res = new ApiResultClass() {ApiError = result};

            await using EasynftprojectsContext db = new(GlobalFunctions.optionsBuilder.Options);

            var transaction = await KoiosFunctions.GetTransactionInformationAsync(txhash);
            if (transaction == null || !transaction.Any())
            {
                res.ApiError.ErrorCode = 10001;
                res.ApiError.ErrorMessage = "Transaction not found";
                res.ApiError.ResultState = ResultStates.Error;
                res.ActionResultStatuscode = 404;

                return res;
            }

            Smartcontract smartcontract = null;
            foreach (var output in transaction.First().Outputs)
            {
                var addr = output.PaymentAddr.Bech32;
                smartcontract = await (from a in db.Smartcontracts
                    where a.Address == addr
                    select a).AsNoTracking().FirstOrDefaultAsync();
                if (smartcontract != null)
                    break;
            }

            if (smartcontract == null)
            {
                foreach (var input in transaction.First().Inputs)
                {
                    var addr = input.PaymentAddr.Bech32;
                    smartcontract = await (from a in db.Smartcontracts
                        where a.Address == addr
                        select a).AsNoTracking().FirstOrDefaultAsync();
                    if (smartcontract != null)
                        break;
                }
            }

            if (smartcontract == null)
            {
                res.ApiError.ErrorCode = 10009;
                res.ApiError.ErrorMessage = "Unsupported Smartcontract";
                res.ApiError.ResultState = ResultStates.Error;
                res.ActionResultStatuscode = 406;
                return res;
            }


            if (smartcontract.Type == "directsale") // OLD V1 Contract - will not be used in the future
            {
                if (txhash.Length < 64)
                {
                    res.ApiError.ErrorCode = 10046;
                    res.ApiError.ErrorMessage = "TXHash is not correct. Please provide it with # and the TxHash";
                    res.ApiError.ResultState = ResultStates.Error;
                    res.ActionResultStatuscode = 406;
                    return res;
                }
                string txhashsplit= txhash.Substring(0, 64);
                var transactionnmkr = await (from a in db.PreparedpaymenttransactionsSmartcontractsjsons
                    where a.Txid == txhashsplit
                    select a).AsNoTracking().FirstOrDefaultAsync();
                if (transactionnmkr != null)
                {
                    return await GetDatumInformationNmkrSmartcontract(db, transactionnmkr, smartcontract);
                }
                else
                {
                    res.ApiError.ErrorCode = 10047;
                    res.ApiError.ErrorMessage = "Transaction could not be found";
                    res.ApiError.ResultState = ResultStates.Error;
                    res.ActionResultStatuscode = 406;
                    return res;
                }
            }




            var metadata = await KoiosFunctions.GetMetadataAsync(txhash);
            if (metadata == null || !metadata.Any())
            {
                res.ApiError.ErrorCode = 10002;
                res.ApiError.ErrorMessage = "Datum not found";
                res.ApiError.ResultState = ResultStates.Error;
                res.ActionResultStatuscode = 406;
                return res;
            }

            if (!metadata.First().Metadata.Any())
            {
                res.ApiError.ErrorCode = 10003;
                res.ApiError.ErrorMessage = "Datum not found";
                res.ApiError.ResultState = ResultStates.Error;
                res.ActionResultStatuscode = 406;
                return res;
            }

            var mt = metadata.First().Metadata;




            var stringbyte = "";
            long txid = 0;
            try
            {
                txid = Convert.ToInt64(txhash.Split('#').Last());
            }
            catch
            {
                res.ApiError.ErrorCode = 10043;
                res.ApiError.ErrorMessage = "TXHash is not correct. Please provide it with # and the TxHash";
                res.ApiError.ResultState = ResultStates.Error;
                res.ActionResultStatuscode = 406;
                return res;
            }

            var bl = BlockfrostFunctions.GetTransactionUtxoFromBlockfrost(txhash);
            if (bl != null && bl.Outputs.Any())
            {
                var asset = bl.Outputs.FirstOrDefault(x => x.OutputIndex == txid && x.DataHash != null);
                stringbyte = asset!=null? BlockfrostFunctions.GetDatumCborFromDatumHash(asset.DataHash) : null;
            }


            if (string.IsNullOrEmpty(stringbyte))
            {
                foreach (var jpgStoreMetadatumValue in mt)
                {
                    if (jpgStoreMetadatumValue.Key != "30")
                    {
                        stringbyte += jpgStoreMetadatumValue.Value.String;
                    }
                }

                if (!string.IsNullOrEmpty(stringbyte))
                {
                    var stringbytes = stringbyte.Split(',');
                    if (stringbytes.Length > txid)
                    {
                        stringbyte = stringbytes[txid];
                    }
                }
            }



            if (string.IsNullOrEmpty(stringbyte))
            {
                res.ApiError.ErrorCode = 10004;
                res.ApiError.ErrorMessage = "Datum not found";
                res.ApiError.ResultState = ResultStates.Error;
                res.ActionResultStatuscode = 406;
                return res;
            }

            try
            {
                var outputs = JpgStoreSmartContractDatumParser.Parse(stringbyte,
                    GlobalFunctions.IsMainnet() ? NetworkType.Mainnet : NetworkType.Preprod);

                if (outputs == null || !outputs.Any())
                {
                    res.ApiError.ErrorCode = 10005;
                    res.ApiError.ErrorMessage = "Outputs not found";
                    res.ApiError.ResultState = ResultStates.Error;
                    res.ActionResultStatuscode = 406;
                    return res;
                }

                var rt = GetDatumInformationJpgstore(stringbyte, outputs, smartcontract);
                return rt;
            }
            catch
            {
                res.ApiError.ErrorCode = 10006;
                res.ApiError.ErrorMessage = "Datum not found or not supported - it must be a JPGStore Smartcontract V2 Datum";
                res.ApiError.ResultState = ResultStates.Error;
                res.ActionResultStatuscode = 406;
                return res;
            }
        }


        private async Task<ApiResultClass> GetDatumInformationNmkrSmartcontract(EasynftprojectsContext db, PreparedpaymenttransactionsSmartcontractsjson transactionnmkr, Smartcontract smartcontract)
        {
            var preparedtransaction = await (from a in db.Preparedpaymenttransactions
                    .Include(a=>a.Nftproject)
                    .ThenInclude(a=>a.Smartcontractssettings).AsSplitQuery()
                    .Include(a=>a.PreparedpaymenttransactionsSmartcontractOutputs)
                    .ThenInclude(a=>a.PreparedpaymenttransactionsSmartcontractOutputsAssets).AsSplitQuery()
                where a.Id == transactionnmkr.PreparedpaymenttransactionsId
                select a).AsNoTracking().FirstOrDefaultAsync();

            if (preparedtransaction == null)
            {
                return new ApiResultClass()
                {
                    ActionResultStatuscode = 500,
                    ApiError = new ApiErrorResultClass()
                        {ErrorCode = 3320, ErrorMessage = "internal error", ResultState = ResultStates.Error}
                };
            }
            SmartcontractDirectsaleDatumInformationClass res = new SmartcontractDirectsaleDatumInformationClass()
            {
                DatumCbor = "",
                TotalPriceInLovelace = preparedtransaction.Lovelace??0,
                NmkrPayLink = GeneralConfigurationClass.Paywindowlink+ $"adsid={preparedtransaction.Transactionuid}&a=buy",
                PreparedPaymentTransactionId = preparedtransaction.Transactionuid,
                SmartContractName = smartcontract.Smartcontractname,
                SmartContractAddress = smartcontract.Address
            };
            List<SmartcontractDirectsaleReceiverClass> receiver = new List<SmartcontractDirectsaleReceiverClass>();


            // TODO: Multiplier and decimals
            // If there was one or more bids, close it with fees for the marketplace and royalty
            foreach (var smartcontractOutput in preparedtransaction.PreparedpaymenttransactionsSmartcontractOutputs)
            {
                receiver.Add(new()
                {
                    Address = smartcontractOutput.Address,
                    AmountInLovelace = smartcontractOutput.Lovelace,
                    RecevierType = smartcontractOutput.Type, Pkh = smartcontractOutput.Pkh, Tokens =
                        (from a in smartcontractOutput.PreparedpaymenttransactionsSmartcontractOutputsAssets
                            select new Tokens()
                            {
                                AssetName = a.Tokennameinhex.FromHex(), AssetNameInHex = a.Tokennameinhex,
                                PolicyId = a.Policyid, CountToken = a.Amount, TotalCount = a.Amount, Decimals = 0,
                                Multiplier = 1
                            }).ToArray()
                });
            }


            res.Receivers = receiver.ToArray();
            return new ApiResultClass()
            {
                ActionResultStatuscode = 200,
                ApiError = new ApiErrorResultClass() {ErrorCode = 0, ResultState = ResultStates.Ok},
                SuccessResultObject = res
            };
        }

        private ApiResultClass GetDatumInformationJpgstore(string stringbyte, List<JpgStoreSmartContractDatumParser.Output> outputs, Smartcontract smartcontract)
        {
            SmartcontractDirectsaleDatumInformationClass res = new SmartcontractDirectsaleDatumInformationClass()
            {
                DatumCbor = stringbyte,
                TotalPriceInLovelace = outputs.Sum(x => x.Lovelace),
                NmkrPayLink = null,
                PreparedPaymentTransactionId = null,
                SmartContractName = smartcontract.Smartcontractname,
                SmartContractAddress=smartcontract.Address,
            };
            res.Receivers = (from a in outputs
                select new SmartcontractDirectsaleReceiverClass()
                {
                    Address = a.AddressBech32, AmountInLovelace = a.Lovelace,
                    Pkh = GlobalFunctions.GetPkhFromAddress(a.AddressBech32)
                }).ToArray();


            return new ApiResultClass()
            {
                ActionResultStatuscode = 200,
                ApiError = new ApiErrorResultClass() {ErrorCode = 0, ResultState = ResultStates.Ok},
                SuccessResultObject = res
            };
        }
    }
}
