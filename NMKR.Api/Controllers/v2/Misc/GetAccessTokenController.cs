using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Google.Authenticator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.Misc
{
    [ApiExplorerSettingsAttribute(IgnoreApi = true)]
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class GetAccessTokenController : ControllerBase
    {

        private readonly IConnectionMultiplexer _redis;

        public GetAccessTokenController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }


        /// <summary>
        /// Creates an access token for the payment window (internal function)
        /// </summary>
        /// <param Name="secret">The secret</param>
        /// <param Name="onetimepassword">The one time password of the secret</param>
        /// <response code="200">Returns the GetAccessTokenResultClass with an valid access token</response>
        /// <response code="401">The access was denied. (IP Address not allowed)</response>     
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetAccessTokenResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{onetimepassword}")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string secret, string onetimepassword)
        {
         //   string secret = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(secret))
                secret = secret.Replace("Bearer ", "");

            if (Request.Method.Equals("HEAD"))
                return null;

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress ?? IPAddress.None;

            string apifunction = this.GetType().Name;
            string apiparameter = secret + onetimepassword;
            string apikey = "";

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<GetAccessTokenResultClass>(cachedResult.ResultString));
                return StatusCode(cachedResult.Statuscode,
                    JsonConvert.DeserializeObject<ApiErrorResultClass>(cachedResult.ResultString));
            }

            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);

            var access = await (from a in db.Getaccesstokensusers
                where a.Secret == secret
                select a).AsNoTracking().FirstOrDefaultAsync();


            var result = new ApiErrorResultClass()
                {ResultState = ResultStates.Ok, ErrorCode = 0, ErrorMessage = ""};

            if (access == null)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorMessage = "Access denied";
                result.ErrorCode = 9;
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                await db.Database.CloseConnectionAsync();
                return Unauthorized(result);
            }

            TwoFactorAuthenticator tfa = new();
            var check = tfa.ValidateTwoFactorPIN(secret, onetimepassword, new TimeSpan(0, 0, 60));


            if (!check)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorMessage = "One time password wrong or expired";
                result.ErrorCode = 9;
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                await db.Database.CloseConnectionAsync();
                return Unauthorized(result);
            }

            var customerid = access.CustomerId ?? 0;
            await db.Database.CloseConnectionAsync();

            string newtoken = "token" + GlobalFunctions.GetGuid();


            IDatabase dbr = _redis.GetDatabase();
            dbr.StringSet(newtoken, GlobalFunctions.GetNetworkName(), expiry: new TimeSpan(0, 30, 0));
            dbr.StringSet("customerid_" + newtoken, customerid, expiry: new TimeSpan(0, 30, 0));
            GetAccessTokenResultClass catrc = new() {AccessToken = newtoken, Expires = DateTime.UtcNow.AddMinutes(30)};
            CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, catrc, apiparameter,10);
            return Ok(catrc);
        }
    }
}
