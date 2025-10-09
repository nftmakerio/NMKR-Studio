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
    public class GetAuctionStateController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        /// <summary>
        /// Returns the state - and the last bids of a auction project 
        /// </summary>
        /// <param name="redis"></param>
        public GetAuctionStateController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Returns the state - and the last bids of a auction project 
        /// </summary>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="auctionuid">The uid of your auction</param>
        /// <response code="200">Returns an array of the GetAuctionStateResultClass</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetAuctionStateResultClass))]
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
            string apiparameter = auctionuid.ToString();

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<GetAuctionStateResultClass>(cachedResult.ResultString));
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

            return await GetAuction(result, new(apikey, apifunction, apiparameter), auctionuid);
        }

        internal async Task<IActionResult> GetAuction(ApiErrorResultClass result,
            CachedApiCallValues cachedApiCallValues, string auctionuid)
        {
            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            var auction = await (from a in db.Legacyauctions
                    .Include(a=>a.LegacyauctionsNfts).AsSplitQuery()
                    .Include(a=>a.Legacyauctionshistories).AsSplitQuery()
                where a.Uid==auctionuid && a.State != "deleted"
                select a).FirstOrDefaultAsync();

            if (auction == null)
            {
                result.ErrorCode = 3401;
                result.ErrorMessage = "Auction not found";
                result.ResultState = ResultStates.Error;
                CheckCachedAccess.SetCachedResult(_redis, cachedApiCallValues, 404, result);
                return StatusCode(404, result);
            }

            GetAuctionStateResultClass resx = new()
            {
                Actualbet = auction.Actualbet, State = auction.State, Runsuntil = auction.Runsuntil,
                Created = auction.Created, Address = auction.Address, Auctionname = auction.Auctionname,
                Highestbidder = auction.Highestbidder, Minbet = auction.Minbet,
                Marketplacefeepercent = auction.Marketplacefeepercent, Royaltyaddress = auction.Royaltyaddress,
                Royaltyfeespercent = auction.Royaltyfeespercent, Selleraddress = auction.Selleraddress, AuctionType = "legacy", 
                Uid= auctionuid
            };
            resx.AuctionsNfts = (from a in auction.LegacyauctionsNfts
                select new AuctionsNft()
                {
                    Tokennamehex = a.Tokennamehex, Tokencount = a.Tokencount, Ipfshash = a.Ipfshash,
                    Metadata = a.Metadata, Policyid = a.Policyid
                }).ToArray();

            resx.Auctionshistories = (from a in auction.Legacyauctionshistories
                select new AuctionsHistory()
                {
                    Bidamount = a.Bidamount, State = a.State, Returntxhash = a.Returntxhash, Created = a.Created,
                    Senderaddress = a.Senderaddress, Txhash = a.Txhash
                }).ToArray();

            await db.Database.CloseConnectionAsync();
            CheckCachedAccess.SetCachedResult(_redis, cachedApiCallValues, 200, resx);
            return Ok(resx);
        }
    }
}
