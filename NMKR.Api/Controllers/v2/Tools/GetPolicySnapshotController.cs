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
    /// <summary>
    /// Returns a snapshot with all addresses and tokens for a specific policyid
    /// </summary>
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class GetPolicySnapshotController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        /// <summary>
        /// Returns a snapshot with all addresses and tokens for a specific policyid - Constructor
        /// </summary>
        /// <param name="redis"></param>
        public GetPolicySnapshotController(IConnectionMultiplexer redis)
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(NmkrAssetAddress[]))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{policyid}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Tools" }
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string policyid,[FromQuery] bool cumulateStakeAddresses=true, [FromQuery] bool withMintingInformation = false, [FromQuery] Blockchain blockchain=Blockchain.Cardano)
        {
          //  string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = policyid + "_" + cumulateStakeAddresses;

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<NmkrAssetPolicySnapshot[]>(cachedResult.ResultString));
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


            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            var snap = new GetPolicySnapshotClass();

            var assetaddresses = await snap.GetAllAddressesForSpecificPolicyIdAsync(_redis, policyid, cumulateStakeAddresses,withMintingInformation, blockchain );
            if (assetaddresses == null)
            {
                result.ErrorMessage = "Internal error - took too long to retrieve the data. Please try again later.";
                result.ErrorCode= 500;
                return StatusCode(500, result);
            }



            await db.Database.CloseConnectionAsync();
            CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, assetaddresses, apiparameter, 120);
            return Ok(assetaddresses);
        }


    }
}
