using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.Api;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;
using NMKR.Shared.Functions.Koios;
using NMKR.Api.Controllers.SharedClasses;
using NMKR.Shared.Functions.Blockfrost;

namespace NMKR.Api.Controllers.v2.SmartContracts
{
    /// <summary>
    /// Returns a Transaction to NMKR Pay for a listed asset in a smartcontract
    /// </summary>
    ///
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class GetListedAssetPaymentTransactionController : Controller
    {
        private readonly IConnectionMultiplexer _redis;

        /// <summary>
        /// Returns a Transaction to NMKR Pay for a listed asset in a smartcontract (works only with NFTs)
        /// </summary>
        /// <param name="redis"></param>
        public GetListedAssetPaymentTransactionController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentTransactionResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{policyid}/{assetnameinhex}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Smartcontracts" }
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string policyid,
            string assetnameinhex)
        {
           // string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            if (Request.Method.Equals("HEAD"))
                return null;

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;

            string apifunction = this.GetType().Name;
            string apiparameter = policyid + "_"+ assetnameinhex ;

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
                return StatusCode(cachedResult.Statuscode,
                    JsonConvert.DeserializeObject<ApiErrorResultClass>(cachedResult.ResultString));


            // Check if the Apikey is valid
            var result = CheckCachedAccess.CheckApikeyOrToken(_redis, apifunction,
               "", apikey, remoteIpAddress?.ToString());
            if (result.ResultState != ResultStates.Ok)
            {
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }

            if (string.IsNullOrEmpty(policyid) || policyid.Length != 56)
            {
                result.ErrorCode = 6601;
                result.ErrorMessage = "Policyid wrong";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            if (string.IsNullOrWhiteSpace(assetnameinhex))
            {
                result.ErrorCode = 6602;
                result.ErrorMessage = "Assetname wrong";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);


            var nft = await KoiosFunctions.GetNftAddressAsync(policyid, assetnameinhex);
            if (nft == null || !nft.Any())
            {
                result.ErrorCode = 6603;
                result.ErrorMessage = "Asset not found";
                result.ResultState = ResultStates.Error;
                return StatusCode(404, result);
            }

            var smartcontract = await (from a in db.Smartcontracts
                where a.Address == nft.First().PaymentAddress
                select a).AsNoTracking().FirstOrDefaultAsync();

            if (smartcontract == null)
            {
                result.ErrorCode = 6604;
                result.ErrorMessage = "Asset not listed for sale in a supported smartcontract";
                result.ResultState = ResultStates.Error;
                return StatusCode(404, result);
            }

            var transaction = await BlockfrostFunctions.GetLastAssetTransactionAsync(policyid, assetnameinhex);
            if (transaction == null)
            {
                result.ErrorCode = 6605;
                result.ErrorMessage = "Last transaction for the asset could not found";
                result.ResultState = ResultStates.Error;
                return StatusCode(500, result);
            }
           
            CreatePaymentTransactionClass pt = new CreatePaymentTransactionClass()
            {
                
                CustomerIpAddress="",
                ProjectUid = await StaticTransactionFunctions.GetDefaultProjectUid(db, "directsaleV2"),
                PaymentTransactionType = PaymentTransactionTypes.smartcontract_directsale,
                DirectSaleParameters = new DirectSaleParameterClass()
                {
                    TxHashForAlreadyLockedinAssets = transaction.TxHash + "#0"
                }
            };

            var preparedtransaction=await ApiFunctions.CallCreatePaymentTransactionAsync(pt);

            if (preparedtransaction == null)
            {
                result.ErrorCode = 6606;
                result.ErrorMessage = "Transaction could not be established";
                result.ResultState = ResultStates.Error;
                return StatusCode(500, result);
            }



            return Ok(preparedtransaction);
        }

    }
}
