using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using Asp.Versioning;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NMKR.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ApiVersion("1")]
    public class GetWalletValidationAddressController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public GetWalletValidationAddressController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Returns an address for wallet validation
        /// </summary>
        /// <remarks>
        /// When you call this API, you will receive an address for a wallet validation. The user can send any ada to this address and the ada (and tokens) will sent back to the sender. With the function CheckWalletValidation you can check the state of the address
        /// </remarks>
        /// <param Name="apikey">The apikey you have created on NMKR Studio</param>
        /// <param Name="validationname">An optional name for the validation - will returned in CheckWalletValidation</param>
        /// <response code="200">Returns the GetWalletValidationAddressResultClass Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="500">Internal server error - see the errormessage in the result</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetWalletValidationAddressResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{apikey}/{validationname}")]
        [MapToApiVersion("1")]
        public IActionResult Get(string apikey, string validationname = "")
        {
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;

            string apifunction = this.GetType().Name;
            string apiparameter = "";

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
                return StatusCode(cachedResult.Statuscode,
                    JsonConvert.DeserializeObject<ApiErrorResultClass>(cachedResult.ResultString));


            // Check if the Apikey is valid
            var result = CheckCachedAccess.CheckApikeyOrToken(_redis, apifunction,
                "", apikey, remoteIpAddress?.ToString());
            if (result.ResultState != ResultStates.Ok)
            {
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }


            var cn = ConsoleCommand.CreateNewPaymentAddress(GlobalFunctions.IsMainnet());
            if (cn.ErrorCode != 0)
            {
                result.ErrorCode = cn.ErrorCode;
                result.ErrorMessage = cn.ErrorMessage;
                result.ResultState = ResultStates.Error;
                return StatusCode(500, result);
            }


            GetWalletValidationAddressResultClass gwvarc = new();
            using (var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options))
            {
                Validationaddress adr = (from a in db.Validationaddresses
                        .Include(a => a.Validationamounts)
                    where a.State == "active"
                    orderby a.Id descending
                    select a).FirstOrDefault();

                if (adr == null || adr.Validationamounts.Count()>1000)
                {
                    CryptographyProcessor cp = new();
                    string salt = cp.CreateSalt(30);
                    string password = salt + GeneralConfigurationClass.Masterpassword;

                    adr = new()
                    {
                        Address = cn.Address,
                        Privatevkey = Encryption.EncryptString(cn.privatevkey, password),
                        Privateskey = Encryption.EncryptString(cn.privateskey, password),
                        State = "active",
                        Password = salt,
                    };
                    db.Validationaddresses.Add(adr);
                    db.SaveChanges();
                }

                long ll = 0;
                var rand = new Random();
                do
                {
                    ll = 1500000 + rand.Next(399999);
                } while (adr.Validationamounts.FirstOrDefault(x => x.Lovelace == ll) != null);

                var valamount = new Validationamount()
                {
                    Lovelace = ll,
                    State = "notvalidated",
                    ValidationaddressId = adr.Id,
                    Validuntil = DateTime.Now.AddDays(1),
                    Uid = Guid.NewGuid().ToString(),
                    Optionalvalidationname = validationname
                };
                db.Validationamounts.Add(valamount);
                db.SaveChanges();

                gwvarc.ValidationUId = valamount.Uid;
                gwvarc.Address = adr.Address;
                gwvarc.Lovelace = ll;
                gwvarc.Expires = valamount.Validuntil;
            }

            return Ok(gwvarc);
        }


        /// <summary>
        /// Returns an address for wallet validation
        /// </summary>
        /// <remarks>
        /// When you call this API, you will receive an address for a wallet validation. The user can send any ada to this address and the ada (and tokens) will sent back to the sender. With the function CheckWalletValidation you can check the state of the address
        /// </remarks>
        /// <param Name="apikey">The apikey you have created on NMKR Studio</param>
        /// <response code="200">Returns the GetWalletValidationAddressResultClass Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="500">Internal server error - see the errormessage in the result</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetWalletValidationAddressResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{apikey}")]
        public IActionResult Get(string apikey)
        {
            return Get(apikey, "");
        }

    }
}
