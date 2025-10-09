using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions.Koios;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;

namespace NMKR.Api.Controllers.v2.Tools
{
    /// <summary>
    /// Returns the royalty information for a specific policyid
    /// </summary>
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class GetRoyaltyInformationController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        /// <summary>
        /// Returns the royalty information for a specific policyid - Constructor
        /// </summary>
        /// <param name="redis"></param>
        public GetRoyaltyInformationController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Returns the royalty information for a specific policyid
        /// </summary>
        /// <remarks>
        /// You will receive the rate in percent and the wallet address for the royalties (if applicable) of a specific policyid
        /// </remarks>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="policyid">The policyid</param>
        /// <response code="200">Returns an array of RoyaltyClass</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong policyid etc.)</response>     
        /// <response code="404">There are no royalty informations for this policyid</response>
        /// <response code="406">The policyid is not valid</response>  
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RoyaltyClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{policyid}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] {"Tools"}
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string policyid)
        {
           // string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = policyid;

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<RoyaltyClass>(cachedResult.ResultString));
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

            if (policyid.Length!=56)
            {
                result.ErrorCode = 1378;
                result.ErrorMessage = "PolicyId is not valid";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }


            var royalties = await KoiosFunctions.GetRoyaltiesFromPolicyIdAsync(policyid);

            if (royalties != null)
            {
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, royalties, apiparameter, 120);
                return Ok(royalties);
            }

            result.ErrorCode = 1379;
            result.ErrorMessage = "This project has no royalty information";
            result.ResultState = ResultStates.Error;
            return StatusCode(404, result);
        }

    }
}
