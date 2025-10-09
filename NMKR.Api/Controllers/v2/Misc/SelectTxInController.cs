using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Api.Controllers.SharedClasses;
using NMKR.Shared.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;

namespace NMKR.Api.Controllers.v2.Misc
{
    /// <summary>
    /// Returns a snapshot with all addresses and tokens for a specific policyid
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class SelectTxInController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        /// <summary>
        /// Returns a snapshot with all addresses and tokens for a specific policyid - Constructor
        /// </summary>
        /// <param name="redis"></param>
        public SelectTxInController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Returns a snapshot with all addresses and tokens for a specific policyid
        /// </summary>
        /// <remarks>
        /// You will receive all tokens and the holding addresses of a specific policyid
        /// </remarks>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="policyid">The policyid</param>
        /// <response code="200">Returns an array of NmkrAssetAddress</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="404">The policyid was not found</response>            
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TxInAddressesClass[]))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{lovelaceneeded}/{address}/{collateral}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Tools" }
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, long lovelaceneeded, string address, string collateral, string tokenneeded=null, long? tokencount=null)
        {
           // string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = address + lovelaceneeded;

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<TxInAddressesClass[]>(cachedResult.ResultString));
                return StatusCode(cachedResult.Statuscode, JsonConvert.DeserializeObject<ApiErrorResultClass>(cachedResult.ResultString));
            }



            // Check if the Apikey is valid
            var result = CheckCachedAccess.CheckApikeyOrToken(_redis, apifunction,
                "", apikey, remoteIpAddress?.ToString());
            if (result.ResultState != ResultStates.Ok)
            {
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }


            var utxofinal = StaticTransactionFunctions.GetAllNeededTxin(_redis,new []{address},
                lovelaceneeded,
                tokencount??0, tokenneeded, collateral, out string errormessage);

            if (!string.IsNullOrEmpty(errormessage))
            {
                result.ErrorCode = 1210;
                result.ErrorMessage = errormessage;
                result.ResultState = ResultStates.Error;
                return StatusCode(500, result);
            }

            return Ok(utxofinal);
        }


    }
}
