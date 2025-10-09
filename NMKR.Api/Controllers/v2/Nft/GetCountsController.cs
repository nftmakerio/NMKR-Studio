using System;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;

namespace NMKR.Api.Controllers.v2.Nft
{
    /// <summary>
    /// Get counts controller description
    /// </summary>
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class GetCountsController : ControllerBase
    {

        private readonly IConnectionMultiplexer _redis;

        public GetCountsController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Returns the count of the sold, reserved and free nfts (project uid)
        /// </summary>
        /// <remarks>
        /// You will get the count of all sold, reserved and free nfts of a particular project
        /// </remarks>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="nftprojectid">The id of your project</param>
        /// <response code="200">Returns the NftCountsClass</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(NftCountsClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{projectuid}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Projects" }
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string projectuid)
        {
          //  string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            if (Request.Method.Equals("HEAD"))
                return null;


            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = projectuid;

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<NftCountsClass>(cachedResult.ResultString));
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
            NftCountsClass ncc = new();
            var project = await (from a in db.Nftprojects
                where a.Uid == projectuid
                select a).FirstOrDefaultAsync();

            if (project != null)
            {
                ncc.NftTotal = project.Total1;

                if (project.Maxsupply == 1)
                {
                    ncc.Free = Math.Max(0,project.Free1 - (project.Nftsblocked??0));
                    ncc.Reserved = project.Reserved1;
                    ncc.Sold = project.Sold1;
                    ncc.Error = project.Error1;
                    ncc.TotalTokens = project.Total1;
                    ncc.Blocked = (project.Blocked1??0);
                    ncc.TotalBlocked = project.Nftsblocked??0;
                    ncc.UnknownOrBurnedState = ncc.TotalTokens - ncc.Free - ncc.Reserved - ncc.Error - ncc.Blocked - ncc.TotalBlocked - ncc.Sold;
                }
                else
                {
                    ncc.Sold = project.Tokenssold1;
                    ncc.Reserved = project.Tokensreserved1;
                    ncc.Free = Math.Max(0, project.Totaltokens1 - project.Tokenssold1 - project.Tokensreserved1 - (project.Nftsblocked??0));
                    ncc.Error = project.Error1;
                    ncc.TotalTokens = project.Totaltokens1;
                    ncc.TotalBlocked = project.Nftsblocked??0;
                    ncc.UnknownOrBurnedState = 0;
                }
            }

            CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, ncc, apiparameter);
            await db.Database.CloseConnectionAsync();
            return Ok(ncc);
        }

    
    }
}
