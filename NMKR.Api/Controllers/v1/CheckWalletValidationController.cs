using System.Linq;
using Asp.Versioning;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NMKR.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ApiVersion("1")]

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
        /// <param Name="apikey">The apikey you have created on NMKR Studio</param>
        /// <param Name="validationuid">The validation id you got from GetWalletValidationAddress</param>
        /// <param Name="lovelace">The lovelace you got from GetWalletValidationAddress</param>
        /// <response code="200">Returns the CheckWalletValidationResultClass Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="500">Internal server error - see the errormessage in the result</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CheckWalletValidationResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{apikey}/{validationuid}/{lovelace:long}")]
        [MapToApiVersion("1")]
        public IActionResult Get(string apikey, string validationuid, long lovelace)
        {
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;

            string apifunction = this.GetType().Name;
            string apiparameter = validationuid + "_" + lovelace;

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
                "", apikey, remoteIpAddress?.ToString() ?? string.Empty);
            if (result.ResultState != ResultStates.Ok)
            {
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }

            CheckWalletValidationResultClass cwvrc = new();

            using (var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options))
            {
                var valaddress = (from a in db.Validationamounts
                    where a.Uid == validationuid
                    select a).FirstOrDefault();

                if (valaddress == null)
                {
                    result.ErrorCode = 1001;
                    result.ErrorMessage = "Validation Id not found";
                    result.ResultState = ResultStates.Error;
                    CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 404, result, apiparameter);
                    return NotFound(result);
                }

                if (valaddress.Lovelace != lovelace)
                {
                    result.ErrorCode = 1002;
                    result.ErrorMessage = "Lovelace Amount does not match Validation Id";
                    result.ResultState = ResultStates.Error;
                    CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 404, result, apiparameter);
                    return NotFound(result);
                }

                cwvrc.ValidationResult = valaddress.State;
                cwvrc.SenderAddress = valaddress.Senderaddress;
                cwvrc.StakeAddress = Bech32Engine.GetStakeFromAddress(valaddress.Senderaddress);
                cwvrc.ValidUntil = valaddress.Validuntil;
                cwvrc.Lovelace = valaddress.Lovelace;
                cwvrc.Validationaddress = valaddress.Validationaddress.Address;
                cwvrc.ValidationName = valaddress.Optionalvalidationname;
            }
            CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, cwvrc, apiparameter);
            return Ok(cwvrc);
        }
    }

}