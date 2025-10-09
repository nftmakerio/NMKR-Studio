using System;
using System.Linq;
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

namespace NMKR.Api.Controllers.v2.Projects
{
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class CreateBurningAddressController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public CreateBurningAddressController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Creates a burning endpoint for a specific address
        /// </summary>
        /// <remarks>
        /// When you call this endpoint, a Burning Address is created for this project. All NFTs associated with this project (same policyid) that are sent to this endpoint will be deleted (burned). All other NFTs will be sent back. 
        /// The policy of the project must still be active.If it is already locked, it can no longer be deleted.
        /// </remarks>
        /// <param Name="authorization">The apikey you have created on NMKR Studio - the apikey mus have the PurchaseSpecificNft permission</param>
        /// <param Name="projectuid">The uid of your project</param>
        /// <param Name="addressactiveinhours">How long the burning address should be active (in hours)</param>
        /// <response code="200">The burning address was created successfully</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectuid etc.)</response>     
        /// <response code="406">Some parameters where not correct or the project already has 10 or more burning addresses</response>
        /// <response code="500">Internal server error - see the errormessage in the result</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CreateBurningEndpointClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status402PaymentRequired, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{projectuid}/{addressactiveinhours:int}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Projects" }
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string projectuid,
            int addressactiveinhours, [FromQuery]Blockchain blockchain=Blockchain.Cardano)
        {
           // string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            if (Request.Method.Equals("HEAD"))
                return null;

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;



            string apifunction = this.GetType().Name;
            string apiparameter = projectuid + "_" + addressactiveinhours;

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
                return StatusCode(cachedResult.Statuscode,
                    JsonConvert.DeserializeObject<ApiErrorResultClass>(cachedResult.ResultString));


            // Check if the Apikey is valid
            var result = CheckCachedAccess.CheckApikeyOrToken(_redis, apifunction,
                "", apikey, remoteIpAddress?.ToString() ?? string.Empty);
            if (result.ResultState != ResultStates.Ok)
            {
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }


            var project = await (from a in db.Nftprojects
                                 where a.Uid == projectuid && a.State=="active"
                                 select a).FirstOrDefaultAsync();

            if (project == null)
            {
                LogClass.LogMessage(db, "API-CALL: ERROR: Project not found " + projectuid);
                result.ErrorCode = 56;
                result.ErrorMessage = "Internal error. Please contact support";
                result.ResultState = ResultStates.Error;

                return StatusCode(500, result);
            }


            if (project.Enabledcoins.Contains(Coin.ADA.ToString()) == false && blockchain==Blockchain.Cardano)
            {
                result.ErrorCode = 4102;
                result.ErrorMessage = "Cardano is not enabled in this project";
                result.ResultState = ResultStates.Error;
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, result, apiparameter);
                return StatusCode(406, result);
            }
            if (project.Enabledcoins.Contains(Coin.SOL.ToString()) == false && blockchain == Blockchain.Solana)
            {
                result.ErrorCode = 4102;
                result.ErrorMessage = "Solana is not enabled in this project";
                result.ResultState = ResultStates.Error;
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, result, apiparameter);
                return StatusCode(406, result);
            }


            if (blockchain==Blockchain.Cardano && project.Policyexpire != null && project.Policyexpire < DateTime.Now)
            {
                LogClass.LogMessage(db, "API-CALL: ERROR: Project not found " + projectuid);
                result.ErrorCode = 4101;
                result.ErrorMessage = "Policy on this project is already locked. Burning not longer possible";
                result.ResultState = ResultStates.Error;
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, result, apiparameter);
                return StatusCode(406, result);
            }
            if (addressactiveinhours < 0)
            {
                result.ErrorCode = 4104;
                result.ErrorMessage = "The parameter 'addressActiveInHours' must be at least 1 hour";
                result.ResultState = ResultStates.Error;
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, result, apiparameter);
                return StatusCode(406, result);
            }
            if (addressactiveinhours > 100000)
            {
                result.ErrorCode = 4105;
                result.ErrorMessage = "The parameter 'addressActiveInHours' can not exceed 100000 hours";
                result.ResultState = ResultStates.Error;
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, result, apiparameter);
                return StatusCode(406, result);
            }


            int nftprojectid = project.Id;
            await GlobalFunctions.UpdateLastActionProjectAsync(db, nftprojectid, _redis);

            var be = await GlobalFunctions.CreateBurningAddressAsync(db, nftprojectid, DateTime.Now.AddHours(addressactiveinhours), blockchain,false,false);

            if (be == null || string.IsNullOrEmpty(be.Address))
            {
                result.ErrorCode = 4405;
                result.ErrorMessage = "Error while creating burning endpoint. Please contact support";
                result.ResultState = ResultStates.Error;
                return StatusCode(500, result);
            }

            CreateBurningEndpointClass cbepc = new() {Address = be.Address, Validuntil = be.Validuntil, Blockchain = blockchain};

            return Ok(cbepc);

        }
    }
}
