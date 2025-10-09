using System;
using System.Collections.Generic;
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

namespace NMKR.Api.Controllers.v2.Customer
{
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class GetPayoutWalletsController : ControllerBase
    {

        private readonly IConnectionMultiplexer _redis;

        public GetPayoutWalletsController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Returns all payout wallets in your account
        /// </summary>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <response code="200">Returns the GetPayoutWalletsResultClass</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<GetPayoutWalletsResultClass>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [HttpGet]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Customer" }
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey)
        {
          //  string apikey = Request.Headers["authorization"];
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
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<List<GetPayoutWalletsResultClass>>(cachedResult.ResultString));
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


            await using (var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options))
            {
                var customer = CheckApiAccess.GetCustomer(_redis, db, apikey);
                if (customer == null)
                {
                    result.ErrorCode = 124;
                    result.ErrorMessage = "Customer not found";
                    result.ResultState = ResultStates.Error;
                    return StatusCode(401, result);
                }

                List<GetPayoutWalletsResultClass> pwrc = new();
                var custwall = await (from a in db.Customerwallets
                    where a.CustomerId == customer.Id && a.State != "deleted"
                    select a).ToListAsync();

                foreach (var customerwallet in custwall)
                {
                    pwrc.Add(new() {Created = customerwallet.Created, State = GetState(customerwallet), WalletAddress = customerwallet.Walletaddress, Comment = customerwallet.Comment});
                }

                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, pwrc, apiparameter);
                await db.Database.CloseConnectionAsync();
                return Ok(pwrc);
            }
        }

        private PayoutWalletState GetState(Customerwallet customerwallet)
        {
            if (customerwallet.State=="active")
                return PayoutWalletState.Active;

            if (customerwallet.State == "notactive" && customerwallet.Confirmationvalid != null &&
                customerwallet.Confirmationvalid > DateTime.Now)
                return PayoutWalletState.NotActive;

            return PayoutWalletState.ConfirmationExpired;
        }
    }
}
