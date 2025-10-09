using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Classes;
using NMKR.Shared.Classes.Auctions;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;

namespace NMKR.Api.Controllers.v2.Auctions
{
    /// <summary>
    /// Returns the state - and the last bids of a auction project 
    /// </summary>
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class GetAllAuctionsController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        /// <summary>
        /// Returns the state - and the last bids of a auction project 
        /// </summary>
        /// <param name="redis"></param>
        public GetAllAuctionsController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Returns all auctions of the customer
        /// </summary>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="auctionuid">The uid of your auction</param>
        /// <response code="200">Returns an array of the GetAuctionStateResultClass</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<GetAuctionsClass>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{customerid}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Auctions" }
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, int customerid)
        {
            // string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            if (Request.Method.Equals("HEAD"))
                return null;
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = customerid.ToString();

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
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
            var customerid1 = CheckCachedAccess.GetCustomerIdFromApikey(apikey);

            if (customerid1 == null)
            {
                result.ErrorMessage = "The apikey is not valid";
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 1901;
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }

            if (customerid1 != -1 && customerid1 != customerid)
            {
                result.ErrorMessage = "The apikey is not valid to this customerid";
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 1902;
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }

            return await GetAuctions(result, new(apikey, apifunction, apiparameter), customerid);
        }

        internal async Task<IActionResult> GetAuctions(ApiErrorResultClass result,
            CachedApiCallValues cachedApiCallValues, int customerid)
        {
            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            var auctions = await (from a in db.Legacyauctions
                                 where a.CustomerId ==customerid && a.State != "deleted"
                                 select new GetAuctionsClass()
                                 {
                                     Address = a.Address, State = a.State, Created = a.Created, Uid = a.Uid,
                                     AuctionType = "legacy", Auctionname = a.Auctionname, RunsUntil= a.Runsuntil
                                 } ).ToListAsync();

            await db.Database.CloseConnectionAsync();
            return Ok(auctions);
        }
    }
}
