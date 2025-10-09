using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;

namespace NMKR.Api.Controllers.v2.Tools
{
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class GetAdaRatesController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public GetAdaRatesController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Returns the actual price in EUR and USD for ADA
        /// </summary>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <response code="200">Returns the RatesClass</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(NewRatesClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [HttpGet]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Tools" }
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey)
        {
            //  string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            if (Request.Method.Equals("HEAD"))
                return null;

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = "";

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<NewRatesClass>(cachedResult.ResultString));
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

            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            var rates = (await GlobalFunctions.GetNewRatesAsync(_redis, Coin.ADA));

            CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, rates, apiparameter);
            await db.Database.CloseConnectionAsync();
            return Ok(rates);
        }
    }
}