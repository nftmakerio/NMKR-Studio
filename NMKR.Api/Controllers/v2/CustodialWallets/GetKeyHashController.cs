using System.Linq;
using NMKR.Shared.Classes.CustodialWallets;
using NMKR.Shared.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;
using CardanoSharp.Wallet.Utilities;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using System;
using Asp.Versioning;
using NMKR.Shared.Classes.Cardano_Sharp;

namespace NMKR.Api.Controllers.v2.CustodialWallets
{
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class GetKeyHashController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public GetKeyHashController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Returns the key hash of a Managed Wallet
        /// </summary>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <response code="200">Returns the CreateWalletResultClass Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="500">Internal server error - see the errormessage in the result</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpPost("{customerid}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] {"Managed Wallets"}
        )]
        public async Task<IActionResult> Post([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, int customerid, [FromBody] GetKeyHashClass keyHashClass)
        {
           // string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;

            string apifunction = this.GetType().Name;
            string apiparameter = customerid.ToString() + keyHashClass.ManagedWalletAddress + keyHashClass.Walletpassword;

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

            if (string.IsNullOrEmpty(keyHashClass.Walletpassword))
            {
                result.ErrorCode = 4439;
                result.ErrorMessage = "Walletpassword is empty";
                result.ResultState = ResultStates.Error;
                return StatusCode(500, result);
            }

            if (keyHashClass.Walletpassword.Length < 6)
            {
                result.ErrorCode = 4440;
                result.ErrorMessage = "Walletpassword is too short. Minimum length is 6";
                result.ResultState = ResultStates.Error;
                return StatusCode(500, result);
            }

            if (keyHashClass.Walletpassword.Length > 64)
            {
                result.ErrorCode = 4441;
                result.ErrorMessage = "Walletpassword is too long. Maximum length is 64";
                result.ResultState = ResultStates.Error;
                return StatusCode(500, result);
            }
            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);

            var custodialwallet = await (from a in db.Custodialwallets
                where a.Address == keyHashClass.ManagedWalletAddress
                      && a.CustomerId == customerid
                select a).AsNoTracking().FirstOrDefaultAsync();

            if (custodialwallet == null)
            {
                result.ErrorCode = 30001;
                result.ErrorMessage = "Wallet not found";
                result.ResultState = ResultStates.Error;
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 404, result, apiparameter);
                return NotFound(result);
            }

            if (custodialwallet.State != "active")
            {
                result.ErrorCode = 30002;
                result.ErrorMessage = "Wallet is not in active state";
                result.ResultState = ResultStates.Error;
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 404, result, apiparameter);
                return StatusCode(403, result);
            }
            // Check for Pincode
            string salt = keyHashClass.Walletpassword;
            string password = salt + GeneralConfigurationClass.Masterpassword;
            string skey = Encryption.DecryptString(custodialwallet.Skey, password);
            string vkey = Encryption.DecryptString(custodialwallet.Vkey, password);
            if (string.IsNullOrEmpty(skey))
            {
                result.ErrorCode = 30003;
                result.ErrorMessage = "Walletpassword is not correct";
                result.ResultState = ResultStates.Error;
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 404, result, apiparameter);
                return StatusCode(401, result);
            }
            var policyKeyHash = HashUtility.Blake2b224(Convert.FromHexString(CardanoSharpFunctions.GetKeyFromCbor(vkey)));

           return Ok(Convert.ToHexString(policyKeyHash));
        }
    }
}
