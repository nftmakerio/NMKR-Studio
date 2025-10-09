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

namespace NMKR.Api.Controllers.v2.Projects
{
    /// <summary>
    /// Returns detail information about a project 
    /// </summary>
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class GetProjectDetailsController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        /// <summary>
        /// Returns detail information about a project - Constructor
        /// </summary>
        /// <param name="redis"></param>
        public GetProjectDetailsController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Returns detail information about a project 
        /// </summary>
        /// <remarks>
        /// You will receive all information about this project
        /// </remarks>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="uid">The uid of the project</param>
        /// <response code="200">Returns the NftProjectsDetails Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="404">The nft was not found</response>            
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(NftProjectsDetails))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{projectuid}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Projects" }
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string projectuid)
        {
            // TODO: Change this to authorisation when max is available
          //  string apikey = Request.Headers["authorization"];
            string Token = Request.Headers["apikey"];
            if (Token!=null)
            {
                apikey = Token;
            }
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = projectuid;

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<NftProjectsDetails>(cachedResult.ResultString));
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
            var proj = await (from a in db.Nftprojects
                    .Include(a => a.Customerwallet)
                    .AsSplitQuery()
                    .Include(a => a.Usdcwallet)
                    .AsSplitQuery()
                    .Include(a => a.Solanacustomerwallet)
                    .AsSplitQuery()
                 /*   .Include(a => a.Nftprojectsadditionalpayouts)
                    .AsSplitQuery()
                    .Include(a => a.Nftprojectsaleconditions)
                    .AsSplitQuery()
                    .Include(a => a.Notifications)
                    .AsSplitQuery()
                    .Include(a => a.Pricelists)
                    .AsSplitQuery()
                    .Include(a => a.Pricelistdiscounts)
                    .AsSplitQuery()*/
                where a.Uid == projectuid && a.State != "deleted"
                select a).AsNoTracking().FirstOrDefaultAsync();


            if (proj == null)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorMessage = "Project not found";
                result.ErrorCode = 404;
                await db.Database.CloseConnectionAsync();
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 404, result, apiparameter);
                return NotFound(result);
            }
            
            var proj1 = new NftProjectsDetails(db, proj);
          
            await db.Database.CloseConnectionAsync();
            CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, proj1, apiparameter);
            return Ok(proj1);
        }


    }
}
