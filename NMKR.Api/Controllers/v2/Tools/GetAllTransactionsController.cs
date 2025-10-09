using System;
using System.Linq;
using NMKR.Shared.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace NMKR.Api.Controllers.v2.Tools
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class GetAllTransactionsController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public GetAllTransactionsController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Returns all transactions of a specific date
        /// </summary>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <response code="200">Returns the Transactions as an array</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Transaction[]))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [HttpGet]
        [MapToApiVersion("2")]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, [FromQuery] DateOnly date)
        {
            // string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;

            string apifunction = this.GetType().Name;
            string apiparameter = date.ToString();


            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                return StatusCode(cachedResult.Statuscode,
                    JsonConvert.DeserializeObject<ApiErrorResultClass>(cachedResult.ResultString));
            }


            // Check if the Apikey is valid
            var result = CheckCachedAccess.CheckApikeyOrToken(_redis, apifunction,
                "", apikey, remoteIpAddress?.ToString() ?? string.Empty);
            if (result.ResultState != ResultStates.Ok)
            {
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }


            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            var transactions = await (from a in db.Transactions
                where DateOnly.FromDateTime(a.Created) == date
                select new
                {
                    a.Senderaddress,
                    a.Receiveraddress,
                    Amount = a.Ada,
                    a.Created,
                    a.Fee,
                    a.Transactiontype,
                    a.Transactionid,
                    a.CustomerId,
                    a.Projectaddress,
                    ProjectAmpount = a.Projectada,
                    a.Mintingcostsaddress,
                    Mintingcosts = a.Mintingcostsada,
                    a.NftprojectId,
                    a.State,
                    a.Eurorate,
                    a.Stakereward,
                    a.Discount,
                    a.RefererId,
                    a.RefererCommission,
                    a.Originatoraddress,
                    a.Stakeaddress,
                    a.Confirmed,
                    a.Tokenreward,
                    a.Nmkrcosts,
                    a.Paymentmethod,
                    a.Nftcount,
                    a.Priceintokensquantity,
                    PriceintokensPolicyOrCollection = a.Priceintokenspolicyid,
                    a.Priceintokenstokennamehex,
                    a.Priceintokensmultiplier,
                    a.Customerproperty,
                    a.Discountcode,
                    a.Coin,
                }).AsNoTracking().ToArrayAsync();

            return Ok(transactions);
        }
    }
}
