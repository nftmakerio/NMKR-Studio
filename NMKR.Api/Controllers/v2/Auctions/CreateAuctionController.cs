using System;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Classes;
using NMKR.Shared.Classes.Auctions;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    public class CreateAuctionController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        /// <summary>
        /// Returns the state - and the last bids of a auction project 
        /// </summary>
        /// <param name="redis"></param>
        public CreateAuctionController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Creates a new legacy auction in the cardano network
        /// </summary>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="auctionuid">The uid of your auction</param>
        /// <response code="200">Returns an array of the GetAuctionStateResultClass</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetAuctionStateResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [HttpPost("{customerid}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Auctions" }
        )]
        public async Task<IActionResult> Post([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey,int customerid, [FromBody] CreateAuctionClass createAuctionClass)
        {
            // string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            if (Request.Method.Equals("HEAD"))
                return null;
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = "";

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

            return await CreateAuction(result, new(apikey, apifunction, apiparameter),customerid, createAuctionClass);
        }

        private async Task<IActionResult> CreateAuction(ApiErrorResultClass result,
            CachedApiCallValues cachedApiCallValues,int customerId, CreateAuctionClass createAuctionClass)
        {
            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);


            if (string.IsNullOrEmpty(createAuctionClass.AuctionName))
            {
                result.ErrorCode = 3420;
                result.ErrorMessage = "Auctionname is empty";
                result.ResultState = ResultStates.Error;
                CheckCachedAccess.SetCachedResult(_redis, cachedApiCallValues, 404, result);
                return StatusCode(406, result);
            }

            if (createAuctionClass.MinimumBidInAda < 3)
            {
                result.ErrorCode = 3422;
                result.ErrorMessage = "Minimum Bid is less than 3 Ada";
                result.ResultState = ResultStates.Error;
                CheckCachedAccess.SetCachedResult(_redis, cachedApiCallValues, 404, result);
                return StatusCode(406, result);
            }

            if (createAuctionClass.AuctionRunsUntil < DateTime.Now)
            {
                result.ErrorCode = 3423;
                result.ErrorMessage = "AuctionRunsUntil is in the past";
                result.ResultState = ResultStates.Error;
                CheckCachedAccess.SetCachedResult(_redis, cachedApiCallValues, 404, result);
                return StatusCode(406, result);
            }

            var checkaddress=ConsoleCommand.CheckIfAddressIsValid(db, createAuctionClass.PayoutWallet, GlobalFunctions.IsMainnet(),
                out string outaddress, out Blockchain blockchain, true, false);

            if (checkaddress == false)
            {
                result.ErrorCode = 3424;
                result.ErrorMessage = "PayoutWallet is not a valid cardano address";
                result.ResultState = ResultStates.Error;
                CheckCachedAccess.SetCachedResult(_redis, cachedApiCallValues, 404, result);
                return StatusCode(406, result);
            }
            if (blockchain != Blockchain.Cardano)
            {
                result.ErrorCode = 3425;
                result.ErrorMessage = "PayoutWallet is not a cardano address";
                result.ResultState = ResultStates.Error;
                CheckCachedAccess.SetCachedResult(_redis, cachedApiCallValues, 404, result);
                return StatusCode(406, result);
            }


            var newaddress = ConsoleCommand.CreateNewPaymentAddress(GlobalFunctions.IsMainnet());
            if (newaddress.ErrorCode != 0)
            {
                result.ErrorCode = 3422;
                result.ErrorMessage = "Internal error while creating an auction address";
                result.ResultState = ResultStates.Error;
                CheckCachedAccess.SetCachedResult(_redis, cachedApiCallValues, 404, result);
                return StatusCode(500, result);
            }

            var salt = GlobalFunctions.GetGuid();
            var auctionuid = GlobalFunctions.GetGuid();

            Legacyauction la = new()
            {
                Actualbet = 0,
                Address = newaddress.Address,
                Created = DateTime.Now,
                Minbet = (long)(createAuctionClass.MinimumBidInAda * 1000000),
                Runsuntil =createAuctionClass.AuctionRunsUntil,
                State = "waitforlock",
                Salt = salt,
                Skey = Encryption.EncryptString(newaddress.privateskey, salt + GeneralConfigurationClass.Masterpassword),
                Vkey = Encryption.EncryptString(newaddress.privatevkey, salt + GeneralConfigurationClass.Masterpassword),
                Marketplacefeepercent = 2,
                Royaltyaddress = "",
                Selleraddress = outaddress,
                Auctionname = createAuctionClass.AuctionName,
                CustomerId = customerId,
                Uid = auctionuid,
            };

            await db.Legacyauctions.AddAsync(la);
            await db.SaveChangesAsync();
            GetAuctionStateController gasc=new (_redis);

            return Ok(await gasc.GetAuction(result, cachedApiCallValues, auctionuid));
        }
    }
}
