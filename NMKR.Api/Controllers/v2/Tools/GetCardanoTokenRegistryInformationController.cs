using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Classes;
using NMKR.Shared.Classes.Koios;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.Koios;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;

namespace NMKR.Api.Controllers.v2.Tools
{
    /// <summary>
    /// Returns tha Token Registry Information for a specific token (if available)
    /// </summary>
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class GetCardanoTokenRegistryInformationController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        /// <summary>
        /// Returns the Token Registry Information for a specific token (if available) - Constructor
        /// </summary>
        /// <param name="redis"></param>
        public GetCardanoTokenRegistryInformationController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Returns the Token Registry Information for a specific token (if available)
        /// </summary>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="policyid">The policyid</param>
        /// <param name="tokenname">The Name of the Token (not HEX)</param>
        /// <response code="200">Returns TokenInformationClass</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="404">No Registry Information was not found</response>      
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TokenInformationClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{policyid}/{tokenname}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] {"Tools"}
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string policyid,
            string tokenname)
        {
          //  string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = policyid + " " + tokenname;

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<TokenRegistryMetadata>(cachedResult.ResultString));
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

            if (policyid.Length != 56)
            {
                result.ErrorCode = 1378;
                result.ErrorMessage = "PolicyId is not valid";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            var assetinformations =
                await KoiosFunctions.GetTokenInformationAsync(policyid, GlobalFunctions.ToHexString(tokenname));


            if (assetinformations == null || !assetinformations.Any())
            {
                result.ErrorCode = 1368;
                result.ErrorMessage = "No Token registry information found (1)";
                result.ResultState = ResultStates.Error;
                return StatusCode(404, result);
            }


            CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200,
                assetinformations.First(), apiparameter, 120);
            return Ok(assetinformations.First());
        }
    }
}
