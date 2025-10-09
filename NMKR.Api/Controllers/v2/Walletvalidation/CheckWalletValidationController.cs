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

namespace NMKR.Api.Controllers.v2.Walletvalidation
{
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class CheckWalletValidationController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public CheckWalletValidationController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Returns the result of a wallet validation
        /// </summary>
        /// <remarks>
        /// Here you can check the result of a wallet validation. The result are "notvalidated", "validated","expired"
        /// </remarks>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="validationuid">The validation uid you got from GetWalletValidationAddress</param>
        /// <response code="200">Returns the CheckWalletValidationResultClass Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="500">Internal server error - see the errormessage in the result</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CheckWalletValidationResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{validationuid}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Wallet validation" }
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string validationuid)
        {
          //  string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;

            string apifunction = this.GetType().Name;
            string apiparameter = validationuid;

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<CheckWalletValidationResultClass>(cachedResult.ResultString));
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

            CheckWalletValidationResultClass cwvrc = new();

            await using (var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options))
            {
                var valaddress = await (from a in db.Validationamounts
                        .Include(a=>a.Validationaddress)
                    where a.Uid == validationuid
                    select a).FirstOrDefaultAsync();

                if (valaddress == null)
                {
                    result.ErrorCode = 1001;
                    result.ErrorMessage = "Validation Id not found";
                    result.ResultState = ResultStates.Error;
                    CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 404, result, apiparameter);
                    return NotFound(result);
                }

                cwvrc.ValidationResult = valaddress.State;
                cwvrc.SenderAddress = valaddress.Senderaddress;
                cwvrc.StakeAddress = Bech32Engine.GetStakeFromAddress(valaddress.Senderaddress);
                cwvrc.Lovelace = valaddress.Lovelace;
                cwvrc.Validationaddress = valaddress.Validationaddress.Address;
                cwvrc.ValidUntil = valaddress.Validuntil;
                cwvrc.ValidationName = valaddress.Optionalvalidationname;
            }
            CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, cwvrc, apiparameter);
            return Ok(cwvrc);
        }
    }

}