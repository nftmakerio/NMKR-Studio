using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Enums;

namespace NMKR.Api.Controllers.v2.Tools
{
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class GetRatesController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public GetRatesController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Returns the actual price in EUR and USD for ADA,APT,SOL,ETH, etc.
        /// </summary>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <response code="200">Returns the NewRatesClass</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(NewRatesClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [HttpGet]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Tools" }
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, [FromQuery]Coin coin=Coin.ADA)
        {
            //  string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            if (Request.Method.Equals("HEAD"))
                return null;

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = coin.ToString();

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
              return StatusCode(cachedResult.Statuscode, JsonConvert.DeserializeObject<ApiErrorResultClass>(cachedResult.ResultString));
            }
            // Check if the Apikey is valid
            var result = CheckCachedAccess.CheckApikeyOrToken(_redis, apifunction,
               "", apikey, remoteIpAddress?.ToString() ?? string.Empty);
            if (result.ResultState != ResultStates.Ok)
            {
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }


            if (coin==Coin.USD || coin == Coin.EUR || coin == Coin.JPY || coin==Coin.USDC)
            {
                result.ErrorMessage = "Coin is not supported for the call";
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 6522;
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, result, apiparameter);
                return StatusCode(406,result);
            }


            var rates = (await GlobalFunctions.GetNewRatesAsync(_redis, coin));

            return Ok(rates);
        }
    }
}
