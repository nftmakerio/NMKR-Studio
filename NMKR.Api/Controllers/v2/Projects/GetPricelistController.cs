using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.PricelistFunctions;
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
    /// Returns the actual valid pricelist for this project (project uid)
    /// </summary>
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class GetPricelistController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        /// <summary>
        /// Returns the actual valid pricelist for this project (project uid) - Constructor
        /// </summary>
        /// <param name="redis"></param>
        public GetPricelistController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }


        /// <summary>
        /// Returns the actual valid pricelist for this project (project uid)
        /// </summary>
        /// <remarks>
        /// You will get the predefined prices for one or more NFTs
        /// </remarks>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="projectuid">The uid of your project (not the id)</param>
        /// <param Name="returnAllPrices">Returns all prices, even those that are not yet active or have already expired</param> 
        /// <response code="200">Returns an array of the PricelistClass</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PricelistClass>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{projectuid}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Projects" }
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string projectuid, [FromQuery]bool returnAllPrices=false)
        {
           // string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            if (Request.Method.Equals("HEAD"))
                return null;
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = projectuid.ToString() + returnAllPrices.ToString();

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<List<PricelistClass>>(cachedResult.ResultString));
                return StatusCode(cachedResult.Statuscode,
                    JsonConvert.DeserializeObject<ApiErrorResultClass>(cachedResult.ResultString));
            }


            // Check if the Apikey is valid
            var result = CheckCachedAccess.CheckApikeyOrToken(_redis, apifunction,
               "", apikey, remoteIpAddress?.ToString());
            if (result.ResultState != ResultStates.Ok)
            {
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter,60);
                return Unauthorized(result);
            }
            return await GetPricelist(result, new(apikey, apifunction, apiparameter), projectuid, returnAllPrices);
        }

        internal async Task<IActionResult> GetPricelist(ApiErrorResultClass result, CachedApiCallValues cachedApiCallValues, string projectuid, bool returnAllPrices)
        {
            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
          
            var project = await (from a in db.Nftprojects
                where a.Uid == projectuid
                select a).FirstOrDefaultAsync();

            if (project == null)
            {
                result.ErrorCode = 50;
                result.ErrorMessage = "Projectuid not known";
                result.ResultState = ResultStates.Error;
                CheckCachedAccess.SetCachedResult(_redis, cachedApiCallValues, 404, result);
                return StatusCode(406, result);
            }

            var pl1 = await GetPricelistClass.GetPriceList(db, project, _redis, returnAllPrices);

            await db.Database.CloseConnectionAsync();
            CheckCachedAccess.SetCachedResult(_redis, cachedApiCallValues, 200, pl1);
            return Ok(pl1);
        }

     

     
    }
}
