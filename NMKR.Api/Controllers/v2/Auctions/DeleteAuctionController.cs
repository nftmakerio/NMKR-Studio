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

namespace NMKR.Api.Controllers.v2.Auctions
{
    /// <summary>
    /// Returns the state - and the last bids of a auction project 
    /// </summary>
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class DeleteAuctionController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        /// <summary>
        /// Returns the state - and the last bids of a auction project 
        /// </summary>
        /// <param name="redis"></param>
        public DeleteAuctionController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Deletes an auction - if the auction is not already started
        /// </summary>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="auctionuid">The uid of your auction</param>
        /// <response code="200">Returns an array of the GetAuctionStateResultClass</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{auctionuid}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Auctions" }
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string auctionuid)
        {
            // string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            if (Request.Method.Equals("HEAD"))
                return null;
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = auctionuid;

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
            var customerid = CheckCachedAccess.GetCustomerIdFromApikey(apikey);
            if (customerid == null)
            {
                result.ErrorMessage = "The apikey is not valid";
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 1901;
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }

            return await DeleteAuction(result, new(apikey, apifunction, apiparameter), auctionuid, customerid);
        }

        internal async Task<IActionResult> DeleteAuction(ApiErrorResultClass result,
            CachedApiCallValues cachedApiCallValues, string auctionuid, int? customerid)
        {
            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);

            var auction = await (from a in db.Legacyauctions
                where a.Uid == auctionuid
                select a).FirstOrDefaultAsync();

            if (auction == null)
            {
                result.ErrorCode = 3401;
                result.ErrorMessage = "Auction not found";
                result.ResultState = ResultStates.Error;
                CheckCachedAccess.SetCachedResult(_redis, cachedApiCallValues, 404, result);
                return StatusCode(404, result);
            }

            if (auction.CustomerId != customerid)
            {
                result.ErrorCode = 3402;
                result.ErrorMessage = "Auction can not be deleted";
                result.ResultState = ResultStates.Error;
                CheckCachedAccess.SetCachedResult(_redis, cachedApiCallValues, 404, result);
                return StatusCode(401, result);
            }

            auction.State = "deleted";
            await db.SaveChangesAsync();

            return Ok();
        }
    }
}
