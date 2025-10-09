using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.SaleConditions;
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
    public class CheckIfSaleConditionsMetController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public CheckIfSaleConditionsMetController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }



        /// <summary>
        /// Checks, if an address matches the sale conditions
        /// </summary>
        /// <remarks>
        /// Checks, if an address matches the sale conditions of a project
        /// </remarks>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="projectuid">The uid of the project</param>
        /// <param Name="address">The cardano address to be tested on the saleconditions</param>
        /// <param Name="countnft">The amount of nfts/tokens</param>
        /// <response code="200">Returns the CheckConditionsResultClass Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="500">Internal server error - see the errormessage in the result</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CheckConditionsResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{projectuid}/{address}/{countnft}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] {"Tools"}
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string projectuid,
            string address, long countnft, [FromQuery]Blockchain blockchain=Blockchain.Cardano)
        {
          //  string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;

            string apifunction = this.GetType().Name;
            string apiparameter = projectuid + "_" + address + "_" + countnft;


            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<CheckConditionsResultClass>(cachedResult.ResultString));
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

            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            var project = await (from a in db.Nftprojects
                where a.Uid == projectuid
                      && a.State != "deleted"
                select a).FirstOrDefaultAsync();

            if (project == null)
            {
                LogClass.LogMessage(db, "API-CALL: ERROR: Project not found " + projectuid);
                result.ErrorCode = 56;
                result.ErrorMessage = "Project not found. Please submit valid project UID";
                result.ResultState = ResultStates.Error;

                return StatusCode(406, result);
            }

            if (project.State != "active")
            {
                LogClass.LogMessage(db, "API-CALL: ERROR: Project not active " + projectuid);
                result.ErrorCode = 57;
                result.ErrorMessage = "Project is not active";
                result.ResultState = ResultStates.Error;

                return StatusCode(406, result);
            }

            var res=await CheckSalesConditionClass.CheckForSaleConditionsMet(db,_redis, project.Id, address, countnft, 0,project.Usefrankenprotection, blockchain);

            CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, res, apiparameter);
            return Ok(res);
        }
    }
}
